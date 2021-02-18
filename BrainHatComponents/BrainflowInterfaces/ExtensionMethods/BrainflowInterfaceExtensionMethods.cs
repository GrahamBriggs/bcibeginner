using System;
using System.Collections.Generic;
using System.Linq;

namespace BrainflowInterfaces
{
    public static class BFCollectionMethods
    {
        public static int SampleIndexDifference(this double value, int lastIndex)
        {
            var index = (int)value;

            if ( index > lastIndex )
            {
                return index - lastIndex;
            }    
            else if ( index < lastIndex  )
            {
                return index + (256 - lastIndex);
            }
            else 
            {
                return 0;
            }
        }

        public static int SampleIndexDecrement(this double value, int boardId)
        {
            var index = (int)value;

            switch ( boardId )
            {
                case 0: //  Cyton
                    if (index == 0)
                        return 255;
                    else
                        return index - 1;

                case 2: //  Cyton+Daisy
                    if (index == 0)
                        return 254;
                    else
                        return index - 2;

                default:
                    return 0;       //  TODO ganglion
            }
        }


        public static double TimeBetweenSamples(this double value, int lastSampleIndex, int boardId, int sampleRate)
        {
            switch ( boardId )
            {
                case 0: //  Cyton 0-255 samples per second in increments of 1
                    return ((1.0 / sampleRate) * value.SampleIndexDifference(lastSampleIndex));
                case 2: //  Cyton+Daisy 0-254 samples per second in increments of 2
                    return ((1.0 / sampleRate) * (value.SampleIndexDifference(lastSampleIndex)/2.0));
                default:
                    return 0;        //  TODO ganglion
            }
        }

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

