using BrainflowInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    public class OBCIGuiFormatFileReader : IBrainHatFileReader
    {
        //  Public Properties
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

        public bool IsValidFile => (BoardId >= 0 && NumberOfChannels > 0 && SampleRate > 0 && StartTime.HasValue && EndTime.HasValue);
        
        /// <summary>
        /// Open the file and read the header, first record and last record (to calculate duration)
        /// does not save any other samples from the file
        /// Returns true if the file has valid information
        /// </summary>
        public async Task<bool> ReadFileForHeader(string fileName)
        {
            _Samples = new List<IBFSample>();
            using (var fileReader = await FileSystemExtensionMethods.WaitForFileAsync(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileReader))
            {
                string previousLine = "";
                var nextLine = reader.ReadLine();
                while (nextLine != null)
                {
                    if (nextLine.Contains("%") || nextLine.Contains("Sample Index"))
                    {
                        ParseHeaderLine(nextLine);
                    }
                    else
                    {


                        if (_Samples.Count == 0)
                        {
                            var firstSample = CreateSample(nextLine);
                            if (firstSample != null)
                                _Samples.Add(firstSample);       //  save the first sample
                        }

                        if (IsCompleteLine(nextLine))
                            previousLine = nextLine;
                    }

                    nextLine = reader.ReadLine();
                }

                var lastSample = CreateSample(previousLine);
                if ( lastSample != null )
                    _Samples.Add(lastSample);   //  save the last sample
            }

            return IsValidFile;
        }


        bool IsCompleteLine(string nextLine)
        {
            var tokens = nextLine.Split(',');
            switch ( BoardId )
            {
                case 0:
                    return tokens.Length >= 23;
                case 2:
                    return tokens.Length >= 31;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Open the file and read it into memory
        /// </summary>
        public async Task<bool> ReadFile(string fileName)
        {
            _Samples = new List<IBFSample>();
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
                        var newSample = CreateSample(nextLine);
                        if (newSample == null)
                            break;

                        _Samples.Add(newSample);
                    }

                    nextLine = reader.ReadLine();
                }
            }

            return IsValidFile;
        }


        /// <summary>
        /// Create a sample from a single line of ascii text
        /// </summary>
        private IBFSample CreateSample(string nextLine)
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

            //  check the timestamp for valid data, this indicates we read a partial line for example the file is actively being written
            if (Math.Abs(newSample.TimeStamp-0) < 0.0000001)
                return null;

            //  cache the start time of the first record
            if (!StartTime.HasValue)
                StartTime = newSample.TimeStamp;
            //  cahce the end time
            EndTime = newSample.TimeStamp;

            return newSample;
        }


        /// <summary>
        /// Parse a single line of header text
        /// </summary>
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

      

        protected List<IBFSample> _Samples;
    }
}
