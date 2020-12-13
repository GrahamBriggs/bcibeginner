#pragma once
#include <queue>
#include "UdpMulticastServerThread.h"
#include "TimeExtensions.h"



//  UDP multicast thread for status broadcast
//
class BroadcastStatus : public UdpMulticastServerThread
{
public:
	BroadcastStatus();
	virtual ~BroadcastStatus();
	
	virtual void Start();
	
	virtual void RunFunction();
	
		
protected:
	
	
	std::string HostName;
	std::string Eth0Address;
	std::string Wlan0Address;
	std::string Wlan0Mode;
	
	ChronoTimer BroadcastStatusTimer;
	ChronoTimer CheckIpConfigTimer;

	void SetIpConfig();
	
	void BroadcastStatusOverMulticast();
};

