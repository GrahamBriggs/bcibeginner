#pragma once
#include <string>
#include <queue>
#include <condition_variable>
#include "Thread.h"
#include "BFSample.h"
#include "TimeExtensions.h"

#define RECORDINGFOLDER ("/home/pi/EEG")



bool CheckRecordingFolder(std::string sessionName, bool tryUsb, std::string& pathToRecFolder);

typedef void(*RecordingStateChangedCallbackFn)(bool);

class BrainHatFileWriter : public Thread
{
	
public:
	BrainHatFileWriter(RecordingStateChangedCallbackFn fn);
	virtual ~BrainHatFileWriter();
	
	virtual bool StartRecording(std::string fileName, bool tryUsb, int boardId, int sampleRate);
	
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
	
	void SetFilePath(std::string pathToRecFolder, std::string sessionName, std::string extension);
	virtual bool OpenFile(std::string fileName, bool tryUsb) = 0;
	virtual void CloseFile() = 0;
	
	bool WroteHeader;

	
	bool Recording;
	std::mutex RecordingFileMutex;
	
	//  queue lock
	std::mutex QueueMutex;
	std::queue<BFSample*> SamplesQueue;
	
	ChronoTimer ElapsedTime;
	
	RecordingStateChangedCallbackFn RecordingStateChangedCallback;
	
};
