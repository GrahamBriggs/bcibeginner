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

class LightsThread : public Thread
{
public:
	LightsThread();
	virtual ~LightsThread();
	
	void StartThreadForHost(std::string hostName);
	
	virtual void Cancel();
	
	virtual void RunFunction();
	
	LightStates Mode; 	
	
	void AllOff();
	void AllOn();
	
	void PowerToBoard(bool enable);
	
protected:
	
	void SetupGpio(std::string hostName);
	
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