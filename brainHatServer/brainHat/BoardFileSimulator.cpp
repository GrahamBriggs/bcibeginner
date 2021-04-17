#include <stdlib.h>
#include <iostream>
#include <unistd.h>
#include <sstream>
#include <iomanip>
#include <sys/time.h>
#include <math.h>
#include <chrono>
#include <brainflow_constants.h>
#include "brainHat.h"
#include "BoardFileSimulator.h"
#include "StringExtensions.h"
#include "BFCyton8.h"
#include "BoardIds.h"


using namespace std;
using namespace chrono;

//  Board Demo File Reader, reads data from a demo file and simulates live board
//  Construct with callback functions:
//    -  ConnectionChanged will be called on first discovery of board parameters, then on connect / disconnect state
//    -  NewSample will be called when new data is read from the board
//
BoardFileSimulator::BoardFileSimulator(ConnectionChangedCallbackFn connectionChangedFn, NewSampleCallbackFn newSampleFn)
{
	ConnectionChangedCallback = connectionChangedFn;
	NewSampleCallback = newSampleFn;
}


//  Destructor
//
BoardFileSimulator::~BoardFileSimulator()
{
	Cancel();
}


//  Describe the source of the data
//
string BoardFileSimulator::ReportSource()
{
	return format("Demo File board %d at %d Hz.", FileName.c_str(), BoardId, SampleRate);
}



//  Thread Start
//
int BoardFileSimulator::Start(string fileName)
{
	if (LoadFile(fileName))
	{
		Thread::Start();
		return 0;
	}
	
	return 1;
}


//  Thread Cancel
//  
void BoardFileSimulator::Cancel()
{
	Thread::Cancel();
	
	for (auto it = DataRecords.begin(); it != DataRecords.end(); ++it)
		delete *it;
}


//  Thread Run Function
//
void BoardFileSimulator::RunFunction()
{
	//  we will broadcast the simulator data as if it started now
	double realStartTime = (duration_cast< seconds >(system_clock::now().time_since_epoch())).count();
	
	//  cache the time properties of the original data file
	double fileStartTime = DataRecords.front()->TimeStamp;
	double fileDuration = DataRecords.back()->TimeStamp - fileStartTime;
	double previousTimeStamp = fileStartTime;
	
	//  we will keep looping until we run out of double
	double loopCounter = 0.0;
	
	while (ThreadRunning)
	{
		for (auto it = DataRecords.begin(); it != DataRecords.end(); ++it)
		{
			if (!ThreadRunning)
				break;
			
			//  make new BCI data from the original
			BFSample* nextSample = (*it)->Copy();
			
			//  set the demo time = start time of simulator + delta time in test + number of times looped * duration
			nextSample->TimeStamp = realStartTime + ((*it)->TimeStamp - fileStartTime);
			LastTimeStampSync = nextSample->TimeStamp;
			
			InspectDataStream(nextSample);
			
			//  broadcast the data
			NewSampleCallback(nextSample);

			
			//  calculate the delay to wait for next epoch
			usleep(((*it)->TimeStamp - previousTimeStamp) * 1000000.0);
			previousTimeStamp = (*it)->TimeStamp;
			
			if(LastLoggedStatusTime.ElapsedMilliseconds() > 5000)
			{
				Logging.AddLog("BoardFileSimulator", "RunFunction", format("Reading raw data from %s. File time %.3lf.", FileName.c_str(), ((*it)->TimeStamp - fileStartTime)), LogLevelTrace);
				LastLoggedStatusTime.Reset();
			}
		}
		
		realStartTime = (duration_cast< seconds >(system_clock::now().time_since_epoch())).count();
		previousTimeStamp = realStartTime;
	}
}



// Load OpenBCI format txt file into the demo data collecion
//
bool BoardFileSimulator::LoadFile(std::string fileName)
{
	FileName = fileName;
	
	ifstream dataFile;
	dataFile.open(fileName);

	if (dataFile.is_open())
	{	
		string readLine;
		while (!dataFile.eof())
		{
			getline(dataFile, readLine);
			if (readLine.size() != 0)
			{
				if (readLine.substr(0, 1) == "%" || readLine.substr(0, 1) == "S")
					ReadHeaderLine(readLine);
				else
					AddSample(readLine);
			}
		}

		dataFile.close();
		
		ConnectionChanged(New, BoardId, SampleRate);
		return true;
	}
	return false;
}


//  Add a sample from data file text line
//  TODO - finish this function for all supported boards
void BoardFileSimulator::AddSample(string readLine)
{
	switch ((BrainhatBoardIds)BoardId)
	{
	case BrainhatBoardIds::GANGLION_BOARD:
		break;
		
	case BrainhatBoardIds::CYTON_BOARD:
		DataRecords.push_back(new Cyton8Sample(readLine));
		break;
		
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
		break;
		
	}
}




//  Read a header line to setup board parameters
//
void BoardFileSimulator::ReadHeaderLine(string readLine)
{
	if (readLine.substr(0, 12) == "%Sample Rate")
	{
		auto rateString = readLine.substr(12);
		Parser parseRate(rateString, " ");
		auto discard = parseRate.GetNextString();
		SampleRate = parseRate.GetNextInt();
	}
	else if (readLine.substr(0, 19) == "%Number of channels")
	{
		auto channelString = readLine.substr(19);
		Parser parseRate(channelString, " ");
		auto discard = parseRate.GetNextString();
		int channels = parseRate.GetNextInt();
		switch (channels)
		{
			
		case 4:
			BoardId = (int)BoardIds::GANGLION_BOARD;
			break;
			
		case 8:
			BoardId = (int)BoardIds::CYTON_BOARD;
			break;
			
		case 16:
			BoardId = (int)BoardIds::CYTON_DAISY_BOARD;
			break;
		default:
			BoardId = (int)BrainhatBoardIds::UNDEFINED;
			break;
		}
	}	
}



