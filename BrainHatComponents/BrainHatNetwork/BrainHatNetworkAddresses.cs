using System;
using System.Collections.Generic;
using System.Text;

namespace BrainHatNetwork
{
    public static class BrainHatNetworkAddresses
    {
        //  Network Address Constants - must be synced with bcHat on the RPi side
        public static readonly string MulticastGroupAddress = "234.5.6.7";

        //  C++ hatSimulator
        //  TCPIP
        static readonly int ComServerPort = 49997;
        //  UDP Multicast Status
        static readonly int MulticastStatusPort = 49999;
        //  UDP Multicast Logs
        static readonly int MulticastLogPort = 49998;

        //  C# hatSimulator
        //  TCPIP
        static readonly int ComServerPortChannel1 = 50997;
        //  UDP Multicast
        static readonly int MulticastStatusPortChannel1 = 50999;
        //  UDP Multicast Logs
        static readonly int MulticastLogPortChannel1 = 50998;

        //  set this flag to to true to monitor alternate channel 1
        public static bool Channel1 { get; set; } = false;

        public static int ServerPort => Channel1 ? ComServerPortChannel1 : ComServerPort;
        public static int StatusPort => Channel1 ? MulticastStatusPortChannel1 : MulticastStatusPort;
        public static int LogPort => Channel1 ? MulticastLogPortChannel1 : MulticastLogPort;
    }
}
