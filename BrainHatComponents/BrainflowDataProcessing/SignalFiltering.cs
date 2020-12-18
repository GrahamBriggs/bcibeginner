﻿using brainflow;
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
        public GetBFChunkDelegate GetUnfilteredData;

        /// <summary>
        /// Period in milliseconds for the filter to update
        /// </summary>
        public int PeriodMilliseconds { get; set; }

        /// <summary>
        /// Amount of raw data in seconds the filter will process
        /// </summary>
        public double FilterBufferLength { get; set; }


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
        /// <param name="seconds"></param>
        /// <returns></returns>
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

            PeriodMilliseconds = 200;   //  default 5 Hz

            FilterBufferLength = 5;

            ProcessingTimes = new ConcurrentQueue<double>();


            FilteredData = new ConcurrentStack<IBFSample>();
        }

        //  Board Properties
        public int BoardId { get; protected set; }
        public int NumberOfChannels { get; protected set; }
        public int SampleRate { get; protected set; }
        public string Name { get; protected set; }

        //  Filtered Data Collection
        public ConcurrentStack<IBFSample> FilteredData { get; protected set; }

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

                var data = GetUnfilteredData(5.0);


                if (data == null || data.Count() == 0)
                {
                    FilteredData.RemoveAll();
                    return;
                }

                var filteredSamples = new List<IBFSample>();
                foreach (var nextSample in data)
                    filteredSamples.Add(MakeNewSample(nextSample));

                for (int i = 0; i < NumberOfChannels; i++)
                {
                    //var filtered = DataFilter.perform_rolling_filter(data.GetExgDataForChannel(i), 3, (int)AggOperations.EACH);
                    // var filtered = DataFilter.perform_bandpass(data.GetExgDataForChannel(i), SampleRate, 15, 30, 2, (int)FilterTypes.BESSEL, 0.0);
                    //var filtered = DataFilter.perform_bandstop(data.GetExgDataForChannel(i), SampleRate, 50.0, 1.0, 6, (int)FilterTypes.CHEBYSHEV_TYPE_1, 1.0);
                    var filtered = DataFilter.perform_bandpass(data.GetExgDataForChannel(i), SampleRate, 15.0, 5.0, 2, (int)FilterTypes.BUTTERWORTH, 0.0);

                    for (int j = 0; j < data.Count(); j++)
                    {
                        if (i == 0)
                            filteredSamples[j].TimeStamp = filteredSamples[0].TimeStamp - (1.0 / SampleRate) * j;
                        filteredSamples[j].SetExgDataForChannel(i, filtered[j]);
                    }
                }

                //  remove the first bit to avoid filtering artifact
                if ( filteredSamples.Count > SampleRate )
                    filteredSamples.RemoveRange(0, (SampleRate));

                //  update the filtered data collection
                FilteredData.RemoveAll();
                FilteredData.PushRange(filteredSamples.ToArray());

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