using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowInterfaces
{
  


    //  Brainflow sample read event
    public class BFSampleEventArgs : EventArgs
    {
        public BFSampleEventArgs(IBFSample sample)
        {
            Sample = sample;
        }

        public IBFSample Sample;
    }
    public delegate void BFSampleEventDelegate(object sender, BFSampleEventArgs e);


    //  Brainflow chunk read event
    public class BFChunkEventArgs : EventArgs
    {
        public BFChunkEventArgs(IEnumerable<IBFSample> chunk)
        {
            Chunk = chunk;
        }

        public IEnumerable<IBFSample> Chunk;
    }
    public delegate void BFChunkEventDelegate(object sender, BFChunkEventArgs e);



    //  Get an enumerable of samples for the last number of seconds
    public delegate IBFSample[] GetBFChunkSecondsDelegate(double seconds);

    public delegate IBFSample[] GetBFChunkSecondsRangeDelegate(double from, double to);

    public delegate IBFSample[] GetBFChunkSinceDelegate(DateTimeOffset since);

    //  Get a single sample
    public delegate IBFSample GetBFSampleDelegate();

    //  Get Band Power Delegate
    public delegate IBFSample GetBandPowerDelegate(double band);
}
