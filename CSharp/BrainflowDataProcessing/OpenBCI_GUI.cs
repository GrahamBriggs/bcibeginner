using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainflowDataProcessing
{

    public static class OpenBCI_GUI
    {
        /**
        * @description Get the correct points of FFT based on sampling rate
        * @returns `int` - Points of FFT. 125Hz, 200Hz, 250Hz -> 256points. 1000Hz -> 1024points. 1600Hz -> 2048 points.
*/
        public static int getNfftSafe(int sampleRate)
        {
            switch (sampleRate)
            {
                case 500:
                    return 512;
                case 1000:
                    return 1024;
                case 1600:
                    return 2048;
                case 125:
                case 200:
                case 250:
                default:
                    return 256;
            }
        }
    }

}
