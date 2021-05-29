#pragma once
#include <queue>
#include <condition_variable>
#include <lsl_cpp.h>
#include <fstream>

#include "Thread.h"
#include "BFSample.h"
#include "TimeExtensions.h"
#include "BrainHatFileWriter.h"

//  File recording class
//
class OpenBCIFileWriter : public BrainHatFileWriter
{
public:
	OpenBCIFileWriter(RecordingStateChangedCallbackFn fn);
	virtual ~OpenBCIFileWriter();
	
	
protected:
		
	
	virtual bool OpenFile(std::string fileName, bool tryUsb);
	virtual void CloseFile();
	
	virtual void WriteDataToFile();
	virtual void WriteHeader(BFSample* firstSample);
	virtual void WriteSample(BFSample* sample);
	

	std::ofstream RecordingFile;
	

	
};

