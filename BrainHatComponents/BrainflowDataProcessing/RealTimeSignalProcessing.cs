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
    public class RealTimeSignalProcessing
    {
        public static string KeyName(string filterName, string montageName)
        {
            return $"{montageName}$$$$${filterName}";
        }

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
        public double FilterBufferLength { get; set; }


        //  Public Methods
        #region PublicMethods

        /// <summary>
        /// Start async task to run data processing
        /// </summary>
        public async Task StartRealTimeProcessingAsync()
        {
            await StopRealTimeProcessingAsyncAsync();

            FilteredData.RemoveAll();

            // fire off the run tasks
            CancelTokenSource = new CancellationTokenSource();
            MonitorRunTask = RunSignalFilteringAsync(CancelTokenSource.Token);

            Log?.Invoke(this, new LogEventArgs(Name, this, "StartMonitorAsync", $"Started signal filtering for {Name}.", LogLevel.INFO));
        }


        /// <summary>
        /// Stop the data processor async task
        /// </summary>
        public async Task StopRealTimeProcessingAsyncAsync()
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
        public IBFSample[] GetFilteredData(double seconds)
        {
            if (FilteredData.Count > 0)
            {
                var last = FilteredData.Last().TimeStamp;
                var filtered = FilteredData.Where(x => (last - x.TimeStamp) < seconds);
                return filtered.ToArray();
            }

            return null;
        }


        /// <summary>
        /// Get filtered data newer than since time
        /// </summary>
        public IBFSample[] GetFilteredData(DateTimeOffset since)
        {
            if (FilteredData.Count > 0)
            {
                var filtered = FilteredData.Where(x => x.ObservationTime > since);
                return filtered.ToArray();
            }

            return null;
        }


        #endregion


        //  Implementation
        #region Implementation


        /// <summary>
        /// Constructor
        /// </summary>
        public RealTimeSignalProcessing(int boardId, int sampleRate, SignalFilter filter, ISignalMontage montage)
        {
            BoardId = boardId;
            NumberOfChannels = BrainhatBoardShim.GetNumberOfExgChannels(BoardId);
            SampleRate = sampleRate;
            
            Filter = filter;
            Montage = montage;

            Name = KeyName((Filter == null ? "" : Filter.Name), Montage.Name);

            PeriodMilliseconds = 33;

            FilterBufferLength = 10;

            ProcessingTimes = new ConcurrentQueue<double>();


            FilteredData = new ConcurrentQueue<IBFSample>();
        }

        //  Board Properties
        public int BoardId { get; private set; }
        public int NumberOfChannels { get; private set; }
        public int SampleRate { get; private set; }
        public string Name { get; private set; }
        public string FilterName => Filter.Name;

        ISignalMontage Montage;
        SignalFilter Filter;

        //  Filtered Data Collection
        public ConcurrentQueue<IBFSample> FilteredData { get; private set; }

        //  Run task properties
        CancellationTokenSource CancelTokenSource;
        Task MonitorRunTask;
        ConcurrentQueue<double> ProcessingTimes;


        /// <summary>
        /// Run function, spins and updates the filter at the specified period
        /// </summary>
        async Task RunSignalFilteringAsync(CancellationToken cancelToken)
        {
            try
            {
                var swDetect = new System.Diagnostics.Stopwatch();
                var swReport = new System.Diagnostics.Stopwatch();
                var swClean = new System.Diagnostics.Stopwatch();
                swDetect.Start();
                swReport.Start();
                swClean.Start();
                while (!cancelToken.IsCancellationRequested)
                {
                    await Task.Delay(1);

                    if (swDetect.ElapsedMilliseconds >= PeriodMilliseconds)
                    {
                        swDetect.Restart();
                        FilterSignal();
                    }

                    if (swClean.ElapsedMilliseconds >= 1000)
                    {
                        var newestSample = FilteredData.LastOrDefault();
                        if (newestSample != null)
                        {
                            while (newestSample.TimeStamp - FilteredData.First().TimeStamp > FilterBufferLength)
                                FilteredData.TryDequeue(out var discard);
                        }

                        swClean.Restart();
                    }

                    if (swReport.ElapsedMilliseconds >= 5000)
                    {
                        System.Diagnostics.Debug.WriteLine("Signal filtering");

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


        /// <summary>
        /// Apply the filter to the signal
        /// </summary>
        void FilterSignal()
        {
            try
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                var rawSamples = GetRawChunk(3);

                var filteredSamples = Montage.ApplyMontage(rawSamples, Filter, BoardId, NumberOfChannels, SampleRate);

                var oldestSample = FilteredData.LastOrDefault()?.TimeStamp ?? filteredSamples[0].TimeStamp;
                FilteredData.AddRange(filteredSamples.Where(x => x.TimeStamp > oldestSample));

                sw.Stop();
                ProcessingTimes.Enqueue(sw.Elapsed.TotalSeconds);
            }
            catch (ArgumentException ae)
            {
                Log?.Invoke(this, new LogEventArgs(Name, this, "FilterSignal", ae, LogLevel.WARN));
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(Name, this, "FilterSignal", e, LogLevel.ERROR));
            }
        }


        #endregion
    }
}
