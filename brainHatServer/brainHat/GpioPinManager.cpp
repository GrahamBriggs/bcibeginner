#include <unistd.h>
#include "GpioPinManager.h"
#include "wiringPi.h"

using namespace std;

//  Set all pin numbers to zero
//  by default this will do nothing to the GPIO
//  this is overridden by host name mapping for certain devices below
//
int PinRightRising = 0;
int PinRightFalling = 0;
int PinRightBlink = 0;

int PinLeftRising = 0;
int PinLeftFalling = 0;
int PinLeftBlink = 0;

int PinLightStringA5 = 0;
int PinLightStringA4 = 0;
int PinLightStringA3 = 0;
int PinLightStringA2 = 0;
int PinLightStringA1 = 0;

int PinLightStringB5 = 0;
int PinLightStringB4 = 0;
int PinLightStringB3 = 0;
int PinLightStringB2 = 0;
int PinLightStringB1 = 0;

int PinHapticMotor = 0;

int PinPowerSwitch = 0;

//  Wrap wiringPi functions to do nothing with pin 0
void DigitalWrite(int pin, int value)
{
	if (pin != 0)
		digitalWrite(pin, value);
}
//
void AllPinsOff(vector<int> pins)
{
	for (auto it = pins.begin(); it != pins.end(); ++it)
	{
		if (*it != 0)
			DigitalWrite(*it, LOW);
	}
}
//
void AllPinsOn(vector<int> pins)
{
	for (auto it = pins.begin(); it != pins.end(); ++it)
	{
		if (*it != 0)
			DigitalWrite(*it, HIGH);
	}
}


// Setup GPIO
// this will set pin numbers for known host names where the prototypes have wired pins
//
void GpioManager::SetupGpio()
{
	if (HostName.compare("brainHelmet") == 0)
	{
		PinRightRising = 37;
		PinRightFalling = 38;
		PinRightBlink = 36;

		PinLeftRising = 0;
		PinLeftFalling = 0;
		PinLeftBlink = 0;

		PinLightStringA5 = 35;
		PinLightStringA4 = 32;
		PinLightStringA3 = 31;
		PinLightStringA2 = 29;
		PinLightStringA1 = 33;

		PinLightStringB5 = 23;
		PinLightStringB4 = 21;
		PinLightStringB3 = 24;
		PinLightStringB2 = 22;
		PinLightStringB1 = 26;

		PinHapticMotor = 12;

		PinPowerSwitch = 40;
	}
	else if (HostName.compare("brainHat") == 0)
	{
		PinLeftRising = 40;
		PinLeftFalling = 38;
		PinLeftBlink = 36;

		PinRightRising = 7;
		PinRightFalling = 11;
		PinRightBlink = 13;

		PinLightStringA5 = 29;
		PinLightStringA4 = 31;
		PinLightStringA3 = 32;
		PinLightStringA2 = 33;
		PinLightStringA1 = 35;

		PinLightStringB5 = 26;
		PinLightStringB4 = 23;
		PinLightStringB3 = 24;
		PinLightStringB2 = 21;
		PinLightStringB1 = 19;

		PinHapticMotor = 16;
		
		PinPowerSwitch = 0;
	}
	
	wiringPiSetupPhys();
	
	PinsInUse.push_back(PinLeftRising);
	PinsInUse.push_back(PinLeftFalling);
	PinsInUse.push_back(PinLeftBlink);
	//
	BlinkLightsLeft.push_back(PinLeftRising);
	BlinkLightsLeft.push_back(PinLeftFalling);
	BlinkLightsLeft.push_back(PinLeftBlink);
	BlinkLightsLeft.push_back(PinLeftFalling);
	BlinkLightsLeft.push_back(PinLeftRising);
	
	
	PinsInUse.push_back(PinRightRising);
	PinsInUse.push_back(PinRightFalling);
	PinsInUse.push_back(PinRightBlink);
	//
	BlinkLightsRight.push_back(PinRightRising);
	BlinkLightsRight.push_back(PinRightFalling);
	BlinkLightsRight.push_back(PinRightBlink);
	BlinkLightsRight.push_back(PinRightFalling);
	BlinkLightsRight.push_back(PinRightRising);

	PinsInUse.push_back(PinLightStringA1);
	PinsInUse.push_back(PinLightStringA2);
	PinsInUse.push_back(PinLightStringA3);
	PinsInUse.push_back(PinLightStringA4);
	PinsInUse.push_back(PinLightStringA5);
	LightStringLeft.push_back(PinLightStringA1);
	LightStringLeft.push_back(PinLightStringA2);
	LightStringLeft.push_back(PinLightStringA3);
	LightStringLeft.push_back(PinLightStringA4);
	LightStringLeft.push_back(PinLightStringA5);
            
	PinsInUse.push_back(PinLightStringB1);
	PinsInUse.push_back(PinLightStringB2);
	PinsInUse.push_back(PinLightStringB3);
	PinsInUse.push_back(PinLightStringB4);
	PinsInUse.push_back(PinLightStringB5);
	LightStringRight.push_back(PinLightStringB1);
	LightStringRight.push_back(PinLightStringB2);
	LightStringRight.push_back(PinLightStringB3);
	LightStringRight.push_back(PinLightStringB4);
	LightStringRight.push_back(PinLightStringB5);

	PinsInUse.push_back(PinHapticMotor);
	PinsInUse.push_back(PinPowerSwitch);
	
	for (auto it = PinsInUse.begin(); it != PinsInUse.end(); ++it)
	{
		if (*it != 0 )
			pinMode(*it, OUTPUT);
	}
	
	for (auto it = PinsInUse.begin(); it != PinsInUse.end(); ++it)
	{
		if (*it == PinPowerSwitch)
			continue;
		
		DigitalWrite(*it, HIGH);
	}
	DigitalWrite(PinHapticMotor, LOW);
}

GpioManager::GpioManager()
{
	
}

GpioManager::~GpioManager()
{
	
}


void GpioManager::StartThreadForHost()
{
	char host[1024];
	gethostname(host, 1024);
	
	HostName = string(host);
	
	//  only do something if this is a device with pins hooked up
	if(HostName.compare("brainHat") == 0 || HostName.compare("brainHelmet") == 0)
	{
		SetupGpio();
	
		Thread::Start();
	}
}

void GpioManager::Cancel()
{
	Thread::Cancel();
	AllOff();
}

void GpioManager::AllOn()
{
	AllPinsOn(PinsInUse);
}

void GpioManager::AllOff()
{
	AllPinsOff(PinsInUse);
}

void GpioManager::PowerToBoard(bool enable)
{
	DigitalWrite(PinPowerSwitch, enable ? HIGH : LOW);
}


void GpioManager::RunFunction()
{
	while (ThreadRunning)
	{
		switch (Mode)
		{
		case LightsOff:
			AllPinsOff(LightStringLeft);
			AllPinsOff(LightStringRight);
			AllPinsOff(BlinkLightsLeft);
			AllPinsOff(BlinkLightsRight);
			Sleep(250);
			break;
			
		case LightsOn:
			AllPinsOn(LightStringLeft);
			AllPinsOn(LightStringRight);
			AllPinsOn(BlinkLightsLeft);
			AllPinsOn(BlinkLightsRight);
			Sleep(250);
			break;
			
		case LightsFlash:
			RunLightsFlash();
			break;
			
		case LightsSequence:
			RunLightsSequence();
			break;
		}
	}
}



void GpioManager::RunLightsFlash()
{
	
	while (RunningLightsFlash())
	{
		Sleep(111);
		AllPinsOn(LightStringLeft);
		AllPinsOn(LightStringRight);
		AllPinsOn(BlinkLightsLeft);
		AllPinsOn(BlinkLightsRight);
		if (! RunningLightsFlash())
			return;
		Sleep(333);
		AllPinsOff(LightStringLeft);
		AllPinsOff(LightStringRight);
		AllPinsOff(BlinkLightsLeft);
		AllPinsOff(BlinkLightsRight);
	}
}



void GpioManager::RunLightsSequence()
{
	while (RunningLightsSequence())
	{
		for (int i = 0; i < LightStringLeft.size(); i++)
		{
			if (Mode != LightsSequence)
				return;
			Sleep(111);
			DigitalWrite(LightStringLeft[i], HIGH);
			DigitalWrite(LightStringRight[i], HIGH);
			DigitalWrite(BlinkLightsLeft[i], HIGH);
			DigitalWrite(BlinkLightsRight[i], HIGH);
			if (!RunningLightsSequence())
				return;
			Sleep(333);
			DigitalWrite(LightStringLeft[i], LOW);
			DigitalWrite(LightStringRight[i], LOW);
			DigitalWrite(BlinkLightsLeft[i], LOW);
			DigitalWrite(BlinkLightsRight[i], LOW);
		}
		Sleep(111);
		for (int i = LightStringLeft.size() - 1; i >= 0; i--)
		{
			if (!RunningLightsSequence())
				return;
			DigitalWrite(LightStringLeft[i], HIGH);
			DigitalWrite(LightStringRight[i], HIGH);
			DigitalWrite(BlinkLightsLeft[i], HIGH);
			DigitalWrite(BlinkLightsRight[i], HIGH);
			if (!RunningLightsSequence())
				return;
			Sleep(333);
			DigitalWrite(LightStringLeft[i], LOW);
			DigitalWrite(LightStringRight[i], LOW);	
			DigitalWrite(BlinkLightsLeft[i], LOW);
			DigitalWrite(BlinkLightsRight[i], LOW);
		}
	}
}