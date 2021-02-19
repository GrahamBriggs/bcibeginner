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
    public class SignalFilterTests
    {
        [TestMethod]
        public void perform_lowpassTest()
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

                var filter = signalFilters.GetFilter("perform_lowpass");

                var result = filter.ApplyFilter(data.GetExgDataForChannel(0), 250);

                var gold = DataFilter.perform_lowpass(data.GetExgDataForChannel(0), 250, 20.0, 4,  (int)FilterTypes.BESSEL, 0.0);

                Assert.AreEqual(gold.Count(), result.Count());
                for (int i = 0; i < gold.Count(); i++)
                {
                    Assert.AreEqual(gold[i], result[i], 0.000001);
                }
            }
        }

        [TestMethod]
        public void perform_highpassTest()
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

                var filter = signalFilters.GetFilter("perform_highpass");

                var result = filter.ApplyFilter(data.GetExgDataForChannel(0), 250);

                var gold = DataFilter.perform_highpass(data.GetExgDataForChannel(0), 250, 2.0, 4, (int)FilterTypes.BUTTERWORTH, 0.0);

                Assert.AreEqual(gold.Count(), result.Count());
                for (int i = 0; i < gold.Count(); i++)
                {
                    Assert.AreEqual(gold[i], result[i], 0.000001);
                }
            }
        }

        [TestMethod]
        public void perform_bandpassTest()
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

                var filter = signalFilters.GetFilter("perform_bandpass");

                var result = filter.ApplyFilter(data.GetExgDataForChannel(0), 250);

                var gold =  DataFilter.perform_bandpass(data.GetExgDataForChannel(0), 250, 15.0, 5.0, 2, (int)FilterTypes.BUTTERWORTH, 0.0);

                Assert.AreEqual(gold.Count(), result.Count());
                for(int i = 0; i < gold.Count(); i++)
                {
                    Assert.AreEqual(gold[i], result[i], 0.000001);
                }
            }
        }

        [TestMethod]
        public void perform_bandstopTest()
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

                var filter = signalFilters.GetFilter("perform_bandstop");

                var result = filter.ApplyFilter(data.GetExgDataForChannel(0), 250);

                var gold = DataFilter.perform_bandstop(data.GetExgDataForChannel(0), 250, 50.0, 1.0, 6, (int)FilterTypes.CHEBYSHEV_TYPE_1, 1.0);

                Assert.AreEqual(gold.Count(), result.Count());
                for (int i = 0; i < gold.Count(); i++)
                {
                    Assert.AreEqual(gold[i], result[i], 0.000001);
                }
            }
        }

        [TestMethod]
        public void perform_rolling_filterTest()
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

                var filter = signalFilters.GetFilter("perform_rolling_filter");

                var result = filter.ApplyFilter(data.GetExgDataForChannel(0), 250);

                var gold = DataFilter.perform_rolling_filter(data.GetExgDataForChannel(0), 3, (int)AggOperations.MEAN);
                Assert.AreEqual(gold.Count(), result.Count());
                for (int i = 0; i < gold.Count(); i++)
                {
                    Assert.AreEqual(gold[i], result[i], 0.000001);
                }
            }
        }

        [TestMethod]
        public void detrendTest()
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

                var filter = signalFilters.GetFilter("detrend");

                var result = filter.ApplyFilter(data.GetExgDataForChannel(0), 250);

                var gold = DataFilter.detrend(data.GetExgDataForChannel(0), (int)DetrendOperations.LINEAR);
                Assert.AreEqual(gold.Count(), result.Count());
                for (int i = 0; i < gold.Count(); i++)
                {
                    Assert.AreEqual(gold[i], result[i], 0.000001);
                }
            }
        }

        [TestMethod]
        public void perform_wavelet_denoisingTest()
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

                var filter = signalFilters.GetFilter("perform_wavelet_denoising");

                var result = filter.ApplyFilter(data.GetExgDataForChannel(0), 250);

                var gold = DataFilter.perform_wavelet_denoising(data.GetExgDataForChannel(0), "db4", 3);
                Assert.AreEqual(gold.Count(), result.Count());
                for (int i = 0; i < gold.Count(); i++)
                {
                    Assert.AreEqual(gold[i], result[i], 0.000001);
                }
            }
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

                var filter = signalFilters.GetFilter("Filter2");

                var result = filter.ApplyFilter(data.GetExgDataForChannel(0), 250);

                var gold = DataFilter.perform_bandstop(data.GetExgDataForChannel(0), 250, 50.0, 1.0, 6, (int)FilterTypes.CHEBYSHEV_TYPE_1, 1.0);
                gold = DataFilter.perform_bandpass(gold, 250, 20.5, 20, 2, (int)FilterTypes.BUTTERWORTH, 0.0);
                

                Assert.AreEqual(gold.Count(), result.Count());
                for (int i = 0; i < gold.Count(); i++)
                {
                    Assert.AreEqual(gold[i], result[i], 0.000001);
                }
            }



        }

        [TestMethod]
        public void ThreeStageFilter()
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

                var filter = signalFilters.GetFilter("Filter3");

                var result = filter.ApplyFilter(data.GetExgDataForChannel(0), 250);
                
                var gold = DataFilter.detrend(data.GetExgDataForChannel(0), (int)DetrendOperations.LINEAR);
                gold = DataFilter.perform_bandstop(gold, 250, 50.0, 1.0, 6, (int)FilterTypes.CHEBYSHEV_TYPE_1, 1.0);
                gold = DataFilter.perform_bandpass(gold, 250, 20.5, 20, 2, (int)FilterTypes.BUTTERWORTH, 0.0);


                Assert.AreEqual(gold.Count(), result.Count());
                for (int i = 0; i < gold.Count(); i++)
                {
                    Assert.AreEqual(gold[i], result[i], 0.000001);
                }
            }



        }


    }
}
