using System;
using System.Collections.Generic;
using System.Text;

namespace BrainflowDataProcessing
{
    public static class BFDataProcessingExtensionMethods
    {
        public static string BandPowerKey(this double value)
        {
            return $"{value:N1}";
        }
    }
}
