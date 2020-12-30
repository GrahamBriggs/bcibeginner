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
    public class BlinkDetector
    {
        //  Events
        public event LogEventDelegate Log;
        public event DetectedBlinkDelegate DetectedBlink;

        //  Delegates
        public GetBFChunkSecondsDelegate GetData;
        public GetBFSampleDelegate GetStdDevMedians;
        
        //  Blink detector properties
        //  turn the dials to tune your detector
        public double NoisyStdDevThreshold { get; set; }

        //  Period of time in seconds that rising and falling must happen to be considered a blink
        //  rising and falling must take more than this amount of time  default  = .2
        public double BlinkPeriodThresholdMin { get; set; }
        //  rising and falling must take less than this amount of time defualt = .65
        public double BlinkPeriodThresholdMax { get; set; }

        //  Rising edge trigger: reading stdDeviation / medianStdDeviation must be greater than this threshold default = 2.0
        public double BlinkUpDevThreshold { get; set; }

        //  Falling edge trigger: reading stdDeviation / medianStdDeviation must be lower than this threshold default = 1.7
        public double BlinkDownDevThreshold { get; set; }


        /// <summary>
        /// Handler for new reading event
        /// will check for blinks on each new reading
        /// </summary>
        public void OnNewSample(object sender, BFSampleEventArgs e)
        {
            var data = GetData(.25);
            var stdDevLeft = data.GetExgDataForChannel(0).StdDev();
            var stdDevRight = data.GetExgDataForChannel(1).StdDev();
            var stdDevMedians = GetStdDevMedians();
            DetectBlinks(e.Sample, stdDevLeft, stdDevMedians.GetExgDataForChannel(0), stdDevRight, stdDevMedians.GetExgDataForChannel(1));
        }

        /// <summary>
        /// Detect blinks function
        /// using current reading, the standard deviation from channel 0 (FP1) and channel 1 (FP2), 
        /// and the average deviaiton from channel 0,1
        /// </summary>
        public void DetectBlinks(IBFSample currentReading, double stdDev0, double stdDevAvg0,  double stdDev1, double stdDevAvg1)
        {
            try
            {                   
                CheckForBlink(currentReading, stdDev0, stdDevAvg0, Eyes.Left);
                CheckForBlink(currentReading, stdDev1, stdDevAvg1, Eyes.Right);
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "DetectBlinks", e, LogLevel.ERROR));
            }
        }

        
        /// <summary>
        /// Constructor
        /// </summary>
        public BlinkDetector()
        {
            //  blink detector parameter defaults
            BlinkPeriodThresholdMin = .2;
            BlinkPeriodThresholdMax = .65;
            BlinkUpDevThreshold = 1.7;
            BlinkDownDevThreshold = 1.2;
            //
            DataFileStartTimeTag = -0.01;
            NoisyStdDevThreshold = 75.0;

            DataToProcess = new ConcurrentQueue<IBFSample>();
            NotifyAddedData = new SemaphoreSlim(0);
        }


        public double DataFileStartTimeTag { get; set; }


        protected CancellationTokenSource CancelTokenSource { get; set; }
        protected Task DataQueueProcessorTask { get; set; }
        protected SemaphoreSlim NotifyAddedData { get; set; }
        ConcurrentQueue<IBFSample> DataToProcess;


        //  Flags to keep track of left/right rising edge event by saving the reading that triggered it
        IBFSample BlinkLeftRisingEdgeTrigger;
        IBFSample BlinkRightRisingEdgeTrigger;


       


        /// <summary>
        /// Check for Blink in the specified eye
        /// </summary>
        private void CheckForBlink(IBFSample currentReading, double stdDev, double stdDevAvg, Eyes eye)
        {
            IBFSample trigger = (eye == Eyes.Left) ? BlinkLeftRisingEdgeTrigger : BlinkRightRisingEdgeTrigger;
          
            //  search for rising and falling edge of the signal    
            if (trigger != null)
            {
                //  rising edge triggered, check for signal going below falling threashold
                if (stdDev / stdDevAvg < BlinkDownDevThreshold)
                {
                    if ((currentReading.TimeStamp - trigger.TimeStamp) > BlinkPeriodThresholdMin && (currentReading.TimeStamp - trigger.TimeStamp) < BlinkPeriodThresholdMax)
                    {
                        DetectedBlink?.Invoke(this, new DetectedBlinkEventArgs(eye, WinkState.Wink, currentReading.TimeStamp));
                        ClearTrigger(eye);
                    }
                    else
                    {
                        //  reject as noise
                        DetectedBlink?.Invoke(this, new DetectedBlinkEventArgs(eye, WinkState.Falling, currentReading.TimeStamp));
                        ClearTrigger(eye);
                    }
                }
                else if (currentReading.TimeStamp - trigger.TimeStamp > BlinkPeriodThresholdMax)
                {
                    //  taken too long, clear the rising flag
                    DetectedBlink?.Invoke(this, new DetectedBlinkEventArgs(eye, WinkState.Falling, currentReading.TimeStamp));
                    ClearTrigger(eye);
                }
            }
            else if (trigger == null /*&&  (stdDevAvg < NoisyStdDevThreshold)*/ && ( stdDev / stdDevAvg > BlinkUpDevThreshold) )
            {
                DetectedBlink?.Invoke(this, new DetectedBlinkEventArgs(eye, WinkState.Rising, currentReading.TimeStamp));
                SetTrigger(currentReading, eye);
            }
        }


        void SetTrigger(IBFSample reading, Eyes eye)
        {
            switch (eye)
            {
                case Eyes.Left:
                    BlinkLeftRisingEdgeTrigger = reading;
                    break;

                case Eyes.Right:
                    BlinkRightRisingEdgeTrigger = reading;
                    break;
            }
        }


        void ClearTrigger(Eyes eye)
        {
            switch (eye)
            {
                case Eyes.Left:
                    BlinkLeftRisingEdgeTrigger = null;
                    break;

                case Eyes.Right:
                    BlinkRightRisingEdgeTrigger = null;
                    break;
            }
        }



    }
}
