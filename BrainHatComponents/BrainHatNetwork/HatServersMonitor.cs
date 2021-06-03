using LoggingInterfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static LSL.liblsl;

namespace BrainHatNetwork
{
    /// <summary>
    /// Monitor communications from all servers on the network.
    /// Creates and manages HatClient object for each connected server.
    /// </summary>
    public class HatServersMonitor
    {
        //  Events
        public event LogEventDelegate Log;

        public event HatConnectionStatusUpdateDelegate HatConnectionStatusUpdate;
        public event HatConnectionUpdateDelegate HatConnectionChanged;


        /// <summary>
        /// Start monitor
        /// </summary>
        public async Task StartMonitorAsync()
        {
            await StopMonitorAsync();

            MonitorCancelTokenSource = new CancellationTokenSource();
            StartUdpMulticastReaders(MonitorCancelTokenSource.Token);
            ConnectionMonitorRunTask = RunConnectionStatusMonitorAsync(MonitorCancelTokenSource.Token);
            ReadLogPortTask = RunReadLogPortAsync(MonitorCancelTokenSource.Token);
            LslScannerRunTask = RunLslScannerAsync(MonitorCancelTokenSource.Token);

            ReportNetworkTimeInterval.Start();
        }


        /// <summary>
        ///  Stop monitor
        /// </summary>
        public async Task StopMonitorAsync()
        {
            if (MonitorCancelTokenSource != null)
            {
                MonitorCancelTokenSource.Cancel();
                if ( MulticastReaders != null )
                    await Task.WhenAll(MulticastReaders);
                await Task.WhenAll(ConnectionMonitorRunTask, LslScannerRunTask, ReadLogPortTask);

                foreach (var nextServer in DiscoveredServers)
                {
                    await nextServer.Value.StopReadingFromLslAsync();
                }

                MonitorCancelTokenSource = null;
                MulticastReaders.Clear();
                MulticastReaders = null;
                ConnectionMonitorRunTask = null;
                ReadLogPortTask = null;
            }
        }


        /// <summary>
        /// Get a collection of connected servers
        /// </summary>
        public IEnumerable<HatClient> ConnectedServers
        {
            get
            {
                return new List<HatClient>(DiscoveredServers.Values);
            }
        }


        /// <summary>
        /// Get the client object for a specified server
        /// </summary>
        public HatClient GetHatClient(string hostName)
        {
            if (DiscoveredServers.ContainsKey(hostName))
            {
                return DiscoveredServers[hostName];
            }

            return null;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public HatServersMonitor()
        {
            DiscoveredServers = new ConcurrentDictionary<string, HatClient>();
            DiscoveredLslStreams = new ConcurrentDictionary<string, StreamInfo>();
        }

        ConcurrentDictionary<string, HatClient> DiscoveredServers;
        ConcurrentDictionary<string, StreamInfo> DiscoveredLslStreams;

        //  Thread cancel token and task
        CancellationTokenSource MonitorCancelTokenSource;

        Task ConnectionMonitorRunTask;
        Task LslScannerRunTask;
        Task ReadLogPortTask;
        List<Task> MulticastReaders;

        Stopwatch ReportNetworkTimeInterval = new Stopwatch();


        /// <summary>
        /// Task to monitor when discovered servers go stale (disconnected)
        /// </summary>
        async Task RunConnectionStatusMonitorAsync(CancellationToken cancelToken)
        {
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    var oldConnections = DiscoveredServers.Where(x => (DateTimeOffset.UtcNow - x.Value.TimeStamp) > TimeSpan.FromSeconds(30)); 

                    if (oldConnections.Any())
                    {
                        foreach (var nextConnection in oldConnections)
                        {
                            if ( nextConnection.Value.SecondsSinceLastSample < 30 )
                            {
                                //  glitch in the matrix, lost server status but it is still receiving streaming data
                                continue;
                            }

                            try
                            {
                                //  remove this server
                                Log?.Invoke(this, new LogEventArgs(nextConnection.Key, this, "RunConnectionStatusMonitor", $"Lost connection to brainHat server {nextConnection.Key}.", LogLevel.INFO));
                                HatConnectionChanged?.Invoke(this, new HatConnectionEventArgs(HatConnectionState.Lost, nextConnection.Key));

                                DiscoveredServers.TryRemove(nextConnection.Key, out var discard);
                                await discard.StopReadingFromLslAsync();
                                discard.Log -= OnComponentLog;

                                DiscoveredLslStreams.TryRemove(nextConnection.Key, out var discardLsl);
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

       
        /// <summary>
        /// Task to scan for LSL streams
        /// </summary>
        async Task RunLslScannerAsync(CancellationToken cancelToken)
        {
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    StreamInfo[] results = resolve_stream("type", "BFSample", 0, 1);

                    List<string> discoveredStreams = new List<string>();
                    foreach (var nextStreamInfo in results)
                    {
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
                                    Log?.Invoke(this, new LogEventArgs(this, "RunLslScannerAsync", $"Discovered new LSL on host {hostName}.", LogLevel.INFO));
                                    Log?.Invoke(this, new LogEventArgs(this, "RunLslScannerAsync", $"LSL Stream Info. {nextStreamInfo.as_xml()}", LogLevel.DEBUG));
                                }
                            }
                        }
                    }


                    var missingStreams = DiscoveredLslStreams.Where(x => !discoveredStreams.Contains(x.Key));
                    foreach (var nextStreamInfo in missingStreams)
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
        /// Start a UDP multicast reader thread for each available network interface
        /// </summary>
        void StartUdpMulticastReaders(CancellationToken cancelToken)
        {
            MulticastReaders = new List<Task>();

            // list of UdpClients to send multicasts
            List<UdpClient> sendClients = new List<UdpClient>();

            // join multicast group on all available network interfaces
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface nextInterface in networkInterfaces)
            {
                if ((!nextInterface.Supports(NetworkInterfaceComponent.IPv4)) ||
                    (nextInterface.OperationalStatus != OperationalStatus.Up))
                {
                    continue;
                }

                IPInterfaceProperties adapterProperties = nextInterface.GetIPProperties();
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

                if (ipAddress == null)
                {
                    continue;
                }

                Log?.Invoke(this, new LogEventArgs(this, "StartUdpMulticastReaders", $"Starting UDP multicast client for {nextInterface.Name} at {ipAddress}.", LogLevel.ERROR));
                MulticastReaders.Add(RunUdpMulticastReaderAsync(cancelToken, ipAddress));
            }
        }


        /// <summary>
        /// Task to read from the server UDP multicast socket.
        /// This function will initiate creation of HatClient for discovered brainHat server.
        /// </summary>
        async Task RunUdpMulticastReaderAsync(CancellationToken cancelToken, IPAddress interfaceAddress)
        {
            try
            {
                //  create UDP client
                using (var udpClient = new UdpClient())
                {
                    cancelToken.Register(() => udpClient.Close());

                    IPEndPoint localpt = new IPEndPoint(IPAddress.Any, BrainHatNetworkAddresses.StatusPort);
                    udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    udpClient.Client.Bind(localpt);

                    //  join the multicast group
                    udpClient.JoinMulticastGroup(IPAddress.Parse(BrainHatNetworkAddresses.MulticastGroupAddress), interfaceAddress);

                    //  spin until canceled
                    while (!cancelToken.IsCancellationRequested)
                    {
                        try
                        {
                            //  read from multi cast server and process the result
                            await ProcessReceivedResult(await udpClient.ReceiveAsync());
                        }
                        catch (ObjectDisposedException)
                        { }
                        catch (Exception ex)
                        {
                            Log?.Invoke(this, new LogEventArgs(this, "RunUdpMulticastReaderAsync", ex, LogLevel.ERROR));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunUdpMulticastReaderAsync", e, LogLevel.FATAL));
            }
        }


        /// <summary>
        /// Process whatever we read from UDP data stream
        /// </summary>
        async Task ProcessReceivedResult(UdpReceiveResult receiveResult)
        {
            try
            {
                //  we are expecting to read something like "requestType?arg1=abc&arg2=def&arg3=ghi"
                var argParser = new UriArgParser(Encoding.ASCII.GetString(receiveResult.Buffer));

                //  verify correct numberr of arguments
                switch (argParser.Request)
                {
                    case "networkstatus":
                        await ProcessNetworkStatus(argParser);
                        break;

                    default:
                        Log?.Invoke(this, new LogEventArgs(this, "ProcessReceivedResult", $"Unexpected data type {argParser.Request}.", LogLevel.WARN));
                        break;
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
        async Task ProcessNetworkStatus(UriArgParser argParser)
        {
            try
            {
                var hostName = argParser.GetArg("hostname");
                var status = argParser.GetArg("status");

                if (hostName.Length > 0)
                {
                    var serverStatus = JsonConvert.DeserializeObject<BrainHatServerStatus>(status);

                    if (serverStatus.IpAddress.Length == 0)
                        return;

                    //  check the list of discovered servers
                    if (!DiscoveredServers.ContainsKey(hostName))
                    {
                        if (!await CreateNewHatClientAsync(serverStatus))
                            return;
                    }
                    else
                    {
                        var server = DiscoveredServers[hostName];

                        //  update server connection state
                        var updateTime = server.TimeStamp;

                        await server.UpdateConnectionAsync(serverStatus);
                        serverStatus.OffsetTime = DateTimeOffset.UtcNow - serverStatus.TimeStamp;
                        server.TimeStamp = DateTimeOffset.UtcNow;
                        server.OffsetTime = serverStatus.OffsetTime;
                        server.CytonSRB1 = serverStatus.CytonSRB1;
                        server.DaisySRB1 = serverStatus.DaisySRB1;
                        server.IsStreaming = serverStatus.IsStreaming;
                        server.Eth0Address = serverStatus.Eth0Address;
                        server.Wlan0Address = serverStatus.Wlan0Address;

                        //  set raw data status for the event message
                        serverStatus.ReceivingRaw = server.ReceivingRaw;
                        serverStatus.RawLatency = server.RawLatency;
                    }

                    SendConnectionStatusUpdateEvents(serverStatus);
                }
            }
            catch (Exception ex)
            {
                Log?.Invoke(this, new LogEventArgs(this, "ProcessReceivedResult", ex, LogLevel.ERROR));
            }
        }


        /// <summary>
        /// Create a new HatClient for the discovered server
        /// </summary>
        async Task<bool> CreateNewHatClientAsync(BrainHatServerStatus serverStatus)
        {
            if (!DiscoveredLslStreams.ContainsKey(serverStatus.HostName))
            {
                Log?.Invoke(this, new LogEventArgs(this, "CreateNewHatClient", $"Discovered new brainHat server {serverStatus.HostName}, but no matching LSL stream - ignoring.", LogLevel.DEBUG));
                return false;
            }

            serverStatus.OffsetTime = DateTimeOffset.UtcNow - serverStatus.TimeStamp;

            Log?.Invoke(this, new LogEventArgs(this, "CreateNewHatClient", $"Discovered new brainHat server {serverStatus.HostName}.", LogLevel.INFO));

            string localIpAddress = "";
            try
            {
                localIpAddress = NetworkUtilities.GetLocalIPAddress();
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "CreateNewHatClient", $"Unable to get local IP address.{e}", LogLevel.ERROR));
            }

            var hatServer = new HatClient(serverStatus, DiscoveredLslStreams[serverStatus.HostName], localIpAddress);
            serverStatus.OffsetTime = DateTimeOffset.UtcNow - serverStatus.TimeStamp;

            //  sync time on the server if it is more than 5 seconds off
            if (serverStatus.OffsetTime.TotalSeconds > 5)
            {
                Log?.Invoke(this, new LogEventArgs(this, "CreateNewHatClient", "Server time is more than 5 seconds behind system time, setting server time.", LogLevel.INFO));

                var sw = new Stopwatch();
                sw.Start();

                var response = await Tcpip.GetTcpResponseAsync(serverStatus.IpAddress, BrainHatNetworkAddresses.ServerPort, "ping");
                if (response.Contains("ACK"))
                {
                    sw.Stop();

                    response = await Tcpip.GetTcpResponseAsync(serverStatus.IpAddress, BrainHatNetworkAddresses.ServerPort, $"settime?time={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (sw.Elapsed.Milliseconds/2)}");

                    if ( response == null || !response.Contains("ACK"))
                    {
                        Log?.Invoke(this, new LogEventArgs(this, "CreateNewHatClient", "Failed to set server time.", LogLevel.ERROR));
                    }
                }
                else
                {
                    Log?.Invoke(this, new LogEventArgs(this, "CreateNewHatClient", "Failed to get server ping.", LogLevel.ERROR));
                }


            }


            hatServer.OffsetTime = serverStatus.OffsetTime;
            hatServer.TimeStamp = DateTimeOffset.UtcNow;
            hatServer.Log += OnComponentLog;

            DiscoveredServers.TryAdd(serverStatus.HostName, hatServer);
            HatConnectionChanged?.Invoke(this, new HatConnectionEventArgs(HatConnectionState.Discovered, serverStatus.HostName, serverStatus.IpAddress, serverStatus.BoardId, serverStatus.SampleRate));

            return true;
        }
     

        /// <summary>
        /// Send connection status update event
        /// </summary>
        void SendConnectionStatusUpdateEvents(BrainHatServerStatus status)
        {
            try
            {
                if (DiscoveredLslStreams.ContainsKey(status.HostName))
                {
                    var streamInfo = DiscoveredLslStreams[status.HostName];
                    var doc = XDocument.Parse(streamInfo.as_xml());
                    if (doc != null)
                    {
                        var port = doc.Element("info")?.Element("v4data_port").Value;
                        int portNumber;
                        if (int.TryParse(port, out portNumber))
                            status.DataPort = portNumber;
                    }
                }

                HatConnectionStatusUpdate?.Invoke(this, new BrainHatStatusEventArgs(status));

                if (ReportNetworkTimeInterval.ElapsedMilliseconds > 30000)
                {
                    ReportNetworkTimeInterval.Restart();
                    Log?.Invoke(this, new LogEventArgs(status.HostName, this, "ProcessNetworkStatus", $"Network status for {status.HostName}: Offset time {status.OffsetTime.TotalSeconds:F4} s.", LogLevel.TRACE));
                }
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "SendConnectionStatusChangedEvents", e, LogLevel.ERROR));
            }
        }


        //  Remote Log Monitor
        #region RemoteLogMonitor

        async Task RunReadLogPortAsync(CancellationToken cancelToken)
        {
            try
            {
                using (var udpClient = new UdpClient())
                {
                    cancelToken.Register(() => { try { udpClient.Close(); } catch { } });

                    try
                    {
                        IPEndPoint localpt = new IPEndPoint(IPAddress.Any, BrainHatNetworkAddresses.LogPort);
                        udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        udpClient.Client.Bind(localpt);
                        udpClient.JoinMulticastGroup(IPAddress.Parse(BrainHatNetworkAddresses.MulticastGroupAddress));
                        
                        while (!cancelToken.IsCancellationRequested)
                        {
                            try
                            {
                                //  wait for the next read, and then split it into the command and the arguments
                                UriArgParser argParser = new UriArgParser(Encoding.ASCII.GetString((await udpClient.ReceiveAsync()).Buffer));

                                //  see if this is recognized command
                                switch (argParser.Request)
                                {
                                    case "log":
                                        var logString = argParser.GetArg("log");
                                        var hostName = argParser.GetArg("hostname");
                                        if (logString != null)
                                        {
                                            var log = JsonConvert.DeserializeObject<RemoteLogEventArgs>(logString);
                                            Log?.Invoke(this, new LogEventArgs(log));
                                        }
                                        break;

                                    default:
                                        Log?.Invoke(this, new LogEventArgs(this, "RunReadLogPortAsync", $"Received invalid remote log: {argParser.Request}.", LogLevel.WARN));
                                        break;
                                }

                            }
                            catch (ObjectDisposedException)
                            { }
                            catch (Exception exc)
                            {
                                Log?.Invoke(this, new LogEventArgs(this, "RunReadLogPortAsync", exc, LogLevel.ERROR));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log?.Invoke(this, new LogEventArgs(this, "RunReadLogPortAsync", ex, LogLevel.FATAL));
                    }
                }
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunReadLogPortAsync", e, LogLevel.FATAL));
            }
        }


        #endregion



        /// <summary>
        /// Pass through to the log function for the data processor component
        /// </summary>
        void OnComponentLog(object sender, LogEventArgs e)
        {
            Log?.Invoke(sender, e);
        }


    }
}
