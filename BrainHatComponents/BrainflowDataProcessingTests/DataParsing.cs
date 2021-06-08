using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrainflowDataProcessing;
using CsvHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BrainflowInterfaces;

namespace BrainflowDataProcessingTests
{
    [TestClass]
    public class DataParsing
    {
        [TestMethod]
        public async Task ReadCsvDataFile()
        {
            IEnumerable<BFSampleImplementation> records;

            using (var reader = new StreamReader("./TestFiles/blinkWink10_082530.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.HasHeaderRecord = false;
                records = csv.GetRecords<BFSampleImplementation>().ToList();
            }

            BrainflowDataProcessor processor = new BrainflowDataProcessor("", 0, 250);
            

            await processor.StartDataProcessorAsync();

            //  get data up to the first end of the first blink

            foreach ( var nextRecord in records )
            {
                processor.AddDataToProcessor(nextRecord);
                await Task.Delay(4);
            }

            await processor.StopDataProcessorAsync(true);



        }
    }
}
