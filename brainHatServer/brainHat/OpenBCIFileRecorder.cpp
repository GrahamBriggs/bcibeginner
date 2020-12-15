#include <list>
#include <unistd.h>
#include <iostream>
#include "brainHat.h"
#include "OpenBCIFileRecorder.h"
#include "StringExtensions.h"
#include "TimeExtensions.h"
#include "wiringPi.h"
#include "json.hpp"
#include "NetworkAddresses.h"
#include "BFSample.h"
#include "BrainHatServerStatus.h"
#include "NetworkExtensions.h"
#include <lsl_cpp.h>
#include <iomanip>
#include <sstream>
#include <dirent.h>
#include <errno.h>
#include "FileExtensions.h"

using namespace std;
using json = nlohmann::json;
using namespace lsl;




//  Constructor
//
OpenBCIFileRecorder::OpenBCIFileRecorder()
{
	Recording = false;
}


//  Destructor
//
OpenBCIFileRecorder::~OpenBCIFileRecorder()
{
	
}


// Start recording, opens the file and kicks off a thread to write to the file
//
bool OpenBCIFileRecorder::StartRecording(string fileName, int boardId, int sampleRate)
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
void OpenBCIFileRecorder::Cancel()
{
	Thread::Cancel();	
	Recording = false;
	RecordingFile.close();
	Logging.AddLog("OpenBCIFileRecorder", "Cancel", format("Closed recording file %s.", RecordingFileName.c_str()), LogLevelInfo);
}


//  Recording folder
#define RECORDINGFOLDER ("/home/pi/bhRecordings/")


// Format string description for this board type
//
string FileBoardDescription(int boardId)
{
	switch (boardId)
	{
	case 0:
		return "OpenBCI_GUI$BoardCytonSerial";
	case 2:
		return "OpenBCI_GUI$BoardCytonSerialDaisy";
	default:
		return "Unknown?";
	}
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
			Logging.AddLog("OpenBCIFileRecorder", "OpenFile", format("Failed to create directory %s", RECORDINGFOLDER), LogLevelError);
			return false;
		}
	}
	else 
	{
		//  some other directory error
		Logging.AddLog("OpenBCIFileRecorder", "OpenFile", "Directory error opening recording file.", LogLevelError);
		return false;
	}
	
	return true;
}	
	
	
//  Open the  file
//
bool OpenBCIFileRecorder::OpenFile(string fileName)
{
	if (!CheckRecordingFolder())
		return false;
	
	timeval tv;
	gettimeofday(&tv, NULL);
	tm* logTime = localtime(&(tv.tv_sec));

	//  create file name from test name and start time
	ostringstream os;		
	os << fileName << "_" << setfill('0') << setw(2) << logTime->tm_hour << setw(2) << logTime->tm_min  << setw(2) << logTime->tm_sec << ".txt";	
	RecordingFileName = os.str();	
	os.str("");
	os << RECORDINGFOLDER << RecordingFileName;	
	auto fullPath = os.str();
	
	//  open the file
	{
		LockMutex lockFile(RecordingFileMutex);
			
		//  open log file
		RecordingFile.open(os.str());
		if (RecordingFile.is_open())
		{
			Logging.AddLog("OpenBCIFileRecorder", "OpenFile", format("Opened recording file %s.", fullPath.c_str()), LogLevelInfo);
			return true;
		}
		else
		{
			Logging.AddLog("OpenBCIFileRecorder", "OpenFile", format("Failed to open recording file %s.", fullPath.c_str()), LogLevelError);
			return false;
		}
	}
}


//  Add data to the queue
//
void OpenBCIFileRecorder::AddData(BFSample* data)
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
void OpenBCIFileRecorder::RunFunction()
{
	while (ThreadRunning)
	{		
		Sleep(10);
		WriteDataToFile();
	}
}


//  Write all of the available data in the queue to the file
//
void OpenBCIFileRecorder::WriteDataToFile()
{
	//  empty the queue and put the samples to send into a list
	list<BFSample*> samples;
	{
		LockMutex lockQueue(QueueMutex);
		
		while (SamplesQueue.size() > 0)
		{
			samples.push_back(SamplesQueue.front());
			SamplesQueue.pop();
		}
	}
	
	//  write the data to the file the data
	for(auto nextSample = samples.begin() ; nextSample != samples.end() ; ++nextSample)
	{
		if (!WroteHeader)
		{
			WriteHeader(*nextSample);
		}
		
		WriteSample(*nextSample);
		
		delete(*nextSample);
	}
}


//  Write the header to the file
//
void OpenBCIFileRecorder::WriteHeader(BFSample* firstSample)
{
	{
		LockMutex lockFile(RecordingFileMutex);
		
		//  metadata header
		RecordingFile << "%OpenBCI Raw EEG Data" << endl;
		RecordingFile << "%Number of channels = " << firstSample->GetNumberOfExgChannels() << endl;
		RecordingFile << "%Sample Rate = " << SampleRate << " Hz" << endl;
		RecordingFile << "%Board = " << FileBoardDescription(BoardId) << endl;
		RecordingFile << "%Logger = brainHat" << endl;
		//  data header
		RecordingFile << "Sample Index";
		
		for (int i = 0; i < firstSample->GetNumberOfExgChannels(); i++)
			RecordingFile << ", EXG Channel " << i;
	
		for (int i = 0; i < firstSample->GetNumberOfAccelChannels(); i++)
			RecordingFile << ", Accel Channel " << i;
	
		for (int i = 0; i < firstSample->GetNumberOfOtherChannels(); i++)
			RecordingFile << ", Other";
	
		for (int i = 0; i < firstSample->GetNumberOfAnalogChannels(); i++)
			RecordingFile << ", Analog Channel " << i;
	
		RecordingFile << ", Timestamp, Timestamp (Formatted)" << endl; 
	
		RecordingFile <<  fixed << showpoint;
	
		WroteHeader = true;
	}
}


//  Write a sample to the file
//
void OpenBCIFileRecorder::WriteSample(BFSample* sample)
{
	{
		LockMutex lockFile(RecordingFileMutex);
		
		//  sample index
		RecordingFile << setprecision(1);
		RecordingFile << sample->SampleIndex;
		
		//  exg channels
		RecordingFile << setprecision(6);
		for (int i = 0; i < sample->GetNumberOfExgChannels(); i++)
			RecordingFile << "," << sample->GetExg(i);
		
		//  accel channels
		RecordingFile << setprecision(6);
		for (int i = 0; i < sample->GetNumberOfAccelChannels(); i++)
			RecordingFile << "," << sample->GetAccel(i);
		
		//  other channels
		RecordingFile << setprecision(1);
		for(int i = 0 ; i < sample->GetNumberOfOtherChannels() ; i++)
			RecordingFile << "," << sample->GetOther(i);
		
		//  analog channels
		RecordingFile << setprecision(1);
		for (int i = 0; i < sample->GetNumberOfAnalogChannels(); i++)
			RecordingFile << "," << sample->GetAnalog(i);
	
		//  analog channels
		RecordingFile << setprecision(6);
		RecordingFile << "," << sample->TimeStamp;
		
		//  local time from time stamp
		double seconds;
		double microseconds = modf(sample->TimeStamp, &seconds);
		time_t timeSeconds = (int)seconds;
		tm* logTime = localtime(&timeSeconds);
		RecordingFile << "," << setw(4) << logTime->tm_year + 1900 << "-" << setfill('0') << setw(2) << logTime->tm_mon + 1 << "-" << logTime->tm_mday << " " << logTime->tm_hour << ":" <<  logTime->tm_min << ":" <<  logTime->tm_sec <<  "." << setw(4) << (int)(microseconds * 10000);
		
		RecordingFile << endl;
	}
}