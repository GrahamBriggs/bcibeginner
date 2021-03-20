using System;
using BrainflowInterfaces;

namespace BrainHatNetwork
{
    public interface IBrainHatServerConnection
    {
        string HostName { get; }
        string Eth0Address { get; }
        string Wlan0Address { get; }
        string Wlan0Mode { get; }
        string IpAddress { get; }
        int DataPort { get; }
        int LogPort { get; }
    }


    public interface IBrainHatServerStatus : IBrainHatServerConnection
    {
        int BoardId { get; }
        int SampleRate { get; }
        int NumberOfChannels { get; }
        SrbSet CytonSRB1 { get; }
        SrbSet DaisySRB1 { get; }
        bool IsStreaming { get; }

        bool RecordingDataBrainHat { get; }
        bool RecordingDataBoard { get; }
        string RecordingFileNameBrainHat { get; }
        string RecordingFileNameBoard { get; }
        double RecordingDurationBrainHat { get;  }
        double RecordingDurationBoard { get;  }

        bool ReceivingRaw { get; }
        double RawLatency { get;  }

        TimeSpan OffsetTime { get;  }

        DateTimeOffset TimeStamp { get; }
    }


    public class BrainHatServerStatus : IBrainHatServerStatus
    {
        public BrainHatServerStatus()
        {
            HostName = "";
        }

        public string HostName { get; set; }
        public string Eth0Address { get; set; }
        public string Wlan0Address { get; set; }

        public string IpAddress
        {
            get
            {
                if (Eth0Address.Length > 0)
                    return Eth0Address;
                else
                    return Wlan0Address;
            }
        }

        public string Wlan0Mode { get; set; }
        public int DataPort { get; set; }
        public int LogPort { get; set; }

        public int BoardId { get; set; }
        public int SampleRate { get; set; }
        public int NumberOfChannels => brainflow.BoardShim.get_exg_channels(BoardId).Length;
        public SrbSet CytonSRB1 { get; set; }
        public SrbSet DaisySRB1 { get; set; }
        public bool IsStreaming { get; set; }

        public bool RecordingDataBrainHat { get; set; }
        public bool RecordingDataBoard { get; set; }
        public string RecordingFileNameBrainHat { get; set; }
        public string RecordingFileNameBoard { get; set; }
        public double RecordingDurationBrainHat { get; set; }
        public double RecordingDurationBoard { get; set; }

        public long UnixTimeMillis { get; set; }

        public TimeSpan OffsetTime { get; set; }

        public bool ReceivingRaw { get; set; }
        public double RawLatency { get; set; }

        public DateTimeOffset TimeStamp
        {
            get
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(UnixTimeMillis);
            }
            set
            {
                UnixTimeMillis = value.ToUnixTimeMilliseconds();
            }
        }
    }
}
