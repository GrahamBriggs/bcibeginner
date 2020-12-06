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


using namespace std;
using namespace chrono;

//  Constructor
//
BoardFileSimulator::BoardFileSimulator(ConnectionChangedCallbackFn connectionChangedFn)
{
	ConnectionChangedCallback = connectionChangedFn;
}


//  Destructor
//
BoardFileSimulator::~BoardFileSimulator()
{
	Cancel();
}



string BoardFileSimulator::ReportSource()
{
	return format("Reading data from file %s id %d at %d Hz.", FileName.c_str(), BoardId, SampleRate);
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
			//  TODO - this is hard coded expecting cyton 8 samples
			BFSample* nextSample = CopySample(*it);
			
			//  set the demo time = start time of simulator + delta time in test + number of times looped * duration
			nextSample->TimeStamp = realStartTime + ((*it)->TimeStamp - fileStartTime);
			LastTimeStampSync = nextSample->TimeStamp;
			
			//  broadcast the data
			BroadcastData.AddData(nextSample);
			InspectDataStream(nextSample);
			
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


//  TODO MoreBoards - finish this function
void BoardFileSimulator::AddSample(string readLine)
{
	switch ((BoardIds)BoardId)
	{
	case BoardIds::GANGLION_BOARD:
		break;
		
	case BoardIds::CYTON_BOARD:
		DataRecords.push_back(new Cyton8Sample(readLine));
		break;
		
	case BoardIds::CYTON_DAISY_BOARD:
		break;
		
	}

}


//  TODO MoreBoards - finish this function
BFSample* BoardFileSimulator::CopySample(BFSample* copy)
{
	switch ((BoardIds)BoardId)
	{
	case BoardIds::GANGLION_BOARD:
		break;
		
	case BoardIds::CYTON_BOARD:
		return new Cyton8Sample((Cyton8Sample*)copy);
		break;
		
	case BoardIds::CYTON_DAISY_BOARD:
		break;
		
	}
}




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
			BoardId = -99;
			break;
		}
	}	
}


