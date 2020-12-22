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
        public GetBFChunkDelegate GetUnfilteredData;

        //  Properties
        public int PeriodMilliseconds { get; set; }


        //  Public Methods
        #region PublicMethods


        /// <summary>
        /// Create a band power range list for 8,10,12 and 18,20,22 
        /// </summary>
        public void CreateSampleBandPowerRangeList()
        {
            //  create a list of tuples for your band power ranges
            var rangeList = new List<Tuple<double, double>>() {
                    new Tuple<double, double>(7.0,9.0),
                    new Tuple<double, double>(9.0,11.0),
                    new Tuple<double, double>(11.0,13.0),

                    new Tuple<double, double>(17.0,19.0),
                    new Tuple<double, double>(19.0,21.0),
                    new Tuple<double, double>(21.0,23.0),
                };

            SetBandPowerRangeList(rangeList);
        }


        /// <summary>
        /// Create a band power range list for 1-60
        /// </summary>
        public void CreateFullBandPowerRangeList()
        {
            var rangeList = new List<Tuple<double, double>>();


            for (int i = 1; i <= 60; i++)
            {
                rangeList.Add(new Tuple<double, double>(i - 1, i + 1));
            }

            SetBandPowerRangeList(rangeList);
        }

        /// <summary>
        /// Set the collection of band power ranges you wish to calculate
        /// must be unique to 0.1 (for example 7.7 and 7.75 are not allowed in the same list)
        /// </summary>
        /// <param name="rangeList"></param>
        public void SetBandPowerRangeList(List<Tuple<double, double>> rangeList)
        {
            BandPowerCalcRangeList = rangeList;

            //  create a matching array so we can process multiple frequencies sequentially
            BandPowers = new IBFSample[BandPowerCalcRangeList.Count];
            for (int i = 0; i < BandPowerCalcRangeList.Count; i++)
            {
                BandPowers[i] = new BFSampleImplementation(BoardId);
            }

            //  create a dictionary for the results of the band power calculation
            //  must match the number of frequency ranges list above
            BandPowersCollection = new ConcurrentDictionary<string, IBFSample>();
            for (int j = 0; j < BandPowerCalcRangeList.Count; j++)
            {
                var key = MakeKey(BandPowerCalcRangeList[j].Item1 + (BandPowerCalcRangeList[j].Item2 - BandPowerCalcRangeList[j].Item1) / 2);
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
            if (BandPowersCollection.ContainsKey(MakeKey(band)))
                return BandPowersCollection[MakeKey(band)];

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

            ProcessingTimes = new ConcurrentQueue<double>();


        }


        //  Board Properties
        public int BoardId { get; protected set; }
        public int NumberOfChannels { get; protected set; }
        public int SampleRate { get; protected set; }
        public string Name { get; protected set; }

        //  Band Power Collection
        private ConcurrentDictionary<string, IBFSample> BandPowersCollection { get; set; }
        private string MakeKey(double band)
        {
            return $"{band:N1}";
        }

        private IBFSample[] BandPowers { get; set; }
        List<Tuple<double, double>> BandPowerCalcRangeList { get; set; }

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
                            Log?.Invoke(this, new LogEventArgs(Name, this, "RunBadPowerMonitorAsync", $"{Name} band power processing {BandPowerCalcRangeList.Count} ranges {(ProcessingTimes.Count / swReport.Elapsed.TotalSeconds).ToString("F0")} times per second: med {ProcessingTimes.Median().ToString("F4")} s | max {ProcessingTimes.Max().ToString("F4")} s.", LogLevel.TRACE));
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

                var data = GetUnfilteredData(5.0);

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
                ProcessingTimes.Enqueue(sw.Elapsed.TotalSeconds);
            }
            catch (Exception e)
            {
                //Log?.Invoke(this, new LogEventArgs(Name, this, "DetectBandPowers", e, LogLevel.ERROR));
            }
        }



        #endregion

    }
}

