using LoggingInterfaces;
using BrainflowInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using brainflow;

namespace BrainflowDataProcessing
{
    /// <summary>
    /// Band power monitor
    /// Runs real time calculation of band powers at specified period update rate
    /// </summary>
    public class BandPowerMonitor
    {
        //  Events
        public event LogEventDelegate Log;

        //  Delegates
        public GetBFChunkSecondsDelegate GetRawChunk;

        //  Update rate period
        public int PeriodMilliseconds { get; set; }


        //  Public Methods
        #region PublicMethods

        /// <summary>
        /// Setup the band powers range lists
        /// </summary>
        /// <param name="rangeList"></param>
        public void SetBandPowerRangeList()
        {
            BandPowers = new IBFSample[BandPowerCalc.NumberOfBands];

            //  create a dictionary for the results of the band power calculation
            //  must match the number of frequency ranges list above
            BandPowersCollection = new ConcurrentDictionary<string, IBFSample>();
            for (int j = 0; j < BandPowerCalc.NumberOfBands; j++)
            {
                BandPowers[j] = BFSample.MakeNewSample(BoardId);

                var key = (BandPowerCalc.BandPowerCalcRangeList[j].Item1 + (BandPowerCalc.BandPowerCalcRangeList[j].Item2 - BandPowerCalc.BandPowerCalcRangeList[j].Item1) / 2).BandPowerKey();
                BandPowersCollection.TryAdd(key, BandPowers[j]);
            }
        }


        /// <summary>
        /// Start async task to run data processing
        /// </summary>
        public async Task StartMonitorAsync()
        {
            await StopMonitorAsync();

            // fire off the run tasks
            CancelTokenSource = new CancellationTokenSource();
            MonitorRunTask = RunBadPowerMonitorAsync(CancelTokenSource.Token);

            Log?.Invoke(this, new LogEventArgs(Name, this, "StartMonitorAsync", $"Starting band power monnitor for {Name}.", LogLevel.INFO));
        }


        /// <summary>
        /// Stop the data processor async task
        /// </summary>
        public async Task StopMonitorAsync()
        {
            if (CancelTokenSource != null)
            {
                CancelTokenSource.Cancel();
                await MonitorRunTask;

                CancelTokenSource = null;
                MonitorRunTask = null;

                Log?.Invoke(this, new LogEventArgs(Name, this, "StopMonitorAsync", $"Stopped band power monitor for {Name}.", LogLevel.INFO));
            }
        }


        /// <summary>
        /// Get the current band power for all channels at the specified band
        /// </summary>
        public IBFSample GetBandPower(double band)
        {
            if (BandPowersCollection.ContainsKey(band.BandPowerKey()))
                return BandPowersCollection[band.BandPowerKey()];

            return null;
        }


        /// <summary>
        /// Get enumerable of the band powers we are calculating
        /// </summary>
        public IEnumerable<KeyValuePair<string,IBFSample>> GetBandPowers()
        {
            foreach (var nextBandPower in BandPowersCollection)
                yield return nextBandPower;
        }


        #endregion


        //  Implementation
        #region Implementation

        /// <summary>
        /// Constructor
        /// </summary>
        public BandPowerMonitor(string name, int boardId, int sampleRate)
        {
            BoardId = boardId;
            NumberOfChannels = BoardShim.get_exg_channels(BoardId).Length;
            SampleRate = sampleRate;
            Name = name;
         
            PeriodMilliseconds = 200;   //  default 5 Hz

            ProcessingTimes = new ConcurrentQueue<double>();
            BandPowersCollection = new ConcurrentDictionary<string, IBFSample>();
            BandPowerCalc = new BandPowerCalculator(BoardId, NumberOfChannels, SampleRate);
            SetBandPowerRangeList();
        }


        //  Board Properties
        public int BoardId { get; protected set; }
        public int NumberOfChannels { get; protected set; }
        public int SampleRate { get; protected set; }
        public string Name { get; protected set; }

        //  Band Power Calculator
        BandPowerCalculator BandPowerCalc;

        //  Results collection
        IBFSample[] BandPowers;
        ConcurrentDictionary<string, IBFSample> BandPowersCollection;

        //  Run function task
        protected CancellationTokenSource CancelTokenSource { get; set; }
        protected Task MonitorRunTask { get; set; }
        ConcurrentQueue<double> ProcessingTimes { get; set; }


        /// <summary>
        /// Monitor run function
        /// </summary>
        private async Task RunBadPowerMonitorAsync(CancellationToken cancelToken)
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
                        DetectBandPowers();
                    }

                    if (swReport.Elapsed >= TimeSpan.FromSeconds(30))
                    {
                        if (ProcessingTimes.Count > 0)
                        {
                            Log?.Invoke(this, new LogEventArgs(Name, this, "RunBadPowerMonitorAsync", $"{Name} band power processing {BandPowerCalc.NumberOfBands} ranges {(ProcessingTimes.Count / swReport.Elapsed.TotalSeconds).ToString("F0")} times per second: med {ProcessingTimes.Median().ToString("F4")} s | max {ProcessingTimes.Max().ToString("F4")} s.", LogLevel.TRACE));
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
        /// Detect band power
        /// </summary>
        /// <param name="data"></param>
        private void DetectBandPowers()
        {
            try
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                var data = GetRawChunk(1.5);

                var bandPowers = BandPowerCalc.CalculateBandPowers(data);

                for (int i = 0; i < BandPowerCalc.NumberOfBands; i++)
                {
                    for (int j = 0; j < NumberOfChannels; j++)
                        BandPowers[i].SetExgDataForChannel(j, bandPowers[i].GetExgDataForChannel(j));
                }

                sw.Stop();
                ProcessingTimes.Enqueue(sw.Elapsed.TotalSeconds);
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(Name, this, "DetectBandPowers", e, LogLevel.WARN));
            }
        }

        #endregion
    }
}

