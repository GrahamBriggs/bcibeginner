#pragma once
#include <queue>
#include "UdpMulticastServerThread.h"
#include "OpenBciData.h"

//  UDP multicast thread for status broadcast
//
class BroadcastDataThread : public UdpMulticastServerThread
{
public:
	BroadcastDataThread();
	virtual ~BroadcastDataThread();
	
	virtual void Start();
	
	virtual void RunFunction();
	
	void AddData(OpenBciData* data);
	
protected:
	
	//  queue lock
	std::mutex QueueMutex;
	std::queue<OpenBciData*> DataQueue;
	
	void SetIpConfig();
	
	void BroadcastStatus();
	void BroadcastData();
	
};

