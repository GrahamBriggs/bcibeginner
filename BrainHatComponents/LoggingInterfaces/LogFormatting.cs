using System;
using System.Collections.Generic;
using System.Text;

namespace LoggingInterfaces
{
    /// <summary>
    /// Logging object extension methods
    /// </summary>
    public static class LoggingExtensionMethods
    {
        public static string FormatLogForFile(this LogEventArgs value)
        {
            return $"{value.Time.FormatTimeHoursHHmmssfff()},[{ value.Thread.ToString().Right(3) }],{value.Level},{(value.HostName ?? "program")},{value.Sender ?? "senderUnknown"},{value.Function ?? "functionUnknown"},{value.Data ?? "dataUnknown"}";
        }

        public static string FormatLogForConsole(this LogEventArgs value)
        {
            return $"{value.Time.FormatTimeHoursHHmmssfff()} [{string.Format("{0,3}", value.Thread.ToString().Right(3))}] {string.Format("{0,7}", value.Level)} {string.Format("{0,-40}", value?.Sender?.ToString().Left(39) ?? "senderUnknown")} {string.Format("{0,-30}", value?.Function?.Left(29) ?? "functionUnknown")} {value.Data ?? "dataUnknown"}";
        }
        
    }
}
