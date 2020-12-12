#pragma once

#include <string>
#include "TCPServerThread.h"

typedef bool(*HandleRequestCallbackFn)(std::string);

//  TCP Server thread waits for query or command from remote client applicaiton
//
class CommandServer : public TCPServerThread
{
public:
	CommandServer(HandleRequestCallbackFn handleRequestFn);
	virtual ~CommandServer();
	
	virtual void Start();
	
	virtual void RunFunction();
	
protected:
	
	HandleRequestCallbackFn HandleRequestCallback;
	
	void HandleLogLevelChangeRequest(int acceptFileDesc, std::string args);
};


