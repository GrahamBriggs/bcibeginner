using BrainflowDataProcessing;
using LoggingInterface;
using Newtonsoft.Json;
using OpenBCIInterfaces;
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


namespace BrainHatClient
{
    /// <summary>
    /// Hat monitor reads status and raw data from the hat
    /// </summary>
    class HatServerMonitor
    {
        //  Events
        public event LogEventDelegate Log;

        public event HatConnectionStatusUpdateDelegate HatConnectionStatusChanged;
        public event HatConnectionUpdateDelegate HatConnectionChanged;
        public event HatRawDataReceivedEventDelegate RawDataReceived;


        /// <summary>
        /// Start monitor
        /// </summary>
        public async Task StartMonitorAsync()
        {
            await StopMonitorAsync();

            
            MonitorCancelTokenSource = new CancellationTokenSource();
            UdpReaderRunTask = RunUdpMulticastReader(MonitorCancelTokenSource.Token);
            ConnectionMonitorRunTask = RunConnectionStatusMonitor(MonitorCancelTokenSource.Token);

            CountRecordsTimer.Start();
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
                    await Task.WhenAll(UdpReaderRunTask, ConnectionMonitorRunTask);

                MonitorCancelTokenSource = null;
                UdpReaderRunTask = null;
                ConnectionMonitorRunTask = null;
            }
        }


        /// <summary>
        /// Change the connected server
        /// </summary>
        public void ChangeConnectedServer(string hostName)
        {
            if (hostName != null)
            {
                if (DiscoveredServers.ContainsKey(hostName))
                {
                    ConnectedServer = DiscoveredServers[hostName];
                    Log?.Invoke(this, new LogEventArgs(this, "ChangeConnectedServer", $"Change connection to brainHat server {ConnectedServer.HostName}.", LogLevel.INFO));
                    HatConnectionChanged?.Invoke(this, new HatConnectionEventArgs(HatConnectionState.Connected, hostName));
                }
                else
                {
                    Log?.Invoke(this, new LogEventArgs(this, "ChangeConnectedServer", $"Unable to change to server {hostName}. It does not exist?", LogLevel.WARN));
                }
            }
            else
            {
                if (ConnectedServer != null)
                {
                    Log?.Invoke(this, new LogEventArgs(this, "ChangeConnectedServer", $"Disconnected from brainHat server {ConnectedServer.HostName}.", LogLevel.INFO));
                    HatConnectionChanged?.Invoke(this, new HatConnectionEventArgs(HatConnectionState.Disconnected, hostName));
                    ConnectedServer = null;
                }
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public HatServerMonitor()
        {
            DiscoveredServers = new ConcurrentDictionary<string, HatConnectionStatus>();
            ConnectedServer = null;

            CountRecordsTimer = new System.Diagnostics.Stopwatch();

        }

        protected ConcurrentDictionary<string, HatConnectionStatus> DiscoveredServers;
        public HatConnectionStatus ConnectedServer { get; protected set; }
        public bool IsConnected { get { return ConnectedServer != null; } }

        /// <summary>
        /// IP Address of the connected brainHat server device
        /// </summary>
        public string IpAddress
        {
            get
            {
                if (IsConnected)
                {
                    if (ConnectedServer.Eth0.Length > 0)
                        return ConnectedServer.Eth0;
                    else
                        return ConnectedServer.Wlan0;
                }
                return "";
            }
        }


        /// <summary>
        /// Get the host name of the connected server
        /// </summary>
        public string HostName
        {
            get
            {
                if (IsConnected)
                {
                    return ConnectedServer.HostName;
                }
                else
                    return "--";
            }
        }
     


        //  Thread cancel token and task
        CancellationTokenSource MonitorCancelTokenSource { get; set; }
        Task UdpReaderRunTask { get; set; }
        Task ConnectionMonitorRunTask { get; set; }
            

        /// <summary>
        /// Task to monitor when discovered servers go stale
        /// </summary>
        protected async Task RunConnectionStatusMonitor(CancellationToken cancelToken)
        {
            try
            {
                while ( ! cancelToken.IsCancellationRequested )
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    var oldConnections = DiscoveredServers.Where(x => (DateTimeOffset.UtcNow - x.Value.TimeStamp) > TimeSpan.FromSeconds(1000));
                    
                    if ( oldConnections.Any() )
                    {
                        foreach ( var nextConnection in oldConnections )
                        {
                            Log?.Invoke(this, new LogEventArgs(this, "RunConnectionStatusMonitor", $"Lost connection to brainHat server {nextConnection.Key}.", LogLevel.INFO));
                            HatConnectionChanged?.Invoke(this, new HatConnectionEventArgs(HatConnectionState.Lost, nextConnection.Key));
                            DiscoveredServers.TryRemove(nextConnection.Key, out var discard);

                            if ( ConnectedServer != null && ConnectedServer.HostName == nextConnection.Key )
                            {
                                HatConnectionChanged?.Invoke(this, new HatConnectionEventArgs(HatConnectionState.Disconnected, nextConnection.Key));
                                ConnectedServer = null;
                            }
                        }
                    }
                }
            }
            catch ( OperationCanceledException)
            { }
            catch ( Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunConnectionStatusMonitor", e, LogLevel.ERROR));
            }
        }


        /// <summary>
        /// Task to read from the UDP multicast socket
        /// </summary>
        protected async Task RunUdpMulticastReader(CancellationToken cancelToken)
        {
            try
            {
                //  create UDP client
                using (var udpClient = new UdpClient(HatConnection.DataPort) )
                {
                    //  join the multicast group
                    udpClient.JoinMulticastGroup(IPAddress.Parse(HatConnection.MulticastGroupAddress));
                    cancelToken.Register(() => udpClient.Close());

                    try
                    {
                        //  spin until canceled
                        while (!cancelToken.IsCancellationRequested)
                        {
                            try
                            {
                                //  read from multi cast server and process the result
                                ProcessReceivedResult(await udpClient.ReceiveAsync());
                            }
                            catch (Exception ex)
                            {
                                Log?.Invoke(this, new LogEventArgs(this, "RunUdpMulticastReader", ex, LogLevel.ERROR));
                            }
                        }
                    }
                    catch ( OperationCanceledException )
                    {  }
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
        private void ProcessReceivedResult(UdpReceiveResult receiveResult)
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
                            ProcessNetworkStatus(parseReceived[1]);
                            break;

                        case "rawData":
                            ProcessRawData(parseReceived[1]);
                            break;

                        default:
                            Log?.Invoke(this, new LogEventArgs(this, "ProcessReceivedResult", $"Unknown command {parseReceived[0]}.", LogLevel.WARN));
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
        protected void ProcessNetworkStatus(string argString)
        {
            try
            {
                var args = HttpUtility.ParseQueryString(argString);

                var hostName = args.Get("hostname");
                var eth0 = args.Get("eth0");
                var wlan0 = args.Get("wlan0");
                var wlanMode = args.Get("wlanmode");
                var timeString = args.Get("time");
             
                var timeTag = long.Parse(timeString);
                var timeOffset = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()  - timeTag;

                //  check the list of discovered servers
                if ( ! DiscoveredServers.ContainsKey(hostName) )
                {
                    DiscoveredServers.TryAdd(hostName, new HatConnectionStatus() { HostName = hostName, Eth0 = eth0, Wlan0 = wlan0, WlanMode = wlanMode, TimeStamp = DateTimeOffset.UtcNow });
                    Log?.Invoke(this, new LogEventArgs(this, "ProcessNetworkStatus", $"Discovered new brainHat server {hostName}.", LogLevel.INFO));
                    HatConnectionChanged?.Invoke(this, new HatConnectionEventArgs(HatConnectionState.Discovered, hostName));
                }
                else
                {
                    DiscoveredServers[hostName].TimeStamp = DateTimeOffset.UtcNow;
                }

                if ( ! IsConnected )
                {
                    ConnectedServer = DiscoveredServers[hostName];
                    Log?.Invoke(this, new LogEventArgs(this, "ProcessNetworkStatus", $"Connected to new brainHat server {hostName}.", LogLevel.INFO));
                    HatConnectionChanged?.Invoke(this, new HatConnectionEventArgs(HatConnectionState.Connected, hostName));
                }

                Log?.Invoke(this, new LogEventArgs(this, "ProcessNetworkStatus", $"Network status offset time {(timeOffset/1000.0).ToString("F6")} s", LogLevel.TRACE));

                SendConnectionStatusUpdateEvents(timeString);
            }
            catch (Exception ex)
            {
                Log?.Invoke(this, new LogEventArgs(this, "ProcessReceivedResult", ex, LogLevel.ERROR));
            }
        }

        private async void SendConnectionStatusUpdateEvents(string timeTag)
        {
            try
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                var response = await TcpRequest.GetTcpResponse(IpAddress, HatConnection.ServerPort, "ping\n", 5000, 5000);
                sw.Stop();
                
                HatConnectionStatus status = new HatConnectionStatus()
                {
                    Eth0 = ConnectedServer.Eth0,
                    Wlan0 = ConnectedServer.Wlan0,
                    WlanMode = ConnectedServer.WLan0Connection,
                };

                HatConnectionStatusChanged?.Invoke(this, new HatConnectionStatusEventArgs(status, sw.Elapsed));
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "SendConnectionStatusChangedEvents", e, LogLevel.ERROR));
            }
        }


        /// <summary>
        /// Process hat raw data message
        /// </summary>
        protected void ProcessRawData(string argString)
        {
            if (!IsConnected)
                return;

            try
            {
                var args = HttpUtility.ParseQueryString(argString);
                var sender = args.Get("sender");
                if (sender == ConnectedServer.HostName)
                {
                    var data = JsonConvert.DeserializeObject<OpenBciCyton8Reading>(args.Get("data"));
                    RawDataReceived?.Invoke(this, new HatRawDataReceivedEventArgs(data));

                    RecordsCount++;

                    LogRawDataProcessingPerformance(data);
                }
            }
            catch ( Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "ProcessRawData", e, LogLevel.ERROR));
            }
        }

        int RecordsCount = 0;
        System.Diagnostics.Stopwatch CountRecordsTimer;

        /// <summary>
        /// Log raw data processing performance
        /// </summary>
        private void LogRawDataProcessingPerformance(OpenBciCyton8Reading data)
        {
            if (CountRecordsTimer.ElapsedMilliseconds > 5000)
            {
                var timeOffset = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000 - data.TimeStamp;
                Log?.Invoke(this, new LogEventArgs(this, "ProcessRawData", $"Logged {(int)(RecordsCount / CountRecordsTimer.Elapsed.TotalSeconds)} records per second. Offset time {timeOffset.ToString("F6")} s.", LogLevel.TRACE));
                CountRecordsTimer.Restart();
                RecordsCount = 0;
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
