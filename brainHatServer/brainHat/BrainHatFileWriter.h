#pragma once
#include <string>
#include <queue>
#include <condition_variable>
#include "Thread.h"
#include "BFSample.h"
#include "TimeExtensions.h"

#define RECORDINGFOLDER ("/home/pi/bhRecordings/")

bool CheckRecordingFolder();


class BrainHatFileWriter : public Thread
{
	
public:
	BrainHatFileWriter();
	virtual ~BrainHatFileWriter();
	
	virtual bool StartRecording(std::string fileName, int boardId, int sampleRate);
	
	virtual void Cancel();
	virtual void RunFunction();
	
		
	void AddData(BFSample* data);
	
	bool IsRecording() {return Recording;}
	double ElapsedRecordingTime() {return ElapsedTime.ElapsedSeconds();}
	std::string FileName() { return RecordingFileName;}
	
protected:
	
	std::string RecordingFileName;
	std::string RecordingFileFullPath;
	
	int BoardId;
	int SampleRate;
	
	virtual void WriteDataToFile() = 0;
	
	void SetFilePath(std::string fileRootName, std::string extension);
	virtual bool OpenFile(std::string fileName) = 0;
	virtual void CloseFile() = 0;
	
	bool WroteHeader;

	
	bool Recording;
	std::mutex RecordingFileMutex;
	
	//  queue lock
	std::mutex QueueMutex;
	std::queue<BFSample*> SamplesQueue;
	
	ChronoTimer ElapsedTime;
	
};
