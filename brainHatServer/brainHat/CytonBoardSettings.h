#pragma once
#include <vector>
#include <string>

#include "BrainflowConfiguration.h"


//  Individual Cyton Channel Settings
//
class CytonChannelSettings
{
public:
	int ChannelNumber;
	bool PowerDown;
	ChannelGain Gain;
	AdsChannelInputType InputType;
	bool Bias;
	bool LlofP;
	bool LlofN;
	bool Srb2;
};


//  Cyton Board Settings
//  Collection of 8 individual channel settings, plus SRB1 setting for the board
//
class CytonBoardSettings
{
public:
	CytonBoardSettings();
	virtual ~CytonBoardSettings();
	
	bool Srb1Set;
	
	void AddChannel(CytonChannelSettings* channelSettings);

	std::vector<CytonChannelSettings*> Channels;
};


//  Cyton Boards
//  Collection of Cyton Board Settings
//
class CytonBoards
{
public:
	CytonBoards();

	virtual ~CytonBoards();
	
	bool ReadFromRegisterString(std::string registerReport);
	bool ValidateBoardChannels();

	std::vector<CytonBoardSettings*> Boards;
	
	void ClearBoards();
};



//  Helper Functions
//

//  Get the index of the bit number or register report comma delimited string
inline int IndexOfBit(int bitNumber) {	return 10 - bitNumber; }


//  Get the channel gain from register report row data
inline ChannelGain GetChannelGain(std::vector<std::string> value)
{
	if (value.size() == 11)
	{
		int channelGain = 0;
		channelGain += value[IndexOfBit(4)] == "1" ? 1 : 0;
		channelGain += value[IndexOfBit(5)] == "1" ? 2 : 0;
		channelGain += value[IndexOfBit(6)] == "1" ? 4 : 0;

		return (ChannelGain)channelGain;
	}

	return ChannelGain::x1;
}


//  Get the channel input type from register report row data
inline AdsChannelInputType GetChannelInputType(std::vector<std::string> value)
{
	if (value.size() == 11)
	{
		int channelInputType = 0;
		channelInputType += value[IndexOfBit(0)] == "1" ? 1 : 0;
		channelInputType += value[IndexOfBit(1)] == "1" ? 2 : 0;
		channelInputType += value[IndexOfBit(2)] == "1" ? 4 : 0;

		return (AdsChannelInputType)channelInputType;
	}

	return AdsChannelInputType::Normal;
}