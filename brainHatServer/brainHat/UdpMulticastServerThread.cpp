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
	
	
	//  success
	PortNumber = port;
	return PortNumber;
}


void UdpMulticastServerThread::WriteMulticastString(string writeString)
{
	auto size = writeString.size();
	if (size > 478)
		int i = 0;
	
	int sent = sendto(SocketFileDescriptor, writeString.c_str(), strlen(writeString.c_str()), 0, (struct sockaddr*) &SocketAddress, sizeof(SocketAddress));
	if (sent != writeString.length())
		Logging.AddLog("UdpMulticastServerThread", "WriteMulticaststring", format("Bytes written %d not equal to string length %d for %s.",sent, writeString.length(), writeString.c_str()), LogLevelError);
}
