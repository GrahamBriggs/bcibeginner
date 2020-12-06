#include <list>
#include <unistd.h>

#include "brainHat.h"
#include "BroadcastStatus.h"
#include "BrainHatServerStatus.h"
#include "StringExtensions.h"
#include "TimeExtensions.h"
#include "json.hpp"
#include "NetworkAddresses.h"
#include "NetworkExtensions.h"


using namespace std;
using json = nlohmann::json;


//  Constructor
//
BroadcastStatus::BroadcastStatus(GetBoardParamsCallbackFn getBoardParams)
{
	SocketFileDescriptor = -1;	
	GetBoardParams = getBoardParams;
	
	Eth0Address = "";
	Wlan0Address = "";
	Wlan0Mode = "";
}


//  Destructor
//
BroadcastStatus::~BroadcastStatus()
{
}



//  Start the thread
//  make sure we can open the server socket for multicast before starting thread
void BroadcastStatus::Start()
{
	HostName = GetHostName();
	SetIpConfig();
	
	BroadcastStatusTimer.Start();
	CheckIpConfigTimer.Start(); 
	
	int port = OpenServerSocket(MULTICAST_STATUSPORT, MULTICAST_GROUPADDRESS);
	
	if (port < 0)
	{
		Logging.AddLog("BroadcastData", "Start", "Unable to open server socket port.", LogLevelFatal);
		return;
	}

	
	UdpMulticastServerThread::Start();
}



//  broadcast timeouts for different status properties
int BroadcastTimeoutStatus = (2 * 1000);
int CheckIpConfigTimeout = (60 * 1000);


void BroadcastStatus::SetIpConfig()
{	
	auto addresses = GetNetworkIp4Addresses();
	for (auto it = addresses.begin(); it != addresses.end(); ++it)
	{
		if (get<0>(*it).compare("eth0") == 0)
			Eth0Address = get<1>(*it);
		else if (get<0>(*it).compare("wlan0") == 0)
			Wlan0Address = get<1>(*it);
	}
	
	//  TODO - set wlan mode
}


//  Run function
//
void BroadcastStatus::RunFunction()
{
	while (ThreadRunning)
	{
		if (CheckIpConfigTimer.ElapsedMilliseconds() >= CheckIpConfigTimeout)
		{
			SetIpConfig();
			CheckIpConfigTimer.Reset();
		}
		
		if (BroadcastStatusTimer.ElapsedMilliseconds() >= BroadcastTimeoutStatus)
		{
			BroadcastStatusOverMulticast();
			BroadcastStatusTimer.Reset();
		}

		Sleep(1);
	}
}


//  Broadcast the hat status
//
void BroadcastStatus::BroadcastStatusOverMulticast()
{
	BrainHatServerStatus status;
	status.HostName = HostName;
	status.Eth0Address = Eth0Address;
	status.Wlan0Address  = Wlan0Address;
	status.Wlan0Mode = Wlan0Mode;
	status.LogPort = Logging.GetPortNumber();
	
	GetBoardParams(status.BoardId, status.SampleRate);
	
	status.UnixTimeMillis = GetUnixTimeMilliseconds();
	
	string networkString = format("networkstatus?hostname=%s&status=%s\n", HostName.c_str(), status.AsJson().dump().c_str());
				
	WriteMulticastString(networkString);	
}


