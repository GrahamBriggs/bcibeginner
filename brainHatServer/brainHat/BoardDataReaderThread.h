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

//  Sensor connection state changed event
//  callback function for C code
typedef void(*ConnectionChangedCallbackFn)(int);
//  callback function C++ class
typedef std::function<void(int)> ConnectionChangedDelegateFn;


class BoardDataReader : public BoardDataSource
{
public:
	BoardDataReader();
	BoardDataReader(ConnectionChangedCallbackFn cbf);
	virtual ~BoardDataReader();
	
	int Start(int board_id, struct BrainFlowInputParams params);
	
	virtual void Cancel();
	
	virtual void RunFunction();
	
	void RegisterConnectionChangedDelegate(ConnectionChangedDelegateFn cbf);
	
	bool Enabled() { return BoardOn;}
	void EnableBoard(bool enable);
	
protected:
	
	virtual void Init();
	
	BoardShim* Board;
	struct BrainFlowInputParams BoardParamaters;

	bool BoardOn;
	
	BFSample* ParseRawData(double** data, int sample);
	void CalculateReadingTimeThisChunk(double** chunk, int samples, double& period, double& oldestSampleTime);
	int	 InitializeBoard();
	void ReleaseBoard();
	void ReconnectToBoard();
	
	//  Count invalid points for reconnection trigger
	int InvalidSampleCounter;
	
	//  Process the chunk read from the board
	void ProcessData(double **data_buf, int sampleCount);
				
	ChronoTimer ReadTimer;
	

	
	ConnectionChangedCallbackFn ConnectionChangedCallback;
	ConnectionChangedDelegateFn ConnectionChangedDelegate;
	
	void ConnectionChanged(int state);
};