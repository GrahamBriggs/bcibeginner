using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BrainflowInterfaces
{
    public class CustomArray<T>
    {
        public T[] GetColumn(T[,] matrix, int columnNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                    .Select(x => matrix[x, columnNumber])
                    .ToArray();
        }

        public T[] GetRow(T[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }
    }

    /// <summary>
    /// Data structure for Brainflow sample for Cyton16 board
    /// mimicing the property names from OpenBCI_GUI csv file
    /// </summary>
    public class BFCyton16Sample : IBFSample
    {
        //  Properties - based on OpenBCI_GUI text file recording for this sensor
        public double SampleIndex { get; set; }
        public double ExgCh0 { get; set; }
        public double ExgCh1 { get; set; }
        public double ExgCh2 { get; set; }
        public double ExgCh3 { get; set; }
        public double ExgCh4 { get; set; }
        public double ExgCh5 { get; set; }
        public double ExgCh6 { get; set; }
        public double ExgCh7 { get; set; }
        public double ExgCh8 { get; set; }
        public double ExgCh9 { get; set; }
        public double ExgCh10 { get; set; }
        public double ExgCh11 { get; set; }
        public double ExgCh12 { get; set; }
        public double ExgCh13 { get; set; }
        public double ExgCh14 { get; set; }
        public double ExgCh15 { get; set; }
        public double AcelCh0 { get; set; }
        public double AcelCh1 { get; set; }
        public double AcelCh2 { get; set; }
        public double Other0 { get; set; }
        public double Other1 { get; set; }
        public double Other2 { get; set; }
        public double Other3 { get; set; }
        public double Other4 { get; set; }
        public double Other5 { get; set; }
        public double Other6 { get; set; }
        public double AngCh0 { get; set; }
        public double AngCh1 { get; set; }
        public double AngCh2 { get; set; }
        public double TimeStamp { get; set; }

        public int SampleSize => 31;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BFCyton16Sample()
        {

        }

        
        /// <summary>
        /// Copy constructor
        /// </summary>
        public BFCyton16Sample(BFCyton16Sample sample)
        {
            SampleIndex = sample.SampleIndex;
            ExgCh0 = sample.ExgCh0;
            ExgCh1 = sample.ExgCh1;
            ExgCh2 = sample.ExgCh2;
            ExgCh3 = sample.ExgCh3;
            ExgCh4 = sample.ExgCh4;
            ExgCh5 = sample.ExgCh5;
            ExgCh6 = sample.ExgCh6;
            ExgCh7 = sample.ExgCh7;
            ExgCh8 = sample.ExgCh8;
            ExgCh9 = sample.ExgCh9;
            ExgCh10 = sample.ExgCh10;
            ExgCh11 = sample.ExgCh11;
            ExgCh12 = sample.ExgCh12;
            ExgCh13 = sample.ExgCh13;
            ExgCh14 = sample.ExgCh14;
            ExgCh15 = sample.ExgCh15;
            AcelCh0 = sample.AcelCh0;
            AcelCh1 = sample.AcelCh1;
            AcelCh2 = sample.AcelCh2;
            Other0 = sample.Other0;
            Other1 = sample.Other1;
            Other2 = sample.Other2;
            Other3 = sample.Other3;
            Other4 = sample.Other4;
            Other5 = sample.Other5;
            Other6 = sample.Other6;
            AngCh0 = sample.AngCh0;
            AngCh1 = sample.AngCh1;
            AngCh2 = sample.AngCh2;
            TimeStamp = sample.TimeStamp;
        }


        /// <summary>
        /// Construct from a column of a raw data chunk (brainflow paradigm)
        /// </summary>
        public BFCyton16Sample(double[,] chunk, int column)
        {
            var sample = chunk.GetColumn(column);

            InitializeFromSample(sample);
        }

        /// <summary>
        /// Construct from a row of raw data chunk (LSL paradigm)
        /// </summary>
        public static BFCyton16Sample FromChunkRow(double[,] chunk, int row)
        {
            return new BFCyton16Sample(chunk.GetRow(row));
        }


        /// <summary>
        /// Construct from a raw data sample
        /// </summary>
        public BFCyton16Sample(double[] sample)
        {
            InitializeFromSample(sample);
        }


        /// <summary>
        /// Set properties from a raw sample
        /// </summary>
        private void InitializeFromSample(double[] sample)
        {
            int index = 0;
            SampleIndex = sample[index++];
            ExgCh0 = sample[index++];
            ExgCh1 = sample[index++];
            ExgCh2 = sample[index++];
            ExgCh3 = sample[index++];
            ExgCh4 = sample[index++];
            ExgCh5 = sample[index++];
            ExgCh6 = sample[index++];
            ExgCh7 = sample[index++];
            ExgCh8 = sample[index++];
            ExgCh9 = sample[index++];
            ExgCh10 = sample[index++];
            ExgCh11 = sample[index++];
            ExgCh12 = sample[index++];
            ExgCh13 = sample[index++];
            ExgCh14 = sample[index++];
            ExgCh15 = sample[index++];
            AcelCh0 = sample[index++];
            AcelCh1 = sample[index++];
            AcelCh2 = sample[index++];
            Other0 = sample[index++];
            Other1 = sample[index++];
            Other2 = sample[index++];
            Other3 = sample[index++];
            Other4 = sample[index++];
            Other5 = sample[index++];
            Other6 = sample[index++];
            AngCh0 = sample[index++];
            AngCh1 = sample[index++];
            AngCh2 = sample[index++];
            TimeStamp = sample[index++];
        }


        /// <summary>
        /// Construct from a text string in OpenBCI_GUI format
        /// </summary>
        public BFCyton16Sample(string sample)
        {
            var fields = sample.Split(',');
            if ( fields.Length == 32 )
            {
                int index = 0;
                SampleIndex = double.Parse(fields[index++]);
                ExgCh0 = double.Parse(fields[index++]);
                ExgCh1 = double.Parse(fields[index++]);
                ExgCh2 = double.Parse(fields[index++]);
                ExgCh3 = double.Parse(fields[index++]);
                ExgCh4 = double.Parse(fields[index++]);
                ExgCh5 = double.Parse(fields[index++]);
                ExgCh6 = double.Parse(fields[index++]);
                ExgCh7 = double.Parse(fields[index++]);
                ExgCh8 = double.Parse(fields[index++]);
                ExgCh9 = double.Parse(fields[index++]);
                ExgCh10 = double.Parse(fields[index++]);
                ExgCh11 = double.Parse(fields[index++]);
                ExgCh12 = double.Parse(fields[index++]);
                ExgCh13 = double.Parse(fields[index++]);
                ExgCh14 = double.Parse(fields[index++]);
                ExgCh15 = double.Parse(fields[index++]);
                AcelCh0 = double.Parse(fields[index++]);
                AcelCh1 = double.Parse(fields[index++]);
                AcelCh2 = double.Parse(fields[index++]);
                Other0 = double.Parse(fields[index++]);
                Other1 = double.Parse(fields[index++]);
                Other2 = double.Parse(fields[index++]);
                Other3 = double.Parse(fields[index++]);
                Other4 = double.Parse(fields[index++]);
                Other5 = double.Parse(fields[index++]);
                Other6 = double.Parse(fields[index++]);
                AngCh0 = double.Parse(fields[index++]);
                AngCh1 = double.Parse(fields[index++]);
                AngCh2 = double.Parse(fields[index++]);
                TimeStamp = double.Parse(fields[index++]);
            }
        }


        /// <summary>
        /// Get TimeSamp as DateTime in local time
        /// </summary>
        public DateTime ObservationTime => DateTimeOffset.FromUnixTimeMilliseconds((long)(TimeStamp * 1000.0)).ToLocalTime().DateTime;
       


        /// <summary>
        /// Exg channel methods
        /// </summary>
        public int NumberExgChannels =>  16;

        /// <summary>
        /// Get Enumerable of EXG channel data
        /// </summary>
        public IEnumerable<double> ExgData
        {
            get
            {
                yield return ExgCh0;
                yield return ExgCh1;
                yield return ExgCh2;
                yield return ExgCh3;
                yield return ExgCh4;
                yield return ExgCh5;
                yield return ExgCh6;
                yield return ExgCh7;
                yield return ExgCh8;
                yield return ExgCh9;
                yield return ExgCh10;
                yield return ExgCh11;
                yield return ExgCh12;
                yield return ExgCh13;
                yield return ExgCh14;
                yield return ExgCh15;
            }
        }
        

        /// <summary>
        /// Get EXG channel data for specified channel
        /// </summary>
        public double GetExgDataForChannel(int channel)
        {
            switch (channel)
            {
                case 0:
                    return ExgCh0;
                case 1:
                    return ExgCh1;
                case 2:
                    return ExgCh2;
                case 3:
                    return ExgCh3;
                case 4:
                    return ExgCh4;
                case 5:
                    return ExgCh5;
                case 6:
                    return ExgCh6;
                case 7:
                    return ExgCh7;
                case 8:
                    return ExgCh8;
                case 9:
                    return ExgCh9;
                case 10:
                    return ExgCh10;
                case 11:
                    return ExgCh11;
                case 12:
                    return ExgCh12;
                case 13:
                    return ExgCh13;
                case 14:
                    return ExgCh14;
                case 15:
                    return ExgCh15;
                default:
                    return BrainflowConstants.MissingValue;
            }
        }

        
        /// <summary>
        /// Set EXG Channel data for specified channel
        /// </summary>
        public void SetExgDataForChannel(int channel, double data)
        {
            switch (channel)
            {
                case 0:
                    ExgCh0 = data;
                    break;
                case 1:
                    ExgCh1 = data;
                    break;
                case 2:
                    ExgCh2 = data;
                    break;
                case 3:
                    ExgCh3 = data;
                    break;
                case 4:
                    ExgCh4 = data;
                    break;
                case 5:
                    ExgCh5 = data;
                    break;
                case 6:
                    ExgCh6 = data;
                    break;
                case 7:
                    ExgCh7 = data;
                    break;
                case 8:
                    ExgCh8 = data;
                    break;
                case 9:
                    ExgCh9 = data;
                    break;
                case 10:
                    ExgCh10 = data;
                    break;
                case 11:
                    ExgCh11 = data;
                    break;
                case 12:
                    ExgCh12 = data;
                    break;
                case 13:
                    ExgCh13 = data;
                    break;
                case 14:
                    ExgCh14 = data;
                    break;
                case 15:
                    ExgCh15 = data;
                    break;
            }
        }


        /// <summary>
        /// Accelerometer channel methods
        /// </summary>
        public int NumberAccelChannels => 3;


        /// <summary>
        /// Get Enumerable of accelerometer channel data
        /// </summary>
        public IEnumerable<double> AccelData
        {
            get
            {
                yield return AcelCh0;
                yield return AcelCh1;
                yield return AcelCh2;
            }
        }


        /// <summary>
        /// Get accelerometer data for specified channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public double GetAccelDataForChannel(int channel)
        {
            switch (channel)
            {
                case 0:
                    return AcelCh0;
                case 1:
                    return AcelCh1;
                case 2:
                    return AcelCh2;

                default:
                    return BrainflowConstants.MissingValue;
            }
        }


        /// <summary>
        /// Other channel methods
        /// </summary>
        public int NumberOtherChannels => 7;


        /// <summary>
        /// Get Enumerable of other channel data
        /// </summary>
        public IEnumerable<double> OtherData
        {
            get
            {
                yield return Other0;
                yield return Other1;
                yield return Other2;
                yield return Other3;
                yield return Other4;
                yield return Other5;
                yield return Other6;
            }
        }

        /// <summary>
        /// Get other data for specified channel
        /// </summary>
        public double GetOtherDataForChannel(int channel)
        {
            switch (channel)
            {
                case 0:
                    return Other0;
                case 1:
                    return Other1;
                case 2:
                    return Other2;
                case 3:
                    return Other3;
                case 4:
                    return Other4;
                case 5:
                    return Other5;
                case 6:
                    return Other6;
                default:
                    return BrainflowConstants.MissingValue;
            }
        }


        public int NumberAnalogChannels => 3;

        /// <summary>
        /// Get Enumerable of accelerometer channel data
        /// </summary>
        public IEnumerable<double> AnalogData
        {
            get
            {
                yield return AngCh0;
                yield return AngCh1;
                yield return AngCh2;
            }
        }


        /// <summary>
        /// Get accelerometer data for specified channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public double GetAnalogDataForChannel(int channel)
        {
            switch (channel)
            {
                case 0:
                    return AngCh0;
                case 1:
                    return AngCh1;
                case 2:
                    return AngCh2;
                default:
                    return BrainflowConstants.MissingValue;
            }
        }

        public double[] AsRawSample()
        {
            var sample = new double[SampleSize];

            sample[0] = SampleIndex;
            sample[1] = ExgCh0;
            sample[2] = ExgCh1;
            sample[3] = ExgCh2;
            sample[4] = ExgCh3;
            sample[5] = ExgCh4;
            sample[6] = ExgCh5;
            sample[7] = ExgCh6;
            sample[8] = ExgCh7;
            sample[9] = ExgCh8;
            sample[10] = ExgCh9;
            sample[11] = ExgCh10;
            sample[12] = ExgCh11;
            sample[13] = ExgCh12;
            sample[14] = ExgCh13;
            sample[15] = ExgCh14;
            sample[16] = ExgCh15;
            sample[17] = AcelCh0;
            sample[18] = AcelCh1;
            sample[19] = AcelCh2;
            sample[20] = Other0;
            sample[21] = Other1;
            sample[22] = Other2;
            sample[23] = Other3;
            sample[24] = Other4;
            sample[25] = Other5;
            sample[26] = Other6;
            sample[27] = AngCh0;
            sample[28] = AngCh1;
            sample[29] = AngCh2;
            sample[30] = TimeStamp;
            
            return sample;

        }
    }
}
    

