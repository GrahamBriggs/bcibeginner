#pragma once
#include <vector>
#include <string>

#include "CytonBoardConfiguration.h"


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
	
	bool HasValidSettings() { return Boards.size() > 0;}
	
	void ClearBoards();
};


