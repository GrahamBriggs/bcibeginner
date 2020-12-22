using BrainflowInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrainflowDataProcessing
{

    /// <summary>
    /// Processor receeived data event
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        public DataReceivedEventArgs(string details)
        {
            Details = details;
        }

        //  Number of samples received in this reporting epoch
        public int NumberOfSamples { get; set; }
        //  Duration of this reporting epoch
        public TimeSpan EpochDuration { get; set; }
        //  Details string
        public string Details { get; set; }
    }
    //
    public delegate void DataReceivedDelegate(object sender, DataReceivedEventArgs e);



    /// <summary>
    /// Event to broadcast the current state of the processor
    /// </summary>
    public class ProcessorCurrentStateReportEventArgs : EventArgs
    {
        public ProcessorCurrentStateReportEventArgs()
        {
            ValidData = true;

            BandPowers = new Dictionary<string, IBFSample>();
            
        }

        public IBFSample CurrentSample { get; set; }
        public IBFSample CurrentDeviation { get; set; }
        public IBFSample CurrentDevMedian { get; set; }

        public IBFSample GetBandPower(double band)
        {
            if (BandPowers.ContainsKey(band.BandPowerKey()))
                return BandPowers[band.BandPowerKey()];

            return null;
        }

        public bool ValidData { get; set; }
       
        public string Details { get; set; }

        public Dictionary<string, IBFSample> BandPowers { get; set; }
    }
    //
    public delegate void ProcessorCurrentStateReportDelegate(object sender, ProcessorCurrentStateReportEventArgs e);


}
