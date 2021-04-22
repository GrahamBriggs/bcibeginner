using System;
using System.Collections.Generic;
using System.Linq;


namespace BrainflowInterfaces
{
    public static class BFCollectionMethods
    {
        /// <summary>
        /// Calculate the difference in sample index between this sample and last sample
        /// accounts for roll over at 255
        /// </summary>
        public static int SampleIndexDifference(this double value, int lastIndex)
        {
            var index = (int)(value + 0.5);

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


        /// <summary>
        /// Calculate the difference in sample index between this sample and last sample
        /// accounts for roll over at 255
        /// </summary>
        public static int SampleIndexDifference(this double value, double lastIndex)
        {
            int lastIndexInt = (int)(lastIndex + 0.5);
            return value.SampleIndexDifference(lastIndexInt);
        }


        /// <summary>
        /// Decrement this sample index by one unit (depending on board type)
        /// </summary>
        public static int SampleIndexDecrement(this double value, int boardId)
        {
            var index = (int)(value+0.5);

            switch ( (BrainhatBoardIds)boardId )
            {
                case BrainhatBoardIds.CONTEC_KT88:
                case BrainhatBoardIds.CYTON_BOARD: //  Cyton
                    if (index == 0)
                        return 255;
                    else
                        return index - 1;

                case BrainhatBoardIds.CYTON_DAISY_BOARD: //  Cyton+Daisy
                    if (index == 0)
                        return 254;
                    else
                        return index - 2;

                default:
                    return 0;      
            }
        }


        /// <summary>
        /// Increment sample index by one unit (depending on board type)
        /// </summary>
        public static int SampleIndexIncrement(this double value, int boardId)
        {
            var index = (int)(value + 0.5);

            switch ((BrainhatBoardIds)boardId)
            {
                case BrainhatBoardIds.CONTEC_KT88:
                case BrainhatBoardIds.CYTON_BOARD: //  Cyton
                    if (index == 255)
                        return 0;
                    else
                        return index + 1;

                case BrainhatBoardIds.CYTON_DAISY_BOARD: //  Cyton+Daisy
                    if (index == 254)
                        return 0;
                    else
                        return index + 2;

                default:
                    return 0;     
            }
        }


        /// <summary>
        /// Calculate tthe time between samples, based on sample index (board type) and sample rate
        /// </summary>
        public static double TimeBetweenSamples(this double value, int lastSampleIndex, int boardId, int sampleRate)
        {
            switch ( (BrainhatBoardIds)boardId )
            {
                case BrainhatBoardIds.CONTEC_KT88:
                case BrainhatBoardIds.CYTON_BOARD: 
                    //  The default behavior is 0-255 sample index in increments of 1
                    return ((1.0 / sampleRate) * value.SampleIndexDifference(lastSampleIndex));

                case BrainhatBoardIds.CYTON_DAISY_BOARD: 
                    //  Cyton+Daisy 0-254 samples per second in increments of 2
                    return ((1.0 / sampleRate) * (value.SampleIndexDifference(lastSampleIndex)/2.0));
                default:
                    return 0;       
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


     

    }
}

