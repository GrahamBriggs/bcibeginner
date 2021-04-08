using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowInterfaces
{
    /// <summary>
    /// Data structure for Brainflow sample from Cyton8 board
    /// mimicing the property names from OpenBCI_GUI csv file
    /// </summary>
    public class BFCyton8Sample : IBFSample
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

        public int SampleSize => 23;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BFCyton8Sample()
        {

        }




        /// <summary>
        /// Copy constructor
        /// </summary>
        public BFCyton8Sample(BFCyton8Sample sample)
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
        /// Construct from a column of a raw data chunk
        /// </summary>
        public BFCyton8Sample(double[,] chunk, int column)
        {
            var sample = chunk.GetColumn(column);

            InitializeFromSample(sample);
        }

        public static BFCyton8Sample FromChunkRow(double[,] chunk, int row)
        {
            return new BFCyton8Sample(chunk.GetRow(row));
        }

        public BFCyton8Sample(double[] sample)
        {
            InitializeFromSample(sample);
        }

        void InitializeFromSample(double[] data)
        {
            int index = 0;
            SampleIndex = data[index++];
            ExgCh0 = data[index++];
            ExgCh1 = data[index++];
            ExgCh2 = data[index++];
            ExgCh3 = data[index++];
            ExgCh4 = data[index++];
            ExgCh5 = data[index++];
            ExgCh6 = data[index++];
            ExgCh7 = data[index++];
            AcelCh0 = data[index++];
            AcelCh1 = data[index++];
            AcelCh2 = data[index++];
            Other0 = data[index++];
            Other1 = data[index++];
            Other2 = data[index++];
            Other3 = data[index++];
            Other4 = data[index++];
            Other5 = data[index++];
            Other6 = data[index++];
            AngCh0 = data[index++];
            AngCh1 = data[index++];
            AngCh2 = data[index++];
            TimeStamp = data[index++];
        }


        /// <summary>
        /// Construct from a text string in OpenBCI_GUI format
        /// </summary>
        public BFCyton8Sample(string fromTxt)
        {
            var fields = fromTxt.Split(',');
            if (fields.Length == 24)
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
        public int NumberExgChannels => 8;

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
        /// Set accel channel data
        /// </summary>
        public void SetAccelDataForChannel(int channel, double data)
        {
            switch (channel)
            {
                case 0:
                    AcelCh0 = data;
                    break;

                case 1:
                    AcelCh1 = data;
                    break;

                case 2:
                    AcelCh2 = data;
                    break;
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


        /// <summary>
        /// Set other data
        /// </summary>
        public void SetOtherDataForChannel(int channel, double data)
        {
            switch (channel)
            {
                case 0:
                    Other0 = data;
                    break;
                case 1:
                    Other1 = data;
                    break;
                case 2:
                    Other2 = data;
                    break;
                case 3:
                    Other3 = data;
                    break;
                case 4:
                    Other4 = data;
                    break;
                case 5:
                    Other5 = data;
                    break;
                case 6:
                    Other6 = data;
                    break;
            }
        }


        public int NumberAnalogChannels => 3;

        /// <summary>
        /// Get Enumerable of analog channel data
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
        /// Get analog data for specified channel
        /// </summary>
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


        /// <summary>
        /// Set analog data for specified channel
        /// </summary>
        public void SetAnalogDataForChannel(int channel, double data)
        {
            switch (channel)
            {
                case 0:
                    AngCh0 = data;
                    break;
                case 1:
                    AngCh1 = data;
                    break;
                case 2:
                    AngCh2 = data;
                    break;
            }
        }


        /// <summary>
        /// Return the sample as a double array
        /// </summary>
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
            sample[9] = AcelCh0;
            sample[10] = AcelCh1;
            sample[11] = AcelCh2;
            sample[12] = Other0;
            sample[13] = Other1;
            sample[14] = Other2;
            sample[15] = Other3;
            sample[16] = Other4;
            sample[17] = Other5;
            sample[18] = Other6;
            sample[19] = AngCh0;
            sample[20] = AngCh1;
            sample[21] = AngCh2;
            sample[22] = TimeStamp;

            return sample;
        }


    }
}


