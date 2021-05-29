#pragma once
#include <queue>
#include <condition_variable>
#include <lsl_cpp.h>

#include "Thread.h"
#include "BFSample.h"

typedef void(*ClientConnectionChangedCallbackFn)(bool);

//  UDP multicast thread for status broadcast
//
class BroadcastData : public Thread
{
public:
	BroadcastData(ClientConnectionChangedCallbackFn fn);
	virtual ~BroadcastData();
	
	void SetBoard(int boardId, int sampleRate);
	
	virtual void RunFunction();
	
	void AddData(BFSample* data);
	
	bool HasClients() { return ClientsConnected; }
	bool LslEnabled;

protected:
	
	//  LSL
	void SetupLslForBoard();
	lsl::stream_outlet* LSLOutlet;
	bool ClientsConnected;
		
	int BoardId;
	int SampleRate;
	int SampleSize;
	
	std::string HostName;
	
	//  queue lock
	std::mutex QueueMutex;
	std::queue<BFSample*> SamplesQueue;
	
	
	int GetAvailableDataPort();
	
	void BroadcastDataToLslOutlet();
	
	ClientConnectionChangedCallbackFn ClientConnectionChangedCallback;
	
	
	
};

