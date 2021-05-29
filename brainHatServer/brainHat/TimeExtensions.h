#pragma once
#include <sys/time.h>
#include <chrono>



#define USLEEP_MILI (1000)
#define USLEEP_SEC (1000000)


inline long long ToUnixTimeMilliseconds(struct timeval& tv)
{
	gettimeofday(&tv, NULL);
	return ((unsigned long long)(tv.tv_sec) * 1000 + (unsigned long long)(tv.tv_usec) / 1000);
}


inline long long GetUnixTimeMilliseconds()
{
	struct timeval tv;
	gettimeofday(&tv, NULL);
	return ToUnixTimeMilliseconds(tv);
}


inline bool SetSystemTime(int year, int month, int day, int hour, int minute, int second, int microseconds)
{
	struct tm setTimeStruct;
	struct timeval setTimeValue;
	time_t new_time;
	int result;

	//Set Time struct
	setTimeStruct.tm_sec = second;        // Seconds
	setTimeStruct.tm_min = minute;        // Minutes
	setTimeStruct.tm_hour = hour + 1;     	 // Hours
	setTimeStruct.tm_mday = day;          // Day of Month
	setTimeStruct.tm_mon = month - 1;       // Month
	setTimeStruct.tm_year = year - 1900;  	 // Year


	new_time = mktime(&setTimeStruct);
	setTimeValue.tv_sec = new_time;
	setTimeValue.tv_usec = microseconds;

	return (settimeofday(&setTimeValue, NULL) >= 0);
}


inline bool SetSystemTime(long long unixTimeMilliseconds)
{
	time_t t_unix = unixTimeMilliseconds / 1000;
	int t_ms = unixTimeMilliseconds % 1000;
	
	struct timeval setTimeValue;
	setTimeValue.tv_sec = t_unix;
	setTimeValue.tv_usec = t_ms;

	return (settimeofday(&setTimeValue, NULL) >= 0);
}


class ChronoTimer
{
public:
	void Start();
	void Stop();
	void Reset();
	
	double ElapsedSeconds();
	int ElapsedMilliseconds();
	
protected:
	bool Running;
	std::chrono::steady_clock::time_point StartTime;
	std::chrono::steady_clock::time_point StopTime;
};
