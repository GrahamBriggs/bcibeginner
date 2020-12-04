#pragma once
#include <list>
#include <tuple>

// Get a list of IP v4 adapter name / network address
std::list<std::tuple<std::string, std::string>> GetNetworkIp4Addresses();

//  Get a list of IP v6 adapter name / network address
std::list<std::tuple<std::string, std::string>> GetNetworkIp6Addresses();

//  Get the host name of this machine
std::string GetHostName();
