using System;


namespace BrainHatClient
{


    /// <summary>
    /// Hat connection state event
    /// Signals when hat is discovered | connected | disconnected
    /// </summary>
    public class HatConnectionEventArgs : EventArgs
    {
        public HatConnectionEventArgs(HatConnectionState state, string hostName)
        {
            State = state;
            HostName = hostName;
        }

        public HatConnectionState State { get; set; }
        public string HostName { get; set; }
    }
    //
    public delegate void HatConnectionUpdateDelegate(object sender, HatConnectionEventArgs e);
    //
    public enum HatConnectionState
    {
        Discovered,
        Lost,
        Connected,
        Disconnected,
    }



    /// <summary>
    /// Hat connection status event
    /// Signals periodic update of hat connection properties
    /// </summary>
    public class HatConnectionStatusEventArgs : EventArgs
    {
        public HatConnectionStatusEventArgs(HatConnectionStatus status, TimeSpan ping)
        {
            Connection = status;
            PingSpeed = ping;
        }

        public HatConnectionStatus Connection { get; set; }
        public TimeSpan PingSpeed { get; set; }
    }
    //
    public delegate void HatConnectionStatusUpdateDelegate(object sender, HatConnectionStatusEventArgs e);




    /// <summary>
    /// Hat connection status object
    /// </summary>
    public class HatConnectionStatus
    {
        public HatConnectionStatus()
        {

        }

        public string Eth0Connection
        {
            get
            {
                if (Eth0.Length > 0)
                {
                    return Eth0;
                }
                else
                {
                    return " - not connected - ";
                }
            }
        }

        public string WLan0Connection
        {
            get
            {
                if (Wlan0.Length > 0 && WlanMode.Length > 0)
                {
                    return $"{Wlan0} {WlanMode}";
                }
                else
                {
                    return " - NC - ";
                }
            }
        }

        public string HostName { get; set; }
        public string Eth0 { get; set; }
        public string Wlan0 { get; set; }
        public string WlanMode { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }


    



}
