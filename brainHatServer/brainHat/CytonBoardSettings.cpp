#include "brainHat.h"
#include "CytonBoardSettings.h"
#include "StringExtensions.h"

using namespace std;


// Cyton Board Settings
//
#pragma  region CytonBoard

CytonBoardSettings::CytonBoardSettings()
{
	Srb1Set = false;
}
	

//  Destructor
//
CytonBoardSettings::~CytonBoardSettings()
{
	for (auto it = Channels.begin(); it != Channels.end(); ++it)
	{
		delete *it;
	}
	Channels.clear();
}
	
	
//  Add a channel to the channels collection	
//
void CytonBoardSettings::AddChannel(CytonChannelSettings* channelSettings)
{
	Channels.push_back(channelSettings);
}
#pragma endregion



//  CytonBoards
//
#pragma region CytonBoards

CytonBoards::CytonBoards()
{
}

// Destructor
//
CytonBoards::~CytonBoards()
{
	ClearBoards();
}
	

//  Clear Boards
//
void CytonBoards::ClearBoards()
{
	for (auto it = Boards.begin(); it != Boards.end(); ++it)
	{
		delete *it;
	}
	Boards.clear();
}


//  Read from register string
//
bool CytonBoards::ReadFromRegisterString(string registerReport)
{
	registerReport = registerReport.replace(registerReport.begin(), registerReport.end(), '\r', ' ');
	auto end_pos = remove(registerReport.begin(), registerReport.end(), ' ');
	registerReport.erase(end_pos, registerReport.end());
	
	int boardChannelOffset = 0;
	
	string CHx = "CHx";
	string CH = "CH";
	string BIAS_SENSP = "BIAS_SENSP";
	string LOFF_SENSP = "LOFF_SENSP";
	string LOFF_SENSN = "LOFF_SENSN";
	string MISC1 = "MISC1";
		
	vector<string> lines;
	Tokenize(registerReport, lines, "\n");
		
	for (auto it = lines.begin(); it != lines.end(); ++it)
	{
		auto nextLine = *it;
		if (nextLine.find("ADSRegisters") >= 0)
		{
			if (Boards.size() > 0)
			{
				if (Boards[Boards.size() - 1]->Channels.size() != 8)
				{
					Logging.AddLog("CytonBoardSettings", "ReadFromRegisterString", "Board does not have 8 channels.", LogLevelError);
					ClearBoards();
					return false;
				}
				boardChannelOffset += Boards.back()->Channels.size();
			}

			Boards.push_back(new CytonBoardSettings());
		}
		else if (nextLine.length() > CHx.length() && nextLine.substr(0, CH.length()) == CH)
		{
			vector<string> columns;
			Tokenize(nextLine, columns, ",");
			if (columns.size() == 11)
			{
				auto newChannel = new CytonChannelSettings();
				Boards.back()->AddChannel(newChannel);
					
				newChannel->ChannelNumber = ParseInt(nextLine.substr(CH.length(), 1)) + boardChannelOffset;
                           
				if (Boards.back()->Channels.size() > 0)
				{
					if (newChannel->ChannelNumber - Boards.back()->Channels.back()->ChannelNumber != 1)
					{
						Logging.AddLog("CytonBoardSettings", "ReadFromRegisterString", "Board channels are not sequential.", LogLevelError);
						ClearBoards();
						return false;
					}
				}
                            
				newChannel->PowerDown = columns[IndexOfBit(7)] == "1" ? true : false;
				newChannel->Gain = GetChannelGain(columns);
				newChannel->InputType = GetChannelInputType(columns);
				newChannel->Srb2 = columns[IndexOfBit(3)] == "1" ? true : false;

				Boards.back()->AddChannel(newChannel);

			}
		}
		else if (nextLine.length() > BIAS_SENSP.length() && nextLine.substr(0, BIAS_SENSP.length()).compare(BIAS_SENSP) == 0)
		{
			if (!ValidateBoardChannels())
			{
				Logging.AddLog("CytonBoardSettings", "ReadFromRegisterString", "Board does not have 8 channels.", LogLevelError);
				ClearBoards();
				return false;
			}

			vector<string> columns;
			Tokenize(nextLine, columns, ",");
			if (columns.size() == 11)
			{
				int bit = 0;
				for (auto nextChannel = Boards.back()->Channels.begin(); nextChannel != Boards.back()->Channels.end(); ++nextChannel) 
				{
					(*nextChannel)->Bias = columns[IndexOfBit(bit)] == "1" ? true : false;
					bit++;
				}
			}
		}
		else if (nextLine.length() > LOFF_SENSP.length() && nextLine.substr(0, LOFF_SENSP.length()).compare(LOFF_SENSP) == 0)
		{
			if (!ValidateBoardChannels())
			{
				Logging.AddLog("CytonBoardSettings", "ReadFromRegisterString", "Board does not have 8 channels.", LogLevelError);
				ClearBoards();
				return false;
			}

			vector<string> columns;
			Tokenize(nextLine, columns, ",");
			if (columns.size() == 11)
			{
				int bit = 0;
				for (auto nextChannel = Boards.back()->Channels.begin(); nextChannel != Boards.back()->Channels.end(); ++nextChannel) 
				{
					(*nextChannel)->LlofP = columns[IndexOfBit(bit)] == "1" ? true : false;
					bit++;
				}
			}
		}
		else if (nextLine.length() > LOFF_SENSN.length() && nextLine.substr(0, LOFF_SENSN.length()).compare(LOFF_SENSN) == 0)
		{
			if (!ValidateBoardChannels())
			{
				Logging.AddLog("CytonBoardSettings", "ReadFromRegisterString", "Board does not have 8 channels.", LogLevelError);
				ClearBoards();
				return false;
			}

			vector<string> columns;
			Tokenize(nextLine, columns, ",");
			if (columns.size() == 11)
			{
				int bit = 0;
				for (auto nextChannel = Boards.back()->Channels.begin(); nextChannel != Boards.back()->Channels.end(); ++nextChannel) 
				{
					(*nextChannel)->LlofN = columns[IndexOfBit(bit)] == "1" ? true : false;
					bit++;
				}
			}
		}
		else if (nextLine.length() > MISC1.length() && nextLine.substr(0, MISC1.length()).compare(MISC1) == 0)
		{
			if (!ValidateBoardChannels())
			{
				Logging.AddLog("CytonBoardSettings", "ReadFromRegisterString", "Board does not have 8 channels.", LogLevelError);
				ClearBoards();
				return false;
			}

			vector<string> columns;
			Tokenize(nextLine, columns, ",");
			if (columns.size() == 11)
			{
				Boards.back()->Srb1Set = columns[IndexOfBit(5)] == "1" ? true : false;
			}
		}
	}
	
	return true;

   
	
}


bool CytonBoards::ValidateBoardChannels()
{
	return Boards.back()->Channels.size() == 8;
}
	
#pragma endregion
