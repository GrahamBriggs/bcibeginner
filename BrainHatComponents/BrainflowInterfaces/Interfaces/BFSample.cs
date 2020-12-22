using System;
using System.Collections.Generic;
using System.Text;

namespace BrainflowInterfaces
{
    public interface IBFSample
    {
        double SampleIndex { get; }

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

        int NumberOtherChannels { get; }
        IEnumerable<double> OtherData { get; }
        double GetOtherDataForChannel(int channel);

        int NumberAnalogChannels { get; }
        IEnumerable<double> AnalogData { get; }
        double GetAnalogDataForChannel(int channel);

        double[] AsRawSample();
    }
}
