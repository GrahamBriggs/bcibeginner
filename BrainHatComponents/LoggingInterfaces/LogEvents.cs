using System;
using System.Collections.Generic;
using System.Text;

namespace LoggingInterfaces
{

    /// <summary>
    /// Define LogLevel to match WiringPiExtension.h log levels
    /// </summary>
    public enum LogLevel
    {
        All = 0,
        VERBOSE,
        TRACE,
        DEBUG,
        INFO,
        WARN,
        ERROR,
        FATAL,
        OFF,
    };


    /// <summary>
    /// Remote Logging event args
    /// </summary>
    public class RemoteLogEventArgs : EventArgs
    {
        public RemoteLogEventArgs()
        {
            Thread = -1;
            Sender = "";
            Level = LogLevel.All;
            Function = "";
            Time = 0;
            Data = null;
        }

		public RemoteLogEventArgs(LogEventArgs e)
        {
			Thread = e.Thread;         
			Sender = e.Sender.ToString();
			Level = e.Level;
			Function = e.Function;

			Time = e.Time.HasValue ?  e.Time.Value.ToUnixTimeMilliseconds() : 0;
			Data = e.Data.ToString();
        }

        public string HostName { get; set; }
        public int Thread { get; set; }
        public object Sender { get; set; }
        public LogLevel Level { get; set; }
        public string Function { get; set; }
        public long Time { get; set; }
        public object Data { get; set; }
    }
    //
    public delegate void RemoteLogEventDelegate(object sender, RemoteLogEventArgs e);


    /// <summary>
    /// Logging event args
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        public LogEventArgs()
        {

        }


        public LogEventArgs(object sender, string functionName, object data, LogLevel level)
        {
            Time = DateTimeOffset.UtcNow;
            Level = level;
            Thread = System.Threading.Thread.CurrentThread.ManagedThreadId;

            Sender = sender;
            Function = functionName;
            Data = data;

            Remote = false;
        }

        public LogEventArgs(long unixTimeMilliseconds, int thread, object sender, LogLevel level, string functionName, object data)
        {
            Time = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMilliseconds);
            Thread = thread;

            Sender = sender;
            Level = level;
            Function = functionName;
            Data = data;

            Remote = false;
        }

        public LogEventArgs(RemoteLogEventArgs log)
        {
            Time = DateTimeOffset.FromUnixTimeMilliseconds(log.Time);
            Thread = log.Thread;
            Sender = log.Sender;
            Level = log.Level;

            HostName = log.HostName;
            Sender = log.Sender;
            Function = log.Function;
            Data = log.Data;

            Remote = true;
        }

        public string HostName { get; set; }
        public DateTimeOffset? Time { get; protected set; }
        public int Thread { get; protected set; }
        public LogLevel Level { get; protected set; }

        public object Sender { get; protected set; }
        public string Function { get; protected set; }
        public object Data { get; protected set; }

        public bool Remote { get; protected set; }
    }
    //
    public delegate void LogEventDelegate(object sender, LogEventArgs e);
    public delegate void LogEventsDelegate(object sender, IEnumerable<LogEventArgs> e);
}
