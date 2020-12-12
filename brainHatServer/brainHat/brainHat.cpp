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
void GetBoardParams(int& boardId, int& sampleRate);
void NewSample(BFSample* sample);
bool HandleServerRequest(string request);

//  Program Components
Logger Logging;
BroadcastData BroadcastData;
BroadcastStatus BroadcastStatus(GetBoardParams);
CommandServer ComServer(HandleServerRequest);
GpioManager PinManager;

//  Board Data Reader
int Board_Id = -99;
struct BrainFlowInputParams BrainflowBoardParams;
BoardDataReader Board(BoardConnectionStateChanged, NewSample);

//  Board file simulator
string DemoFileName = "";
BoardFileSimulator SimulatorThread(BoardConnectionStateChanged, NewSample);

//  File Recorder
FileRecorder FileRecording;

//  Program functions
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
	if (argc > 1)
	{
		//  parse the args
		if (!parse_args(argc, argv, &BrainflowBoardParams, &Board_Id, DemoFileName))
		{
			return -1;
		}
		
		//  default serial port if it was not specified
		if(LiveData() && BrainflowBoardParams.serial_number.size() == 0)
			BrainflowBoardParams.serial_port = "/dev/ttyUSB0";
		
		//  default demo file if it was not 
		if(! LiveData() && DemoFileName == "")
		{
			cout << "Invalid startup parameters. This program only supports --board-id 0|1|2 (Cyton, Ganglion, Cyton+Daisy) Exiting program." << endl;
			getchar();
			return -1;
		}	
	}
	else
	{
		// no command line args,  default to Cyton board on the default port
		Board_Id = 0;
		BrainflowBoardParams.serial_port = "/dev/ttyUSB0";
	}
		
	//  start program threads
	Logging.Start();

	
	StartLights();
	BroadcastStatus.Start();
	ComServer.Start();
	
	//  start board or file simulator data
	if(Board_Id < 0)
		RunFileData();
	else
		RunBoardData();

	// stop threads
	BroadcastData.Cancel();
	BroadcastStatus.Cancel();
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
	
	if (Board.Start(Board_Id, BrainflowBoardParams) != 0)
	{
		Logging.AddLog("main", "RunBoardData", "Unable to start board", LogLevelError);
	}

	Logging.AddLog("main", "RunBoardData", "Starting board data. Enter Q to quit.", LogLevelInfo);
	
	string input;
	while (true)
	{
		getline(cin, input);
		toUpper(input);
		
		if (!ProcessKeyboardInput(input))
		{
			Board.Cancel();
			break;
		}
	}
}


//  Run the program with data from the demo file
//
void RunFileData()
{
	if (SimulatorThread.Start(DemoFileName) != 0)
	{
		Logging.AddLog("main", "RunFileData", "Unable to load file.", LogLevelError);
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
			SimulatorThread.Cancel();
			break;
		}
	}
}


//  Handle samples from the data source
void NewSample(BFSample* sample)
{
	//  broadcast it
	BroadcastData.AddData(sample->Copy());
	
	if (FileRecording.IsRecording())
		FileRecording.AddData(sample->Copy());
	
	//  log it to the file
	delete sample;
}



void BoardConnectionStateChanged(BoardConnectionStates state, int boardId, int sampleRate)
{
	switch (state) 
	{
	case New:
		PinManager.Mode = LightsSequence;
		BroadcastData.SetBoard(boardId, sampleRate);
		break;
		
	case PowerOff:	//  power On board
		PinManager.AllOff();
		break;
		
	case PowerOn:	//  power on board
		if(Board_Id > -1)
		{
			PinManager.PowerToBoard(true);
			Sleep(2000);
		}
		PinManager.Mode = LightsFlash;
		break;
		
	case Connected:	//  board connected
		PinManager.Mode = LightsSequence;
		break;
		
	case Disconnected: //  board disconnected	
		PinManager.Mode = LightsFlash;
		break;
	}
}


void GetBoardParams(int& boardId, int& sampleRate)
{
	if (Board_Id > -1)
	{
		boardId = Board.GetBoardId();
		sampleRate = Board.GetSampleRate();
	}
	else
	{
		boardId = SimulatorThread.GetBoardId();
		sampleRate = SimulatorThread.GetSampleRate();
	}
	
}



bool HandleServerRequest(string request)
{
	int i = 0;
	Parser requestParser(request, "?");
	string command = requestParser.GetNextString();
	
	if (command == "recording")
	{
		UriArgParser args(requestParser.GetNextString());
		
		auto fileName = args.GetValue("filename");
		auto enable = args.GetValue("enable");
		
		if (enable == "true")
		{
			if (FileRecording.IsRecording())
			{
				FileRecording.Cancel();
			}
			
			FileRecording.StartRecording(fileName, Board_Id, Board.GetSampleRate());
			
		}
		else if (enable == "false")
		{
			if (FileRecording.IsRecording())
				FileRecording.Cancel();
		}
	}
}


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
		Board.EnableBoard(!Board.Enabled());
	}
	
	return true;
}


#define HWADDR_len 6




//  Parse the command line args 
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


void StartLights()
{
	char host[1024];
	gethostname(host, 1024);
	
	string hostName = string(host);
	PinManager.Mode = LightsSequence;
	PinManager.StartThreadForHost(hostName);
	
}