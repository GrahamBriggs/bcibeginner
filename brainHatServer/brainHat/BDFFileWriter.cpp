#include <list>
#include <vector>
#include <unistd.h>
#include <iostream>
#include <EDFfile.h>
#include <board_shim.h>
#include "brainHat.h"
#include "BDFFileWriter.h"
#include "StringExtensions.h"
#include "TimeExtensions.h"
#include "BFSample.h"
#include <iomanip>
#include <sstream>
#include <dirent.h>
#include "FileExtensions.h"


using namespace std;





//  Constructor
//
BDFFileWriter::BDFFileWriter()
{
	
}


//  Destructor
//
BDFFileWriter::~BDFFileWriter()
{
	
}



//  Cancel thread, close the file
//
void BDFFileWriter::CloseFile()
{
	{
		LockMutex lockFile(RecordingFileMutex);
		edfCloseFile(FileHandle);
	}
	Logging.AddLog("BDFFileWriter", "Cancel", format("Closed recording file %s.", RecordingFileName.c_str()), LogLevelInfo);
}



	
	
//  Open the  file
//
bool BDFFileWriter::OpenFile(string fileName)
{
	if (!CheckRecordingFolder())
		return false;
		
	SetFilePath(fileName, "bdf");

}




string GetEquipmentName(int boardId)
{
	switch (boardId)
	{
	case 0:
		return "Cyton";
	case 2:
		return "Cyton+Daisy";
	default:
		return "";
	}
}


string GetSampleNameShort(int boardId)
{
	switch (boardId)
	{
	case 0:
		return "CY08";
	case 2:
		return "CY16";
	case 1:
		return "GAN4";
	default:
		return "";
	}
}




//  Write all of the available data in the queue to the file
//
void BDFFileWriter::WriteDataToFile()
{
	while (SamplesQueue.size() >= SampleRate)
	{
		//  get the next chunk
		vector<BFSample*> samples;
		{
			LockMutex lockQueue(QueueMutex);
		
			while (samples.size() < SampleRate)
			{
				samples.push_back(SamplesQueue.front());
				SamplesQueue.pop();
			}
		}
				
		WriteHeader(samples.front());
		WriteChunk(samples);
		
	}
}


//  Write the header to the file
//
void BDFFileWriter::WriteHeader(BFSample* firstSample)
{
	if (WroteHeader)
		return;
	
	{
		LockMutex lockFile(RecordingFileMutex);
			
		FileHandle = edfOpenFileWriteOnly(RecordingFileFullPath.c_str(), 3, firstSample->SampleSize());
		
		if (FileHandle < 0)
		{
			//  TODO - log error and bail out
			return;
		}
		
		int signalCount = 0;

		//  Signal Properties
		//
		//  sample index
		edfSetSamplesInDataRecord(FileHandle, signalCount, SampleRate);
		edfSetPhysicalMaximum(FileHandle, signalCount, 255);
		edfSetPhysicalMinimum(FileHandle, signalCount, 0);
		edfSetDigitalMaximum(FileHandle, signalCount, 8388607);
		edfSetDigitalMinimum(FileHandle, signalCount, -8388608);
		edfSetLabel(FileHandle, signalCount, "SampleIndex");
		edfSetPrefilter(FileHandle, signalCount, "");
		edfSetTransducer(FileHandle, signalCount, "");
		edfSetPhysicalDimension(FileHandle, signalCount, "counter");
		signalCount++;
		//
		//  exg channels
		for(int i = 0 ; i < firstSample->GetNumberOfExgChannels() ; i++)
		{
			edfSetSamplesInDataRecord(FileHandle, signalCount, SampleRate);
			edfSetPhysicalMaximum(FileHandle, signalCount, 187500.000);
			edfSetPhysicalMinimum(FileHandle, signalCount, -187500.000);
			edfSetDigitalMaximum(FileHandle, signalCount, 8388607);
			edfSetDigitalMinimum(FileHandle, signalCount, -8388608);
			edfSetLabel(FileHandle, signalCount, format("EXG%d",i).c_str());
			edfSetPrefilter(FileHandle, signalCount, "");
			edfSetTransducer(FileHandle, signalCount, "");
			edfSetPhysicalDimension(FileHandle, signalCount, "uV");
			signalCount++;
		}
		NumberOfExgChannels = firstSample->GetNumberOfExgChannels();
		//
		//  acel channels
		for(int i = 0 ; i < firstSample->GetNumberOfAccelChannels() ; i++)
		{
			edfSetSamplesInDataRecord(FileHandle, signalCount, SampleRate);
			edfSetPhysicalMaximum(FileHandle, signalCount, 1.0);
			edfSetPhysicalMinimum(FileHandle, signalCount, -1.0);
			edfSetDigitalMaximum(FileHandle, signalCount, 8388607);
			edfSetDigitalMinimum(FileHandle, signalCount, -8388608);
			edfSetLabel(FileHandle, signalCount, format("Acel%d",i).c_str());
			edfSetPrefilter(FileHandle, signalCount, "");
			edfSetTransducer(FileHandle, signalCount, "");
			edfSetPhysicalDimension(FileHandle, signalCount, "unit");
			signalCount++;
		}
		NumberOfAcelChannels = firstSample->GetNumberOfAccelChannels();
		//
		//  other channels
		for(int i = 0 ; i < firstSample->GetNumberOfOtherChannels() ; i++)
		{
			edfSetSamplesInDataRecord(FileHandle, signalCount, SampleRate);
			edfSetPhysicalMaximum(FileHandle, signalCount, 9999.0);
			edfSetPhysicalMinimum(FileHandle, signalCount, -9999.0);
			edfSetDigitalMaximum(FileHandle, signalCount, 8388607);
			edfSetDigitalMinimum(FileHandle, signalCount, -8388608);
			edfSetLabel(FileHandle, signalCount, format("Other%d",i).c_str());
			edfSetPrefilter(FileHandle, signalCount, "");
			edfSetTransducer(FileHandle, signalCount, "");
			edfSetPhysicalDimension(FileHandle, signalCount, "other");
			signalCount++;
		}
		NumberOfOtherChannels = firstSample->GetNumberOfOtherChannels();
		//
		//  analog channels
		for(int i = 0 ; i < firstSample->GetNumberOfAnalogChannels() ; i++)
		{
			edfSetSamplesInDataRecord(FileHandle, signalCount, SampleRate);
			edfSetPhysicalMaximum(FileHandle, signalCount, 9999.0);
			edfSetPhysicalMinimum(FileHandle, signalCount, -9999.0);
			edfSetDigitalMaximum(FileHandle, signalCount, 8388607);
			edfSetDigitalMinimum(FileHandle, signalCount, -8388608);
			edfSetLabel(FileHandle, signalCount, format("Analog%d",i).c_str());
			edfSetPrefilter(FileHandle, signalCount, "");
			edfSetTransducer(FileHandle, signalCount, "");
			edfSetPhysicalDimension(FileHandle, signalCount, "analog");
			signalCount++;
		}
		NumberOfAnalogChannels = firstSample->GetNumberOfAnalogChannels();
		//
		//  timestamp
		edfSetSamplesInDataRecord(FileHandle, signalCount, SampleRate);
		edfSetPhysicalMaximum(FileHandle, signalCount, 999999.0);
		edfSetPhysicalMinimum(FileHandle, signalCount, 0);
		edfSetDigitalMaximum(FileHandle, signalCount, 8388607);
		edfSetDigitalMinimum(FileHandle, signalCount, -8388608);
		edfSetLabel(FileHandle, signalCount, "TestTime");
		edfSetPrefilter(FileHandle, signalCount, "");
		edfSetTransducer(FileHandle, signalCount, "");
		edfSetPhysicalDimension(FileHandle, signalCount, "seconds");
		FirstTimeStamp = firstSample->TimeStamp;

		uint32_t time_date_stamp = (uint32_t)firstSample->TimeStamp;
		time_t temp = time_date_stamp;
		tm* t = std::localtime(&temp);

		//  File Header Properties
		//
		edfSetStartDatetime(FileHandle, t->tm_year+1900, t->tm_mon+1, t->tm_mday, t->tm_hour, t->tm_min, t->tm_sec);
		edfSetPatientName(FileHandle, "");
		edfSetPatientCode(FileHandle, "");
		edfSetPatientYChromosome(FileHandle, 1);
		
		edfSetPatientAdditional(FileHandle, "");
		edfSetAdminCode(FileHandle, "");
		edfSetTechnician(FileHandle, "");
		edfSetEquipment(FileHandle, GetEquipmentName(BoardId).c_str());
		edfSetRecordingAdditional(FileHandle, GetSampleNameShort(BoardId).c_str());

		WroteHeader = true;
	}
}


//  Write a sample to the file
//
void BDFFileWriter::WriteChunk(vector<BFSample*> chunk)
{
	{
		LockMutex lockFile(RecordingFileMutex);
		
		double* row = new double[SampleRate];
		
		//  sample index
		for(int j = 0 ; j < SampleRate ; j++)
			row[j] = chunk[j]->SampleIndex;
		auto result = edfWritePhysicalSamples(FileHandle, row);
		if (result < 0)
		{
			Logging.AddLog("BDFFileWriter", "WriteChunk", format("Error writing chunk %d", result), LogLevelError);
		}

		//  EXG channels
		for(int i = 0 ; i < NumberOfExgChannels ; i++)
		{
			for (int j = 0; j < SampleRate; j++)
				row[j] = chunk[j]->GetExg(i);
			result = edfWritePhysicalSamples(FileHandle, row);
			if (result < 0)
			{
				Logging.AddLog("BDFFileWriter", "WriteChunk", format("Error writing chunk %d", result), LogLevelError);
			}
		}

		//  Acel channels
		for(int i = 0 ; i < NumberOfAcelChannels ; i++)
		{
			for (int j = 0; j < SampleRate; j++)
				row[j] = chunk[j]->GetAccel(i);
			result = edfWritePhysicalSamples(FileHandle, row);
			if (result < 0)
			{
				Logging.AddLog("BDFFileWriter", "WriteChunk", format("Error writing chunk %d", result), LogLevelError);
			}
		}

		//  Other channels
		for(int i = 0 ; i < NumberOfOtherChannels ; i++)
		{
			for (int j = 0; j < SampleRate; j++)
				row[j] = chunk[j]->GetOther(i);
			result = edfWritePhysicalSamples(FileHandle, row);
			if (result < 0)
			{
				Logging.AddLog("BDFFileWriter", "WriteChunk", format("Error writing chunk %d", result), LogLevelError);
			}
		}

		//  Analog channels
		for(int i = 0 ; i < NumberOfAnalogChannels ; i++)
		{
			for (int j = 0; j < SampleRate; j++)
				row[j] = chunk[j]->GetAnalog(i);
			result = edfWritePhysicalSamples(FileHandle, row);
			if (result < 0)
			{
				Logging.AddLog("BDFFileWriter", "WriteChunk", format("Error writing chunk %d", result), LogLevelError);
			}
		}

		//  Time stamp
		for(int j = 0 ; j < SampleRate ; j++)
				row[j] = chunk[j]->TimeStamp - FirstTimeStamp;
		result = edfWritePhysicalSamples(FileHandle, row);
		if (result < 0)
		{
			Logging.AddLog("BDFFileWriter", "WriteChunk", format("Error writing chunk %d", result), LogLevelError);
		}
		
		delete[] row;
		
		for (auto it = chunk.begin(); it != chunk.end(); ++it)
		{
			delete *it;
		}	
		
		Logging.AddLog("BDFFileWriter", "WriteChunk", "Wrote chunk", LogLevelTrace);
	}
}