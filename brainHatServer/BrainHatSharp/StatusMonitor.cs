﻿using LoggingInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrainHatNetwork;
using System.Net.NetworkInformation;

namespace BrainHatSharp
{
    public class StatusEventArgs : EventArgs
    {
        public string HostName { get; set; }
        public string Eth0Address { get; set; }
        public string WlanAddress { get; set; }
    }
    public delegate void StatusEventDelegate(object sender, StatusEventArgs e);


    class StatusMonitor
    {
        public event LogEventDelegate Log;

        public event StatusEventDelegate StatusUpdate;

        //  Public Interface
        #region PublicInterface


        /// <summary>
        /// Start monitoring status and generating events
        /// </summary>
        public async Task StartStatusMonitorAsync()
        {
            await StopStatusMonitorAsync();

            CancelTokenSource = new CancellationTokenSource();
            RunTask = RunStatusMonitorAsync(CancelTokenSource.Token);

        }


        /// <summary>
        /// Stop status monitor
        /// </summary>
        public async Task StopStatusMonitorAsync()
        {
            if (CancelTokenSource != null)
            {
                CancelTokenSource.Cancel();
                if (RunTask != null)
                {
                    await RunTask;
                }

                CancelTokenSource = null;
                RunTask = null;
            }
        }


        #endregion


        // Implementation
        #region Implementation


        //  Thread run objects
        CancellationTokenSource CancelTokenSource;
        Task RunTask;



        private async Task RunStatusMonitorAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    string eth0, wlan0;
                    GetNetworkAddresses(out eth0, out wlan0);
                    var hostName = NetworkUtilities.GetHostName();

                    StatusUpdate?.Invoke(this, new StatusEventArgs() { HostName = hostName, Eth0Address = eth0, WlanAddress = wlan0 });

                    await Task.Delay(10000);
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunStatusMonitorAsync", e, LogLevel.ERROR));
            }
        }

        private static void GetNetworkAddresses(out string eth0, out string wlan0)
        {
            eth0 = "";
            wlan0 = "";
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            switch (ni.Name)
                            {
                                case "eth0":
                                    eth0 = ip.Address.ToString();
                                    break;
                                case "wlan0":
                                    wlan0 = ip.Address.ToString();
                                    break;
                            }
                        }
                    }
                }
            }
        }



        #endregion
    }
}