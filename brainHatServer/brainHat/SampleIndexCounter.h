#pragma once
#include "BoardIds.h"

class SampleIndexCounter
{
public:
	SampleIndexCounter(int boardId)
	{
		BoardId = (BrainhatBoardIds)boardId ;
		SampleCounter = 0;
	}
	
	double GetNextSampleIndex()
	{
		SampleCounter++;
		switch (BoardId)
		{
		case BrainhatBoardIds::CYTON_DAISY_BOARD:
			SampleCounter++;
		default:
			break;
		}
		
		if (SampleCounter > 255)
			SampleCounter = 0;
		
		return (double)SampleCounter;
	}
		
protected:
	
	int SampleCounter;
	BrainhatBoardIds BoardId;
};
