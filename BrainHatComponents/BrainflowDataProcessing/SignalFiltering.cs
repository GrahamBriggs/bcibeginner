using brainflow;
using BrainflowInterfaces;
using LoggingInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    public class SignalFiltering
    {
        //  Events
        public event LogEventDelegate Log;

        //  Delegates
        public GetBFChunkSecondsDelegate GetRawChunk;

        /// <summary>
        /// Period in milliseconds for the filter to update
        /// </summary>
        public int PeriodMilliseconds { get; set; }

        /// <summary>
        /// Amount of raw data in seconds the filter will retain
        /// </summary>
        public double FilterBufferLength { get;  set; }


        //  Public Methods
        #region PublicMethods

        /// <summary>
        /// Start async task to run data processing
        /// </summary>
        public async Task StartSignalFilteringAsync()
        {
            await StopSignalFilteringAsync();

            // fire off the run tasks
            CancelTokenSource = new CancellationTokenSource();
            MonitorRunTask = RunSignalFilteringAsync(CancelTokenSource.Token);

            Log?.Invoke(this, new LogEventArgs(Name, this, "StartMonitorAsync", $"Started signal filtering for {Name}.", LogLevel.INFO));
        }

       

        /// <summary>
        /// Stop the data processor async task
        /// </summary>
        public async Task StopSignalFilteringAsync()
        {
            if (CancelTokenSource != null)
            {
                CancelTokenSource.Cancel();
                await MonitorRunTask;

                CancelTokenSource = null;
                MonitorRunTask = null;

                Log?.Invoke(this, new LogEventArgs(Name, this, "StopMonitorAsync", $"Stopped signal filtering for {Name}.", LogLevel.INFO));
            }
        }


        /// <summary>
        /// Get some number of seconds of the filtered data
        /// </summary>
        public IEnumerable<IBFSample> GetFilteredData(double seconds)
        {
            if ( FilteredData.Count > 0 )
            {
                var first = FilteredData.First().TimeStamp;
                var filtered = FilteredData.Where(x => (first - x.TimeStamp) < seconds);
                return filtered;
            }

            return null;
        }


        /// <summary>
        /// Get filtered data newer than since time
        /// </summary>
        public IEnumerable<IBFSample> GetFilteredData(DateTimeOffset since)
        {
            if (FilteredData.Count > 0)
            {
                var filtered = FilteredData.Where(x => x.ObservationTime > since);
                return filtered;
            }

            return null;
        }


        #endregion


        //  Implementation
        #region Implementation


        /// <summary>
        /// Constructor
        /// </summary>
        public SignalFiltering(string name, int boardId, int sampleRate)
        {
            BoardId = boardId;
            NumberOfChannels = BoardShim.get_exg_channels(BoardId).Length;
            SampleRate = sampleRate;
            Name = name;

            PeriodMilliseconds = 50;   //  default 20 Hz

            FilterBufferLength = 10;

            ProcessingTimes = new ConcurrentQueue<double>();


            FilteredData = new ConcurrentQueue<IBFSample>();
        }

        //  Board Properties
        public int BoardId { get; protected set; }
        public int NumberOfChannels { get; protected set; }
        public int SampleRate { get; protected set; }
        public string Name { get; protected set; }

        //  Filtered Data Collection
        public ConcurrentQueue<IBFSample> FilteredData { get; protected set; }

        //  Run task properties
        protected CancellationTokenSource CancelTokenSource { get; set; }
        protected Task MonitorRunTask { get; set; }
        ConcurrentQueue<double> ProcessingTimes { get; set; }


        /// <summary>
        /// Run function, spins and updates the filter at the specified period
        /// </summary>
        private async Task RunSignalFilteringAsync(CancellationToken cancelToken)
        {
            try
            {
                var swDetect = new System.Diagnostics.Stopwatch();
                var swReport = new System.Diagnostics.Stopwatch();
                swDetect.Start();
                swReport.Start();
                while (!cancelToken.IsCancellationRequested)
                {
                    await Task.Delay(1);

                    if (swDetect.ElapsedMilliseconds >= PeriodMilliseconds)
                    {
                        swDetect.Restart();
                        FilterSignal();

                    }

                    if (swReport.ElapsedMilliseconds >= 30000)
                    {
                        if (ProcessingTimes.Count > 0)
                        {
                            Log?.Invoke(this, new LogEventArgs(Name, this, "RunSignalFilteringAsync", $"{Name} signal filtering {(ProcessingTimes.Count / swReport.Elapsed.TotalSeconds):F0} times per second: med {ProcessingTimes.Median():F4} s | max {ProcessingTimes.Max():F4} s.", LogLevel.TRACE));
                            ProcessingTimes.RemoveAll();
                        }
                        swReport.Restart();
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(Name, this, "RunBandPowerMonitorAsync", e, LogLevel.FATAL));
            }
        }


        private void FilterSignal()
        {
            try
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                var rawSamples = GetRawChunk(FilterBufferLength);

                if (rawSamples == null || rawSamples.Count() == 0)
                {
                    return;
                }

                //  copy the data for filtering
                var filteredSamples = new List<IBFSample>(rawSamples.Select(x => MakeNewSample(x)));

                for (int i = 0; i < NumberOfChannels; i++)
                {
                    //var filtered = DataFilter.perform_rolling_filter(data.GetExgDataForChannel(i), 3, (int)AggOperations.EACH);
                    // var filtered = DataFilter.perform_bandpass(data.GetExgDataForChannel(i), SampleRate, 15, 30, 2, (int)FilterTypes.BESSEL, 0.0);
                    //var filtered = DataFilter.perform_bandstop(data.GetExgDataForChannel(i), SampleRate, 50.0, 1.0, 6, (int)FilterTypes.CHEBYSHEV_TYPE_1, 1.0);
                    var samples = rawSamples.GetExgDataForChannel(i);
                    var filtered = DataFilter.perform_bandpass(samples, SampleRate, 15.0, 5.0, 2, (int)FilterTypes.BUTTERWORTH, 0.0);

                    for (int j = 0; j < rawSamples.Count(); j++)
                    {
                        filteredSamples[j].SetExgDataForChannel(i, filtered[j]);
                    }
                }

                var startTime = rawSamples.First().TimeStamp;

                for (int i = 0; i < filteredSamples.Count; i++)
                    filteredSamples[i].TimeStamp = startTime + (1.0 / SampleRate * i);


                FilteredData.RemoveAll();
                FilteredData.AddRange(filteredSamples);

                sw.Stop();
                ProcessingTimes.Enqueue(sw.Elapsed.TotalSeconds);
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(Name, this, "FilterSignal", e, LogLevel.ERROR));
            }
        }

      

        private IBFSample MakeNewSample(IBFSample sample)
        {
            if (sample is BFCyton8Sample cyton8Sample)
                return new BFCyton8Sample(cyton8Sample);
            else if (sample is BFCyton16Sample cyton16Sample)
                return new BFCyton16Sample(cyton16Sample);
            else
                return null;    // TODO - ganglion
        }

        #endregion
    }
}
