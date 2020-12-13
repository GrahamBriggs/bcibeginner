#pragma once
#include <string>
#include <map>
#include "Parser.h"

class UriArgParser
{
public:
	inline UriArgParser(std::string uri)
	{
		Parser requestParser(uri, "?");
		Request = requestParser.GetNextString();
		
		Parser argParser(requestParser.GetNextString(), "&");
		
		// cycle through all the args
		auto nextArg = argParser.GetNextString();
		while (nextArg.length() > 0)
		{
			Parser args(nextArg,"=");
			Args[args.GetNextString()] = args.GetNextString();
			nextArg = argParser.GetNextString();
		}
	}
	
	inline std::string GetRequest() { return Request;}
		
	inline std::string GetArg(std::string key)
	{
		if (Args.find(key) != Args.end())
			return Args[key];
		else
			return "";
	}
	
	
	
protected:
	std::string Request;
	std::map<std::string, std::string> Args;
	
};