using BrainflowInterfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace BrainflowInterfacesTests
{
    [TestClass]
    public class ParseCytonRegisterReport
    {
        [TestMethod]
        public void ParseRegisterReport()
        {

            using (var reader = new StreamReader("./TestFiles/AdsRegisters.txt"))
            {
                var report = reader.ReadToEnd();

                var boardSettings = new BrainHatBoardSettingsImplementation(report);

                Assert.AreEqual(2, boardSettings.Boards.Count());
                Assert.AreEqual(8, boardSettings.Boards.First().Channels.Count());
                Assert.AreEqual(8, boardSettings.Boards.Last().Channels.Count());

                foreach ( var nextChannel in boardSettings.Boards.First().Channels)
                {
                    Assert.AreEqual(false, nextChannel.PowerDown);
                    Assert.AreEqual(ChannelGain.Gain24, nextChannel.Gain);
                    Assert.AreEqual(ChannelInputType.AdsinputNormal, nextChannel.InputType);
                    Assert.AreEqual(true, nextChannel.Srb2);
                    Assert.AreEqual(true, nextChannel.Bias);
                }
                Assert.AreEqual(false, boardSettings.Boards.First().Srb1Set);

                foreach (var nextChannel in boardSettings.Boards.Last().Channels)
                {
                    Assert.AreEqual(false, nextChannel.PowerDown);
                    Assert.AreEqual(ChannelGain.Gain24, nextChannel.Gain);
                    Assert.AreEqual(ChannelInputType.AdsinputNormal, nextChannel.InputType);
                    Assert.AreEqual(true, nextChannel.Srb2);
                    Assert.AreEqual(true, nextChannel.Bias);
                }
                Assert.AreEqual(false, boardSettings.Boards.Last().Srb1Set);
            }
        }
    }
}
