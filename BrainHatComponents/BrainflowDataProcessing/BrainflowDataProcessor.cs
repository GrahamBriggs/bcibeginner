using brainflow;
using LoggingInterfaces;
using BrainflowInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{

    /// <summary>
    /// Brainflow data notify queue data processor
    /// </summary>
    public class BrainflowDataProcessor
    {
        //  Events
        public event LogEventDelegate Log;
        public event ProcessorCurrentStateReportDelegate CurrentDataStateReported;
        public event BFSampleEventDelegate NewSample;


        //  Public Methods
        #region PublicMethods

        /// <summary>
        /// Start async task to run data processing
        /// </summary>
        public async Task StartDataProcessorAsync()
        {
            await StopDataProcessorAsync();

            InitializeProcessor();

            // fire off the run tasks
            CancelTokenSource = new CancellationTokenSource();
            RawDataQueueProcessorTask = RunDataQueueProcessorAsync(CancelTokenSource.Token);
            PeriodicProcessorTask = RunPeriodicProcessorAsync(CancelTokenSource.Token);
            DataMonitorTask = RunDataMonitorAsync(CancelTokenSource.Token);

            TimeTagFirstSample = -1;

            Log?.Invoke(this, new LogEventArgs(Name, this, "StartDataProcessor", $"Starting Brainflow data processor for {Name}.", LogLevel.INFO));
        }


        /// <summary>
        /// Stop the data processor async task, and all component processor tasks
        /// </summary>
        public async Task StopDataProcessorAsync(bool flush = false)
        {
            FlushQueue = flush;

            if (CancelTokenSource != null)
            {
                await SignalFilter.StopSignalFilteringAsync();
                await BandPowers.StopMonitorAsync();

                CancelTokenSource.Cancel();
                await Task.WhenAll(RawDataQueueProcessorTask, DataMonitorTask, PeriodicProcessorTask);

                CancelTokenSource = null;
                RawDataQueueProcessorTask = null;
                PeriodicProcessorTask = null;
                DataMonitorTask = null;

                Log?.Invoke(this, new LogEventArgs(Name, this, "StopDataProcessor", $"Stopped Brainflow data processor for {Name}.", LogLevel.INFO));
            }
        }


        /// <summary>
        /// Start the signal filtering task
        /// </summary>
        public async Task StartSignalFilteringAsync()
        {
            if ( CancelTokenSource == null )
            {
                Log?.Invoke(this, new LogEventArgs(Name, this, "StartSignalFiltering", $"You must start the processor first.", LogLevel.ERROR));
                return;
            }

            await SignalFilter.StartSignalFilteringAsync();
        }


        /// <summary>
        /// Stop signal filtering task
        /// </summary>
        public async Task StopSignalFilteringAsync()
        {
            await SignalFilter.StopSignalFilteringAsync();
        }


        /// <summary>
        /// Start band power monitor task
        /// </summary>
        public async Task StartBandPowerMonitorAsync()
        {
            if (CancelTokenSource == null)
            {
                Log?.Invoke(this, new LogEventArgs(Name, this, "StartSignalFiltering", $"You must start the processor first.", LogLevel.ERROR));
                return;
            }

            await BandPowers.StartMonitorAsync();
        }


        /// <summary>
        /// Stop band power monitor task
        /// </summary>
        public async Task StopBandPowerMonitorAsync()
        {
            await BandPowers.StopMonitorAsync();
        }


        /// <summary>
        /// Set the band power processor ranges
        /// supply a list of tuples: lowFrequency,highFrequency in each range
        /// </summary>
        /// <param name="rangeList"></param>
        public void SetBandPowerRangeList(List<Tuple<double,double>> rangeList)
        {
            BandPowers.SetBandPowerRangeList(rangeList);
        }


        /// <summary>
        /// Add data to the proecssor queue from an event
        /// </summary>
        public void AddDataToProcessor(object sender, BFSampleEventArgs e)
        {
            AddDataToProcessor(e.Sample);
        }


        /// <summary>
        /// Add data to the proecssor queue
        /// </summary>
        public void AddDataToProcessor(IBFSample data)
        {
            DataToProcess.Enqueue(data);
            NotifyAddedData.Release();
        }


        /// <summary>
        /// Get the last 'seconds' worth of raw samples relative to the timestamp of the newest sample
        /// </summary>
        public IEnumerable<IBFSample> GetRawChunk(double seconds)
        {
            return GetUnfilteredData(seconds)?.Reverse().ToArray();
        }


        /// <summary>
        /// Get a range of raw samples starting at 'from' seconds, and ending at 'to' seconds, relative to the timestamp of the newest sample 
        /// </summary>
        public IEnumerable<IBFSample> GetRawChunk(double from, double to)
        {
            return GetUnfilteredData(from, to)?.Reverse().ToArray();
        }


        /// <summary>
        /// Get a range of raw samples newer than since time
        /// </summary>
        public IEnumerable<IBFSample> GetRawChunk(DateTimeOffset since)
        {
            return GetUnfilteredData(since)?.Reverse().ToArray();
        }


        /// <summary>
        /// Get the last 'seconds' of filtered data, relative to the timestamp of newest sample
        /// </summary>
        public IEnumerable<IBFSample> GetFilteredChunk(double seconds)
        {
            return SignalFilter.GetFilteredData(seconds);
        }


        /// <summary>
        /// Get the filtered samples newer than since time
        /// </summary>
        public IEnumerable<IBFSample> GetFilteredChunk(DateTimeOffset since)
        {
            return SignalFilter.GetFilteredData(since);
        }



        /// <summary>
        /// Get the current standard deviation median
        /// </summary>
        public IBFSample GetStdDevianMedians()
        {
            return StdDevMedians;
        }


        /// <summary>
        /// Get current band powers at specified frequency band (in Hz)
        /// </summary>
        public IBFSample GetBandPower(double band)
        {
            return BandPowers.GetBandPower(band);
        }


       


        #endregion


        //  Implementation
        #region Implementation

        /// <summary>
        /// Constructor
        /// </summary>
        public BrainflowDataProcessor(string name, int boardId, int sampleRate)
        {
            BoardId = boardId;
            NumberOfChannels = BoardShim.get_exg_channels(boardId).Length;
            SampleRate = sampleRate;
            Name = name;

            CreateChannelStdDevRunningCollection();

            UnfilteredData = new List<IBFSample>();
            DataToProcess = new ConcurrentQueue<IBFSample>();
            NotifyAddedData = new SemaphoreSlim(0);

            BandPowers = new BandPowerMonitor(Name, BoardId, SampleRate);
            BandPowers.GetRawChunk = GetRawChunk;
            BandPowers.Log += OnComponentLog;

            RealTimeBufferLengthSeconds = 10;

            SignalFilter = new SignalFiltering(name, boardId, sampleRate)
            {
                FilterBufferLength = RealTimeBufferLengthSeconds,
            };


            SignalFilter.GetRawChunk = GetRawChunk;
            SignalFilter.Log += OnComponentLog;

            TimeTagFirstSample = -1;

            //  default periods for periodic loops
            PeriodMsUpdateData = 200;       //  5Hz for update data message
            PeriodMsUpdateProcessorStats = 5000;   //  every five seconds update processor stats

            PeriodMsUpdateStdDev = 100;         //  10Hz std deviation update
            PeriodMsUpdateDataFilter = 200;     // 5 Hz data filter update
            PeriodMsFlushOldData = 2000;          // Flush old every two seconds

            ProcessingTimesQueue = new ConcurrentQueue<double>();
        }

        //  Properties
        public int BoardId { get; private set; }
        public int NumberOfChannels { get; private set; }
        public int SampleRate { get; private set; }
        public string Name { get; private set; }

        public double RealTimeBufferLengthSeconds { get; set; }

        //  Running tasks
        protected CancellationTokenSource CancelTokenSource { get; set; }
        protected Task RawDataQueueProcessorTask { get; set; }
        protected Task PeriodicProcessorTask { get; set; }
        protected Task DataMonitorTask { get; set; }

        // Processing queue to hold data to be processed
        protected SemaphoreSlim NotifyAddedData { get; set; }
        ConcurrentQueue<IBFSample> DataToProcess;
        protected bool FlushQueue;

        //  Collection of sensor observations that have been processed
        List<IBFSample> UnfilteredData;

        //  Running collection of observation standard deviation
        List<ConcurrentQueue<double>> ChannelSdtDevRunningCollection;

        //  Current calculated properties per channel kept in a data class
        public IBFSample StdDevMedians { get; private set; }

        //  Band power monitor
        protected BandPowerMonitor BandPowers { get; private set; }
        protected SignalFiltering SignalFilter { get; private set; }

        double TimeTagFirstSample { get; set; }


        //  Performance testing and monitoring
        ConcurrentQueue<double> ProcessingTimesQueue { get; set; }
        private int LastSampleIndex { get; set; }



        /// <summary>
        /// Create the collection of std deviations median
        /// </summary>
        private void CreateChannelStdDevRunningCollection()
        {
            ChannelSdtDevRunningCollection = new List<ConcurrentQueue<double>>();
            //  add a collection for each channel
            for (int i = 0; i < NumberOfChannels; i++)
                ChannelSdtDevRunningCollection.Add(new ConcurrentQueue<double>());

            StdDevMedians = new BFSampleImplementation(BoardId);
        }


        /// <summary>
        /// Clear out the std dev collections
        /// </summary>
        private void ClearStdDevRunningCollection()
        {
            for (int i = 0; i < NumberOfChannels; i++)
            {
                ChannelSdtDevRunningCollection[i].RemoveAll();
            }

            StdDevMedians = new BFSampleImplementation(BoardId);
        }


        /// <summary>
        /// Init the processor 
        /// </summary>
        private void InitializeProcessor()
        {
            //  clean out our collections
            lock (UnfilteredData)
                UnfilteredData.Clear();

            DataToProcess.RemoveAll();
            ClearStdDevRunningCollection();

            ProcessingTimesQueue.RemoveAll();
        }


        //  Data monitor update periods
        private int PeriodMsUpdateData { get; set; }
        private int PeriodMsUpdateProcessorStats { get; set; }

        /// <summary>
        /// Background task to monitor the data
        /// will send event with current data (to update the UI for example) at a frequency of 1/PeriodUpdateData
        /// will log calculation performance metrics at a frequency of 1/PeriodUpdateProcessorStats
        /// </summary>
        private async Task RunDataMonitorAsync(CancellationToken cancelToken)
        {
            try
            {
                var swData = new System.Diagnostics.Stopwatch();
                swData.Start();
                var swStats = new System.Diagnostics.Stopwatch();
                swStats.Start();

                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(1, cancelToken);

                        var timeNow = DateTimeOffset.UtcNow;

                        if (swData.ElapsedMilliseconds > PeriodMsUpdateData)
                        {
                            SendCurrentDataEvent();
                            swData.Restart();
                        }

                        if (swStats.ElapsedMilliseconds > PeriodMsUpdateProcessorStats)
                        {
                            LogProcessorStats(swStats.Elapsed);
                            swStats.Restart();
                        }
                    }
                    catch (OperationCanceledException)
                    { }
                    catch (Exception ex)
                    {
                        Log?.Invoke(this, new LogEventArgs(Name, this, "RunDataMonitorAsync", ex, LogLevel.ERROR));
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(Name, this, "RunDataMonitorAsync", e, LogLevel.FATAL));
            }
        }


        /// <summary>
        /// Send an event with the current state of the data
        /// intended to enable continuous update of UI, including signal that we do not have fresh data
        /// </summary>
        private void SendCurrentDataEvent()
        {
            ProcessorCurrentStateReportEventArgs report = new ProcessorCurrentStateReportEventArgs();

            if (UnfilteredData.Count > 0 /*&& DateTimeOffset.UtcNow.ToUnixTimeInDoubleSeconds() - Data.First().TimeStamp < 5*/)
            {

                report.CurrentSample = UnfilteredData.First();
                report.CurrentDeviation = GenerateDeviationReport(.25);
                report.CurrentDevMedian = StdDevMedians;
                foreach (var nextBandPower in BandPowers.GetBandPowers())
                {
                    report.BandPowers.Add(nextBandPower.Key, nextBandPower.Value);
                }
            }
            else
            {
                report.ValidData = false;
                report.Details = "Stale Data ";
            }

            CurrentDataStateReported?.Invoke(this, report);
        }


        /// <summary>
        /// Keep track of processor steps efficiency for benchmarking purposes
        /// </summary>
        private void LogProcessorStats(TimeSpan elapsed)
        {
            if ( ProcessingTimesQueue.Count > 0)
            {
                IBFSample first;
                lock (UnfilteredData)
                {
                    first = UnfilteredData.First();
                }

                var processor = "Not processing !";
                if (ProcessingTimesQueue.Count > 0)
                    processor = $"Queue {(ProcessingTimesQueue.Count / elapsed.TotalSeconds).ToString("F0")} times per second: Av {ProcessingTimesQueue.Average().ToString("F4")} s | Max {ProcessingTimesQueue.Max().ToString("F4")} s.";

               
                Log?.Invoke(this, new LogEventArgs(Name, this, "LogProcessorStats", $"{Name} processing: Age {(DateTimeOffset.UtcNow.ToUnixTimeInDoubleSeconds() - first.TimeStamp).ToString("F6")}.  {processor}", LogLevel.TRACE));

                if (CountMissingIndex > 0)
                {
                    Log?.Invoke(this, new LogEventArgs(Name, this, "LogProcessorStats", $"Missed {CountMissingIndex} samples in the past {(int)(PeriodMsUpdateProcessorStats / 1000)} seconds.", LogLevel.WARN));
                    CountMissingIndex = 0;
                }
                ProcessingTimesQueue.RemoveAll();
            }
        }


        /// <summary>
        /// Raw data processing queue task
        /// Uses notifications and will process the data one at a time
        /// </summary>
        private async Task RunDataQueueProcessorAsync(CancellationToken cancelToken)
        {
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    await NotifyAddedData.WaitAsync(cancelToken);

                    try
                    {
                        var sw = new System.Diagnostics.Stopwatch();
                        sw.Start();

                        DataToProcess.TryDequeue(out var nextSample);
                        ProcessDataQueue(nextSample);

                        sw.Stop();
                        ProcessingTimesQueue.Enqueue(sw.Elapsed.TotalSeconds);
                    }
                    catch (Exception ex)
                    {
                        Log?.Invoke(this, new LogEventArgs(Name, this, "RunProcessor", ex, LogLevel.ERROR));
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(Name, this, "RunProcessor", e, LogLevel.FATAL));
            }
            finally
            {
                //  flush queue is for automated testing purposes, to ensure your entire data set is processed before task exits
                if (FlushQueue)
                {
                    while (DataToProcess.Count > 0)
                    {
                        try
                        {
                            DataToProcess.TryDequeue(out var nextSample);
                            ProcessDataQueue(nextSample);
                        }
                        catch (Exception ex)
                        {
                            Log?.Invoke(this, new LogEventArgs(Name, this, "RunProcessor", ex, LogLevel.ERROR));
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Process the data in the queue
        /// </summary>
        private void ProcessDataQueue(IBFSample sample)
        {
            if (sample != null)
            {
                lock (UnfilteredData)
                    UnfilteredData.Insert(0, sample);

                InspectSampleIndex(sample);

                NewSample?.Invoke(this, new BFSampleEventArgs(sample));
            }
        }

        int SampleIndexDifference(int nextIndex)
        {
            int difference = -1;
            if (Math.Abs((LastSampleIndex - nextIndex) - 255) < 0.1)
            {
                difference = 1;
            }
            else if (nextIndex < LastSampleIndex)
            {
                difference = (int)nextIndex + (255 - (int)LastSampleIndex);
            }
            else
            {
                difference = nextIndex - LastSampleIndex;
            }
            LastSampleIndex = nextIndex;
            return difference;
        }


        private int CountMissingIndex;

        /// <summary>
        /// Check the sample index sequence
        /// log a warning if sample index is missing
        /// </summary>
        private void InspectSampleIndex(IBFSample sample)
        {
            var difference = SampleIndexDifference((int)(sample.SampleIndex));

            switch (BoardId)
            {
                case 0:
                    if (difference > 1)
                        CountMissingIndex++;
                    break;

                case 2:
                    if (difference > 2)
                        CountMissingIndex++;
                    break;

            }
        }


        //  Periodic monitoring
        public int PeriodMsUpdateStdDev { get; set; }
        public int PeriodMsUpdateDataFilter { get; set; }
        public int PeriodMsFlushOldData { get; set; }

        /// <summary>
        /// Periodic processing tasks
        /// </summary>
        private async Task RunPeriodicProcessorAsync(CancellationToken cancelToken)
        {
            try
            {
                var swDev = new System.Diagnostics.Stopwatch();
                swDev.Start();
                var swFilter = new System.Diagnostics.Stopwatch();
                swFilter.Start();
                var swFlush = new System.Diagnostics.Stopwatch();
                swFlush.Start();

                while (!cancelToken.IsCancellationRequested)
                {
                    await Task.Delay(1);


                    if (swDev.ElapsedMilliseconds > PeriodMsUpdateStdDev)
                    {
                        swDev.Restart();
                        UpdateStandardDeviations();
                    }

                    //if (swFilter.ElapsedMilliseconds > PeriodMsUpdateDataFilter)
                    //{
                    //    swFilter.Restart();
                    //    var data = GetUnfilteredData(1.1);
                    //    RunSomeDataFiltering(data);
                    //}

                    if (swFlush.ElapsedMilliseconds > PeriodMsFlushOldData)
                    {
                        FlushOldData(RealTimeBufferLengthSeconds);
                        swFlush.Restart();
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(Name, this, "RunProcessor", e, LogLevel.FATAL));
            }
        }

        private void UpdateStandardDeviations()
        {
            var data = GetUnfilteredData(.25);
            if (data != null && data.Count() > 0)
            {
                UpdateStdDevCollections(data);
                UpdateStdDevMedians();
            }
        }



        /// <summary>
        /// Update the running collection of standard deviations for each channel
        /// </summary>
        private void UpdateStdDevCollections(IEnumerable<IBFSample> data)
        {
            for (int i = 0; i < NumberOfChannels; i++)
            {
                UpdateStdDevCollection(ChannelSdtDevRunningCollection[i], data.GetExgDataForChannel(i).StdDev());
            }
        }


        /// <summary>
        /// Update the running list of standard deviations for a specific channel
        /// return the median std dev for the collection
        /// </summary>
        private double UpdateStdDevCollection(ConcurrentQueue<double> avgCollection, double stdDev)
        {
            try
            {
                avgCollection.Enqueue(stdDev);
                while (avgCollection.Count > 50)
                {
                    avgCollection.TryDequeue(out var throwAway);
                }
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(Name, this, "UpdateStdAverage", e, LogLevel.ERROR));
            }

            return avgCollection.Median();
        }


        /// <summary>
        /// Update the current running standard deviation median for each channel
        /// </summary>
        private void UpdateStdDevMedians()
        {
            for (int i = 0; i < NumberOfChannels; i++)
            {
                StdDevMedians.SetExgDataForChannel(i, ChannelSdtDevRunningCollection[i].Median());
            }
        }



       




        /// <summary>
        /// Get the last number of seconds of Exg data for the specified channel
        /// </summary>
        private IEnumerable<IBFSample> GetUnfilteredData(double seconds)
        {
            var data = new List<IBFSample>();
            lock (UnfilteredData)
            {
                var firstTimeStamp = UnfilteredData.Count > 0 ? UnfilteredData.First().TimeStamp : 0.0;
                foreach (var nextData in UnfilteredData)
                {
                    if (firstTimeStamp - nextData.TimeStamp < seconds)
                    {
                        data.Add(nextData);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return data;
        }


        /// <summary>
        /// Get a range of Exg data starting at from seconds ago, and ending at to seconds ago
        /// </summary>
        private IEnumerable<IBFSample> GetUnfilteredData(double from, double to)
        {
            var data = new List<IBFSample>();
            lock (UnfilteredData)
            {
                var firstTimeStamp = UnfilteredData.Count > 0 ? UnfilteredData.First().TimeStamp : 0.0;
                foreach (var nextData in UnfilteredData)
                {
                    if (firstTimeStamp - nextData.TimeStamp < from)
                        continue;
                    else if ((firstTimeStamp - nextData.TimeStamp >= from) && (firstTimeStamp - nextData.TimeStamp <= to))
                    {
                        data.Add(nextData);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return data;
        }


        /// <summary>
        /// Get the last number of seconds of Exg data for the specified channel
        /// </summary>
        private IEnumerable<IBFSample> GetUnfilteredData(DateTimeOffset since)
        {
            var data = new List<IBFSample>();

            lock (UnfilteredData)
            {
                foreach (var nextData in UnfilteredData)
                {
                    if (nextData.ObservationTime > since)
                    {
                        data.Add(nextData);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return data;
        }



        /// <summary>
        /// Remove specified seconds of old data from the list
        /// </summary>
        private void FlushOldData(double seconds)
        {
            lock (UnfilteredData)
            {
                //  keep only 30 seconds of data
                while (UnfilteredData.Count > 0 && ((UnfilteredData.First().TimeStamp - UnfilteredData.Last().TimeStamp) > seconds))
                    UnfilteredData.RemoveAt(UnfilteredData.Count - 1);
            }

        }


        /// <summary>
        /// Calculate the deviation for each Exg channel with specified seconds worth of data
        /// </summary>
        private IBFSample GenerateDeviationReport(double seconds)
        {
            var devReport = new BFSampleImplementation(BoardId);
            var data = GetUnfilteredData(seconds);

            for (int i = 0; i < NumberOfChannels; i++)
            {
                devReport.SetExgDataForChannel(i, data.GetExgDataForChannel(i).StdDev());
            }

            return devReport;
        }


        /// <summary>
        /// Calculate the average for each Exg channel using specified seconds worth of data
        /// </summary>
        private IBFSample GenerateAverageReport(double seconds)
        {
            var averageReport = new BFSampleImplementation(BoardId);
            var data = GetUnfilteredData(seconds);

            for (int i = 0; i < NumberOfChannels; i++)
            {
                averageReport.SetExgDataForChannel(i, data.GetExgDataForChannel(i).Average());
            }

            return averageReport;
        }

        /// <summary>
        /// Calculate the median for each Exg channel using specified seconds worth of data
        /// </summary>
        private IBFSample GenerateMedianReport(double seconds)
        {
            var medianReport = new BFSampleImplementation(BoardId);
            var data = GetUnfilteredData(seconds);

            for (int i = 0; i < NumberOfChannels; i++)
            {
                medianReport.SetExgDataForChannel(i, data.GetExgDataForChannel(i).Median());
            }

            return medianReport;
        }




        /// <summary>
        /// Pass through function for members with log function to forward log events
        /// </summary>
        private void OnComponentLog(object sender, LogEventArgs e)
        {
            Log?.Invoke(sender, e);
        }

        #endregion
    }
}

