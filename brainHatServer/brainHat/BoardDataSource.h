#pragma once

#include <fstream>
#include <string>
#include <chrono>
#include <list>
#include <functional>

#include "Thread.h"
#include "board_shim.h"
#include "BFSample.h"
#include "TimeExtensions.h"



class BoardDataSource : public Thread
{
public:
	BoardDataSource();

	virtual ~BoardDataSource();
	
	
	int GetBoardId() { return BoardId; }
	int GetSampleRate() { return SampleRate; }
	

	
protected:
	
	virtual void Init();
	
	int BoardId;
	int SampleRate;
	int DataRows;
	int TimeStampIndex;
	
	
	
	double LastSampleIndex;
	double LastTimeStampSync;
	int CountMissingIndex;
	void InspectSampleIndexDifference(double nextIndex);
	int SampleIndexDifference(double nextIndex);
	
			
	ChronoTimer ReadTimer;
	
	//  Data inspection mechanisms
	int RecordsLogged;
	ChronoTimer InspectDataStreamLogTimer;
	void InspectDataStream(BFSample* data);
	std::list<BFSample*> DataInspecting;
	
};