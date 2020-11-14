#pragma once

#include "Thread.h"
#include "board_shim.h"
#include "TerminalDisplay.h"



class SensorThread : public Thread
{
public:
	SensorThread();
	virtual ~SensorThread();
	
	int Start(int board_id, struct BrainFlowInputParams params);
	
	virtual void Cancel();
	
	virtual void RunFunction();
	
	void PauseDisplay() {DisplayOn = true; }
	void ResumeDisplay() {DisplayOn = false;}
	void ToggleDisplayOn() {DisplayOn = !DisplayOn;}
	void RegisterConsoleInput();
	
protected:
	BoardShim* Board;
	int BoardId;
	
	void PrintData(double **data_buf, int num_channels, int num_data_points);
	
	TerminalDisplay Display;
	bool DisplayOn;
};