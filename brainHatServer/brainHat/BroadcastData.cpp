#include <list>
#include <unistd.h>
#include <iostream>
#include <lsl_cpp.h>

#include "brainHat.h"
#include "BroadcastData.h"
#include "StringExtensions.h"
#include "TimeExtensions.h"
#include "wiringPi.h"
#include "json.hpp"
#include "NetworkAddresses.h"
#include "BFSample.h"
#include "BrainHatServerStatus.h"
#include "NetworkExtensions.h"
#include "BoardIds.h"



using namespace std;
using json = nlohmann::json;
using namespace lsl;




//  Broadcast data thread
//  Sends samples to the network using LSL
//
BroadcastData::BroadcastData()
{
	LslEnabled = true;
	LSLOutlet = NULL;
}


//  Destructor
//
BroadcastData::~BroadcastData()
{
	if(LSLOutlet != NULL)
		delete LSLOutlet;
}


//  Set Board properties, and kick off the broadcast thread
//
void BroadcastData::SetBoard(int boardId, int sampleRate)
{
	BoardId = boardId;
	SampleRate = sampleRate;
	HostName = GetHostName();
	
	SetupLslForBoard();
	
	Thread::Start();
}


//  Stream name for board
string StreamName(int boardId)
{
	switch ((BrainhatBoardIds)boardId)
	{
	case BrainhatBoardIds::CYTON_BOARD:
		return "Cyton8_BFSample";
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
		return "Cyton16_BFSample";
	case BrainhatBoardIds::CONTEC_KT88:
		return "ContecKT88_BFSample";
	default:
		return "BFSample";
	}
}


//  Stream name for board
string ManufacturerName(int boardId)
{
	switch ((BrainhatBoardIds)boardId)
	{
	case BrainhatBoardIds::CYTON_BOARD:
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
	case BrainhatBoardIds::GANGLION_BOARD:
		return "OpenBCI";
		
	case BrainhatBoardIds::CONTEC_KT88:
		return "Contec";
		
	default:
		return "Unknown";
	}
}


//  Configure the LSL stream for the specific board
//
void BroadcastData::SetupLslForBoard()
{
	int numChannels = getNumberOfExgChannels(BoardId);
	int accelChannels = getNumberOfAccelChannels(BoardId);
	int otherChannels = getNumberOfOtherChannels(BoardId);
	int analogChannels = getNumberOfAnalogChannels(BoardId);
	
	//  calculate sample size, is number of data elements plus time stamp plus sample index
	SampleSize = 2 + numChannels + accelChannels + otherChannels + analogChannels;
	
	lsl::stream_info info(StreamName(BoardId), "BFSample", SampleSize, SampleRate, lsl::cf_double64, HostName);
	
	auto response = lsl_library_info();
	cout << response;

	// add some description fields
	info.desc().append_child_value("manufacturer", ManufacturerName(BoardId));
	info.desc().append_child_value("boardId", format("%d", BoardId));
	lsl::xml_element chns = info.desc().append_child("channels");
	
	chns.append_child("channel")
		.append_child_value("label", "SampleIndex")
		.append_child_value("unit", "0-255")
		.append_child_value("type", "index");
	
	for (int k = 0; k < numChannels; k++)
		chns.append_child("channel")
		.append_child_value("label", format("ExgCh%d", k))
		.append_child_value("unit", "uV")
		.append_child_value("type", "EEG");
	
	for (int k = 0; k < accelChannels; k++)
		chns.append_child("channel")
		.append_child_value("label", format("AcelCh%d", k))
		.append_child_value("unit", "1.0")
		.append_child_value("type", "Accelerometer");
	
	for (int k = 0; k < otherChannels; k++)
		chns.append_child("channel")
		.append_child_value("label", format("Other%d", k));
	
	for (int k = 0; k < analogChannels; k++)
		chns.append_child("channel")
		.append_child_value("label", format("AngCh%d", k));
	
	chns.append_child("channel")
		.append_child_value("label", "TimeStamp")
		.append_child_value("unit", "s");

	// make a new outlet
	LSLOutlet = new lsl::stream_outlet(info);
}



//  Add data to the broadcast queue
//
void BroadcastData::AddData(BFSample* data)
{
	{
		LockMutex lockQueue(QueueMutex);
		SamplesQueue.push(data);
	}
}


//  Run function
//  sends out data from the queue
//
void BroadcastData::RunFunction()
{
	while (ThreadRunning)
	{		
		Sleep(1);
		BroadcastDataToLslOutlet();
	}
}



//  Broadcast the sample data to the LSL outlet
//
void BroadcastData::BroadcastDataToLslOutlet()
{
	int queueCount = SamplesQueue.size();
	if (queueCount == 0)
		return;
	
	double rawSample[SampleSize];
	
	//  empty the queue and put the samples to send into a list
	list<BFSample*> samples;
	{
		LockMutex lockQueue(QueueMutex);
		
		while (SamplesQueue.size() > 0)
		{
			samples.push_back(SamplesQueue.front());
			SamplesQueue.pop();
		}
	}
	
	//  broadcast the data
	for(auto nextSample = samples.begin() ; nextSample != samples.end() ; ++nextSample)
	{
		//  LSL broadcast
		if(LSLOutlet->have_consumers())
		{
			(*nextSample)->AsRawSample(rawSample);
			LSLOutlet->push_sample(rawSample);
		}
		
		delete(*nextSample);
	}
	
	//  monitor performance, generate warning any time the queue is backed up more than one second
	if(queueCount > SampleRate )
	{
		Logging.AddLog("BroadcastData", "BroadcastDataToLslOutlet", format("Broadcast is more than one second behind. Queue size %d", queueCount), LogLevelWarn);
	}
}

