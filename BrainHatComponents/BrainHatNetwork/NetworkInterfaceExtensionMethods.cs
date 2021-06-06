using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BrainHatNetwork
{
    public static class NetworkInterfaceExtensionMethods
    {

        public static IEnumerable<NetworkInterface> GetActiveNetworkInterfaces()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nextInterface in networkInterfaces)
            {
                if ((!nextInterface.Supports(NetworkInterfaceComponent.IPv4)) ||
                    (nextInterface.OperationalStatus != OperationalStatus.Up))
                {
                    continue;
                }

                var ipAddress = GetInterfaceIpAddress(nextInterface);

                if (ipAddress == null)
                {
                    continue;
                }

                yield return nextInterface;
            }
        }

        public static IPAddress GetInterfaceIpAddress(NetworkInterface networkInterface)
        {
            IPInterfaceProperties adapterProperties = networkInterface.GetIPProperties();
            UnicastIPAddressInformationCollection unicastIPAddresses = adapterProperties.UnicastAddresses;
            IPAddress ipAddress = null;

            foreach (UnicastIPAddressInformation unicastIPAddress in unicastIPAddresses)
            {
                if (unicastIPAddress.Address.AddressFamily != AddressFamily.InterNetwork)
                {
                    continue;
                }

                ipAddress = unicastIPAddress.Address;
                break;
            }

            return ipAddress;
        }
    }
}
