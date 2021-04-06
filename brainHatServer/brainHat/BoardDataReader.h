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


class BoardDataReader : public BoardDataSource
{
public:
	BoardDataReader(ConnectionChangedCallbackFn connectionChangedFn, NewSampleCallbackFn newSampleFn);
	virtual ~BoardDataReader();
	
	int Start(int board_id, struct BrainFlowInputParams params);
	
	virtual void Cancel();
	
	virtual void RunFunction();

	virtual int GetSrb1(int board);
	virtual bool GetIsStreamRunning() { return StreamRunning;}
	
	virtual bool RequestSetSrb1(int board, bool enable);
	virtual bool RequestEnableStreaming(bool enable);
	
protected:
	
	virtual std::string ReportSource();
	
	virtual void Init();

	//  The Board
	BoardShim* Board;
	struct BrainFlowInputParams BoardParamaters;
	bool StreamRunning;
	bool IsConnected;
	bool RequestToggleStreaming;
	bool StartSrb1CytonSet;
	bool StartSrb1DaisySet;
	//
	bool BoardReady();
	int	 InitializeBoard();
	void InitializeDataReadCounters();
	void ReleaseBoard();
	void DiscardFirstChunk();
	void StartStreaming();
	void StopStreaming();
	
	//  Run function reading loop
	ChronoTimer ReadTimer;
	int InvalidSampleCounter;
	//
	void EstablishConnectionWithBoard();
	bool PreparedToReadBoard();
	void ProcessData(double **data_buf, int sampleCount);
	BFSample* ParseRawData(double** data, int sample);
	void CalculateReadingTimeThisChunk(double** chunk, int samples, double& period, double& oldestSampleTime);
	
	
	//  Board hardware settings and configuration commands
	std::timed_mutex CommandsQueueLock;
	std::queue<std::string> CommandsQueue;
	void ProcessCommandsQueue();
	//
	bool LoadBoardRegistersSettings();
	bool GetRegistersString(std::string& registersString);
	bool ValidateRegisterSettingsString(std::string registerSettings);
	bool ValidateFirmwareString(std::string firmware);	
	CytonBoards BoardSettings;
	

};