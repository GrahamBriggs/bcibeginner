﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LoggingInterfaces;
using BrainHatNetwork;
using brainflow;
using BrainflowInterfaces;
using System.Web;
using Newtonsoft.Json;
using BrainflowDataProcessing;
using System.IO;
using System.Configuration;

namespace brainHatSharpGUI
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            BoardShim.set_log_file("./brainflowLogs.txt");
            BoardShim.log_message((int)LogLevels.LEVEL_DEBUG, "Logging Message Test");
#endif
            BrainflowBoard = null;
            LoggingWindow = null;

            FileWriter = new BrainHatFileWriter();
            FileWriter.Log += OnLog;
        }


        Logging Logger;
        BoardDataReader BrainflowBoard;
        StatusBroadcastServer BroadcastStatus;
        LSLDataBroadcast LslBroadcast;
        TcpipCommandServer CommandServer;
        StatusMonitor MonitorStatus;
        BrainHatFileWriter FileWriter;
        ConfigurationWindow ConfigWindow = null;
        LogWindow LoggingWindow = null;

        bool SelectBoard = true;

        /// <summary>
        /// On Window Loaded
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Text = Properties.Resources.AppName;

            var selectBoard = ConfigurationManager.AppSettings.Get("selectBoard");
            if ( selectBoard != null && selectBoard.Length > 0 )
            {
                bool.TryParse(selectBoard, out SelectBoard);
            }

            if (SelectBoard == false)
            {
                radioButtonDaisy.Visible = false;
                radioButtonCyton.Text = ConfigurationManager.AppSettings.Get("boardName");
            }

            SetUiLabels();

            SetComPortComboBox();
            SetBoardIdRadioButton();
            SetBrainflowStreamingUi();
            SetSrbCheckbox();

            EnableConnectionButtons(true);
            buttonConfigureBoard.Enabled = false;

            Task.Run(async () =>
                {
                    await SetupLoggingAsync();
                    await StartProgramComponentsAsync();
                }).Wait();

            
            Logger.AddLog(this, new LogEventArgs(this, "OnLoad", $"Program started.", LogLevel.INFO));
        }


        /// <summary>
        /// Setup the UI labels 
        /// </summary>
        void SetUiLabels()
        {
            groupBoxBoard.Text = Properties.Resources.ConnectionSettings;
            groupBoxRunStatus.Visible = false;
            labelComPort.Text = Properties.Resources.ComPort;
            buttonRefresh.Text = Properties.Resources.Refresh;
            checkBoxSRB.Text = Properties.Resources.StartWithSrb;
            checkBoxSRB.Visible = false;

            checkBoxUseBFStream.Text = Properties.Resources.EnableBrainflowStreamingLabel;
            labelIpAddress.Text = Properties.Resources.IpAddress;
            labelIpPort.Text = Properties.Resources.Port;

            buttonStart.Text = Properties.Resources.StartServer;
            buttonConfigureBoard.Text = Properties.Resources.ConfigureBoard;
            buttonViewLogs.Text = Properties.Resources.ViewConsole;

            checkBoxLogToFile.Text = Properties.Resources.LogToFile;

            checkBoxLogToFile.Checked = Properties.Settings.Default.LogToFile;
            checkBoxLogToFile.CheckedChanged += checkBoxLogToFile_CheckedChanged;
            
            var version = System.Reflection.Assembly.GetAssembly(typeof(Program)).GetName().Version;
            var date = new DateTime(2000, 1, 1)     // baseline is 01/01/2000
           .AddDays(version.Build)             // build is number of days after baseline
           .AddSeconds(version.Revision * 2);    // revision is half the number of seconds into the day

            labelVersion.Text = $"{Properties.Resources.Version}: {version.Major.ToString()}.{version.Minor}  {date.ToString("MM/dd/yyyy HH:mm:ss")} {PlatformHelper.PlatformHelper.GetLibraryEnvironment()}";
        }


        /// <summary>
        /// Create and start the program components
        /// </summary>
        private async Task StartProgramComponentsAsync()
        {
            //  create status broadcaster
            BroadcastStatus = new StatusBroadcastServer();
            BroadcastStatus.Log += OnLog;

            //  create the TCPIP command server
            CommandServer = new TcpipCommandServer();
            CommandServer.Log += OnLog;
            CommandServer.ProcessReceivedRequest = CommandServerProcessRequestAsync;

            //  create status monitor
            MonitorStatus = new StatusMonitor();
            MonitorStatus.Log += OnLog;
            MonitorStatus.StatusUpdate += OnStatusUpdate;


            await CommandServer.StartCommandServerAsync();
            await MonitorStatus.StartStatusMonitorAsync();
        }


        /// <summary>
        /// Setup the COM port combo box
        /// </summary>
        private void SetComPortComboBox()
        {
            comboBoxComPort.Items.Clear();
            foreach (var nextPort in SerialPort.GetPortNames())
            {
                comboBoxComPort.Items.Add(nextPort.ToString());
            }

            if (comboBoxComPort.Items.Count > 0)
            {
                if (comboBoxComPort.Items.Contains(Properties.Settings.Default.ComPort))
                    comboBoxComPort.SelectedItem = Properties.Settings.Default.ComPort;
                else
                    comboBoxComPort.SelectedIndex = 0;
            }
        }


        /// <summary>
        /// Setup the board Id radio button
        /// </summary>
        private void SetBoardIdRadioButton()
        {
            switch ((BrainhatBoardIds) Properties.Settings.Default.BoardId)
            {
                case BrainhatBoardIds.CYTON_BOARD:
                case BrainhatBoardIds.MENTALIUM:
                default:
                    radioButtonCyton.Checked = true;
                    break;

                case BrainhatBoardIds.CYTON_DAISY_BOARD:
                    radioButtonDaisy.Checked = true;
                    break;
            }
        }


        /// <summary>
        /// Set SrB checkbox state from settings
        /// </summary>
        private void SetSrbCheckbox()
        {
            checkBoxSRB.Checked = Properties.Settings.Default.StartSRB;
        }


        /// <summary>
        /// Set brainflow streaming UI state from settings
        /// </summary>
        private void SetBrainflowStreamingUi()
        {
            checkBoxUseBFStream.Checked = Properties.Settings.Default.UseBFStream;
            textBoxIpAddress.Text = Properties.Settings.Default.IPAddress;
            textBoxIpPort.Text = Properties.Settings.Default.IPPort.ToString();
        }


        /// <summary>
        /// Enable buttons based on connection state
        /// </summary>
        private void EnableConnectionButtons(bool enable)
        {
            

            radioButtonCyton.Enabled = enable;
            radioButtonDaisy.Enabled = enable;
            comboBoxComPort.Enabled = enable;
            buttonRefresh.Enabled = enable;
            checkBoxSRB.Enabled = enable;
            checkBoxUseBFStream.Enabled = enable;
            textBoxIpAddress.Enabled = enable;
            textBoxIpPort.Enabled = enable;
            groupBoxBoard.Visible = enable;
            groupBoxRunStatus.Visible = !enable;

            if ( ! enable )
            {
                labelRunStatus.Text = $"{Properties.Resources.ConnectingToBoard} ...";
                labelDataStatus.Text = checkBoxSRB.Checked ? $"- {Properties.Resources.ConnectSRB1}" : $"- {Properties.Resources.ConnectSRB1Not}";
                labelSrbStatus.Text = checkBoxUseBFStream.Checked ? $"- {Properties.Resources.EnableBrainflowStreaming}" : $"- {Properties.Resources.EnableBrainflowStreamingNot}";
                pictureBoxStatus.Image = Properties.Resources.yellowLight;
            }
        }


        /// <summary>
        /// Save the connection settings and 
        /// create a brainflow input params object to start the board
        /// </summary>
        private BrainFlowInputParams SaveConnectionSettings()
        {
            SaveUiInputToProperties();

            BrainFlowInputParams startupParams = new BrainFlowInputParams()
            {
                serial_port = Properties.Settings.Default.ComPort,
            };

            if (checkBoxUseBFStream.Checked)
            {
                startupParams.ip_address = Properties.Settings.Default.IPAddress;
                startupParams.ip_port = Properties.Settings.Default.IPPort;
            }

            return startupParams;
        }


        /// <summary>
        /// Get the input from the UI and save it to properties
        /// </summary>
        private void SaveUiInputToProperties()
        {
            if (radioButtonCyton.Checked)
            {
                Properties.Settings.Default.BoardId = SelectBoard ? (int)BrainhatBoardIds.CYTON_BOARD : (int)BrainhatBoardIds.MENTALIUM;
            }
            else if (radioButtonDaisy.Checked)
            {
                Properties.Settings.Default.BoardId = (int)BrainhatBoardIds.CYTON_DAISY_BOARD;
            }

            Properties.Settings.Default.ComPort = (string)comboBoxComPort.SelectedItem;

            Properties.Settings.Default.StartSRB = checkBoxSRB.Checked;

            Properties.Settings.Default.UseBFStream = checkBoxUseBFStream.Checked;
            Properties.Settings.Default.IPAddress = textBoxIpAddress.Text;
            if (int.TryParse(textBoxIpPort.Text, out var port))
                Properties.Settings.Default.IPPort = port;
            else
            {
                Properties.Settings.Default.IPPort = 6677;
                textBoxIpAddress.Text = "6677";
            }
            Properties.Settings.Default.Save();
        }


        /// <summary>
        /// Refresh com port combo box button
        /// </summary>
        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            SetComPortComboBox();
        }


        /// <summary>
        /// Start / Stop button
        /// </summary>
        private async void buttonStart_Click(object sender, EventArgs e)
        {
            if (BrainflowBoard == null)
            {
                if ( comboBoxComPort.SelectedItem == null )
                {
                    MessageBox.Show(Properties.Resources.InvalidComPort, Properties.Resources.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                EnableConnectionButtons(false);
                buttonStart.Text = Properties.Resources.Cancel;
             
                BrainFlowInputParams startupParams = SaveConnectionSettings();

                await StartBoard(startupParams);

                Logger.AddLog(this, new LogEventArgs(this, "buttonStart_Click", $"Started server {Properties.Settings.Default.BoardId} on {Properties.Settings.Default.ComPort}.", LogLevel.INFO));
            }
            else
            {
                if (ConfigWindow != null)
                {
                    MessageBox.Show(Properties.Resources.CloseConfigWindowBeforeStoppingServer, Properties.Resources.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Logger.AddLog(this, new LogEventArgs(this, "buttonStart_Click", $"Stopping server.", LogLevel.INFO));

                await ShutDownBoard();

                EnableConnectionButtons(true);

                buttonStart.Text = Properties.Resources.StartServer;
                buttonStart.Enabled = true;
                buttonConfigureBoard.Enabled = false;
            }
        }


        /// <summary>
        /// Create and start a board data reader
        /// </summary>
        private async Task StartBoard(BrainFlowInputParams startupParams)
        {
            DataLatencyTimer = new System.Diagnostics.Stopwatch();
            DataLatencyTimer.Start();


            BrainflowBoard = new BoardDataReader();
            BrainflowBoard.ConnectToBoard += OnConnectToBoard;
            BrainflowBoard.Log += OnLog;

            await Task.Run(async () =>
            {
                await BrainflowBoard.StartBoardDataReaderAsync(Properties.Settings.Default.BoardId, startupParams, checkBoxSRB.Checked);
            });
        }


        /// <summary>
        /// Shut down the board data reader
        /// </summary>
        private async Task ShutDownBoard()
        {
            if (LslBroadcast != null)
            {
                await LslBroadcast.StopLslBroadcastAsync();
                LslBroadcast.Log -= OnLog;
                await BroadcastStatus.StopDataBroadcastServerAsync();
            }

            await BrainflowBoard.StopBoardDataReaderAsync();
            BrainflowBoard.Log -= OnLog;
            BrainflowBoard.ConnectToBoard -= OnConnectToBoard;
            BrainflowBoard.BoardReadData -= OnBrainflowBoardReadData;
            BrainflowBoard = null;
            LslBroadcast = null;
        }


        /// <summary>
        /// Connected to the board, startup the LSL broadcast
        /// </summary>
        private async void OnConnectToBoard(object sender, ConnectToBoardEventArgs e)
        {
            if (LslBroadcast == null && BrainflowBoard != null)
            {
                LslBroadcast = new LSLDataBroadcast();
                LslBroadcast.Log += OnLog;
                await LslBroadcast.StartLslBroadcastAsyc(e.BoardId, e.SampleRate);
                await BroadcastStatus.StartDataBroadcastServerAsync(e.BoardId, e.SampleRate);

                BrainflowBoard.BoardReadData += OnBrainflowBoardReadData;

                //  update the UI
                groupBoxBoard.Invoke(new Action(() =>
                {
                    pictureBoxStatus.Image = Properties.Resources.greenLight;
                    buttonStart.Text = Properties.Resources.StopServer;
                    buttonConfigureBoard.Enabled = true;
                }));
            }
        }


        /// <summary>
        /// Board status update handler
        /// </summary>
        private void OnStatusUpdate(object sender, BrainHatStatusEventArgs e)
        {
            try
            {
                if (BroadcastStatus != null && BrainflowBoard != null)
                {
                    if (BrainflowBoard != null)
                    {
                        e.Status.SampleRate = BrainflowBoard.SampleRate;
                        e.Status.BoardId = BrainflowBoard.BoardId;
                        e.Status.IsStreaming = BrainflowBoard.IsStreaming;
                        e.Status.CytonSRB1 = BrainflowBoard.CytonSRB1;
                        e.Status.DaisySRB1 = BrainflowBoard.DaisySRB1;
                    }
                    if (FileWriter.IsLogging)
                    {
                        e.Status.RecordingDataBrainHat = FileWriter.IsLogging;
                        e.Status.RecordingFileNameBrainHat = Path.GetFileName(FileWriter.FileName);
                        e.Status.RecordingDurationBrainHat = FileWriter.FileDuration;
                    }
                    else
                    {
                        e.Status.RecordingDataBrainHat = false;
                        e.Status.RecordingFileNameBrainHat = "";
                        e.Status.RecordingDurationBrainHat = 0.0;
                    }

                    UpdateStatusUi(e.Status);

                    BroadcastStatus.QueueStringToBroadcast(JsonConvert.SerializeObject(e.Status));
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog(new LogEventArgs(this, "OnStatusUpdate", ex, LogLevel.ERROR));
            }
        }

        private void UpdateStatusUi(BrainHatServerStatus status)
        {
            if (LslBroadcast == null)
                return;

            //  update the UI
            groupBoxRunStatus.Invoke(new Action(() =>
            {
                labelRunStatus.Text = (DataLatencyTimer.Elapsed.TotalSeconds < 0.5) ? Properties.Resources.ReadingData : Properties.Resources.ReadingDataNot;
                labelDataStatus.Text = status.IsStreaming ? Properties.Resources.StreamRunning : Properties.Resources.StreamStopped;

                if (status.IsStreaming)
                {
                    if (DataLatencyTimer.Elapsed.TotalSeconds < .5)
                    {
                        pictureBoxStatus.Image = Properties.Resources.greenLight;
                    }
                    else
                    {
                        pictureBoxStatus.Image = Properties.Resources.redLight;
                    }
                }
                else
                {
                    if (BrainflowBoard != null && BrainflowBoard.UserPausedStream)
                        pictureBoxStatus.Image = Properties.Resources.yellowLight;
                    else
                        pictureBoxStatus.Image = Properties.Resources.redLight;
                }


          
                string srbStatus = "";
                switch ( status.CytonSRB1 )
                {
                    case SrbSet.Unknown:
                        srbStatus = $"{Properties.Resources.SRB1} {Properties.Resources.Unknown}";
                        break;
                    case SrbSet.Disconnected:
                        srbStatus = $"{Properties.Resources.SRB1} {Properties.Resources.Disconnected}";
                        break;
                    case SrbSet.Connected:
                        srbStatus = $"{Properties.Resources.SRB1} {Properties.Resources.Connected}";
                        break;
                }
                if ((BrainhatBoardIds)status.BoardId == BrainhatBoardIds.CYTON_DAISY_BOARD)
                {
                    switch (status.DaisySRB1)
                    {
                        case SrbSet.Unknown:
                            srbStatus = $"Daisy {Properties.Resources.SRB1} {Properties.Resources.Unknown}";
                            break;
                        case SrbSet.Disconnected:
                            srbStatus = $"Daisy {Properties.Resources.SRB1} {Properties.Resources.Disconnected}";
                            break;
                        case SrbSet.Connected:
                            srbStatus = $"Daisy {Properties.Resources.SRB1} {Properties.Resources.Connected}";
                            break;
                    }
                }
                labelSrbStatus.Text = srbStatus;
            }));
        }

        System.Diagnostics.Stopwatch DataLatencyTimer;

        /// <summary>
        /// Data was read from the board, send it to the broadcaster
        /// </summary>
        private void OnBrainflowBoardReadData(object sender, BFChunkEventArgs e)
        {
            if ( e.Chunk.Count() > 0)
                DataLatencyTimer.Restart();

            LslBroadcast.AddData(e.Chunk);
            if (FileWriter.IsLogging)
                FileWriter.AddData(e.Chunk);
        }


        /// <summary>
        /// Configure board button
        /// </summary>
        private void buttonConfigureBoard_Click(object sender, EventArgs e)
        {

            if (BrainflowBoard != null && ConfigWindow == null)
            {
                ConfigWindow = new ConfigurationWindow(BrainflowBoard);
                ConfigWindow.Log += OnLog;
                ConfigWindow.FormClosed += ConfigurationWindowFormClosed;
                ConfigWindow.Show();
            }
            else if (ConfigWindow != null)
            {
                ConfigWindow.WindowState = FormWindowState.Minimized;
                ConfigWindow.Show();
                ConfigWindow.WindowState = FormWindowState.Normal;
            }
            else
            {
                MessageBox.Show(Properties.Resources.StartServerBeforeConfigure, Properties.Resources.AppName);
            }
        }
        //
        private void ConfigurationWindowFormClosed(object sender, FormClosedEventArgs e)
        {
            ConfigWindow.Log -= OnLog;
            ConfigWindow = null;
        }


        // Handle TCPIP server commands
        #region CommandServer

        /// <summary>
        /// Process a request from the TCPIP command server port
        /// </summary>
        private async Task<string> CommandServerProcessRequestAsync(string request)
        {
            try
            {
                UriArgParser args = new UriArgParser(request);
                switch (args.Request)
                {
                    case "loglevel":
                        return ProcessChangeLogLevel(args);

                    case "recording":
                        return await ProcessSetRecording(args);

                    case "srbset":
                        return ProcessSrbSet(args);

                    case "streamset":
                        return ProcessStreamSet(args);

                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.AddLog(new LogEventArgs(this, "CommandServerProcessRequest", e, LogLevel.WARN));
            }

            Logger.AddLog(new LogEventArgs(this, "CommandServerProcessRequest", $"Invalid request {request}", LogLevel.WARN));
            return $"NAK?response=Invalid request&time={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}\n";
        }

        private string ProcessStreamSet(UriArgParser args)
        {
            try
            {
                var enable = bool.Parse(args.GetArg("enable"));
                BrainflowBoard.RequestEnableStreaming(enable);
                return $"ACK?response=Request enable stream {enable}";
            }
            catch (Exception e)
            {
                Logger.AddLog(new LogEventArgs(this, "ProcessStreamSet", e, LogLevel.ERROR));
            }
            return  $"NAK?response=bad arguments";

        }

        private string ProcessSrbSet(UriArgParser args)
        {
            try
            {
                var enable = bool.Parse(args.GetArg("enable"));
                var board = int.Parse(args.GetArg("board"));
                BrainflowBoard.RequestSetSrb1(board, enable);
                return $"ACK?response=Request set SRB1 {enable}";
            }
            catch (Exception e)
            {
                Logger.AddLog(new LogEventArgs(this, "ProcessSrbSet", e, LogLevel.ERROR));
            }
            return $"NAK?response=bad arguments";
        }


        /// <summary>
        /// Process set recording command
        /// </summary>
        private async Task<string> ProcessSetRecording(UriArgParser args)
        {
            try
            {
                var enable = bool.Parse(args.GetArg("enable"));
                var fileName = args.GetArg("filename");
                var formatType = args.GetArg("format");
                if (BrainflowBoard == null)
                {
                    return $"NAK?response=Board is not reading data.";
                }
                if (enable)
                {
                    if (FileWriter.IsLogging)
                    {
                        return $"NAK?response=You must close the current recording file before starting a new one.";
                    }


                    FileWriterType format = FileWriterType.Bdf;
                    switch (formatType)
                    {
                        case "txt":
                            format = FileWriterType.OpenBciTxt;
                            break;
                        case "bdf":
                            format = FileWriterType.Bdf;
                            break;
                    }

                    var recordingDir = ConfigurationManager.AppSettings.Get("defaultDir");
                    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), recordingDir);
                    await FileWriter.StartWritingToFileAsync(path, fileName, BrainflowBoard.BoardId, BrainflowBoard.SampleRate, format, new FileHeaderInfo() { SessionName = fileName });

                    return $"ACK?response=File {Path.GetFileName(FileWriter.FileName)} started.";
                }
                else
                {
                    if (FileWriter != null && !FileWriter.IsLogging)
                    {
                        return $"NAK?response=No recording in progress.";
                    }
                    await FileWriter.StopWritingToFileAsync();
                    return $"ACK?response=Recording stopped.";
                }
            }
            catch (Exception)
            {
                return $"NAK?response=Invalid parameters";
            }
        }


        /// <summary>
        /// Process change log level command
        /// </summary>
        private string ProcessChangeLogLevel(UriArgParser args)
        {
            Logger.LogLevelDisplay = (LogLevel)int.Parse(args.GetArg("level"));
            Logger.AddLog(new LogEventArgs(this, "CommandServerProcessRequest", $"Process request to set log level to {Logger.LogLevelDisplay}.", LogLevel.INFO));
            if (LoggingWindow != null)
                LoggingWindow.ChangeLogLevel();

            return $"ACK?response=Log level set to {Logger.LogLevelDisplay}.";
        }

        #endregion


        //  Logging
        #region Logging

        /// <summary>
        /// Setup the logging
        /// </summary>
        private async Task SetupLoggingAsync()
        {
            Logger = new Logging();
            Logger.LogToFile = Properties.Settings.Default.LogToFile;
            Logger.LoggedEvents += OnLoggedEvents;
            Logger.LogLevelDisplay = LogLevel.VERBOSE;
            await Logger.StartLogging();
        }


        /// <summary>
        /// Log handler
        /// </summary>
        private void OnLog(object sender, LogEventArgs e)
        {
            Logger.AddLog(sender, e);
        }


        /// <summary>
        /// Message handler for logger on logs
        /// </summary>
        private void OnLoggedEvents(object sender, IEnumerable<LogEventArgs> e)
        {
            if (LoggingWindow != null)
            {
                LoggingWindow.OnLoggedEvents(sender, e);
            }
        }

        /// <summary>
        /// View logs button
        /// </summary>
        private void buttonViewLogs_Click(object sender, EventArgs e)
        {
            if (LoggingWindow == null)
            {
                LoggingWindow = new LogWindow(Logger, Logger.LogBuffer.ToArray());
                LoggingWindow.Show();
                LoggingWindow.FormClosing += LoggingWindow_OnFormClosing;
            }
            else
            {
                LoggingWindow.WindowState = FormWindowState.Minimized;
                LoggingWindow.Show();
                LoggingWindow.WindowState = FormWindowState.Normal;
            }
        }


        /// <summary>
        /// Log window form closing
        /// </summary>
        private void LoggingWindow_OnFormClosing(object sender, FormClosingEventArgs e)
        {
            LoggingWindow.FormClosing -= LoggingWindow_OnFormClosing;
            LoggingWindow = null;
        }

        private void checkBoxLogToFile_CheckedChanged(object sender, EventArgs e)
        {
            Logger.LogToFile = checkBoxLogToFile.Checked;
            Properties.Settings.Default.LogToFile = Logger.LogToFile;
            Properties.Settings.Default.Save();
        }

        #endregion



    }
}
