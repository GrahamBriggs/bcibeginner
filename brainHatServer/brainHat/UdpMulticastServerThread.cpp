#include <netinet/in.h>
#include <unistd.h>
#include <string>
#include <arpa/inet.h>
#include <iostream>
#include <sstream>
#include <string.h>
#include <net/if.h>

#include "brainHat.h"
#include "UdpMulticastServerThread.h"
#include "StringExtensions.h"


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


//  OpenServerSocket
//
int UdpMulticastServerThread::OpenServerSocket(int port, string group, string interface)
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
	

	
	//  if the interface is specified, set this in socket options
	//  note, you must be running as sudo or setsockopt will fail
	if (interface.length() > 0)
	{
		struct ifreq ifr;
		memset(&ifr, 0, sizeof(ifr));
		snprintf(ifr.ifr_name, sizeof(ifr.ifr_name), interface.c_str());
		
		Logging.AddLog("UdpMulticastServerThread", "OpenServerSocket", format("Setting interface %s for socket %d.", interface.c_str(), SocketFileDescriptor), LogLevelDebug);
		int res = setsockopt(SocketFileDescriptor, SOL_SOCKET, SO_BINDTODEVICE, (void *)&ifr, sizeof(ifr)); 
		if (res < 0) 
		{
			Logging.AddLog("UdpMulticastServerThread", "OpenServerSocket", format("Unable to set socket options for %s res %d.", interface.c_str(), res), LogLevelError);
			shutdown(SocketFileDescriptor, SHUT_RDWR);
			close(SocketFileDescriptor);
			return -1;
		}
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
