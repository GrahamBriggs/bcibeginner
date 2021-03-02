using brainflow;
using BrainflowDataProcessing;
using BrainflowInterfaces;
using LoggingInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

                await ReleaseBoardAsync();
            }
        }


        public bool IsStreaming => StreamRunning;

        /// <summary>
        /// Start the board data stream
        /// </summary>
        public async Task StartStreamAsync()
        {
            if (!StreamRunning)
            {
                Log?.Invoke(this, new LogEventArgs(this, "StartStreamAsync", $"Starting stream.", LogLevel.DEBUG));
                await StartStreamingAsync();
            }
        }


        /// <summary>
        /// Stop the board data stream
        /// </summary>
        public async Task StopStreamAsync()
        {
            if (StreamRunning)
            {
                Log?.Invoke(this, new LogEventArgs(this, "StopStreamAsync", $"Stopping stream.", LogLevel.DEBUG));
                await StopStreamingAsync();
            }
        }


        /// <summary>
        /// Stop the streaming and empty the serial buffer
        /// </summary>
        public async Task<bool> StopStreamAndEmptyBufferAsnyc()
        {
            Log?.Invoke(this, new LogEventArgs(this, "StopStreamAndEmptyBufferAsnyc", $"Stopping stream.", LogLevel.DEBUG));
            await StopStreamingAsync();
            //  read any data out of the buffer
            int retries = 0;
            while (retries < 5)
            {
                var discard = TheBoard.get_board_data();
                if (discard.GetLength(1) == 0)
                    break;
                await Task.Delay(200);
                retries++;
            }
            if ( retries == 5 )
                Log?.Invoke(this, new LogEventArgs(this, "StopStreamAndEmptyBufferAsnyc", $"Read {retries} chunks from the buffer before empty.", LogLevel.WARN));

            await Task.Delay(2000);

            retries = 0;
            while (retries < 5)
            {
                var test = ConfigureBoard("V");
                if (ValidateFirmwareString(test))
                {
                    return true;
                }
                else
                {
                    Log?.Invoke(this, new LogEventArgs(this, "StopStreamAndEmptyBufferAsnyc", $"Flushing buffer: {test}", retries == 0 ? LogLevel.TRACE : LogLevel.WARN));
                }
                retries++;
                await Task.Delay(1000);
            }

            Log?.Invoke(this, new LogEventArgs(this, "StopStreamAndEmptyBufferAsnyc", $"Failed to verify empty serial buffer.", LogLevel.ERROR));
            return false;
        }


        /// <summary>
        /// Get current board configuration
        /// </summary>
        public async Task<string> GetBoardConfigurationAsync()
        {
            string config = "";
            bool success = false; ;

            Log?.Invoke(this, new LogEventArgs(this, "GetBoardConfigurationAsync", $"Getting board registers string.", LogLevel.DEBUG));

            await Task.Run(() => { success = GetBoardRegistersString(out config); });
            if ( success )
            {
                return config;
            }

            Log?.Invoke(this, new LogEventArgs(this, "GetBoardConfigurationAsync", $"Invalid register string:{config}.", LogLevel.WARN));
            throw new Exception("Unable to get valid board configuration");
        }


       


        /// <summary>
        /// Set Board Channels
        /// </summary>
        public async Task<bool> SetBoardChannelAsync(IEnumerable<int> channels, ICytonChannelSettings settings)
        {
            if (await SettingsLock.WaitAsync(0))
            {
                try
                {
                    Log?.Invoke(this, new LogEventArgs(this, "SetBoardChannelAsync", $"Setting board channels {string.Join(",", channels)} to {settings}.", LogLevel.DEBUG));

                    string settingsString = "";
                    int setChannelCounter = 0;
                    foreach (var nextChannel in channels)
                    {
                        setChannelCounter++;
                        settingsString += $"x{nextChannel.ChannelSetCharacter()}{settings.PowerDown.BoolCharacter()}{(int)(settings.Gain)}{(int)(settings.InputType)}{settings.Bias.BoolCharacter()}{settings.Srb2.BoolCharacter()}0X";
                        if ( setChannelCounter > 0 )
                        {
                            var response = ConfigureBoard(settingsString);
                            Log?.Invoke(this, new LogEventArgs(this, "SetBoardChannelAsync", $"Response:{response}.", LogLevel.DEBUG));
                            settingsString = "";
                            setChannelCounter = 0;
                            await Task.Delay(500);
                        }
                    }

                    if (settingsString.Length > 0)
                    {
                        var response = ConfigureBoard(settingsString);
                        Log?.Invoke(this, new LogEventArgs(this, "SetBoardChannelAsync", $"Response:{response}.", LogLevel.DEBUG));
                        await Task.Delay(500);
                    }
                   
                    return true;
                }
                finally
                {
                    SettingsLock.Release();
                }
            }
            else
            {
                Log?.Invoke(this, new LogEventArgs(this, "SetBoardChannelAsync", $"Another process is busy with the settings.", LogLevel.WARN));
                return false;
            }
        }


        /// <summary>
        /// Set Board Channels
        /// </summary>
        public async Task<bool> SetImpedanceModeAsync(IEnumerable<int> channels, ICytonChannelSettings settings)
        {
            if (await SettingsLock.WaitAsync(0))
            {
                try
                {
                    Log?.Invoke(this, new LogEventArgs(this, "SetImpedanceModeAsync", $"Setting board impedance {string.Join(",", channels)} to P {settings.LlofP} N {settings.LlofN}.", LogLevel.DEBUG));

                    string settingsString = "";
                    int setChannelCounter = 0;
                    foreach (var nextChannel in channels)
                    {
                        setChannelCounter++;
                        settingsString += $"z{nextChannel.ChannelSetCharacter()}{settings.LlofP.BoolCharacter()}{settings.LlofN.BoolCharacter()}Z";
                        if (setChannelCounter > 0)
                        {
                            var response = ConfigureBoard(settingsString);
                            Log?.Invoke(this, new LogEventArgs(this, "SetImpedanceModeAsync", $"Response:{response}.", LogLevel.DEBUG));
                            settingsString = "";
                            setChannelCounter = 0;
                            await Task.Delay(500);
                        }
                    }

                    if (settingsString.Length > 0)
                    {
                        var response = ConfigureBoard(settingsString);
                        Log?.Invoke(this, new LogEventArgs(this, "SetImpedanceModeAsync", $"Response:{response}.", LogLevel.DEBUG));
                        await Task.Delay(500);
                    }

                    return true;
                }
                finally
                {
                    SettingsLock.Release();
                }
            }
            else
            {
                Log?.Invoke(this, new LogEventArgs(this, "SetImpedanceModeAsync", $"Another process is busy with the settings.", LogLevel.WARN));
                return false;
            }
        }



        public async Task<bool> SetSrb1Async(ICytonChannelSettings settings, bool connect)
        {
            if (await SettingsLock.WaitAsync(0))
            {
                try
                {
                    Log?.Invoke(this, new LogEventArgs(this, "SetSrb1Async", $"{(connect ? "Connecting" : "Disconnecting")} SRB1 for {(settings.ChannelNumber < 9 ? "Cyton" : "Daisy")}.", LogLevel.DEBUG));

                    string settingsString = $"x{settings.ChannelNumber.ChannelSetCharacter()}{settings.PowerDown.BoolCharacter()}{(int)(settings.Gain)}{(int)(settings.InputType)}{settings.Bias.BoolCharacter()}{settings.Srb2.BoolCharacter()}{(connect ? "1" : "0")}X";

                    var response = ConfigureBoard(settingsString);
                    Log?.Invoke(this, new LogEventArgs(this, "SetSrb1Async", $"Response:{response}.", LogLevel.DEBUG));
                    
                    return true;
                }
                finally
                {
                    SettingsLock.Release();
                }
            }
            else
            {
                Log?.Invoke(this, new LogEventArgs(this, "SetSrb1Async", $"Another process is busy with the settings.", LogLevel.WARN));
                return false;
            }
        }


        /// <summary>
        /// Reset channels to default
        /// </summary>
        public async Task<bool> ResetChannelsToDefaultAsync()
        {
            if (await SettingsLock.WaitAsync(0))
            {
                try
                {
                    Log?.Invoke(this, new LogEventArgs(this, "ResetChannelsToDefaultAsync", $"Resetting channels to defaults", LogLevel.DEBUG));

                    //  send the command to set channel defaults
                    var response = ConfigureBoard("d");
                    Log?.Invoke(this, new LogEventArgs(this, "ResetChannelsToDefaultAsync", $"Response:{response}.", LogLevel.DEBUG));
                    return true;
                }
                finally
                {
                    SettingsLock.Release();
                }
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Start signal test
        /// </summary>
        public async Task<bool> SetSignalTestModeAsync(TestSignalMode mode)
        {
            if (await SettingsLock.WaitAsync(0))
            {
                try
                {
                    Log?.Invoke(this, new LogEventArgs(this, "SetSignalTestModeAsync", $"Setting board to signal test mode {mode}.", LogLevel.DEBUG));

                    //  send command to enter test mode
                    var response = ConfigureBoard(mode.TestModeCharacter());
                    Log?.Invoke(this, new LogEventArgs(this, "StartSignalTestAsync", $"Response:{response}.", LogLevel.DEBUG));
                    return true;
                }
                finally
                {
                    SettingsLock.Release();
                }
            }
            else
            {
                Log?.Invoke(this, new LogEventArgs(this, "SetSignalTestModeAsync", $"Another process is busy with the settings.", LogLevel.WARN));
                return false;
            }
        }
        
        #endregion


        //  Implementation
        #region Implementation

        public BoardDataReader()
        {
            BoardShim.set_log_level((int)LogLevels.LEVEL_DEBUG);

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
        /// Init the board session
        /// </summary>
        private async Task InitializeBoardAsync()
        {
            try
            {
                Log?.Invoke(this, new LogEventArgs(this, "InitializeBoardAsync", $"Initializaing board", LogLevel.DEBUG));

                await ReleaseBoardAsync();

                TheBoard = new BoardShim(BoardId, InputParams);
                SampleRate = BoardShim.get_sampling_rate(BoardId);
                TimeStampIndex = BoardShim.get_timestamp_channel(BoardId);
                TheBoard.prepare_session();
                await StartStreamingAsync();

                // for STREAMING_BOARD you have to query information using board id for master board
                // because for STREAMING_BOARD data format is determined by master board!
                if (BoardId == (int)brainflow.BoardIds.STREAMING_BOARD)
                {
                    BoardId = int.Parse(InputParams.other_info);
                }

                await Task.Delay(TimeSpan.FromSeconds(5));

                ConnectToBoard?.Invoke(this, new ConnectToBoardEventArgs(BoardId, SampleRate));
            }
            catch (Exception e)
            {
                Log?.Invoke(this, new LogEventArgs(this, "InitializeBoardAsync", e, LogLevel.ERROR));

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
        private async Task ReleaseBoardAsync()
        {
            if (TheBoard != null)
            {
                if (TheBoard.is_prepared())
                {
                    Log?.Invoke(this, new LogEventArgs(this, "ReleaseBoardAsync", $"Releasing board.", LogLevel.DEBUG));

                    if (StreamRunning)
                        await StopStreamingAsync();
                    TheBoard.release_session();
                }

                InvalidReadCounter = 0;
            }
        }


        /// <summary>
        /// Stop the active session streaming data
        /// </summary>
        protected async Task StopStreamingAsync()
        {
            if (StreamRunning)
            {
                StreamRunning = false;
                await Task.Run(() => { TheBoard.stop_stream(); });
            }
        }

        /// <summary>
        /// Start the active session streaming data
        /// </summary>
        protected async Task StartStreamingAsync()
        {
            if (!StreamRunning)
            {
                await Task.Run(() => { TheBoard.start_stream(); });
                StreamRunning = true;
            }
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


        private bool GetBoardRegistersString(out string config)
        {
            config = "";
            var registerSettings = ConfigureBoard("?");
            if (!ValidateRegisterSettingsString(registerSettings))
            {
                config = registerSettings;
                return false;
            }

            var version = ConfigureBoard("V");
            if (!ValidateFirmwareString(version))
            {
                config = registerSettings;
                return false;
            }

            config = $"Firmware: {version}{registerSettings}";
            return true;
        }

        private bool ValidateRegisterSettingsString(string registerSettings)
        {
            if (registerSettings.Length > "Board ADS Registers".Length)
            {
                if (registerSettings.Trim('\r', '\n').Substring(0, "Board ADS Registers".Length) == "Board ADS Registers" && registerSettings.Substring(registerSettings.Length - 3, "$$$".Length) == "$$$")
                    return true;
            }
            return false;
        }

        private bool ValidateFirmwareString(string firmware)
        {
            if (firmware.Length > 3 && firmware.Substring(0, 1) == "v" && firmware.Substring(firmware.Length - 3, "$$$".Length) == "$$$")
                return true;
            return false;
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
                    await EstablishConnectionWithBoardAsync();

                    if (!BoardReady || !StreamRunning)
                    {
                        //  board is not ready, wait a second before trying again
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        continue;
                    }
                    else if (InvalidReadCounter > InvalidReadCounterTimeout)
                    {
                        //  board was connected, but it has not given any data for a while, release it to attempt reconnect
                        Log?.Invoke(this, new LogEventArgs(this, "RunBoardDataReaderAsync", $"Not receiving data from the board. Attempt to receonnect.", LogLevel.WARN));
                        ReleaseBoardAsync();
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
                Stopwatch sw = new Stopwatch();
                sw.Start();

                var rawData = TheBoard.get_board_data();

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
            double newestReadingTime = rawData[TimeStampIndex, 0];
            oldestReadingTime = rawData[TimeStampIndex, rawData.GetLength(1) - 1];
            if (LastReadingTimestamp > 0)
            {
                oldestReadingTime = LastReadingTimestamp;
                LastReadingTimestamp = newestReadingTime;
            }
            else
            {
                LastReadingTimestamp = oldestReadingTime;
            }

            period = (newestReadingTime - oldestReadingTime) / rawData.GetLength(1);
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


        /// <summary>
        /// Board is initialized and in streaming mode
        /// </summary>
        private bool BoardReady => TheBoard != null && TheBoard.is_prepared();




        #endregion
    }
}
