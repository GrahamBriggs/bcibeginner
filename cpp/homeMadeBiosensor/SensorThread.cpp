#include <stdlib.h>
#include <iostream>
#include <unistd.h>
#include <sstream>
#include <iomanip>
#include <sys/time.h>
#include <math.h>
#include <chrono>
#include <wiringPi.h>
#include <ads1115.h>


#include "SensorThread.h"



using namespace std;
using namespace chrono;


#define USLEEP_MILI (1000)
#define USLEEP_SEC (1000000)

#define ADC0_PINBASE (100)
#define ADC1_PINBASE (200)



//  Constructor
//
SensorThread::SensorThread()
{	
	Logging = false;
	RecordsLogged = 0;	
}


//  Destructor
//
SensorThread::~SensorThread()
{
	Cancel();
}


void SensorThread::Start()
{
	wiringPiSetup();
	ads1115Setup(ADC0_PINBASE, 0x48);
	ads1115Setup(ADC1_PINBASE, 0x49);
	
	//  set to fine resolution +-0.256V range = Gain 16
	digitalWrite(ADC0_PINBASE, 5);
	digitalWrite(ADC1_PINBASE, 5);
	
	// set sample rate to 475 samples/sec
	digitalWrite(ADC0_PINBASE + 1, 6);
	digitalWrite(ADC1_PINBASE + 1, 6);
	
	Thread::Start();
}


//  Thread Cancel
//  
void SensorThread::Cancel()
{
	Thread::Cancel();
}



//  Start Logging
//  begin logging board data to the a new file with this testname
void SensorThread::StartLogging(string testName)
{
	StopLogging();
	
	{
		LockMutex lockFile(LogFileMutex);
			
		timeval tv;
		gettimeofday(&tv, NULL);
		tm* logTime = localtime(&(tv.tv_sec));

		//  create file name from test name and start time
		ostringstream os;		
		os << "/home/pi/Source/BCI/DataLogs/" <<testName << "_" << setfill('0') << setw(2) << logTime->tm_hour <<  logTime->tm_min  << setw(2) << logTime->tm_sec << ".txt";	
		
		//  open log file
		LogFile.open(os.str());
		
		cout << "Starting log file " << os.str() << endl;
		
		//  header
		LogFile << "%OpenBCI Raw EEG Data" << endl;
		LogFile << "%Number of channels = 8" << endl;
		LogFile << "%Sample Rate = 250 Hz" << endl;
		LogFile << "%Board = OpenBCI_GUI$BoardCytonSerial" << endl;
		LogFile << "%Logger = OTTOMH HomeMadeLogger" << endl;
		LogFile << "Sample Index, EXG Channel 0, EXG Channel 1, EXG Channel 2, EXG Channel 3, EXG Channel 4, EXG Channel 5, EXG Channel 6, EXG Channel 7, Accel Channel 0, Accel Channel 1, Accel Channel 2, Other, Other, Other, Other, Other, Other, Other, Analog Channel 0, Analog Channel 1, Analog Channel 2, Timestamp, Timestamp (Formatted)" << endl;
	
		LogFile <<  fixed << showpoint;
		
		StartTime = steady_clock::now();
		LastLoggedTime = StartTime;
		
		RecordsLogged = 0;
		
		Logging = true;
	}
}


//  Stop Logging
//  end logging and close open log file
void SensorThread::StopLogging()
{
	{
		LockMutex lockFile(LogFileMutex);
		
		Logging = false;
	
		if (LogFile.is_open())
			LogFile.close();
	}
}

int sampleIndex = 0;

double GetVoltageMicrovolts(int pinBase, int channel)
{
	auto value = analogRead(pinBase+channel);
	
	return  ((double)value*(.256 / 32768.0))*1000000.0;
}


//  Thread Run Function
//
void SensorThread::RunFunction()
{
	
	int res = 0;
	int num_rows = 0;
	int data_count = 0;
	
	while (ThreadRunning)
	{
		
		if (Logging)
		{
			double data[23] = { 0.0 };
			
			// Sample Index, EXG Channel 0, EXG Channel 1, EXG Channel 2, EXG Channel 3, EXG Channel 4, EXG Channel 5, EXG Channel 6, EXG Channel 7, Accel Channel 0, Accel Channel 1, Accel Channel 2, Other, Other, Other, Other, Other, Other, Other, Analog Channel 0, Analog Channel 1, Analog Channel 2, Timestamp, Timestamp (Formatted)" << endl;
	
			//  Sample index
			data[0] = sampleIndex;
			
			// voltage readings
			data[1] = GetVoltageMicrovolts(ADC0_PINBASE, 0);
			data[2] = GetVoltageMicrovolts(ADC0_PINBASE, 1);
			data[3] = GetVoltageMicrovolts(ADC0_PINBASE, 2);
			data[4] = GetVoltageMicrovolts(ADC0_PINBASE, 3);
			data[5] = GetVoltageMicrovolts(ADC1_PINBASE, 0);
			data[6] = GetVoltageMicrovolts(ADC1_PINBASE, 1);
			data[7] = GetVoltageMicrovolts(ADC1_PINBASE, 2);
			data[8] = GetVoltageMicrovolts(ADC1_PINBASE, 3);
			
			//  Accel channels
			data[9] = 0.0;
			data[10] = 0.0;
			data[11] = 0.0;
			
			//  Other channels
			data[12] = 0.0;
			data[13] = 0.0;
			data[14] = 0.0;
			data[15] = 0.0;
			data[16] = 0.0;
			data[17] = 0.0;
			data[18] = 0.0;
			
			//Analog Channels
			data[19] = 0.0;
			data[20] = 0.0;
			data[21] = 0.0;
			
			//  Timestamps
			data[22] = chrono::duration_cast< milliseconds >(system_clock::now().time_since_epoch()).count() / 1000.0;
						
			SaveDataToFile(data);
			
			sampleIndex++;
			if (sampleIndex > 255)
				sampleIndex = 0;
			
			//  update the display once a second
			if(duration_cast<milliseconds>(steady_clock::now() - LastLoggedTime).count() > 1000)
			{
				cout << "Time Elapsed: " << chrono::duration_cast<seconds>(steady_clock::now() - StartTime).count() << "s. Records logged: " << RecordsLogged << endl;
				LastLoggedTime = steady_clock::now();
			}
		}
		

		usleep(10);	
		
	}
}


//  Save Data To File
//  saves a data buffer of data points to the file
//  note data buffer is rows of channels with columns of epochs
void SensorThread::SaveDataToFile(double* data_buf)
{
	{
		LockMutex lockFile(LogFileMutex);
				
		for (int i = 0; i < 23; i++)
		{
			if (i == 0)
			{
				LogFile << setprecision(1);
			}
			if (i == 1)
			{
				LogFile << setprecision(6);
			}	
			else if (i == 9)
			{
				LogFile << setprecision(1);
			}
			else if (i == 22)
			{
				LogFile << setprecision(6);
			}
				
			LogFile << data_buf[i] << ",";
		}

		//  local time from time stamp
		double seconds;
		double microseconds = modf(data_buf[22], &seconds);
		time_t timeSeconds = (int)seconds;
		tm* logTime = localtime(&timeSeconds);
		LogFile << setw(4) << logTime->tm_year + 1900 << "-" << setfill('0') << setw(2) << logTime->tm_mon + 1 << "-" << logTime->tm_mday << " " << logTime->tm_hour << ":" <<  logTime->tm_min << ":" <<  logTime->tm_sec <<  "." << setw(4) << (int)(microseconds * 10000);
		
		LogFile << endl;
	}
		
	RecordsLogged += 1;
}
