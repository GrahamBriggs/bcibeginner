#include <stdlib.h>
#include <iostream>
#include <unistd.h>
#include <sstream>
#include <iomanip>
#include <sys/time.h>
#include <math.h>
#include <chrono>

#include "brainHat.h"
#include "SensorThread.h"
#include "StringExtensions.h"


#define USLEEP_MILI (1000)
#define USLEEP_SEC (1000000)
#define SENSOR_SLEEP (50 * USLEEP_MILI)

using namespace std;
using namespace chrono;

//  Constructor
//
SensorThread::SensorThread()
{
	BoardId = 0;
	Board = NULL;

	RecordsLogged = 0;
	
	NumInvalidPointsCounter = 0;
}


//  Destructor
//
SensorThread::~SensorThread()
{
	Cancel();
}


int LastSampleIndex;


//  Thread Start
//
int SensorThread::Start(int board_id, struct BrainFlowInputParams params)
{
	BoardParamaters = params;
	BoardId = board_id;
	LastSampleIndex = -1;
	
	int res = InitializeBoard();
	
	if (res == 0)
		Thread::Start();
	
	return res;
}




//  Thread Cancel
//  
void SensorThread::Cancel()
{
	Thread::Cancel();
	
	ReleaseBoard();
}


//  Release Board
//  stops the session and deletes the board if it is initialized
void SensorThread::ReleaseBoard()
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
		NumInvalidPointsCounter = 0;
	}
}


//  Initialize Board
//  creates a new Brainflow board object and starts streaming data
int SensorThread::InitializeBoard()
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
		
		usleep(7 * USLEEP_SEC);
	}
	catch (const BrainFlowException &err)
	{
		Logging.AddLog("SensorThread", "InitializeBoard", format("Failed to connect to board. Error %d.", err.exit_code), LogLevelError);
		Logging.AddLog("SensorThread", "InitializeBoard", string(err.what()), LogLevelWarn);
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
void SensorThread::ReconnectToBoard()
{
	Logging.AddLog("SensorThread", "ReconnectToBoard", "Lost connection to the board. Attempting to reconnect", LogLevelWarn);

	ReleaseBoard();
	
	if (InitializeBoard() != 0)
	{
		Logging.AddLog("SensorThread", "ReconnectToBoard", "Faled to reconnect to the board.", LogLevelWarn);
	}
}
	



//  Thread Run Function
//
void SensorThread::RunFunction()
{
	double **data = NULL;
	int res = 0;
	int num_rows = 0;
	int data_count = 0;
	
	while (Board != NULL && ThreadRunning)
	{
		try
		{
			//  approximately three seconds without data will trigger reconnect
			if(NumInvalidPointsCounter > 300)
			{
				ReconnectToBoard();
				continue;
			}
			
			data = Board->get_board_data(&data_count);
		
			//  count the epochs where we have no data, will trigger a reconnect eventually
			if(data_count == 0)
				NumInvalidPointsCounter++;
			else
				NumInvalidPointsCounter = 0;
				
			num_rows = BoardShim::get_num_rows(BoardId);

			SendDataToBroadcast(data, num_rows, data_count);
					
			if (data != NULL)
			{
				for (int i = 0; i < num_rows; i++)
				{
					delete[] data[i];
				}
			}
			delete[] data;
		
			usleep(SENSOR_SLEEP);	
		}
		catch (const BrainFlowException &err)
		{
			BoardShim::log_message((int)LogLevels::LEVEL_ERROR, err.what());
			
			//  this is the error code thrown when board read fails due to power outage
			if(err.exit_code == 15)
				NumInvalidPointsCounter += 101;	//  increment this by one hundred because we have one second sleep on this condition, and we want to trigger reconnect at two seconds of normal time
			
			usleep(1 * USLEEP_SEC);
		}
	}
}

int lastSampleIndex = 0;

double lastTimeStampSync = -1;

//  Save Data To File
//  saves a data buffer of data points to the file
//  note data buffer is rows of channels with columns of epochs
void SensorThread::SendDataToBroadcast(double **data_buf, int num_channels, int num_data_points)
{
	if (num_data_points > 500 || num_data_points == 0)
		return;	//  rush at startup, avoid dumping this onto the broadcast socket
	
	int num_points = num_data_points;
	
	//  fix the reading time
	double newestReadingTime = data_buf[22][num_data_points-1];
	double oldestReadingTime = data_buf[22][0];
	if (lastTimeStampSync > 0)
	{
		oldestReadingTime = lastTimeStampSync;
		lastTimeStampSync = newestReadingTime;
	}
	else
	{
		lastTimeStampSync = oldestReadingTime;
	}
	
	double period = (newestReadingTime - oldestReadingTime) / num_data_points;
	
	for (int i = 0; i < num_points; i++)
	{
		OpenBciData* data = new OpenBciData(data_buf, i);
		data->TimeStamp = oldestReadingTime + ((i+1)*period);
		BroadcastData.AddData(data);
		
		InspectDataStream(data);
	}
	
	lastTimeStampSync = newestReadingTime;
	
}


void InspectSampleIndex(OpenBciData* reading)
{
	if (LastSampleIndex < 0)
	{
		LastSampleIndex = (int)reading->SampleIndex;
		return;
	}
	else
	{
		switch (LastSampleIndex)
		{
		case 255:
			if ((int)reading->SampleIndex == 0)
			{
				LastSampleIndex = 0;
				return;
			}
			break;

		default:
			if ((int)reading->SampleIndex - LastSampleIndex == 1)
			{
				LastSampleIndex = (int)reading->SampleIndex;
				return;
			}
			break;
		}
	}

	Logging.AddLog("SensorThread", "InspectSampleIndex", format("Mismatch of sample index %d. Last Index %d. At reading %.6lf.", (int)reading->SampleIndex, LastSampleIndex, reading->TimeStamp), LogLevelWarn);
	LastSampleIndex = (int)reading->SampleIndex;
}

void SensorThread::InspectDataStream(OpenBciData* data)
{
	DataInspecting.push_back(new OpenBciData(data));
	InspectSampleIndex(data);
	
	//  update the display once a second
	if(duration_cast<milliseconds>(steady_clock::now() - LastLoggedTime).count() > 5000)
	{
		double averageTimeBetweenSamples = 0.0;	
		double sampleTimeHigh = 0.0;
		double sampleTimeLow = 1000000.0;
		
		OpenBciData* previousRecord = DataInspecting.front();
		for (auto it = DataInspecting.begin(); it != DataInspecting.end(); ++it)
		{
			if (it != DataInspecting.begin())
			{
				auto timeBetween = (*it)->TimeStamp - previousRecord->TimeStamp;
				averageTimeBetweenSamples += timeBetween;
				
				if (timeBetween > sampleTimeHigh)
					sampleTimeHigh = timeBetween;
				else if (timeBetween < sampleTimeLow)
					sampleTimeLow = timeBetween;
				
				previousRecord = *it;
			}
		}
		
		averageTimeBetweenSamples /= DataInspecting.size();
		Logging.AddLog("SensorThread", "InspectDataStream", format("Inspecting %d readings. %d readings per second.  Average time %.6lf s. Max time %.6lf s. Min time %.6lf s.", DataInspecting.size(), DataInspecting.size()/5,  averageTimeBetweenSamples, sampleTimeHigh, sampleTimeLow), LogLevelTrace);

		for (auto it = DataInspecting.begin(); it != DataInspecting.end(); ++it)
		{
			delete *it;
		}
		DataInspecting.clear();
		
		
		LastLoggedTime = steady_clock::now();
	}
}