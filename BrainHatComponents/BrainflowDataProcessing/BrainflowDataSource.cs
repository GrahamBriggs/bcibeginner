using BrainflowInterfaces;
using LoggingInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    public class BrainflowDataSource
    {
        public BrainflowDataSource()
        {

        }

        public BrainflowDataSource(int boardId, int numberOfChannels, int sampleRate, IEnumerable<IBFSample> data)
        {
            UnfilteredData = new List<IBFSample>(data);
            NumberOfChannels = numberOfChannels;
            SampleRate = sampleRate;
            BoardId = boardId;
        }


        public event LogEventDelegate Log;

        public double? StartTime => UnfilteredData?.First()?.TimeStamp;
        public double? EndTime => UnfilteredData?.Last()?.TimeStamp;

        public int BoardId { get; protected set; }
        public int NumberOfChannels { get; protected set; }
        public int SampleRate { get; protected set; }

        public async Task<bool> LoadDataFile(string fileName)
        {
            BrainHatFileReader reader = new BrainHatFileReader();
            if ( await reader.LoadDataFileAsync(fileName) )
            {
                BoardId = reader.BoardId;
                NumberOfChannels = reader.NumberOfChannels;
                SampleRate = reader.SampleRate;
                UnfilteredData = new List<IBFSample>(reader.Samples);
                return true;
            }

            Log?.Invoke(this, new LogEventArgs(this, "LoadDataFile", $"Failed to load data file {fileName}.", LogLevel.ERROR));
            return false;
        }

        /// <summary>
        /// Get a range of raw samples starting at 'from' seconds, and ending at 'to' seconds, relative to the timestamp of the newest sample 
        /// </summary>
        public IBFSample[] GetRawChunk(double from, double to)
        {
            return UnfilteredData.Where(x => x.TimeStamp >= from && x.TimeStamp <= to).ToArray();
        }

        List<IBFSample> UnfilteredData;
    }
}
