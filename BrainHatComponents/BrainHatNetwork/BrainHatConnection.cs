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
            }

            public HatConnectionState State { get; set; }
            public string HostName { get; set; }
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
