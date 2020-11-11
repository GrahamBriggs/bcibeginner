using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainHatClient
{
    public static class HatConnection
    {
        //  Network Address Constants - must be synced with bcHat on the RPi side
        public static readonly string MulticastGroupAddress = "234.5.6.7";

        //  C++ hatSimulator
        //  TCPIP
        private static readonly int ComServerPort = 49997;
        //  UDP Multicast
        private static readonly int MulticastLogPort = 49998;
        private static readonly int MulticastDataPort = 49999;

        //  C# hatSimulator
        //  TCPIP
        private static readonly int ComServerPortMono = 50997;
        //  UDP Multicast
        private static readonly int MulticastLogPortMono = 50998;
        private static readonly int MulticastDataPortMono = 50999;

        //  set this flag to false to monitor the C++ program
        public static bool MonoVersion { get; set; }

        public static int ServerPort => MonoVersion ? ComServerPortMono : ComServerPort;
        public static int DataPort => MonoVersion ? MulticastDataPortMono : MulticastDataPort;
        public static int LogPort => MonoVersion ? MulticastLogPortMono : MulticastLogPort;
       





        //  Extension method to check TCPIP message response
        public static bool CheckResponse(this string value)
        {
            if (value != null && value.Length > 2)
            {
                var response = value.Substring(0, 3);
                if (response == "ACK")
                    return true;
            }
            return false;
        }


        


    }
}
