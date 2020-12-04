using LoggingInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BrainflowInterfaces;
using static BrainHatNetwork.Tcpip;
using Newtonsoft.Json;
using BrainHatNetwork;
using LSL;
using System.Xml.Linq;
using static LSL.liblsl;

namespace BrainHatServersMonitor
{
    /// <summary>
    /// Monitor communications from all servers on the network
    /// </summary>
    public class HatServersMonitor
    {
        //  Events
        public event LogEventDelegate Log;

        public event HatStatusUpdateDelegate HatStatusUpdate;
        public event HatConnectionUpdateDelegate HatConnectionChanged;

        /// <summary>
        /// Start monitor
        /// </summary>
        public async Task StartMonitorAsync()
        {
            await StopMonitorAsync();


            MonitorCancelTokenSource = new CancellationTokenSource();
            UdpReaderRunTask = RunUdpMulticastReaderAsync(MonitorCancelTokenSource.Token);
            ConnectionMonitorRunTask = RunConnectionStatusMonitorAsync(MonitorCancelTokenSource.Token);
            LslScannerRunTask = RunLslScannerAsync(MonitorCancelTokenSource.Token);
        }

        


        /// <summary>
        ///  Stop monitor
        /// </summary>
        public async Task StopMonitorAsync()
        {
            if (MonitorCancelTokenSource != null)
            {
                MonitorCancelTokenSource.Cancel();
                if (UdpReaderRunTask != null)
                    await Task.WhenAll(UdpReaderRunTask, ConnectionMonitorRunTask, LslScannerRunTask);

                MonitorCancelTokenSource = null;
                UdpReaderRunTask = null;
                ConnectionMonitorRunTask = null;
            }
        }


        public IEnumerable<IBrainHatServerConnection> ConnectedServers
        {
            get
            {
                return new List<IBrainHatServerConnection>(DiscoveredServers.Values);
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public HatServersMonitor()
        {
            DiscoveredServers = new ConcurrentDictionary<string, HatServer>();
            DiscoveredLslStreams = new ConcurrentDictionary<string, StreamInfo>();

        }

        protected ConcurrentDictionary<string, HatServer> DiscoveredServers;
       

        public HatServer GetServer(string hostName)
        {
            if ( DiscoveredServers.ContainsKey(hostName) )
            {
                return DiscoveredServers[hostName];
            }

            return null;
        }




        //  Thread cancel token and task
        CancellationTokenSource MonitorCancelTokenSource { get; set; }
        Task UdpReaderRunTask { get; set; }
        Task ConnectionMonitorRunTask { get; set; }
        Task LslScannerRunTask { get; set; }


        /// <summary>
        /// Task to monitor when discovered servers go stale
        /// </summary>
        protected async Task RunConnectionStatusMonitorAsync(CancellationToken cancelToken)
        {
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    var oldConnections = DiscoveredServers.Where(x => (DateTimeOffset.UtcNow - x.Value.TimeStamp) > TimeSpan.FromSeconds(1000));

                    if (oldConnections.Any())
                    {
                        foreach (var nextConnection in oldConnections)
                        {
                            try
                            {
                                Log?.Invoke(this, new LogEventArgs(this, "RunConnectionStatusMonitor", $"Lost connection to brainHat server {nextConnection.Key}.", LogLevel.INFO));
                                HatConnectionChanged?.Invoke(this, new HatConnectionEventArgs(HatConnectionState.Lost, nextConnection.Key));
                                
                                DiscoveredServers.TryRemove(nextConnection.Key, out var discard);
                                await discard.StopMonitorAsync();
                                discard.Log -= OnComponentLog;
                            }
                            catch (Exception ex)
                            {
                                Log?.Invoke(this, new LogEventArgs(this, "RunConnectionStatusMonitor", ex, LogLevel.ERROR));
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunConnectionStatusMonitor", e, LogLevel.ERROR));
            }
        }


        private ConcurrentDictionary<string, StreamInfo> DiscoveredLslStreams;

        private async Task RunLslScannerAsync(CancellationToken cancelToken)
        {
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    StreamInfo[] results =  resolve_stream("type", "BFSample", 0, 1);
                 
                    List<string> discoveredStreams = new List<string>();
                    foreach (var nextStreamInfo in results)
                    {
                        
                        
                        System.Console.Write(nextStreamInfo.as_xml());
                        var doc = XDocument.Parse(nextStreamInfo.as_xml());
                        if (doc != null)
                        {
                            var hostName = doc.Element("info")?.Element("hostname").Value;
                            var type = doc.Element("info")?.Element("type").Value;
                            if (hostName != null)
                            {
                                discoveredStreams.Add(hostName);
                                if (!DiscoveredLslStreams.ContainsKey(hostName))
                                {
                                    DiscoveredLslStreams.TryAdd(hostName, nextStreamInfo);
                                    Log?.Invoke(this, new LogEventArgs(this, "RunLslScannerAsync", $"Discovered new LSL on host {hostName}." , LogLevel.INFO));
                                }
                            }
                        }
                    }

                    var missingStreams = DiscoveredLslStreams.Where(x => !discoveredStreams.Contains(x.Key));
                    foreach ( var nextStreamInfo in missingStreams )
                    {
                        DiscoveredLslStreams.TryRemove(nextStreamInfo.Key, out var discard);
                        Log?.Invoke(this, new LogEventArgs(this, "RunLslScannerAsync", $"Lost LSL host {nextStreamInfo.Key}.", LogLevel.INFO));
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunLslScannerAsync", e, LogLevel.ERROR));
            }
        }


        /// <summary>
        /// Task to read from the UDP multicast socket
        /// </summary>
        protected async Task RunUdpMulticastReaderAsync(CancellationToken cancelToken)
        {
            try
            {
                //  create UDP client
                using (var udpClient = new UdpClient(BrainHatNetworkAddresses.StatusPort))
                {
                    //  join the multicast group
                    udpClient.JoinMulticastGroup(IPAddress.Parse(BrainHatNetworkAddresses.MulticastGroupAddress));
                    cancelToken.Register(() => udpClient.Close());

                    try
                    {
                        //  spin until canceled
                        while (!cancelToken.IsCancellationRequested)
                        {
                            try
                            {
                                //  read from multi cast server and process the result
                                await ProcessReceivedResult(await udpClient.ReceiveAsync());
                            }
                            catch (Exception ex)
                            {
                                Log?.Invoke(this, new LogEventArgs(this, "RunUdpMulticastReader", ex, LogLevel.ERROR));
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    { }
                }
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunUdpMulticastReader", e, LogLevel.FATAL));
            }
        }


        /// <summary>
        /// Process whatever we read from UDP data stream
        /// </summary>
        private async Task ProcessReceivedResult(UdpReceiveResult receiveResult)
        {
            try
            {
                //  we are expecting to read something like "messageType?arg1=abc&arg2=def&arg3=ghi"
                var parseReceived = Encoding.ASCII.GetString(receiveResult.Buffer).Split('?');

                //  verify correct numberr of arguments
                if (parseReceived.Count() == 2)
                {
                    switch (parseReceived[0])
                    {
                        case "networkstatus":
                            await ProcessNetworkStatus(parseReceived[1]);
                            break;

                        default:
                            Log?.Invoke(this, new LogEventArgs(this, "ProcessReceivedResult", $"Unexpected data type {parseReceived[0]}.", LogLevel.WARN));
                            break;
                    }
                }
                else
                {
                    Log?.Invoke(this, new LogEventArgs(this, "ProcessReceivedResult", $"Invalid message from server with {parseReceived.Count()} parameters.", LogLevel.ERROR));
                }
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "ProcessReceivedResult", e, LogLevel.ERROR));
            }
        }


        /// <summary>
        /// Process network connection status message
        /// </summary>
        protected async Task ProcessNetworkStatus(string argString)
        {
            try
            {
                var args = HttpUtility.ParseQueryString(argString);

                var hostName = args.Get("hostName");
                var status = args.Get("status");

                var serverStatus = JsonConvert.DeserializeObject<BrainHatServerStatus>(status);

                var timeOffset = DateTimeOffset.UtcNow - serverStatus.TimeStamp;

                //  check the list of discovered servers
                if (!DiscoveredServers.ContainsKey(hostName))
                {
                    await CreateNewHatServer(serverStatus);
                }
                else
                {
                    var server = DiscoveredServers[hostName];

                    //  update server connection state
                    await server.UpdateConnection(serverStatus);
                    server.TimeStamp = serverStatus.TimeStamp;

                    //  set raw data status for the event message
                    serverStatus.ReceivingRaw = server.ReceivingRaw;
                    serverStatus.RawLatency = server.RawLatency;
                }

                Log?.Invoke(this, new LogEventArgs(this, "ProcessNetworkStatus", $"Network status offset time for host {hostName}: {timeOffset.TotalSeconds:F6} s", LogLevel.TRACE));

                SendConnectionStatusUpdateEvents(serverStatus);
            }
            catch (Exception ex)
            {
                Log?.Invoke(this, new LogEventArgs(this, "ProcessReceivedResult", ex, LogLevel.ERROR));
            }
        }

        private async Task CreateNewHatServer(BrainHatServerStatus serverStatus)
        {
            if ( ! DiscoveredLslStreams.ContainsKey(serverStatus.HostName) )
            {
                Log?.Invoke(this, new LogEventArgs(this, "ProcessNetworkStatus", $"Discovered new brainHat server {serverStatus.HostName}, but no matching LSL stream - ignoring.", LogLevel.DEBUG));
                return;
            }

            Log?.Invoke(this, new LogEventArgs(this, "ProcessNetworkStatus", $"Discovered new brainHat server {serverStatus.HostName}.", LogLevel.INFO));

            var hatServer = new HatServer(serverStatus, DiscoveredLslStreams[serverStatus.HostName]);
            hatServer.Log += OnComponentLog;
            await hatServer.StartMonitorAsync();
            DiscoveredServers.TryAdd(serverStatus.HostName, hatServer);
            HatConnectionChanged?.Invoke(this, new HatConnectionEventArgs(HatConnectionState.Discovered, serverStatus.HostName));
        }


        /// <summary>
        /// Send connection status update event
        /// </summary>
        private async void SendConnectionStatusUpdateEvents(BrainHatServerStatus status)
        {
            try
            {
                var pingSpeed = TimeSpan.FromSeconds(-1);
                try
                {
                    var server = GetServer(status.HostName);
                    if (server != null)
                    {
                        var sw = new System.Diagnostics.Stopwatch();
                        sw.Start();
                        var response = await Tcpip.GetTcpResponse(server.IpAddress, BrainHatNetworkAddresses.ServerPort, "ping\n", 2000, 2000);
                        sw.Stop();
                        pingSpeed = sw.Elapsed;
                    }
                }
                catch (Exception e)
                {
                    Log?.Invoke(this, new LogEventArgs(this, "SendConnectionStatusChangedEvents", e, LogLevel.WARN));
                }

                status.PingSpeed = pingSpeed;
            

                HatStatusUpdate?.Invoke(this, new BrainHatStatusEventArgs(status));
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "SendConnectionStatusChangedEvents", e, LogLevel.ERROR));
            }
        }



        /// <summary>
        /// Pass through to the log function for the data processor component
        /// </summary>
        private void OnComponentLog(object sender, LogEventArgs e)
        {
            Log?.Invoke(sender, e);
        }


    }
}
