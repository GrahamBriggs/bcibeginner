using brainflow;
using BrainflowInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{
    public class BandPowerCalculator
    {
        public static IEnumerable<double> CalculateBandPower(IEnumerable<IBFSample> data, int sampleRate, int channel, IEnumerable<Tuple<double,double>> freqRanges)
        {
            try
            {
                var results = new List<double>();

                int nfft = DataFilter.get_nearest_power_of_two(sampleRate);

                if (data.Count() <= nfft)
                    return results;

                double[] detrend = DataFilter.detrend(data.GetExgDataForChannel(channel), (int)DetrendOperations.LINEAR);

                Tuple<double[], double[]> psd = DataFilter.get_psd_welch(detrend, nfft, nfft / 2, sampleRate, (int)WindowFunctions.HANNING);

                foreach (var nextRange in freqRanges)
                {
                    results.Add(DataFilter.get_band_power(psd, nextRange.Item1, nextRange.Item2));
                }

                return results;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
