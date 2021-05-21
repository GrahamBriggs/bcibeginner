#pragma once
#include <string>
#include "json.hpp"
#include "Parser.h"
#include "BFSample.h"
#include "StringExtensions.h"
#include "BoardIds.h"

class Sample : public BFSample
{
protected:
	
	int ExgChannelCount;
	double* ExgData;
	int AccelChannelCount;
	double* AccelData;
	int OtherChannelCount;
	double* OtherData;
	int AnalogChannelCount;
	double* AnalogData;
	
	
public:
	
	//  Generic EEG sample with X channels
	Sample(int exgChannels, int accelChannels, int otherChannels, int analogChannels)
	{
		ExgData = NULL;
		ExgChannelCount = exgChannels;
		if (exgChannels > 0)
			ExgData = new double[exgChannels];
		
		AccelData = NULL;
		AccelChannelCount = accelChannels;
		if (accelChannels > 0)
			AccelData = new double[accelChannels];
		
		OtherData = NULL;
		OtherChannelCount = otherChannels;
		if (otherChannels > 0)
			OtherData = new double[otherChannels];
		
		AnalogData = NULL;
		AnalogChannelCount = analogChannels;
		if (analogChannels > 0)
			AnalogData = new double[analogChannels];
		
		Init();
	}
	
	//  Generic EEG sample with X channels
	Sample(int boardId)
	{
		ExgChannelCount = getNumberOfExgChannels(boardId);
		AccelChannelCount = getNumberOfAccelChannels(boardId);
		OtherChannelCount = getNumberOfOtherChannels(boardId);
		AnalogChannelCount = getNumberOfAnalogChannels(boardId);
			
		ExgData = NULL;
		if (ExgChannelCount > 0)
			ExgData = new double[ExgChannelCount];
		
		AccelData = NULL;
		if (AccelChannelCount > 0)
			AccelData = new double[AccelChannelCount];
		
		OtherData = NULL;
		if (OtherChannelCount > 0)
			OtherData = new double[OtherChannelCount];
		
		AnalogData = NULL;
		if (AnalogChannelCount > 0)
			AnalogData = new double[AnalogChannelCount];
		
		Init();
	}

	//  Copy Constructor
	Sample(BFSample* copy)
	{
		TimeStamp = copy->TimeStamp;
		SampleIndex = copy->SampleIndex;
		
		ExgData = NULL;
		ExgChannelCount = copy->GetNumberOfExgChannels();
		if (copy->GetNumberOfExgChannels() > 0)
		{
			ExgData = new double[copy->GetNumberOfExgChannels()];
			for (int i = 0; i < copy->GetNumberOfExgChannels(); i++)
				ExgData[i] = copy->GetExg(i);
		}
		
		AccelData = NULL;
		AccelChannelCount = copy->GetNumberOfAccelChannels();
		if (copy->GetNumberOfAccelChannels() > 0)
		{
			AccelData = new double[copy->GetNumberOfAccelChannels()];
			for (int i = 0; i < copy->GetNumberOfAccelChannels(); i++)
				AccelData[i] = copy->GetAccel(i);
		}
		
		OtherData = NULL;
		OtherChannelCount = copy->GetNumberOfOtherChannels();
		if (copy->GetNumberOfOtherChannels() > 0)
		{
			OtherData = new double[copy->GetNumberOfOtherChannels()];
			for (int i = 0; i < copy->GetNumberOfOtherChannels(); i++)
				OtherData[i] = copy->GetOther(i);
		}
		
		AnalogData = NULL;
		AnalogChannelCount = copy->GetNumberOfAnalogChannels();
		if (copy->GetNumberOfAnalogChannels() > 0)
		{
			AnalogData = new double[copy->GetNumberOfAnalogChannels()];
			for (int i = 0; i < copy->GetNumberOfAnalogChannels(); i++)
				AnalogData[i] = copy->GetAnalog(i);
		}
	}
	
	
	//  Construct from a brainflow get_board_data chunk
	void InitializeFromChunk(double** chunk, int sampleIndex)
	{
		int indexCount = 0;
		SampleIndex = chunk[indexCount++][sampleIndex];
		
		for(int i = 0; i < ExgChannelCount; i++)
			ExgData[i] = chunk[indexCount++][sampleIndex];
		
		for (int i = 0; i < AccelChannelCount; i++)
			AccelData[i] = chunk[indexCount++][sampleIndex];
		
		for (int i = 0; i < OtherChannelCount; i++)
			OtherData[i] = chunk[indexCount++][sampleIndex];
		
		for (int i = 0; i < AnalogChannelCount; i++)
			AnalogData[i] = chunk[indexCount++][sampleIndex];

		TimeStamp = chunk[indexCount][sampleIndex];
	}
	
	
	//  Construct from OpenBCI_GUI format text file raw data string
	void InitializeFromText(std::string rawData)
	{
		Parser parser(rawData, ",");
		SampleIndex = parser.GetNextDouble();
		
		for (int i = 0; i < ExgChannelCount; i++)
			ExgData[i] = parser.GetNextDouble();
		
		for (int i = 0; i < AccelChannelCount; i++)
			AccelData[i] = parser.GetNextDouble();
		
		for (int i = 0; i < OtherChannelCount; i++)
			OtherData[i] = parser.GetNextDouble();
		
		for (int i = 0; i < AnalogChannelCount; i++)
			AnalogData[i] = parser.GetNextDouble();
		
		TimeStamp = parser.GetNextDouble();
	}
	
	//  Destructor
	virtual ~Sample()
	{
		if (ExgData != NULL)
			delete ExgData;
		if (AccelData != NULL)
			delete AccelData;
		if (OtherData != NULL)
			delete OtherData;
		if (AnalogData != NULL)
			delete AnalogData;
	}
	
	void Init()
	{
		SampleIndex = MISSING_VALUE;
		
		for (int i = 0; i < ExgChannelCount; i++)
			ExgData[i] = MISSING_VALUE;
		
		for (int i = 0; i < AccelChannelCount; i++)
			AccelData[i] = MISSING_VALUE;
		
		for (int i = 0; i < OtherChannelCount; i++)
			OtherData[i] = MISSING_VALUE;
		
		for (int i = 0; i < AnalogChannelCount; i++)
			AnalogData[i] = MISSING_VALUE;
		
		TimeStamp = MISSING_VALUE;
	}
		
	
	virtual BFSample* Copy()
	{
		return new Sample(this);
	}
	
	
	//  EXG Channels
	virtual int GetNumberOfExgChannels() { return ExgChannelCount;}
	//
	virtual double GetExg(int channel)
	{
		if (channel < ExgChannelCount)
			return ExgData[channel];
		else
			return MISSING_VALUE;
	}
	//
	virtual void SetExg(int channel, double value)
	{
		if (channel < ExgChannelCount)
			ExgData[channel] = value;
	}
	
	
	//  Accelerometer Channels
	virtual int GetNumberOfAccelChannels() { return AccelChannelCount;}
	//
	virtual double GetAccel(int channel)
	{
		if (channel < AccelChannelCount)
			return AccelData[channel];
		else
			return MISSING_VALUE;
	}
	//
	virtual void SetAccel(int channel, double value)
	{
		if (channel < AccelChannelCount)
			AccelData[channel] = value;
	}
	
	//  Other Channels
	virtual int GetNumberOfOtherChannels() { return OtherChannelCount;}
	//
	virtual double GetOther(int channel)
	{
		if (channel < OtherChannelCount)
			return OtherData[channel];
		else
			return MISSING_VALUE;
	}
	//
	virtual void SetOther(int channel, double value)
	{
		if (channel < OtherChannelCount)
			OtherData[channel] = value;
	}
	
	//  Analog Channels
	virtual int GetNumberOfAnalogChannels() { return AnalogChannelCount;}
	//
	virtual double GetAnalog(int channel)
	{
		if (channel < AnalogChannelCount)
			return AnalogData[channel];
		else
			return MISSING_VALUE;
	}
	//
	virtual void SetAnalog(int channel, double value)
	{
		if (channel < AnalogChannelCount)
			AnalogData[channel] = value;
	}
	
	
	//  Convert to a raw sample
	virtual void AsRawSample(double* sample)
	{		
		int indexCount = 0;
		sample[indexCount++] = SampleIndex;
		
		for (int i = 0; i < ExgChannelCount; i++)
			sample[indexCount++] = ExgData[i];
		
		for (int i = 0; i < AccelChannelCount; i++)
			sample[indexCount++] = AccelData[i];
		
		for (int i = 0; i < OtherChannelCount; i++)
			sample[indexCount++] = OtherData[i];
		
		for (int i = 0; i < AnalogChannelCount; i++)
			sample[indexCount++] = AnalogData[i];
		
		sample[indexCount] = TimeStamp;
	}
	
	
	//  Convert to json
	virtual void AsJson(std::string& json)
	{					
		nlohmann::json j;

		j["SampleIndex"] = SampleIndex;
		
		for (int i = 0; i < ExgChannelCount; i++)
		{
			j[format("ExgCh%d", i).c_str()] = ExgData[i];
		}
		
		for (int i = 0; i < AccelChannelCount; i++)
		{
			j[format("AcelCh%d", i).c_str()] = AccelData[i];
		}
		
		for (int i = 0; i < OtherChannelCount; i++)
		{
			j[format("Other%d", i).c_str()] = OtherData[i];
		}
		
		for (int i = 0; i < AnalogChannelCount; i++)
		{
			j[format("AngCh%d", i).c_str()] = AnalogData[i];
		}
		
		j["TimeStamp"] = TimeStamp;
		
		json = j.dump();
	}
	

};