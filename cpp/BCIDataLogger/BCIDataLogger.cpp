#include <iostream>
#include <stdlib.h>
#include <string>


#include <unistd.h>

#include "board_shim.h"
#include "SensorThread.h"
#include <unistd.h>#include <unistd.h>#include <un
using namespace std;

 
bool parse_args(int argc, char *argv[], struct BrainFlowInputParams *params, int *board_id);
void DoLogging();

SensorThread sensorThread;

int main(int argc, char *argv[])
{
	struct BrainFlowInputParams params;
	int board_id = 0;
	if (argc == 1)
	{
		params.serial_port = "/dev/ttyUSB0";
	}
	else
	{
		if (!parse_args(argc, argv, &params, &board_id))
		{
			return -1;
		}
	}
	
	//  setup board library and logging
	BoardShim::enable_dev_board_logger();
	BoardShim::set_log_level((int)LogLevels::LEVEL_DEBUG);
	
	//  start the sensor thread
	int res = 0;
	res = sensorThread.Start(board_id, params);
	if (res != 0)
	{
		BoardShim::log_message((int)LogLevels::LEVEL_ERROR, "Failed to start board streaming.");
		getchar();
		return res;
	}
	
	
	cout << "============================================================" << endl;
	cout << "OTTOMY Brainflow Cyton Board Data Logger " << endl;
	cout << "============================================================" << endl;
	cout << endl;
	cout << "Press 't' to start a test, q to quit." << endl;
	
	//int data_count = 0;
	while(true)
	{
		int input = getchar();
		
		if (input == 'q')
		{
			sensorThread.StopLogging();
			break;
		}
		else if (input == 't')
		{
			DoLogging();
		}
	}
	
	sensorThread.Cancel();
	
	return 0;
}


void DoLogging()
{
	cout << "Enter name for this test." << endl;
			
	string testName;
	cin >> testName;
			
	cout << "Enter 's' to start a log file in this test." << endl;
			
	while (true)
	{
		int input = getchar();
		if (input == 'q')
		{
			cout << "Ended test " << testName << endl;
			cout << "Press 't' to start a test, q to quit." << endl;
					
			sensorThread.StopLogging();
			break;
		}
		else if (input == 'e')
		{
			sensorThread.StopLogging();
			cout << "Stopped log file. Press s to start new log file in test " << testName << endl;
		}
		else if (input == 's')
		{
			sensorThread.StartLogging(testName);
			cout << "Press e to end log file." << endl;
		}
	}
}


bool parse_args(int argc, char *argv[], struct BrainFlowInputParams *params, int *board_id)
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
	if (!board_id_found)
	{
		std::cerr << "board id is not provided" << std::endl;
		return false;
	}
	return true;
}