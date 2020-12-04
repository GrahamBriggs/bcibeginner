#pragma once

#include <netinet/in.h>

#include "Thread.h"





class UdpMulticastServerThread : public Thread
{
	
public:
	UdpMulticastServerThread();
	virtual ~UdpMulticastServerThread();
	
	// open a server socket on the port with the group address
	// returns the port number opened, or -1 if failure
	int OpenServerSocket(int port, std::string group);
	
	virtual void Cancel();
	
	virtual void RunFunction()  = 0;
	
	int GetPortNumber() {return PortNumber;}

protected:
	
	//  server port number
	int PortNumber;
	
	//  group address
	std::string Group;
	
	//  socket address
	struct sockaddr_in SocketAddress;

	//  file descriptor of the server socket
	int SocketFileDescriptor;
	
	//  write a string to the multicast stream
	void WriteMulticastString(std::string writeString);
	
	
};