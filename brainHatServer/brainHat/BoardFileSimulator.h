#pragma once
#include <fstream>
#include <string>
#include <chrono>
#include <list>
#include <string>
#include <brainflow_constants.h>
#include "Thread.h"
#include "board_shim.h"
#include "TimeExtensions.h"
#include "BoardDataSource.h"
#include "BFSample.h"


//  File Simulator Thread
//  Will play back an OpenBCI_GUI format .txt file (adjusting time stamps so the data appears current)
//  TODO - this is hard coded for Cyton8
//
class BoardFileSimulator : public BoardDataSource
{
public:
	BoardFileSimulator(ConnectionChangedCallbackFn connectionChangedFn, NewSampleCallbackFn newSampleFn);
	virtual ~BoardFileSimulator();
	
	int Start(std::string fileName);
	
	virtual void Cancel();
	
	virtual void RunFunction();
	
	bool LoadFile(std::string fileName);
	
protected :
	
	virtual std::string ReportSource();
		
	void ReadHeaderLine(std::string readLine);
	void AddSample(std::string readLine);
	BFSample* CopySample(BFSample* sample);
	
	
	
	double StartTime;
	double EndTime;
	
	std::string FileName;
	
	ChronoTimer LastLoggedStatusTime;
	
	std::list<BFSample*> DataRecords;

};