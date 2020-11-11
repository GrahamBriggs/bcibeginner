using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBCIInterfaces
{

    /// <summary>
    /// Blink Detection
    /// </summary>
    public enum WinkState
    {
        Rising,
        Falling,
        Wink,
    }
    //
    public enum Eyes
    {
        Left,
        Right,
    }

    /// <summary>
    /// Event to broadcast blink event
    /// </summary>
    public class DetectedBlinkEventArgs : EventArgs
    {
        public DetectedBlinkEventArgs(Eyes eye, WinkState state, double timeStamp)
        {
            Eye = eye;
            State = state;
            TimeStamp = timeStamp;
        }


        public Eyes Eye { get; set; }
        public WinkState State { get; set; }
        public double TimeStamp { get; set; }

        public string Details { get; set; }
    }
    //
    public delegate void DetectedBlinkDelegate(object sender, DetectedBlinkEventArgs e);



}
