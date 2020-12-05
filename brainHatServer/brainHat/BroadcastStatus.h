#pragma once
#include <queue>
#include "UdpMulticastServerThread.h"
#include "TimeExtensions.h"


//  callback function for get board parameters
typedef void(*GetBoardParamsCallbackFn)(int&,int&);


//  UDP multicast thread for status broadcast
//
class BroadcastStatus : public UdpMulticastServerThread
{
public:
	BroadcastStatus(GetBoardParamsCallbackFn getBoardParams);
	virtual ~BroadcastStatus();
	
	virtual void Start();
	
	virtual void RunFunction();
	
		
protected:
	
	GetBoardParamsCallbackFn GetBoardParams;
	
	std::string HostName;
	std::string Eth0Address;
	std::string Wlan0Address;
	std::string Wlan0Mode;
	
	ChronoTimer BroadcastStatusTimer;
	ChronoTimer CheckIpConfigTimer;

	void SetIpConfig();
	
	void BroadcastStatusOverMulticast();
};

