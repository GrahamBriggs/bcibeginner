#pragma once
#include <chrono>

//  Pin Control Modes
//
typedef enum PinControlMode
{
	Undefined,
	Binary,
	Blink,
} PinControlMode;
	
	
//  Pin Controller
//  Wrap up a single GPIO pin to control on/off and blinking behavior
//
class PinController
{
public:
	PinController(int pinNumber);
	
	//  Start flashing, 
	//  switch on and off for number of repetitions with delay between each sequence
	void StartFlash(int onMs, int offMs, int reps, int delayMs);
	
	//  Switch on or off
	void Switch(bool on);
	
	//  Trigger check of flash state, and change of state if necessary
	void Update();
	
protected:
	
	PinControlMode Mode;
	int PinNumber;
	int BlinkTimeOnMs;
	int BlinkTimeOffMs;
	int BlinkReps;
	int BlinkTimeDelayMs;
	int BlinkTimeWaiting;
	
	std::chrono::steady_clock::time_point SetTime;
};