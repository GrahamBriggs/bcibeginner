using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowInterfaces
{
    public static class DateTimeExtensionMethods
    {
        public static double UnixTimeInSeconds(this DateTimeOffset? value)
        {
            if (!value.HasValue)
                return 0.0;

            return (double)value.Value.ToUnixTimeInDoubleSeconds();
        }

        public static double ToUnixTimeInDoubleSeconds(this DateTimeOffset value)
        {
            return (double)value.ToUnixTimeMilliseconds() / 1000.0;
        }
    }
}
