using BrainHatNetwork;
using LoggingInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrainHatSharp
{
    public class StatusBroadcastServer
    {
        public event LogEventDelegate Log;

        // Public Interface
        #region PublicINterface
        public async Task StartDataBroadcastServerAsync()
        {
            await StopDataBroadcastServerAsync();

            CancelTokenSource = new CancellationTokenSource();
            RunTask = RunDataBroadcastServerAsync(CancelTokenSource.Token);

        }



        public async Task StopDataBroadcastServerAsync()
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

        public void QueueStringToBroadcast(string broadcast)
        {
            if (RunTask != null)
            {
                StringsToBroadcast.Enqueue(broadcast);
                NotifyDataToBroadcast.Release();
            }
        }

        #endregion


        // Implementation
        #region Implementation

        public StatusBroadcastServer()
        {
            NotifyDataToBroadcast = new SemaphoreSlim(0);
            StringsToBroadcast = new ConcurrentQueue<string>();
        }


        //  Thread run objects
        CancellationTokenSource CancelTokenSource;
        Task RunTask;
        protected SemaphoreSlim NotifyDataToBroadcast { get; set; }
        ConcurrentQueue<string> StringsToBroadcast { get; set; }

        private async Task RunDataBroadcastServerAsync(CancellationToken cancelToken)
        {
            try
            {
                //  create UDP client
                using (var udpClient = new UdpClient())
                {
                    try
                    {
                        ////  join the multicast group
                        //udpClient.JoinMulticastGroup(IPAddress.Parse(NetworkAddress.MulticastGroupAddress));

                        while (!cancelToken.IsCancellationRequested)
                        {
                            await NotifyDataToBroadcast.WaitAsync(cancelToken);

                            while (!StringsToBroadcast.IsEmpty)
                            {
                                try
                                {
                                    StringsToBroadcast.TryDequeue(out var broadcastString);
                                    var sendBytes = Encoding.UTF8.GetBytes(broadcastString);
                                    await udpClient.SendAsync(sendBytes, sendBytes.Length, BrainHatNetworkAddresses.MulticastGroupAddress, BrainHatNetworkAddresses.StatusPort);
                                }
                                catch (Exception ex)
                                {
                                    Log?.Invoke(this, new LogEventArgs(this, "RunBroadcastServerAsync", ex, LogLevel.ERROR));
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    { }
                    catch (Exception e)
                    {
                        Log?.Invoke(this, new LogEventArgs(this, "RunBroadcastServerAsync", e, LogLevel.ERROR));
                    }
                }
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunBroadcastServerAsync", e, LogLevel.ERROR));
            }
        }

        #endregion


    }
}
