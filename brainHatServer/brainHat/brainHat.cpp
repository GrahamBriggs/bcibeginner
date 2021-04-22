#include <iostream>
#include <algorithm>
#include <unistd.h>
#include <lsl_cpp.h>
#include <net/if.h>
#include <ifaddrs.h>


#include "brainHat.h"
#include "Logger.h"
#include "StringExtensions.h"
#include "BroadcastData.h"
#include "BroadcastStatus.h"
#include "BoardDataReader.h"
#include "ContecDataReader.h"
#include "CommandServer.h"
#include "OpenBciDataFile.h"
#include "BDFFileWriter.h"
#include "BoardFileSimulator.h"
#include "TimeExtensions.h"
#include "Parser.h"
#include "UriParser.h"
#include "BoardIds.h"
#include "BrainHatFileWriter.h"




using namespace std;

//  Callback Functions
void BoardConnectionStateChanged(BoardConnectionStates state, int boardId, int sampleRate);
void NewSample(BFSample* sample);
bool HandleServerRequest(string request);

//  Program Components
Logger Logging;
BroadcastData DataBroadcaster;
list<BroadcastStatus*> StatusBroadcasters;
CommandServer ComServer(HandleServerRequest);

//  Data Source is either board or demo file reader
BoardDataSource* DataSource = NULL;

//  Command line arguments
int BoardId = 0;
bool RecordToUsb = true;
bool StartSrbOn = false;
string DemoFileName = "";
struct BrainFlowInputParams InputParams;


//  Data Reader
ContecDataReader ContecReader(BoardConnectionStateChanged, NewSample);
BoardDataReader BoardReader(BoardConnectionStateChanged, NewSample);
BoardFileSimulator DemoFileReader(BoardConnectionStateChanged, NewSample);


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
void StartStatusBroadcast();
void StopStatusBroadcast();


//  Main function
//
int main(int argc, char *argv[])
{
	if (!ParseArguments(argc, argv))
		return -1;

	//BoardShim::set_log_file((char*)"./brainflowLogs.txt");
	BoardShim::set_log_level(6);
	
	//  start logging thread
	//  todo logging is disconnected for Contec board console output
	if((BrainhatBoardIds)BoardId != BrainhatBoardIds::CONTEC_KT88)  
		Logging.Start() ;
	
	ComServer.Start();
	StartStatusBroadcast();
	
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
	StopStatusBroadcast();
	ComServer.Cancel();
	Logging.Cancel();
	
	return 0;
}

void RunBrainflowBoardData()
{
	Logging.AddLog("main", "RunBrainflowBoardData", "Starting board data. Enter Q to quit.", LogLevelInfo);
	
	BoardReader.Start(BoardId, InputParams, StartSrbOn);
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

void RunContecBoardData()
{
	Logging.AddLog("main", "RunContecBoardData", "Starting board data. Enter Q to quit.", LogLevelInfo);
	
	ContecReader.Start(BoardId, InputParams, StartSrbOn);
	string input;
	while (true)
	{
		getline(cin, input);
		toUpper(input);
		
		if (!ProcessKeyboardInput(input))
		{
			ContecReader.Cancel();
			break;
		}
	}
}

//  Run the program with data from the live board
//
void RunBoardData()
{	
	
	switch ((BrainhatBoardIds)BoardId)
	{
	default:
		RunBrainflowBoardData();
		
	case BrainhatBoardIds::CONTEC_KT88:
		RunContecBoardData();
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

	if (IsRecording())
		FileWriter->AddData(sample->Copy());
	
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
			DataBroadcaster.SetBoard(boardId, sampleRate);
		}
		break;
		
	case PowerOff:	//  power On board
		{
		}
		break;
		
	case PowerOn:	//  power on board
		{
		}
		break;
		
	case Connected:	//  board connected
		{

		}
		break;
		
	case Disconnected: //  board disconnected	
		{

		}
		break;
	}
}


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
			
			int format = 1;
			if (formatType.compare("txt") == 0)
				format = 0;
			
			switch (format)
			{
			case 0:
				FileWriter = new OpenBCIFileWriter();
				break;
			case 1:
				FileWriter = new BDFFileWriter();
				break;
				
			default :
				return false;
			}
			
			
			FileWriter->StartRecording(fileName, RecordToUsb, BoardId, DataSource->GetSampleRate());
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


//  Handle callback from ComServer to process a request
//
bool HandleServerRequest(string request)
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
	else if (input.compare("b") == 0)
	{
		DataSource->EnableBoard(!DataSource->Enabled());
	}
	
	return true;
}

bool SupportedBoard()
{
	switch ((BrainhatBoardIds)BoardId)
	{
	default:
		return false;
		
	case BrainhatBoardIds::CYTON_BOARD:
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
	case BrainhatBoardIds::CONTEC_KT88:
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
		//  todo function for supported boards
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



typedef unsigned long uint32;

uint32 SockAddrToUint32(struct sockaddr * a)
{
	return ((a)&&(a->sa_family == AF_INET)) ? ntohl(((struct sockaddr_in *)a)->sin_addr.s_addr) : 0;
}

// convert a numeric IP address into its string representation
static void Inet_NtoA(uint32 addr, char * ipbuf)
{
	sprintf(ipbuf, "%li.%li.%li.%li", (addr >> 24) & 0xFF, (addr >> 16) & 0xFF, (addr >> 8) & 0xFF, (addr >> 0) & 0xFF);
}


void StartStatusBroadcast()
{

	struct ifaddrs * ifap;
	if (getifaddrs(&ifap) == 0)
	{
		struct ifaddrs * p = ifap;
		while (p)
		{
			uint32 ifaAddr  = SockAddrToUint32(p->ifa_addr);
			uint32 maskAddr = SockAddrToUint32(p->ifa_netmask);
			uint32 dstAddr  = SockAddrToUint32(p->ifa_dstaddr);
			if (ifaAddr > 0)
			{
				char ifaAddrStr[32]; Inet_NtoA(ifaAddr, ifaAddrStr);
				char maskAddrStr[32]; Inet_NtoA(maskAddr, maskAddrStr);
				char dstAddrStr[32]; Inet_NtoA(dstAddr, dstAddrStr);
				//printf("  Found interface:  name=[%s] desc=[%s] address=[%s] netmask=[%s] broadcastAddr=[%s]\n", p->ifa_name, "unavailable", ifaAddrStr, maskAddrStr, dstAddrStr);
				BroadcastStatus* newBroadcaster = new BroadcastStatus(p->ifa_name);
				newBroadcaster->Start();
				StatusBroadcasters.push_back(newBroadcaster);
			}
			p = p->ifa_next;
		}
		freeifaddrs(ifap);
	}
}


void StopStatusBroadcast()
{
	for (auto nextBroadcaster = StatusBroadcasters.begin(); nextBroadcaster != StatusBroadcasters.end(); ++nextBroadcaster)
	{
		(*nextBroadcaster)->Cancel();
		delete *nextBroadcaster;
	}
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
	}
	
	return true;
}


