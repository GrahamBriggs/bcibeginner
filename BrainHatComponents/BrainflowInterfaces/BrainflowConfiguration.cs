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

    public enum ChannelGain
    {
        x1,
        x2,
        x4,
        x6,
        x8,
        x12,
        x24,
    }

    public enum AdsChannelInputType
    {
        Normal,
        Shorted,
        BiasMeas,
        Mvdd,
        Temp,
        Testsig,
        BiasDrp,
        BiasDrn,
    }


    public static class BrainflowConfigurationExtensionMethods
    {
        public static string TestModeCharacter(this TestSignalMode value)
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

        public static string ChannelSetCharacter(this int value)
        {
            switch ( value )
            {
                case 1:
                    return "1";
                case 2:
                    return "2";
                case 3:
                    return "3";
                case 4:
                    return "4";
                case 5:
                    return "5";
                case 6:
                    return "6";
                case 7:
                    return "7";
                case 8:
                    return "8";
                case 9:
                    return "Q";
                case 10:
                    return "W";
                case 11:
                    return "E";
                case 12:
                    return "R";
                case 13:
                    return "T";
                case 14:
                    return "Y";
                case 15:
                    return "U";
                case 16:
                    return "I";
                default:
                    return "";
            }
        }

        public static string BoolCharacter(this bool value)
        {
            return value ? "1" : "0";
        }

        public static string ChannelSettingsToString(this ICytonChannelSettings value)
        {
            return $"PwrDwn{value.PowerDown},Gain {value.Gain}, Input {value.InputType}, Bias {value.Bias}, SRB2 {value.Srb2}";
        }
    }


}
