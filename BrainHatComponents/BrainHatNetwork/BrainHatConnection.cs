using System;
using System.Collections.Generic;
using System.Text;

namespace BrainHatNetwork
{

    /// <summary>
    /// Hat connection state event
    /// Signals when hat is discovered | connected | disconnected
    /// </summary>
    public class HatConnectionEventArgs : EventArgs
    {
        public HatConnectionEventArgs(HatConnectionState state, string hostName)
        {
            State = state;
            HostName = hostName;
            IpAddress = "";
            
        }

        public HatConnectionEventArgs(HatConnectionState state, string hostName, string ipAddress, int boardId, int sampleRate)
        {
            State = state;
            HostName = hostName;
            IpAddress = ipAddress;
            BoardId = boardId;
            SampleRate = sampleRate;
        }

        public HatConnectionState State { get; set; }
        public string HostName { get; set; }
        public string IpAddress { get; set; }
        public int BoardId { get; set; }
        public int SampleRate { get; set; }

    }
    //
    public delegate void HatConnectionUpdateDelegate(object sender, HatConnectionEventArgs e);
    //
    public enum HatConnectionState
    {
        Discovered,
        Lost,
        Connected,
        Disconnected,
    }

}
