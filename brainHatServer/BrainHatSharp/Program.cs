using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoggingInterfaces;
using BrainflowDataProcessing;
using brainflow;
using BrainflowInterfaces;
using BrainHatNetwork;
using Newtonsoft.Json;
using System.Web;

namespace BrainHatSharp
{
    class Program
    {
        //  Program components
        static Logging Logger;
        static BoardDataReader BrainflowBoard;
        static StatusBroadcastServer BroadcastStatus;
        static LSLDataBroadcast LslBroadcast;
        static TcpipCommandServer CommandServer;
        static StatusMonitor MonitorStatus;


        static async Task Main(string[] args)
        {
            //  logging
            await SetupLoggingAsync();


            //  parse command line input params
            if (!ParseProgramStartupParameters(args, out var input_params, out var board_id, out var demoFileName))
            {
                return;
            }

            if (!await CreateProgramComponentsAsync(input_params, board_id, demoFileName))
            {
                return;
            }

            //  spin until quit
            bool runProgram = true;
            while (runProgram)
            {
                string readLine = Console.ReadLine();

                if (readLine == null)
                {
                    //  this hack is necessary because monodevelop debug console is buggy
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    continue;
                }

                var input = readLine.Split(' ');
                if (input.Length > 0)
                {
                    var command = input[0].ToUpper();

                    switch (command)
                    {
                        case "Q":
                            Logger.AddLog(new LogEventArgs(main, "Main", $"Keyboard entry Q. Quitting program.", LogLevel.INFO));
                            runProgram = false;
                            break;
                    }
                }
            }

            //  stop all the running tasks
            await Task.WhenAll(
                MonitorStatus.StopStatusMonitorAsync(),
                CommandServer.StopCommandServerAsync(),
                BroadcastStatus.StopDataBroadcastServerAsync(),
                LslBroadcast.StopLslBroadcastAsync(),
                Logger.StopLogging()); ;

            if (BrainflowBoard != null)
                await BrainflowBoard.StopBoardDataReaderAsync();
        }


        /// <summary>
        /// Setup the logging
        /// </summary>
        private static async Task SetupLoggingAsync()
        {
            Logger = new Logging();
            Logger.LoggedEvents += OnLoggedEvents;
            Logger.LogLevelDisplay = LogLevel.TRACE;
            await Logger.StartLogging();
            main = new MainFunction();
            Logger.AddLog(new LogEventArgs(main, "Main", $"Program started", LogLevel.INFO));
        }


        /// <summary>
        /// Create and hookup the program components
        /// </summary>
        private static async Task<bool> CreateProgramComponentsAsync(BrainFlowInputParams input_params, int board_id, string demoFileName)
        {

            //  create udp multicast broadcaster
            BroadcastStatus = new StatusBroadcastServer();
            BroadcastStatus.Log += OnProgramComponentLog;
            await BroadcastStatus.StartDataBroadcastServerAsync();

            //  create the TCPIP command server
            CommandServer = new TcpipCommandServer();
            CommandServer.Log += OnProgramComponentLog;
            CommandServer.ProcessReceivedRequest = CommandServerProcessRequest;
            await CommandServer.StartCommandServerAsync();

            //  create status monitor
            MonitorStatus = new StatusMonitor();
            MonitorStatus.Log += OnProgramComponentLog;
            MonitorStatus.StatusUpdate += OnStatusUpdate;
            await MonitorStatus.StartStatusMonitorAsync();

            //  create a board reader or a file reader depending on command line args
            if (board_id >= -1)
            {
                //  create data reader
                BrainflowBoard = new BoardDataReader();
                BrainflowBoard.ConnectToBoard += OnConnectToBoard;
                BrainflowBoard.Log += OnProgramComponentLog;
                await BrainflowBoard.StartBoardDataReaderAsync(board_id, input_params);
            }

            return true;
        }

        private static void OnReadSample(object sender, BFSampleEventArgs e)
        {
            LslBroadcast.AddData(e.Sample);
        }

        private static async void OnConnectToBoard(object sender, ConnectToBoardEventArgs e)
        {
            if ( LslBroadcast == null )
            {
                LslBroadcast = new LSLDataBroadcast();
                LslBroadcast.Log += OnProgramComponentLog;
                await LslBroadcast.StartLslBroadcastAsyc(e.BoardId, e.SampleRate);

                if ( BrainflowBoard != null )
                {
                    BrainflowBoard.BoardReadData += OnBrainflowBoardReadData;
                }
                
            }
        }


        /// <summary>
        /// Process a request received on the TCPIP server thread
        /// return a response string if it is a valid request and it is successfully completed
        /// </summary>
        private static async Task<string> CommandServerProcessRequest(string request)
        {
            try
            {
                var parseRequest = request.Split('?');
                if (parseRequest.Length == 2)
                {
                    var args = HttpUtility.ParseQueryString(parseRequest[1]);
                    switch (parseRequest[0])
                    {
                        case "loglevel":
                            Logger.LogLevelDisplay = (LogLevel)int.Parse(args.Get("level"));
                            Logger.AddLog(new LogEventArgs(main, "CommandServerProcessRequest", $"Process request to set log level to {Logger.LogLevelDisplay}.", LogLevel.INFO));
                            return $"ACK?response=Log level set to {Logger.LogLevelDisplay}.\n";

                        default:
                            await Task.Delay(1);
                            break;
                    }
                }
                else
                {
                    Logger.AddLog(new LogEventArgs(main, "CommandServerProcessRequest", $"Invalid request {request}.", LogLevel.WARN));
                }
            }
            catch (Exception)
            {
                Logger.AddLog(new LogEventArgs(main, "CommandServerProcessRequest", $"Invalid request {request}.", LogLevel.WARN));
            }

            return $"NAK?time={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}\n";
        }


        /// <summary>
        /// Data was read from the board, do something with it
        /// </summary>
        private static void OnBrainflowBoardReadData(object sender, BFChunkEventArgs e)
        {
            LslBroadcast.AddData(e.Chunk);          
        }


        /// <summary>
        /// Status update, broadcast it to the multicast if it is running
        /// </summary>
        private static void OnStatusUpdate(object sender, BrainHatStatusEventArgs e)
        {
            try
            {
                if (BroadcastStatus != null && BrainflowBoard != null)
                {
                    e.Status.SampleRate = BrainflowBoard.SampleRate;
                    e.Status.BoardId = BrainflowBoard.BoardId;

                    BroadcastStatus.QueueStringToBroadcast($"networkstatus?hostname={e.Status.HostName}&status={JsonConvert.SerializeObject(e.Status)}\n");
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog(new LogEventArgs(main, "OnStatusUpdate", ex, LogLevel.ERROR));
            }
        }


        /// <summary>
        /// Message handler for components OnLog
        /// </summary>
        private static void OnProgramComponentLog(object sender, LogEventArgs e)
        {
            Logger.AddLog(e);
        }


        /// <summary>
        /// Message handler for logger on logs
        /// </summary>
        private static void OnLoggedEvents(object sender, IEnumerable<LogEventArgs> e)
        {
            foreach (var nextLog in e)
            {
                if (nextLog.Level >= Logger.LogLevelDisplay)
                    Console.WriteLine(nextLog.FormatLogForConsole());
            }
        }


        /// <summary>
        /// Parse program startup parameters from command line
        /// </summary>
        private static bool ParseProgramStartupParameters(string[] args, out BrainFlowInputParams input_params, out int board_id, out string demoFileName)
        {
            board_id = 0;
            demoFileName = "";
            input_params = new BrainFlowInputParams();

            if (args.Count() < 2)
            {
                SetDefaultSerialPort(input_params);

                return true;
            }
            else
            {
                board_id = parse_args(args, input_params, out demoFileName);

                if (board_id < -1 && demoFileName.Length == 0)
                {
                    Logger.AddLog(new LogEventArgs(main, "ParseProgramStartupParameters", $"Invalid startup parameters.", LogLevel.ERROR));
                    return false;
                }

                if ( board_id > -1 && input_params.serial_port == "")
                {
                    SetDefaultSerialPort(input_params);
                }

                return true;
            }
        }


        /// <summary>
        /// Set default serial port for the platform
        /// </summary>
        /// <param name="input_params"></param>
        private static void SetDefaultSerialPort(BrainFlowInputParams input_params)
        {
            if (PlatformHelper.GetLibraryEnvironment() == LibraryEnvironment.Linux)
            {
                input_params.serial_port = "/dev/ttyUSB0";
            }
            else
            {
                input_params.serial_port = "COM3";
            }
        }


        /// <summary>
        /// Parse command line arguments from brainflow sample
        /// </summary>
        static int parse_args(string[] args, BrainFlowInputParams input_params, out string demoFileName)
        {
            demoFileName = "";
            int board_id = -99; //  invalid board id

            // use docs to get params for your specific board, e.g. set serial_port for Cyton
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("--ip-address"))
                {
                    input_params.ip_address = args[i + 1];
                }
                if (args[i].Equals("--mac-address"))
                {
                    input_params.mac_address = args[i + 1];
                }
                if (args[i].Equals("--serial-port"))
                {
                    input_params.serial_port = args[i + 1];
                }
                if (args[i].Equals("--other-info"))
                {
                    input_params.other_info = args[i + 1];
                }
                if (args[i].Equals("--ip-port"))
                {
                    input_params.ip_port = Convert.ToInt32(args[i + 1]);
                }
                if (args[i].Equals("--ip-protocol"))
                {
                    input_params.ip_protocol = Convert.ToInt32(args[i + 1]);
                }
                if (args[i].Equals("--board-id"))
                {
                    board_id = Convert.ToInt32(args[i + 1]);
                }
                if (args[i].Equals("--timeout"))
                {
                    input_params.timeout = Convert.ToInt32(args[i + 1]);
                }
                if (args[i].Equals("--serial-number"))
                {
                    input_params.serial_number = args[i + 1];
                }
                if (args[i].Equals("--file"))
                {
                    input_params.file = args[i + 1];
                }
                if (args[i].Equals("--demo-file"))
                {
                    demoFileName = args[i + 1];
                }
            }
            return board_id;
        }

        //  Object for program main function logging purposes
        static MainFunction main;
    }


    class MainFunction
    {
        public override string ToString()
        {
            return "Main";
        }
    }
}
