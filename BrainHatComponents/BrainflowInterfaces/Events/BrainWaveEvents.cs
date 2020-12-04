using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowInterfaces
{
    public enum BrainWave
    {
        None,
        Alpha,
        Beta,
        Motion,
    }


    /// <summary>
    /// Event to broadcast brainwave detection event
    /// </summary>
    public class DetectedBrainWaveEventArgs : EventArgs
    {
        public DetectedBrainWaveEventArgs(BrainWave type, DateTimeOffset time)
        {
            Type = type;
            Time = time;
        }

        public BrainWave Type { get; protected set; }
        public DateTimeOffset Time { get; protected set; }
    }
    public delegate void DetectedBrainWaveDelegate(object sender, DetectedBrainWaveEventArgs e);
}
