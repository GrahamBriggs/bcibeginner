#include "CMDifconfig.h"
#include "Parser.h"

using namespace std;


/////////////////////////////////////////////////////////////////////////////
//  CMDifconfig
//  Command "ifconfig"
//

//  Constructor
//
CMDifconfig::CMDifconfig()
{
	Command = "ifconfig";

}

CMDifconfig::~CMDifconfig()
{
}


//  Parse
//  override of base class CMD::Parse to extract specific bits of the ifconfig response
//
bool CMDifconfig::Parse()
{
	Parser parser("", "");

	for ( int i = 0; i < CommandResponse.size(); i++ )
	{

		string nextString = CommandResponse[i];


		parser.SetBuffer(nextString, " ");

		string getString = parser.GetNextString();
		while ( getString.size() != 0 )
		{
			if ( getString.find("eth0") == 0 )
			{
				_Eth0Info.Type = getString;

				//  parse the wired connection

				nextString = CommandResponse[++i];
				parser.SetBuffer(nextString, " :");

				//  discard inet addr:
				getString = parser.GetNextString();
				//getString = parser.GetNextString();

				//  parse the IP address
				_Eth0Info.Inet4Address = parser.GetNextString();
				if (_Eth0Info.Inet4Address.size() < 7)
					_Eth0Info.Inet4Address = "";

				//  done with this connection
				break;
			}
			else if ( getString.find("lo") == 0 )
			{
				_LoopbackInfo.Type = getString;

				//  parse the wired connection
				nextString = CommandResponse[++i];
				parser.SetBuffer(nextString, " :");

				//  discard inet addr:
				getString = parser.GetNextString();
				//getString = parser.GetNextString();

				//  parse the IP address
				_LoopbackInfo.Inet4Address = parser.GetNextString();

				//  done with this connection
				break;
			}
			else if ( getString.find("wlan0") == 0 )
			{
				_WlanInfo.Type = getString;

				//  parse the wired connection
				nextString = CommandResponse[++i];
				parser.SetBuffer(nextString, " :");

				//  discard inet addr:
				getString = parser.GetNextString();
				//getString = parser.GetNextString();

				//  parse the IP address
				_WlanInfo.Inet4Address = parser.GetNextString();

				//  done with this connection
				break;
			}
			else
			{
				break;
			}
		}
	}


	return true;
}

