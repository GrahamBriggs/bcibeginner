using OpenBCIInterfaces;
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

        //  Number of readings received in this reporting epoch
        public int NumberOfReadings { get; set; }
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
            
        }

        public OpenBciCyton8Reading CurrentReading { get; set; }
        public OpenBciCyton8Reading CurrentDeviation { get; set; }
        public OpenBciCyton8Reading CurrentDevMedian { get; set; }
        public OpenBciCyton8Reading CurrentBandPower08 { get; set; }
        public OpenBciCyton8Reading CurrentBandPower10 { get; set; }
        public OpenBciCyton8Reading CurrentBandPower12 { get; set; }

        public bool ValidData { get; set; }
       
        public string Details { get; set; }
    }
    //
    public delegate void ProcessorCurrentStateReportDelegate(object sender, ProcessorCurrentStateReportEventArgs e);


}
