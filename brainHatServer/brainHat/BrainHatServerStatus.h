#pragma once
#include <string>
#include "json.hpp"
#include "BoardIds.h"

struct BrainHatServerStatus
{
public:
	
	std::string HostName;
	std::string Eth0Address;
	std::string Wlan0Address;
	std::string Wlan0Mode;
	//
	int BoardId;
	int SampleRate;
	int CytonSRB1;
	int DaisySRB1;
	bool IsStreaming;
	//
	bool RecordingDataBrainHat;
	bool RecordingDataBoard;
	std::string RecordingFileNameBrainHat;
	std::string RecordingFileNameBoard;
	double RecordingDurationBrainHat;
	double RecordingDurationBoard;
	//
	long long UnixTimeMillis;
	
	
	BrainHatServerStatus()
	{
		HostName = "";
		Eth0Address = "";
		Wlan0Address = "";
		Wlan0Mode = "";
		BoardId = (int)BrainhatBoardIds::UNDEFINED;
		SampleRate = 0;
		CytonSRB1 = -1;
		DaisySRB1 = -1;
		IsStreaming = false;
		RecordingDataBrainHat = false;
		RecordingDataBoard = false;
		RecordingFileNameBrainHat = "";
		RecordingFileNameBoard = "";
		RecordingDurationBrainHat = 0.0;
		RecordingDurationBoard = 0.0;
		UnixTimeMillis = 0;
	}
	
	
	nlohmann::json AsJson()
	{
		nlohmann::json j;
		
		j["HostName"] = HostName;
		j["Eth0Address"] = Eth0Address;
		j["Wlan0Address"] = Wlan0Address;
		j["Wlan0Mode"] = Wlan0Mode;
		j["BoardId"] = BoardId;
		j["SampleRate"] = SampleRate;
		j["CytonSRB1"] = CytonSRB1;
		j["DaisySRB1"] = DaisySRB1;
		j["IsStreaming"] = IsStreaming;
		j["RecordingDataBrainHat"] = RecordingDataBrainHat;
		j["RecordingDataBoard"] = RecordingDataBoard;
		j["RecordingFileNameBrainHat"] = RecordingFileNameBrainHat;
		j["RecordingFileNameBoard"] = RecordingFileNameBoard;
		j["RecordingDurationBrainHat"] = RecordingDurationBrainHat;
		j["RecordingDurationBoard"] = RecordingDurationBoard;
		j["UnixTimeMillis"] = UnixTimeMillis;
		
		return j;
	}
	
	
protected:
	
};
