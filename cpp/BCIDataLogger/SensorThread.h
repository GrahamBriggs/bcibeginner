#pragma once
#include <fstream>
#include <string>
#include <chrono>

#include "Thread.h"
#include "board_shim.h"




class SensorThread : public Thread
{
public:
	SensorThread();
	virtual ~SensorThread();
	
	int Start(int board_id, struct BrainFlowInputParams params);
	
	virtual void Cancel();
	
	virtual void RunFunction();
	
	void StartLogging(std::string testName);
	void StopLogging();

	
protected:
	
	struct BrainFlowInputParams BoardParamaters;
	int BoardId;
	BoardShim* Board;
	
	int InitializeBoard();
	void ReleaseBoard();
	void ReconnectToBoard();
	
	void SaveDataToFile(double **data_buf, int num_channels, int num_data_points);
	
	std::mutex LogFileMutex;
	std::ofstream LogFile;
	
	std::chrono::steady_clock::time_point StartTime;
	bool Logging;
	
	std::chrono::steady_clock::time_point LastLoggedTime;
	int RecordsLogged;
	
	
	int NumInvalidPointsCounter;
};