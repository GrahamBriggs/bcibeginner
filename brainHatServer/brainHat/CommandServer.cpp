#include <netinet/in.h>
#include <string>
#include  <algorithm>
#include <sstream>

#include "brainHat.h"
#include "CommandServer.h"
#include "Parser.h"
#include "StringExtensions.h"
#include "TimeExtensions.h"
#include "NetworkAddresses.h"



using namespace std;


//  TCPIP server socket accepting command requests from clients
//
CommandServer::CommandServer(HandleRequestCallbackFn handleRequestFn)
{
	HandleRequestCallback = handleRequestFn;
}


//  Destructor
//
CommandServer::~CommandServer()
{
}


//  Thread Start
//  
void CommandServer::Start()
{
	int port = OpenServerSocket(COMSERVER_PORT, true);
	if (port < 0)
	{
		Logging.AddLog("CommandServer", "Start", "Unable to open server socket port", LogLevelFatal);
		return;
	}
	
	TCPServerThread::Start();
}
	

//  Thread run function
//  waits on accept client TCPIP request, then handles the request
void CommandServer::RunFunction()
{
	Logging.AddLog("CommandServer", "RunFunction", "Starting CommandServer::RunFunction", LogLevelDebug);
	
	while (ThreadRunning)
	{
		struct sockaddr_in clientAddress;
		string readFromSocket;

		int acceptFileDescriptor = ReadStringFromSocket(&clientAddress, readFromSocket);
	
		if (acceptFileDescriptor < 0)
			continue;
		
		readFromSocket.erase(remove(readFromSocket.begin(), readFromSocket.end(), '\r'), readFromSocket.end());
		readFromSocket.erase(remove(readFromSocket.begin(), readFromSocket.end(), '\n'), readFromSocket.end());	
		UriArgParser argParser(readFromSocket);

		if (argParser.GetRequest() == "loglevel")
		{
			HandleLogLevelChangeRequest(acceptFileDescriptor, argParser);
		}
		else if(argParser.GetRequest() == "ping")
		{	
			WriteStringToSocket(acceptFileDescriptor, format("ACK?time=%llu\n", GetUnixTimeMilliseconds()));
		}
		else
		{
			if (HandleRequestCallback(readFromSocket))
			{
				WriteStringToSocket(acceptFileDescriptor, format("ACK?request=%s&time=%llu\n", argParser.GetRequest().c_str(), GetUnixTimeMilliseconds()));
			}
			else
			{
				WriteStringToSocket(acceptFileDescriptor, "NAK?response=Invalid.\n");
			}
		}
	}
}





//  Change log level request
//
void CommandServer::HandleLogLevelChangeRequest(int acceptFileDesc, UriArgParser& argParser)
{		
	if (argParser.GetArg("object") == "a")
	{	
		auto logLevel = argParser.GetArg("level");
		
		int level;
		stringstream converter;
		converter << logLevel;
		converter >> level;

		if (!converter.fail())
		{
			Logging.ToggleAppLogLevel((LogLevel)level);
			WriteStringToSocket(acceptFileDesc, format("ACK?response=Log level set to %d.\n", level).c_str());
			return;
		}
	}
	
	//  failed
	WriteStringToSocket(acceptFileDesc, "NAK?response=Invalid arguments for loglevel.\n");	
}