using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrainflowDataProcessing;
using CsvHelper;
using LoggingInterfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BrainflowInterfaces;

namespace BrainflowDataProcessingTests
{
    [TestClass]
    public class NewBlinkTestQueueProcessing
    {
        double TestTimeStart;
        int CountLeft = 0;
        int CountRight = 0;

        [TestMethod]
        public async Task NewBlinkTestFiveBlinks()
        {
            CountLeft = 0;
            CountRight = 0;

            //  create a processor
            BrainflowDataProcessor processor = new BrainflowDataProcessor("test", 0, 250);
            BlinkDetector detector = new BlinkDetector();
            detector.GetData = processor.GetRawData;
            detector.GetStdDevMedians = processor.GetStdDevianMedians;
            processor.NewReading += detector.OnNewReading;
            detector.Log += Detector_Log;
            detector.DetectedBlink += Processor_DetectedBlink;
            //  start the processor
            await processor.StartDataProcessorAsync();

            //  read this test file
            using (var reader = new StreamReader("./TestFiles/NewBlinkTest.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.HasHeaderRecord = false;
                var records = csv.GetRecords<BFCyton8Sample>();

                //  get data up to the first end of the first blink sequence, 15 seconds into the data file
                TestTimeStart = records.First().TimeStamp;
                foreach (var nextRecord in records)
                {
                    processor.AddDataToProcessor(nextRecord);
                    await Task.Delay(1);

                    if (nextRecord.TimeStamp - TestTimeStart > 10)
                        break;
                }
            }

            await processor.StopDataProcessorAsync(true);

            //  should have been five full blinks
            Assert.AreEqual(5, CountLeft);
            Assert.AreEqual(5, CountRight);

        }

        private void Processor_DetectedBlink(object sender, DetectedBlinkEventArgs e)
        {
            int count;

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

            System.Diagnostics.Debug.WriteLine($"{(e.TimeStamp - TestTimeStart).ToString("N4")}  Check Detected blink {count} in {e.Eye} eye.");
        }

        private void Detector_Log(object sender, LogEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Data);
        }
    }
}
