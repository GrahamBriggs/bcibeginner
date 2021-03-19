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


#define SENSOR_SLEEP (50)

using namespace std;
using namespace chrono;


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



//  Initialize Board
//  creates a new Brainflow board object and starts streaming data
int BoardDataReader::InitializeBoard()
{
	int res = 0;
	
	ReleaseBoard();
	
	bool newConnection = SampleRate < 0;
	Board = new BoardShim(BoardId, BoardParamaters);
	BoardShim::set_log_level(LogLevel::LogLevelOff);
	DataRows = BoardShim::get_num_rows(BoardId);
	SampleRate = BoardShim::get_sampling_rate(BoardId);
	
	try
	{
		Board->prepare_session();
		
		StartStreaming();
		
		InitializeDataReadCounters();
		
		usleep(5 * USLEEP_SEC);

		ConnectionChanged(newConnection ? New : Connected ,BoardId, SampleRate);
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


void BoardDataReader::InitializeDataReadCounters()
{
	LastSampleIndex = -1;
	LastTimeStampSync = -1;
	ReadTimer.Start();
	InspectDataStreamLogTimer.Start(); 
}


//  Release Board
//  stops the session and deletes the board if it is initialized
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



void BoardDataReader::StartStreaming()
{
	if (! StreamRunning)
	{
		try
		{
			if (BoardParamaters.ip_address.length() > 0) 
			{
				string streamingFormat = format("streaming_board://%s:%d", BoardParamaters.ip_address.c_str(), BoardParamaters.ip_port);
				char streamingArg[streamingFormat.size() + 1];
				strcpy(streamingArg, streamingFormat.c_str());
				streamingArg[streamingFormat.size() + 1] = 0x00;
				Board->start_stream(50000, streamingArg);
			}
			else
			{
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


void BoardDataReader::StopStreaming()
{
	if (StreamRunning)
	{
		try
		{
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
void BoardDataReader::EstablishConnectionWithBoard()
{
	if (!BoardReady())
	{
		if (InitializeBoard() != 0)
			usleep(3*USLEEP_SEC);
	}
}
	



//  Thread Run Function
//
void BoardDataReader::RunFunction()
{
	double **chunk = NULL;
	int res = 0;
	int sampleCount = 0;
	
	while (ThreadRunning)
	{
		if (!BoardOn)
		{
			usleep(1*USLEEP_SEC);
			continue;
		}
		
		try
		{
			//  make sure we are connected to the board
			EstablishConnectionWithBoard();
			
			if( !BoardReady() || !StreamRunning )
			{
				usleep(1*USLEEP_SEC); //  if we are not prepared, or if the stream is disabled, wait a bit and try again
				continue;
			}
			else if((InvalidSampleCounter * SENSOR_SLEEP) > 3000)
			{
				//  have not received fresh samples in a while, release board and reinitialize
				Logging.AddLog("BoardDataReader", "RunFunction", "Too long without valid sample. Reconnecting to board.", LogLevelError);
				ReleaseBoard();
				usleep(1*USLEEP_SEC);
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
			
			ReleaseBoard();
			usleep(3*USLEEP_SEC);
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
	default:
		return NULL;
	}
}


//  Calculate the reading time this chunk
//  used to smooth out the sample times
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


void BoardDataReader::DiscardFirstChunk()
{
	try
	{
		int sampleCount = 0;
	
		auto chunk = Board->get_board_data(&sampleCount);
					
		if (chunk != NULL)
		{
			for (int i = 0; i < DataRows; i++)
			{
				delete[] chunk[i];
			}
		}
		delete[] chunk;
	}
	catch (const BrainFlowException &err)
	{
		Logging.AddLog("BoardDataReader", "DiscardFirstChunk", format("Failed to read data from board. Error %d %s.", err.exit_code, err.what()), LogLevelError);
	}
}


