#include <iostream>
#include <algorithm>
#include <unistd.h>
#include <lsl_cpp.h>

#include "brainHat.h"
#include "Logger.h"
#include "StringExtensions.h"
#include "BroadcastData.h"
#include "BroadcastStatus.h"
#include "BoardDataReader.h"
#include "CommandServer.h"
#include "OpenBciDataFile.h"
#include "BoardFileSimulator.h"
#include "GpioPinManager.h"
#include "TimeExtensions.h"
#include "Parser.h"
#include "UriParser.h"



using namespace std;

//  Callback Functions
void BoardConnectionStateChanged(BoardConnectionStates state, int boardId, int sampleRate);
void NewSample(BFSample* sample);
bool HandleServerRequest(string request);

//  Program Components
Logger Logging;
BroadcastData DataBroadcaster;
BroadcastStatus StatusBroadcaster;
CommandServer ComServer(HandleServerRequest);
GpioManager PinManager;


//  Data Source is either board or demo file reader
BoardDataSource* DataSource = NULL;

//  Board Data Reader
int Board_Id = -99;
struct BrainFlowInputParams BrainflowBoardParams;
BoardDataReader BoardReader(BoardConnectionStateChanged, NewSample);

//  demo file reader
string DemoFileName = "";
BoardFileSimulator DemoFileReader(BoardConnectionStateChanged, NewSample);

//  File Recorder
OpenBCIFileRecorder FileRecorder;

//  Program functions
bool ParseArguments(int argc, char *argv[]);
bool parse_args(int argc, char *argv[], struct BrainFlowInputParams *params, int *board_id, string &demoFileName);
void RunBoardData();
void RunFileData();
bool ProcessKeyboardInput(string input); 
void StartLights();
bool LiveData() { return Board_Id >= 0; }




//  Main function
//
int main(int argc, char *argv[])
{
	if (!ParseArguments(argc, argv))
		return -1;
		
	//  start program threads
	Logging.Start();
	StartLights();
	ComServer.Start();
	StatusBroadcaster.Start();
	//  DataBroadcaster thread is started in BoardConnectionStateChanged( ) when the new board parameters are discovered
	
	//  start board or file simulator data
	if(LiveData())
	{
		RunBoardData();
	}
	else
	{
		RunFileData();
	}

	// user quit, stop threads
	DataBroadcaster.Cancel();
	StatusBroadcaster.Cancel();
	ComServer.Cancel();
	Logging.Cancel();
	PinManager.Cancel();
	
	return 0;
}


//  Run the program with data from the live board
//
void RunBoardData()
{	
	PinManager.PowerToBoard(true);
	
	BoardReader.Start(Board_Id, BrainflowBoardParams);
	
	Logging.AddLog("main", "RunBoardData", "Starting board data. Enter Q to quit.", LogLevelInfo);
	
	string input;
	while (true)
	{
		getline(cin, input);
		toUpper(input);
		
		if (!ProcessKeyboardInput(input))
		{
			BoardReader.Cancel();
			break;
		}
	}
}


//  Run the program with data from the demo file
//
void RunFileData()
{
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
void NewSample(BFSample* sample)
{
	//  broadcast it
	DataBroadcaster.AddData(sample->Copy());
	
	if (FileRecorder.IsRecording())
		FileRecorder.AddData(sample->Copy());
	
	//  done with this sample
	delete sample;
}


//  Handle callback from data source when board connection state changed
//  this includes discovery of new board, 
//  which will call SetBoard( ) on the data source to kick off the reading thread
//
void BoardConnectionStateChanged(BoardConnectionStates state, int boardId, int sampleRate)
{
	switch (state) 
	{
	case New:
		{
			PinManager.Mode = LightsSequence;
			DataBroadcaster.SetBoard(boardId, sampleRate);
		}
		break;
		
	case PowerOff:	//  power On board
		{
			PinManager.AllOff();
		}
		break;
		
	case PowerOn:	//  power on board
		{
			if (LiveData())
			{
				PinManager.PowerToBoard(true);
				Sleep(2000);
			}
			PinManager.Mode = LightsFlash;
		}
		break;
		
	case Connected:	//  board connected
		{
			PinManager.Mode = LightsSequence;
		}
		break;
		
	case Disconnected: //  board disconnected	
		{
			PinManager.Mode = LightsFlash;
		}
		break;
	}
}



//  Handle callback from ComServer to process a request
//
bool HandleServerRequest(string request)
{
	UriArgParser requestParser(request);
	
	//  request to start recording
	if (DataSource != NULL && requestParser.GetRequest() == "recording")
	{
		auto fileName = requestParser.GetArg("filename");
		auto enable = requestParser.GetArg("enable");
		
		if (enable == "true")
		{
			if (FileRecorder.IsRecording())
			{
				FileRecorder.Cancel();
			}
			
			FileRecorder.StartRecording(fileName, Board_Id, DataSource->GetSampleRate());
		}
		else if (enable == "false")
		{
			if (FileRecorder.IsRecording())
				FileRecorder.Cancel();
		}
		
		return true;
	}
	else
	{
		Logging.AddLog("main", "HandleServerRequest", format("Invalid request %s",request.c_str()), LogLevelInfo);
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
	else if (input.compare("0") == 0)
	{
		PinManager.Mode = LightsOff;
	}
	else if (input.compare("1") == 0)
	{
		PinManager.Mode = LightsOn;
	}
	else if (input.compare("2") == 0)
	{
		PinManager.Mode = LightsFlash;
	}
	else if (input.compare("3") == 0)
	{
		PinManager.Mode = LightsSequence;
	}
	else if (input.compare("b") == 0)
	{
		DataSource->EnableBoard(!DataSource->Enabled());
	}
	
	return true;
}


//  Parse the command line arguments for the brainHat program
//
bool ParseArguments(int argc, char *argv[])
{
	if (argc > 1)
	{
		//  parse the args
		if(!parse_args(argc, argv, &BrainflowBoardParams, &Board_Id, DemoFileName))
		{
			cout << "Invalid startup parameters. Exiting program." << endl;
			getchar();
			return false;
		}
		
		//  for live data, check supported boards
		if(LiveData() && !(Board_Id == 0 || Board_Id == 1 || Board_Id == 2))
		{
			cout << "Invalid startup parameters. This program only supports --board-id 0|1|2 (Cyton, Ganglion, Cyton+Daisy) Exiting program." << endl;
			getchar();
			return false;
		}
		
		//  for file data, check demo file name
		if(!LiveData() && DemoFileName == "")
		{
			cout << "Invalid startup parameters. Empty demo file name." << endl;
			getchar();
			return false;
		}	
		
		//  default serial port if it was not specified
		if(LiveData() && BrainflowBoardParams.serial_number.size() == 0)
			BrainflowBoardParams.serial_port = "/dev/ttyUSB0";
	}
	else
	{
		// no command line args,  default to Cyton board on the default port
		Board_Id = 0;
		BrainflowBoardParams.serial_port = "/dev/ttyUSB0";
	}
	
	if (LiveData())
	{
		DataSource = &BoardReader;
	}
	else
	{
		DataSource = &DemoFileReader;
	}
	
	return true;
}


//  Parse the command line args from the brainflow sample
//
bool parse_args(int argc, char *argv[], struct BrainFlowInputParams *params, int *board_id, string &demoFileName)
{
	bool board_id_found = false;
	for (int i = 1; i < argc; i++)
	{
		if (std::string(argv[i]) == std::string("--board-id"))
		{
			if (i + 1 < argc)
			{
				i++;
				board_id_found = true;
				*board_id = std::stoi(std::string(argv[i]));
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
				demoFileName = std::string(argv[i]);
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
				params->ip_address = std::string(argv[i]);
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
				params->ip_port = std::stoi(std::string(argv[i]));
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
				params->serial_port = std::string(argv[i]);
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
				params->ip_protocol = std::stoi(std::string(argv[i]));
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
				params->timeout = std::stoi(std::string(argv[i]));
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
				params->other_info = std::string(argv[i]);
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
				params->mac_address = std::string(argv[i]);
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
				params->serial_number = std::string(argv[i]);
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


//  GPIO manager function kickoff
//
void StartLights()
{
	PinManager.Mode = LightsSequence;
	PinManager.StartThreadForHost();
	
}