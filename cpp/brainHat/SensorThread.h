#pragma once
#include <fstream>
#include <string>
#include <chrono>
#include <list>

#include "Thread.h"
#include "board_shim.h"
#include "OpenBciData.h"




class SensorThread : public Thread
{
public:
	SensorThread();
	virtual ~SensorThread();
	
	int Start(int board_id, struct BrainFlowInputParams params);
	
	virtual void Cancel();
	
	virtual void RunFunction();
	
	
protected:
	
	struct BrainFlowInputParams BoardParamaters;
	int BoardId;
	BoardShim* Board;
	
	int InitializeBoard();
	void ReleaseBoard();
	void ReconnectToBoard();
	
	//  Count invalid points for reconnection trigger
	int NumInvalidPointsCounter;
	
	//  Send read data to the UDP broadcast
	void SendDataToBroadcast(double **data_buf, int num_channels, int num_data_points);
	
			
	std::chrono::steady_clock::time_point StartTime;
	
	//  Data inspection mechanisms
	std::chrono::steady_clock::time_point LastLoggedTime;
	int RecordsLogged;
	void InspectDataStream(OpenBciData* data);
	std::list<OpenBciData*> DataInspecting;
	
	
};