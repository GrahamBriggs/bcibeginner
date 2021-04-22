#pragma once
#include "json.hpp"

#define MISSING_VALUE -9.99e99

class BFSample
{
public:
	
	BFSample()
	{
	}
	
	
	virtual ~BFSample()
	{
	}
	
	virtual BFSample* Copy() = 0;
		
	double SampleIndex;
	double TimeStamp;
	
	
	virtual int GetNumberOfExgChannels() = 0;
	virtual double GetExg(int channel) = 0;
	
	virtual int GetNumberOfAccelChannels() = 0;
	virtual double GetAccel(int channel) = 0;
	
	virtual int GetNumberOfOtherChannels() = 0;
	virtual double GetOther(int channel) = 0;
	
	virtual int GetNumberOfAnalogChannels() = 0;
	virtual double GetAnalog(int channel) = 0;
	
	//  Sample Size is:
	//		- Sample Index
	//		- Number of EXG Channels
	//		- Number of Accelerometer Channels
	//		- Number of Other Channels
	//		- Number of Analog Channels
	//		- Time Stamp
	virtual inline int SampleSize()
	{
		return (
			1 +
			GetNumberOfExgChannels() +
			GetNumberOfAccelChannels() + 
			GetNumberOfOtherChannels() + 
			GetNumberOfAnalogChannels() + 
			1);
	}


	virtual void AsRawSample(double* sample) = 0;
	
	virtual void AsJson(std::string& json) = 0;
	
protected:
};

