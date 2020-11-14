#pragma once
#include <fstream>
#include <string>
#include <chrono>

#include "Thread.h"





class SensorThread : public Thread
{
public:
	SensorThread();
	virtual ~SensorThread();
	
	virtual void Start();
	virtual void Cancel();
	
	virtual void RunFunction();
	
	void StartLogging(std::string testName);
	void StopLogging();

	
protected:
	
	
	void SaveDataToFile(double* data_buf);
	
	std::mutex LogFileMutex;
	std::ofstream LogFile;
	
	std::chrono::steady_clock::time_point StartTime;
	bool Logging;
	
	std::chrono::steady_clock::time_point LastLoggedTime;
	int RecordsLogged;

};