using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrainflowDataProcessing;
using CsvHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenBCIInterfaces;

namespace BrainflowDataParserTests
{
    [TestClass]
    public class SeekingAlpha
    {
        //[TestMethod]
        public async Task SeekingAlphaTestOne()
        {
            //  create a processor
            BrainflowDataProcessor processor = new BrainflowDataProcessor();
            processor.SetBoard(0);
            processor.Log += Processor_Log;
        
            //  start the processor
            await processor.StartDataProcessorAsync();

            //  read this test file
            using (var reader = new StreamReader("./TestFiles/SeekingAlpha_20201012-155132.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.HasHeaderRecord = false;
                var records = csv.GetRecords<OpenBciCyton8Reading>().ToList();

                var testTimeStart = records.First().TimeStamp;
                foreach (var nextRecord in records)
                {
                    //if (nextRecord.TimeStamp - testTimeStart < 12)
                    //    continue;

                    processor.AddDataToProcessor(nextRecord);
                    await Task.Delay(4);
                }
            }
            await Task.Delay(5000);
            await processor.StopDataProcessorAsync(true);

            

        }


        /// <summary>
        /// Log handler 
        private void Processor_Log(object sender, LoggingInterface.LogEventArgs e)
        {
            if ( e.Level >= LoggingInterface.LogLevel.INFO)
                System.Diagnostics.Debug.WriteLine(e.Data);
        }
    }
}
