#pragma once
#include <vector>
#include "Thread.h"
#include "BFSample.h"
#include "TimeExtensions.h"
#include "BrainHatFileWriter.h"

//  File recording class
//
class BDFFileWriter : public BrainHatFileWriter
{
public:
	BDFFileWriter(RecordingStateChangedCallbackFn fn);
	virtual ~BDFFileWriter();
	
	
protected:
		
	int FileHandle;
	
	int NumberOfExgChannels;
	int NumberOfAcelChannels;
	int NumberOfOtherChannels;
	int NumberOfAnalogChannels;
	double FirstTimeStamp;
	
	virtual bool OpenFile(std::string fileName, bool tryUsb);
	virtual void CloseFile();
	
	virtual void WriteDataToFile();
	void WriteHeader(BFSample* firstSample);
	void WriteChunk(std::vector<BFSample*> chunk);
	


	
}
;