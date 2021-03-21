using System;
using System.Collections.Generic;
using System.Text;

namespace BrainflowInterfaces
{
    public interface IBFSample
    {
        double SampleIndex { get; set; }

        double TimeStamp { get; set; }

        int SampleSize { get; }

        DateTime ObservationTime { get; }

        int NumberExgChannels { get; }
        IEnumerable<double> ExgData { get; }
        double GetExgDataForChannel(int channel);
        void SetExgDataForChannel(int channel, double data);

        int NumberAccelChannels { get; }
        IEnumerable<double> AccelData { get; }
        double GetAccelDataForChannel(int channel);
        void SetAccelDataForChannel(int channel, double data);

        int NumberOtherChannels { get; }
        IEnumerable<double> OtherData { get; }
        double GetOtherDataForChannel(int channel);
        void SetOtherDataForChannel(int channel, double data);

        int NumberAnalogChannels { get; }
        IEnumerable<double> AnalogData { get; }
        double GetAnalogDataForChannel(int channel);
        void SetAnalogDataForChannel(int channel, double data);

        double[] AsRawSample();
    }


    public static class BFSample
    {
        public static IBFSample MakeNewSample(this IBFSample value)
        {
            if (value is BFCyton8Sample cyton8Sample)
                return new BFCyton8Sample(cyton8Sample);
            else if (value is BFCyton16Sample cyton16Sample)
                return new BFCyton16Sample(cyton16Sample);
            else
                return null;    // TODO - ganglion
        }

        public static IBFSample MakeNewSample(this int value)
        {
            switch (value)
            {
                case 0:
                    return new BFCyton8Sample();
                case 2:
                    return new BFCyton16Sample();
                default:
                    return null;    // TODO - ganglion
            }
        }
    }
}
