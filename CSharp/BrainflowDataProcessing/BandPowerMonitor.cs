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
    public class BandPowerMonitor
    {
        //  Events
        public event LogEventDelegate Log;

        //  Delegates
        public GetOpenBciCyton8DataDelegate GetData;
     
        //  Properties
        public int PeriodMilliseconds { get; set; }


        //  Public Methods
        #region PublicMethods

        /// <summary>
        /// Set the board properties for the data
        /// </summary>
        public void SetBoardProperties(int numChannels, int sampleRate)
        {
            NumberOfChannels = numChannels;
            SampleRate = sampleRate;
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

            Log?.Invoke(this, new LogEventArgs(this, "StartMonitorAsync", $"Starting band power monnitor.", LogLevel.INFO));
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

            Log?.Invoke(this, new LogEventArgs(this, "StopDataProcessor", $"Stopped Brainflow data processor.", LogLevel.INFO));
        }


        /// <summary>
        /// Get the current band power for all channels at the specified band
        /// </summary>
        public OpenBciCyton8Reading GetBandPower(int band)
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
        public BandPowerMonitor()
        {
            //CreateSampleBandPowerRangeList();
            CreateFullBandPowerRangeList();
            PeriodMilliseconds = 200;   //  default 5 Hz

            ProcessingTimesBandPower = new ConcurrentQueue<double>();
        }


        //  Board Properties
        int NumberOfChannels { get; set; }
        int SampleRate { get; set; }


        private ConcurrentDictionary<int, OpenBciCyton8Reading> BandPowersCollection { get; set; }
        private OpenBciCyton8Reading[] BandPowers { get; set; }
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
            BandPowers = new OpenBciCyton8Reading[BandPowerCalcRangeList.Count];
            for (int i = 0; i < BandPowerCalcRangeList.Count; i++)
            {
                BandPowers[i] = new OpenBciCyton8Reading();
            }
            
            //  create a dictionary for the results of the band power calculation
            //  must match the number of frequency ranges list above
            BandPowersCollection = new ConcurrentDictionary<int, OpenBciCyton8Reading>();
            for (int j = 0; j < BandPowerCalcRangeList.Count; j++)
            {
                int key = (int)(BandPowerCalcRangeList[j].Item1 + (BandPowerCalcRangeList[j].Item2 - BandPowerCalcRangeList[j].Item1)/2);
                BandPowersCollection.TryAdd(key, BandPowers[j]);
            }
        }

        void CreateFullBandPowerRangeList()
        {
            BandPowerCalcRangeList = new List<Tuple<double, double>>();
            BandPowers = new OpenBciCyton8Reading[60];
            BandPowersCollection = new ConcurrentDictionary<int, OpenBciCyton8Reading>();

            for (int i =1; i <= 60; i++)
            {
                BandPowerCalcRangeList.Add(new Tuple<double, double>(i - 1, i + 1));
                BandPowers[i - 1] = new OpenBciCyton8Reading();
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
                        //    Log?.Invoke(this, new LogEventArgs(this, "RunBandPowerMonitor", $"Not ready to detect band powers.", LogLevel.ERROR));
                        //}
                    }

                    if (swReport.ElapsedMilliseconds >= 5000)
                    {
                        if (ProcessingTimesBandPower.Count > 0)
                        {
                            Log?.Invoke(this, new LogEventArgs(this, "RunBadPowerMonitorAsync", $"Band Power processing {BandPowerCalcRangeList.Count} ranges {(ProcessingTimesBandPower.Count/swReport.Elapsed.TotalSeconds).ToString("F0")} times per second: Av {ProcessingTimesBandPower.Average().ToString("F4")} s | Max {ProcessingTimesBandPower.Max().ToString("F4")} s.", LogLevel.TRACE));
                            ProcessingTimesBandPower.RemoveAll();
                        }
                        else
                        {
                            Log?.Invoke(this, new LogEventArgs(this, "RunBadPowerMonitorAsync", $"Not processing band powers ?", LogLevel.TRACE));
                        }
                        swReport.Restart();
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunBandPowerMonitorAsync", e, LogLevel.FATAL));
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

                var data = GetData(2.0);

                if (data == null || data.Count() == 0)
                    return;

                for (int i = 0; i < NumberOfChannels; i++)
                {
                    var bandPowers = BandPowerCalculator.CalculateBandPower(data, SampleRate, i, BandPowerCalcRangeList);

                    int j = 0;
                    foreach (var nextBandPower in bandPowers)
                    {
                        BandPowers[j++].SetExgData(i, nextBandPower);
                    }
                }

                sw.Stop();
                ProcessingTimesBandPower.Enqueue(sw.Elapsed.TotalSeconds);
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "DetectBandPowers", e, LogLevel.ERROR));
            }
        }



        #endregion

    }
}

