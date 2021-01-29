using System.Collections.Generic;
using System.Linq;

namespace BrainflowInterfaces
{
    public static class BFCollectionMethods
    {
        /// <summary>
        /// Get Exg channel data from enumerable of brainflow sample objects
        /// </summary>
        public static double[] GetExgDataForChannel(this IEnumerable<IBFSample> value, int channel)
        {
            return value.Select(x => x.GetExgDataForChannel(channel)).ToArray();
        }

        /// <summary>
        /// Get Exg channel data from enumerable of brainflow sample objects
        /// </summary>
        public static float[] GetFloatExgDataForChannel(this IEnumerable<IBFSample> value, int channel)
        {
            var doubleValues = value.Select(x => x.GetExgDataForChannel(channel)).ToArray();
            float[] floatValues = new float[doubleValues.Count()];
            for (int i = 0; i < doubleValues.Count(); i++)
                floatValues[i] = (float)doubleValues[i];
           
            return floatValues;
        }
    }
}
