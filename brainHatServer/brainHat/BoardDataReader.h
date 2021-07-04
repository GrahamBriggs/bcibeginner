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
	
	virtual int Start(int boardId, struct BrainFlowInputParams params, bool srb1On);
	
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
	
	bool RequestToggleStreaming;
	bool StartSrb1CytonSet;
	bool StartSrb1DaisySet;
	//
	bool BoardReady();
	int	 InitializeBoard();
	bool InitializeSrbOnStartup();
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
	void ProcessData(BrainFlowArray<double,2>& chunk);
	BFSample* ParseRawData(BrainFlowArray<double, 2>& chunk, int sample);
	void CalculateReadingTimeThisChunk(BrainFlowArray<double, 2>& chunk, double& period, double& oldestSampleTime);
	
	
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