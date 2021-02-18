using BrainflowInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrainflowDataProcessing
{
    public class OBCIGuiFormatFileReader
    {
        public bool ReadFile(string fileName)
        {
            _Samples = new List<IBFSample>();

            using (var reader = new StreamReader(fileName))
            {

                var nextLine = reader.ReadLine();
                while (nextLine !=  null )
                {
                    if (nextLine.Contains("%") || nextLine.Contains("Sample Index"))
                    {
                        ParseHeaderLine(nextLine);
                    }
                    else
                    {
                        CreateSample(nextLine);
                    }

                    nextLine = reader.ReadLine();
                }
            }

            return IsValidFile();
        }

        bool IsValidFile()
        {
            return BoardId >= 0 && NumberOfChannels > 0 && SampleRate > 0 && StartTime.HasValue && EndTime.HasValue;
        }


        private void CreateSample(string nextLine)
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

            if (!StartTime.HasValue)
                StartTime = newSample.TimeStamp;
            EndTime = newSample.TimeStamp;

            _Samples.Add(newSample);
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
