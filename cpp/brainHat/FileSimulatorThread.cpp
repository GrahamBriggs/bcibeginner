#include <stdlib.h>
#include <iostream>
#include <unistd.h>
#include <sstream>
#include <iomanip>
#include <sys/time.h>
#include <math.h>
#include <chrono>

#include "brainHat.h"
#include "FileSimulatorThread.h"
#include "StringExtensions.h"


#define USLEEP_MILI (1000)
#define USLEEP_SEC (1000000)
#define SENSOR_SLEEP (50 * USLEEP_MILI)

using namespace std;
using namespace chrono;

//  Constructor
//
FileSimulatorThread::FileSimulatorThread()
{
	
}


//  Destructor
//
FileSimulatorThread::~FileSimulatorThread()
{
	Cancel();
}


//  Thread Start
//
int FileSimulatorThread::Start(string fileName)
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
void FileSimulatorThread::Cancel()
{
	Thread::Cancel();
	
	for (auto it = DataRecords.begin(); it != DataRecords.end(); ++it)
		delete *it;
}


//  Thread Run Function
//
void FileSimulatorThread::RunFunction()
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
			OpenBciData* newData = new OpenBciData(*it);
			
			//  set the demo time = start time of simulator + delta time in test + number of times looped * duration
			newData->TimeStamp = realStartTime + (loopCounter*fileDuration) + ((*it)->TimeStamp - fileStartTime);
			
			
			//  broadcast the data
			BroadcastData.AddData(newData);
			
			//  calculate the delay to wait for next epoch
			usleep(((*it)->TimeStamp - previousTimeStamp) * 1000000.0);
			previousTimeStamp = (*it)->TimeStamp;
			
			//  TODO - occasional trace of running stats
		}
		
		loopCounter += 1.0;
		previousTimeStamp = fileStartTime;
	}
}



// Load OpenBCI format txt file into the demo data collecion
//
bool FileSimulatorThread::LoadFile(std::string fileName)
{
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
					continue;
				
				DataRecords.push_back(new OpenBciData(readLine));
			}
		}

		dataFile.close();
		
		return true;
	}
	
	return false;
}



