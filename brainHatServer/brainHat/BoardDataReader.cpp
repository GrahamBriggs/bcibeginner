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


#define SENSOR_SLEEP (100)

using namespace std;
using namespace chrono;


//  Construct with C callback function for connection state change
//
BoardDataReader::BoardDataReader(ConnectionChangedCallbackFn connectionChangedFn, NewSampleCallbackFn newSampleFn)
{
	Init();
	BoardOn = true;
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
	ConnectionChangedCallback  = NULL;
	ConnectionChangedDelegate = NULL;
	InvalidSampleCounter = 0;
	
	BoardDataSource::Init();
}


//  Describe the source of the data
//
string BoardDataReader::ReportSource()
{
	return format("Reading data from board id %d at %d Hz.", BoardId, SampleRate);
}


//  Thread Start
//
int BoardDataReader::Start(int board_id, struct BrainFlowInputParams params)
{
	BoardParamaters = params;
	BoardId = board_id;
	LastSampleIndex = -1;
	
	int res = InitializeBoard();
	
	BoardDataSource::Start();
	
	return res;
}




//  Thread Cancel
//  
void BoardDataReader::Cancel()
{
	Thread::Cancel();
	
	ReleaseBoard();
}


//  Release Board
//  stops the session and deletes the board if it is initialized
void BoardDataReader::ReleaseBoard()
{
	if (Board != NULL)
	{
		if (Board->is_prepared())
		{
			Board->stop_stream();
			Board->release_session();
		}
		
		delete Board;
		Board = NULL;
		InvalidSampleCounter = 0;
		ConnectionChanged(Disconnected, BoardId, SampleRate);
	}
}


//  Initialize Board
//  creates a new Brainflow board object and starts streaming data
int BoardDataReader::InitializeBoard()
{
	int res = 0;
	
	ReleaseBoard();
	
	Board = new BoardShim(BoardId, BoardParamaters);
	BoardShim::set_log_level(LogLevel::LogLevelError);
	
	try
	{
		Board->prepare_session();
		Board->start_stream();
		
			
		// for STREAMING_BOARD you have to query information using board id for master board
		// because for STREAMING_BOARD data format is determined by master board!
		if(BoardId == (int)BoardIds::STREAMING_BOARD)
		{
			BoardId = std::stoi(BoardParamaters.other_info);
		}	
		
		bool newConnection = SampleRate < 0;
			
		LastSampleIndex = -1;
		LastTimeStampSync = -1;
		ReadTimer.Start();
		usleep(7 * USLEEP_SEC);
		DataRows = BoardShim::get_num_rows(BoardId);
		SampleRate = BoardShim::get_sampling_rate(BoardId);
		TimeStampIndex = BoardShim::get_timestamp_channel(BoardId);
		int channels;
		auto channelsArray = BoardShim::get_exg_channels(BoardId, &channels);
		
		InspectDataStreamLogTimer.Start(); 
		ConnectionChanged(newConnection ? New : Connected ,BoardId, SampleRate);
	}
	catch (const BrainFlowException &err)
	{
		Logging.AddLog("BoardDataReader", "InitializeBoard", format("Failed to connect to board. Error %d.", err.exit_code), LogLevelError);
		Logging.AddLog("BoardDataReader", "InitializeBoard", string(err.what()), LogLevelWarn);
		res = err.exit_code;
		if (Board->is_prepared())
		{
			Board->release_session();
		}
	}
	
	return res;
}


//  Reconnect to Board
//  tries to restart board streaming
void BoardDataReader::ReconnectToBoard()
{
	Logging.AddLog("BoardDataReader", "ReconnectToBoard", "Lost connection to the board. Attempting to reconnect", LogLevelWarn);

	ReleaseBoard();
	
	if (InitializeBoard() != 0)
	{
		Sleep(3000);
		Logging.AddLog("BoardDataReader", "ReconnectToBoard", "Faled to reconnect to the board.", LogLevelWarn);
	}
}
	



//  Thread Run Function
//
void BoardDataReader::RunFunction()
{
	double **chunk = NULL;
	int res = 0;
	int sampleCount = 0;
	
	while (Board != NULL && ThreadRunning)
	{
		if (!BoardOn)
		{
			Sleep(1000);
			continue;
		}
		
		try
		{
			//  approximately three seconds without data will trigger reconnect
			if(!Board->is_prepared() || (InvalidSampleCounter * SENSOR_SLEEP) > 3000)
			{
				ReconnectToBoard();
				continue;
			}
			
			//  read at our specified interval
			if (ReadTimer.ElapsedMilliseconds() > SENSOR_SLEEP)
			{
				ReadTimer.Reset();
			
				chunk = Board->get_board_data(&sampleCount);
		
				ProcessData(chunk, sampleCount);
					
				if (chunk != NULL)
				{
					for (int i = 0; i < DataRows; i++)
					{
						delete[] chunk[i];
					}
				}
				delete[] chunk;
			}
		
			usleep(1*USLEEP_MILI);	
		}
		catch (const BrainFlowException &err)
		{
			Logging.AddLog("BoardDataReader", "RunFunction", err.what(), LogLevelError);
			
			//  this is the error code thrown when board read fails due to power outage
			if(err.exit_code == 15)
				ReleaseBoard();
		}
	}
}


// Process a chunk of data read from the board
// send to broadcast thread and logging if enabled
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




BFSample* BoardDataReader::ParseRawData(double** chunk, int sampleCount)
{
	switch (BoardId)
	{	
	case 0:
		return new Cyton8Sample(chunk, sampleCount);
	case 2:
		return new Cyton16Sample(chunk, sampleCount);
	default:
		return NULL;
	}
}

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



