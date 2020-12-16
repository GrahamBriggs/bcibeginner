using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LoggingInterfaces;

namespace BrainHatClient
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
            await StopLoggingAsync();

            LogsQueue.RemoveAll();

            RunFunctionCancelTokenSource = new CancellationTokenSource();
            RunFunctionTask = RunLogging(RunFunctionCancelTokenSource.Token);
        }


        /// <summary>
        /// Stop the logging queue
        /// </summary>
        public async Task StopLoggingAsync()
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
        }


        //  Run function task
        protected CancellationTokenSource RunFunctionCancelTokenSource { get; set; }
        protected Task RunFunctionTask { get; set; }

        //  Queue
        protected SemaphoreSlim NotifyAddedLog { get; set; }
        protected ConcurrentQueue<LogEventArgs> LogsQueue { get; set; }

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
                while (!cancelToken.IsCancellationRequested)
                {
                    await NotifyAddedLog.WaitAsync(cancelToken);

                    List<LogEventArgs> allEvents = new List<LogEventArgs>();
                    while (!LogsQueue.IsEmpty)
                    {
                        if (LogsQueue.TryDequeue(out var nextLog))
                        {
                            allEvents.AddRange(GenerateLogsForLogEvent(nextLog));
                        }
                    }

                    LoggedEvents?.Invoke(this, allEvents);

                    LogToLog4(allEvents);
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in logging: {e}.");
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


        /// <summary>
        /// Logging using Log4 framework to generate log files
        /// </summary>
        private static readonly log4net.ILog logSystem = log4net.LogManager.GetLogger("SystemLogger");

        /// <summary>
        /// Log to the Log4 Framework
        /// </summary>
        private void LogToLog4(IEnumerable<LogEventArgs> logs)
        {
            if (LogToFile)
            {
                foreach (var log in logs)
                {
                    switch (log.Level)
                    {
                        case LogLevel.VERBOSE:
                            if (LogLevelDisplay == LogLevel.VERBOSE)
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
