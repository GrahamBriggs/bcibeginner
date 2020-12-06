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


class BoardDataReader : public BoardDataSource
{
public:
	BoardDataReader(ConnectionChangedCallbackFn connectionChangedFn);
	virtual ~BoardDataReader();
	
	int Start(int board_id, struct BrainFlowInputParams params);
	
	virtual void Cancel();
	
	virtual void RunFunction();

protected:
	
	virtual void Init();
	
	virtual std::string ReportSource();
	
	BoardShim* Board;
	struct BrainFlowInputParams BoardParamaters;

	//  read data on a timer
	ChronoTimer ReadTimer;
	
	BFSample* ParseRawData(double** data, int sample);
	void CalculateReadingTimeThisChunk(double** chunk, int samples, double& period, double& oldestSampleTime);
	int	 InitializeBoard();
	void ReleaseBoard();
	void ReconnectToBoard();
	
	//  Count invalid points for reconnection trigger
	int InvalidSampleCounter;
	
	//  Process the chunk read from the board
	void ProcessData(double **data_buf, int sampleCount);

};