using Microsoft.VisualStudio.TestTools.UnitTesting;
using BrainflowDataProcessing;

namespace BrainflowDataProcessingTests
{
    [TestClass]
    public class OBCITxtFileReaderTest
    {
        [TestMethod]
        public void ReadObciGuiTxtFileCyton()
        {
            OBCIGuiFormatFileReader reader = new OBCIGuiFormatFileReader();
            reader.ReadFile("./TestFiles/ObciGuiTxtFileCyton.txt");

            Assert.AreEqual(0, reader.BoardId);
            Assert.AreEqual(250, reader.SampleRate);
            Assert.AreEqual(8, reader.NumberOfChannels);
        }

        [TestMethod]
        public void ReadObciGuiTxtFileDaisy()
        {
            OBCIGuiFormatFileReader reader = new OBCIGuiFormatFileReader();
            reader.ReadFile("./TestFiles/ObciGuiTxtFileDaisy.txt");

            Assert.AreEqual(2, reader.BoardId);
            Assert.AreEqual(125, reader.SampleRate);
            Assert.AreEqual(16, reader.NumberOfChannels);
        }
    }
}
