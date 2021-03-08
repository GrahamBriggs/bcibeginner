#pragma once
#include <iomanip>
#include <sstream>


inline std::string FormatJSonPropertyDouble(std::string name, double value, int precision)
{
	std::ostringstream outStream;
 	outStream << std::fixed; 
	outStream << std::setprecision(precision);
 
	//Add double to stream
	outStream << "\"" << name << "\":" << value;
 
	// Get string from output string stream
	return outStream.str();
}

inline std::string FormatJSonPropertyInt(std::string name, int value)
{
	std::ostringstream outStream;
	
	//Add double to stream
	outStream << "\"" << name << "\":" << value;
 
	// Get string from output string stream
	return outStream.str();
}