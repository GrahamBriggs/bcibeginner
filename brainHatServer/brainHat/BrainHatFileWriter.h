#pragma once
#include <string>
#include <queue>
#include <condition_variable>
#include "Thread.h"
#include "BFSample.h"
#include "TimeExtensions.h"

#define RECORDINGFOLDER ("/home/pi/EEG")

struct FileHeaderInfo
{
public:
	std::string SessionName;
	std::string SubjectName;
	std::string SubjectCode;
	std::string SubjectBirthday;
	std::string SubjectAdditional;
	std::string SubjectGender;
	std::string AdminCode;
	std::string Technician;
	std::string Device;
	
	int GetGender()
	{
		if (SubjectGender == "0")
			return 0;
		return 1;
	}
	
	int GetBirthdayYear()
	{
		if (SubjectBirthday.length() > 3)
		{
			int year = std::stoi(SubjectBirthday.substr(0, 4));
			return year;
		}
		return 0;
	}
	
	int GetBirthdayMonth()
	{
		if (SubjectBirthday.length() > 5)
			return std::stoi(SubjectBirthday.substr(4, 2));
		return 0;
	}
	
	int GetBirthdayDay()
	{
		if (SubjectBirthday.length() > 7)
			return std::stoi(SubjectBirthday.substr(6, 2));
		return 0;
	}
	
	
	
};

bool CheckRecordingFolder(std::string sessionName, bool tryUsb, std::string& pathToRecFolder);

typedef void(*RecordingStateChangedCallbackFn)(bool);

class BrainHatFileWriter : public Thread
{
	
public:
	BrainHatFileWriter(RecordingStateChangedCallbackFn fn);
	virtual ~BrainHatFileWriter();
	
	virtual bool StartRecording(std::string fileName, bool tryUsb, int boardId, int sampleRate, FileHeaderInfo info);
	
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
	FileHeaderInfo HeaderInfo;
	
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
