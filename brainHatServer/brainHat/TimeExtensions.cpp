#include "TimeExtensions.h"

using namespace std;
using namespace chrono;


void ChronoTimer::Start()
{
	Running = true;
	StartTime = steady_clock::now();
}

void  ChronoTimer::Stop()
{
	Running = false;
	StopTime = steady_clock::now();
}


void  ChronoTimer::Reset()
{
	Running = true;
	StartTime = steady_clock::now();
}
	
double  ChronoTimer::ElapsedSeconds()
{
	return (double)ElapsedMilliseconds() / 1000.0;
}


int ChronoTimer::ElapsedMilliseconds()
{
	if (Running)
		return duration_cast<milliseconds>(steady_clock::now() - StartTime).count();
	else
		return duration_cast<milliseconds>(StopTime - StartTime).count(); 
}

