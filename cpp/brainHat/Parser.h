#ifndef _PARSER_H
#define _PARSER_H

#include <string>


//  Parser
//  a class to take a string and return the value of substring elements
//
class Parser
{
public:

	Parser(std::string buffer, std::string delimiters);
	virtual ~Parser();

	void SetBuffer(std::string buffer);
	void SetBuffer(std::string buffer, std::string delimiters);

	void SetDelimiter(std::string delimiter) { Delimiters = delimiter;	}

	//  returns the next item as a string
	std::string GetNextString();

	//  returns the next item as an int
	int GetNextInt();
	
	double GetNextDouble();

	//  returns the remaining buffer as a string
	std::string GetRemainingBuffer();
	

protected:

	std::string OriginalBuffer;
	std::string Delimiters;
	std::string RemainingBuffer;
};


#endif