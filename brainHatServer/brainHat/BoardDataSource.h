#pragma once

#include <fstream>
#include <string>
#include <chrono>
#include <list>
#include <functional>

#include "Thread.h"
#include "board_shim.h"
#include "BFSample.h"
#include "TimeExtensions.h"

//  Board connection states
enum BoardConnectionStates
{
	New,
	PowerOn,
	PowerOff,
	Connected,
	Disconnected,
	StreamOn,
	StreamOff,
};

//  Sensor connection state changed event
//  callback function for C code
//  state, boardId, sampleRate
typedef void(*ConnectionChangedCallbackFn)(BoardConnectionStates, int, int);
//  callback function C++ class
typedef std::function<void(BoardConnectionStates, int, int)> ConnectionChangedDelegateFn;

typedef void(*NewSampleCallbackFn)(BFSample* sample);


class BoardDataSource : public Thread
{
public:
	BoardDataSource();
	BoardDataSource(ConnectionChangedCallbackFn connectionChangedFn, NewSampleCallbackFn newSampleFn);
	virtual ~BoardDataSource();
	
	virtual int Start(int boardId, struct BrainFlowInputParams params, bool srb1On) = 0;
	
	void RegisterConnectionChangedDelegate(ConnectionChangedDelegateFn connectionChangedDel);
	
	bool GetIsConnected() { return IsConnected;}
		
	bool Enabled() { return BoardOn;}
	void EnableBoard(bool enable);
	
	int GetBoardId() { return BoardId; }
	int GetSampleRate() { return SampleRate; }
	
	virtual int GetSrb1(int board) { return -1;	}
	virtual bool GetIsStreamRunning() { return StreamRunning;}
	
	virtual bool RequestSetSrb1(int board, bool enable) { return false;}
	virtual bool RequestEnableStreaming(bool enable) { return false;}
	
	virtual void EnableRawConsole(bool enable) { return ;}
		
protected:
	
	virtual void Init();
	
	virtual std::string ReportSource() = 0;
	
	//  basic properties
	int BoardId;
	int SampleRate;
	int DataRows;
	//
	int ExgChannelCount;
	int AccelChannelCount;
	int OtherChannelCount;
	int AnalogChannelCount;
	//
	bool BoardOn;
	bool IsConnected;
	bool StreamRunning;

	
	
	//  inspecting data mechanisms
	double LastSampleIndex;
	double LastTimeStampSync;
	int CountMissingIndex;
	void InspectSampleIndexDifference(double nextIndex);
	int SampleIndexDifference(double nextIndex);
	
	int NumberOfSamplesCounted;
	ChronoTimer InspectDataStreamLogTimer;
	void InspectDataStream(BFSample* data);
	std::list<BFSample*> DataInspecting;
	
	void ConnectionChanged(BoardConnectionStates state, int boardId, int sampleRate);
	
	NewSampleCallbackFn NewSampleCallback;
	ConnectionChangedCallbackFn ConnectionChangedCallback;
	ConnectionChangedDelegateFn ConnectionChangedDelegate;
};