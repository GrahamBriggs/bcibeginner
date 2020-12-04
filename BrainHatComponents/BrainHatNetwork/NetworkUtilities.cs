using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BrainHatNetwork
{
    public static class NetworkUtilities
    {
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
