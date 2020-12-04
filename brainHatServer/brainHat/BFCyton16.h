#pragma once
#include <string>
#include "json.hpp"
#include "Parser.h"
#include "BFSample.h"

class Cyton16Sample : public BFSample
{
public:
	
	//  Cyton 16 Channel Sample, based on OpenBCI_GUI (v4) .txt file logging format
	
	//double SampleIndex;
	double ExgCh0;
	double ExgCh1;
	double ExgCh2;
	double ExgCh3;
	double ExgCh4;
	double ExgCh5;
	double ExgCh6;
	double ExgCh7;
	double ExgCh8;
	double ExgCh9;
	double ExgCh10;
	double ExgCh11;
	double ExgCh12;
	double ExgCh13;
	double ExgCh14;
	double ExgCh15;
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
	//double TimeStamp;
	
	void Init()
	{
		SampleIndex = MISSING_VALUE;
		ExgCh0 = MISSING_VALUE;
		ExgCh1 = MISSING_VALUE;
		ExgCh2 = MISSING_VALUE;
		ExgCh3 = MISSING_VALUE;
		ExgCh4 = MISSING_VALUE;
		ExgCh5 = MISSING_VALUE;
		ExgCh6 = MISSING_VALUE;
		ExgCh7 = MISSING_VALUE;
		ExgCh8 = MISSING_VALUE;
		ExgCh9 = MISSING_VALUE;
		ExgCh10 = MISSING_VALUE;
		ExgCh11 = MISSING_VALUE;
		ExgCh12 = MISSING_VALUE;
		ExgCh13 = MISSING_VALUE;
		ExgCh14 = MISSING_VALUE;
		ExgCh15 = MISSING_VALUE;
		AcelCh0 = MISSING_VALUE;
		AcelCh1 = MISSING_VALUE;
		AcelCh2 = MISSING_VALUE;
		Other0 = MISSING_VALUE;
		Other1 = MISSING_VALUE;
		Other2 = MISSING_VALUE;
		Other3 = MISSING_VALUE;
		Other4 = MISSING_VALUE;
		Other5 = MISSING_VALUE;
		Other6 = MISSING_VALUE;
		AngCh0 = MISSING_VALUE;
		AngCh1 = MISSING_VALUE;
		AngCh2 = MISSING_VALUE;
		TimeStamp = MISSING_VALUE;
	}
	
	Cyton16Sample()
	{
		Init();
	}
	
	//  Construct from a brainflow get_board_data chunk
	Cyton16Sample(double** rawData, int sample)
	{
		SampleIndex = rawData[0][sample];
		ExgCh0 = rawData[1][sample];
		ExgCh1 = rawData[2][sample];
		ExgCh2 = rawData[3][sample];
		ExgCh3 = rawData[4][sample];
		ExgCh4 = rawData[5][sample];
		ExgCh5 = rawData[6][sample];
		ExgCh6 = rawData[7][sample];
		ExgCh7 = rawData[8][sample];
		ExgCh8 = rawData[9][sample];
		ExgCh9 = rawData[10][sample];
		ExgCh10 = rawData[11][sample];
		ExgCh11 = rawData[12][sample];
		ExgCh12 = rawData[13][sample];
		ExgCh13 = rawData[14][sample];
		ExgCh14 = rawData[15][sample];
		ExgCh15 = rawData[16][sample];
		AcelCh0 = rawData[17][sample];
		AcelCh1 = rawData[18][sample];
		AcelCh2 = rawData[19][sample];
		Other0 = rawData[20][sample];
		Other1 = rawData[21][sample];
		Other2 = rawData[22][sample];
		Other3 = rawData[23][sample];
		Other4 = rawData[24][sample];
		Other5 = rawData[25][sample];
		Other6 = rawData[26][sample];
		AngCh0 = rawData[27][sample];
		AngCh1 = rawData[28][sample];
		AngCh2 = rawData[29][sample];
		TimeStamp = rawData[30][sample];
	}
	
	
	//  CopyConstructor
	Cyton16Sample(BFSample* copy)
	{
		Init();
		SampleIndex = copy->SampleIndex;
		TimeStamp = copy->TimeStamp;
	}
	
	
	//  Copy Constructor
	Cyton16Sample(Cyton16Sample* copy)
	{
		*this = *copy;
	}
	
	
	//  Construct from OpenBCI_GUI format text file raw data string
	Cyton16Sample(std::string rawData)
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
		ExgCh8  = parser.GetNextDouble();
		ExgCh9  = parser.GetNextDouble();
		ExgCh10  = parser.GetNextDouble();
		ExgCh11  = parser.GetNextDouble();
		ExgCh12  = parser.GetNextDouble();
		ExgCh13  = parser.GetNextDouble();
		ExgCh14  = parser.GetNextDouble();
		ExgCh15  = parser.GetNextDouble();
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
	
	
	//  Destructor
	virtual ~Cyton16Sample()
	{
	}
	
	
	//  Get EXG data for a channel
	virtual double GetExg(int channel)
	{
		switch (channel)
		{
		case 0:
			return ExgCh0;
		case 1:
			return ExgCh1;
		case 2:
			return ExgCh2;
		case 3:
			return ExgCh3;
		case 4:
			return ExgCh4;
		case 5:
			return ExgCh5;
		case 6:
			return ExgCh6;
		case 7:
			return ExgCh7;
		case 8:
			return ExgCh8;
		case 9:
			return ExgCh9;
		case 10:
			return ExgCh10;
		case 11:
			return ExgCh11;
		case 12:
			return ExgCh12;
		case 13:
			return ExgCh13;
		case 14:
			return ExgCh14;
		case 15:
			return ExgCh15;
		default:
			return MISSING_VALUE;
		}
	}
	
	
	//  Get accelerometer data for a channel
	virtual double GetAccel(int channel)
	{
		switch (channel)
		{
		case 0:
			return AcelCh0;
		case 1:
			return AcelCh1;
		case 2:
			return AcelCh2;
		default:
			return MISSING_VALUE;
		}
	}
	
	
	//  Convert to a raw sample
	virtual void AsRawSample(double* sample)
	{					
		sample[0] = SampleIndex;
		sample[1] = ExgCh0;
		sample[2] = ExgCh1;
		sample[3] = ExgCh2;
		sample[4] = ExgCh3;
		sample[5] = ExgCh4;
		sample[6] = ExgCh5;
		sample[7] = ExgCh6;
		sample[8] = ExgCh7;
		sample[9] = ExgCh8;
		sample[10] = ExgCh9;
		sample[11] = ExgCh10;
		sample[12] = ExgCh11;
		sample[13] = ExgCh12;
		sample[14] = ExgCh13;
		sample[15] = ExgCh14;
		sample[16] = ExgCh15;
		sample[17] = AcelCh0;
		sample[18] = AcelCh1;
		sample[19] = AcelCh2;
		sample[20] = Other0;
		sample[21] = Other1;
		sample[22] = Other2;
		sample[23] = Other3;
		sample[24] = Other4;
		sample[25] = Other5;
		sample[26] = Other6;
		sample[27] = AngCh0;
		sample[28] = AngCh1;
		sample[29] = AngCh2;
		sample[30] = TimeStamp;
	}
	
	
	//  Convert to json
	virtual void AsJson(std::string& json)
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
		j["ExgCh8"] = ExgCh8;
		j["ExgCh9"] = ExgCh9;
		j["ExgCh10"] = ExgCh10;
		j["ExgCh11"] = ExgCh11;
		j["ExgCh12"] = ExgCh12;
		j["ExgCh13"] = ExgCh13;
		j["ExgCh14"] = ExgCh14;
		j["ExgCh15"] = ExgCh15;
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
		
		json = j.dump();
	}
	

	

};