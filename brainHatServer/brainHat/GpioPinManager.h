#pragma once
#include "Thread.h"
#include <vector>
#include <string>

enum LightStates
{
	LightsOff,
	LightsOn,
	LightsFlash,
	LightsSequence,
};


//  Class to manage GPIO pins on the Pi running brainHat
//  the brainHat bread board prototypes have lights on them to display status and otherwise flash and look cool
//  by default all the pins used by this program are set to number 0, which means that nothign will happen on the GPIO
//  there is mapping to known host names that will set specific pins for different prototypes
//
class GpioManager : public Thread
{
public:
	GpioManager();
	virtual ~GpioManager();
	
	void StartThreadForHost();
	
	virtual void Cancel();
	
	virtual void RunFunction();
	
	LightStates Mode; 	
	
	void AllOff();
	void AllOn();
	
	void PowerToBoard(bool enable);
	
protected:
	
	std::string HostName;
	
	void SetupGpio();
	
	std::vector<int> PinsInUse;
	
	void RunLightsFlash();
	bool RunningLightsFlash() { return Mode == LightsFlash && ThreadRunning;}
	
	void RunLightsSequence();
	bool RunningLightsSequence() { return Mode == LightsSequence && ThreadRunning;}
	
	std::vector<int> LightStringLeft;
	std::vector<int> LightStringRight;
	std::vector<int> BlinkLightsLeft;
	std::vector<int> BlinkLightsRight;
};