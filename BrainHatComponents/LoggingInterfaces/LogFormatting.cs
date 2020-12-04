﻿using System;
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
            return $"{value.Time.FormatTimeHoursHHmmssfff()},[{ value.Thread.ToString().Right(3) }],{value.Level},{value.Sender ?? "senderUnknown"},{value.Function ?? "functionUnknown"},{value.Data ?? "dataUnknown"}";
        }

        public static string FormatLogForConsole(this LogEventArgs value)
        {
            return $"{value.Time.FormatTimeHoursHHmmssfff()} [{string.Format("{0,3}", value.Thread.ToString().Right(3))}] {string.Format("{0,7}", value.Level)}   {string.Format("{0,-20}", value.Sender.ToString().Left(19) ?? "senderUnknown")} {string.Format("{0,-20}", value.Function.Left(19) ?? "functionUnknown")} {value.Data ?? "dataUnknown"}";
        }
        
    }
}