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

            ConnectionMonitorRunTask = RunConnectionStatusMonitorAsync(MonitorCancelTokenSource.Token);
            LslScannerRunTask = RunLslSampleStreamScannerAsync(MonitorCancelTokenSource.Token);
            ReadStatusTask = RunLslStatusStreamScannerAsync(MonitorCancelTokenSource.Token);

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
                await Task.WhenAll(ConnectionMonitorRunTask, LslScannerRunTask, ReadStatusTask);

                foreach (var nextServer in DiscoveredServers)
                {
                    await nextServer.Value.StopReadingFromLslAsync();
                }

                MonitorCancelTokenSource = null;
                ReadStatusTask = null;
                ConnectionMonitorRunTask = null;
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
        Task ReadStatusTask;

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
                                await discard.StopHatClientAsync();
                                RegisterHatClientEvents(discard, false);
                               
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
        async Task RunLslSampleStreamScannerAsync(CancellationToken cancelToken)
        {
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    StreamInfo[] results = resolve_stream("type", "BFSample", 0, 1);

                    var availableHosts = new List<string>();
                    foreach (var nextStreamInfo in results)
                    {
                        DiscoverNewDataStreams(nextStreamInfo, availableHosts );
                    }

                    CleanUpLostDataStreams(availableHosts);
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
        /// Process the available data streams 
        /// Add any new streams to our colleciton of discovered streams
        /// </summary>
        private void DiscoverNewDataStreams(StreamInfo nextStreamInfo, List<string> availableHosts)
        {
            var doc = XDocument.Parse(nextStreamInfo.as_xml());
            if (doc != null)
            {
                var hostName = doc.Element("info")?.Element("hostname").Value;
                var type = doc.Element("info")?.Element("type").Value;
                if (hostName != null)
                {
                    availableHosts.Add(hostName);
                    if (!DiscoveredLslStreams.ContainsKey(hostName))
                    {
                        DiscoveredLslStreams.TryAdd(hostName, nextStreamInfo);
                        Log?.Invoke(this, new LogEventArgs(this, "RunLslScannerAsync", $"Discovered new LSL on host {hostName}.", LogLevel.INFO));
                        Log?.Invoke(this, new LogEventArgs(this, "RunLslScannerAsync", $"LSL Stream Info. {nextStreamInfo.as_xml()}", LogLevel.DEBUG));
                    }
                }
            }
        }


        /// <summary>
        /// Go through the list of available streams 
        /// and remove any missing strams from our collection of discovered streams
        /// </summary>
        private void CleanUpLostDataStreams(List<string> availableHosts)
        {
            var missingStreams = DiscoveredLslStreams.Where(x => !availableHosts.Contains(x.Key));
            foreach (var nextStreamInfo in missingStreams)
            {
                DiscoveredLslStreams.TryRemove(nextStreamInfo.Key, out var discard);
                Log?.Invoke(this, new LogEventArgs(this, "RunLslScannerAsync", $"Lost LSL host {nextStreamInfo.Key}.", LogLevel.INFO));
            }
        }

       


        /// <summary>
        /// Task to read from status stream
        /// This function will initiate creation of HatClient for discovered brainHat server.
        /// </summary>
        async Task RunLslStatusStreamScannerAsync(CancellationToken cancelToken)
        {
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    await Task.Delay(1000);

                    StreamInfo[] status = resolve_stream("type", "bhStatus", 0, 1);

                    List<string> discoveredStreams = new List<string>();
                    foreach (var nextStreamInfo in status)
                    {
                        await ProcessLslStatusStreamAsync(nextStreamInfo);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunUdpMulticastReaderAsync", e, LogLevel.FATAL));
            }
        }

                      
        /// <summary>
        /// Process network connection status message
        /// </summary>
        async Task ProcessLslStatusStreamAsync(StreamInfo streamInfo)
        {
            try
            {
                string hostName = null;
                var doc = XDocument.Parse(streamInfo.as_xml());

                hostName = doc?.Element("info")?.Element("hostname").Value;
                if (hostName == null || hostName.Length == 0)
                {
                    Log?.Invoke(this, new LogEventArgs(this, "ProcessLslStatusStreamAsync", "Invalid host name in status stream.", LogLevel.ERROR));
                    return;
                }

                //  create a new hat client for newly discovered status stream
                if (!DiscoveredServers.ContainsKey(hostName))
                {
                    await CreateNewHatClientAsync(streamInfo);
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
        async Task CreateNewHatClientAsync(StreamInfo streamInfo)
        {
            var doc = XDocument.Parse(streamInfo.as_xml());
            var hostName = doc.Element("info")?.Element("hostname").Value;

            if (!DiscoveredLslStreams.ContainsKey(hostName))
            {
                Log?.Invoke(this, new LogEventArgs(this, "CreateNewHatClient", $"Discovered new brainHat server {hostName}, but no matching LSL stream - ignoring.", LogLevel.DEBUG));
                return;
            }

            Log?.Invoke(this, new LogEventArgs(this, "CreateNewHatClient", $"Discovered new brainHat server {hostName}.", LogLevel.INFO));
            var hatClient = new HatClient(hostName, streamInfo, DiscoveredLslStreams[hostName]);
            RegisterHatClientEvents(hatClient, true);
            await hatClient.StartHatClientAsync();

            DiscoveredServers.TryAdd(hostName, hatClient);
        }

        private void RegisterHatClientEvents(HatClient hatClient, bool register)
        {
            if (register)
            {
                hatClient.Log += OnComponentLog;
                hatClient.HatConnectionChanged += OnHatConnectionChanged;
                hatClient.HatConnectionStatusUpdate += OnHatConnectionStatusUpdate;
            }
            else
            {
                hatClient.Log -= OnComponentLog;
                hatClient.HatConnectionChanged -= OnHatConnectionChanged;
                hatClient.HatConnectionStatusUpdate -= OnHatConnectionStatusUpdate;
            }
        }



        private void OnHatConnectionStatusUpdate(object sender, BrainHatStatusEventArgs e)
        {
            HatConnectionStatusUpdate?.Invoke(sender, e);
        }

        private void OnHatConnectionChanged(object sender, HatConnectionEventArgs e)
        {
            HatConnectionChanged?.Invoke(sender, e);
        }


        /// <summary>
        /// Pass through to the log function for the data processor component
        /// </summary>
        void OnComponentLog(object sender, LogEventArgs e)
        {
            Log?.Invoke(sender, e);
        }


    }
}
