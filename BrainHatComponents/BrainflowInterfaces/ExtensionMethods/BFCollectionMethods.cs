using System.Collections.Generic;
using System.Linq;

namespace BrainflowInterfaces
{
    public static class BFCollectionMethods
    {
        /// <summary>
        /// Get Exg channel data from enumerable of brainflow data objects
        /// </summary>
        public static double[] GetExgDataForChannel(this IEnumerable<IBFSample> value, int channel)
        {
            return value.Select(x => x.GetExgDataForChannel(channel)).ToArray();
        }
    }
}
