#include <board_shim.h>
#include "BoardIds.h"

using namespace std;

//   Number of EXG channels
//
int getNumberOfExgChannels(int boardId)
{
	switch ((BrainhatBoardIds)boardId)
	{
	case BrainhatBoardIds::UNDEFINED:
		return 0;
	default:
		int numChannels;
		BoardShim::get_exg_channels(boardId, &numChannels);
		return numChannels;
	}
}


//  Number of accelerometer channels
//
int getNumberOfAccelChannels(int boardId)
{
	switch ((BrainhatBoardIds)boardId)
	{
	case BrainhatBoardIds::UNDEFINED:
		return 0;
	default:
		int accelChannels;
		BoardShim::get_accel_channels(boardId, &accelChannels);
		return accelChannels;
	}
}


//  Number of other channels
//
int getNumberOfOtherChannels(int boardId)
{
	switch ((BrainhatBoardIds)boardId)
	{
	case BrainhatBoardIds::UNDEFINED:
		return 0;
	default:
		int otherChannels;
		BoardShim::get_other_channels(boardId, &otherChannels);
		return otherChannels;
	}
}


// Number of analog channels
//
int getNumberOfAnalogChannels(int boardId)
{
	switch ((BrainhatBoardIds)boardId)
	{
	case BrainhatBoardIds::UNDEFINED:
		return 0;
	default:
		int analogChannels;
		BoardShim::get_analog_channels(boardId, &analogChannels);
		return analogChannels;
	}
}


	
//  Sample name
//  Used by LSL stream
string getSampleName(int boardId)
{
	switch ((BrainhatBoardIds)boardId)
	{
	case BrainhatBoardIds::CYTON_BOARD:
		return "Cyton8_BFSample";
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
		return "Cyton16_BFSample";
	case BrainhatBoardIds::GANGLION_BOARD:
		return "Ganglion_BFSample";
	default:
		return "BFSample";
	}
}
	

//  Sample name short form
//  Used by BDF file writer
string getSampleNameShort(int boardId)
{
	switch ((BrainhatBoardIds)boardId)
	{
	case BrainhatBoardIds::CYTON_BOARD:
		return "CY08";
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
		return "CY16";
	case BrainhatBoardIds::GANGLION_BOARD:
		return "GAN4";
	default:
		return "BF";
	}
}



//  Equipment manufacturer name
//
string getEquipmentName(int boardId)
{
	switch ((BrainhatBoardIds)boardId)
	{
	case BrainhatBoardIds::CYTON_BOARD:
		return "Cyton";
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
		return "Cyton+Daisy";
	case BrainhatBoardIds::GANGLION_BOARD:
		return "Ganglion";
	default:
		return "";
	}
}


//  Board manufacturer
//
string getManufacturerName(int boardId)
{
	switch ((BrainhatBoardIds)boardId)
	{
	case BrainhatBoardIds::CYTON_BOARD:
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
	case BrainhatBoardIds::GANGLION_BOARD:
		return "OpenBCI";
		
	default:
		return "Unknown";
	}
}
