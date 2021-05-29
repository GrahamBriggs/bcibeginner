#include <wiringPi.h>
#include <list>

#include "GpioControl.h"
#include "SimpleTimer.h"

using namespace std;

list<PinController*> Pins;
PinController* PinConnectionStatus;
PinController* PinRecordingStatus;

bool Running = false;

// Timer task running to update PIN states
//
void TimerTask()
{
	while (Running)
	{
		delay(5);
	
		for (auto nextPin = Pins.begin(); nextPin != Pins.end(); ++nextPin)
		{
			(*nextPin)->Update();
		}
	}
	
	//  shut down
	for (auto nextPin = Pins.begin(); nextPin != Pins.end(); ++nextPin)
	{
		(*nextPin)->Switch(false);
	}
}


//  Start the controller
//  Initialize wiringPi and begin the timer task 
void StartGpioController(int pinConnection, int pinRecording)
{
	if (pinConnection > 0 || pinRecording > 0)
		wiringPiSetupPhys();
	
	PinConnectionStatus = new PinController(pinConnection);
	PinRecordingStatus = new PinController(pinRecording);
	
	if (pinConnection > 0 || pinRecording > 0)
	{
		Running = true;
		
		if (pinConnection > 0)
		{
			Pins.push_back(PinConnectionStatus);
		}
		
		if (pinRecording > 0)
		{
			Pins.push_back(PinRecordingStatus);
		}
	
		SimpleTimer startTimerTask(0, true, &TimerTask);
	}
}


//  Stop the controller
//
void StopGpioController()
{
	Running = false;
}


void ConnectionLightShowConnecting()
{
	PinConnectionStatus->StartFlash(20, 50, 2, 2000);
}

void ConnectionLightShowReady()
{
	PinConnectionStatus->StartFlash(1000, 1000, 0, 0);
}
	
void ConnectionLightShowConnected()
{
	PinConnectionStatus->Switch(true);
}

void ConnectionLightShowPaused()
{
	PinConnectionStatus->StartFlash(222, 111, 0, 0);
}

void RecordingLight(bool enable)
{
	PinRecordingStatus->Switch(enable);
}