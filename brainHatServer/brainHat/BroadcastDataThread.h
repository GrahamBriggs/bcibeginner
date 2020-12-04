#pragma once
#include <queue>
#include <condition_variable>
#include <lsl_cpp.h>

#include "Thread.h"
#include "BFSample.h"

//  UDP multicast thread for status broadcast
//
class BroadcastDataThread : public Thread
{
public:
	BroadcastDataThread();
	virtual ~BroadcastDataThread();
	
	void Start(int boardId, int sampleRate);
	
	virtual void RunFunction();
	
	void AddData(BFSample* data);
	
	bool LslEnabled;

protected:
	
	//  LSL
	void SetupLslForBoard();
	lsl::stream_outlet* LSLOutlet;
		
		
	int BoardId;
	int SampleRate;
	int SampleSize;
	
	std::string HostName;
	
	//  queue lock
	std::mutex QueueMutex;
	std::queue<BFSample*> SamplesQueue;
	
	
	int GetAvailableDataPort();
	
	void BroadcastData();
	
	
	
};

