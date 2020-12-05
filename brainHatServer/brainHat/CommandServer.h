#pragma once

#include <string>
#include "TCPServerThread.h"


//  TCP Server thread waits for query or command from remote client applicaiton
//
class CommandServer : public TCPServerThread
{
public:
	CommandServer();
	virtual ~CommandServer();
	
	virtual void Start();
	
	virtual void RunFunction();
	
protected:
	
	void HandleKeyboardInputRequest(int acceptFileDesc, std::string args);
	void HandleLogLevelChangeRequest(int acceptFileDesc, std::string args);
};


