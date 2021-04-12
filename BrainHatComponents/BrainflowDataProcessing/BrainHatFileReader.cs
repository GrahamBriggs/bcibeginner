using BrainflowInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    public class BrainHatFileReader
    {
        public int BoardId => FileReader.BoardId;

        public int SampleRate => FileReader.SampleRate;

        public int NumberOfChannels => FileReader.NumberOfChannels;

        public double? StartTime => FileReader.StartTime;

        public double? EndTime => FileReader.EndTime;

        public double Duration => FileReader.Duration;

        public bool IsValidFile => FileReader.IsValidFile;

        public IEnumerable<IBFSample> Samples => FileReader.Samples;


        public async Task<bool> LoadDataFileAsync(string fileName)
        {
            if (Path.GetExtension(fileName).ToUpper() == ".BDF")
            {
                FileReader = new BDFFormatFileReader();
                return await FileReader.ReadFile(fileName);
            }
            else if (Path.GetExtension(fileName).ToUpper() == ".TXT")
            {
                FileReader = new OBCIGuiFormatFileReader();
                return await FileReader.ReadFile(fileName);
            }

            return false;
        }

        IBrainHatFileReader FileReader;
    }
}
