#pragma once
#include <string>
#include <stdarg.h>



//	std::string format()
//  replaces missing string printf   (this is safe and convenient but not exactly efficient )
//  can be used like:  std::string mystr = format("%s %d %10.5f", "omg", 1, 10.5);
//  see http://stackoverflow.com/users/642882/piti-ongmongkolkul
inline std::string format(const char* fmt, ...) {
	int size = 512;
	char* buffer = 0;
	buffer = new char[size];
	va_list vl;
	va_start(vl, fmt);
	int nsize = vsnprintf(buffer, size, fmt, vl);
	if (size <= nsize) {
		 //fail delete buffer and try again
		delete[] buffer;
		buffer = 0;
		buffer = new char[nsize + 1];  //+1 for /0
		nsize = vsnprintf(buffer, size, fmt, vl);
	}
	std::string ret(buffer);
	va_end(vl);
	delete[] buffer;
	return ret;
}