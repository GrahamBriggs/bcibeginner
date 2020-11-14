#include <iostream>
#include <algorithm>
#include "Logger.h"
#include "StringExtensions.h"
#include "BroadcastDataThread.h"
#include "SensorThread.h"
#include "CommandServerThread.h"
#include "OpenBciDataFile.h"
#include "FileSimulatorThread.h"


using namespace std;


Logger Logging;
BroadcastDataThread BroadcastData;
CommandServerThread ComServer;

//  The board
int Board_Id = -1;
struct BrainFlowInputParams BrainflowBoardParams;
SensorThread Board;

//  The Simulator
string DemoFileName = "";
FileSimulatorThread SimulatorThread;

bool parse_args(int argc, char *argv[], struct BrainFlowInputParams *params, int *board_id, string &demoFileName);
void RunBoardData();
void RunFileData();

//  Main function
//
int main(int argc, char *argv[])
{
	//  did we launch with command line args
	if (argc > 1)
	{
		//  parse the args
		if (!parse_args(argc, argv, &BrainflowBoardParams, &Board_Id, DemoFileName))
		{
			return -1;
		}
		
		//  for conveinence, set defaults in absense of command line args
		if(Board_Id >= 0 && BrainflowBoardParams.serial_number.size() == 0)
			BrainflowBoardParams.serial_port = "/dev/ttyUSB0";
		
		//  default demo file
		if(Board_Id < 0 && DemoFileName == "")
			DemoFileName = "/home/pi/Source/BCI/DataLogs/demoFile.txt";
	}
	else
	{
		// no args default is to run the built in demo file
		DemoFileName = "/home/pi/Source/BCI/DataLogs/demoFile.txt";
	}
	
	//  start program threads
	Logging.Start();
	BroadcastData.Start();
	ComServer.Start();
	
	//  start board or file simulator data
	if (Board_Id >= 0)
		RunBoardData();
	else
		RunFileData();
	
	// stop threads
	BroadcastData.Cancel();
	ComServer.Cancel();
	Logging.Cancel();
	
	return 0;
}


//  Run the program with data from the live board
//
void RunBoardData()
{	
	if (Board.Start(Board_Id, BrainflowBoardParams) != 0)
	{
		Logging.AddLog("main", "RunBoardData", "Unable to start board", LogLevelError);
		return;
	}

	Logging.AddLog("main", "RunBoardData", "Starting board data. Enter Q to quit.", LogLevelInfo);
	
	string input;
	while (true)
	{
		getline(cin, input);
		toUpper(input);
		
		if (input.compare("Q") == 0) 
		{
			Board.Cancel();
			break;
		}
		else if (input.compare("S") == 0)
		{
			Logging.ResetDisplay();
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
		
		if (input.compare("Q") == 0) 
		{
			SimulatorThread.Cancel();
			break;
		}
		else if (input.compare("S") == 0)
		{
			Logging.ResetDisplay();
		}
	}
}


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
		if (std::string(argv[i]) == std::string("--file-name"))
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