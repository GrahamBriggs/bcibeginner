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
    public class BlinkWink10QueueProcessing
    {
        double TestTimeStart;
        int CountLeft, CountRight;
     
        [TestMethod]
        public async Task BlinkWink10_DetectFirstFiveBlinks()
        {
            CountLeft = 0;
            CountRight = 0;
         
            //  create a processor
            BrainflowDataProcessor processor = new BrainflowDataProcessor("", 0, 250);
            BlinkDetector detector = new BlinkDetector();
            detector.GetData = processor.GetRawData;
            detector.GetStdDevMedians = processor.GetStdDevianMedians;
            processor.NewSample += detector.OnNewSample;
            detector.Log += Detector_Log;
            detector.DetectedBlink += Processor_DetectedBlink;
          
           
            //  read this test file
            using (var reader = new StreamReader("./TestFiles/blinkWink10_082530.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.HasHeaderRecord = false;
                var data = csv.GetRecords<BFCyton8Sample>().ToList();

                await processor.StartDataProcessorAsync();

                //  get data up to the first end of the first blink sequence, 15 seconds into the data file
                TestTimeStart = data.First().TimeStamp;
                foreach (var nextReading in data)
                {
                    if (nextReading.TimeStamp - TestTimeStart < 5)
                        continue;

                    processor.AddDataToProcessor(nextReading);

                    await Task.Delay(1);

                    if (nextReading.TimeStamp - TestTimeStart > 15)
                        break;
                }
            }

            await processor.StopDataProcessorAsync();


            //  should have been five full blinks
            Assert.AreEqual(5, CountLeft);
            Assert.AreEqual(5, CountRight);
            
        }

      

        [TestMethod]
        public async Task BlinkWink10_DetectFiveLeftWinks()
        {
            CountLeft = 0;
            CountRight = 0;

            //  create a processor
            BrainflowDataProcessor processor = new BrainflowDataProcessor("", 0, 250);
            BlinkDetector detector = new BlinkDetector();
            detector.GetData = processor.GetRawData;
            detector.GetStdDevMedians = processor.GetStdDevianMedians;
            processor.NewSample += detector.OnNewSample;
            detector.Log += Detector_Log;
            detector.DetectedBlink += Processor_DetectedBlink;

            //  read this test file
            using (var reader = new StreamReader("./TestFiles/blinkWink10_082530.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.HasHeaderRecord = false;
                var records = csv.GetRecords<BFCyton8Sample>().ToList(); ;

                await processor.StartDataProcessorAsync();

                //  get data up to the first end of the first blink sequence, 15 seconds into the data file
                TestTimeStart = records.First().TimeStamp;
                foreach (var nextRecord in records)
                {
                    if (nextRecord.TimeStamp - TestTimeStart < 18)
                        continue;

                    processor.AddDataToProcessor(nextRecord);
                    await Task.Delay(1);

                    if (nextRecord.TimeStamp - TestTimeStart > 28)
                        break;
                }
            }

            await processor.StopDataProcessorAsync(true);

            //  should have been winks on the left
            Assert.AreEqual(5, CountLeft);
            Assert.AreEqual(0, CountRight);

        }


        [TestMethod]
        public async Task BlinkWink10_DetectLastFiveBlinks()
        {
            CountLeft = 0;
            CountRight = 0;

            //  create a processor
            BrainflowDataProcessor processor = new BrainflowDataProcessor("", 0, 250);
            BlinkDetector detector = new BlinkDetector();
            detector.GetData = processor.GetRawData;
            detector.GetStdDevMedians = processor.GetStdDevianMedians;
            processor.NewSample += detector.OnNewSample;
            detector.Log += Detector_Log;
            detector.DetectedBlink += Processor_DetectedBlink;

            //  read this test file
            using (var reader = new StreamReader("./TestFiles/blinkWink10_082530.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.HasHeaderRecord = false;
                var records = csv.GetRecords<BFCyton8Sample>().ToList();

                await processor.StartDataProcessorAsync();

                //  get data up to the first end of the first blink sequence, 15 seconds into the data file
                TestTimeStart = records.First().TimeStamp;
                foreach (var nextRecord in records)
                {
                    if (nextRecord.TimeStamp - TestTimeStart < 44)
                        continue;

                    processor.AddDataToProcessor(nextRecord);
                    await Task.Delay(1);

                    if (nextRecord.TimeStamp - TestTimeStart > 53)
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
