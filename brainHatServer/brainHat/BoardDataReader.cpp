#include <stdlib.h>
#include <iostream>
#include <unistd.h>
#include <sstream>
#include <iomanip>
#include <sys/time.h>
#include <math.h>
#include <chrono>

#include "brainHat.h"
#include "BoardDataReader.h"
#include "StringExtensions.h"
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
int BoardDataReader::Start(int board_id, struct BrainFlowInputParams params)
{
	BoardParamaters = params;
	BoardId = board_id;
	
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
	
	switch (BoardId)
	{
	case 0:
		if (board == 0)
			return BoardSettings.Boards[0]->Srb1Set;
		break;
		
	case 2:
		if (board < 2 && BoardSettings.Boards.size() > 1)
			return BoardSettings.Boards[board]->Srb1Set;
		break;
	}
	return -1;
}


//  Public function to set SRB1 state
bool BoardDataReader::RequestSetSrb1(int board, bool enable)
{
	if (!BoardSettings.HasValidSettings() && BoardSettings.Boards.size() < board)
		return false;
	
	auto channelSettings = BoardSettings.Boards[board]->Channels.front();
	if (CommandsQueueLock.try_lock_for(chrono::milliseconds(1000)))
	{
		//    string settingsString = $"x{settings.ChannelNumber.ChannelSetCharacter()}{settings.PowerDown.BoolCharacter()}{(int)(settings.Gain)}{(int)(settings.InputType)}{settings.Bias.BoolCharacter()}{settings.Srb2.BoolCharacter()}{(connect ? "1" : "0")}X";

		string settingsString = format("x%s%s%d%d%s%s%sX",
									ChannelSetCharacter(channelSettings->ChannelNumber).c_str(),
									BoolCharacter(channelSettings->PowerDown).c_str(),
									channelSettings->Gain,
									channelSettings->InputType,
									BoolCharacter(channelSettings->Bias).c_str(),
									BoolCharacter(channelSettings->Srb2).c_str(),
									BoolCharacter(enable).c_str());
		
		CommandsQueue.push(settingsString);

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
		
		if (!GetBoardConfiguration())
		{
			Logging.AddLog("BoardDataReader", "InitializeBoard", "Failed to get board configuration.", LogLevelError);
			if (Board->is_prepared())
			{
				Board->release_session();
			}
			return -1;
		}
		
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
//				char streamingArg[streamingFormat.size() + 1];
//				strcpy(streamingArg, streamingFormat.c_str());
//				streamingArg[streamingFormat.size() + 1] = 0x00;
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
		Logging.AddLog("BoardDataReader", "RunFunction", "Too long without valid sample. Reconnecting to board.", LogLevelError);
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
	switch (BoardId)
	{	
	case 0:
		return new Cyton8Sample(chunk, sampleCount);
	case 2:
		return new Cyton16Sample(chunk, sampleCount);
	case 1:
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
			Logging.AddLog("BoardDataReader", "ProcessCommandsQueue", format("Send to board: %s",nextCommand.c_str()), LogLevelInfo);
			Board->config_board((char*)nextCommand.c_str());
			usleep(50*USLEEP_MILI);
		}
		
		if (!GetBoardConfiguration())
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


//  Get Board Configuration
//  load the board settings with successful configuraiton string parsing
//
bool BoardDataReader::GetBoardConfiguration()
{
	Logging.AddLog("BoardDataReader", "GetBoardConfiguration", "Getting board configuration.", LogLevelDebug);
	
	string registersString = "";
	BoardSettings.ClearBoards();
	if (GetRegistersString(registersString))
	{
		return BoardSettings.ReadFromRegisterString(registersString);
	}
	
	return false;
}


//  Get the cyton board registers string
//
bool BoardDataReader::GetRegistersString(std::string& registersString)
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