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

BrainHatFileWriter::BrainHatFileWriter()
{
	Recording = false;
}

BrainHatFileWriter::~BrainHatFileWriter()
{
	
}


// Start recording, opens the file and kicks off a thread to write to the file
//
bool BrainHatFileWriter::StartRecording(string fileName, int boardId, int sampleRate)
{	
	if (OpenFile(fileName))
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
	Logging.AddLog("OpenBCIFileWriter", "Cancel", format("Closed recording file %s.", RecordingFileName.c_str()), LogLevelInfo);
}



//  Check that the recording folder exists, if not create it
//
bool CheckRecordingFolder()
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
			Logging.AddLog("OpenBCIFileWriter", "OpenFile", format("Failed to create directory %s", RECORDINGFOLDER), LogLevelError);
			return false;
		}
	}
	else 
	{
		//  some other directory error
		Logging.AddLog("OpenBCIFileWriter", "OpenFile", "Directory error opening recording file.", LogLevelError);
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


void BrainHatFileWriter::SetFilePath(string fileRootName, string extension)
{
	timeval tv;
	gettimeofday(&tv, NULL);
	tm* logTime = localtime(&(tv.tv_sec));

	//  create file name from test name and start time
	ostringstream os;		
	os << fileRootName << "_" << setfill('0') << setw(2) << logTime->tm_hour << setw(2) << logTime->tm_min  << setw(2) << logTime->tm_sec << "." << extension;	
	RecordingFileName = os.str();	
	os.str("");
	os << RECORDINGFOLDER << RecordingFileName;	
	RecordingFileFullPath = os.str();
}


	
	
