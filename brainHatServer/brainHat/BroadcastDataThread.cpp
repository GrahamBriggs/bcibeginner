#include <list>
#include <unistd.h>
#include <iostream>

#include "brainHat.h"
#include "BroadcastDataThread.h"
#include "StringExtensions.h"
#include "CMDifconfig.h"
#include "CMDiwconfig.h"
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




//  Constructor
//
BroadcastDataThread::BroadcastDataThread()
{
	LslEnabled = true;
	LSLOutlet = NULL;
}


//  Destructor
//
BroadcastDataThread::~BroadcastDataThread()
{
	//  TODO - other LSL close?
	if(LSLOutlet != NULL)
		delete LSLOutlet;
}


//  Start the thread
//  make sure we can open the server socket for multicast before starting thread
void BroadcastDataThread::Start(int boardId, int sampleRate)
{
	HostName = GetHostName();

	BoardId = boardId;
	SampleRate = sampleRate;
	
	SetupLslForBoard();
	
	Thread::Start();
}




void BroadcastDataThread::SetupLslForBoard()
{
	int numChannels, accelChannels, otherChannels, analogChannels;
	BoardShim::get_exg_channels(BoardId, &numChannels);
	BoardShim::get_accel_channels(BoardId, &accelChannels);
	BoardShim::get_other_channels(BoardId, &otherChannels);
	BoardShim::get_analog_channels(BoardId, &analogChannels);
	
	SampleSize = 2 + numChannels + accelChannels + otherChannels + analogChannels;
	
	lsl::stream_info info("BrainHat", "BFSample", SampleSize, SampleRate, lsl::cf_double64, HostName);

	// add some description fields
	info.desc().append_child_value("manufacturer", "OpenBCI");
	info.desc().append_child_value("boardId", format("%d",BoardId));
	lsl::xml_element chns = info.desc().append_child("channels");
	
	chns.append_child("channel")
		.append_child_value("label", "SampleIndex")
		.append_child_value("unit", "0-255")
		.append_child_value("type", "index");
	
	for (int k = 0; k < numChannels; k++)
		chns.append_child("channel")
		.append_child_value("label", format("ExgCh%d",k))
		.append_child_value("unit", "uV")
		.append_child_value("type", "EEG");
	
	for (int k = 0; k < accelChannels; k++)
		chns.append_child("channel")
		.append_child_value("label", format("AcelCh%d",k))
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




void BroadcastDataThread::AddData(BFSample* data)
{
	{
		LockMutex lockQueue(QueueMutex);
		SamplesQueue.push(data);
	}
}


//  Run function
//
void BroadcastDataThread::RunFunction()
{
	unsigned int lastStatusBroadcast = millis();
	unsigned int lastDataBroadcast = millis();
	unsigned int lastIpSetup = millis();

	while (ThreadRunning)
	{		
	
		Sleep(10);
		
		BroadcastData();
	}
}



//  Broadcast the sample data
//
void BroadcastDataThread::BroadcastData()
{
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
		if (LSLOutlet->have_consumers())
		{
			(*nextSample)->AsRawSample(rawSample);
		
			LSLOutlet->push_sample(rawSample);
		}
		
		delete(*nextSample);
	}
}

