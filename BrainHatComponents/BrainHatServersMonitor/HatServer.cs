using LoggingInterfaces;
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
using System.Xml.Linq;

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

        public double RecordingDurationBrainHat { get; set; }

        public double RecordingDurationBoard { get; set; }

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
                    await ReadDataPortTask;

                MonitorCancelTokenSource = null;
                ReadDataPortTask = null;
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

            var doc = XDocument.Parse(streamInfo.as_xml());
            int.TryParse(doc.Element("info")?.Element("channel_count").Value, out SampleSize);


            CountRecordsTimer = new System.Diagnostics.Stopwatch();
            RawDataProcessedLast = new System.Diagnostics.Stopwatch();

            TimeStamp = DateTimeOffset.UtcNow;
        }

        StreamInfo StreamInfo;
        int SampleSize;

        //  Cancel Token for background tasks
        CancellationTokenSource MonitorCancelTokenSource { get; set; }
        //  Read data port task
        Task ReadDataPortTask { get; set; }



        List<Tuple<long, long>> PullSampleTimes = new List<Tuple<long, long>>();
        int SampleCounter = 0;

        /// <summary>
        /// Run function for reading data on LSL multicast data port
        /// </summary>
        protected async Task RunReadDataPortAsync(CancellationToken cancelToken)
        {
            var sw = new System.Diagnostics.Stopwatch();
            var sampleReportingTime = new System.Diagnostics.Stopwatch();
            sampleReportingTime.Start();

            StreamInlet inlet = null;
            try
            {
                await Task.Delay(100);

                inlet = new StreamInlet(StreamInfo);
               
                //inlet.open_stream();


                double[] rawSample = new double[SampleSize];

                double[,] buffer = new double[512, SampleSize];
                double[] timestamps = new double[512];
                IBFSample nextSample = null;

                //  spin until canceled
                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                      
                        int num = inlet.pull_chunk(buffer, timestamps);
                        for (int s = 0; s < num; s++)
                        {
                            nextSample = null;
                            switch ( BoardId )
                            {
                                case 0:
                                    nextSample = new BFCyton8Sample(buffer, s);
                                    break;

                                case 2:
                                    nextSample = new BFCyton16Sample(buffer, s);
                                    break;
                            }

                            RawDataReceived?.Invoke(this, new BFSampleEventArgs(nextSample));
                            LogRawDataProcessingPerformance(nextSample);
                        }
                        await Task.Delay(1);


                        //sw.Restart();
                        //inlet.pull_sample(rawSample, 2);
                        //var pullSampleTime = sw.ElapsedMilliseconds;
                        //var newSample = ParseSample(rawSample);
                        //RawDataReceived?.Invoke(this, new BFSampleEventArgs(newSample));
                        //LogRawDataProcessingPerformance(newSample);
                        //var totalTime = sw.ElapsedMilliseconds;
                        //PullSampleTimes.Add(new Tuple<long, long>(pullSampleTime, totalTime));
                        //SampleCounter++;
                    }
                    catch (Exception ex)
                    {
                        Log?.Invoke(this, new LogEventArgs(HostName, this, "RunReadDataPortAsync", ex, LogLevel.WARN));
                        await Task.Delay(500);
                    }

                    //if ( sampleReportingTime.ElapsedMilliseconds > 5000)
                    //{
                    //    sampleReportingTime.Restart();
                    //    Log?.Invoke(this, new LogEventArgs(HostName, this, "RunReadDataPortAsync", $"LSL Sample: Pulled {SampleCounter} samples in 5 s. Pull { PullSampleTimes.Select(x => x.Item1).Average()} ms max {PullSampleTimes.Select(x => x.Item1).Max()} ms. Total {PullSampleTimes.Select(x => x.Item2).Average()} ms max {PullSampleTimes.Select(x => x.Item2).Max()} ms. ", LogLevel.DEBUG));
                    //    PullSampleTimes.Clear();
                    //    SampleCounter = 0;
                    //}
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
                if ( inlet != null )
                    inlet.close_stream();
            }
        }


        /// <summary>
        /// Parse the sample into a known data type
        /// Currently only Cyton and Cyton+Daisy are supported
        /// </summary>
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

    }
}
