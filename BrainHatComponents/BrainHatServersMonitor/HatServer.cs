﻿using LoggingInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BrainflowInterfaces;
using static BrainHatNetwork.Tcpip;
using Newtonsoft.Json;
using BrainHatNetwork;
using LSL;
using static LSL.liblsl;

namespace BrainHatServersMonitor
{
    /// <summary>
    /// Monitor communications from all servers on the network
    /// </summary>
    public class HatServer : IBrainHatServerConnection, IBrainHatServerStatus
    {
        //  Events
        public event LogEventDelegate Log;

        public event BFSampleEventDelegate RawDataReceived;

        //  Properties
        public string HostName { get; set; }

        public string Eth0Address { get; set; }

        public string Wlan0Address { get; set; }

        public string Wlan0Mode { get; set; }

        public int DataPort { get; set; }

        public int LogPort { get; set; }

        public int BoardId { get; set; }

        public int SampleRate { get; set; }

        public bool RecordingDataBrainHat { get; set; }

        public bool RecordingDataBoard { get; set; }

        public string RecordingFileNameBrainHat { get; set; }

        public string RecordingFileNameBoard { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public bool ReceivingRaw => RawDataProcessedLast.ElapsedMilliseconds < 5000;

        public double RawLatency => RawDataOffsetTime;


        public string IpAddress
        {
            get
            {
                if (Eth0Address.Length > 0)
                    return Eth0Address;
                else
                    return Wlan0Address;
            }
        }


        /// <summary>
        /// Start monitor
        /// </summary>
        public async Task StartMonitorAsync()
        {
            await StopMonitorAsync();

            MonitorCancelTokenSource = new CancellationTokenSource();
            ReadDataPortTask = RunReadDataPortAsync(MonitorCancelTokenSource.Token);
            ReadLogPortTask = RunReadLogPortAsync(MonitorCancelTokenSource.Token);
      
            CountRecordsTimer.Start();
        }


        /// <summary>
        ///  Stop monitor
        /// </summary>
        public async Task StopMonitorAsync()
        {
            if (MonitorCancelTokenSource != null)
            {
                MonitorCancelTokenSource.Cancel();
                if (ReadDataPortTask != null)
                    await Task.WhenAll(ReadDataPortTask, ReadLogPortTask);

                MonitorCancelTokenSource = null;
                ReadDataPortTask = null;
                ReadLogPortTask = null;
            }
        }

        /// <summary>
        /// Update the connection information
        /// Force restart if ports have changed
        /// </summary>
        public async Task UpdateConnection(IBrainHatServerConnection connection)
        {
            Eth0Address = connection.Eth0Address;
            Wlan0Address = connection.Wlan0Address;

            if (DataPort != connection.DataPort || LogPort != connection.LogPort)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunUdpMulticastReader", $"Host connection changed, restarting server monitor.", LogLevel.WARN));

                await StopMonitorAsync();

                DataPort = connection.DataPort;
                LogPort = connection.LogPort;

                await StartMonitorAsync();
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public HatServer(BrainHatServerStatus connection, StreamInfo streamInfo)
        {
            HostName = connection.HostName;
            DataPort = connection.DataPort;
            LogPort = connection.LogPort;
            Eth0Address = connection.Eth0Address;
            Wlan0Address = connection.Wlan0Address;

            BoardId = connection.BoardId;
            SampleRate = connection.SampleRate;

            StreamInfo = streamInfo;

            CountRecordsTimer = new System.Diagnostics.Stopwatch();
            RawDataProcessedLast = new System.Diagnostics.Stopwatch();

            TimeStamp = DateTimeOffset.UtcNow;
        }

        StreamInfo StreamInfo;

        //  Cancel Token for background tasks
        CancellationTokenSource MonitorCancelTokenSource { get; set; }
        //  Read data port task
        Task ReadDataPortTask { get; set; }
        // Monitor task
        Task ReadLogPortTask { get; set; }



        /// <summary>
        /// Run function for reading data on UDP multicast data port
        /// </summary>
        protected async Task RunReadDataPortAsync(CancellationToken cancelToken)
        {
            var inlet = new StreamInlet(StreamInfo);
            double[] maxSample = new double[31];

            await Task.Delay(1000);

            try
            {
                //  spin until canceled
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        inlet.pull_sample(maxSample);
                        var newSample = ParseSample(maxSample);
                        RawDataReceived?.Invoke(this, new BFSampleEventArgs(newSample));
                        LogRawDataProcessingPerformance(newSample);

                    }
                    catch (Exception ex)
                    {
                        Log?.Invoke(this, new LogEventArgs(this, "RunUdpMulticastReader", ex, LogLevel.ERROR));
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunUdpMulticastReader", e, LogLevel.FATAL));
            }
            finally
            {
                inlet.close_stream();
            }
        }

        private IBFSample ParseSample(double[] sample)
        {

            switch ( BoardId )
            {
                case 0:
                    return new BFCyton8Sample(sample);

                case 2:
                    return new BFCyton16Sample(sample);

            }

            return null;
        }



    





        int RecordsCount = 0;
        System.Diagnostics.Stopwatch CountRecordsTimer;

        double RawDataOffsetTime;
        System.Diagnostics.Stopwatch RawDataProcessedLast;

        double LastTimeStamp;

        /// <summary>
        /// Log raw data processing performance
        /// </summary>
        private void LogRawDataProcessingPerformance(IBFSample data)
        {
            RecordsCount++;
            RawDataProcessedLast.Restart();
            RawDataOffsetTime = Math.Max(Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000 - data.TimeStamp), RawDataOffsetTime);

            LastTimeStamp = data.TimeStamp;

            if (CountRecordsTimer.ElapsedMilliseconds > 5000)
            {
                Log?.Invoke(this, new LogEventArgs(this, "LogRawDataProcessingPerformance", $"{HostName} Logged {(int)(RecordsCount / CountRecordsTimer.Elapsed.TotalSeconds)} records per second. Offset time {RawDataOffsetTime.ToString("F6")} s.", LogLevel.TRACE));
                CountRecordsTimer.Restart();
                RawDataOffsetTime = 0.0;
                RecordsCount = 0;
            }
        }


        //  Remote Log Monitor
        #region RemoteLogMonitor

        private async Task RunReadLogPortAsync(CancellationToken cancelToken)
        {
            try
            {
                using (var udpClient = new UdpClient(LogPort))
                {
                    try
                    {
                        udpClient.JoinMulticastGroup(IPAddress.Parse(BrainHatNetworkAddresses.MulticastGroupAddress));
                        cancelToken.Register(() => udpClient.Close());

                        while (!cancelToken.IsCancellationRequested)
                        {
                            try
                            {
                                //  wait for the next read, and then split it into the command and the arguments
                                var parseReceived = Encoding.ASCII.GetString((await udpClient.ReceiveAsync()).Buffer).Split('?');

                                if (parseReceived.Count() == 2)
                                {
                                    //  see if this is recognized command
                                    switch (parseReceived[0])
                                    {
                                        case "log":
                                            //  parse the arguments
                                            var responseArgs = HttpUtility.ParseQueryString(parseReceived[1]);
                                            var logString = responseArgs.Get("log");
                                            var hostName = responseArgs.Get("hostname");
                                            if (logString != null)
                                            {
                                                var log = JsonConvert.DeserializeObject<RemoteLogEventArgs>(logString);
                                                Log?.Invoke(this, new LogEventArgs(log));
                                            }
                                            break;

                                        default:
                                            Log?.Invoke(this, new LogEventArgs(this, "RunMonitor", $"Received invalid remote log: {parseReceived}.", LogLevel.WARN));
                                            break;
                                    }
                                }
                            }
                            catch (Exception exc)
                            {
                                Log?.Invoke(this, new LogEventArgs(this, "RunMonitor", exc, LogLevel.ERROR));
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    { }
                    catch (Exception ex)
                    {
                        Log?.Invoke(this, new LogEventArgs(this, "RunMonitor", ex, LogLevel.FATAL));
                    }
                }
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunMonitor", e, LogLevel.FATAL));
            }
        }


        #endregion
        /// <summary>
        /// Pass through to the log function for the data processor component
        /// </summary>
        private void OnComponentLog(object sender, LogEventArgs e)
        {
            Log?.Invoke(sender, e);
        }






    }
}