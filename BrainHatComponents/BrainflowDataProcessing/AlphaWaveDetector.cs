using LoggingInterfaces;
using BrainflowInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    public class AlphaWaveDetector
    {
        //  Events
        public event LogEventDelegate Log;
        public event DetectedBrainWaveDelegate DetectedBrainWave;

        //  Delegates
        public GetBandPowerDelegate GetBandPower;

        //  Public Interface
        #region PublicInterface

        public int PeriodMilliseconds { get; set; }

        public double AlphaThresholdRising { get; set; }
        public double AlphaThresholdFalling { get; set; }

        /// <summary>
        /// Start async task to run data processing
        /// </summary>
        public async Task StartDetectorAsync()
        {
            await StopDetectorAsync();

            // fire off the run tasks
            CancelTokenSource = new CancellationTokenSource();
            RunTask = RunAlphaWaveDetector(CancelTokenSource.Token);

            Log?.Invoke(this, new LogEventArgs(this, "StartDetectorAsync", $"Starting alpha wave detector.", LogLevel.INFO));
        }




        /// <summary>
        /// Stop the data processor async task
        /// </summary>
        public async Task StopDetectorAsync()
        {
            if (CancelTokenSource != null)
            {
                CancelTokenSource.Cancel();
                await RunTask;

                CancelTokenSource = null;
                RunTask = null;
            }

            Log?.Invoke(this, new LogEventArgs(this, "StopDetectorAsync", $"Stopped alpha wave detector.", LogLevel.INFO));
        }


        #endregion


        //  Implementation
        #region Implementation

        public AlphaWaveDetector()
        {
            PeriodMilliseconds = 200;
            AlphaThresholdRising = 10.0;
            AlphaThresholdFalling = 4.0;
        }

        CancellationTokenSource CancelTokenSource;
        Task RunTask;

        DateTimeOffset? TimeEnteredAlphaState;
        // DateTimeOffset? TimeAlphaStateSlip { get; set; }

        async Task RunAlphaWaveDetector(CancellationToken cancelToken)
        {
            try
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                while (!cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(1);

                        if (sw.ElapsedMilliseconds > PeriodMilliseconds)
                        {
                            sw.Restart();
                            var band10 = GetBandPower(10);
                            var band12 = GetBandPower(12);

                            if (TimeEnteredAlphaState.HasValue)
                            {
                                if ((band10.GetExgDataForChannel(6) / band12.GetExgDataForChannel(6) < AlphaThresholdFalling) && (band10.GetExgDataForChannel(7) / band12.GetExgDataForChannel(7) < AlphaThresholdFalling))
                                {
                                    TimeEnteredAlphaState = null;
                                    Log?.Invoke(this, new LogEventArgs(this, "RunAlphaWaveDetector", $"Alpha state exit at {DateTimeOffset.UtcNow.ToLocalTime().ToString("HH:mm:ss.fff")}.", LogLevel.DEBUG));
                                    DetectedBrainWave?.Invoke(this, new DetectedBrainWaveEventArgs(BrainWave.None, DateTimeOffset.UtcNow));
                                }
                                else
                                {
                                    DetectedBrainWave?.Invoke(this, new DetectedBrainWaveEventArgs(BrainWave.Alpha, TimeEnteredAlphaState.Value));
                                }
                            }
                            else if ((band10.GetExgDataForChannel(6) / band12.GetExgDataForChannel(6) > AlphaThresholdRising) && (band10.GetExgDataForChannel(7) / band12.GetExgDataForChannel(7) > AlphaThresholdRising))
                            {
                                TimeEnteredAlphaState = DateTimeOffset.UtcNow;
                                DetectedBrainWave?.Invoke(this, new DetectedBrainWaveEventArgs(BrainWave.Alpha, TimeEnteredAlphaState.Value));
                                Log?.Invoke(this, new LogEventArgs(this, "RunAlphaWaveDetector", $"Alpha state entered at {TimeEnteredAlphaState.Value.ToLocalTime().ToString("HH:mm:ss.fff")}.", LogLevel.DEBUG));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log?.Invoke(this, new LogEventArgs(this, "RunAlphaWaveDetector", ex, LogLevel.ERROR));
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunAlphaWaveDetector", e, LogLevel.FATAL));
            }
        }


        #endregion


    }
}
