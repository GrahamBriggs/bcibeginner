#include <stdlib.h>
#include <iostream>
#include <unistd.h>
#include <sstream>
#include <iomanip>

#include "OpenBciData.h"
#include "SensorThread.h"


using namespace std;

SensorThread::SensorThread()
{
	Board = NULL;
	DisplayOn = true;
}



SensorThread::~SensorThread()
{
	Cancel();
}



void SensorThread::Cancel()
{
	Thread::Cancel();
	
	if (Board != NULL && Board->is_prepared())
	{
		Board->stop_stream();
		Board->release_session();
		delete Board;
		Board = NULL;
	}
}


int SensorThread::Start(int board_id, struct BrainFlowInputParams params)
{
	int res = 0;
	
	Board = new BoardShim(board_id, params);
	
	try
	{
		Board->prepare_session();
		Board->start_stream();
		BoardId = board_id;
		
		// for STREAMING_BOARD you have to query information using board id for master board
		// because for STREAMING_BOARD data format is determined by master board!
		if(BoardId == (int)BoardIds::STREAMING_BOARD)
		{
			BoardId = std::stoi(params.other_info);
			BoardShim::log_message((int)LogLevels::LEVEL_INFO, "Use Board Id %d", board_id);
		}	
		
		usleep(5 * 1000000);
	}
	catch (const BrainFlowException &err)
	{
		BoardShim::log_message((int)LogLevels::LEVEL_ERROR, err.what());
		res = err.exit_code;
		if (Board->is_prepared())
		{
			Board->release_session();
		}
	}
	
	if (res == 0)
		Thread::Start();
	
	return res;
}
	


void SensorThread::RunFunction()
{
	double **data = NULL;
	int res = 0;
	int num_rows = 0;
	int data_count = 0;
	
	while (Board != NULL && ThreadRunning)
	{
		data = Board->get_board_data(&data_count);
		
		num_rows = BoardShim::get_num_rows(BoardId);

		if (DisplayOn)
		{
			PrintData(data, num_rows, data_count);
		}
		
		if (data != NULL)
		{
			for (int i = 0; i < num_rows; i++)
			{
				delete[] data[i];
			}
		}
		delete[] data;
			
		usleep(100000);
	}
}

int lastPrinted = 0;

void SensorThread::RegisterConsoleInput()
{
	lastPrinted += 1;
}

void SensorThread::PrintData(double **data_buf, int num_channels, int num_data_points)
{
	
	int num_points = (num_data_points < 5) ? num_data_points : 5;
	
	//  erase console lines to reset display
	for(int i = 0 ; i < lastPrinted ; i++)
	{
		fputs("\033[A\033[2K", stdout);
	}
	rewind(stdout);
	
	ostringstream os;		
	//os <<  setfill('0') << setw(2) << logTime->tm_hour << ":" << setw(2) << logTime->tm_min << ":" << setw(2) << logTime->tm_sec <<  "." << std::setw(3) << log->Time.tv_usec / 1000;
	//os << setfill(' ') << "   "  << left << setw(7) << LogLevelString(log->Level) << " " << left << setw(25) << log->Sender << "  " << setw(25) << log->Function << "  " <<  log->Data;			
	
	for (int i = 0; i < num_channels; i++)
	{
		if (i == 0)
			Display.SetColour(RED, BLACK);
		else if (i > 0 && i < 9)
			Display.SetColour(WHITE, CYAN);
		else if (i >= 9 && i < 12)
			Display.SetColour(WHITE, BLUE);
		else if (i >= 12)
			Display.SetColour(GREEN, BLACK);
		
		std::cout << "Channel " << setfill('0') << setw(2) << i << ": ";		
		
		for (int k =0; k < num_points; k++)
		{
			OpenBciData data(data_buf, k);
		}
		
		for (int j = 0; j < num_points; j++)
		{
			int width = 20;
			int precision = 3;
			if (i == 22)
			{
				precision = 5;
			}
			
			std::cout <<  fixed << showpoint << setprecision(precision) << setfill(' ') << setw(width) << data_buf[i][j] << ",";
		}
		std::cout << std::endl;
	}
	lastPrinted = num_channels;
	
}