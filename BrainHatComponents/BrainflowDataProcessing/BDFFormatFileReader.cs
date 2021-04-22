using BrainflowInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static EDFfile.EDFfile;
using EDFfile;
using Newtonsoft.Json;

namespace BrainflowDataProcessing
{
    public class BDFFormatFileReader : IBrainHatFileReader
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


        public bool IsValidFile => (BrainhatBoardShim.IsSupportedBoard(BoardId)  && NumberOfChannels > 0 && SampleRate > 0 && StartTime.HasValue && EndTime.HasValue);


        /// <summary>
        /// Open the file and read the header
        /// does not save any samples from the file
        /// Returns true if the file has a valid header
        /// </summary>
        public async Task<bool> ReadFileForHeader(string fileName)
        {
            using (var fileReader = await FileSystemExtensionMethods.WaitForFileAsync(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fileReader.Close();
            };

            _Samples = new List<IBFSample>();

            int fileHandle = -1;
            try
            {
                fileHandle = edfOpenFileReadOnly(fileName);
                if (fileHandle < 0)
                {
                    return false;
                }

                // get the header json and convert to header object
                var header = JsonConvert.DeserializeObject<EdfHeaderStruct>(edfGetHeaderAsJson(fileHandle));

                SetFilePropertiesFromHeader(header);
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (fileHandle >= 0)
                    edfCloseFile(fileHandle);
            }

            return IsValidFile;
        }


        ulong ReadDataRecordsCount;
        double DataRecordDuration;

        /// <summary>
        /// Open the file and read the data into memory
        /// </summary>
        public async Task<bool> ReadFile(string fileName)
        {
            int fileHandle = -1;
            try
            {
                using (var fileReader = await FileSystemExtensionMethods.WaitForFileAsync(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fileReader.Close();
                };

                _Samples = new List<IBFSample>();

                fileHandle = edfOpenFileReadOnly(fileName);
                if (fileHandle < 0)
                {
                    throw new Exception($"Unable to open file. Error {fileHandle}.");
                }

                // get the header json and convert to header object
                var header = JsonConvert.DeserializeObject<EdfHeaderStruct>(edfGetHeaderAsJson(fileHandle));
                SetFilePropertiesFromHeader(header);

                //  make the samples
                var signalCount = header.edfsignals;
                var samplesPerDataRecord = header.signalparam[0].smp_in_datarecord;

                ReadDataRecordsCount = 0;
                double[,] chunk;
                for (ulong i = 0; i < header.datarecords_in_file; i++)
                {
                    chunk = new double[signalCount, samplesPerDataRecord];
                    for (int j = 0; j < header.edfsignals; j++)
                    {
                        var thisSignal = edfReadPhysicalSamples(fileHandle, j, header.signalparam[j].smp_in_datarecord);
                        for (int k = 0; k < samplesPerDataRecord; k++)
                            chunk[j, k] = thisSignal[k];
                    }

                    CreateSamples(chunk);

                    ReadDataRecordsCount++;
                }

            }
            finally
            {
                if (fileHandle >= 0)
                    edfCloseFile(fileHandle);
            }

            return IsValidFile;
        }


        /// <summary>
        /// Create samples from a single data record (chunk of signals per sample)
        /// </summary>
        void CreateSamples(double[,] chunk)
        {
            double dataRecordTime = StartTime.Value + (ReadDataRecordsCount * DataRecordDuration);
            for (int i = 0; i < chunk.GetRow(0).Length; i++)
            {
                IBFSample newSample = null;
                newSample = new BFSampleImplementation(BoardId);
                if (newSample != null)
                {
                    newSample.InitializeFromSample(chunk.GetColumn(i));
                    newSample.TimeStamp = StartTime.Value + newSample.TimeStamp;
                    _Samples.Add(newSample);
                }
            }
        }


        /// <summary>
        /// Set the file properties from the header information
        /// </summary>
        bool SetFilePropertiesFromHeader(EdfHeaderStruct header)
        {
            BoardId = header.recording_additional.Trim().GetBoardId();
            switch (BoardId)
            {
                case 0:
                case 1:
                case 2:
                    break;

                default:
                    return false;
            }

            SampleRate = (int)(header.signalparam[0].smp_in_datarecord / (header.datarecord_duration * 1.0E-7));
            DataRecordDuration = header.datarecord_duration * 1.0E-7;

            NumberOfChannels = BrainhatBoardShim.GetNumberOfExgChannels(BoardId);

            var date = new DateTime(header.startdate_year, header.startdate_month, header.startdate_day, header.starttime_hour, header.starttime_minute, header.starttime_second);
            date = date.AddMilliseconds(header.starttime_subsecond / 10_000);
            StartTime = new DateTimeOffset(date.ToUniversalTime(), TimeSpan.FromHours(0)).ToUnixTimeInDoubleSeconds();

            EndTime = StartTime + (header.datarecords_in_file * (header.datarecord_duration * 1.0E-7));

            return true;
        }


        protected List<IBFSample> _Samples;
    }
}
