#pragma once
#include <string>
#include "json.hpp"
#include "Parser.h"


struct OpenBciData
{
	OpenBciData(double** rawData, int point)
	{
		SampleIndex = rawData[0][point];
		ExgCh0 = rawData[1][point];
		ExgCh1 = rawData[2][point];
		ExgCh2 = rawData[3][point];
		ExgCh3 = rawData[4][point];
		ExgCh4 = rawData[5][point];
		ExgCh5 = rawData[6][point];
		ExgCh6 = rawData[7][point];
		ExgCh7 = rawData[8][point];
		AcelCh0 = rawData[9][point];
		AcelCh1 = rawData[10][point];
		AcelCh2 = rawData[11][point];
		Other0 = rawData[12][point];
		Other1 = rawData[13][point];
		Other2 = rawData[14][point];
		Other3 = rawData[15][point];
		Other4 = rawData[16][point];
		Other5 = rawData[17][point];
		Other6 = rawData[18][point];
		AngCh0 = rawData[19][point];
		AngCh1 = rawData[20][point];
		AngCh2 = rawData[21][point];
		TimeStamp = rawData[22][point];
	}
	
	OpenBciData(OpenBciData* copy)
	{
		*this = *copy;
	}
	
	OpenBciData(std::string rawData)
	{
		Parser parser(rawData, ",");
		SampleIndex = parser.GetNextDouble();
		ExgCh0  = parser.GetNextDouble();
		ExgCh1  = parser.GetNextDouble();
		ExgCh2  = parser.GetNextDouble();
		ExgCh3  = parser.GetNextDouble();
		ExgCh4  = parser.GetNextDouble();
		ExgCh5  = parser.GetNextDouble();
		ExgCh6  = parser.GetNextDouble();
		ExgCh7  = parser.GetNextDouble();
		AcelCh0  = parser.GetNextDouble();
		AcelCh1  = parser.GetNextDouble();
		AcelCh2  = parser.GetNextDouble();
		Other0  = parser.GetNextDouble();
		Other1  = parser.GetNextDouble();
		Other2  = parser.GetNextDouble();
		Other3  = parser.GetNextDouble();
		Other4  = parser.GetNextDouble();
		Other5  = parser.GetNextDouble();
		Other6  = parser.GetNextDouble();
		AngCh0  = parser.GetNextDouble();
		AngCh1  = parser.GetNextDouble();
		AngCh2  = parser.GetNextDouble();
		TimeStamp = parser.GetNextDouble();
	}
	
	
	nlohmann::json AsJson()
	{
		nlohmann::json j;
		
		j["SampleIndex"] = SampleIndex;
		j["ExgCh0"] = ExgCh0;
		j["ExgCh1"] = ExgCh1;
		j["ExgCh2"] = ExgCh2;
		j["ExgCh3"] = ExgCh3;
		j["ExgCh4"] = ExgCh4;
		j["ExgCh5"] = ExgCh5;
		j["ExgCh6"] = ExgCh6;
		j["ExgCh7"] = ExgCh7;
		j["AcelCh0"] = AcelCh0;
		j["AcelCh1"] = AcelCh1;
		j["AcelCh2"] = AcelCh2;
		j["Other0"] = Other0;
		j["Other1"] = Other1;
		j["Other2"] = Other2;
		j["Other3"] = Other3;
		j["Other4"] = Other4;
		j["Other5"] = Other5;
		j["Other6"] = Other6;
		j["AngCh0"] = AngCh0;
		j["AngCh1"] = AngCh1;
		j["AngCh2"] = AngCh2;
		j["TimeStamp"] = TimeStamp;
		
		return j;
	}
	
	
	double SampleIndex;
	double ExgCh0;
	double ExgCh1;
	double ExgCh2;
	double ExgCh3;
	double ExgCh4;
	double ExgCh5;
	double ExgCh6;
	double ExgCh7;
	double AcelCh0;
	double AcelCh1;
	double AcelCh2;
	double Other0;
	double Other1;
	double Other2;
	double Other3;
	double Other4;
	double Other5;
	double Other6;
	double AngCh0;
	double AngCh1;
	double AngCh2;
	double TimeStamp;
};