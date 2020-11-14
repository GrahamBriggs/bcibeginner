#pragma once
#include <fstream>
#include <string>
#include <chrono>
#include <list>

#include "Thread.h"
#include "board_shim.h"
#include "OpenBciData.h"




class FileSimulatorThread : public Thread
{
public:
	FileSimulatorThread();
	virtual ~FileSimulatorThread();
	
	int Start(std::string fileName);
	
	virtual void Cancel();
	
	virtual void RunFunction();
	
	bool LoadFile(std::string fileName);
	
protected:
	
	double StartTime;
	double EndTime;
	
	std::list<OpenBciData*> DataRecords;
	
	
};