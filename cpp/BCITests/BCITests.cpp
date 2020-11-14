#include <iostream>
#include <stdlib.h>
#include <string>


#include <unistd.h>

#include "board_shim.h"
#include "SensorThread.h"

#include "data_filter.h"

using namespace std;

 
bool parse_args(int argc, char *argv[], struct BrainFlowInputParams *params, int *board_id);


//  TODO - remove
void print_head(double **data_buf, int num_channels, int num_data_points)
{
	std::cout << "Total Channels for this board: " << num_channels << std::endl;
	int num_points = (num_data_points < 5) ? num_data_points : 5;
	for (int i = 0; i < num_channels; i++)
	{
		std::cout << "Channel " << i << ": ";
		for (int j = 0; j < num_points; j++)
		{
			std::cout << data_buf[i][j] << ",";
		}
		std::cout << std::endl;
	}
}


int main(int argc, char *argv[])
{
	
	int res = 0;
	struct BrainFlowInputParams params;
	int board_id = 0;
	if (!parse_args(argc, argv, &params, &board_id))
	{
		return -1;
	}

	BoardShim::enable_dev_board_logger();
	BoardShim::set_log_level((int)LogLevels::LEVEL_ERROR);
	
	// TODOint res = 0;

	cout << "============================================================" << endl;
	cout << "Brainflow Cyton Board Sensor Readings" << endl;
	cout << "============================================================" << endl;
	
	SensorThread sensorThread;
	res = sensorThread.Start(board_id, params);
	if (res != 0)
	{
		BoardShim::log_message((int)LogLevels::LEVEL_ERROR, "Failed to start board streaming.");
		getchar();
		return res;
	}
	

	//int data_count = 0;
	while (true)
	{
		int input = getchar();
		sensorThread.RegisterConsoleInput();
		
		if (input == 'p')
			sensorThread.ToggleDisplayOn();
		else if (input == 'q')
			break;
		
	}
	
	
	sensorThread.Cancel();
	
	return 0;
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