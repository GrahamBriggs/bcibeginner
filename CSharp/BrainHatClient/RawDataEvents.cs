using OpenBCIInterfaces;
using System;


namespace BrainHatClient
{

    /// <summary>
    /// Data received event
    /// </summary>
    public class HatRawDataReceivedEventArgs : EventArgs
    {
        public HatRawDataReceivedEventArgs(OpenBciCyton8Reading data)
        {
            Data = data;
        }

        public OpenBciCyton8Reading Data { get; set; }
    }
    //
    public delegate void HatRawDataReceivedEventDelegate(object sender, HatRawDataReceivedEventArgs e);

}
