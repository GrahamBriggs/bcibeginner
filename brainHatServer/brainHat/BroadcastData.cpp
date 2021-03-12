#include <list>
#include <unistd.h>
#include <iostream>

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

#include <lsl_cpp.h>

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
	switch (boardId)
	{
	case 0:
		return "Cyton8_BFSample";
	case 2:
		return "Cyton16_BFSample";
	case 1:
		return "Ganglion_BFSample";
	default:
		return "BFSample";
	}
}


//  Configure the LSL stream for the specific board
//
void BroadcastData::SetupLslForBoard()
{
	int numChannels, accelChannels, otherChannels, analogChannels;
	BoardShim::get_exg_channels(BoardId, &numChannels);
	BoardShim::get_accel_channels(BoardId, &accelChannels);
	BoardShim::get_other_channels(BoardId, &otherChannels);
	BoardShim::get_analog_channels(BoardId, &analogChannels);
	
	SampleSize = 2 + numChannels + accelChannels + otherChannels + analogChannels;
	
	lsl::stream_info info(StreamName(BoardId), "BFSample", SampleSize, SampleRate, lsl::cf_double64, HostName);
	
	auto response = lsl_library_info();
	cout << response;

	// add some description fields
	info.desc().append_child_value("manufacturer", "OpenBCI");
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
	
	//  monitor performance, generate warning any time the queue is backed up more than two reads (we are reading at 20 hz)
	if(queueCount > SampleRate )
	{
		Logging.AddLog("BroadcastData", "BroadcastDataToLslOutlet", format("Broadcast is more than one second behind. Queue size %d", queueCount), LogLevelWarn);
	}
}

