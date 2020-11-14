#include <list>

#include "brainHat.h"
#include "BroadcastDataThread.h"
#include "StringExtensions.h"
#include "CMDifconfig.h"
#include "CMDiwconfig.h"
#include "TimeExtensions.h"
#include "wiringPi.h"
#include "json.hpp"
#include "NetworkAddresses.h"

using namespace std;
using json = nlohmann::json;




//  Constructor
//
BroadcastDataThread::BroadcastDataThread()
{
	SocketFileDescriptor = -1;	
}


//  Destructor
//
BroadcastDataThread::~BroadcastDataThread()
{
}


//  Start the thread
//  make sure we can open the server socket for multicast before starting thread
void BroadcastDataThread::Start()
{
	int port = OpenServerSocket(MULTICAST_DATAPORT, MULTICAST_GROUPADDRESS);
	
	if (port < 0)
	{
		Logging.AddLog("BroadcastDataThread", "Start", "Unable to open server socket port.", LogLevelFatal);
		return;
	}
	
	SetIpConfig();
	
	UdpMulticastServerThread::Start();
}


void BroadcastDataThread::AddData(OpenBciData* data)
{
	{
		LockMutex lockQueue(QueueMutex);
		DataQueue.push(data);
	}
	
}

//  broadcast timeouts for different status properties
int BroadcastTimeoutStatus = (10 * 1000);
int BroadcastTimeoutData = (100);


string eth0Address;
string wlanAddress;
string wlanStatus;


void BroadcastDataThread::SetIpConfig()
{
	CMDifconfig ifconfig;
	CMDiwconfig iwconfig;

	if (ifconfig.Execute() && iwconfig.Execute())
	{
		if (iwconfig.Mode() == iwcMaster)
			wlanStatus = format("wlanmode=%s&wlanssid=%s", iwconfig.ModeString().c_str(), iwconfig.Essid().c_str());
		else
			wlanStatus = format("wlanmode=%s", iwconfig.ModeString().c_str());
				
		eth0Address = ifconfig.Eth0Info().Inet4Address;
		wlanAddress = ifconfig.WlanInfo().Inet4Address;
	}
	else
	{
		Logging.AddLog("BroadcastDataThread", "SetIpConfig", "Failed to execute ifconfig or iwconfig.", LogLevelFatal);	
	}
}


//  Run function
//
void BroadcastDataThread::RunFunction()
{
	unsigned int lastStatusBroadcast = millis();
	unsigned int lastDataBroadcast = millis();

	while (ThreadRunning)
	{
		if (millis() - lastStatusBroadcast >= BroadcastTimeoutStatus)
		{
			BroadcastStatus();
			lastStatusBroadcast = millis();
		}
		
		if (millis() - lastDataBroadcast >= BroadcastTimeoutData)
		{
			BroadcastData();
			lastDataBroadcast = millis();
		}

		Sleep(5);
	}
}


//  Broadcast the hat status
//
void BroadcastDataThread::BroadcastStatus()
{
	string networkString = format("networkstatus?eth0=%s&wlan0=%s&%s&time=%lld\n", eth0Address.c_str(), wlanAddress.c_str(), wlanStatus.c_str(), GetUnixTimeMilliseconds());
				
	WriteMulticastString(networkString);	
}



//  Broadcast the hat data
//
void BroadcastDataThread::BroadcastData()
{
	//  empty the queue and put the events to send into a list
	list<OpenBciData*> data;
	{
		LockMutex lockQueue(QueueMutex);
		
		while (DataQueue.size() > 0)
		{
			OpenBciData* nextData = DataQueue.front();
			data.push_back(nextData);
			DataQueue.pop();
		}
	}
	
	//  broadcast this data
	json bciObservations = json::array();
	//	for (int i = 0; i <= 5; i++)
	//	{
	//		JsonObjects.append(JsonObject)
	//	}
		list<OpenBciData*>::iterator nextItem;
	for (nextItem = data.begin(); nextItem != data.end(); ++nextItem)
	{
		bciObservations.push_back((*nextItem)->AsJson());
		WriteMulticastString(format("rawData?data=%s\n", (*nextItem)->AsJson().dump().c_str()));
		
		delete(*nextItem);
	}
}

