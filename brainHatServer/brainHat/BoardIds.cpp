#include <board_shim.h>
#include "BoardIds.h"



int getNumberOfExgChannels(int boardId)
{
	switch ((BrainhatBoardIds)boardId)
	{
	case BrainhatBoardIds::UNDEFINED:
		return 0;
	case BrainhatBoardIds::CONTEC_KT88:
		return 16;
	default:
		int numChannels;
		BoardShim::get_exg_channels(boardId, &numChannels);
		return numChannels;
	}
}

int getNumberOfAccelChannels(int boardId)
{
	switch ((BrainhatBoardIds)boardId)
	{
	case BrainhatBoardIds::UNDEFINED:
		return 0;
	case BrainhatBoardIds::CONTEC_KT88:
		return 0;
	default:
		int accelChannels;
		BoardShim::get_accel_channels(boardId, &accelChannels);
		return accelChannels;
	}
}

int getNumberOfOtherChannels(int boardId)
{
	switch ((BrainhatBoardIds)boardId)
	{
	case BrainhatBoardIds::UNDEFINED:
		return 0;
	case BrainhatBoardIds::CONTEC_KT88:
		return 2;
	default:
		int otherChannels;
		BoardShim::get_other_channels(boardId, &otherChannels);
		return otherChannels;
	}
}

int getNumberOfAnalogChannels(int boardId)
{
	switch ((BrainhatBoardIds)boardId)
	{
	case BrainhatBoardIds::UNDEFINED:
		return 0;
	case BrainhatBoardIds::CONTEC_KT88:
		return 0;
	default:
		int analogChannels;
		BoardShim::get_analog_channels(boardId, &analogChannels);
		return analogChannels;
	}
}
