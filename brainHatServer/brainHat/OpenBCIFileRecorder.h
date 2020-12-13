#pragma once
#include <queue>
#include <condition_variable>
#include <lsl_cpp.h>
#include <fstream>

#include "Thread.h"
#include "BFSample.h"
#include "TimeExtensions.h"

//  File recording class
//
class OpenBCIFileRecorder : public Thread
{
public:
	OpenBCIFileRecorder();
	virtual ~OpenBCIFileRecorder();
	
	bool StartRecording(std::string fileName, int boardId, int sampleRate);
	virtual void Cancel();
	virtual void RunFunction();
	
	void AddData(BFSample* data);
	
	bool IsRecording() {return Recording;}
	double ElapsedRecordingTime() {return ElapsedTime.ElapsedSeconds();}
	std::string FileName() { return RecordingFileName;}

protected:
		
	int BoardId;
	int SampleRate;
	
	bool OpenFile(std::string fileName);
	
	std::string RecordingFileName;
	
	bool WroteHeader;
	void WriteHeader(BFSample* firstSample);
	void WriteSample(BFSample* sample);
	
	bool Recording;
	std::mutex RecordingFileMutex;
	std::ofstream RecordingFile;
	
	void WriteDataToFile();
	
	//  queue lock
	std::mutex QueueMutex;
	std::queue<BFSample*> SamplesQueue;
	
	ChronoTimer ElapsedTime;
};

