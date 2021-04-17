#pragma once
#include <fstream>
#include <string>
#include <chrono>
#include <list>
#include <functional>

#include "BoardDataSource.h"
#include "board_shim.h"
#include "BFSample.h"
#include "TimeExtensions.h"
#include "CytonBoardSettings.h"


class ContecDataReader : public BoardDataSource
{
public:
	ContecDataReader(ConnectionChangedCallbackFn connectionChangedFn, NewSampleCallbackFn newSampleFn);
	virtual ~ContecDataReader();
	
	int Start(int board_id, struct BrainFlowInputParams params, bool srb1On);
	
	virtual void Cancel();
	
	virtual void RunFunction();

	virtual int GetSrb1(int board);
	virtual bool GetIsStreamRunning() { return StreamRunning;}
	
	virtual bool RequestSetSrb1(int board, bool enable) {return false;}
	virtual bool RequestEnableStreaming(bool enable);
	
protected:
	
	virtual std::string ReportSource();
	
	virtual void Init();

	//  The Board data source
	std::string ComPort;
	int BoardComPortFd;
	bool StreamRunning;
	bool IsConnected;
	bool RequestToggleStreaming;

	//
	bool BoardReady();
	int	 InitializeBoard();
	void InitializeDataReadCounters();
	void ReleaseBoard();

	bool StartStreaming();
	void StopStreaming();
	
	//  Run function reading loop
	ChronoTimer ReadTimer;
	int InvalidSampleCounter;
	//
	void EstablishConnectionWithBoard();
	bool PreparedToReadBoard();
	void ProcessData(double *sample);
	BFSample* ParseRawData(double *sample);
	void CalculateReadingTimeThisChunk(double** chunk, int samples, double& period, double& oldestSampleTime);
	
	void FindHeader();
	
};