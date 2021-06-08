using Microsoft.VisualStudio.TestTools.UnitTesting;
using BrainflowDataProcessing;
using System.Threading.Tasks;

namespace BrainflowDataProcessingTests
{
    [TestClass]
    public class OBCITxtFileReaderTest
    {
        [TestMethod]
        public async Task ReadObciGuiTxtFileCyton()
        {
            OBCIGuiFormatFileReader reader = new OBCIGuiFormatFileReader();
            await reader.ReadFileAsync("./TestFiles/ObciGuiTxtFileCyton.txt");

            Assert.AreEqual(0, reader.BoardId);
            Assert.AreEqual(250, reader.SampleRate);
            Assert.AreEqual(8, reader.NumberOfChannels);
        }

        [TestMethod]
        public async Task ReadObciGuiTxtFileDaisy()
        {
            OBCIGuiFormatFileReader reader = new OBCIGuiFormatFileReader();
            await reader.ReadFileAsync("./TestFiles/ObciGuiTxtFileDaisy.txt");

            Assert.AreEqual(2, reader.BoardId);
            Assert.AreEqual(125, reader.SampleRate);
            Assert.AreEqual(16, reader.NumberOfChannels);
        }
    }
}
