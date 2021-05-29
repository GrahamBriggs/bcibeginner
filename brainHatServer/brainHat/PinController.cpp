#include <wiringPi.h>
#include <chrono>

#include "PinController.h"


using namespace std;
using namespace chrono;

//  Constructor
//
PinController::PinController(int pinNumber)
{
	PinNumber = pinNumber;
	if (PinNumber > 0)
	{
		pinMode(PinNumber, OUTPUT);
		digitalWrite(PinNumber, LOW);
	}

}

//  Start Flashing
//
void PinController::StartFlash(int onMs, int offMs, int reps,  int delayMs)
{
	Mode = PinControlMode::Blink;
	BlinkTimeWaiting = 0;
	BlinkTimeOnMs = onMs;
	BlinkTimeOffMs = offMs;
	BlinkTimeDelayMs = delayMs;
	BlinkReps = reps;
	
	if (BlinkReps == 0)
		BlinkTimeDelayMs = BlinkTimeOffMs;
	
	digitalWrite(PinNumber, HIGH);
	SetTime = steady_clock::now();
	
}


//  Switch on or off
//
void PinController::Switch(bool on)
{
	if (PinNumber > 0)
	{
		Mode = PinControlMode::Binary;
		SetTime = steady_clock::now();
	
		digitalWrite(PinNumber, on ? HIGH : LOW);
	}
}



//  Update, will check blink state and change on the timer interval
//
int blinkCounter;
void PinController::Update()
{
	if (PinNumber > 0)
	{
		switch (Mode)
		{
		default:
			break;
		
		case PinControlMode::Blink:
			{
				auto timeNow = steady_clock::now();
				auto elapsed = duration_cast<milliseconds>(timeNow - SetTime).count();
				if (elapsed > BlinkTimeWaiting)
				{
					if (digitalRead(PinNumber) == 0)
					{
						digitalWrite(PinNumber, HIGH);
						BlinkTimeWaiting = BlinkTimeOnMs;
						blinkCounter++;
					}
					else
					{
						digitalWrite(PinNumber, LOW);
						if (blinkCounter >= BlinkReps)
						{
							BlinkTimeWaiting = BlinkTimeDelayMs;
							blinkCounter = 0;
						}
						else
						{
							BlinkTimeWaiting = BlinkTimeOffMs;
						}
					}
				
					SetTime = timeNow;
				}
			}
		
			break;
		}
	}
}