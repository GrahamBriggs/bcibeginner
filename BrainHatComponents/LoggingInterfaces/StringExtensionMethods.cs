using System;
using System.Collections.Generic;
using System.Text;

namespace LoggingInterfaces
{
    public static class StringExtensionMethods
    {
        public static string Left(this string value, int count)
        {
            return value.Substring(0, Math.Min(count, value.Length));
        }

        public static string Right(this string value, int count)
        {
            if (value.Length <= count)
                return value;
            return value.Substring(value.Length - count, count);
        }
    }
}
