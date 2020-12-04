#ifndef _CMDIWCONFIG_H
#define _CMDIWCONFIG_H

#include <string>

#include "CMD.h"

using namespace std;


enum iwcMode
{
	iwcUnknown,
	iwcMaster,
	iwcManaged
};


// CMDiwconfig
// Command "iwconfig"
//
class CMDiwconfig : public CMD
{
public:
	CMDiwconfig();

	virtual ~CMDiwconfig();

	void Init();

	//  override base class CMD::Parse so we can parse internet address out of system response
	virtual bool Parse();

	//  Properties
	string Essid() { return _Essid; }
	iwcMode Mode() { return _Mode; }
	std::string ModeString();
	bool Connected() { return _Connected && _Essid.size() > 0; }

protected:
	
	string _Essid;
	iwcMode _Mode;
	bool _Connected;
};


#endif