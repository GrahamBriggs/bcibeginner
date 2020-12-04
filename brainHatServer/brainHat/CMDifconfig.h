#ifndef _CMDIFCONFIG_H
#define _CMDIFCONFIG_H

#include <string>

#include "CMD.h"



//  ConnectionInfo
//  container class for internet information
//
class ConnectionInfo
{
public:

	//  TODO:  expand this to support ipv6 addresses
	std::string Type;
	std::string Inet4Address;

	ConnectionInfo()
	{
		Type = "";
		Inet4Address = "";
	}
};



// CMDifconfig
// Command "ifconfig"
//
class CMDifconfig : public CMD
{
public:
	CMDifconfig();

	virtual ~CMDifconfig();

	//  override base class CMD::Parse so we can parse internet address out of system response
	virtual bool Parse();

	ConnectionInfo Eth0Info() { return _Eth0Info; }
	ConnectionInfo LoopbackInfo() { return _LoopbackInfo; }
	ConnectionInfo WlanInfo() { return _WlanInfo; }

protected:
	ConnectionInfo _Eth0Info;
	ConnectionInfo _LoopbackInfo;
	ConnectionInfo _WlanInfo;
};


#endif