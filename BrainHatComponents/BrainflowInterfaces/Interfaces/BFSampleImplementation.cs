using System;
using System.Collections.Generic;
using System.Text;

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

        protected double[] ExgChannels;
        protected double[] AcelChannels;
        protected double[] OtherChannels;
        protected double[] AnalogChannels;



        public BFSampleImplementation()
        {
            ExgChannels = new double[0];
            AcelChannels = new double[0];
            OtherChannels = new double[0];
            AnalogChannels = new double[0];
        }

        public BFSampleImplementation(IBFSample template)
        {
            ExgChannels = new double[template.NumberExgChannels];
            AcelChannels = new double[template.NumberAccelChannels];
            OtherChannels = new double[template.NumberOtherChannels];
            AnalogChannels = new double[template.NumberAnalogChannels];
        }

        public BFSampleImplementation(int boardId)
        {
            switch (boardId)
            {
                case 0: //  cyton
                    ExgChannels = new double[8];
                    AcelChannels = new double[3];
                    OtherChannels = new double[6];
                    AnalogChannels = new double[3];
                    break;

                case 2: //  cyton + Daisy
                    ExgChannels = new double[16];
                    AcelChannels = new double[3];
                    OtherChannels = new double[6];
                    AnalogChannels = new double[3];
                    break;

                //  todo - add more cases for boards here
                default:
                    ExgChannels = new double[0];
                    AcelChannels = new double[0];
                    OtherChannels = new double[0];
                    AnalogChannels = new double[0];
                    break;
            }
        }

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
