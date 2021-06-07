#pragma once
#include <queue>
#include "TimeExtensions.h"
#include "Thread.h"


//  UDP multicast thread for status broadcast
//
class BroadcastStatus : public Thread
{
public:
	BroadcastStatus();
	virtual ~BroadcastStatus();

	void StartBroadcast(int boardId, int sampleRate);
	
	virtual void RunFunction();
	
		
protected:
	
	int BoardId;
	int SampleRate;
	void SetupLslForStatus();
	
	lsl::stream_outlet* LSLOutlet;
	
	std::string Interface;
	
	std::string HostName;
	std::string Eth0Address;
	std::string Wlan0Address;
	std::string Wlan0Mode;
	
	ChronoTimer BroadcastStatusTimer;
	ChronoTimer CheckIpConfigTimer;

	void SetIpConfig();
	
	void BroadcastStatusOverMulticast();
};

