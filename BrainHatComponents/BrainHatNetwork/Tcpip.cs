using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BrainHatNetwork
{
    public static class Tcpip
    {
        public static async Task<string> GetTcpResponse(string address, int port, string send, int readTimeout = 1000, int writeTimeout = 1000)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    // Asynchronsly attempt to connect to server
                    await client.ConnectAsync(address, port);

                    using (var netstream = client.GetStream())
                    using (var writer = new StreamWriter(netstream) { AutoFlush = true })
                    using (var reader = new StreamReader(netstream))
                    {
                        netstream.ReadTimeout = readTimeout;
                        netstream.WriteTimeout = writeTimeout;

                        await writer.WriteLineAsync(send);

                        string response = await reader.ReadLineAsync();
                        return response;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
