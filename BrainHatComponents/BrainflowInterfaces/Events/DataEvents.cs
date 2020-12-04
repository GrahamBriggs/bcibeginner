using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowInterfaces
{
  


    //  Open BCI Reading from Cyton 8 channel board Event
    public class BFSampleEventArgs : EventArgs
    {
        public BFSampleEventArgs(IBFSample reading)
        {
            Reading = reading;
        }

        public IBFSample Reading;
    }
    public delegate void BFSampleEventDelegate(object sender, BFSampleEventArgs e);


    //  Get an enumerable of samples for the last number of seconds
    public delegate IEnumerable<IBFSample> GetBFSamplesDelegate(double seconds);

    //  Get a single sample
    public delegate IBFSample GetBFSampleDelegate();

    //  Get Band Power Delegate
    public delegate IBFSample GetBandPowerDelegate(int band);
}
