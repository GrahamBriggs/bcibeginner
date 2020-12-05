#include <netinet/in.h>
#include <unistd.h>
#include <string>
#include <arpa/inet.h>
#include <iostream>
#include <sstream>
#include <string.h>

#include "TCPServerThread.h"


using namespace std;



/////////////////////////////////////////////////////////////////////////////
//  TCPServerThread
//  runs a simple TCP/IP socket server thread
//

//  Constructor
//
TCPServerThread::TCPServerThread()
{
	PortNumber = -1;

	//  set the timeouts
	ReceiveTimeout = 0;
	SendTimeout = 5;
}



//  Destructor
//
TCPServerThread::~TCPServerThread()
{
	if ( ThreadRunning )
	{
		Cancel();
	}

	SocketFileDescriptor = -1;
}



//  Cancel
//  override of thread base class cancel so we can close socket before killing thread run function
void TCPServerThread::Cancel()
{
	//  close the socket connection - this will kill the accept call if it is blocking in the thread
	if ( SocketFileDescriptor > 0 )
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



//  GetConnectedOnPortNumber
// 
int TCPServerThread::GetConnectedOnPortNumber()
{
	if ( SocketFileDescriptor >= 0 && PortNumber >= 0 )
		return PortNumber;
	else
		return -1;
}



//  SetSocketTimeouts
//
bool TCPServerThread::SetSocketTimeouts(long sendTimeout, long receiveTimeout)
{
	if ( TheThread != 0 )
		return false;	//  can't change timeouts when we are running

	if ( sendTimeout < 1 )
		SendTimeout = 1;
	else
		SendTimeout = sendTimeout;
	
	ReceiveTimeout = receiveTimeout;

	return true;
}



//  OpenServerSocket
//
int TCPServerThread::OpenServerSocket(int portToOpen, bool exactPort)
{
	//  you can't open new port if we are running in the thread
	if ( TheThread != 0 )
		return -1;

	//  init our connection 
	PortNumber = -1;
	SocketFileDescriptor = -1;

	//  create the server socket
	SocketFileDescriptor = socket(AF_INET, SOCK_STREAM, 0);
	if (SocketFileDescriptor < 0) 
	{
		//  failed to create the socket
		return -1;
	}

	//  set the socket options
	int yes = 1;
	if (setsockopt(SocketFileDescriptor, SOL_SOCKET, SO_REUSEADDR, &yes, sizeof(int)) == -1)
	{
		return -1;
	}



	//  set the socket timeouts  
	struct timeval timeout;
	timeout.tv_usec = 0;
	//
	//  send timeout
	if ( SendTimeout > 0 )
	{
		timeout.tv_sec = SendTimeout;
		if ( setsockopt(SocketFileDescriptor, SOL_SOCKET, SO_SNDTIMEO, (char *)&timeout, sizeof(timeout)) < 0 )
			return -1;
	}
	//
	//  receive timeout
	if ( ReceiveTimeout > 0 )
	{
		timeout.tv_sec = ReceiveTimeout;
		if ( setsockopt(SocketFileDescriptor, SOL_SOCKET, SO_RCVTIMEO, (char *)&timeout, sizeof(timeout)) < 0 )
			return -1;
	}
	
	//  setup the server address struct
	struct sockaddr_in serverAddress;
	memset(&serverAddress, 0, sizeof(serverAddress));

	//  set to IP4 address usage
	serverAddress.sin_family = AF_INET;
	//  set to accept connection from any address
	serverAddress.sin_addr.s_addr = INADDR_ANY;
	
	//  Set the port number
	int bindToPortNumber = portToOpen;
	//  call htons to convert to network byte order
	serverAddress.sin_port = htons(bindToPortNumber);

	//  bind the socket
	int bindReturn = -1;
	while ( bindReturn < 0 )
	{
		bindReturn =  ::bind(SocketFileDescriptor, (struct sockaddr *) &serverAddress, sizeof(serverAddress) );
		if ( bindReturn == 0 )
		{
			//  success
			break;	
		}
		else if ( errno == EADDRINUSE )
		{
			//  that port is in use
			//  if we are demanding exact port, this is faiulre
			if ( exactPort == true )
			{
				shutdown(SocketFileDescriptor, 2);
				return -1;
			}

			//  otherwise we will try binding to the next port
			bindToPortNumber ++;
			serverAddress.sin_port = htons(bindToPortNumber);
		}
		else
		{
			//  fail to bind to port for some dire reason ?
			

			shutdown(SocketFileDescriptor, 2);
			return -1;	
		}
		
		//  fail after 1000 tries
		if ( bindToPortNumber - portToOpen > 1000 )
		{
			shutdown(SocketFileDescriptor, 2);
			return -1;
		}
	}

	//  setup listening
	if ( listen(SocketFileDescriptor,5) < 0 )
	{
		
		shutdown(SocketFileDescriptor, 2);
		return -1;
	}

	//  we are bound to a port
	PortNumber = bindToPortNumber;

	//  success
	return PortNumber;
}



//  ReadStringFromSocket
//
int TCPServerThread::ReadStringFromSocket(struct sockaddr_in *clientAddress, string& readString)
{
	//  init values
	readString = "";
	int acceptFileDescriptor = -1;

	//  cast length of client address struct to an int
	int clientLength = sizeof(*clientAddress);

	//  accept
	acceptFileDescriptor = accept(SocketFileDescriptor, (struct sockaddr *) clientAddress, (socklen_t*)&clientLength);

	//  nothing accepted
	if (acceptFileDescriptor < 0) 
	{
		return acceptFileDescriptor;
	}

	//  read the sent buffer into the string
	//  note, we allocate one extra byte in our buffer so we are guaranteed to have a null at the end when we cast to string
	char buffer[TCPServerThread_READBUFFERSIZE+1];
	memset(buffer, 0, TCPServerThread_READBUFFERSIZE+1);

	int bytesRead = read(acceptFileDescriptor,buffer,TCPServerThread_READBUFFERSIZE);
	readString += string(buffer);		//  note:  this simple implementation will lose any bytes after first null, is designed for strings and not byte data

	//  did we get all the bytes, check for more bytes if we read 1024
	if ( bytesRead == TCPServerThread_READBUFFERSIZE )
	{
		//  read( ) will return -1 when it reaches End Of File (EOF)
		while ( bytesRead > 0 )
		{
			memset(buffer, 0, TCPServerThread_READBUFFERSIZE);
			bytesRead = read(acceptFileDescriptor,buffer,TCPServerThread_READBUFFERSIZE);
			readString += string(buffer);
		}
	}

	//  return file descriptor of the client socket
	return acceptFileDescriptor;
}



//  WriteStringToSocket
//
int TCPServerThread::WriteStringToSocket(int socketFileDescriptor, string writeString)
{
	//  size of string
	int stringSize = writeString.size();

	//  allocate buffer one extra byte for null character
	char buffer[stringSize+1];
	memset(buffer, 0, stringSize+1);

	//  copy the string to the buffer
	memcpy(buffer, writeString.c_str(), stringSize);

	//  write string to the socket
	int writeBytes = write(socketFileDescriptor, buffer, stringSize+1);

	return writeBytes;
}
