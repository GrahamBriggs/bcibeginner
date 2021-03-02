﻿using BrainflowInterfaces;
using LoggingInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static LSL.liblsl;

namespace BrainHatNetwork
{
    /// <summary>
    /// A client connection to a single brainHat server
    /// Receives raw data from the server via LSL,     
    /// and provides TCPIP connection to send query/command and receive response from server.
    /// </summary>
    public class HatClient : IBrainHatServerConnection, IBrainHatServerStatus
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

        public int NumberOfChannels => brainflow.BoardShim.get_exg_channels(BoardId).Length;

        public bool RecordingDataBrainHat { get; set; }

        public bool RecordingDataBoard { get; set; }

        public string RecordingFileNameBrainHat { get; set; }

        public string RecordingFileNameBoard { get; set; }

        public double RecordingDurationBrainHat { get; set; }

        public double RecordingDurationBoard { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public bool ReceivingRaw => (RunTaskCancelTokenSource != null && TimeSinceLastSample.ElapsedMilliseconds < 5000);

        public double RawLatency => RawDataOffsetTime;

        public TimeSpan OffsetTime { get; set; }

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
        /// Start reading from the LSL data stream
        /// </summary>
        public async Task StartReadingFromLslAsync()
        {
            await StopReadingFromLslAsync();

            RunTaskCancelTokenSource = new CancellationTokenSource();
            ReadDataPortTask = RunReadDataPortAsync(RunTaskCancelTokenSource.Token);
            CountRecordsTimer.Start();
            TimeSinceLastSample.Start();

            Log?.Invoke(this, new LogEventArgs(HostName, this, "StartMonitorAsync", $"Started HatServer for {HostName}.", LogLevel.INFO));
        }


        /// <summary>
        ///  Stop reading from the LSL data stream
        /// </summary>
        public async Task StopReadingFromLslAsync()
        {
            if (RunTaskCancelTokenSource != null)
            {
                RunTaskCancelTokenSource.Cancel();
                if (ReadDataPortTask != null)
                    await ReadDataPortTask;

                RunTaskCancelTokenSource = null;
                ReadDataPortTask = null;

                Log?.Invoke(this, new LogEventArgs(HostName, this, "StopMonitorAsync", $"Stopped HatServer for {HostName}.", LogLevel.INFO));
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

                await StopReadingFromLslAsync();

                DataPort = connection.DataPort;
                LogPort = connection.LogPort;

                await StartReadingFromLslAsync();
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public HatClient(BrainHatServerStatus connection, StreamInfo streamInfo)
        {
            HostName = connection.HostName;
            DataPort = connection.DataPort;
            LogPort = connection.LogPort;
            Eth0Address = connection.Eth0Address;
            Wlan0Address = connection.Wlan0Address;

            BoardId = connection.BoardId;
            SampleRate = connection.SampleRate;

            StreamInfo = streamInfo;

            int.TryParse(XDocument.Parse(streamInfo.as_xml()).Element("info")?.Element("channel_count").Value, out var sampleSize);
            SampleSize = sampleSize;

            CountRecordsTimer = new System.Diagnostics.Stopwatch();
            TimeSinceLastSample = new System.Diagnostics.Stopwatch();

            TimeStamp = DateTimeOffset.UtcNow;

            RunTaskCancelTokenSource = null;
        }

        private StreamInfo StreamInfo { get; set; }
        public int SampleSize { get; protected set; }

        //  Read data port task
        CancellationTokenSource RunTaskCancelTokenSource { get; set; }
        Task ReadDataPortTask { get; set; }

        //  read disgnostics
        List<Tuple<long, long, long>> PullSampleTimes = new List<Tuple<long, long, long>>();
        List<long> PullSampleCount = new List<long>();


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
                inlet = new StreamInlet(StreamInfo);
                cancelToken.Register(() => inlet.close_stream());
                inlet.open_stream();

                Log?.Invoke(this, new LogEventArgs(HostName, this, "RunReadDataPortAsync", $"Create LSL stream: {inlet.info().as_xml()}.", LogLevel.DEBUG));

                double[,] buffer = new double[512, SampleSize];
                double[] timestamps = new double[512];
            
                sw.Restart();

                //  spin until canceled
                while (!cancelToken.IsCancellationRequested)
                {
                    var timeBetweenReads = sw.ElapsedMilliseconds;

                    try
                    {
                        sw.Restart();
                        int num = inlet.pull_chunk(buffer, timestamps);

                        var pullTime = sw.ElapsedMilliseconds;
                        PullSampleCount.Add(num);

                        ProcessChunk(buffer, num);

                        var processTime = sw.ElapsedMilliseconds;
                        PullSampleTimes.Add(new Tuple<long, long, long>(pullTime, processTime, timeBetweenReads));
                    }
                    catch (ObjectDisposedException)
                    { }
                    catch (Exception ex)
                    {
                        Log?.Invoke(this, new LogEventArgs(HostName, this, "RunReadDataPortAsync", ex, LogLevel.WARN));
                        await Task.Delay(500);
                    }
                    sw.Restart();

                    await Task.Delay(1);

                    if (sampleReportingTime.ElapsedMilliseconds > 5000)
                    {
                        //Log?.Invoke(this, new LogEventArgs(HostName, this, "RunReadDataPortAsync", $"LSL Sample: Pulled {PullSampleCount.Sum()} samples in 5 s {(int)(PullSampleCount.Sum()/sampleReportingTime.Elapsed.TotalSeconds)} sps . Per chunk: median = {PullSampleCount.Median()} max = {PullSampleCount.Max()} min = {PullSampleCount.Min()}.  Time per chunk median = { PullSampleTimes.Select(x => x.Item1).Median()} ms max = {PullSampleTimes.Select(x => x.Item1).Max()} ms. Total time median = {PullSampleTimes.Select(x => x.Item2).Median()} ms max =  {PullSampleTimes.Select(x => x.Item2).Max()} ms.  Time per chunk median = { PullSampleTimes.Select(x => x.Item1).Median()} ms max = {PullSampleTimes.Select(x => x.Item1).Max()} ms. Time Between median = {PullSampleTimes.Select(x => x.Item3).Median()} ms max =  {PullSampleTimes.Select(x => x.Item3).Max()} ms.  ", LogLevel.TRACE));
                        sampleReportingTime.Restart();
                        PullSampleTimes.Clear();
                        PullSampleCount.Clear();
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
                if (inlet != null)
                    inlet.close_stream();
            }
        }


        /// <summary>
        /// Process chunk read
        /// </summary>
        private void ProcessChunk(double[,] buffer, int num)
        {
            for (int s = 0; s < num; s++)
            {
                IBFSample nextSample = null;
                switch (BoardId)
                {
                    case 0:
                        nextSample = BFCyton8Sample.FromChunkRow(buffer, s);
                        break;

                    case 2:
                        nextSample = BFCyton16Sample.FromChunkRow(buffer, s);
                        break;

                    default:
                        return;  //  TODO ganglion
                }

                RawDataReceived?.Invoke(this, new BFSampleEventArgs(nextSample));
                LogRawDataProcessingPerformance(nextSample);
            }
        }



        int RecordsCount = 0;
        System.Diagnostics.Stopwatch CountRecordsTimer;

        double RawDataOffsetTime;
        System.Diagnostics.Stopwatch TimeSinceLastSample;
        

        /// <summary>
        /// Log raw data processing performance
        /// </summary>
        private void LogRawDataProcessingPerformance(IBFSample data)
        {
            RecordsCount++;
            TimeSinceLastSample.Restart();
            RawDataOffsetTime = Math.Max(Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000 - data.TimeStamp), RawDataOffsetTime);

            if (CountRecordsTimer.ElapsedMilliseconds > 5000)
            {
                Log?.Invoke(this, new LogEventArgs(HostName, this, "LogRawDataProcessingPerformance", $"{HostName} Logged {(int)(RecordsCount / CountRecordsTimer.Elapsed.TotalSeconds)} records per second. Offset time {RawDataOffsetTime.ToString("F6")} s.", LogLevel.TRACE));
                CountRecordsTimer.Restart();
                RawDataOffsetTime = 0.0;
                RecordsCount = 0;
            }
        }

    }
}
