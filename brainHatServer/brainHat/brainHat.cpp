#include <iostream>
#include <algorithm>
#include <unistd.h>
#include <lsl_cpp.h>


#include <wiringPi.h>
#include <list>

#include "brainHat.h"
#include "Logger.h"
#include "StringExtensions.h"
#include "BroadcastData.h"
#include "BroadcastStatus.h"
#include "BoardDataReader.h"
#include "CommandServer.h"
#include "OpenBciDataFile.h"
#include "BDFFileWriter.h"
#include "BoardFileSimulator.h"
#include "TimeExtensions.h"
#include "NetworkExtensions.h"
#include "Parser.h"
#include "UriParser.h"
#include "BoardIds.h"
#include "BrainHatFileWriter.h"
#include "PinController.h"
#include "GpioControl.h"




using namespace std;

//  Callback Functions for component state changes
void OnBoardConnectionStateChanged(BoardConnectionStates state, int boardId, int sampleRate);
void OnLslConnectionStateChanged(bool connected);
void OnRecordingStateChanged(bool recording);

//  Callback function for sample received
void OnNewSample(BFSample* sample);

//  Callback function for TCPIP server request to process
bool OnServerRequest(string request);

//  Program Components
Logger Logging;
BroadcastData DataBroadcaster(OnLslConnectionStateChanged);
BroadcastStatus StatusBroadcaster;
CommandServer ComServer(OnServerRequest);

//  Command line arguments
int BoardId = 0;
bool RecordToUsb = true;
bool StartSrbOn = false;
string DemoFileName = "";
struct BrainFlowInputParams InputParams;


//  Data Source
BoardDataSource* DataSource = NULL;

//  Demo file reader
BoardFileSimulator DemoFileReader(OnBoardConnectionStateChanged, OnNewSample);

//  File Recorder
BrainHatFileWriter* FileWriter;
bool IsRecording() {return FileWriter != NULL && FileWriter->IsRecording();}

//  Program functions
bool ParseArguments(int argc, char *argv[]);
bool parse_args(int argc, char *argv[]);
void RunBoardData();
void RunFileData();
bool ProcessKeyboardInput(string input); 
bool LiveData() { return BoardId != (int)BrainhatBoardIds::UNDEFINED; }


//  Pin numbers for status LED
//  Default is zero (pins will not be activated), use command line arguments to specify one or both status lights
int PinNumberConnectionStatus = 0;
int PinNumberRecordingStatus = 0;
	


//  Main function
//
int main(int argc, char *argv[])
{	
	if (!ParseArguments(argc, argv))
		return -1;
	
	//  star the GPIO controller for status LEDs
	StartGpioController(PinNumberConnectionStatus, PinNumberRecordingStatus);
	ConnectionLightShowConnecting();
		
	//  init brainflow logging level to off
	//BoardShim::set_log_level(6);
	
	//  start logging thread
	Logging.Start() ;
	
	//  start the tcpip command server
	ComServer.Start();
	
	//  start board or file simulator data
	if(LiveData())
	{
		RunBoardData();
	}
	else
	{
		RunFileData();
	}
	
	//  shut down LED lights
	StopGpioController();

	// user quit, stop threads
	DataBroadcaster.Cancel();
	StatusBroadcaster.Cancel();
	ComServer.Cancel();
	Logging.Cancel();
	
	return 0;
}



//  Run the program with data from the live board
//
void RunBoardData()
{	
	switch ((BrainhatBoardIds)BoardId)
	{
	default:
		DataSource = new BoardDataReader(OnBoardConnectionStateChanged, OnNewSample);
		break;
	}
	
	DataSource->Start(BoardId, InputParams, StartSrbOn);
	
	string input;
	while (true)
	{
		getline(cin, input);
		toUpper(input);
		
		if (!ProcessKeyboardInput(input))
		{
			DataSource->Cancel();
			break;
		}
	}
}


//  Run the program with data from the demo file
//
void RunFileData()
{
	DataSource = &DemoFileReader;
	
	if (DemoFileReader.Start(DemoFileName) != 0)
	{
		Logging.AddLog("main", "RunFileData", "Unable to load file.", LogLevelError);
		getchar();
		return;
	}

	Logging.AddLog("main", "RunFileData", "Starting file data. Enter Q to quit.", LogLevelInfo);
	
	string input;
	while (true)
	{
		getline(cin, input);
		toUpper(input);
		
		if (!ProcessKeyboardInput(input))
		{
			DemoFileReader.Cancel();
			break;
		}
	}
}


//  Handle samples from the data source
void OnNewSample(BFSample* sample)
{
	if (IsRecording())
		FileWriter->AddData(sample->Copy());

	//  broadcast it
	DataBroadcaster.AddData(sample);
}


//  Handle callback from data source when board connection state changed
//  this will set the status lights for any board transition
//  and for new board discovery, call SetBoard( ) on the data source now that the sample rate is known
//
void OnBoardConnectionStateChanged(BoardConnectionStates state, int boardId, int sampleRate)
{
	switch (state) 
	{
	case New:	
		{
			BoardId = boardId;
			DataBroadcaster.SetBoard(boardId, sampleRate);
			StatusBroadcaster.StartBroadcast(boardId, sampleRate);
		}
		break;
		
	case Connected:	
		{
			ConnectionLightShowReady();
		}
		break;
		
	case Disconnected: 
		{
			ConnectionLightShowConnecting();
		}
		break;
		
	case StreamOn:
		{
			if (DataBroadcaster.HasClients())
				ConnectionLightShowConnected();
			else
				ConnectionLightShowReady();
		}
		break;
		
	case StreamOff:
		{
			ConnectionLightShowPaused();
		}
		break;
		
	case PowerOff:	//  power Off board
		{
		}
		break;
		
	case PowerOn:	//  power on board
		{
		}
		break;
	}
}


// Handle callback from LSL broadcaster regarding client connections
//
void OnLslConnectionStateChanged(bool state)
{
	if (state)
	{
		ConnectionLightShowConnected();
	}
	else if (DataSource->GetIsStreamRunning())
	{
		ConnectionLightShowReady();
	}
	else if (DataSource->GetIsConnected())
	{
		ConnectionLightShowPaused();
	}
	else
	{
		ConnectionLightShowConnecting();
	}
}


// Handle callback from file recorder on the state of the recording file
//
void OnRecordingStateChanged(bool recording)
{
	RecordingLight(recording);
}



//  Handle request to start/stop recording
//
bool HandleRecordingRequest(UriArgParser& requestParser)
{
	if (DataSource != NULL)
	{
		auto fileName = requestParser.GetArg("filename");
		auto enable = requestParser.GetArg("enable");
	
		if (enable == "true")
		{
			if (FileWriter != NULL)
			{
				FileWriter->Cancel();
				delete FileWriter;
				FileWriter = NULL;
			}
			
			auto formatType = requestParser.GetArg("format");
			
			FileHeaderInfo info;
			info.SubjectName = requestParser.GetArg("name");
			info.SubjectCode = requestParser.GetArg("code");
			info.SubjectBirthday = requestParser.GetArg("birthday");
			info.SubjectAdditional = requestParser.GetArg("additional");
			info.SubjectGender = requestParser.GetArg("gender");
			info.AdminCode = requestParser.GetArg("admin");
			info.Technician = requestParser.GetArg("tech");
			
			int format = 1;
			if (formatType.compare("txt") == 0)
				format = 0;
			
			switch (format)
			{
			case 0:
				FileWriter = new OpenBCIFileWriter(OnRecordingStateChanged);
				break;
			case 1:
				FileWriter = new BDFFileWriter(OnRecordingStateChanged);
				break;
				
			default :
				return false;
			}
			
			
			FileWriter->StartRecording(fileName, RecordToUsb, BoardId, DataSource->GetSampleRate(), info);
		}
		else if (enable == "false")
		{
			if (IsRecording())
			{
				FileWriter->Cancel();
				delete FileWriter;
				FileWriter = NULL;
			}
		}
		
		return true;
	}
	
	return false;
}


//  Handle request to set SRB1
//
bool HandleSrbSetRequest(UriArgParser& requestParser)
{
	auto enable = requestParser.GetArg("enable");
	toUpper(enable);
	int board = ParseInt(requestParser.GetArg("board"));
	if (enable == "TRUE" && board > -1)
	{
		DataSource->RequestSetSrb1(board, true);
	}
	else if (enable == "FALSE" && board > -1)
	{
		DataSource->RequestSetSrb1(board, false);
	}
	else
	{
		return false;
	}
	
	return true;
}


//  Handle request to start / stop stream
//
bool HandleSetStreamRequest(UriArgParser& requestParser)
{
	auto enable = requestParser.GetArg("enable");
	toUpper(enable);
	if (enable == "TRUE")
	{
		DataSource->RequestEnableStreaming(true);
	}
	else if (enable == "FALSE")
	{
		DataSource->RequestEnableStreaming(false);
	}
	else
	{
		return false;
	}
	
	return true;
}


//  Handle request to set system time
//
bool HandleSetTimeRequest(UriArgParser& requestParser)
{
	bool result = false;
	auto timeString = requestParser.GetArg("time");
	if (timeString.length() > 0)
	{
		size_t sz;
		long long unixTimeMillis = std::stoll(timeString, &sz, 0);
		
		Logging.AddLog("main", "HandleSetTimeRequest", format("Setting system time. %lld", unixTimeMillis), LogLevelDebug);
		
		result = SetSystemTime(unixTimeMillis);
		if (result)
		{
			Logging.AddLog("main", "HandleSetTimeRequest", format("Set system time. %lld", unixTimeMillis), LogLevelInfo);
		}
		else
		{
			Logging.AddLog("main", "HandleSetTimeRequest", "Failed to set system time.", LogLevelError);
		}
	}
	
	return result;
}
	

//  Handle callback from ComServer to process a request
//
bool OnServerRequest(string request)
{
	UriArgParser requestParser(request);
	
	if (requestParser.GetRequest() == "recording")
	{
		//  request to start recording
		return HandleRecordingRequest(requestParser);
	}
	else if (requestParser.GetRequest() == "srbset")
	{
		return HandleSrbSetRequest(requestParser);
	}
	else if (requestParser.GetRequest() == "streamset")
	{
		return HandleSetStreamRequest(requestParser);
	}
	else if (requestParser.GetRequest() == "settime")
	{
		return HandleSetTimeRequest(requestParser);	
	}
	else
	{
		Logging.AddLog("main", "OnServerRequest", format("Invalid request %s",request.c_str()), LogLevelWarn);
		return false;
	}
}


//  Process keyboard input from the board read / demo file run loop
//
bool ProcessKeyboardInput(string input)
{
	if (input.compare("Q") == 0) 
	{
		return false;
	}
	else if (input.compare("S") == 0)
	{
		Logging.ResetDisplay();
	}
	else if (input.compare("B") == 0)
	{
		DataSource->EnableBoard(!DataSource->Enabled());
	}
	else if (input.compare("C") == 0 && DataSource != NULL)
	{
		if (Logging.IsDisplayOutputEnabled())
		{
			
			Logging.PauseDisplayOutput();
			DataSource->EnableRawConsole(true);
		}
		else
		{
			Logging.ResumeDisplayOutput();
			DataSource->EnableRawConsole(false);
		}
	}
	
	return true;
}


//  Check if board ID is supported by the program
//
bool SupportedBoard()
{
	switch ((BrainhatBoardIds)BoardId)
	{
	default:
		return false;
		
	case BrainhatBoardIds::MENTALIUM:
	case BrainhatBoardIds::CYTON_BOARD:
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
		return true;
	}
}


//  Parse the command line arguments for the brainHat program
//
bool ParseArguments(int argc, char *argv[])
{
	if (argc > 1)
	{
		//  parse the args
		if(!parse_args(argc, argv))
		{
			cout << "Invalid startup parameters. Exiting program." << endl;
			getchar();
			return false;
		}
		
		//  for file data, check demo file name
		if(LiveData() && DemoFileName.size() > 0 )
		{
			BoardId = (int)BrainhatBoardIds::UNDEFINED;
		}
		
		//  for live data, check supported boards
		if(LiveData() && !SupportedBoard())
		{
			cout << "Invalid startup parameters. This board is not supported. Exiting program." << endl;
			getchar();
			return false;
		}
		
		//  default serial port if it was not specified
		if(LiveData() && InputParams.serial_port.size() == 0)
			InputParams.serial_port = "/dev/ttyUSB0";
	}
	else
	{
		// no command line args,  default to Cyton board on the default port
		BoardId = (int)BrainhatBoardIds::CYTON_BOARD;
		InputParams.serial_port = "/dev/ttyUSB0";
	}
	
	
	
	return true;
}


//  Parse the command line args from the brainflow sample
//
bool parse_args(int argc, char *argv[])
{
	for (int i = 1; i < argc; i++)
	{
		if (std::string(argv[i]) == std::string("--board-id"))
		{
			if (i + 1 < argc)
			{
				i++;
				BoardId = std::stoi(std::string(argv[i]));
			}
			else
			{
				std::cerr << "missed argument" << std::endl;
				return false;
			}
		}
		if (std::string(argv[i]) == std::string("--demo-file"))
		{
			if (i + 1 < argc)
			{
				i++;
				DemoFileName = std::string(argv[i]);
			}
			else
			{
				std::cerr << "missed argument" << std::endl;
				return false;
			}
		}
		if (std::string(argv[i]) == std::string("--srb-on"))
		{
			if (i + 1 < argc)
			{
				i++;
				if (std::string(argv[i]) == "true")
					StartSrbOn = true;
			}
			else
			{
				std::cerr << "missed argument" << std::endl;
				return false;
			}
		}
		if (std::string(argv[i]) == std::string("--rec-usb"))
		{
			if (i + 1 < argc)
			{
				i++;
				if (std::string(argv[i]) == "false")
					RecordToUsb = false;
			}
			else
			{
				std::cerr << "missed argument" << std::endl;
				return false;
			}
		}
		if (std::string(argv[i]) == std::string("--ip-address"))
		{
			if (i + 1 < argc)
			{
				i++;
				InputParams.ip_address = std::string(argv[i]);
			}
			else
			{
				std::cerr << "missed argument" << std::endl;
				return false;
			}
		}
		if (std::string(argv[i]) == std::string("--ip-port"))
		{
			if (i + 1 < argc)
			{
				i++;
				InputParams.ip_port = std::stoi(std::string(argv[i]));
			}
			else
			{
				std::cerr << "missed argument" << std::endl;
				return false;
			}
		}
		if (std::string(argv[i]) == std::string("--serial-port"))
		{
			if (i + 1 < argc)
			{
				i++;
				InputParams.serial_port = std::string(argv[i]);
			}
			else
			{
				std::cerr << "missed argument" << std::endl;
				return false;
			}
		}
		if (std::string(argv[i]) == std::string("--ip-protocol"))
		{
			if (i + 1 < argc)
			{
				i++;
				InputParams.ip_protocol = std::stoi(std::string(argv[i]));
			}
			else
			{
				std::cerr << "missed argument" << std::endl;
				return false;
			}
		}
		if (std::string(argv[i]) == std::string("--timeout"))
		{
			if (i + 1 < argc)
			{
				i++;
				InputParams.timeout = std::stoi(std::string(argv[i]));
			}
			else
			{
				std::cerr << "missed argument" << std::endl;
				return false;
			}
		}
		if (std::string(argv[i]) == std::string("--other-info"))
		{
			if (i + 1 < argc)
			{
				i++;
				InputParams.other_info = std::string(argv[i]);
			}
			else
			{
				std::cerr << "missed argument" << std::endl;
				return false;
			}
		}
		if (std::string(argv[i]) == std::string("--mac-address"))
		{
			if (i + 1 < argc)
			{
				i++;
				InputParams.mac_address = std::string(argv[i]);
			}
			else
			{
				std::cerr << "missed argument" << std::endl;
				return false;
			}
		}
		if (std::string(argv[i]) == std::string("--serial-number"))
		{
			if (i + 1 < argc)
			{
				i++;
				InputParams.serial_number = std::string(argv[i]);
			}
			else
			{
				std::cerr << "missed argument" << std::endl;
				return false;
			}
		}
		if (std::string(argv[i]) == std::string("--pin-conn"))
		{
			if (i + 1 < argc)
			{
				i++;
				PinNumberConnectionStatus = std::stoi(std::string(argv[i]));
			}
			else
			{
				std::cerr << "missed argument" << std::endl;
				return false;
			}
		}
		if (std::string(argv[i]) == std::string("--pin-rec"))
		{
			if (i + 1 < argc)
			{
				i++;
				PinNumberRecordingStatus = std::stoi(std::string(argv[i]));
			}
			else
			{
				std::cerr << "missed argument" << std::endl;
				return false;
			}
		}
	}
	
	return true;
}


