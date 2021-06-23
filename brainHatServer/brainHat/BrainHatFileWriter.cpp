#include <string>
#include <dirent.h>
#include <sstream>
#include <iostream>
#include <iomanip>
#include "BrainHatFileWriter.h"
#include "StringExtensions.h"
#include "FileExtensions.h"
#include "brainHat.h"


using namespace std;

BrainHatFileWriter::BrainHatFileWriter(RecordingStateChangedCallbackFn fn)
{
	Recording = false;
	RecordingStateChangedCallback = fn;
}

BrainHatFileWriter::~BrainHatFileWriter()
{
	
}


// Start recording, opens the file and kicks off a thread to write to the file
//
bool BrainHatFileWriter::StartRecording(string fileName, bool tryUsb, int boardId, int sampleRate, FileHeaderInfo info)
{	
	HeaderInfo = info;
	
	if (OpenFile(fileName, tryUsb))
	{
		BoardId = boardId;
		SampleRate = sampleRate;
	
		WroteHeader = false;
		Recording = true;
		ElapsedTime.Start();
		Thread::Start();
		return true;
	}
	
	return false;
}


//  Cancel thread, close the file
//
void BrainHatFileWriter::Cancel()
{
	Thread::Cancel();	
	Recording = false;
	CloseFile();
}


//  Find the path to the USB drive folder
//  return empty string if no USB drive found
string FindUsbDrive(const char *path)
{
	struct dirent *dp;
	DIR *dir = opendir(path);

	// Unable to open directory stream
	if(!dir) 
	    return ""; 

	while ((dp = readdir(dir)) != NULL)
	{
		if (string(dp->d_name).compare(".") != 0 && string(dp->d_name).compare("..") != 0)
		{
			closedir(dir); 
			return format("/media/pi/%s",dp->d_name);
		}
	}

	// Close directory stream
	closedir(dir);
	return "";
}


//  Check that the default folder exists, if not create it
//
bool CreateDefaultFolder()
{
	//  check to see that our recording folder exists
	DIR* dir = opendir(RECORDINGFOLDER);
	if (dir) 	
	{
		closedir(dir);
	}
	else if (ENOENT == errno) 
	{
		//  does not exist, make it
		if(!MakePath(RECORDINGFOLDER))
		{
			Logging.AddLog("BrainHatFileWriter", "CheckRecordingFolder", format("Failed to create directory %s", RECORDINGFOLDER), LogLevelError);
			return false;
		}
	}
	else 
	{
		//  some other directory error
		Logging.AddLog("BrainHatFileWriter", "CheckRecordingFolder", "Directory error.", LogLevelError);
		return false;
	}
	
	return true;
}


//  Check that the recording folder exists, if not create it
//
bool CheckRecordingFolder(string fileName, bool tryUsb, std::string& pathToRecFolder)
{
	//  set the path for file recording
	string rootPath = "";
	if (tryUsb)
		rootPath = FindUsbDrive("/media/pi");
	if (rootPath.length() == 0)
	{
		if (!CreateDefaultFolder())
			return false;
		
		rootPath = RECORDINGFOLDER;
	}
	
	pathToRecFolder = format("%s/%s/", rootPath.c_str(), fileName.c_str());
	
	//  check to see that our recording folder exists
	DIR* dir = opendir(pathToRecFolder.c_str());
	if (dir) 	
	{
		closedir(dir);
	}
	else if (ENOENT == errno) 
	{
		//  does not exist, make it
		if(!MakePath(pathToRecFolder.c_str()))
		{
			Logging.AddLog("BrainHatFileWriter", "CheckRecordingFolder", format("Failed to create directory %s", pathToRecFolder.c_str()), LogLevelError);
			pathToRecFolder = "";
			return false;
		}
	}
	else 
	{
		//  some other directory error
		Logging.AddLog("BrainHatFileWriter", "CheckRecordingFolder", "Directory error opening recording file.", LogLevelError);
		pathToRecFolder = "";
		return false;
	}
	
	return true;
}	



//  Add data to the queue
//
void BrainHatFileWriter::AddData(BFSample* data)
{
	if (!Recording)
		return;
	
	{
		LockMutex lockQueue(QueueMutex);
		SamplesQueue.push(data);
	}
}


//  Run function
//
void BrainHatFileWriter::RunFunction()
{
	while (ThreadRunning)
	{		
		Sleep(10);
		WriteDataToFile();
	}
}


void BrainHatFileWriter::SetFilePath(string pathToRecFolder, string sessionName, string extension)
{
	timeval tv;
	gettimeofday(&tv, NULL);
	tm* timeNow = localtime(&(tv.tv_sec));

	//  create file name from test name and start time
	ostringstream os;		
	os << sessionName << "_" << setw(4) << timeNow->tm_year + 1900 << setfill('0') << setw(2) << timeNow->tm_mon + 1 << setfill('0') << setw(2) << timeNow->tm_mday << "-" << setfill('0') << setw(2) << timeNow->tm_hour << setfill('0') << setw(2) << timeNow->tm_min  << setfill('0') << setw(2) << timeNow->tm_sec << "." << extension;	
	RecordingFileName = os.str();	
	os.str("");
	os << pathToRecFolder << RecordingFileName;	
	RecordingFileFullPath = os.str();
}


	
	
