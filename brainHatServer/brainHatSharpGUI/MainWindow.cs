using System;
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

namespace brainHatSharpGUI
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();

            BrainflowBoard = null;
            LoggingWindow = null;

            FileWriter = new OBCIGuiFormatFileWriter();
        }

        LogWindow LoggingWindow;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _ = Task.Run(async () => 
            {
                await SetupLoggingAsync(); 
                await StartProgramComponentsAsync();
            });

            SetComPortComboBox();

            SetBoardIdRadioButton();

            Logger.AddLog(this, new LogEventArgs(this, "OnLoad", $"Program started.", LogLevel.INFO));
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

            if (comboBoxComPort.Items.Contains(Properties.Settings.Default.ComPort))
                comboBoxComPort.SelectedItem = Properties.Settings.Default.ComPort;
            else if (comboBoxComPort.Items.Count > 0)
                comboBoxComPort.SelectedIndex = 0;
        }


        /// <summary>
        /// Setup the board Id radio button
        /// </summary>
        private void SetBoardIdRadioButton()
        {
            switch (Properties.Settings.Default.BoardId)
            {
                case 0:
                default:
                    radioButtonCyton.Checked = true;
                    break;

                case 2:
                    radioButtonDaisy.Checked = true;
                    break;
            }
        }


        /// <summary>
        /// Create and start the program components
        /// </summary>
        private async Task StartProgramComponentsAsync()
        {
            //  create udp multicast broadcaster
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

            await Task.Run(async () =>
            {
                await BroadcastStatus.StartDataBroadcastServerAsync();
                await CommandServer.StartCommandServerAsync();
                await MonitorStatus.StartStatusMonitorAsync();
            });
        }


        /// <summary>
        /// Status update handler
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
                    }
                    e.Status.RecordingDataBrainHat = FileWriter.IsLogging;
                    e.Status.RecordingFileNameBrainHat = Path.GetFileName(FileWriter.FileName);
                    e.Status.RecordingDurationBrainHat = FileWriter.FileDuration;


                    BroadcastStatus.QueueStringToBroadcast($"networkstatus?hostname={e.Status.HostName}&status={JsonConvert.SerializeObject(e.Status)}\n");
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog(new LogEventArgs(this, "OnStatusUpdate", ex, LogLevel.ERROR));
            }
        }


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


        /// <summary>
        /// Process set recording command
        /// </summary>
        private async Task<string> ProcessSetRecording(UriArgParser args)
        {
            try
            {
                var enable = bool.Parse(args.GetArg("enable"));
                var fileName = args.GetArg("filename");
                if ( BrainflowBoard ==  null )
                {
                    return $"NAK?response=Board is not reading data.";
                }
                if (enable)
                {
                    if (FileWriter.IsLogging)
                    {
                        return $"NAK?response=You must close the current recording file before starting a new one.";
                    }
                    await FileWriter.StartWritingToFileAsync(fileName, BrainflowBoard.BoardId, BrainflowBoard.SampleRate);
                    return $"ACK?response=File {Path.GetFileName(FileWriter.FileName)} started.";
                }
                else
                {
                    if (!FileWriter.IsLogging)
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


        Logging Logger;
        BoardDataReader BrainflowBoard;
        StatusBroadcastServer BroadcastStatus;
        LSLDataBroadcast LslBroadcast;
        TcpipCommandServer CommandServer;
        StatusMonitor MonitorStatus;
        OBCIGuiFormatFileWriter FileWriter;

      
        /// <summary>
        /// Start / Stop button
        /// </summary>
        private void buttonStart_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = false;

            if (BrainflowBoard == null)
            {
                BrainFlowInputParams startupParams = SaveConnectionSettings();

                StartBoard(startupParams);

                SetUiForConnectionState(false);

                buttonStart.Text = "Stop Server";
                groupBoxBoard.Text = " --- Connecting to Board --- ";

                Logger.AddLog(this, new LogEventArgs(this, "buttonStart_Click", $"Started board {Properties.Settings.Default.BoardId} on {Properties.Settings.Default.ComPort}.", LogLevel.INFO));
            }
            else
            {
                ShutDownBoard();

                SetUiForConnectionState(true);

                buttonStart.Text = "Start Server";
                groupBoxBoard.Text = " Connect to Board ";
            }

            buttonStart.Enabled = true;
        }

        private void SetUiForConnectionState(bool enable)
        {
            radioButtonCyton.Enabled = enable;
            radioButtonDaisy.Enabled = enable;
            comboBoxComPort.Enabled = enable;
            buttonRefresh.Enabled = enable;
        }


        /// <summary>
        /// Create and start a board data reader
        /// </summary>
        private void StartBoard(BrainFlowInputParams startupParams)
        {
            BrainflowBoard = new BoardDataReader();
            BrainflowBoard.ConnectToBoard += OnConnectToBoard;
            BrainflowBoard.Log += OnLog;

            _= Task.Run(async () =>
            {
                await BrainflowBoard.StartBoardDataReaderAsync(Properties.Settings.Default.BoardId, startupParams);
            });
        }


        /// <summary>
        /// Shut down the board data reader
        /// </summary>
        private void ShutDownBoard()
        {
            if (LslBroadcast != null)
            {
                _ = Task.Run(async () => { await LslBroadcast.StopLslBroadcastAsync(); });
                LslBroadcast.Log -= OnLog;
            }

            _ = Task.Run(async () => { await BrainflowBoard.StopBoardDataReaderAsync(); });
            BrainflowBoard.Log -= OnLog;
            BrainflowBoard.ConnectToBoard -= OnConnectToBoard;
            BrainflowBoard.BoardReadData -= OnBrainflowBoardReadData;
            BrainflowBoard = null;
            LslBroadcast = null;
        }


        /// <summary>
        /// Save the connection settings and 
        /// create a brainflow input params object to start the board
        /// </summary>
        private BrainFlowInputParams SaveConnectionSettings()
        {
            if (radioButtonCyton.Checked)
                Properties.Settings.Default.BoardId = 0;
            else if (radioButtonDaisy.Checked)
                Properties.Settings.Default.BoardId = 2;

            Properties.Settings.Default.ComPort = (string)comboBoxComPort.SelectedItem;

            BrainFlowInputParams startupParams = new BrainFlowInputParams()
            {
                serial_port = Properties.Settings.Default.ComPort,
            };
            return startupParams;
        }


        /// <summary>
        /// Connected to the board, startup the LSL broadcast
        /// </summary>
        private void OnConnectToBoard(object sender, ConnectToBoardEventArgs e)
        {
            if (LslBroadcast == null && BrainflowBoard != null)
            {
                LslBroadcast = new LSLDataBroadcast();
                LslBroadcast.Log += OnLog;
                _ = Task.Run(async () =>
                {
                    await LslBroadcast.StartLslBroadcastAsyc(e.BoardId, e.SampleRate);
                });

                BrainflowBoard.BoardReadData += OnBrainflowBoardReadData;

                groupBoxBoard.Invoke(new Action(() => { groupBoxBoard.Text = " <<<  Connected to Board   >>> "; }));
            }
        }


        /// <summary>
        /// Data was read from the board, send it to the broadcaster
        /// </summary>
        private void OnBrainflowBoardReadData(object sender, BFChunkEventArgs e)
        {
            LslBroadcast.AddData(e.Chunk);
            if (FileWriter.IsLogging)
                FileWriter.AddData(e.Chunk);
        }


        //  Logging
        #region Logging

        /// <summary>
        /// Setup the logging
        /// </summary>
        private async Task SetupLoggingAsync()
        {
            Logger = new Logging();
            Logger.LoggedEvents += OnLoggedEvents;
            Logger.LogLevelDisplay = LogLevel.TRACE;
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
            foreach (var nextLog in e)
            {
                if (LoggingWindow != null)
                    LoggingWindow.OnLoggedEvents(sender, e);
            }
        }


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

        private void LoggingWindow_OnFormClosing(object sender, FormClosingEventArgs e)
        {
            LoggingWindow = null;
        }



        #endregion

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            SetComPortComboBox();

        }
    }
}
