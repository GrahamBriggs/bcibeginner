#include "CMDiwconfig.h"
#include "Parser.h"

using namespace std;


/////////////////////////////////////////////////////////////////////////////
//  CMDiwconfig
//  Command "iwconfig"
//

//  Constructor
//
CMDiwconfig::CMDiwconfig()
{
	Command = "iwconfig wlan0";

	Init();

}

CMDiwconfig::~CMDiwconfig()
{
}

void CMDiwconfig::Init()
{
	_Mode = iwcUnknown;
	_Essid = "";
	_Connected = true;
}


//  Parse
//  override of base class CMD::Parse to extract specific bits of the ifconfig response
//
bool CMDiwconfig::Parse()
{
	//  clear out values
	Init();

	for ( int i = 0; i < CommandResponse.size(); i++ )
	{

		string nextString = CommandResponse[i];
	
		//  get mode
		if (nextString.find("Mode:Master") != string::npos)
		{
			_Mode = iwcMaster;
		}
		else if (nextString.find("Mode:Managed") != string::npos)
		{
			_Mode = iwcManaged;
		}

		//  get ESSID
		size_t pos = nextString.find("ESSID:\"");
		if (pos != string::npos)
		{
			size_t posEnd = nextString.find("\"", pos + 7);
			string substr = nextString.substr(pos + 7, (posEnd - (pos+7)));
			_Essid = substr;
		}

		//  get connected status
		if (nextString.find("Access Point: Not-Associated") != string::npos)
		{
			_Connected = false;
		}
	}


	return true;
}

string CMDiwconfig::ModeString()
{
	switch (_Mode)
	{
	case iwcMaster:
		return "Master";
	case iwcManaged:
		return "Managed";
	default:
		return "Unknown";
	}
}

/*

//  Router Mode
//

pi@raspberrypi ~ $ iwconfig
wlan0     IEEE 802.11bg  ESSID:"RPi"  Nickname:"<WIFI@REALTEK>"
Mode:Master  Frequency:2.412 GHz  Access Point: 00:13:EF:D0:1E:6C
Bit Rate:54 Mb/s   Sensitivity:0/0
Retry:off   RTS thr:off   Fragment thr:off
Power Management:off
Link Quality=68/100  Signal level=62/100  Noise level=0/100
Rx invalid nwid:0  Rx invalid crypt:0  Rx invalid frag:0
Tx excessive retries:0  Invalid misc:0   Missed beacon:0

lo        no wireless extensions.

eth0      no wireless extensions.


//  Client Mode - not connected
//

pi@raspberrypi ~/source $ iwconfig
wlan0     unassociated  Nickname:"<WIFI@REALTEK>"
Mode:Managed  Frequency=2.412 GHz  Access Point: Not-Associated
Sensitivity:0/0
Retry:off   RTS thr:off   Fragment thr:off
Power Management:off
Link Quality:0  Signal level:0  Noise level:0
Rx invalid nwid:0  Rx invalid crypt:0  Rx invalid frag:0
Tx excessive retries:0  Invalid misc:0   Missed beacon:0

lo        no wireless extensions.

eth0      no wireless extensions.


//  Client mode, connected
//

wlan0     IEEE 802.11bg  ESSID:"getac9"  Nickname:"<WIFI@REALTEK>"
Mode:Managed  Frequency:2.412 GHz  Access Point: 00:10:60:92:F3:DC
Bit Rate:54 Mb/s   Sensitivity:0/0
Retry:off   RTS thr:off   Fragment thr:off
Power Management:off
Link Quality=100/100  Signal level=90/100  Noise level=0/100
Rx invalid nwid:0  Rx invalid crypt:0  Rx invalid frag:0
Tx excessive retries:0  Invalid misc:0   Missed beacon:0

lo        no wireless extensions.

eth0      no wireless extensions.


*/