using Microsoft.VisualStudio.TestTools.UnitTesting;
using BrainflowInterfaces;

namespace BrainflowInterfacesTests
{
    [TestClass]
    public class Cyton8Sample
    {
        [TestMethod]
        public void Cyton8SampleExgChannelEnumerable()
        {
            BFCyton8Sample sample = new BFCyton8Sample()
            {
                ExgCh0 = 0.0,
                ExgCh1 = 1.0,
                ExgCh2 = 2.0,
                ExgCh3 = 3.0,
                ExgCh4 = 4.0,
                ExgCh5 = 5.0,
                ExgCh6 = 6.0,
                ExgCh7 = 7.0,
            };

            double prevChannel = -1.0;
            int countChannels = 0;
            foreach ( var nextChannel in sample.ExgData)
            {
                countChannels++;
                Assert.AreEqual(1.0, nextChannel - prevChannel, 0.0000001);
                prevChannel = nextChannel;
            }

            Assert.AreEqual(sample.NumberExgChannels, countChannels);
        }


        [TestMethod]
        public void Cyton8SampleExgChannelByIndex()
        {
            BFCyton8Sample sample = new BFCyton8Sample()
            {
                ExgCh0 = 0.0,
                ExgCh1 = 1.0,
                ExgCh2 = 2.0,
                ExgCh3 = 3.0,
                ExgCh4 = 4.0,
                ExgCh5 = 5.0,
                ExgCh6 = 6.0,
                ExgCh7 = 7.0,
            };

            double prevChannel = -1.0;
            int countChannels = 0;
            for (int i = 0; i < sample.NumberExgChannels; i++)
            {
                countChannels++;
                Assert.AreEqual(1.0, sample.GetExgDataForChannel(i) - prevChannel, 0.0000001);
                prevChannel = sample.GetExgDataForChannel(i);
            }

            Assert.AreEqual(8, countChannels);
        }



        [TestMethod]
        public void Cyton8SampleAcelChannelEnumerable()
        {
            BFCyton8Sample sample = new BFCyton8Sample()
            {
                AcelCh0 = 0.0,
                AcelCh1 = 1.0,
                AcelCh2 = 2.0,
            };

            double prevChannel = -1.0;
            int countChannels = 0;
            foreach (var nextChannel in sample.AccelData)
            {
                countChannels++;
                Assert.AreEqual(1.0, nextChannel - prevChannel, 0.0000001);
                prevChannel = nextChannel;
            }

            Assert.AreEqual(sample.NumberAccelChannels, countChannels);
        }


        [TestMethod]
        public void Cyton8SampleAcelChannelByIndex()
        {
            BFCyton8Sample sample = new BFCyton8Sample()
            {
                AcelCh0 = 0.0,
                AcelCh1 = 1.0,
                AcelCh2 = 2.0,
            };

            double prevChannel = -1.0;
            int countChannels = 0;
            for (int i = 0; i < sample.NumberAccelChannels; i++)
            {
                countChannels++;
                Assert.AreEqual(1.0, sample.GetAccelDataForChannel(i) - prevChannel, 0.0000001);
                prevChannel = sample.GetAccelDataForChannel(i);
            }

            Assert.AreEqual(3, countChannels);
        }
    }
}
