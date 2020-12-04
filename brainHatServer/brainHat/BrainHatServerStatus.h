#pragma once
#include <string>
#include "json.hpp"

struct BrainHatServerStatus
{
public:
	
	std::string HostName;
	std::string Eth0Address;
	std::string Wlan0Address;
	std::string Wlan0Mode;
	int DataPort;
	int LogPort;
	//
	int BoardId;
	int SampleRate;
	//
	bool RecordingDataBrainHat;
	bool RecordingDataBoard;
	std::string RecordingFileNameBrainHat;
	std::string RecordingFileNameBoard;
	//
	long long UnixTimeMillis;
	
	
	BrainHatServerStatus()
	{
		HostName = "";
		Eth0Address = "";
		Wlan0Address = "";
		Wlan0Mode = "";
		DataPort = 0;
		LogPort = 0;
		BoardId = -99;
		SampleRate = 0;
		RecordingDataBrainHat = false;
		RecordingDataBoard = false;
		RecordingFileNameBrainHat = "";
		RecordingFileNameBoard = "";
		UnixTimeMillis = 0;
	}
	
	
	nlohmann::json AsJson()
	{
		nlohmann::json j;
		
		j["HostName"] = HostName;
		j["Eth0Address"] = Eth0Address;
		j["Wlan0Address"] = Wlan0Address;
		j["Wlan0Mode"] = Wlan0Mode;
		j["DataPort"] = DataPort;
		j["LogPort"] = LogPort;
		j["BoardId"] = BoardId;
		j["SampleRate"] = SampleRate;
		j["RecordingDataBrainHat"] = RecordingDataBrainHat;
		j["RecordingDataBoard"] = RecordingDataBoard;
		j["RecordingFileNameBrainHat"] = RecordingFileNameBrainHat;
		j["RecordingFileNameBoard"] = RecordingFileNameBoard;
		j["UnixTimeMillis"] = UnixTimeMillis;
		
		return j;
	}
	
	
protected:
	
};
