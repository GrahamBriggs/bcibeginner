#pragma once

#include <string>
#include <condition_variable>
#include <queue>
#include "Thread.h"
#include "TerminalDisplay.h"

typedef enum
{
	LogLevelAll,
	LogLevelVerbose,
	LogLevelTrace,
	LogLevelDebug,
	LogLevelInfo,
	LogLevelWarn, 
	LogLevelError,
	LogLevelFatal,
	LogLevelUser,
	LogLevelOff,
} LogLevel ;


typedef struct 
{
	unsigned long long LogUnixTimeMilliseconds;
	LogLevel Level;
	int Thread;
	const char* Sender;
	const char* Function;
	const char* Data;
		
} LogEvent;

//  Logger Log
//  Container class for log info
class LoggerLog
{
public:
	
	LoggerLog(LogEvent log);
	LoggerLog(std::string sender, std::string function, std::string data, LogLevel level);
	
	virtual ~LoggerLog();
	
	std::string SerializeAsJson();
	
	timeval Time;
	LogLevel Level;
	int Thread;
	std::string Sender;
	std::string Function;
	std::string Data;
	std::string HostName;
	
};


//  Logger class
//  Takes care of writing logs and program output to the terminal
class Logger : public Thread
{
public:
	Logger();
	virtual ~Logger();
	
	void AddLog(LogEvent log);
	void AddLog(std::string sender, std::string function, std::string data, LogLevel level);
	
	void WriteLineToDisplay(std::string log);

	void WriteLineToDisplay(const char* log);
	
	void ResetDisplay();
	void PauseDisplayOutput();
	void ResumeDisplayOutput();
	bool IsDisplayOutputEnabled();
	
	void ToggleAppLogLevel(LogLevel level);
		
	virtual void Start();
	
	virtual void Cancel();
	
	virtual void RunFunction();
	
protected:
	
	std::string HostName;
	
	
	//  queue lock
	std::mutex QueueMutex;
	std::queue<LoggerLog*> CommandQueue;
	
	//  queue notification
	bool Notified;
	std::condition_variable NotifyQueueCondition;
	std::mutex NotifyMutex;

	void Notify();
	
	//  display settings
	TerminalDisplay Display;
	bool DisplayOutputEnabled;
	LogLevel LogDisplayLevel;
	LogLevel LogLastLevelDisplayed;
	LogLevel LogLevelWiringPi;
	
	std::string LogLevelString(LogLevel level);
	int FgColorForLog(LogLevel level);
	int BgColorForLog(LogLevel level);
};

