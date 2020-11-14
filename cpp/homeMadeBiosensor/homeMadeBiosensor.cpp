#include <iostream>
#include "SensorThread.h"

using namespace std;

#define ADS_PINBASE 100


SensorThread Sensor;
void DoLogging();


int main(int argc, char *argv[])
{
	Sensor.Start();
	
	cout << "============================================================" << endl;
	cout << "OTTOMY Home Made Board Data Logger " << endl;
	cout << "============================================================" << endl;
	cout << endl;
	cout << "Press 't' to start a test, q to quit." << endl;
	
	while (true)
	{
		int input = getchar();
		
		if (input == 'q')
		{
			Sensor.StopLogging();
			break;
		}
		else if (input == 't')
		{
			DoLogging();
		}
	}
	
}


void DoLogging()
{
	cout << "Enter name for this test." << endl;
			
	string testName;
	cin >> testName;
			
	cout << "Enter 's' to start a log file in this test." << endl;
			
	while (true)
	{
		int input = getchar();
		if (input == 'q')
		{
			cout << "Ended test " << testName << endl;
			cout << "Press 't' to start a test, q to quit." << endl;
					
			Sensor.StopLogging();
			break;
		}
		else if (input == 'e')
		{
			Sensor.StopLogging();
			cout << "Stopped log file. Press s to start new log file in test " << testName << endl;
		}
		else if (input == 's')
		{
			Sensor.StartLogging(testName);
			cout << "Press e to end log file." << endl;
		}
	}
}