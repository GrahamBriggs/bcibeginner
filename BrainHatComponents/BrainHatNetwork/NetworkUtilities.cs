using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace BrainHatNetwork
{
    public static class NetworkUtilities
    {
        public static string GetLocalIPAddress()
        {
            GetNetworkAddresses(out string eth0, out string wlan0);
            if (eth0.Length > 0)
                return eth0;
            else
                return wlan0;
        }

        public static void GetNetworkAddresses(out string eth0, out string wlan0)
        {
            eth0 = "";
            wlan0 = "";
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            switch (ni.Name)
                            {
                                case "eth0":
                                case "Ethernet":
                                    eth0 = ip.Address.ToString();
                                    break;
                                case "wlan0":
                                case "Wi-Fi":
                                    wlan0 = ip.Address.ToString();
                                    break;
                            }
                        }
                    }
                }
            }
        }



        static string HostName = null;
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
