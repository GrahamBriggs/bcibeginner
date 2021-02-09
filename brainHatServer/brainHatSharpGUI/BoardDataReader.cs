using Accord.Math;
using brainflow;
using BrainflowDataProcessing;
using BrainflowInterfaces;
using LoggingInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace brainHatSharpGUI
{
    //  Connect / Disconnect
    public class ConnectToBoardEventArgs : EventArgs
    {
        public ConnectToBoardEventArgs(int boardId, int sampleRate)
        {
            BoardId = boardId;
            SampleRate = sampleRate;
        }

        public int BoardId { get; set; }
        public int SampleRate { get; set; }
    }
    public delegate void ConnectBoBoardEventDelegate(object sender, ConnectToBoardEventArgs e);





    /// <summary>
    /// Board Data Reader Class
    /// pretty specific to Cyton 8 channel right now, could be more generalized to work with other boards (?)
    /// </summary>
    public class BoardDataReader
    {
        public event LogEventDelegate Log;
        public event ConnectBoBoardEventDelegate ConnectToBoard;
        public event BFChunkEventDelegate BoardReadData;

        //  Properties
        public int BoardReadDelayMilliseconds { get; set; }

        public string BoardConfigurationString { get; protected set; }

        // Public Interface
        #region PublicInterface

        /// <summary>
        /// Start the board data reader process
        /// </summary>
        public async Task StartBoardDataReaderAsync(int boardId, BrainFlowInputParams inputParams)
        {
            await StopBoardDataReaderAsync();

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

                ReleaseBoard();
            }
        }


        /// <summary>
        /// Reset the active session
        /// </summary>
        public async Task ResetSessionAsync()
        {
            Log?.Invoke(this, new LogEventArgs(this, "ResetSession", $"Resetting active session", LogLevel.DEBUG));

            await StopBoardDataReaderAsync();
            await Task.Delay(3000);
            await StartBoardDataReaderAsync(BoardId, InputParams);
        }


        public async Task StartSignalTestAsync(TestSignalMode mode)
        {
            Log?.Invoke(this, new LogEventArgs(this, "StartSignalTestAsync", $"Setting board to signal test mode {mode}.", LogLevel.DEBUG));

            await Task.Delay(1);

            StopStreaming();

            await Task.Delay(2000);

            _ = ConfigureBoard("?");

            await Task.Delay(3000);

            var response = ConfigureBoard(mode.TestModeCommand());

            Log?.Invoke(this, new LogEventArgs(this, "StartSignalTestAsync", $"Set test mode response:{response}.", LogLevel.DEBUG));

            StartStreaming();
        }


        public async Task ResetChannelsToDefaultAsync()
        {
            Log?.Invoke(this, new LogEventArgs(this, "ResetChannelsToDefaultAsync", $"Resetting channels to defaults", LogLevel.DEBUG));

            await Task.Delay(1);

            StopStreaming();

            await Task.Delay(2000);

            _ = ConfigureBoard("?");

            await Task.Delay(3000);

            var response = ConfigureBoard("d");

            Log?.Invoke(this, new LogEventArgs(this, "ResetChannelsToDefaultAsync", $"Resetting channels to defaults:{response}.", LogLevel.DEBUG));

            BoardConfigurationString = ConfigureBoard("?");

            StartStreaming();
        }

        /// <summary>
        /// Configure board 
        /// send raw ascii characters to the board and return the response
        /// </summary>
        protected string ConfigureBoard(string command)
        {
            Log?.Invoke(this, new LogEventArgs(this, "ConfigureBoard", $"Sending {command} to board.", LogLevel.DEBUG));

            try
            {
                if (TheBoard.is_prepared())
                {
                    return TheBoard.config_board(command);
                }
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "ConfigureBoard", e, LogLevel.ERROR));
            }

            return "";
        }

        #endregion

        //  Implementation
        #region Implementation

        public BoardDataReader()
        {
            BoardShim.set_log_level((int)LogLevels.LEVEL_ERROR);

            BoardReadDelayMilliseconds = 50;    //  default 20 hz

            StreamRunning = false;
        }


        //  Thread run objects
        CancellationTokenSource CancelTokenSource;
        Task RunTask;

        //  The board shim
        BoardShim TheBoard { get; set; }
        public int BoardId { get; private set; }
        public int SampleRate { get; private set; }
        protected int TimeStampIndex { get; set; }
        protected BrainFlowInputParams InputParams { get; private set; }
        private int InvalidReadCounter { get; set; }

        private bool StreamRunning;

        //  Some properties to manage and inspect the data stream
        double LastReadingTimestamp { get; set; }
        int ReadCounter { get; set; }
        int ReadCounterLastReport { get; set; }
        DateTimeOffset LastReportTime { get; set; }


        /// <summary>
        /// Connect (reconnect) to board function
        /// </summary>
        private async Task EstablishConnectionWithBoard()
        {
            if (!BoardReady)
            {
                await InitializeBoard();

                if (!BoardReady)
                    await Task.Delay(1000);
            }
        }


        /// <summary>
        /// Init the board session
        /// </summary>
        private async Task InitializeBoard()
        {
            try
            {
                Log?.Invoke(this, new LogEventArgs(this, "InitializeBoard", $"Initializaing board", LogLevel.DEBUG));

                ReleaseBoard();

                TheBoard = new BoardShim(BoardId, InputParams);
                SampleRate = BoardShim.get_sampling_rate(BoardId);
                TimeStampIndex = BoardShim.get_timestamp_channel(BoardId);
                TheBoard.prepare_session();

                BoardConfigurationString = "";
                int retries = 0;
                while (BoardConfigurationString.Length == 0 && retries < 5)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    BoardConfigurationString = TheBoard.config_board("?");
                    if (retries > 0)
                        Log?.Invoke(this, new LogEventArgs(this, "InitializeBoard", $"Retry get board configuration.", LogLevel.WARN));
                    retries++;
                }

                StartStreaming();

                // for STREAMING_BOARD you have to query information using board id for master board
                // because for STREAMING_BOARD data format is determined by master board!
                if (BoardId == (int)brainflow.BoardIds.STREAMING_BOARD)
                {
                    BoardId = int.Parse(InputParams.other_info);
                }

                await Task.Delay(TimeSpan.FromSeconds(3));

                ConnectToBoard?.Invoke(this, new ConnectToBoardEventArgs(BoardId, SampleRate));
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "InitializeBoard", e, LogLevel.ERROR));

                if (TheBoard != null && TheBoard.is_prepared())
                {
                    TheBoard.release_session();
                }
                TheBoard = null;
            }
        }


        /// <summary>
        /// Release the board session
        /// </summary>
        private void ReleaseBoard()
        {
            if (TheBoard != null)
            {
                if (TheBoard.is_prepared())
                {
                    Log?.Invoke(this, new LogEventArgs(this, "ReleaseBoard", $"Releasing board.", LogLevel.DEBUG));

                    if (StreamRunning)
                        StopStreaming();
                    TheBoard.release_session();
                }

                InvalidReadCounter = 0;
            }
        }


        /// <summary>
        /// Stop the active session streaming data
        /// </summary>
        private void StopStreaming()
        {
            StreamRunning = false;
            TheBoard.stop_stream();
        }

        /// <summary>
        /// Start the active session streaming data
        /// </summary>
        private void StartStreaming()
        {
            TheBoard.start_stream();
            StreamRunning = true;
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
                    await EstablishConnectionWithBoard();

                    if (!BoardReady || ! StreamRunning)
                    {
                        //  board is not ready, wait a second before trying again
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        continue;
                    }
                    else if (InvalidReadCounter > InvalidReadCounterTimeout)
                    {
                        //  board was connected, but it has not given any data for a while, release it to attempt reconnect
                        Log?.Invoke(this, new LogEventArgs(this, "RunBoardDataReaderAsync", $"Not receiving data from the board. Attempt to receonnect.", LogLevel.WARN));
                        ReleaseBoard();
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        continue;
                    }

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
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                var rawData = TheBoard.get_board_data();

                sw.Stop();
                var timeReadData = sw.Elapsed.TotalSeconds;
                sw.Restart();

                if (rawData.Columns() == 0)
                {
                    InvalidReadCounter++;
                }
                else
                {
                    InvalidReadCounter = 0;

                    if (rawData.Columns() > 255)
                        return data;    //  this is the first connection surge, flush these readings

                    //Log?.Invoke(this, new LogEventArgs(this, "ReadDataFromBoard", $"Read {rawData.Columns()}.", LogLevel.VERBOSE));

                    double oldestReadingTime, period;
                    CalculateReadingPeriod(rawData, out oldestReadingTime, out period);

                    for (int i = 0; i < rawData.Columns(); i++)
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
                        Log?.Invoke(this, new LogEventArgs(this, "ReadDataFromBoard", $"Read {ReadCounter - ReadCounterLastReport} in {since.TotalSeconds.ToString("F3")} s. { (int)(((ReadCounter - ReadCounterLastReport)/ since.TotalSeconds)+0.5)} SPS.  Read time {timeReadData.ToString("F4")} Parse Time {timeParseData.ToString("F4")}.", LogLevel.TRACE));
                        LastReportTime = DateTimeOffset.UtcNow;
                        ReadCounterLastReport = ReadCounter;

                        if (CountMissingIndex > 0)
                        {
                            Log?.Invoke(this, new LogEventArgs(this, "ReadDataFromBoard", $"Missed {CountMissingIndex} samples in the past {since.TotalSeconds.ToString("F3")} seconds.", LogLevel.WARN));
                            CountMissingIndex = 0;
                        }
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
            double newestReadingTime = rawData[TimeStampIndex, 0];
            oldestReadingTime = rawData[TimeStampIndex, rawData.Columns() - 1];
            if (LastReadingTimestamp > 0)
            {
                oldestReadingTime = LastReadingTimestamp;
                LastReadingTimestamp = newestReadingTime;
            }
            else
            {
                LastReadingTimestamp = oldestReadingTime;
            }

            period = (newestReadingTime - oldestReadingTime) / rawData.Columns();
        }


        int LastSampleIndex;
        int CountMissingIndex;

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

            var nextIndex = (int)(sample.SampleIndex);
            var difference = BrainflowDataProcessor.SampleIndexDifference(LastSampleIndex, nextIndex);
            LastSampleIndex = nextIndex;


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


        /// <summary>
        /// Board is initialized and in streaming mode
        /// </summary>
        private bool BoardReady => TheBoard != null && TheBoard.is_prepared();




        #endregion
    }
}
