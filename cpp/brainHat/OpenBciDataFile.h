#pragma once
#include <list>
#include <string>

#include "OpenBciData.h"



class OpenBciDataFile
{
public:

	OpenBciDataFile();
	virtual ~OpenBciDataFile();
	
	double FileStartTime() { return StartTime;}
	double FileEndTime() { return EndTime;}
	double FileDuration() { return EndTime - StartTime; }
	
	OpenBciData* GetNextRecord();
	
	
	bool LoadFile(std::string fileName);
	
protected:
	
	double StartTime;
	double EndTime;
	
	std::list<OpenBciData*> DataRecords;
	std::list<OpenBciData*>::iterator DataIterator;
	
};