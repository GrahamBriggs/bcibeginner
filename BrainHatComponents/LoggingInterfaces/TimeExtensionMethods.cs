using System;
using System.Collections.Generic;
using System.Text;

namespace LoggingInterfaces
{
    public static class TimeExtensionMethods
    {
        public static string FormatTimeHoursHHmmssfff(this DateTimeOffset? value)
        {
            if (value.HasValue)
            {
                return value.Value.ToLocalTime().ToString("HH:mm:ss.fff");
            }

            return "---";

        }
    }
}
