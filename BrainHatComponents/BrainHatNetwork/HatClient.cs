using BrainflowInterfaces;
using LoggingInterfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static LSL.liblsl;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
using System.Diagnostics;

namespace BrainHatNetwork
{
    /// <summary>
    /// A client connection to a single brainHat server
    /// Receives raw data from the server via LSL,     
    /// and provides TCPIP connection to send query/command and receive response from server.
    /// </summary>
    public class HatClient : IBrainHatServerConnection, IBrainHatServerStatus
    {
        //  Events
        public event LogEventDelegate Log;

        public event HatConnectionUpdateDelegate HatConnectionChanged;
        public event HatConnectionStatusUpdateDelegate HatConnectionStatusUpdate;

        public event BFSampleEventDelegate RawDataReceived;

        //  Properties
        public string HostName { get; protected set; }

        public string Eth0Address { get; protected set; }

        public string Wlan0Address { get; protected set; }

        public string Wlan0Mode { get; protected set; }

        public int BoardId { get; protected set; }

        public int SampleRate { get; protected set; }

        public int NumberOfChannels => BrainhatBoardShim.GetNumberOfExgChannels(BoardId);

        public SrbSet CytonSRB1 { get; protected set; }

        public SrbSet DaisySRB1 { get; protected set; }

        public bool IsStreaming { get; protected set; }

        public bool RecordingDataBrainHat { get; protected set; }

        public bool RecordingDataBoard { get; protected set; }

        public string RecordingFileNameBrainHat { get; protected set; }

        public string RecordingFileNameBoard { get; protected set; }

        public double RecordingDurationBrainHat { get; protected set; }

        public double RecordingDurationBoard { get; protected set; }

        public DateTimeOffset TimeStamp { get; protected set; }

        public bool ReceivingRaw => (ReadDataTaskCancelTokenSource != null && TimeSinceLastSample.ElapsedMilliseconds < 5000);

        public double SecondsSinceLastSample => TimeSinceLastSample.Elapsed.TotalSeconds;

        public double RawLatency => RawDataOffsetTime;

        public TimeSpan OffsetTime { get; protected set; }

        public string IpAddress
        {
            get
            {
                var address = "";
                if (Eth0Address.Length > 0)
                    address = Eth0Address;
                else
                    address = Wlan0Address;

                //  override the address to use loopback when we are on the same machine
                //  this allows operation of the server/viewer on same machine when not connected to network
                if (address.Length > 0 && (address.CompareTo(LocalIpAddress) == 0 || address.Substring(0, 7).CompareTo("169.254") == 0))
                    address = "127.0.0.1";

                return address;
            }
        }


        /// <summary>
        /// Start the hat client
        /// </summary>
        public async Task StartHatClientAsync()
        {
            TimeStamp = DateTimeOffset.UtcNow;

            await StopHatClientAsync();

            try
            {
                LocalIpAddress = NetworkUtilities.GetLocalIPAddress();
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "StartHatClientAsync", $"Unable to get local IP address.{e}", LogLevel.ERROR));
            }

            ReadStatusTaskCancelTokenSource = new CancellationTokenSource();
            ReadStatusPortTask = RunReadStatusPortAsync(ReadStatusTaskCancelTokenSource.Token);
            Log?.Invoke(this, new LogEventArgs(HostName, this, "StartHatClient", $"Started HatServer for {HostName}.", LogLevel.INFO));
        }


        /// <summary>
        /// Stop the hat client
        /// </summary>
        public async Task StopHatClientAsync()
        {
            await StopReadingFromLslAsync();

            if (ReadStatusTaskCancelTokenSource != null)
            {
                ReadStatusTaskCancelTokenSource.Cancel();
                await ReadStatusPortTask;

                ReadStatusTaskCancelTokenSource = null;
                ReadStatusPortTask = null;

                Log?.Invoke(this, new LogEventArgs(HostName, this, "StopHatClient", $"Stopped HatServer for {HostName}.", LogLevel.INFO));
            }
        }


        /// <summary>
        /// Start reading from the LSL data stream
        /// </summary>
        public async Task StartReadingFromLslAsync()
        {
            await StopReadingFromLslAsync();

            ReadDataTaskCancelTokenSource = new CancellationTokenSource();
            ReadDataPortTask = RunReadDataPortAsync(ReadDataTaskCancelTokenSource.Token);
            CountRecordsTimer.Start();
            TimeSinceLastSample.Start();

            Log?.Invoke(this, new LogEventArgs(HostName, this, "StartMonitorAsync", $"Started HatServer for {HostName}.", LogLevel.INFO));
        }




        /// <summary>
        ///  Stop reading from the LSL data stream
        /// </summary>
        public async Task StopReadingFromLslAsync()
        {
            if (ReadDataTaskCancelTokenSource != null)
            {
                ReadDataTaskCancelTokenSource.Cancel();
                await ReadDataPortTask;

                ReadDataTaskCancelTokenSource = null;
                ReadDataPortTask = null;

                Log?.Invoke(this, new LogEventArgs(HostName, this, "StopMonitorAsync", $"Stopped HatServer for {HostName}.", LogLevel.INFO));
            }
        }



        /// <summary>
        /// Constructor
        /// </summary>
        public HatClient(string hostName, StreamInfo statusStream, StreamInfo dataStream)
        {
            HostName = hostName;

            Eth0Address = "";
            Wlan0Address = "";

            DataStream = dataStream;
            StatusStream = statusStream;

            int.TryParse(XDocument.Parse(dataStream.as_xml()).Element("info")?.Element("channel_count").Value, out var sampleSize);
            SampleSize = sampleSize;

            CountRecordsTimer = new System.Diagnostics.Stopwatch();
            TimeSinceLastSample = new System.Diagnostics.Stopwatch();
            TimeSinceLastSample.Start();

            TimeStamp = DateTimeOffset.UtcNow;

            ReadDataTaskCancelTokenSource = null;

            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
        }
        
        StreamInfo StatusStream;
        StreamInfo DataStream;
        public int SampleSize { get; private set; }

        string LocalIpAddress;

        bool SyncedTime;

        CancellationTokenSource ReadDataTaskCancelTokenSource;
        Task ReadDataPortTask;       
        CancellationTokenSource ReadStatusTaskCancelTokenSource;
        Task ReadStatusPortTask;

        Stopwatch ReportNetworkTimeInterval = new Stopwatch();
        const int ReadingDelay = 20;    //  read the LSL inlet at 50 Hz


        /// <summary>
        /// Update local IP address any time network address changes
        /// </summary>
        void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            LocalIpAddress = NetworkUtilities.GetLocalIPAddress();
        }

        /// <summary>
        /// Read from the LSL stream for server status
        /// </summary>
        async Task RunReadStatusPortAsync(CancellationToken cancelToken)
        {
            ReportNetworkTimeInterval.Restart();

            StreamInlet inlet = null;
            try
            {
                inlet = new StreamInlet(StatusStream);
                cancelToken.Register(() => inlet.close_stream());
                inlet.open_stream();
                SyncedTime = false;
                SetBoardProperties(inlet);

                Log?.Invoke(this, new LogEventArgs(HostName, this, "RunReadStatusPortAsync", $"Create LSL stream for status on host {HostName}.", LogLevel.DEBUG));
                HatConnectionChanged?.Invoke(this, new HatConnectionEventArgs(HatConnectionState.Discovered, HostName, "", BoardId, SampleRate));

                string[] sample = new string[1];
                //  spin until canceled
                while (!cancelToken.IsCancellationRequested)
                {
                    await Task.Delay(ReadingDelay);
                    try
                    {
                        inlet.pull_sample(sample);
                        await ParseServerStatus(sample[0]);
                    }
                    catch (ObjectDisposedException)
                    { }
                    catch (Exception ex)
                    {
                        Log?.Invoke(this, new LogEventArgs(HostName, this, "RunReadStatusPortAsync", ex, LogLevel.WARN));
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunReadStatusPortAsync", e, LogLevel.FATAL));
            }
            finally
            {
                if (inlet != null)
                    inlet.close_stream();
            }
        }

        void SetBoardProperties(StreamInlet inlet)
        {
            var info = inlet.info();

            BoardId = int.Parse(info.desc().child_value("boardId"));
            SampleRate = int.Parse(info.desc().child_value("sampleRate"));
        }


        /// <summary>
        /// Parse the server status string
        /// </summary>
        async Task ParseServerStatus(string sample)
        {
            var connectionStatus = JsonConvert.DeserializeObject<BrainHatServerStatus>(sample);

            //  update connection status with local properties
            connectionStatus.HostName = HostName;
            connectionStatus.OffsetTime = DateTimeOffset.UtcNow - connectionStatus.TimeStamp;
            connectionStatus.ReceivingRaw = ReceivingRaw;
            connectionStatus.RawLatency = RawLatency;

            //  update local properties with connection status
            TimeStamp = DateTimeOffset.UtcNow;
            OffsetTime = connectionStatus.OffsetTime;
            CytonSRB1 = connectionStatus.CytonSRB1;
            DaisySRB1 = connectionStatus.DaisySRB1;
            IsStreaming = connectionStatus.IsStreaming;
            Eth0Address = connectionStatus.Eth0Address;
            Wlan0Address = connectionStatus.Wlan0Address;

            if ( ! SyncedTime )
            {
                await SyncServerTime(connectionStatus);
            }

            HatConnectionStatusUpdate?.Invoke(this, new BrainHatStatusEventArgs(connectionStatus));

            if (ReportNetworkTimeInterval.ElapsedMilliseconds > 30000)
            {
                ReportNetworkTimeInterval.Restart();
                Log?.Invoke(this, new LogEventArgs(connectionStatus.HostName, this, "ParseServerStatus", $"Network status for {connectionStatus.HostName}: Offset time {connectionStatus.OffsetTime.TotalSeconds:F4} s.", LogLevel.TRACE));
            }
        }


        /// <summary>
        /// Check server time offset, 
        /// and sync it with client time if it is more than 5 seconds out
        /// </summary>
        async Task SyncServerTime(BrainHatServerStatus connectionStatus)
        {
            //  sync time on the server if it is more than 2 seconds off
            if (connectionStatus.OffsetTime.TotalSeconds > 2 && connectionStatus.IpAddress.Length > 0)
            {
                Log?.Invoke(this, new LogEventArgs(this, "CreateNewHatClient", "Server time is more than 5 seconds behind system time, setting server time.", LogLevel.INFO));

                var sw = new Stopwatch();
                sw.Start();

                var response = await Tcpip.GetTcpResponseAsync(connectionStatus.IpAddress, BrainHatNetworkAddresses.ServerPort, "ping");
                if (response.Contains("ACK"))
                {
                    sw.Stop();

                    response = await Tcpip.GetTcpResponseAsync(connectionStatus.IpAddress, BrainHatNetworkAddresses.ServerPort, $"settime?time={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (sw.Elapsed.Milliseconds / 2)}");

                    if (response == null || !response.Contains("ACK"))
                    {
                        Log?.Invoke(this, new LogEventArgs(this, "CreateNewHatClient", "Failed to set server time.", LogLevel.ERROR));
                    }
                    else
                    {
                        SyncedTime = true;
                    }
                }
                else
                {
                    Log?.Invoke(this, new LogEventArgs(this, "CreateNewHatClient", "Failed to get server ping.", LogLevel.ERROR));
                }
            }
        }


        /// <summary>
        /// Run function for reading data on LSL multicast data port
        /// </summary>
        async Task RunReadDataPortAsync(CancellationToken cancelToken)
        {
            StreamInlet inlet = null;
            try
            {
                int maxChunkLen = (int)((SampleRate * 1.5) / ReadingDelay);
                inlet = new StreamInlet(DataStream,360, maxChunkLen);
                
                cancelToken.Register(() => inlet.close_stream());
                inlet.open_stream();

                Log?.Invoke(this, new LogEventArgs(HostName, this, "RunReadDataPortAsync", $"Create LSL data stream for host {HostName}.", LogLevel.DEBUG));

                double[,] buffer = new double[512, SampleSize];
                double[] timestamps = new double[512];

                //  spin until canceled
                while (!cancelToken.IsCancellationRequested)
                {
                    await Task.Delay(ReadingDelay);

                    try
                    {
                        int num = inlet.pull_chunk(buffer, timestamps);
                        ProcessChunk(buffer, num);
                    }
                    catch (ObjectDisposedException)
                    { }
                    catch (Exception ex)
                    {
                        Log?.Invoke(this, new LogEventArgs(HostName, this, "RunReadDataPortAsync", ex, LogLevel.WARN));
                    }
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunReadDataPortAsync", e, LogLevel.FATAL));
            }
            finally
            {
                if (inlet != null)
                    inlet.close_stream();
            }
        }


        /// <summary>
        /// Process chunk read
        /// </summary>
        void ProcessChunk(double[,] buffer, int num)
        {
            for (int s = 0; s < num; s++)
            {
                IBFSample nextSample = new BFSampleImplementation(BoardId);
                nextSample.InitializeFromSample(buffer.GetRow(s));
                RawDataReceived?.Invoke(this, new BFSampleEventArgs(nextSample));
                LogRawDataProcessingPerformance(nextSample);
            }
        }

        int RecordsCount = 0;
        System.Diagnostics.Stopwatch CountRecordsTimer;

        double RawDataOffsetTime;
        System.Diagnostics.Stopwatch TimeSinceLastSample;


        /// <summary>
        /// Log raw data processing performance
        /// </summary>
        void LogRawDataProcessingPerformance(IBFSample data)
        {
            RecordsCount++;
            TimeSinceLastSample.Restart();
            RawDataOffsetTime = Math.Max(Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000 - data.TimeStamp), RawDataOffsetTime);

            if (CountRecordsTimer.ElapsedMilliseconds > 5000)
            {
                Log?.Invoke(this, new LogEventArgs(HostName, this, "LogRawDataProcessingPerformance", $"{HostName} Logged {(int)(RecordsCount / CountRecordsTimer.Elapsed.TotalSeconds)} records per second. Offset time {RawDataOffsetTime.ToString("F6")} s.", LogLevel.TRACE));
                CountRecordsTimer.Restart();
                RawDataOffsetTime = 0.0;
                RecordsCount = 0;
            }
        }

    }
}
