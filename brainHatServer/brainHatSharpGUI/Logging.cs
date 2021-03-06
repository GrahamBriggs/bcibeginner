﻿using System;
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

        public bool LogToFile { get; set; }
        /// <summary>
        /// Start the logging queue
        /// </summary>
        public async Task StartLogging()
        {
            await StopLogging();

            LogToFile = false;

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
        CancellationTokenSource RunFunctionCancelTokenSource { get; set; }
        Task RunFunctionTask { get; set; }

        //  Queue
        SemaphoreSlim NotifyAddedLog { get; set; }
        ConcurrentQueue<LogEventArgs> LogsQueue { get; set; }

        //  Buffer
        public ConcurrentQueue<LogEventArgs> LogBuffer { get; protected set; }


        /// <summary>
        /// Handler for component logging
        /// </summary>
        void OnLog(object sender, LogEventArgs e)
        {
            AddLog(e);
        }


        /// <summary>
        /// Handler for remote log monitor logging
        /// </summary>
        void OnRemoteLogReceived(object sender, RemoteLogEventArgs e)
        {
            AddLog(e);
        }


        /// <summary>
        /// Logging queue processing run function
        /// </summary>
        async Task RunLogging(CancellationToken cancelToken)
        {
            try
            {
                try
                {
                    while (!cancelToken.IsCancellationRequested)
                    {
                        await NotifyAddedLog.WaitAsync(cancelToken);

                        ProcessLogs();
                    }
                }
                catch (OperationCanceledException)
                { }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception in logging: {e}.");
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in logging: {e}.");
            }

        }


        /// <summary>
        /// Process the logging queue
        /// </summary>
        void ProcessLogs()
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

                LogToLog4(allEvents);

                //  broadcast to listeners
                foreach (var nextLog in allEvents)
                {
                    if (nextLog.Level >= LogLevelDisplay)
                    {
                        var test = new RemoteLogEventArgs(nextLog);
                        var test2 = test.Sender.ToString();
                        var sendBytes = Encoding.UTF8.GetBytes($"log?sender={NetworkUtilities.GetHostName()}&log={JsonConvert.SerializeObject(new RemoteLogEventArgs(nextLog))}\n");
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
        IEnumerable<LogEventArgs> GenerateLogsForLogEvent(LogEventArgs log)
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

        /// <summary>
        /// Logging using Log4 framework to generate log files
        /// </summary>
        static readonly log4net.ILog logSystem = log4net.LogManager.GetLogger("SystemLogger");

        /// <summary>
        /// Log to the Log4 Framework
        /// </summary>
        void LogToLog4(IEnumerable<LogEventArgs> logs)
        {
            if (LogToFile)
            {
                foreach (var log in logs)
                {
                    switch (log.Level)
                    {
                        case LogLevel.VERBOSE:
                                logSystem.Debug(log.FormatLogForFile());
                            break;
                        case LogLevel.TRACE:
                                logSystem.Debug(log.FormatLogForFile());
                            break;
                        case LogLevel.DEBUG:
                            logSystem.Debug(log.FormatLogForFile());
                            break;

                        case LogLevel.INFO:
                            logSystem.Info(log.FormatLogForFile());
                            break;

                        case LogLevel.WARN:
                            logSystem.Warn(log.FormatLogForFile());
                            break;

                        case LogLevel.ERROR:
                            logSystem.Error(log.FormatLogForFile());
                            break;

                        default:
                        case LogLevel.FATAL:
                            logSystem.Fatal(log.FormatLogForFile());
                            break;
                    }
                }
            }
        }


    }
}
