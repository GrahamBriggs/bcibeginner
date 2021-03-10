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
        /// Get Accel channel data from enumerable of brainflow sample objects
        /// </summary>
        public static double[] GetAcelDataForChannel(this IEnumerable<IBFSample> value, int channel)
        {
            return value.Select(x => x.GetAccelDataForChannel(channel)).ToArray();
        }

        /// <summary>
        /// Get Other channel data from enumerable of brainflow sample objects
        /// </summary>
        public static double[] GetOtherDataForChannel(this IEnumerable<IBFSample> value, int channel)
        {
            return value.Select(x => x.GetOtherDataForChannel(channel)).ToArray();
        }

        /// <summary>
        /// Get Analog channel data from enumerable of brainflow sample objects
        /// </summary>
        public static double[] GetAnalogDataForChannel(this IEnumerable<IBFSample> value, int channel)
        {
            return value.Select(x => x.GetAnalogDataForChannel(channel)).ToArray();
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


        /// <summary>
        /// Get equipment type description string for boardId
        /// </summary>
        public static string GetEquipmentName(this int value)
        {
            switch (value)
            {
                case 0:
                    return "Cyton";
                case 2:
                    return "Cyton+Daisy";
            }
            return "Unknown";
        }

        /// <summary>
        /// Get equipment type description string for boardId
        /// </summary>
        public static string GetSampleName(this int value)
        {
            switch (value)
            {
                case 0:
                    return "Cyton8BFSample";
                case 2:
                    return "Cyton16BFSample";
                case 1:
                    return "GanglionBFSample";
                default:
                    return "";
            }
        }


        /// <summary>
        /// Get board ID from the string
        /// </summary>
        public static int GetBoardId(this string value)
        {
            switch (value)
            {
                case "Cyton8":
                case "Cyton8_BFSample":
                case "Cyton8BFSample":
                    return 0;
                case "Cyton16":
                case "Cyton16_BFSample":
                case "Cyton16BFSample":
                    return 2;
                case "Ganglion_BFSample":
                case "GanglionBFSample":
                    return 1;
                default:
                    return -99;
            }
        }

    }
}

