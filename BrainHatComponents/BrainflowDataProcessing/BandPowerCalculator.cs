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
        /// <summary>
        /// Create a band power range list for 8,10,12 and 18,20,22 
        /// </summary>
        public static List<Tuple<double, double>> CreateSampleBandPowerRangeList()
        {
            //  create a list of tuples for your band power ranges
            var rangeList = new List<Tuple<double, double>>() {
                    new Tuple<double, double>(7.0,9.0),
                    new Tuple<double, double>(9.0,11.0),
                    new Tuple<double, double>(11.0,13.0),

                    new Tuple<double, double>(17.0,19.0),
                    new Tuple<double, double>(19.0,21.0),
                    new Tuple<double, double>(21.0,23.0),
                };

            return rangeList;
        }


        /// <summary>
        /// Create a band power range list for 1-60
        /// </summary>
        public static List<Tuple<double, double>> CreateFullBandPowerRangeList()
        {
            var rangeList = new List<Tuple<double, double>>();
            for (int i = 1; i <= 60; i++)
            {
                rangeList.Add(new Tuple<double, double>(i - 1, i + 1));
            }

            return rangeList;
        }


        /// <summary>
        /// Calculate band power
        /// </summary>
        public static IEnumerable<double> CalculateBandPower(IEnumerable<IBFSample> data, int sampleRate, int channel, IEnumerable<Tuple<double,double>> freqRanges)
        {
            try
            {
                var results = new List<double>();

                int nfft = DataFilter.get_nearest_power_of_two(sampleRate);

                if (data.Count() <= nfft)
                    return results;

                Tuple<double[], double[]> psd = DataFilter.get_psd_welch(data.GetExgDataForChannel(channel), nfft, nfft / 2, sampleRate, (int)WindowFunctions.HANNING);

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



        /// <summary>
        /// Constructor
        /// </summary>
        public BandPowerCalculator(int boardId, int numChannels, int sampleRate)
        {
            BoardId = boardId;
            NumberOfChannels = numChannels;
            SampleRate = sampleRate;
           
            BandPowerCalcRangeList = CreateFullBandPowerRangeList();
           
        }

        public int BoardId { get; private set; }
        public int NumberOfChannels { get; private set; }
        public int SampleRate { get; private set; }


        //  Define bands and ranges in this list
        public List<Tuple<double, double>> BandPowerCalcRangeList { get; set; }

        public int NumberOfBands => BandPowerCalcRangeList.Count;
       
        

        /// <summary>
        /// Calculate band powers for the range list
        /// </summary>
        public IBFSample[] CalculateBandPowers(IEnumerable<IBFSample> samples)
        {
            var bandPowers = new IBFSample[BandPowerCalcRangeList.Count];
            for (int i = 0; i < BandPowerCalcRangeList.Count; i++)
            {
                bandPowers[i] = new BFSampleImplementation(BoardId);
            }

            for (int i = 0; i < NumberOfChannels; i++)
            {
                var bandPower = CalculateBandPower(samples, SampleRate, i, BandPowerCalcRangeList);

                int j = 0;
                foreach (var nextBandPower in bandPower)
                {
                    bandPowers[j++].SetExgDataForChannel(i, nextBandPower);
                }
            }

            return bandPowers;
        }


    }
}
