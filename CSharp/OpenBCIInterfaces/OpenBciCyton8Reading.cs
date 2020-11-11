using Accord.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBCIInterfaces
{
    /// <summary>
    /// Data structure for OpenBCI Cyton Board data
    /// mimicing the property names from OpenBCI_GUI csv file
    /// with a hack to handle 'synthetic' data, ignoring the eight extra channels
    /// </summary>
    public class OpenBciCyton8Reading
    {
        //  Properties
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

        //  Constructor
        public OpenBciCyton8Reading()
        {

        }

        //  Constructor
        public OpenBciCyton8Reading(OpenBciCyton8Reading data)
        {
            SampleIndex = data.SampleIndex;
            ExgCh0 = data.ExgCh0;
            ExgCh1 = data.ExgCh1;
            ExgCh2 = data.ExgCh2;
            ExgCh3 = data.ExgCh3;
            ExgCh4 = data.ExgCh4;
            ExgCh5 = data.ExgCh5;
            ExgCh6 = data.ExgCh6;
            ExgCh7 = data.ExgCh7;
            AcelCh0 = data.AcelCh0;
            AcelCh1 = data.AcelCh1;
            AcelCh2 = data.AcelCh2;
            Other0 = data.Other0;
            Other1 = data.Other1;
            Other2 = data.Other2;
            Other3 = data.Other3;
            Other4 = data.Other4;
            Other5 = data.Other5;
            Other6 = data.Other6;
            AngCh0 = data.AngCh0;
            AngCh1 = data.AngCh1;
            AngCh2 = data.AngCh2;
            TimeStamp = data.TimeStamp;
        }

        //  Constructor from raw data for a specific observation (column)
        public OpenBciCyton8Reading(double[,] rawData, int column)
        {
            var data = rawData.GetColumn(column);

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
            //  synthetic data has 16 exg channels, skip these
            if (rawData.Rows() == 31)
                index += 8;
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


        public OpenBciCyton8Reading(string fromTxt)
        {
            var fields = fromTxt.Split(',');
            if ( fields.Length == 24 )
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

        // Set Data for a specific EXG channel
        public void SetExgData(int channel, double data)
        {
            switch ( channel )
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
    }
}
    

