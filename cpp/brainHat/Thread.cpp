#include "Thread.h"

using namespace std;



/////////////////////////////////////////////////////////////////////////////
//  Sleep
//  sleep in milliseconds
//
void Sleep(long millis)
{
	std::this_thread::sleep_for(std::chrono::milliseconds(millis));
}


/////////////////////////////////////////////////////////////////////////////
//  Thread
//  base class for simple thread wrapper
//

//  Constructor
//  
Thread::Thread() 
{
	TheThread = 0;
	
	ThreadStopped = false;
	ThreadRunning = false;
}


//  Destructor
//  
Thread::~Thread()
{
	//  stop running thread before destruction
	if ( ThreadRunning )
	{
		//  ASSERT - this is bad, you should stop before destruction
		Cancel();
	}

	//  clean up the memory
	if ( TheThread != 0 )
		delete TheThread;
}



//  Run
//  Call the derived class fun function
//
void Run(Thread& thread)
{
	thread.RunFunction();

	thread.SetIsStopped();
}


//  Start
//  
void Thread::Start()
{
	//  stop thread if it is running
	if ( TheThread != 0 )
	{
		//  ASSERT - this is bad, you should stop running thread before starting new one
		Cancel();
	}

	//  initialize thread run variables
	ThreadRunning = true;
	ThreadStopped = false;

	//  start the thread
	TheThread = new std::thread(Run,std::ref(*this));

	return;
}



//  Cancel
//  Stop the thread
//  this function will wait until the thread has exited before returning
//
void Thread::Cancel()
{
	ThreadRunning = false;

	if ( TheThread )
	{
		//  a proper cancel thread function should have an interrupt here, to kill the thread if it is sleeping
		//  unfortunately, std::thread can not be interrupted (I believe?)
		//  use the BoostThread class if you want an interruptable thread

		//  wait for the thread to stop
		while ( ! ThreadStopped )
		{
			Sleep(50);
#ifdef DEBUG
			printf("Thread::Cancel - Waiting for thread to stop... \n");
#endif
		}

		//  join to the thread to ensure that thread shuts down before exit
		TheThread->join();

		//  clean up our mess
		delete TheThread;
		TheThread = 0;
	}

	return;
}
