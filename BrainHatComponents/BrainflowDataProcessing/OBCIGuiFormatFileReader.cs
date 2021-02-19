using BrainflowInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    public class OBCIGuiFormatFileReader
    {
        public async Task<bool> ReadFile(string fileName)
        {
            _Samples = new List<IBFSample>();
            string prevLine = "";
            using (var fileReader = await FileSystemExtensionMethods.WaitForFileAsync(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileReader))
            {

                var nextLine = reader.ReadLine();
                
                while (nextLine != null)
                {
                    if (nextLine.Contains("%") || nextLine.Contains("Sample Index"))
                    {
                        ParseHeaderLine(nextLine);
                    }
                    else
                    {
                        if (!CreateSample(nextLine))
                            break;
                    }

                    nextLine = reader.ReadLine();
                    if (nextLine != null)
                        prevLine = nextLine;
                }
            }

            return IsValidFile();

        }

        bool IsValidFile()
        {
            return BoardId >= 0 && NumberOfChannels > 0 && SampleRate > 0 && StartTime.HasValue && EndTime.HasValue;
        }


        private bool CreateSample(string nextLine)
        {
            IBFSample newSample = null;

            switch ( BoardId )
            {
                case 0:
                    newSample = new BFCyton8Sample(nextLine);
                    break;

                case 2:
                    newSample = new BFCyton16Sample(nextLine);
                    break;

                default:
                    throw new Exception($"Board ID is not set.");
            }

            if (Math.Abs(newSample.TimeStamp-0) < 0.0000001)
                return false;

            if (!StartTime.HasValue)
                StartTime = newSample.TimeStamp;
            EndTime = newSample.TimeStamp;

            _Samples.Add(newSample);

            return true;
        }


        private void ParseHeaderLine(string nextLine)
        {
            if (nextLine.Contains("%Number of channels"))
            {
                NumberOfChannels = int.Parse(nextLine.Split('=')[1].Trim());
            }
            else if (nextLine.Contains("%Sample Rate"))
            {
                SampleRate = int.Parse(nextLine.Split('=')[1].Split('H')[0].Trim());
            }
            else if (nextLine.Contains("%Board"))
            {
                var parse = nextLine.Split('=');

                switch (parse[1].Trim())
                {
                    case "OpenBCI_GUI$BoardCytonSerial":
                        BoardId = 0;
                        break;

                    case "OpenBCI_GUI$BoardCytonSerialDaisy":
                        BoardId = 2;
                        break;
                }
            }
        }

        public int BoardId { get; protected set; }
        public int SampleRate { get; protected set; }
        public int NumberOfChannels { get; protected set; }
        public double? StartTime { get; protected set; }
        public double? EndTime { get; protected set; }
        public double Duration
        {
            get
            {
                if (StartTime.HasValue && EndTime.HasValue)
                    return EndTime.Value - StartTime.Value;
                else
                    return 0.0;
            }
        }

        public IEnumerable<IBFSample> Samples => _Samples;

        protected List<IBFSample> _Samples;
    }
}
