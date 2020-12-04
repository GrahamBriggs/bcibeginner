#pragma once

#include <string>
#include "TCPServerThread.h"


//  TCP Server thread waits for query or command from remote client applicaiton
//
class CommandServerThread : public TCPServerThread
{
public:
	CommandServerThread();
	virtual ~CommandServerThread();
	
	virtual void Start();
	
	virtual void RunFunction();
	
protected:
	
	void HandleKeyboardInputRequest(int acceptFileDesc, std::string args);
	void HandleLogLevelChangeRequest(int acceptFileDesc, std::string args);
};


