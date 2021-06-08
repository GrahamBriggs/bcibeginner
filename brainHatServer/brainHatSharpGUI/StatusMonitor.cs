using BrainHatNetwork;
using LoggingInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace brainHatSharpGUI
{
    class StatusMonitor
    {
        public event LogEventDelegate Log;

        public event HatConnectionStatusUpdateDelegate StatusUpdate;

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
                    string eth0, wlan0, hostName;
                    GetNetworkProperties(out eth0, out wlan0, out hostName);

                    BrainHatServerStatus status = new BrainHatServerStatus()
                    {
                        HostName = hostName,
                        Eth0Address = eth0,
                        Wlan0Address = wlan0,
                        TimeStamp = DateTimeOffset.UtcNow,
                    };

                    StatusUpdate?.Invoke(this, new BrainHatStatusEventArgs(status));

                    await Task.Delay(2000);
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunStatusMonitorAsync", e, LogLevel.ERROR));
            }
        }


        /// <summary>
        /// Get the network properties
        /// </summary>
        private void GetNetworkProperties(out string eth0, out string wlan0, out string hostName)
        {
            eth0 = "";
            wlan0 = "";
            hostName = "";
            try
            {
                BrainHatNetwork.NetworkUtilities.GetNetworkAddresses(out eth0, out wlan0);
                hostName = NetworkUtilities.GetHostName();
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunStatusMonitorAsync", e, LogLevel.ERROR));
            }
        }



        #endregion
    }
}
