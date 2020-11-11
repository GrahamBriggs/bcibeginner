using LoggingInterface;
using Newtonsoft.Json;
using System;
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
    class RemoteLogMonitor
    {
        public event RemoteLogEventDelegate RemoteLogReceived;
        public event LogEventDelegate Log;


        /// <summary>
        /// Start monitor
        /// </summary>
        public async Task StartMonitorAsync()
        {
            await StopMonitorAsync();

            MonitorCancelTokenSource = new CancellationTokenSource();
            MonitorRunTask = RunMonitor(MonitorCancelTokenSource.Token);
        }


        /// <summary>
        /// Stop Monitor
        /// </summary>
        public async Task StopMonitorAsync()
        {
            if (MonitorCancelTokenSource != null)
            {
                MonitorCancelTokenSource.Cancel();

                if (MonitorRunTask != null)
                    await MonitorRunTask;

                MonitorCancelTokenSource = null;
                MonitorRunTask = null;
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public RemoteLogMonitor()
        {

        }


        // Monitor task
        CancellationTokenSource MonitorCancelTokenSource { get; set; }
        Task MonitorRunTask { get; set; }


        /// <summary>
        /// Monitor run function
        /// </summary>
        protected async Task RunMonitor(CancellationToken cancelToken)
        {
            try
            {
                using (var udpClient = new UdpClient(HatConnection.LogPort))
                {
                    try
                    {
                        udpClient.JoinMulticastGroup(IPAddress.Parse(HatConnection.MulticastGroupAddress));
                        cancelToken.Register(() => udpClient.Close());

                        while (!cancelToken.IsCancellationRequested)
                        {
                            try
                            {
                                //  wait for the next read, and then split it into the command and the arguments
                                var parseReceived = Encoding.ASCII.GetString((await udpClient.ReceiveAsync()).Buffer).Split('?');

                                if (parseReceived.Count() == 2)
                                {
                                    //  see if this is recognized command
                                    switch (parseReceived[0])
                                    {
                                        case "log":
                                            //  parse the arguments
                                            var responseArgs = HttpUtility.ParseQueryString(parseReceived[1]);
                                            var logString = responseArgs.Get("log");
                                            var hostName = responseArgs.Get("sender");
                                            if (logString != null)
                                            {
                                                var log = JsonConvert.DeserializeObject<RemoteLogEventArgs>(logString);
                                                log.HostName = hostName;
                                                RemoteLogReceived?.Invoke(this, log);
                                            }
                                            break;

                                        default:
                                            Log?.Invoke(this, new LogEventArgs(this, "RunMonitor", $"Received invalid remote log: {parseReceived}.", LogLevel.WARN));
                                            break;
                                    }
                                }
                            }
                            catch (Exception exc)
                            {
                                Log?.Invoke(this, new LogEventArgs(this, "RunMonitor", exc, LogLevel.ERROR));
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    { }
                    catch (Exception ex)
                    {
                        Log?.Invoke(this, new LogEventArgs(this, "RunMonitor", ex, LogLevel.FATAL));
                    }
                }
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunMonitor", e, LogLevel.FATAL));
            }

        }
    }
}
