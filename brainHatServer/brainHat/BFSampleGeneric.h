#pragma once
#include <string>
#include "json.hpp"
#include "Parser.h"
#include "BFSample.h"
#include "StringExtensions.h"

class GenericSample : public BFSample
{
protected:
	
	double* ExgData;
	int ChannelCount;
	
public:
	
	//  Generic EEG sample with X channels
	GenericSample(int channels)
	{
		ExgData = new double[channels];
		Init();
	}

	//  Copy Constructor
	GenericSample(GenericSample* copy)
	{
		TimeStamp = copy->TimeStamp;
		SampleIndex = copy->SampleIndex;
		
		ExgData = new double[copy->GetNumberOfExgChannels()];
		for (int i = 0; i < copy->GetNumberOfExgChannels(); i++)
			ExgData[i] = copy->GetExg(i);
	}
	
	//  Destructor
	virtual ~GenericSample()
	{
		delete ExgData;
	}
	
	void Init()
	{
		SampleIndex = MISSING_VALUE;
		
		for (int i = 0; i < ChannelCount; i++)
			ExgData[i] = MISSING_VALUE;
		
		TimeStamp = MISSING_VALUE;
	}
		
	
	virtual BFSample* Copy()
	{
		return new GenericSample(this);
	}
	
	
	//  EXG Channels
	virtual int GetNumberOfExgChannels() { return 16;}
	//
	virtual double GetExg(int channel)
	{
		if (channel < ChannelCount)
			return ExgData[channel];
		else
			return MISSING_VALUE;
	}
	
	
	//  Accelerometer Channels
	virtual int GetNumberOfAccelChannels() { return 0;}
	//
	virtual double GetAccel(int channel)
	{
		return MISSING_VALUE;
	}
	
	//  Other Channels
	virtual int GetNumberOfOtherChannels() { return 0;}
	//
	virtual double GetOther(int channel)
	{
			return MISSING_VALUE;
	}
	
	//  Analog Channels
	virtual int GetNumberOfAnalogChannels() { return 0;}
	//
	virtual double GetAnalog(int channel)
	{
			return MISSING_VALUE;
	}
	
	
	//  Convert to a raw sample
	virtual void AsRawSample(double* sample)
	{					
		sample[0] = SampleIndex;
		for (int i = 0; i < ChannelCount; i++)
			sample[1 + i] = ExgData[i];
		sample[1 + ChannelCount] = TimeStamp;
	}
	
	//  Convert to json
	virtual void AsJson(std::string& json)
	{					
		nlohmann::json j;

		j["SampleIndex"] = SampleIndex;
		
		for (int i = 0; i < ChannelCount; i++)
		{
			j[format("ExgCh%d",i).c_str()] = ExgData[i];
		}
		
		j["TimeStamp"] = TimeStamp;
		
		json = j.dump();
	}
	

};