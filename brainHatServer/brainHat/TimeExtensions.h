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
