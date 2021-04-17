#pragma once
#include <string>
#include <stdarg.h>
#include <algorithm>
#include <vector>
#include <sstream>


#define BYTE_TO_BINARY_PATTERN "%c%c%c%c%c%c%c%c"
#define BYTE_TO_BINARY(byte)  \
  (byte & 0x80 ? '1' : '0'), \
  (byte & 0x40 ? '1' : '0'), \
  (byte & 0x20 ? '1' : '0'), \
  (byte & 0x10 ? '1' : '0'), \
  (byte & 0x08 ? '1' : '0'), \
  (byte & 0x04 ? '1' : '0'), \
  (byte & 0x02 ? '1' : '0'), \
  (byte & 0x01 ? '1' : '0') 
	
	

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


inline void toUpper(std::string& value)
{
	std::transform(value.begin(), value.end(), value.begin(),::toupper);
}


inline void removeTrailingCharacters(std::string &str, const char charToRemove) {
	str.erase(str.find_last_not_of(charToRemove) + 1, std::string::npos);
}

inline void removeLeadingCharacters(std::string &str, const char charToRemove) {
	str.erase(0, std::min(str.find_first_not_of(charToRemove), str.size() - 1));
}

//  function is useful to parse an entire string into vector of substrings at once
inline void Tokenize(const std::string& str, std::vector<std::string>& tokens, const std::string& delimiters /*= " "*/)
{
    // Skip delimiters at beginning.
    std::string::size_type lastPos = str.find_first_not_of(delimiters, 0);
    // Find first "non-delimiter".
    std::string::size_type pos     = str.find_first_of(delimiters, lastPos);

	while (std::string::npos != pos || std::string::npos != lastPos)
    {
        // Found a token, add it to the vector.
        tokens.push_back(str.substr(lastPos, pos - lastPos));
        // Skip delimiters.  Note the "not_of"
        lastPos = str.find_first_not_of(delimiters, pos);
        // Find next "non-delimiter"
        pos = str.find_first_of(delimiters, lastPos);
    }
}


inline int ParseInt(std::string value)
{
	int number;
	std::stringstream converter;
	converter << value;
	converter >> number;

	if (converter.fail())
	{
		return -1;
	}
	
	return number;
}