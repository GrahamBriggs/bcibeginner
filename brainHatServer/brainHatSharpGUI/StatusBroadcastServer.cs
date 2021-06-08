using BrainHatNetwork;
using LoggingInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static LSL.liblsl;

namespace brainHatSharpGUI
{
    class StatusBroadcastServer
    {
        public event LogEventDelegate Log;

        // Public Interface
        #region PublicInterface

        /// <summary>
        /// Start data broadcast server
        /// </summary>
        public async Task StartDataBroadcastServerAsync(int boardId, int sampleRate)
        {
            await StopDataBroadcastServerAsync();

            BoardId = boardId;
            SampleRate = sampleRate;

            CancelTokenSource = new CancellationTokenSource();
            RunTask = RunDataBroadcastServerAsync(CancelTokenSource.Token);
        }


        /// <summary>
        /// Stop data broadcast server
        /// </summary>
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


        /// <summary>
        /// Queue a string to broadcast
        /// </summary>
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

        int BoardId;
        int SampleRate;

        //  Thread run objects
        CancellationTokenSource CancelTokenSource;
        Task RunTask;
        protected SemaphoreSlim NotifyDataToBroadcast { get; set; }
        ConcurrentQueue<string> StringsToBroadcast { get; set; }

        

        /// <summary>
        /// Run function
        /// </summary>
        private async Task RunDataBroadcastServerAsync(CancellationToken cancelToken)
        {
            try
            {
                var info = new StreamInfo("bhStatus", "bhStatus", 1, IRREGULAR_RATE, channel_format_t.cf_string, NetworkUtilities.GetHostName());

                info.desc().append_child_value("boardId", BoardId.ToString());
                info.desc().append_child_value("sampleRate", SampleRate.ToString());

                //  create UDP client
                using (var outlet = new StreamOutlet(info))
                {
                    try
                    {
                        while (!cancelToken.IsCancellationRequested)
                        {
                            await NotifyDataToBroadcast.WaitAsync(cancelToken);

                            while (!StringsToBroadcast.IsEmpty)
                            {
                                try
                                {
                                    StringsToBroadcast.TryDequeue(out var broadcastString);
                                    outlet.push_sample(new string[]  { broadcastString});
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
