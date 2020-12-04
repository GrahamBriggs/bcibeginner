using System;
using BrainflowDataProcessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BrainflowInterfaces;

namespace BrainflowDataProcessingTests
{
    [TestClass]
    public class BlinkDetectorTests
    {
        int LeftCount;
        int RightCount;

        [TestMethod]
        public void BlinkDetectorRisingAndFalling()
        {
            LeftCount = 0;
            RightCount = 0;

            BlinkDetector detector = new BlinkDetector();
            detector.DetectedBlink += Detector_DetectedBlink;

            BFCyton8Sample data = new BFCyton8Sample()
            {
                TimeStamp = 1.00100,
            };

            //  this looks like rising edge
            detector.DetectBlinks(data, 31.0, 10.0, 31.0, 10.0);
            
            //  no blinks on rising edge alone
            Assert.AreEqual(0, LeftCount);
            Assert.AreEqual(0, RightCount);

            data = new BFCyton8Sample()
            {
                TimeStamp = 1.25,
            };

            // this looks like falling edge
            detector.DetectBlinks(data, 11.0, 10.0, 11.0, 10.0);

            //  should be one blink
            Assert.AreEqual(1, LeftCount);
            Assert.AreEqual(1, RightCount);
        }



        private void Detector_DetectedBlink(object sender, DetectedBlinkEventArgs e)
        {
            if (e.State == WinkState.Wink)
            {
                if (e.Eye == Eyes.Left)
                    LeftCount++;
                else
                    RightCount++;
            }
        }
    }
}
