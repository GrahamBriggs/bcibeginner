using BrainflowInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    public class OBCIGuiFormatFileReader : IBrainHatFileReader
    {
        //  Public Properties
        public int BoardId { get; private set; }

        public int SampleRate { get; private set; }

        public int NumberOfChannels { get; private set; }

        public double? StartTime { get; private set; }

        public double? EndTime { get; private set; }

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

        List<IBFSample> _Samples;
        public IEnumerable<IBFSample> Samples => _Samples;

        public bool IsValidFile => (BrainhatBoardShim.IsSupportedBoard(BoardId) && NumberOfChannels > 0 && SampleRate > 0 && StartTime.HasValue && EndTime.HasValue);

        /// <summary>
        /// Open the file and read the header, first record and last record (to calculate duration)
        /// does not save any other samples from the file
        /// Returns true if the file has valid information
        /// </summary>
        public async Task<bool> ReadFileForHeaderAsync(string fileName)
        {
            _Samples = new List<IBFSample>();
            using (var fileReader = await FileSystemExtensionMethods.WaitForFileAsync(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileReader))
            {
                int lineCount = 0;
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
                            {
                                _Samples.Add(firstSample);       //  save the first sample
                            }
                        }

                        lineCount++;
                    }

                    nextLine = reader.ReadLine();
                }

                if (_Samples.Count > 0)
                {
                    EndTime = _Samples.First().TimeStamp + lineCount / SampleRate;
                }
            }

            return IsValidFile;
        }


        /// <summary>
        /// Open the file and read it into memory
        /// </summary>
        public async Task<bool> ReadFileAsync(string fileName)
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
        IBFSample CreateSample(string nextLine)
        {
            IBFSample newSample = null;
            newSample = new BFSampleImplementation(BoardId);
            newSample.InitializeFromText(nextLine);
           
            //  check the timestamp for valid data, this indicates we read a partial line for example the file is actively being written
            if (Math.Abs(newSample.TimeStamp - 0) < 0.0000001)
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
        void ParseHeaderLine(string nextLine)
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
                        BoardId = (int)(BrainhatBoardIds.CYTON_BOARD);
                        break;

                    case "OpenBCI_GUI$BoardCytonSerialDaisy":
                        BoardId = (int)(BrainhatBoardIds.CYTON_DAISY_BOARD);
                        break;

                    case "Contec_KT88":
                        BoardId = (int)BrainhatBoardIds.CONTEC_KT88;
                        break;
                }
            }
        }

       
    }
}
