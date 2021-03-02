using LoggingInterfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace brainHatSharpGUI
{
    /// <summary>
    /// Logging object extension methods
    /// </summary>
    public static class LogDisplayExtensionMethods
    {
        /// <summary>
        /// Log Colour
        /// </summary>
        public static Color LogColour(this LogLevel level)
        {
            switch (level)
            {
                case LogLevel.VERBOSE:
                    return Color.DarkCyan;

                case LogLevel.TRACE:
                    return Color.Cyan;

                case LogLevel.DEBUG:
                    return Color.LightGreen;

                case LogLevel.INFO:
                    return Color.White;

                case LogLevel.WARN:
                    return Color.Red;

                case LogLevel.ERROR:
                    return Color.White;

                case LogLevel.FATAL:
                    return Color.Yellow;

                default:
                    return Color.White;

            }
        }

        /// <summary>
        /// Log Colour
        /// </summary>
        public static Color BackgrondColour(this LogLevel level, bool remote)
        {
            if (remote)
            {
                switch (level)
                {
                    case LogLevel.VERBOSE:
                        return Color.FromArgb(50, 50, 50);

                    case LogLevel.TRACE:
                        return Color.FromArgb(50, 50, 50);

                    case LogLevel.DEBUG:
                        return Color.FromArgb(50, 50, 50);

                    case LogLevel.INFO:
                        return Color.FromArgb(50, 50, 50);

                    case LogLevel.WARN:
                        return Color.FromArgb(50, 50, 50);

                    case LogLevel.ERROR:
                        return Color.IndianRed;

                    case LogLevel.FATAL:
                        return Color.IndianRed;

                    default:
                        return Color.White;

                }
            }
            else
            {
                switch (level)
                {
                    case LogLevel.VERBOSE:
                        return Color.Black;

                    case LogLevel.TRACE:
                        return Color.Black;

                    case LogLevel.DEBUG:
                        return Color.Black;

                    case LogLevel.INFO:
                        return Color.Black;

                    case LogLevel.WARN:
                        return Color.Black;

                    case LogLevel.ERROR:
                        return Color.Red;

                    case LogLevel.FATAL:
                        return Color.Red;

                    default:
                        return Color.White;

                }
            }
        }


    }
}
