#include <list>
#include <unistd.h>

#include "brainHat.h"
#include "BroadcastStatusThread.h"
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
BroadcastStatusThread::BroadcastStatusThread(GetBoardParamsCallbackFn getBoardParams)
{
	SocketFileDescriptor = -1;	
	GetBoardParams = getBoardParams;
	
	Eth0Address = "";
	Wlan0Address = "";
	Wlan0Mode = "";
}


//  Destructor
//
BroadcastStatusThread::~BroadcastStatusThread()
{
}



//  Start the thread
//  make sure we can open the server socket for multicast before starting thread
void BroadcastStatusThread::Start()
{
	HostName = GetHostName();
	SetIpConfig();
	
	BroadcastStatusTimer.Start();
	CheckIpConfigTimer.Start(); 
	
	int port = OpenServerSocket(MULTICAST_STATUSPORT, MULTICAST_GROUPADDRESS);
	
	if (port < 0)
	{
		Logging.AddLog("BroadcastDataThread", "Start", "Unable to open server socket port.", LogLevelFatal);
		return;
	}

	
	UdpMulticastServerThread::Start();
}



//  broadcast timeouts for different status properties
int BroadcastTimeoutStatus = (10 * 1000);
int CheckIpConfigTimeout = (60 * 1000);


void BroadcastStatusThread::SetIpConfig()
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
void BroadcastStatusThread::RunFunction()
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
			BroadcastStatus();
			BroadcastStatusTimer.Reset();
		}

		Sleep(1);
	}
}


//  Broadcast the hat status
//
void BroadcastStatusThread::BroadcastStatus()
{
	BrainHatServerStatus status;
	status.HostName = HostName;
	status.Eth0Address = Eth0Address;
	status.Wlan0Address  = Wlan0Address;
	status.Wlan0Mode = Wlan0Mode;
	status.LogPort = Logging.GetPortNumber();
	
	status.BoardId = Board.GetBoardId();
	status.SampleRate = Board.GetSampleRate();
	GetBoardParams(status.BoardId, status.SampleRate);
	
	status.UnixTimeMillis = GetUnixTimeMilliseconds();
	
	string networkString = format("networkstatus?hostname=%s&status=%s\n", HostName.c_str(), status.AsJson().dump().c_str());
				
	WriteMulticastString(networkString);	
}



