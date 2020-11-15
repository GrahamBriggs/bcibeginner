using brainflow;
using LoggingInterface;
using OpenBCIInterfaces;
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
        public event OpenBciCyton8ReadingEventDelegate NewReading;


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

            await BandPowers.StartMonitorAsync();

            TimeTagFirstReading = -1;

            Log?.Invoke(this, new LogEventArgs(this, "StartDataProcessor", $"Starting Brainflow data processor.", LogLevel.INFO));
        }
        

        /// <summary>
        /// Stop the data processor async task
        /// </summary>
        public async Task StopDataProcessorAsync(bool flush = false)
        {
            FlushQueue = flush;

            if (CancelTokenSource != null)
            {
                await BandPowers.StopMonitorAsync();

                CancelTokenSource.Cancel();
                await Task.WhenAll(RawDataQueueProcessorTask, DataMonitorTask, PeriodicProcessorTask);
                
                CancelTokenSource = null;
                RawDataQueueProcessorTask = null;
                PeriodicProcessorTask = null;
                DataMonitorTask = null;
            }

            Log?.Invoke(this, new LogEventArgs(this, "StopDataProcessor", $"Stopped Brainflow data processor.", LogLevel.INFO));
        }


        /// <summary>
        /// Setup the board that we are processing data from
        /// </summary>
        public void SetBoard(int boardId)
        {
            if (BoardId < -1)
            {
                BoardId = boardId;
                SampleRate = BoardShim.get_sampling_rate(BoardId);
                NumberOfChannels = BoardShim.get_eeg_channels(BoardId).Length;
                BandPowers.SetBoardProperties(NumberOfChannels, SampleRate);

                CreateChannelStdDevRunningCollection();
            }
            else
            {
                Log?.Invoke(this, new LogEventArgs(this, "SetBoard", $"Changing board is not supported.", LogLevel.ERROR));
            }
        }


        /// <summary>
        /// Add data to the proecssor queue
        /// </summary>
        public void AddDataToProcessor(OpenBciCyton8Reading data)
        {
            DataToProcess.Enqueue(data);
            NotifyAddedData.Release();
        }


        /// <summary>
        /// Get the most recent 'seconds' worth of raw data from the processor
        /// </summary>
        public IEnumerable<OpenBciCyton8Reading> GetRawData(double seconds)
        {
            return GetUnfilteredData(seconds);
        }
              

        /// <summary>
        /// Get a range of raw data starting at 'from' seconds ago, and ending at 'to' seconds ago 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public IEnumerable<OpenBciCyton8Reading> GetRawData(double from, double to)
        {
            return GetUnfilteredData(from, to);
        }


        /// <summary>
        /// Get the current standard deviation median
        /// </summary>
        /// <returns></returns>
        public OpenBciCyton8Reading GetStdDevianMedians()
        {
            return StdDevMedians;
        }


        /// <summary>
        /// Get current band powers at specified frequency band (in Hz)
        /// </summary>
        public OpenBciCyton8Reading GetBandPower(int band)
        {
            return BandPowers.GetBandPower(band);
        }


        #endregion


        //  Implementation
        #region Implementation

        /// <summary>
        /// Constructor
        /// </summary>
        public BrainflowDataProcessor()
        {
            BoardId = -99;

            Data = new List<OpenBciCyton8Reading>();
            DataToProcess = new ConcurrentQueue<OpenBciCyton8Reading>();
            NotifyAddedData = new SemaphoreSlim(0);

            BandPowers = new BandPowerMonitor();
            BandPowers.GetData = GetUnfilteredData;
            BandPowers.Log += OnComponentLog;
        
            TimeTagFirstReading = -1;
                    
            //  default periods for periodic loops
            PeriodMsUpdateData = 200;       //  5Hz for update data message
            PeriodMsUpdateProcessorStats = 5000;   //  every five seconds update processor stats
   
            PeriodMsUpdateStdDev = 100;         //  10Hz std deviation update
            PeriodMsUpdateDataFilter = 200;     // 5 Hz data filter update
            PeriodMsFlushOldData = 2000;          // Flush old every two seconds
            
            ProcessingTimesFilter = new ConcurrentQueue<double>();
            ProcessingTimesQueue = new ConcurrentQueue<double>();
        }

        //  Properties
        public int BoardId { get; private set; }
        public int NumberOfChannels { get; private set; }
        public int SampleRate { get; private set; }
                
        //  Running tasks
        protected CancellationTokenSource CancelTokenSource { get; set; }
        protected Task RawDataQueueProcessorTask { get; set; }
        protected Task PeriodicProcessorTask { get; set; }
        protected Task DataMonitorTask { get; set; }

        // Processing queue to hold data to be processed
        protected SemaphoreSlim NotifyAddedData { get; set; }
        ConcurrentQueue<OpenBciCyton8Reading> DataToProcess;
        protected bool FlushQueue;

        //  Collection of sensor observations that have been processed
        List<OpenBciCyton8Reading> Data;

        //  Running collection of observation standard deviation
        List<ConcurrentQueue<double>> ChannelSdtDevRunningCollection;
        
        //  Current calculated properties per channel kept in a data class
        public OpenBciCyton8Reading StdDevMedians { get; private set; }

        //  Band power monitor
        protected BandPowerMonitor BandPowers { get; private set; }

        double TimeTagFirstReading { get; set; }
        

        //  Performance testing and monitoring
        ConcurrentQueue<double> ProcessingTimesQueue { get; set; }
        ConcurrentQueue<double> ProcessingTimesFilter { get; set; }
        private int LastSampleIndex { get; set; }



        /// <summary>
        /// Create the collection of std deviations median
        /// </summary>
        private void CreateChannelStdDevRunningCollection()
        {
            ChannelSdtDevRunningCollection = new List<ConcurrentQueue<double>>();
            //  add a collection for each channel
            for(int i = 0; i < NumberOfChannels; i++)
                ChannelSdtDevRunningCollection.Add(new ConcurrentQueue<double>());

            StdDevMedians = new OpenBciCyton8Reading();
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

            StdDevMedians = new OpenBciCyton8Reading();
        }


        /// <summary>
        /// Init the processor 
        /// </summary>
        private void InitializeProcessor()
        {
            //  clean out our collections
            lock (Data)
                Data.Clear();

            DataToProcess.RemoveAll();
            ClearStdDevRunningCollection();

            ProcessingTimesQueue.RemoveAll();
            ProcessingTimesFilter.RemoveAll();

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
                var swGc = new System.Diagnostics.Stopwatch();
                swGc.Start();

                while (!cancelToken.IsCancellationRequested)
                {
                    await Task.Delay(1, cancelToken);

                    var timeNow = DateTimeOffset.UtcNow;

                    if ( swData.ElapsedMilliseconds > PeriodMsUpdateData)
                    {
                        SendCurrentDataEvent();
                        swData.Restart();
                    }

                    if ( swStats.ElapsedMilliseconds > PeriodMsUpdateProcessorStats)
                    {
                        LogProcessorStats(swStats.Elapsed);
                        swStats.Restart();
                    }

                    if (swGc.Elapsed > TimeSpan.FromSeconds(120))
                    {
                        swGc.Restart();
                        GC.Collect();
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch ( Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunDataMonitorTask", e, LogLevel.FATAL));
            }
        }
        

        /// <summary>
        /// Send an event with the current state of the data
        /// intended to enable continuous update of UI, including signal that we do not have fresh data
        /// </summary>
        private void SendCurrentDataEvent()
        {
            ProcessorCurrentStateReportEventArgs report = new ProcessorCurrentStateReportEventArgs();

            if (Data.Count > 0 && DateTimeOffset.UtcNow.ToUnixTimeInDoubleSeconds() - Data.First().TimeStamp < 5)
            {

                report.CurrentReading = Data.First();
                report.CurrentDeviation = GenerateDeviationReport(.25);
                report.CurrentDevMedian = StdDevMedians;
                report.CurrentBandPower08 = BandPowers.GetBandPower(8);
                report.CurrentBandPower10 = BandPowers.GetBandPower(10);
                report.CurrentBandPower12 = BandPowers.GetBandPower(12);
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
            if ( ProcessingTimesFilter.Count > 0 && ProcessingTimesQueue.Count > 0)
            {
                OpenBciCyton8Reading first;
                lock (Data)
                {
                    first = Data.First();
                }

                var processor = "Not processing !";
                if ( ProcessingTimesQueue.Count > 0 )
                    processor = $"Queue {(ProcessingTimesQueue.Count/elapsed.TotalSeconds).ToString("F0")} times per second: Av {ProcessingTimesQueue.Average().ToString("F4")} s | Max {ProcessingTimesQueue.Max().ToString("F4")} s.";

                var filter = "Not filtering !";
                if (ProcessingTimesFilter.Count > 0)
                    filter = $"Filter {(ProcessingTimesFilter.Count/elapsed.TotalSeconds).ToString("F0")} times per second: Av {ProcessingTimesFilter.Average().ToString("F4")} s | Max {ProcessingTimesFilter.Max().ToString("F4")} s.";
                
                Log?.Invoke(this, new LogEventArgs(this, "LogProcessorStats", $"Processing: Age {(DateTimeOffset.UtcNow.ToUnixTimeInDoubleSeconds() - first.TimeStamp).ToString("F6")}.  {processor}  {filter}", LogLevel.TRACE));
                
                ProcessingTimesFilter.RemoveAll();
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

                        DataToProcess.TryDequeue(out var nextReading);
                        ProcessDataQueue(nextReading);

                        sw.Stop();
                        ProcessingTimesQueue.Enqueue(sw.Elapsed.TotalSeconds);
                    }
                    catch(Exception ex)
                    {
                        Log?.Invoke(this, new LogEventArgs(this, "RunProcessor", ex, LogLevel.ERROR));
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunProcessor", e, LogLevel.FATAL));
            }
            finally
            {
                //  flush queue is for automated testing purposes, to ensure your entire data set is processed before task exits
                if (FlushQueue)
                {
                    while ( DataToProcess.Count > 0)
                    {
                        try
                        {
                            DataToProcess.TryDequeue(out var nextReading);
                            ProcessDataQueue(nextReading);
                        }
                        catch (Exception ex)
                        {
                            Log?.Invoke(this, new LogEventArgs(this, "RunProcessor", ex, LogLevel.ERROR));
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Process the data in the queue
        /// </summary>
        private void ProcessDataQueue(OpenBciCyton8Reading reading)
        {
            lock (Data)
                Data.Insert(0, reading);

            InspectSampleIndex(reading);

            NewReading?.Invoke(this, new OpenBciCyton8ReadingEventArgs(reading));
        }


        /// <summary>
        /// Check the sample index sequence
        /// log a warning if sample index is missing
        /// </summary>
        private void InspectSampleIndex(OpenBciCyton8Reading reading)
        {
            if (LastSampleIndex < 0)
            {
                //  this is the very first one, remember it
                LastSampleIndex = (int)reading.SampleIndex;
                return;
            }
            else
            {
                switch (LastSampleIndex)
                {
                    case 255:
                        //  roll over condition
                        if ((int)reading.SampleIndex == 0)
                        {
                            LastSampleIndex = 0;
                            return;
                        }
                        break;

                    default:
                        if ((int)reading.SampleIndex - LastSampleIndex == 1)
                        {
                            LastSampleIndex = (int)reading.SampleIndex;
                            return;
                        }
                        break;
                }
            }

            //  there was a sample index mismatch
            Log?.Invoke(this, new LogEventArgs(this, "InspectSampleIndex", $"Mismatch of sample index {(int)reading.SampleIndex} Last Index {LastSampleIndex}. At reading {reading.TimeStamp.ToString("F6")}.", LogLevel.DEBUG));
            LastSampleIndex = (int)reading.SampleIndex;
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

                    if (swFilter.ElapsedMilliseconds > PeriodMsUpdateDataFilter)
                    {
                        swFilter.Restart();
                        var data = GetUnfilteredData(1.1);
                        RunSomeDataFiltering(data);
                    }

                    if (swFlush.ElapsedMilliseconds > PeriodMsFlushOldData)
                    {
                        FlushOldData(30.0);
                        swFlush.Restart();
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunProcessor", e, LogLevel.FATAL));
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
        private void UpdateStdDevCollections(IEnumerable<OpenBciCyton8Reading> data)
        {
            for (int i = 0; i < NumberOfChannels; i ++)
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
                Log?.Invoke(this, new LogEventArgs(this, "UpdateStdAverage", e, LogLevel.ERROR));
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
                StdDevMedians.SetExgData(i, ChannelSdtDevRunningCollection[i].Median());
            }
        }



        /// <summary>
        /// Run the data filter from brainflow example code on all channels
        /// </summary>
        private void RunSomeDataFiltering(IEnumerable<OpenBciCyton8Reading> data)
        {
            if (data == null || data.Count() == 0)
                return;

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < NumberOfChannels; i++)
            {
                var result = DataFilter.perform_lowpass(data.GetExgDataForChannel(i), SampleRate, 20.0, 4, (int)FilterTypes.BESSEL, 0.0);
                result = DataFilter.perform_highpass(result, SampleRate, 2.0, 4, (int)FilterTypes.BUTTERWORTH, 0.0);
                DataFilter.perform_bandpass(result, SampleRate, 15.0, 5.0, 2, (int)FilterTypes.BUTTERWORTH, 0.0);
            }

            sw.Stop();
            ProcessingTimesFilter.Enqueue(sw.Elapsed.TotalSeconds);
        }




        /// <summary>
        /// Get the last number of seconds of Exg data for the specified channel
        /// </summary>
        private IEnumerable<OpenBciCyton8Reading> GetUnfilteredData( double seconds)
        {
            if (Data.Count < 1)
                return null;

            var data = new List<OpenBciCyton8Reading>();
            lock (Data)
            {
                var firstTimeStamp = Data.First().TimeStamp;
                foreach (var nextData in Data)
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
        private IEnumerable<OpenBciCyton8Reading> GetUnfilteredData(double from, double to)
        {
            var data = new List<OpenBciCyton8Reading>();
            lock (Data)
            {
                var firstTimeStamp = Data.First().TimeStamp;
                foreach (var nextData in Data)
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
        /// Remove specified seconds of old data from the list
        /// </summary>
        private void FlushOldData(double seconds)
        {
            lock (Data)
            {
                //  keep only 30 seconds of data
                while (Data.Count > 0 && ((Data.First().TimeStamp - Data.Last().TimeStamp) > seconds))
                    Data.RemoveAt(Data.Count - 1);
            }

        }


        /// <summary>
        /// Calculate the deviation for each Exg channel with specified seconds worth of data
        /// </summary>
        private OpenBciCyton8Reading GenerateDeviationReport(double seconds)
        {
            var devReport = new OpenBciCyton8Reading();
            var data = GetUnfilteredData(seconds);

            for(int i = 0; i < NumberOfChannels; i++ )
            {
                devReport.SetExgData(i, data.GetExgDataForChannel(i).StdDev());
            }

            return devReport;
        }


        /// <summary>
        /// Calculate the average for each Exg channel using specified seconds worth of data
        /// </summary>
        private OpenBciCyton8Reading GenerateAverageReport(double seconds)
        {
            var averageReport = new OpenBciCyton8Reading();
            var data = GetUnfilteredData(seconds);

            for (int i = 0; i < NumberOfChannels; i++)
            {
                averageReport.SetExgData(i, data.GetExgDataForChannel(i).Average());
            }

            return averageReport;
        }

        /// <summary>
        /// Calculate the median for each Exg channel using specified seconds worth of data
        /// </summary>
        private OpenBciCyton8Reading GenerateMedianReport(double seconds)
        {
            var medianReport = new OpenBciCyton8Reading();
            var data = GetUnfilteredData(seconds);

            for (int i = 0; i < NumberOfChannels; i++)
            {
                medianReport.SetExgData(i, data.GetExgDataForChannel(i).Median());
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

