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
    public class BandPowerMonitor
    {
        //  Events
        public event LogEventDelegate Log;

        //  Delegates
        public GetBFChunkDelegate GetData;

        //  Properties
        public int PeriodMilliseconds { get; set; }


        //  Public Methods
        #region PublicMethods

        /// <summary>
        /// Start async task to run data processing
        /// </summary>
        public async Task StartMonitorAsync()
        {
            await StopMonitorAsync();

            // fire off the run tasks
            CancelTokenSource = new CancellationTokenSource();
            MonitorRunTask = RunBadPowerMonitorAsync(CancelTokenSource.Token);

            Log?.Invoke(this, new LogEventArgs(Name, this, "StartMonitorAsync", $"Starting band power monnitor.", LogLevel.INFO));
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
            }

            Log?.Invoke(this, new LogEventArgs(Name, this, "StopDataProcessor", $"Stopped Brainflow data processor.", LogLevel.INFO));
        }


        /// <summary>
        /// Get the current band power for all channels at the specified band
        /// </summary>
        public IBFSample GetBandPower(int band)
        {
            if (BandPowersCollection.ContainsKey(band))
                return BandPowersCollection[band];

            return null;
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

            //CreateSampleBandPowerRangeList();
            CreateFullBandPowerRangeList();
            PeriodMilliseconds = 200;   //  default 5 Hz

            ProcessingTimesBandPower = new ConcurrentQueue<double>();


        }


        //  Board Properties
        public int BoardId { get; protected set; }
        public int NumberOfChannels { get; protected set; }
        public int SampleRate { get; protected set; }
        public string Name { get; protected set; }


        private ConcurrentDictionary<int, IBFSample> BandPowersCollection { get; set; }
        private IBFSample[] BandPowers { get; set; }
        List<Tuple<double, double>> BandPowerCalcRangeList { get; set; }

        ConcurrentQueue<double> ProcessingTimesBandPower { get; set; }



        protected CancellationTokenSource CancelTokenSource { get; set; }
        protected Task MonitorRunTask { get; set; }


        void CreateSampleBandPowerRangeList()
        {
            //  create a list of tuples for your band power ranges
            BandPowerCalcRangeList = new List<Tuple<double, double>>() {
                    new Tuple<double, double>(7.0,9.0),
                    new Tuple<double, double>(9.0,11.0),
                    new Tuple<double, double>(11.0,13.0),

                    new Tuple<double, double>(17,19.0),
                    new Tuple<double, double>(19.0,21.0),
                    new Tuple<double, double>(21.0,23.0),
                };

            //  create a matching array so we can process multiple frequencies sequentially
            BandPowers = new IBFSample[BandPowerCalcRangeList.Count];
            for (int i = 0; i < BandPowerCalcRangeList.Count; i++)
            {
                BandPowers[i] = new BFSampleImplementation(BoardId);
            }

            //  create a dictionary for the results of the band power calculation
            //  must match the number of frequency ranges list above
            BandPowersCollection = new ConcurrentDictionary<int, IBFSample>();
            for (int j = 0; j < BandPowerCalcRangeList.Count; j++)
            {
                int key = (int)(BandPowerCalcRangeList[j].Item1 + (BandPowerCalcRangeList[j].Item2 - BandPowerCalcRangeList[j].Item1) / 2);
                BandPowersCollection.TryAdd(key, BandPowers[j]);
            }
        }

        void CreateFullBandPowerRangeList()
        {
            BandPowerCalcRangeList = new List<Tuple<double, double>>();
            BandPowers = new IBFSample[60];
            BandPowersCollection = new ConcurrentDictionary<int, IBFSample>();

            for (int i = 1; i <= 60; i++)
            {
                BandPowerCalcRangeList.Add(new Tuple<double, double>(i - 1, i + 1));
                BandPowers[i - 1] = new BFSampleImplementation(BoardId);
                BandPowersCollection.TryAdd(i, BandPowers[i - 1]);
            }
        }



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
                        //if (IsReadyToDetectBandPowers())
                        //{
                        //    DetectingBandPowersTask = DetectBandPowersAsync();
                        //}
                        //else
                        //{
                        //    Log?.Invoke(this, new LogEventArgs(Name,this, "RunBandPowerMonitor", $"Not ready to detect band powers.", LogLevel.ERROR));
                        //}
                    }

                    if (swReport.ElapsedMilliseconds >= 5000)
                    {
                        if (ProcessingTimesBandPower.Count > 0)
                        {
                            Log?.Invoke(this, new LogEventArgs(Name, this, "RunBadPowerMonitorAsync", $"{Name} band power processing {BandPowerCalcRangeList.Count} ranges {(ProcessingTimesBandPower.Count / swReport.Elapsed.TotalSeconds).ToString("F0")} times per second: Av {ProcessingTimesBandPower.Average().ToString("F4")} s | Max {ProcessingTimesBandPower.Max().ToString("F4")} s.", LogLevel.TRACE));
                            ProcessingTimesBandPower.RemoveAll();
                        }
                        else
                        {
                            Log?.Invoke(this, new LogEventArgs(Name, this, "RunBadPowerMonitorAsync", $"{Name} not processing band powers ?", LogLevel.TRACE));
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


        ///// <summary>
        ///// This might take longer than frequency updates on the timer
        ///// experient with doing calcs in background task launched from the periodic timer
        ///// </summary>
        //private Task DetectingBandPowersTask { get; set; }
        //bool IsReadyToDetectBandPowers()
        //{
        //    if (DetectingBandPowersTask == null)
        //    {
        //        return true;
        //    }
        //    else if (DetectingBandPowersTask.IsCompleted)
        //    {
        //        DetectingBandPowersTask = null;
        //        return true;
        //    }

        //    return false;
        //}


        ///// <summary>
        ///// Detect band powers async (experimenting with task and performance monitoring)
        ///// </summary>
        //private async Task DetectBandPowersAsync()
        //{
        //    await Task.Run(() =>
        //    {
        //        DetectBandPowers();

        //    });
        //}


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

                var data = GetData(5.0);

                if (data == null || data.Count() == 0)
                    return;

                for (int i = 0; i < NumberOfChannels; i++)
                {
                    var bandPowers = BandPowerCalculator.CalculateBandPower(data, SampleRate, i, BandPowerCalcRangeList);

                    int j = 0;
                    foreach (var nextBandPower in bandPowers)
                    {
                        BandPowers[j++].SetExgDataForChannel(i, nextBandPower);
                    }
                }

                sw.Stop();
                ProcessingTimesBandPower.Enqueue(sw.Elapsed.TotalSeconds);
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(Name, this, "DetectBandPowers", e, LogLevel.ERROR));
            }
        }



        #endregion

    }
}

