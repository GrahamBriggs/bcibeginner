using System;
using System.Collections.Generic;
using System.Text;

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

        bool RecordingDataBrainHat { get; }
        bool RecordingDataBoard { get; }
        string RecordingFileNameBrainHat { get; }
        string RecordingFileNameBoard { get; }

        bool ReceivingRaw { get; }
        double RawLatency { get;  }

        DateTimeOffset TimeStamp { get; }
    }


    public class BrainHatServerStatus : IBrainHatServerStatus
    {
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

        public bool RecordingDataBrainHat { get; set; }
        public bool RecordingDataBoard { get; set; }
        public string RecordingFileNameBrainHat { get; set; }
        public string RecordingFileNameBoard { get; set; }

        public long UnixTimeMillis { get; set; }

        public TimeSpan PingSpeed { get; set; }

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
