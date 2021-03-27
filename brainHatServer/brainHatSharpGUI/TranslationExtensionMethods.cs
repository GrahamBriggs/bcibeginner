using BrainflowInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace brainHatSharpGUI
{
    public static class TranslationExtensionMethods
    {
        public static string Translate(this SrbSet value)
        {
            switch (value)
            {
                case SrbSet.Unknown:
                    return Properties.Resources.Unknown;
                case SrbSet.Connected:
                    return Properties.Resources.Connected;
                case SrbSet.Disconnected:
                    return Properties.Resources.Disconnected;
            }
            return "";
        }

        public static string Translate(this bool value)
        {
            if (value)
                return Properties.Resources.True;
            else
                return Properties.Resources.False;
        }

        public static string Translate(this AdsChannelInputType value)
        {
            switch ( value )
            {
                case AdsChannelInputType.Normal:
                    return Properties.Resources.Normal;
                case AdsChannelInputType.Shorted:
                    return Properties.Resources.Shorted;
                case AdsChannelInputType.BiasMeas:
                    return Properties.Resources.BiasMeas;
                case AdsChannelInputType.Mvdd:
                    return Properties.Resources.Mvdd;
                case AdsChannelInputType.Temp:
                    return Properties.Resources.Temporary;
                case AdsChannelInputType.Testsig:
                    return Properties.Resources.TestSig;
                case AdsChannelInputType.BiasDrp:
                    return Properties.Resources.BiasDrp;
                case AdsChannelInputType.BiasDrn:
                    return Properties.Resources.BiasDrn;
                default:
                    return "";
            }
        }

    }
}
