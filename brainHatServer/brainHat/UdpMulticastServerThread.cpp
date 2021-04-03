#include <netinet/in.h>
#include <unistd.h>
#include <string>
#include <arpa/inet.h>
#include <iostream>
#include <sstream>
#include <string.h>

#include "brainHat.h"
#include "UdpMulticastServerThread.h"
#include "StringExtensions.h"
#include <net/if.h>
#include <ifaddrs.h>

using namespace std;

UdpMulticastServerThread::UdpMulticastServerThread()
{
	PortNumber = -1;
	Group = "";
}

UdpMulticastServerThread::~UdpMulticastServerThread()
{
	if (ThreadRunning)
	{
		Cancel();
	}

	SocketFileDescriptor = -1;
}

void UdpMulticastServerThread::Cancel()
{
	//  close the socket connection - this will kill the accept call if it is blocking in the thread
	if(SocketFileDescriptor > 0)
	{
		shutdown(SocketFileDescriptor, SHUT_RDWR);
		close(SocketFileDescriptor);
	}

	//  stop the running thread 
	Thread::Cancel();

	SocketFileDescriptor = -1;
	PortNumber = -1;

	return;
}

typedef unsigned long uint32;

 uint32 SockAddrToUint32(struct sockaddr * a)
{
	return ((a)&&(a->sa_family == AF_INET)) ? ntohl(((struct sockaddr_in *)a)->sin_addr.s_addr) : 0;
}

// convert a numeric IP address into its string representation
static void Inet_NtoA(uint32 addr, char * ipbuf)
{
	sprintf(ipbuf, "%li.%li.%li.%li", (addr >> 24) & 0xFF, (addr >> 16) & 0xFF, (addr >> 8) & 0xFF, (addr >> 0) & 0xFF);
}

//  OpenServerSocket
//
int UdpMulticastServerThread::OpenServerSocket(int port, string group)
{
	//  you can't open new port if we are running in the thread
	if(TheThread != 0)
		return - 1;

	//  init our connection 
	PortNumber = -1;
	SocketFileDescriptor = -1;

	//  create the server socket
	SocketFileDescriptor = socket(AF_INET, SOCK_DGRAM, 0);
	if (SocketFileDescriptor < 0) 
	{
		//  failed to create the socket
		return - 1;
	}
	
	
	//  setup the multicast address
	memset(&SocketAddress, 0, sizeof(SocketAddress));
	SocketAddress.sin_family = AF_INET;
	SocketAddress.sin_addr.s_addr = inet_addr(group.c_str());
	SocketAddress.sin_port = htons(port);
	
	//  TODO - setting socket options
	//  Need to finish this to establish connection on all interfaces
	struct ifaddrs * ifap;
	if (getifaddrs(&ifap) == 0)
	{
		struct ifaddrs * p = ifap;
		while (p)
		{
			uint32 ifaAddr  = SockAddrToUint32(p->ifa_addr);
			uint32 maskAddr = SockAddrToUint32(p->ifa_netmask);
			uint32 dstAddr  = SockAddrToUint32(p->ifa_dstaddr);
			if (ifaAddr > 0)
			{
				char ifaAddrStr[32]; Inet_NtoA(ifaAddr, ifaAddrStr);
				char maskAddrStr[32]; Inet_NtoA(maskAddr, maskAddrStr);
				char dstAddrStr[32]; Inet_NtoA(dstAddr, dstAddrStr);
				printf("  Found interface:  name=[%s] desc=[%s] address=[%s] netmask=[%s] broadcastAddr=[%s]\n", p->ifa_name, "unavailable", ifaAddrStr, maskAddrStr, dstAddrStr);
			}
			p = p->ifa_next;
		}
		freeifaddrs(ifap);
	}
	
	//  TODO - this is a quick hack to always use wlan0 as the interface
	//  needs to be finished
	struct ifreq ifr;
	memset(&ifr, 0, sizeof(ifr));
	snprintf(ifr.ifr_name, sizeof(ifr.ifr_name), "wlan0");
	int res = setsockopt(SocketFileDescriptor, SOL_SOCKET, SO_BINDTODEVICE, (void *)&ifr, sizeof(ifr)); 
	if ( res < 0) 
	{
		port = -1;
	}
	
	//  success
	PortNumber = port;
	return PortNumber;
}


void UdpMulticastServerThread::WriteMulticastString(string writeString)
{
	int sent = sendto(SocketFileDescriptor, writeString.c_str(), strlen(writeString.c_str()), 0, (struct sockaddr*) &SocketAddress, sizeof(SocketAddress));
//	if (sent != writeString.length())
//		Logging.AddLog("UdpMulticastServerThread", "WriteMulticaststring", format("Bytes written %d not equal to string length %d for %s.",sent, writeString.length(), writeString.c_str()), LogLevelError);
}
