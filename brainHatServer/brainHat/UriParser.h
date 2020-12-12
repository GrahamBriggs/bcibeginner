#pragma once
#include <string>
#include <map>
#include "Parser.h"

class UriArgParser
{
public:
	inline UriArgParser(std::string uri)
	{
		Parser argParser(uri, "&");
		auto nextArg = argParser.GetNextString();
		while (nextArg.length() > 0)
		{
			Parser args(nextArg,"=");
			Args[args.GetNextString()] = args.GetNextString();
			nextArg = argParser.GetNextString();
		}
	}
	
	inline std::string GetValue(std::string key)
	{
		if (Args.find(key) != Args.end())
			return Args[key];
		else
			return "";
	}
	
	
	std::map<std::string, std::string> Args;
	
};