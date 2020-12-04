#include <algorithm>

#include "CMD.h"

using namespace std;



/////////////////////////////////////////////////////////////////////////////
//  CMD
//  class to execute system("yourCommand")
//

//  Constructor
//
CMD::CMD()
{
	Command = "";
	
}


//  Constructor
//
CMD::CMD(std::string command)
{
	Command = command;
	
}


//  Destructor
//
CMD::~CMD()
{
	Command = "";
}


//  Execute
//  Calls system and parse
//
bool CMD::Execute()
{
	CommandResponse.clear();

	if ( System() && Parse() )
		return true;
	
	return false;
}



//  System
//  calls popen(yourCommand)
//  puts response into a std::string list (with newlines stripped off)
//
//  TODO - refactor usage with fork or select ? 
//  see:  http://stackoverflow.com/questions/478898/how-to-execute-a-command-and-get-output-of-command-within-c
//
bool CMD::System()
{
	//  issue the ifconfig command
	string commandLineString = GetCommandLineString();
	FILE *cmdResponse = popen(commandLineString.c_str(), "r");

	if ( cmdResponse == 0 )
		return false;

	//  parse the return into string list
	char buffer[1024];
	char *nextLine = fgets(buffer, sizeof(buffer), cmdResponse);
	while ( nextLine )
	{
		//  string object from char*
		string nextResponse = nextLine;

		//  remove newlines and carriage returns
		nextResponse.erase(remove(nextResponse.begin(), nextResponse.end(), '\n'), nextResponse.end());
		nextResponse.erase(remove(nextResponse.begin(), nextResponse.end(), '\r'), nextResponse.end());

		//  add to the end of the response list
		CommandResponse.push_back(nextResponse);

		//  look for the next line of the command
		nextLine = fgets(buffer, sizeof(buffer), cmdResponse);
	}

	pclose(cmdResponse);
	return true;
}



// Compare
// compares the latest response to your command with the previous response to your command
// this is useful to know if the state that your command represents has changed
//
bool CMD::Compare()
{
	//  response same size
	if ( CommandResponse.size() != LastCommandResponse.size() )
	{
		LastCommandResponse.clear();
		for ( int i = CommandResponse.size()-1; i >= 0 ; i-- )
			LastCommandResponse.push_back( CommandResponse[i]);

		return false;
	}

	//  all strings in response compare
	for ( int i = 0; i < CommandResponse.size(); i++ )
	{
		if ( CommandResponse[i].compare(LastCommandResponse[i]) != 0 )
		{
			LastCommandResponse.clear();
			for ( int j = 0; j < CommandResponse.size(); j++ )
				LastCommandResponse.insert(LastCommandResponse.end(), CommandResponse[j]);

			return false;
		}
	}

	return true;
}



// GetCommandLineString
//
std::string CMD::GetCommandLineString()
{
	return Command;
}


//  GetCommandResponseLine
//
std::string CMD::GetCommandResponseLine(int i)
{
	if ( i >= CommandResponse.size() )
		return "";

	return CommandResponse[i];
}


//  GetCommandResponse
//
std::string CMD::GetCommandResponse()
{
	string response;

	vector<string>::iterator nextString;
	for (nextString = CommandResponse.begin(); nextString != CommandResponse.end(); nextString++)
	{
		response += *nextString;

		if (next(nextString) != CommandResponse.end())
			response += "\n";
	}

	return response;
}