using LoggingInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrainHatSharp
{
    public class ReceivedCommandEventArgs : EventArgs
    {
        public ReceivedCommandEventArgs()
        {

        }

        public string Command;
    }
    public delegate void ReceivedCommandEventDelegate(object sender, ReceivedCommandEventArgs e);

    public delegate Task<string> ProcessReceivedRequestAsyncDelegate(string request);

    public class TcpipCommandServer
    {
        public event LogEventDelegate Log;

        public ProcessReceivedRequestAsyncDelegate ProcessReceivedRequest;


        //  Public Interface
        #region PublicInterface

        /// <summary>
        /// Start the TCPIP command server
        /// </summary>
        public async Task StartCommandServerAsync()
        {
            await StopCommandServerAsync();

            CancelTokenSource = new CancellationTokenSource();
            RunTask = RunCommandServerAsync(CancelTokenSource.Token);
        }

       
        /// <summary>
        /// Stop the TCPIP command server
        /// </summary>
        public async Task StopCommandServerAsync()
        {
            if (CancelTokenSource != null)
            {
                CancelTokenSource.Cancel();
                if (RunTask != null)
                    await RunTask;

                CancelTokenSource = null;
                RunTask = null;
            }
        }



        #endregion

        //  Implementation
        #region Implementation


        //  Thread run objects
        CancellationTokenSource CancelTokenSource;
        Task RunTask;


        private async Task RunCommandServerAsync(CancellationToken cancelToken)
        {
            var tcpipServer = new TcpListener(IPAddress.Any, NetworkAddress.TcpipServerPort);
            cancelToken.Register(() => tcpipServer.Stop());
            tcpipServer.Start();

            try
            {
                try
                {
                    while (!cancelToken.IsCancellationRequested)
                    {
                        await Process(await tcpipServer.AcceptTcpClientAsync());
                    }
                }
                catch (ObjectDisposedException odex)
                {
                    if (!cancelToken.IsCancellationRequested)
                        Log?.Invoke(this, new LogEventArgs(this, "RunCommandServerAsync", odex, LogLevel.ERROR));
                }
                catch (Exception ex)
                {
                    Log?.Invoke(this, new LogEventArgs(this, "RunCommandServerAsync", ex, LogLevel.ERROR));
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunCommandServerAsync", e, LogLevel.FATAL));
            }
        }

        private async Task Process(TcpClient tcpClient)
        {
            string clientEndPoint = tcpClient.Client.RemoteEndPoint.ToString();
                    try
            {
                NetworkStream networkStream = tcpClient.GetStream();
                StreamReader reader = new StreamReader(networkStream);
                StreamWriter writer = new StreamWriter(networkStream);
                writer.AutoFlush = true;
                while (true)
                {
                    string request = await reader.ReadLineAsync();
					if (request != null && request.Length > 0)
                    {
                        Log?.Invoke(this, new LogEventArgs(this, "ProcessAcceptTcp", $"Received {request} from {clientEndPoint}.", LogLevel.TRACE));

                        switch ( request )
                        {
                            case "ping":
                                await writer.WriteLineAsync($"ACK?time={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}\n");
                                break;

                            default:
                                var response = $"NAK?time={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}\n";
                                if (ProcessReceivedRequest != null)
                                    response = await ProcessReceivedRequest(request);
                                await writer.WriteLineAsync(response);
                                break;
                        }



                    }
                    else
                        break; // Client closed connection
                }
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (tcpClient.Connected)
                    tcpClient.Close();
            }
        }


        #endregion


    }
}
