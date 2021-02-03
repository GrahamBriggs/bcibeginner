using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using brainflow;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using System.Linq;
using BrainflowDataProcessing;
using CsvHelper;
using System.Globalization;
using BrainflowInterfaces;

namespace BrainflowDataProcessingTests
{
    [TestClass]
    public class MethodInfoTests
    {
        [TestMethod]
        public void SingleBandPassFilter()
        {
            var signalFilters = new SignalFilters();

            signalFilters.LoadSignalFilters("TestFiles/SimpleFilter1.xml");

            //  read this test file
            using (var reader = new StreamReader("./TestFiles/BlinkWink5_20201012-153647.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.HasHeaderRecord = false;
                var records = csv.GetRecords<BFCyton8Sample>().ToList();

                //  get data up to the first end of the first blink sequence, 15 seconds into the data file
                var startTime = records.First().TimeStamp;
                var data = records.Where(x => x.TimeStamp - startTime < 3);

                var filter = signalFilters.GetFilter("bandpass1");

                var result = filter.ApplyFilter(data.GetExgDataForChannel(0), 250);

                var gold =  DataFilter.perform_bandpass(data.GetExgDataForChannel(0), 250, 15.0, 5.0, 2, (int)FilterTypes.BUTTERWORTH, 0.0);

                Assert.AreEqual(gold.Count(), result.Count());
                for(int i = 0; i < gold.Count(); i++)
                {
                    Assert.AreEqual(gold[i], result[i], 0.000001);
                }
            }


            //foreach ( var nextFunction in doc.Element("Filter")?.Elements("Function") )
            //{
            //    var functionName = nextFunction.Attribute("Name").Value;
            //    foreach ( var nextParam in nextFunction.Elements("Parameter") )
            //    {
            //        var paramName = nextParam.Attribute("Name");
            //        var paramValue = nextParam.Attribute("Value");
            //    }

            //                   MethodInfo mi = typeof(BoardShim).GetMethod(functionName, BindingFlags.Public | BindingFlags.Static);

            //    var paramDict = nextFunction.Elements("Parameter").ToDictionary(d => d.Attribute("Name").Value, d => d.Attribute("Value").Value);

            //    object[] parameters = mi.GetParameters().Select(p => Convert.ChangeType(paramDict[p.Name], p.ParameterType)).ToArray();

            //    var expectedResult = mi.Invoke(typeof(BoardShim).Assembly, parameters);

            //   var result =  BoardShim.get_num_rows(0);

            //}

            //BoardShim.get_num_rows(0);

        }

        [TestMethod]
        public void TwoStageFilter()
        {
            var signalFilters = new SignalFilters();

            signalFilters.LoadSignalFilters("TestFiles/SimpleFilter1.xml");

            //  read this test file
            using (var reader = new StreamReader("./TestFiles/BlinkWink5_20201012-153647.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.HasHeaderRecord = false;
                var records = csv.GetRecords<BFCyton8Sample>().ToList();

                //  get data up to the first end of the first blink sequence, 15 seconds into the data file
                var startTime = records.First().TimeStamp;
                var data = records.Where(x => x.TimeStamp - startTime < 3);

                var filter = signalFilters.GetFilter("twoStage");

                var result = filter.ApplyFilter(data.GetExgDataForChannel(0), 250);

                var gold = DataFilter.perform_bandpass(data.GetExgDataForChannel(0), 250, 15.0, 5.0, 2, (int)FilterTypes.BUTTERWORTH, 0.0);
                gold = DataFilter.perform_bandstop(gold, 250, 50.0, 1.0, 6, (int)FilterTypes.CHEBYSHEV_TYPE_1, 1.0);

                Assert.AreEqual(gold.Count(), result.Count());
                for (int i = 0; i < gold.Count(); i++)
                {
                    Assert.AreEqual(gold[i], result[i], 0.000001);
                }
            }



        }
    }
}
