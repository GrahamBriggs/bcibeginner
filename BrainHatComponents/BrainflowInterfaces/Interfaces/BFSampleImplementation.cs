using System;
using System.Collections.Generic;
using System.Text;
using brainflow;

namespace BrainflowInterfaces
{
    public class BFSampleImplementation : IBFSample
    {
        public double SampleIndex { get; set; }

        public double TimeStamp { get; set; }

        public int SampleSize => (2 + NumberExgChannels + NumberAccelChannels + NumberOtherChannels + NumberAnalogChannels);

        public DateTime ObservationTime => DateTimeOffset.FromUnixTimeMilliseconds((long)(TimeStamp * 1000.0)).ToLocalTime().DateTime;

        public int NumberExgChannels => ExgChannels.Length;

        public IEnumerable<double> ExgData => ExgChannels;

        public double GetExgDataForChannel(int channel)
        {
            if (channel < ExgChannels.Length)
                return ExgChannels[channel];
            return BrainflowConstants.MissingValue;
        }

        public void SetExgDataForChannel(int channel, double data)
        {
            if (channel < ExgChannels.Length)
                ExgChannels[channel] = data;

        }

        public int NumberAccelChannels => AcelChannels.Length;

        public IEnumerable<double> AccelData => AcelChannels;

        public double GetAccelDataForChannel(int channel)
        {
            if (channel < AcelChannels.Length)
                return AcelChannels[channel];
            return BrainflowConstants.MissingValue;
        }

        public void SetAccelDataForChannel(int channel, double data)
        {
            if (channel < AcelChannels.Length)
                AcelChannels[channel] = data;

        }

        public int NumberOtherChannels => OtherChannels.Length;

        public IEnumerable<double> OtherData => OtherChannels;

        public double GetOtherDataForChannel(int channel)
        {
            if (channel < OtherChannels.Length)
                return OtherChannels[channel];
            return BrainflowConstants.MissingValue;
        }

        public void SetOtherDataForChannel(int channel, double data)
        {
            if (channel < OtherChannels.Length)
                OtherChannels[channel] = data;

        }

        public int NumberAnalogChannels => AnalogChannels.Length;

        public IEnumerable<double> AnalogData => AnalogChannels;


        public double GetAnalogDataForChannel(int channel)
        {
            if (channel < AnalogChannels.Length)
                return AnalogChannels[channel];
            return BrainflowConstants.MissingValue;
        }

        public void SetAnalogDataForChannel(int channel, double data)
        {
            if (channel < AnalogChannels.Length)
                AnalogChannels[channel] = data;

        }

        double[] ExgChannels;
        double[] AcelChannels;
        double[] OtherChannels;
        double[] AnalogChannels;


        public BFSampleImplementation()
        {
            ExgChannels = new double[0];
            AcelChannels = new double[0];
            OtherChannels = new double[0];
            AnalogChannels = new double[0];
        }

        public BFSampleImplementation(IBFSample copy)
        {
            SampleIndex = copy.SampleIndex;

            ExgChannels = new double[copy.NumberExgChannels];
            for (int i = 0; i < NumberExgChannels; i++)
                SetExgDataForChannel(i, copy.GetExgDataForChannel(i));

            AcelChannels = new double[copy.NumberAccelChannels];
            for (int i = 0; i < NumberAccelChannels; i++)
                SetAccelDataForChannel(i, copy.GetAccelDataForChannel(i));

            OtherChannels = new double[copy.NumberOtherChannels];
            for (int i = 0; i < NumberOtherChannels; i++)
                SetOtherDataForChannel(i, copy.GetOtherDataForChannel(i));

            AnalogChannels = new double[copy.NumberAnalogChannels];
            for (int i = 0; i < NumberAnalogChannels; i++)
                SetAnalogDataForChannel(i, copy.GetAccelDataForChannel(i));

            TimeStamp = copy.TimeStamp;
        }


        public BFSampleImplementation(int boardId)
        {
            ExgChannels = new double[BrainhatBoardShim.GetNumberOfExgChannels(boardId)];
            AcelChannels = new double[BrainhatBoardShim.GetNumberOfAccelChannels(boardId)];
            OtherChannels = new double[BrainhatBoardShim.GetNumberOfOtherChannels(boardId)];
            AnalogChannels = new double[BrainhatBoardShim.GetNumberOfAnalogChannels(boardId)];
        }


        /// <summary>
        /// Initialize the sample from CSV text
        /// OpenBCI GUI convention fields in order
        /// SampleIndex,ExgChannels,AccelChannels,OtherChannels,AnalogChannels,TimeStamp
        /// </summary>
        /// <param name="text"></param>
        public void InitializeFromText(string text)
        {
            var fields = text.Split(',');
            if (fields.Length >= SampleSize)
            {
                int index = 0;
                SampleIndex = double.Parse(fields[index++]);

                for (int i = 0; i < NumberExgChannels; i++)
                {
                    SetExgDataForChannel(i,  double.Parse(fields[index++]));
                }

                for ( int i = 0; i < NumberAccelChannels; i++)
                {
                    SetAccelDataForChannel(i, double.Parse(fields[index++]));
                }

                for (int i = 0; i < NumberOtherChannels; i++)
                {
                    SetOtherDataForChannel(i, double.Parse(fields[index++]));
                }

                for (int i = 0; i < NumberAnalogChannels; i++)
                {
                    SetAnalogDataForChannel(i, double.Parse(fields[index++]));
                }

                TimeStamp = double.Parse(fields[index++]);
            }
        }


        /// <summary>
        /// Initialize from a raw sample double vector
        /// </summary>
        public void InitializeFromSample(double[] sample)
        {
            int indexCount = 0;
            SampleIndex = sample[indexCount++];

            for (int i = 0; i < NumberExgChannels; i++)
                SetExgDataForChannel(i, sample[indexCount++]);

            for (int i = 0; i < NumberAccelChannels; i++)
                SetAccelDataForChannel(i, sample[indexCount++]);

            for (int i = 0; i < NumberOtherChannels; i++)
                SetOtherDataForChannel(i, sample[indexCount++]);

            for (int i = 0; i < NumberAnalogChannels; i++)
                SetAnalogDataForChannel(i, sample[indexCount++]);

            TimeStamp = sample[indexCount];
        }


        
        /// <summary>
        /// Create a double array of raw values from the sample
        /// </summary>
        public double[] AsRawSample()
        {
            var sample = new double[SampleSize];
            var index = 0;

            sample[index++] = SampleIndex;

            for (int i = 0; i < NumberExgChannels; i++)
                sample[index++] = GetExgDataForChannel(i);

            for (int i = 0; i < NumberAccelChannels; i++)
                sample[index++] = GetAccelDataForChannel(i);

            for (int i = 0; i < NumberOtherChannels; i++)
                sample[index++] = GetOtherDataForChannel(i);

            for (int i = 0; i < NumberAnalogChannels; i++)
                sample[index++] = GetAnalogDataForChannel(i);

            sample[index] = TimeStamp;

            return sample;
        }
    }
}
