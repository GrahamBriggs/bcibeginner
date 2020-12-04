#pragma once
#include "json.hpp"

#define MISSING_VALUE -9.99e99

class BFSample
{
public:
	
	BFSample()
	{
	}
	
	//  CopyConstructor
	BFSample(BFSample* copy)
	{
		SampleIndex = copy->SampleIndex;
		TimeStamp = copy->TimeStamp;
	}
	
	virtual ~BFSample()
	{
	}
	
	double SampleIndex;
	double TimeStamp;
	
	virtual double GetExg(int channel)
	{
		return MISSING_VALUE;
	}
	
	virtual double GetAccel(int channel)
	{
		return MISSING_VALUE;
	}
	
	virtual void AsRawSample(double* sample)
	{
		
	}
	
	virtual void AsJson(std::string& json)
	{
	}
	
protected:
};
