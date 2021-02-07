using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrainflowInterfaces;
using BrainHatNetwork;
using LoggingInterfaces;
using Newtonsoft.Json;

namespace brainHatSharpGUI
{
    /// <summary>
    /// Logging Handler Class
    /// Implements a queue so when program object logs, it happens instantly
    /// and queue will be processed to update UI and log to file on the main thread
    /// </summary>
    public class Logging
    {
        //  Log event
        public event LogEventsDelegate LoggedEvents;

        //  Set the desired log level
        public LogLevel LogLevelDisplay { get; set; }

        /// <summary>
        /// Start the logging queue
        /// </summary>
        public async Task StartLogging()
        {
            await StopLogging();

            LogsQueue.RemoveAll();

            RunFunctionCancelTokenSource = new CancellationTokenSource();
            RunFunctionTask = RunLogging(RunFunctionCancelTokenSource.Token);
        }


        /// <summary>
        /// Stop the logging queue
        /// </summary>
        public async Task StopLogging()
        {
            if (RunFunctionCancelTokenSource != null)
            {
                RunFunctionCancelTokenSource.Cancel();
                await RunFunctionTask;

                RunFunctionCancelTokenSource = null;
                RunFunctionTask = null;
            }
        }


        /// <summary>
        /// Add a log to the logging queue
        /// </summary>
        public void AddLog(object sender, LogEventArgs log)
        {
            AddLog(log);
        }

        /// <summary>
        /// Add a log to the logging queue
        /// </summary>
        public void AddLog(LogEventArgs log)
        {
            LogsQueue.Enqueue(log);
            NotifyAddedLog.Release();
        }


        ///// <summary>
        ///// Add a log to the logging queue
        ///// </summary>
        public void AddLog(RemoteLogEventArgs log)
        {
            AddLog(new LogEventArgs(log));
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public Logging()
        {
            NotifyAddedLog = new SemaphoreSlim(0);
            LogsQueue = new ConcurrentQueue<LogEventArgs>();
            LogBuffer = new ConcurrentQueue<LogEventArgs>();
        }


        //  Run function task
        protected CancellationTokenSource RunFunctionCancelTokenSource { get; set; }
        protected Task RunFunctionTask { get; set; }

        //  Queue
        protected SemaphoreSlim NotifyAddedLog { get; set; }
        protected ConcurrentQueue<LogEventArgs> LogsQueue { get; set; }

        //  Buffer
        public ConcurrentQueue<LogEventArgs> LogBuffer { get; protected set; }


        /// <summary>
        /// Handler for component logging
        /// </summary>
        private void OnLog(object sender, LogEventArgs e)
        {
            AddLog(e);
        }


        /// <summary>
        /// Handler for remote log monitor logging
        /// </summary>
        private void OnRemoteLogReceived(object sender, RemoteLogEventArgs e)
        {
            AddLog(e);
        }


        /// <summary>
        /// Logging queue processing run function
        /// </summary>
        private async Task RunLogging(CancellationToken cancelToken)
        {
            try
            {
                using (var udpServer = new UdpClient())
                {
                    cancelToken.Register(() => { try { udpServer.Close(); } catch { } });

                    IPEndPoint localpt = new IPEndPoint(IPAddress.Any, BrainHatNetworkAddresses.LogPort);
                    udpServer.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    //udpServer.Client.Bind(localpt);

                    try
                    {
                        while (!cancelToken.IsCancellationRequested)
                        {
                            await NotifyAddedLog.WaitAsync(cancelToken);

                            await ProcessLogs(udpServer);
                        }
                    }
                    catch (OperationCanceledException)
                    { }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine($"Exception in logging: {e}.");
                    }
                }
            }
            catch (Exception e)
            {

                throw;
            }

        }


        /// <summary>
        /// Process the logging queue
        /// </summary>
        private async Task ProcessLogs(UdpClient udpClient)
        {
            try
            {
                //  empty the queue
                List<LogEventArgs> allEvents = new List<LogEventArgs>();
                while (!LogsQueue.IsEmpty)
                {
                    if (LogsQueue.TryDequeue(out var nextLog))
                    {
                        allEvents.AddRange(GenerateLogsForLogEvent(nextLog));
                    }
                }

                //  send event
                LoggedEvents?.Invoke(this, allEvents);

                //  broadcast to listeners
                foreach (var nextLog in allEvents)
                {
                    if (nextLog.Level >= LogLevelDisplay)
                    {
                        var test = new RemoteLogEventArgs(nextLog);
                        var test2 = test.Sender.ToString();
                        var sendBytes = Encoding.UTF8.GetBytes($"log?sender={NetworkUtilities.GetHostName()}&log={JsonConvert.SerializeObject(new RemoteLogEventArgs(nextLog))}\n");
                        await udpClient.SendAsync(sendBytes, sendBytes.Length, BrainHatNetworkAddresses.MulticastGroupAddress, BrainHatNetworkAddresses.LogPort);
                    }

                    LogBuffer.Enqueue(nextLog);
                }

                while (LogBuffer.Count > 333)
                    LogBuffer.TryDequeue(out var discard);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in logging {e}");
            }
        }


        /// <summary>
        /// Generate a collection of logs for this one log
        /// will return a collection of logs with this log as first item
        /// and if the data is an excetion, will generate additional logs for the inner exceptions
        /// </summary>
        private IEnumerable<LogEventArgs> GenerateLogsForLogEvent(LogEventArgs log)
        {
            //  put this log in a list
            List<LogEventArgs> logsList = new List<LogEventArgs>() { log };

            //  also add inner exceptions if this log happens to be an exception
            if (log.Data != null)
            {
                var logException = log.Data as Exception;
                if (logException != null)
                {
                    var inex = logException.InnerException;
                    while (inex != null)
                    {
                        LogEventArgs insertArg = new LogEventArgs(log.Sender, log.Function, inex, log.Level);
                        logsList.Add(insertArg);
                        inex = inex.InnerException;
                    }
                }
            }

            return logsList;
        }


     
    }
}
