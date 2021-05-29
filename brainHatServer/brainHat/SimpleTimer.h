#pragma once

#include <functional>
#include <future>


//  Simple Timer class
//  copied from here:  https://www.tutorialspoint.com/how-to-create-timer-using-cplusplus11 


class SimpleTimer {
public:
	template <class callable, class... arguments>
		SimpleTimer(int after, bool async, callable&& f, arguments&&... args) {
			std::function<typename std::result_of<callable(arguments...)>::type()> task(std::bind(std::forward<callable>(f), std::forward<arguments>(args)...));
			if (async) {
				std::thread([after, task]() {
					std::this_thread::sleep_for(std::chrono::milliseconds(after));
					task();
				}).detach();
			}
			else {
				std::this_thread::sleep_for(std::chrono::milliseconds(after));
				task();
			}
		}
};
