using System;
using System.Collections.Generic;
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
    public class DataParsing
    {
        [TestMethod]
        public async Task ReadCsvDataFile()
        {
            IEnumerable<OpenBciCyton8Reading> records;

            using (var reader = new StreamReader("./TestFiles/blinkWink10_082530.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.HasHeaderRecord = false;
                records = csv.GetRecords<OpenBciCyton8Reading>().ToList();
            }

            BrainflowDataProcessor processor = new BrainflowDataProcessor();
            

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
