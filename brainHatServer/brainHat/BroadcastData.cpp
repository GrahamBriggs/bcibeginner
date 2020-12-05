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




//  Constructor
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
	//  TODO - other LSL close?
	if(LSLOutlet != NULL)
		delete LSLOutlet;
}




void BroadcastData::SetBoard(int boardId, int sampleRate)
{
	BoardId = boardId;
	SampleRate = sampleRate;
	
	SetupLslForBoard();
	
	HostName = GetHostName();

	Thread::Start();
}


void BroadcastData::SetupLslForBoard()
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




void BroadcastData::AddData(BFSample* data)
{
	{
		LockMutex lockQueue(QueueMutex);
		SamplesQueue.push(data);
	}
}


//  Run function
//
void BroadcastData::RunFunction()
{
	while (ThreadRunning)
	{		
		Sleep(10);
		BroadcastDataToLslOutlet();
	}
}



//  Broadcast the sample data to the LSL outlet
//
void BroadcastData::BroadcastDataToLslOutlet()
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

