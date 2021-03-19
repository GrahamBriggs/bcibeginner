#include <string>
#include <sstream>

#include "Parser.h"

using namespace std;


/////////////////////////////////////////////////////////////////////////////
//  Parser
//  parse string with delimiters


//  Constructor
//
Parser::Parser(string buffer, string delimiters)
{
	//  set the string and delimiters
	OriginalBuffer = buffer;
	RemainingBuffer = OriginalBuffer;
	Delimiters = delimiters;

	
}


//  Destructor
//
Parser::~Parser()
{
}


//  SetBuffer
//
void Parser::SetBuffer(string buffer)
{
	OriginalBuffer = buffer;
	RemainingBuffer = OriginalBuffer;
}


//  SetBuffer
//
void Parser::SetBuffer(string buffer, string delimiters)
{
	Delimiters = delimiters;
	SetBuffer(buffer);
}


//  GetNextString
//
string Parser::GetNextString()
{
	if ( RemainingBuffer == "" )
		return "";

	string returnString = "";
	size_t index = RemainingBuffer.find_first_of(Delimiters, 0);
	
	//Skip delimiters at beginning.
	string::size_type lastPos = RemainingBuffer.find_first_not_of(Delimiters, 0);
	// Find first "non-delimiter".
	string::size_type pos     = RemainingBuffer.find_first_of(Delimiters, lastPos);

	if (string::npos != pos && string::npos != lastPos)
		returnString = RemainingBuffer.substr(lastPos, pos - lastPos);
	else
	{
		returnString = RemainingBuffer;
		RemainingBuffer = "";
		return returnString;
	}

	RemainingBuffer = RemainingBuffer.substr(pos+1);
	return returnString;
}


//  GetNextInt
//
int Parser::GetNextInt()
{
	int number;
	stringstream converter;
	converter << GetNextString();
	converter >> number;

	if (converter.fail())
	{
		return -1;
	}
	
	return number;
}



double Parser::GetNextDouble()
{
	double number;
	stringstream converter;
	converter << GetNextString();
	converter >> number;

	if (converter.fail())
	{
		return -1;
	}
	
	return number;
}


//  GetRemainingBuffer
//
string Parser::GetRemainingBuffer()
{
	return RemainingBuffer;
}




