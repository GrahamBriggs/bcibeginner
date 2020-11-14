#pragma once
#include <sys/time.h>
#include <string>



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
