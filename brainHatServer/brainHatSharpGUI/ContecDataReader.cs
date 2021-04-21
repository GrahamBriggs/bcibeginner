using brainflow;
using BrainflowDataProcessing;
using BrainflowInterfaces;
using LoggingInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace brainHatSharpGUI
{
  


    /// <summary>
    /// Board Data Reader Class
    /// pretty specific to Cyton 8 channel right now, could be more generalized to work with other boards (?)
    /// </summary>
    public class ContecDataReader: IBoardDataReader
    {
        public event LogEventDelegate Log;
        public event ConnectBoBoardEventDelegate ConnectToBoard;
        public event BFChunkEventDelegate BoardReadData;

        //  Properties
        public int BoardReadDelayMilliseconds { get; set; }

        //  Is stream running property
        public bool IsStreaming => StreamRunning;

        //  SRB1 for cyton board setting
        public SrbSet CytonSRB1
        {
            get
            {
                 return SrbSet.Unknown;
            }
        }

        //  SRB1 for daisy board setting
        public SrbSet DaisySRB1
        {
            get
            {
               return SrbSet.Unknown;
            }
        }


        // Public Interface
        #region PublicInterface

        /// <summary>
        /// Start the board data reader process
        /// </summary>
        public async Task StartBoardDataReaderAsync(int boardId, BrainFlowInputParams inputParams, bool startSrb1Set)
        {
            await StopBoardDataReaderAsync();

            StartSrb1CytonSet = false;
            StartSrb1DaisySet = false;

            Log?.Invoke(this, new LogEventArgs(this, "StartBoardDataReaderAsync", $"Starting board data reader", LogLevel.DEBUG));

            BoardId = boardId;
            InputParams = inputParams;

            CancelTokenSource = new CancellationTokenSource();
            RunTask = RunBoardDataReaderAsync(CancelTokenSource.Token);

            LastReportTime = DateTimeOffset.UtcNow;
            ReadCounter = 0;
            LastReadingTimestamp = -1.0;
            LastSampleIndex = -1;
            CountMissingIndex = 0;
            RequestToggleStreamingMode = false;
        }


        /// <summary>
        /// Stop the board reader process
        /// </summary>
        public async Task StopBoardDataReaderAsync()
        {
            if (CancelTokenSource != null)
            {
                Log?.Invoke(this, new LogEventArgs(this, "StopBoardDataReaderAsync", $"Stopping board data reader", LogLevel.DEBUG));

                CancelTokenSource.Cancel();
                if (RunTask != null)
                    await RunTask;

                CancelTokenSource = null;
                RunTask = null;

                await ReleaseBoardAsync();
            }
        }


        /// <summary>
        /// Request toggle streaming mode
        /// </summary>
        public void RequestEnableStreaming(bool enable)
        {
            if ((enable && !StreamRunning) || (!enable && StreamRunning))
            {
                RequestToggleStreamingMode = true;
                UserPausedStream = !enable;
            }
        }

        /// <summary>
        /// Request set SRB1, not allowed for this board
        /// </summary>
        /// <param name="board"></param>
        /// <param name="enable"></param>
        /// <returns></returns>
        public bool RequestSetSrb1(int board, bool enable)
        {
            return false;
        }

        #endregion


        //  Implementation
        #region Implementation

        public ContecDataReader()
        {
            BoardReadDelayMilliseconds = 50;    //  default 20 hz

            SettingsLock = new SemaphoreSlim(1);

            StreamRunning = false;
        }


        //  Thread run objects
        CancellationTokenSource CancelTokenSource;
        Task RunTask;

        //  Settings control lock
        SemaphoreSlim SettingsLock;

        //  The board shim
        SerialPort BoardPort;
        public int BoardId { get; private set; }
        public int SampleRate { get; private set; }

        BrainFlowInputParams InputParams;


        bool StartSrb1CytonSet { get; set; }
        bool StartSrb1DaisySet { get; set; }

        public bool UserPausedStream { get; protected set; }
        private bool StreamRunning;
        private bool RequestToggleStreamingMode;

        //  Some properties to manage and inspect the data stream
        double LastReadingTimestamp { get; set; }
        int ReadCounter { get; set; }
        int ReadCounterLastReport { get; set; }
        DateTimeOffset LastReportTime { get; set; }
        int LastSampleIndex;
        int CountMissingIndex;
        private int InvalidReadCounter { get; set; }

     
        /// <summary>
        /// Board is initialized and prepared for commands or streaming
        /// </summary>
        private bool BoardReady => BoardPort != null && BoardPort.IsOpen;

        private void OpenSerialPort()
        {
            BoardPort = new SerialPort(InputParams.serial_port, 921600, Parity.None, 8, StopBits.One);
            //BoardPort.PortName = InputParams.serial_port;
            //BoardPort.BaudRate = 921600;
            //BoardPort.Parity = Parity.None;
            //BoardPort.DataBits = 8;
            //BoardPort.StopBits = StopBits.One;
            //BoardPort.Handshake = Handshake.None;

            BoardPort.Open();
        }


        /// <summary>
        /// Init the board session
        /// </summary>
            private async Task InitializeBoardAsync()
        {
            try
            {
                Log?.Invoke(this, new LogEventArgs(this, "InitializeBoardAsync", $"Initializaing board", LogLevel.DEBUG));

                await ReleaseBoardAsync();
                RequestToggleStreamingMode = false;

                OpenSerialPort();
                if ( ! BoardPort.IsOpen )
                {
                    await ReleaseBoardAsync();
                    return;
                }

                SampleRate = 100;
            
                await StartStreamingAsync();

                ConnectToBoard?.Invoke(this, new ConnectToBoardEventArgs(BoardId, SampleRate));
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "InitializeBoardAsync", e, LogLevel.ERROR));

                if (BoardPort != null && BoardPort.IsOpen)
                    BoardPort.Close();
            }
        }


        /// <summary>
        /// Release the board session
        /// </summary>
        private async Task ReleaseBoardAsync()
        {
            if (BoardPort != null)
            {
                try
                {
                    if (BoardPort.IsOpen)
                    {
                        Log?.Invoke(this, new LogEventArgs(this, "ReleaseBoardAsync", $"Releasing board.", LogLevel.DEBUG));

                        await StopStreamingAsync();
                        BoardPort.Close();
                    }
                    InvalidReadCounter = 0;
                }
                catch (Exception e)
                {
                    Log?.Invoke(this, new LogEventArgs(this, "ReleaseBoardAsync", e, LogLevel.ERROR));
                }
            }
        }


        /// <summary>
        /// Stop the active session streaming data
        /// </summary>
        protected async Task StopStreamingAsync()
        {
            if (StreamRunning)
            {
                try
                {
                    StreamRunning = false;
                    Log?.Invoke(this, new LogEventArgs(this, "StopStreamingAsync", $"Stopping data stream.", LogLevel.DEBUG));
                    //  TODO - send 90 02 command await Task.Run(() => { TheBoard.stop_stream(); });
                }
                catch (Exception e)
                {
                    Log?.Invoke(this, new LogEventArgs(this, "StopStreamingAsync", e, LogLevel.ERROR));
                }
            }
        }


        /// <summary>
        /// Start the active session streaming data
        /// </summary>
        protected async Task StartStreamingAsync()
        {
            if (!StreamRunning)
            {
                try
                {
                    BoardPort.Write(new char[] { (char)0x90, (char)0x09 }, 0, 2);
                    //BoardPort.Read
                    StreamRunning = true;
                }
                catch (Exception e)
                {
                    Log?.Invoke(this, new LogEventArgs(this, "StartStreamingAsync", e, LogLevel.ERROR));
                }
            }
        }


        /// <summary>
        /// Connect (reconnect) to board function
        /// </summary>
        private async Task EstablishConnectionWithBoardAsync()
        {
            if (!BoardReady)
            {
                await InitializeBoardAsync();

                if (!BoardReady)
                    await Task.Delay(1000);
            }
        }

       
        /// <summary>
        /// Are we ready to read the board
        /// </summary>
        private async Task<bool> PreparedToReadBoard()
        {
            await EstablishConnectionWithBoardAsync();

            if (!BoardReady)
            {
                //  board is not ready, wait a second before trying again
                await Task.Delay(TimeSpan.FromSeconds(1));
                return false;
            }
            else if ( RequestToggleStreamingMode )
            {
                if (StreamRunning)
                    await StopStreamingAsync();
                else
                    await StartStreamingAsync();
                RequestToggleStreamingMode = false;
                return false;
            }
           else if ( ! StreamRunning )
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                return false;
            }
            else if (InvalidReadCounter > InvalidReadCounterTimeout)
            {
                //  board was connected, but it has not given any data for a while, release it to attempt reconnect
                Log?.Invoke(this, new LogEventArgs(this, "PreparedToReadBoard", $"Not receiving data from the board. Attempt to receonnect.", LogLevel.WARN));
                await ReleaseBoardAsync();
                await Task.Delay(TimeSpan.FromSeconds(1));
                return false;
            }

            return true;
        }


        /// <summary>
        /// Read board data run function
        /// </summary>
        private async Task RunBoardDataReaderAsync(CancellationToken cancelToken)
        {
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    

                    if (! await PreparedToReadBoard())
                        continue;

                    List<IBFSample> data = ReadDataFromBoard();

                    BoardReadData?.Invoke(this, new BFChunkEventArgs(data));

                    await Task.Delay(BoardReadDelayMilliseconds);
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "RunBoardDataReaderAsync", e, LogLevel.FATAL));
            }
        }


        /// <summary>
        /// Read data from the board, and return collection of data
        /// </summary>
        private List<IBFSample> ReadDataFromBoard()
        {
            var data = new List<IBFSample>();

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                //  TODO read data
                double[,] rawData = null;

                sw.Stop();
                var timeReadData = sw.Elapsed.TotalSeconds;
                sw.Restart();

                if (rawData.GetLength(1) == 0)
                {
                    InvalidReadCounter++;
                }
                else
                {
                    InvalidReadCounter = 0;

                    if (rawData.GetLength(1) > 255)
                        return data;    //  this is the first connection surge, flush these readings

                    //Log?.Invoke(this, new LogEventArgs(this, "ReadDataFromBoard", $"Read {rawData.Columns()}.", LogLevel.VERBOSE));

                    double oldestReadingTime, period;
                    CalculateReadingPeriod(rawData, out oldestReadingTime, out period);

                    for (int i = 0; i < rawData.GetLength(1); i++)
                    {
                        IBFSample nextSample = null;
                        switch (BoardId)
                        {
                            case 0:
                                nextSample = new BFCyton8Sample(rawData, i);
                                break;
                            case 2:
                                nextSample = new BFCyton16Sample(rawData, i);
                                break;
                            default:
                                //  TODO ganglion
                                break;
                        }

                        nextSample.TimeStamp = oldestReadingTime + ((i + 1) * period);
                        data.Add(nextSample);
                        InspectSampleIndex(nextSample);
                    }

                    sw.Stop();
                    var timeParseData = sw.Elapsed.TotalSeconds;

                    ReadCounter += data.Count;
                    var since = (DateTimeOffset.UtcNow - LastReportTime);
                    if (since.TotalMilliseconds > 5000)
                    {
                        Log?.Invoke(this, new LogEventArgs(this, "ReadDataFromBoard", $"Read {ReadCounter - ReadCounterLastReport} in {since.TotalSeconds.ToString("F3")} s. { (int)(((ReadCounter - ReadCounterLastReport) / since.TotalSeconds) + 0.5)} SPS.  Read time {timeReadData.ToString("F4")} Parse Time {timeParseData.ToString("F4")}.", LogLevel.TRACE));
                        LastReportTime = DateTimeOffset.UtcNow;
                        ReadCounterLastReport = ReadCounter;

                        if (CountMissingIndex > 1)
                        {
                            Log?.Invoke(this, new LogEventArgs(this, "ReadDataFromBoard", $"Missed {CountMissingIndex} samples in the past {since.TotalSeconds.ToString("F3")} seconds.", LogLevel.WARN));
                        }
                        CountMissingIndex = 0;
                    }
                }
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "ReadDataFromBoard", e, LogLevel.ERROR));
            }

            return data;
        }


        /// <summary>
        /// Timeout for invalid reads
        /// </summary>
        private int InvalidReadCounterTimeout
        {
            get
            {
                //  five second no data delay
                return (int)(5.0 / (BoardReadDelayMilliseconds / 1000.0));
            }
        }


        /// <summary>
        /// Calculate the period between readings
        /// use this period to estimate more precise time stamp of each reading
        /// </summary>
        private void CalculateReadingPeriod(double[,] rawData, out double oldestReadingTime, out double period)
        {
            //  TODO
            oldestReadingTime = 0;
            period = 0;
           
        }


        /// <summary>
        /// Check the sample index sequence
        /// log a warning if sample index is missing
        /// </summary>
        private void InspectSampleIndex(IBFSample sample)
        {
            if (LastSampleIndex < 0)
            {
                LastSampleIndex = (int)sample.SampleIndex;
                return;
            }
 
            var difference = sample.SampleIndex.SampleIndexDifference(LastSampleIndex);
            LastSampleIndex = (int)sample.SampleIndex;

            switch (BoardId)
            {
                case 0:
                    if (difference > 1)
                        CountMissingIndex++;
                    break;

                case 2:
                    if (difference > 2)
                        CountMissingIndex++;
                    break;
            }
        }


      



        #endregion
    }
}
