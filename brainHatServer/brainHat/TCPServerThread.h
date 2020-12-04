#pragma once

#include "Thread.h"


#define TCPSERVER_READBUFFERSIZE 1024

//  ServerThread
//  Runs a TCP socket server thread
//
class TCPServerThread : public Thread
{
public:
	  
	TCPServerThread();
	virtual ~TCPServerThread();

	// set the send and receive timeouts (in seconds)
	// default send time out is 10 seconds
	// default receive timeout is -1 (for block on accept)
	bool SetSocketTimeouts(long sendTimeout, long receiveTimeout);


	// open a server socket on the portToOpen
	// force using specified port by set exactPort to true
	// allow for next available port by setting exactPort to false
	// returns the port number opened, or -1 if failure
	int OpenServerSocket(int portToOpen, bool exactPort);

	//  returns the port number this socket is bound to
	//  if not connected, returns -1
	int GetConnectedOnPortNumber();


	//  override the base class thread cancel
	//  the socket accept may be blocking in the run function, so we need to close socket before we attempt to stop run function
	virtual void Cancel();


	//  the Run function 
	virtual void RunFunction() = 0;

protected:

	//  server port number
	int PortNumber;

	//  file descriptor of the server socket
	int SocketFileDescriptor;

	//  socket read and write timeouts (in seconds)
	long ReceiveTimeout;
	long SendTimeout;
	
	
	//  read bytes from the socket and return the file descriptor of the sending client
	//  bytes read are cast as a single character string into readString
	//  returns file descriptor of accepted socket
	int ReadStringFromSocket(struct sockaddr_in *clientAddress, std::string& readString);
	
	//  write a single char string to the socket
	//  returns the number of bytes written, or -1 for error
	int WriteStringToSocket(int socketFileDescriptor, std::string writeString);
	
	//  TODO:  Room for Improvement
	//  This class only handles single character strings
	//  it should be able to handle all formats of data over the IP connection, such as
	//  multi byte strings
	//  doubles / ints / floats ... in proper network byte order
	//
	//  For an example on how to properly handle these data types,
	//  check out the socket example available here:  http://www.keithv.com/software/socket/
};
