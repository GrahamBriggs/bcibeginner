using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrainflowDataProcessing;
using CsvHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BrainflowInterfaces;
using LoggingInterfaces;

namespace BrainflowDataProcessingTests
{
    [TestClass]
    public class BlinkWink5
    {
        int CountLeft;
        int CountRight;
        double TestTimeStart;


        [TestMethod]
        public async Task BlinkWink10_DetectFirstFiveBlinks()
        {
            CountLeft = 0;
            CountRight = 0;

            //  create a processor
            BrainflowDataProcessor processor = new BrainflowDataProcessor("", 0, 250);
            BlinkDetector detector = new BlinkDetector();
            detector.GetData = processor.GetRawChunk;
            detector.GetStdDevMedians = processor.GetStdDevianMedians;

            processor.NewSample += detector.OnNewSample;
            detector.Log += Detector_Log;
            detector.DetectedBlink += Processor_DetectedBlink;

            //  start the processor
            await processor.StartDataProcessorAsync();

            //  read this test file
            using (var reader = new StreamReader("./TestFiles/BlinkWink5_20201012-153647.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.HasHeaderRecord = false;
                var records = csv.GetRecords<BFSampleImplementation>().ToList();

                //  get data up to the first end of the first blink sequence, 15 seconds into the data file
                TestTimeStart = records.First().TimeStamp;
                foreach (var nextRecord in records)
                {
                    if (nextRecord.TimeStamp - TestTimeStart < 0)
                        continue;

                    processor.AddDataToProcessor(nextRecord);
                    await Task.Delay(4);

                    if (nextRecord.TimeStamp - TestTimeStart > 14)
                        break;
                }
            }
            await Task.Delay(5000);
            await processor.StopDataProcessorAsync(true);

            //  should have been five full blinks
            Assert.AreEqual(5, CountLeft);
            Assert.AreEqual(5, CountRight);

        }


        /// <summary>
        /// Log handler 
        private void Detector_Log(object sender, LogEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Data);
        }

        private void Processor_DetectedBlink(object sender, DetectedBlinkEventArgs e)
        {
            int count;

            if (e.State == WinkState.Wink)
            {
                if (e.Eye == Eyes.Left)
                {
                    CountLeft++;
                    count = CountLeft;
                }
                else
                {
                    CountRight++;
                    count = CountRight;
                }
            }

            
        }
    }
}
