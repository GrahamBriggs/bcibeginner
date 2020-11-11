using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BrainHatSharp
{
    static class NetworkAddress
    {
        // TCPIP server port
        public static readonly int TcpipServerPort = 50997;

        //  UDP multicast
        public static readonly string MulticastGroupAddress = "234.5.6.7";
        public static readonly int MulticastLogPort = 50998;
        public static readonly int MulticastDataPort = 50999;


        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private static string HostName = null;
        public static string GetHostName()
        {
            if (HostName == null)
            {
                HostName = Dns.GetHostName();
            }

            return HostName;
        }

    }
}
