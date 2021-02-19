using BrainflowInterfaces;
using LoggingInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public void LoadOpenBciGuiTxtFile(string fileName)
        {
            OBCIGuiFormatFileReader reader = new OBCIGuiFormatFileReader();
            reader.ReadFile(fileName);

            BoardId = reader.BoardId;
            NumberOfChannels = reader.NumberOfChannels;
            SampleRate = reader.SampleRate;

            UnfilteredData = new List<IBFSample>(reader.Samples);
        }

        /// <summary>
        /// Get a range of raw samples starting at 'from' seconds, and ending at 'to' seconds, relative to the timestamp of the newest sample 
        /// </summary>
        public IBFSample[] GetRawChunk(double from, double to)
        {
            return UnfilteredData.Where(x => x.TimeStamp >= from && x.TimeStamp <= to).ToArray();
        }

        protected List<IBFSample> UnfilteredData;
    }
}
