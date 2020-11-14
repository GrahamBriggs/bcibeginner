using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformHelper
{
    public enum Platform
    {
        Unknown,
        Linux,
        Win32,
        Win64,
    }

    public static class PlatformHelper
    {
        public static Platform GetPlatform()
        {
            if (_Platform == Platform.Unknown)
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                    case PlatformID.MacOSX:
                        _Platform = Platform.Linux;
                        break;

                    default:
                        {
                            if (System.Environment.Is64BitProcess)
                                _Platform = Platform.Win64;
                            else
                                _Platform = Platform.Win32;
                        }
                        break;
                }
            }

            return _Platform;
        }

        public static bool Windows
        {
            get
            {
                if (_Platform == Platform.Unknown)
                    GetPlatform();

                return _Platform == Platform.Win64 || _Platform == Platform.Win32;
            }
        }

        public static bool Linux
        {
            get
            {
                if (_Platform == Platform.Unknown)
                    GetPlatform();

                return _Platform == Platform.Linux;
            }
        }

        private static Platform _Platform = Platform.Unknown;
    }
}
