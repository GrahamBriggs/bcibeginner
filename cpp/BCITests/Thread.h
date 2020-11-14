#ifndef _THREAD_H
#define _THREAD_H

#include <thread>
#include <mutex>


//  sleep in the current thread for millis milliseconds
void Sleep(long millis);

//  Thread
//  a simple framework for using std::thread 
//  to have a thread, derive your class from Thread
//  and then define  YourDerivedThreadClass::RunFunction()
//  if you want to do specific shutdown when the thread is canceled, also override Cancel()
//


class Thread
{
public:

	Thread();
	virtual ~Thread();

	//  Returns the state of the thread running.
	bool IsRunning() { return ThreadRunning; }

	//  Returns true when the Run() function has exited.
	bool IsStopped() { return ThreadStopped; }

	//  Set the stoped flag when exiting the Run() function.
	void SetIsStopped() { ThreadStopped = true; }
	
	//  Start running the thread.
	//  If the thread is already running, it will be shut down before restarting.
	virtual void Start();

	//  Cancel the running thread.
	//  will join the running therad to ensure complete stop before returning
	virtual void Cancel();

	//  The RunFunction
	//  override this funciton in your derived class, and put the actions in here that this thread should do on each pass
	virtual void RunFunction() = 0;

protected:
	
	//  pointer to the std::thread object
	std::thread* TheThread;

	//  running flags
	bool ThreadRunning;
	bool ThreadStopped;
};




/////////////////////////////////////
//  Mutex lock / unlock helper class
//
//  This class will lock the mutex in the constructor, and unlock in the destructor
//  to use, wrap scope around an instance of this class and your mutex will unlock for you
// 
typedef std::lock_guard<std::mutex> LockMutex;


#endif // _THREAD_H