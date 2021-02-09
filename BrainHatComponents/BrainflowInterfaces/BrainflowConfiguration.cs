using System;
using System.Collections.Generic;
using System.Text;

namespace BrainflowInterfaces
{
    public enum TestSignalMode
    {
        InternalGround,
        Signal1Slow,
        Signal1Fast,
        DcSignal,
        Signal2Slow,
        Signal2Fast,
    }

    public static class BrainflowConfigurationExtensionMethods
    {
        public static string TestModeCommand(this TestSignalMode value)
        {
            switch (value)
            {
                case TestSignalMode.InternalGround:
                    return "0";
                case TestSignalMode.Signal1Slow:
                    return "-";
                case TestSignalMode.Signal1Fast:
                    return "=";
                case TestSignalMode.DcSignal:
                    return "p";
                case TestSignalMode.Signal2Slow:
                    return "[";
                case TestSignalMode.Signal2Fast:
                    return "]";
                default:
                    return "";
            }
        }
    }
}
