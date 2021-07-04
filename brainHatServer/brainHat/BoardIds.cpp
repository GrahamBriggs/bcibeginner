#include <board_shim.h>
#include "BoardIds.h"

using namespace std;

//   Number of EXG channels
//
int getNumberOfExgChannels(int boardId)
{
	int useBoardId = boardId;
	switch ((BrainhatBoardIds)boardId)  
	{
	case BrainhatBoardIds::MENTALIUM:
		useBoardId = 0;
		break;
	}
		
	switch ((BrainhatBoardIds)useBoardId)
	{
	default:
		return 0;
		
	case BrainhatBoardIds::CYTON_BOARD:
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
		auto res = BoardShim::get_exg_channels(useBoardId);
		return res.size();
	}
}


//  Number of accelerometer channels
//
int getNumberOfAccelChannels(int boardId)
{
	int useBoardId = boardId;
	switch ((BrainhatBoardIds)boardId)  
	{
	case BrainhatBoardIds::MENTALIUM:
		useBoardId = 0;
		break;
	}
		
	switch ((BrainhatBoardIds)useBoardId)
	{
	default:
		return 0;
	case BrainhatBoardIds::CYTON_BOARD:
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
		auto res = BoardShim::get_accel_channels(useBoardId);
		return res.size();
	}
}


//  Number of other channels
//
int getNumberOfOtherChannels(int boardId)
{
	int useBoardId = boardId;
	switch ((BrainhatBoardIds)boardId)  
	{
	case BrainhatBoardIds::MENTALIUM:
		useBoardId = 0;
		break;
	}
		
	switch ((BrainhatBoardIds)useBoardId)
	{
	default:
		return 0;
	case BrainhatBoardIds::CYTON_BOARD:
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
		auto res = BoardShim::get_other_channels(useBoardId);
		return res.size();
	}
}


// Number of analog channels
//
int getNumberOfAnalogChannels(int boardId)
{
	int useBoardId = boardId;
	switch ((BrainhatBoardIds)boardId)  
	{
	case BrainhatBoardIds::MENTALIUM:
		useBoardId = 0;
		break;
	}
		
	switch ((BrainhatBoardIds)useBoardId)
	{
	default:
		return 0;
	case BrainhatBoardIds::CYTON_BOARD:
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
		auto res = BoardShim::get_analog_channels(useBoardId);
		return res.size();
	}
}


int getNumberOfRows(int boardId)
{
	int useBoardId = boardId;
	switch ((BrainhatBoardIds)boardId)  
	{
	case BrainhatBoardIds::MENTALIUM:
		useBoardId = 0;
		break;
	}
		
	switch ((BrainhatBoardIds)useBoardId)
	{
	default:
		return 0;
	case BrainhatBoardIds::CYTON_BOARD:
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
		return BoardShim::get_num_rows(useBoardId);
	}
}


int getSamplingRate(int boardId)
{
	int useBoardId = boardId;
	switch ((BrainhatBoardIds)boardId)  
	{
	case BrainhatBoardIds::MENTALIUM:
		useBoardId = 0;
		break;
	}
		
	switch ((BrainhatBoardIds)useBoardId)
	{
	default:
		return 0;
	case BrainhatBoardIds::CYTON_BOARD:
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
		return BoardShim::get_sampling_rate(useBoardId);
	}
}


//  Is this board in the cyton family of boards
//
bool IsCytonFamily(int boardId)
{
	switch ((BrainhatBoardIds)boardId)
	{
	case BrainhatBoardIds::CYTON_BOARD:
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
	case BrainhatBoardIds::MENTALIUM:
		return true;
	default:
		return false;
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
	case BrainhatBoardIds::MENTALIUM:
		return "MENTALIUM8";
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
	case BrainhatBoardIds::MENTALIUM:
		return "MT08";
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
	case BrainhatBoardIds::MENTALIUM:
		return "MENTALIUM";
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
	case BrainhatBoardIds::MENTALIUM:
		return "Nelson";
	case BrainhatBoardIds::CYTON_BOARD:
	case BrainhatBoardIds::CYTON_DAISY_BOARD:
	case BrainhatBoardIds::GANGLION_BOARD:
		return "OpenBCI";
		
	default:
		return "Unknown";
	}
}
