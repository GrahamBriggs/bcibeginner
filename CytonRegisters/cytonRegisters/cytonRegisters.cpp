#include <iostream>
#include <algorithm>
#include <unistd.h>
#include <lsl_cpp.h>
#include <board_shim.h>



using namespace std;


//  Program functions
bool parse_args(int argc, char *argv[], struct BrainFlowInputParams *params, int *board_id);





//  Main function
//
int main(int argc, char *argv[])
{
	BrainFlowInputParams params;
	int boardId = 0;
	if (!parse_args(argc, argv, &params, &boardId))
	{
		params.serial_port = "/dev/ttyUSB0";
	}
		
	auto board = new BoardShim(boardId, params);
	
	BoardShim::set_log_level(0);
	
	try
	{
		board->prepare_session();
	
		if (board->is_prepared())
		{
			char send[1] = { '?' };
		
			string test = board->config_board(send);
	
			cout << "Response: " << test << " :End response." << endl;
		}
	}
	catch (const BrainFlowException &err)
	{
		
		auto res = err.exit_code;
		cout << "Error " << err.what() << endl;
		if (board->is_prepared())
		{
			board->release_session();
		}
	}
	
	return 0;
}



//  Parse the command line args from the brainflow sample
//
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
	
	return true;
}


