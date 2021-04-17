#include <stdlib.h>
#include <iostream>
#include <unistd.h>
#include <sstream>
#include <iomanip>
#include <sys/time.h>
#include <math.h>
#include <chrono>
#include <wiringPi.h>

#include "brainHat.h"
#include "BoardDataReader.h"
#include "StringExtensions.h"
#include "BoardIds.h"
#include "BFCyton8.h"
#include "BFCyton16.h"
#include "CytonBoardConfiguration.h"
#include "board_controller.h"

#define SENSOR_SLEEP (50)

using namespace std;
using namespace chrono;

void DeleteChunk(double** chunk, int rows);

//  Board Data Reader, reads data from the board
//  Construct with callback functions:
//    -  ConnectionChanged will be called on first discovery of board parameters, then on connect / disconnect state
//    -  NewSample will be called when new data is read from the board
//
BoardDataReader::BoardDataReader(ConnectionChangedCallbackFn connectionChangedFn, NewSampleCallbackFn newSampleFn)
{
	Init();

	ConnectionChangedCallback = connectionChangedFn;
	NewSampleCallback = newSampleFn;
}


//  Destructor
//
BoardDataReader::~BoardDataReader()
{
	Cancel();
}


//  Initialize properties
//
void BoardDataReader::Init()
{
	Board = NULL;
	BoardOn = true;
	IsConnected = false;
	StreamRunning = false;
	ConnectionChangedCallback  = NULL;
	ConnectionChangedDelegate = NULL;
	InvalidSampleCounter = 0;
	
	BoardDataSource::Init();
}


//  Describe the source of the data
//
string BoardDataReader::ReportSource()
{
	return format("Board id %d at %d Hz.", BoardId, SampleRate);
}


//  Board object is created and session is prepared
//
bool BoardDataReader::BoardReady()
{
	return (Board != NULL && Board->is_prepared());
}


//  Thread Start
//
int BoardDataReader::Start(int boardId, struct BrainFlowInputParams params, bool srb1On)
{
	BoardParamaters = params;
	BoardId = boardId;
	
	//  TODO - clean up SRB settings
	StartSrb1CytonSet = (boardId == (int)BrainhatBoardIds::CYTON_BOARD || boardId == (int)BrainhatBoardIds::CYTON_DAISY_BOARD) && srb1On;
	StartSrb1DaisySet = boardId == (int)BrainhatBoardIds::CYTON_DAISY_BOARD && srb1On;
	
	LastSampleIndex = -1;
	
	
	BoardDataSource::Start();
	
	return 0;
}


//  Thread Cancel
//  
void BoardDataReader::Cancel()
{
	Thread::Cancel();
	
	ReleaseBoard();
}



//  Public function to get current SRB1 state for specified board
//
int BoardDataReader::GetSrb1(int board)
{
	if (!BoardSettings.HasValidSettings())
		return -1;
	
	switch ((BrainhatBoardIds)BoardId)
	{
	case BrainhatBoardIds::CYTON_BOARD:
		if (board == 0)
			return BoardSettings.Boards[0]->Srb1Set;
		break;
		
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
		if (board < 2 && BoardSettings.Boards.size() > 1)
			return BoardSettings.Boards[board]->Srb1Set;
		break;
	}
	return -1;
}


string FormatSrb1Command(CytonChannelSettings* channelSettings, bool enable)
{
	auto settingsString = format("x%s%s%d%d%s%s%sX",
		ChannelSetCharacter(channelSettings->ChannelNumber).c_str(),
		BoolCharacter(channelSettings->PowerDown).c_str(),
		channelSettings->Gain,
		channelSettings->InputType,
		BoolCharacter(channelSettings->Bias).c_str(),
		BoolCharacter(channelSettings->Srb2).c_str(),
		BoolCharacter(enable).c_str());
	
	return settingsString;
}

//  Public function to set SRB1 state
bool BoardDataReader::RequestSetSrb1(int board, bool enable)
{
	if (!BoardSettings.HasValidSettings() || BoardSettings.Boards.size() < board || BoardSettings.Boards[board]->Channels.size() < 1)
	{
		Logging.AddLog("BoardDataReader", "RequestSetSrb1", "Invalid parameters for request set SRB1", LogLevelError);
		return false;
	}
	
	//   this is the board configuration index, not the board ID
	switch (board)
	{
	case 0: 
		StartSrb1CytonSet = enable;
		break;
	case 1:
		StartSrb1DaisySet = enable;
		break;
	default:
		return false;	//  TODO ganglion 
	}
	
	auto channelSettings = BoardSettings.Boards[board]->Channels.front();
	if (CommandsQueueLock.try_lock_for(chrono::milliseconds(1000)))
	{		
		CommandsQueue.push(FormatSrb1Command(channelSettings, enable));

		CommandsQueueLock.unlock();
		return true;
	}

	return false;
}


//  Public funciton to toggle streaming
bool BoardDataReader::RequestEnableStreaming(bool enable)
{
	if ( (enable && ! StreamRunning) || (!enable && StreamRunning))
		RequestToggleStreaming = true;
	
	return true;
}




//  Initialize Board
//  creates a new Brainflow board object and starts streaming data
int BoardDataReader::InitializeBoard()
{
	int res = 0;
	
	ReleaseBoard();
	RequestToggleStreaming = false;
		
	try
	{
		Board = new BoardShim(BoardId, BoardParamaters);
	
		Board->prepare_session();
		Board->config_board((char*)"s");
		
		if (!LoadBoardRegistersSettings())
		{
			Logging.AddLog("BoardDataReader", "InitializeBoard", "Failed to get board configuration.", LogLevelError);
			if (Board->is_prepared())
			{
				Board->release_session();
			}
			return -1;
		}
		
		if (!InitializeSrbOnStartup())
			return -1;
		
		bool newConnection = SampleRate < 0;
		DataRows = BoardShim::get_num_rows(BoardId);
		SampleRate = BoardShim::get_sampling_rate(BoardId);
		
		StartStreaming();
		
		InitializeDataReadCounters();
		
		usleep(5 * USLEEP_SEC);

		ConnectionChanged(newConnection ? New : Connected, BoardId, SampleRate);
		DiscardFirstChunk();
		IsConnected = true;
		Logging.AddLog("BoardDataReader", "InitializeBoard", format("Connected to board %d. Sample rate %d", BoardId, SampleRate), LogLevelInfo);
	}
	catch (const BrainFlowException &err)
	{
		Logging.AddLog("BoardDataReader", "InitializeBoard", format("Failed to connect to board. Error %d %s.", err.exit_code, err.what()), IsConnected ?  LogLevelError : LogLevelDebug);
		res = err.exit_code;
		if (Board->is_prepared())
		{
			Board->release_session();
		}
	}
	return res;
}


//  Set the state of the SRB1 connected setting on startup initialization
//
bool BoardDataReader::InitializeSrbOnStartup()
{
	try
	{
		if (StartSrb1CytonSet || StartSrb1DaisySet)
		{
			if (StartSrb1CytonSet && BoardSettings.Boards.size() > 0)
			{
				Logging.AddLog("BoardDataReader", "InitializeSrbOnStartup", "Starting Cyton with SRB1 on.", LogLevelInfo);
				auto channelSettings = BoardSettings.Boards[0]->Channels.front();
				auto settingString = FormatSrb1Command(channelSettings, true);
				Board->config_board((char*)settingString.c_str());
			}
			else if (StartSrb1CytonSet)
			{
				Logging.AddLog("BoardDataReader", "InitializeSrbOnStartup", "Unable to set SRB1, invalid board configuration settings.", LogLevelError);
			}
			
			if (StartSrb1DaisySet && BoardSettings.Boards.size() > 1)
			{
				Logging.AddLog("BoardDataReader", "InitializeSrbOnStartup", "Starting Daisy with SRB1 on.", LogLevelInfo);
				auto channelSettings = BoardSettings.Boards[1]->Channels.front();
				auto settingString = FormatSrb1Command(channelSettings, true);
				Board->config_board((char*)settingString.c_str());
			}
			else if (StartSrb1DaisySet)
			{
				Logging.AddLog("BoardDataReader", "InitializeSrbOnStartup", "Unable to set SRB1, invalid board configuration settings.", LogLevelError);
			}
			
			
			if (!LoadBoardRegistersSettings())
			{
				Logging.AddLog("BoardDataReader", "InitializeSrbOnStartup", "Failed to get board configuration.", LogLevelError);
				if (Board->is_prepared())
				{
					Board->release_session();
				}
				return false;
			}
		}
		return true;
	}
	catch (const BrainFlowException &err)
	{
		Logging.AddLog("BoardDataReader", "InitializeSrbOnStartup", format("Failed to set SRB1. Error %d %s.", err.exit_code, err.what()), LogLevelError);
	}
	return false;
}




// Initialize the data reading monitor counters
//
void BoardDataReader::InitializeDataReadCounters()
{
	LastSampleIndex = -1;
	LastTimeStampSync = -1;
	ReadTimer.Start();
	InspectDataStreamLogTimer.Start(); 
}


//  Release Board
//  stops the session and deletes the board if it is initialized
//
void BoardDataReader::ReleaseBoard()
{
	if (Board != NULL)
	{
		try
		{			
			if (Board->is_prepared()) 
			{
				StopStreaming();
				Board->release_session();
			}
		}
		catch (const BrainFlowException &err)
		{
			Logging.AddLog("BoardDataReader", "ReleaseBoard", format("Failed to release to board. Error %d %s.", err.exit_code, err.what()), LogLevelError);
		}
		
		delete Board;
		Board = NULL;
		InvalidSampleCounter = 0;
		ConnectionChanged(Disconnected, BoardId, SampleRate);
	}
	
	IsConnected = false;
}


//  Start board streaming
//
void BoardDataReader::StartStreaming()
{
	if (!StreamRunning)
	{
		try
		{
			if (BoardParamaters.ip_address.length() > 0) 
			{
				string streamingFormat = format("streaming_board://%s:%d", BoardParamaters.ip_address.c_str(), BoardParamaters.ip_port);
				Logging.AddLog("BoardDataReader", "StartStreaming", format("Starting data stream %s",streamingFormat.c_str()), LogLevelInfo);
				Board->start_stream(50000, (char*)streamingFormat.c_str());
			}
			else
			{
				Logging.AddLog("BoardDataReader", "StartStreaming", "Starting data stream.", LogLevelInfo);
				Board->start_stream(50000);
			}
			
			StreamRunning = true;
		}
		catch (const BrainFlowException &err)
		{
			Logging.AddLog("BoardDataReader", "StartStreaming", format("Failed to start stream. Error %d %s.", err.exit_code, err.what()), LogLevelError);
		}
	}
}


//  Stop board streaming
//
void BoardDataReader::StopStreaming()
{
	if (StreamRunning)
	{
		try
		{
			Logging.AddLog("BoardDataReader", "StopStreaming", "Stopping data stream.", LogLevelInfo);
			Board->stop_stream();
			StreamRunning = false;
		}
		catch (const BrainFlowException &err)
		{
			Logging.AddLog("BoardDataReader", "StopStreaming", format("Failed to release to board. Error %d %s.", err.exit_code, err.what()), LogLevelError);
		}
	}
}



//  Reconnect to Board
//  tries to restart board streaming
//
void BoardDataReader::EstablishConnectionWithBoard()
{
	if (!BoardReady())
	{
		if (InitializeBoard() != 0)
			usleep(3*USLEEP_SEC);
	}
}
	

//  Check conditions are OK to read data from the board
//
bool BoardDataReader::PreparedToReadBoard()
{
	if (!BoardOn)
	{
		usleep(1*USLEEP_SEC);
		return false;
	}
	
	//  make sure we are connected to the board
	EstablishConnectionWithBoard();
			
	if (!BoardReady())
	{
		usleep(1*USLEEP_SEC);      
		return false;
	}
	else if (RequestToggleStreaming)
	{
		if (StreamRunning)
			StopStreaming();
		else
			StartStreaming();
		
		RequestToggleStreaming = false;
		return false;
	}
	else if (CommandsQueue.size() > 0)
	{
		ProcessCommandsQueue();
		return false;
	}
	else if (!StreamRunning)
	{
		usleep(1*USLEEP_SEC);
		return false;
	}
	else if ((InvalidSampleCounter * SENSOR_SLEEP) > 3000)
	{
		//  have not received fresh samples in three seconds, release board and reinitialize
		Logging.AddLog("BoardDataReader", "PreparedToReadBoard", "Too long without valid sample. Reconnecting to board.", LogLevelError);
		ReleaseBoard();
		usleep(1*USLEEP_SEC);
		return false;
	}
	
	return true;
}


//  Board reading / control thread run function
//
void BoardDataReader::RunFunction()
{
	double **chunk = NULL;
	int res = 0;
	int sampleCount = 0;
	
	while (ThreadRunning)
	{
		try
		{
			if (!PreparedToReadBoard())
				continue;
			
			//  read at our specified interval
			if(ReadTimer.ElapsedMilliseconds() > SENSOR_SLEEP)
			{
				ReadTimer.Reset();
					
				chunk = Board->get_board_data(&sampleCount);
				ProcessData(chunk, sampleCount);
		
				DeleteChunk(chunk, DataRows);
			}
		
			usleep(1*USLEEP_MILI);	
		}
		catch (const BrainFlowException &err)
		{
			Logging.AddLog("BoardDataReader", "RunFunction", err.what(), LogLevelError);
			ReleaseBoard();
			usleep(3*USLEEP_SEC);
		}
	}
}


// Process a chunk of data read from the board
// send to broadcast thread and logging if enabled
//
void BoardDataReader::ProcessData(double **chunk, int sampleCount)
{
	//  'improve' the time stamp to be more accurate
	double period, oldestSampleTime;
	CalculateReadingTimeThisChunk(chunk, sampleCount, period, oldestSampleTime);
	
	//  count the epochs where we have no data, will trigger a reconnect eventually		
	if(sampleCount == 0)
		InvalidSampleCounter++;
	else
		InvalidSampleCounter = 0;
	
	for (int i = 0; i < sampleCount; i++)
	{
		BFSample* sample = ParseRawData(chunk, i);
	
		//  fix the time stamp
		sample->TimeStamp = oldestSampleTime + ((i + 1)*period);
				
		//  inspect data stream
		InspectDataStream(sample);
		
		//  notify new sample
		NewSampleCallback(sample);
	}
}



//  Parse the raw data and create a sample type for this board
//
BFSample* BoardDataReader::ParseRawData(double** chunk, int sampleCount)
{
	switch ((BrainhatBoardIds)BoardId)
	{	
	case BrainhatBoardIds::CYTON_BOARD:
		return new Cyton8Sample(chunk, sampleCount);
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
		return new Cyton16Sample(chunk, sampleCount);
	case BrainhatBoardIds::GANGLION_BOARD:
		return NULL;	//  TODO Ganglion
	default :
		return NULL;
	}
}


//  Calculate the reading time this chunk
//  used to smooth out the sample times
//
void BoardDataReader::CalculateReadingTimeThisChunk(double** chunk, int samples, double& period, double& oldestSampleTime)
{
	auto timeNow = (chrono::duration_cast< milliseconds >(system_clock::now().time_since_epoch()).count() / 1000.0);	
		
	if (LastTimeStampSync > 0)
	{
		oldestSampleTime = LastTimeStampSync;
	}
	else
	{
		oldestSampleTime = timeNow - (samples / SampleRate);
	}
	
	LastTimeStampSync = timeNow;
		
	period = (timeNow - oldestSampleTime) / samples;
	
}


//  Read and discard the furst chunk of data on initial connection
//
void BoardDataReader::DiscardFirstChunk()
{
	try
	{
		int sampleCount = 0;
		auto chunk = Board->get_board_data(&sampleCount);
		DeleteChunk(chunk, DataRows);
	}
	catch (const BrainFlowException &err)
	{
		Logging.AddLog("BoardDataReader", "DiscardFirstChunk", format("Failed to read data from board. Error %d %s.", err.exit_code, err.what()), LogLevelError);
	}
}


void BoardDataReader::ProcessCommandsQueue()
{
	try
	{
		if (CommandsQueue.size() > 0)
		{
			CommandsQueueLock.lock();
	
			auto wasStreaming = StreamRunning;
			StopStreaming();
		
			BoardSettings.ClearBoards();
		
			while (CommandsQueue.size() != 0) 
			{
				string nextCommand = CommandsQueue.front();
				CommandsQueue.pop();
				Logging.AddLog("BoardDataReader", "ProcessCommandsQueue", format("Send to board: %s", nextCommand.c_str()), LogLevelInfo);
				Board->config_board((char*)nextCommand.c_str());
				usleep(50*USLEEP_MILI);
			}
		
			if (!LoadBoardRegistersSettings())
			{
				Logging.AddLog("BoardDataReader", "ProcessCommandsQueue", "Error restoring board configuration.", LogLevelError);
			}
		
			if (wasStreaming)
			{
				StartStreaming();
			}
		
			CommandsQueueLock.unlock();
		}
	}
	catch (const BrainFlowException &err)
	{
		Logging.AddLog("BoardDataReader", "ProcessCommandsQueue", format("Failed configure_board. Error %d %s.", err.exit_code, err.what()), LogLevelError);
	}
}


//  Get Board registers string and load board settings
//
bool BoardDataReader::LoadBoardRegistersSettings()
{
	Logging.AddLog("BoardDataReader", "LoadBoardRegistersSettings", "Getting board configuration.", LogLevelDebug);
	
	int retries = 0;
	while (retries < 10)
	{
		string registersString = "";
		BoardSettings.ClearBoards();
		if (GetRegistersString(registersString))
		{
			if (BoardSettings.ReadFromRegisterString(registersString))
			{
				return true;
			}
		}
		retries++;
	}
	
	return false;
}


//  Get the cyton board registers string
//
bool BoardDataReader::GetRegistersString(std::string& registersString)
{
	try
	{
		registersString = "";
		auto getRegisters = Board->config_board((char*)"?");
		if (!ValidateRegisterSettingsString(getRegisters))
		{
			Logging.AddLog("BoardDataReader", "GetRegistersString", format("Invalid registers string %s", getRegisters.c_str()), LogLevelError);
			registersString = getRegisters;
			return false;
		}

		auto version = Board->config_board((char*)"V");
		if (!ValidateFirmwareString(version))
		{
			Logging.AddLog("BoardDataReader", "GetRegistersString", format("Invalid firmware string %s", version.c_str()), LogLevelError);
			registersString = getRegisters;
			return false;
		}

		registersString = "Firmware: " + version + getRegisters;
		return true;
	}
	catch (const BrainFlowException &err)
	{
		Logging.AddLog("BoardDataReader", "GetRegistersString", format("Failed to get registers string. Error %d %s.", err.exit_code, err.what()), LogLevelError);
	}
	return false;
}

string Board_ADS_Registers = "Board ADS Registers";
string ThreeDollars = "$$$";

//  Check for valid registers string
//
bool BoardDataReader::ValidateRegisterSettingsString(string registerSettings)
{

	if (registerSettings.length() > Board_ADS_Registers.length())
	{
		auto checkString = registerSettings;
		removeTrailingCharacters(checkString, '\r');
		removeTrailingCharacters(checkString, '\n');
		removeLeadingCharacters(checkString, '\r');
		removeLeadingCharacters(checkString, '\n');
		
		if (checkString.substr(0, Board_ADS_Registers.length()) == Board_ADS_Registers && registerSettings.substr(registerSettings.length() - 3, ThreeDollars.length()) == "$$$")
			return true;
				
		if (registerSettings.substr(registerSettings.length() - 3, ThreeDollars.length()) == "$$$")
			return true;
	}
	return false;
}


//  Check for valid firmware string
//
bool BoardDataReader::ValidateFirmwareString(string firmware)
{
	if (firmware.length() > 3 && firmware.substr(0, 1) == "v" && firmware.substr(firmware.length() - 3, ThreeDollars.length()) == ThreeDollars)
		return true;
	return false;
}


//  Helper Functions
//

//  Delete double array 
void DeleteChunk(double** chunk, int rows)
{
	if (chunk != NULL)
	{
		for (int i = 0; i < rows; i++)
		{
			delete[] chunk[i];
		}
	}
	delete[] chunk;
}