#include <stdlib.h>
#include <iostream>
#include <unistd.h>
#include <sstream>
#include <iomanip>
#include <sys/time.h>
#include <math.h>
#include <chrono>


#include "brainHat.h"
#include "BoardDataSource.h"
#include "StringExtensions.h"
#include "BoardIds.h"


using namespace std;
using namespace chrono;

//  Board Data Source
//  Base class for board data reader and board file simulator
//
BoardDataSource::BoardDataSource()
{
	ConnectionChangedCallback = NULL;
	ConnectionChangedDelegate = NULL;
	NewSampleCallback = NULL;
	
	Init();
}


//  Initialize properties
//
void BoardDataSource::Init()
{
	BoardId = (int)BrainhatBoardIds::UNDEFINED;
	SampleRate = -1;
	DataRows = 0;

	ExgChannelCount = 0;
	AccelChannelCount = 0;
	OtherChannelCount = 0;
	AnalogChannelCount = 0;
	
	LastSampleIndex = -1;
	LastTimeStampSync = 0;
	CountMissingIndex = 0;
	
	NumberOfSamplesCounted = 0;
	InspectDataStreamLogTimer.Start();
}


//  Destructor
//
BoardDataSource::~BoardDataSource()
{
	
}


//  Regiter a C++ connection changed callback
//
void BoardDataSource::RegisterConnectionChangedDelegate(ConnectionChangedDelegateFn connectionChangedDel)
{
	ConnectionChangedDelegate = connectionChangedDel;
}


//  Board connection properties changed
//
void BoardDataSource::ConnectionChanged(BoardConnectionStates state, int boardId, int sampleRate)
{
	if (ConnectionChangedCallback != NULL)
		ConnectionChangedCallback(state, boardId, sampleRate);
	if (ConnectionChangedDelegate != NULL)
		ConnectionChangedDelegate(state, boardId, sampleRate);
}


//  Enable or disable the board
//  will signal the main program to take action required (such as turn on power to the board)
//
void BoardDataSource::EnableBoard(bool enable)
{
	if (BoardOn != enable)
	{
		//  toggle board to opposite state
		if (BoardOn)
			ConnectionChanged(PowerOff, BoardId, SampleRate);
		else
			ConnectionChanged(PowerOn, BoardId, SampleRate);
		
		BoardOn = enable;
	}
}


//  Inspect the data stream and report statistics on a regular basis
//
void BoardDataSource::InspectDataStream(BFSample* data)
{
	//DataInspecting.push_back(data->Copy());
	NumberOfSamplesCounted++;
	InspectSampleIndexDifference(data->SampleIndex);
	
	//  log data stream inspection every five seconds
	if(InspectDataStreamLogTimer.ElapsedMilliseconds() > 5000)
	{
		Logging.AddLog("BoardDataSource", "InspectDataStream", format("%s Read %d samples. %.0lf sps.", ReportSource().c_str(), NumberOfSamplesCounted, NumberOfSamplesCounted / InspectDataStreamLogTimer.ElapsedSeconds()), LogLevelTrace);
	
		InspectDataStreamLogTimer.Reset();
		NumberOfSamplesCounted = 0;
	
		if (CountMissingIndex > 0)
		{
			Logging.AddLog("BoardDataSource", "InspectDataStream", format("Missed %d samples in the last 5 seconds.", CountMissingIndex), LogLevelWarn);
			CountMissingIndex = 0;
		}
	}
}


//  Get the sample index difference accounting for roll over
//
int BoardDataSource::SampleIndexDifference(double nextIndex)
{
	int difference = -1;
	if (fabs((LastSampleIndex - nextIndex) - 255) < 0.1)
	{
		difference = 1;
	}
	else if (nextIndex < LastSampleIndex)
	{
		difference = (int)nextIndex + (255 - (int)LastSampleIndex);
	}
	else
	{
		difference = nextIndex - LastSampleIndex;
	}
	LastSampleIndex = nextIndex;
	return difference;
}


//  Check the sample index difference (different boards count by 1 or 2 ..?
//
void BoardDataSource::InspectSampleIndexDifference(double nextIndex)
{
	auto diff = SampleIndexDifference(nextIndex);
	
	switch (BoardId)
	{
	case 0:	//  Cyton
		{
			if (diff > 1)
				CountMissingIndex++;
		}
		break;
		
	case 2: //  Cyton+Daisy
		{
			if (diff > 2)
				CountMissingIndex++;
		}
		break;
		
	case 1:	//  Ganglion
		{	
			//  TODO
		}
		break;
		
	}
}