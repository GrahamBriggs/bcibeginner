#include <list>
#include <iostream>
#include <iomanip>
#include <sstream>
#include <ctime>    
#include <sys/time.h>
#include <math.h>
#include "json.hpp"

#include "Logger.h"
#include "TimeExtensions.h"
#include "StringExtensions.h"
#include "NetworkAddresses.h"
#include "NetworkExtensions.h"

using namespace std;
using json = nlohmann::json;

//  LoggerLog class
//




LoggerLog::LoggerLog(LogEvent log)
{
	double seconds;
	double milliseconds = modf((double)log.LogUnixTimeMilliseconds / 1000, &seconds) * 1000;
	Time.tv_sec = (int)seconds;
	Time.tv_usec = (int)(milliseconds * 1000);
	Level = log.Level;
	
	Sender = log.Sender;
	Function = log.Function;
	Data = log.Data;
	HostName = "";

}



LoggerLog::LoggerLog(std::string sender, std::string function, std::string data, LogLevel level)
{
	gettimeofday(&Time, NULL);
	Level = level;
	Thread = 0;	
	Sender = sender;
	Function = function;
	Data = data;
	HostName = "";

}
	


LoggerLog::~LoggerLog()
{
	
}

string LoggerLog::SerializeAsJson()
{
	json j;
	
	j["Time"] = ToUnixTimeMilliseconds(Time);
	j["Thread"] = Thread;
	j["Level"] = Level;
	
	j["Sender"] = Sender;
	j["Function"] = Function;
	j["Data"] = Data;
	j["HostName"] = HostName;

	return j.dump();
}



//  Logger class
//

//  Constructor
//
Logger::Logger()
{
	DisplayOutputEnabled = true;
	
	LogLastLevelDisplayed = LogLevelAll;
	LogDisplayLevel = LogLevelTrace;
	RemoteLoggingEnabled = true;
	
	Notified = false;
}


//  Destructor
//
Logger::~Logger()
{
	if (ThreadRunning)
	{
		Cancel();
	}
}


//  Add a log to the queue
//
void Logger::AddLog(LogEvent log)
{
	{
		LockMutex lockQueue(QueueMutex);
		CommandQueue.push(new LoggerLog(log));
	}
	
	Notify();
}


//  Add a log string to the queue
//
void Logger::AddLog(string sender, string function, string data, LogLevel level)
{	
	LogEvent event;	
	event.LogUnixTimeMilliseconds = GetUnixTimeMilliseconds();
	event.Sender = sender.c_str();
	event.Function = function.c_str();
	event.Data = data.c_str();
	event.Level = level;
	
	AddLog(event);
}


const char* LogLevelName[LogLevelOff + 1] = { "All", "Verbose", "Trace", "Debug", "Info", "Warn", "Error", "Fatal", "User", "Off" };

//  Toggle the logs to display for the app
//
void Logger::ToggleAppLogLevel(LogLevel level)
{
	LogDisplayLevel = level;
	AddLog("Logger", "ToggleAppLogLevel", format("Level set to %s.", LogLevelName[LogDisplayLevel]), LogLevelInfo);
}



void Logger::ResetDisplay()
{
	Display.InitDimensions();
}


//  Pause the screen output
//
void Logger::PauseDisplayOutput()
{
	DisplayOutputEnabled = false;
	Display.SetColour(BLACK,WHITE);
}


//  Resume the screen output
//
void Logger::ResumeDisplayOutput()
{
	DisplayOutputEnabled = true;
	Display.InitDimensions();
}

bool Logger::IsDisplayOutputEnabled()
{
	return DisplayOutputEnabled;
}


// Write a line to the display now. Use this to print when display is paused
//
void Logger::WriteLineToDisplay(string log)
{
	Display.PrintLine(log);
}


// Write a line to the display now. Use this to print when display is paused
//
void Logger::WriteLineToDisplay(const char* log)
{
	WriteLineToDisplay(string(log));
}


//  Notify thread that there is something in the queue
//
void Logger::Notify()
{
	Notified = true;
	NotifyQueueCondition.notify_one();
}






void Logger::Start()
{
	HostName = GetHostName();
	
	PortNumber = OpenServerSocket(MULTICAST_LOGPORT, MULTICAST_GROUPADDRESS);
	
	if (PortNumber < 0)
	{
		return;
	}
	
	UdpMulticastServerThread::Start();
}


void Logger::Cancel()
{
	ThreadRunning = false;
	Notify();
	
	Thread::Cancel();
}


//  Thread Run Function
void Logger::RunFunction()
{
	Display.InitDimensions();
	LogLastLevelDisplayed = LogLevelOff;
	
	while (ThreadRunning)
	{
		if (CommandQueue.size() == 0  && ThreadRunning)
		{
			//  wait for messages
			std::unique_lock<std::mutex> lockNotify(NotifyMutex);

			//  avoid spurious wakeups
			while(!Notified)
				NotifyQueueCondition.wait(lockNotify);

			//  check to see if we were woken up because of shutdown
			if(!ThreadRunning)
				return;
		}
		
		if (!DisplayOutputEnabled)
		{
			Sleep(250);
			continue;
		}
		

		//  empty the queue and put the events to send into a list
		list<LoggerLog*> commands;
		{
			LockMutex lockQueue(QueueMutex);
		
			while (CommandQueue.size() > 0)
			{
				commands.push_back(CommandQueue.front());
				CommandQueue.pop();
			}
		}
	
		//  process everything in the queue
		list<LoggerLog*>::iterator nextItem;
		for (nextItem = commands.begin(); nextItem != commands.end(); ++nextItem)
		{
			LoggerLog* log = (*nextItem);
			if (log->Level >= LogDisplayLevel)
			{
				if (LogLastLevelDisplayed != log->Level)
				{
					LogLastLevelDisplayed = log->Level;
					Display.SetColour(FgColorForLog(LogLastLevelDisplayed), BgColorForLog(LogLastLevelDisplayed));
				}
				
				tm* logTime = localtime(&(log->Time.tv_sec));

				ostringstream os;		
				os <<  setfill('0') << setw(2) << logTime->tm_hour << ":" << setw(2) << logTime->tm_min << ":" << setw(2) << logTime->tm_sec <<  "." << std::setw(3) << log->Time.tv_usec / 1000;
				os << setfill(' ') << "   "  << left << setw(7) << LogLevelString(log->Level) << " " << left << setw(25) << log->Sender << "  " << setw(25) << log->Function << "  " <<  log->Data;			

				Display.PrintLine(os.str());
				
				if (RemoteLoggingEnabled)
				{
					log->HostName = HostName;
					auto logText = log->SerializeAsJson().substr(0,480);
					WriteMulticastString(format("log?hostname=%s&log=%s\n", HostName.c_str(), logText.c_str()));
				}
				
				delete(*nextItem);
			}
		}
		
		Notified = false;
	}
}




//  Get string for log level
//
string Logger::LogLevelString(LogLevel level)
{
	switch (level)
	{
	case LogLevelVerbose:
		return "VERBOSE";
	case 	LogLevelTrace:
		return "TRACE";
	case LogLevelDebug:
		return "DEBUG";
	case 	LogLevelInfo:
		return "INFO";
	case 	LogLevelWarn: 
		return "WARN";
	case 	LogLevelError:
		return "ERROR";
	case 	LogLevelFatal:
		return "FATAL";
	default:
		return "Unknown";
	}
}


//  Get colour for log text
//
int Logger::FgColorForLog(LogLevel level)
{
	switch (level)
	{
	case LogLevelTrace:
		return CYAN;
		
	case LogLevelDebug:
		return GREEN;
		
	case LogLevelInfo:
		return WHITE;
		
	case LogLevelWarn:
		return RED;
		
	case LogLevelError:
		return WHITE;
		
	case LogLevelFatal:
		return YELLOW;
		 
	case LogLevelUser:
		return BLUE;
		
	default:
		return WHITE;
	}
}


//  Get color for log background
int Logger::BgColorForLog(LogLevel level)
{
	switch (level)
	{
	case LogLevelDebug:
	case LogLevelTrace:
		return BLACK;
		
	case LogLevelInfo:
		return BLACK;
		
	case LogLevelWarn:
		return BLACK;
		
	case LogLevelError:
		return RED;
		
	case LogLevelFatal:
		return RED;
		
	case LogLevelUser:
		return WHITE;
		
	default:
		return BLACK;
	}
}