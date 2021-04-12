#include <list>
#include "brainHat.h"
#include "OpenBCIFileWriter.h"
#include "StringExtensions.h"
#include "TimeExtensions.h"
#include "BFSample.h"
#include <iomanip>
#include "FileExtensions.h"

using namespace std;





//  Constructor
//
OpenBCIFileWriter::OpenBCIFileWriter()
{
	
}


//  Destructor
//
OpenBCIFileWriter::~OpenBCIFileWriter()
{
	
}



//  Cancel thread, close the file
//
void OpenBCIFileWriter::CloseFile()
{
	RecordingFile.close();
	Logging.AddLog("OpenBCIFileWriter", "CloseFile", format("Closed recording file %s.", RecordingFileName.c_str()), LogLevelInfo);
}



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


	
	
//  Open the  file
//
bool OpenBCIFileWriter::OpenFile(string fileName, bool tryUsb)
{
	string pathToRecFolder = "";
	if (!CheckRecordingFolder(fileName, tryUsb, pathToRecFolder))
		return false;
	
	SetFilePath(pathToRecFolder, fileName, "txt");
	
	//  open the file
	{
		LockMutex lockFile(RecordingFileMutex);
			
		//  open log file
		RecordingFile.open(RecordingFileFullPath);
		if (RecordingFile.is_open())
		{
			Logging.AddLog("OpenBCIFileWriter", "OpenFile", format("Opened recording file %s.", RecordingFileFullPath.c_str()), LogLevelInfo);
			return true;
		}
		else
		{
			Logging.AddLog("OpenBCIFileWriter", "OpenFile", format("Failed to open recording file %s.", RecordingFileFullPath.c_str()), LogLevelError);
			return false;
		}
	}
}





//  Write all of the available data in the queue to the file
//
void OpenBCIFileWriter::WriteDataToFile()
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
void OpenBCIFileWriter::WriteHeader(BFSample* firstSample)
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
void OpenBCIFileWriter::WriteSample(BFSample* sample)
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