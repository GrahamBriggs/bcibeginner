using System;
using System.Collections.Generic;
using System.Text;

namespace BrainHatNetwork
{

  
    /// <summary>
    /// Hat connection status event
    /// Signals periodic update of hat connection properties
    /// </summary>
    public class BrainHatStatusEventArgs : EventArgs
    {
        public BrainHatStatusEventArgs(BrainHatServerStatus status)
        {
            Status = status;

        }

        public BrainHatServerStatus Status { get; set; }

        public string Eth0Description
        {
            get
            {
                if (Status != null && Status.Eth0Address.Length > 0)
                    return Status.Eth0Address;
                else
                    return "not connected";
            }
        }

        public string Wlan0Description
        {
            get
            {
                if (Status != null && Status.Wlan0Address.Length > 0)
                    return $"{Status.Wlan0Address} {Status.Wlan0Mode}";
                else
                    return "not connected";
            }
        }

    }
    //
    public delegate void HatStatusUpdateDelegate(object sender, BrainHatStatusEventArgs e);



}
