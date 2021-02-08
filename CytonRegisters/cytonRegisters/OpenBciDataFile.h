#pragma once
#include <list>
#include <string>

#include "BFSample.h"



class OpenBciDataFile
{
public:

	OpenBciDataFile();
	virtual ~OpenBciDataFile();
	
	double FileStartTime() { return StartTime;}
	double FileEndTime() { return EndTime;}
	double FileDuration() { return EndTime - StartTime; }
	
	BFSample* GetNextRecord();
	
	
	bool LoadFile(std::string fileName);
	
protected:
	
	double StartTime;
	double EndTime;
	
	std::list<BFSample*> DataRecords;
	std::list<BFSample*>::iterator DataIterator;
	
};