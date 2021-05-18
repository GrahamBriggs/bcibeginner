#pragma once
#include <list>
#include <tuple>

typedef unsigned long uint32;

// Get a list of IP v4 adapter name / network address
std::list<std::tuple<std::string, std::string>> GetNetworkIp4Addresses();

//  Get a list of IP v6 adapter name / network address
std::list<std::tuple<std::string, std::string>> GetNetworkIp6Addresses();

//  Get the host name of this machine
std::string GetHostName();

// Convert a sock address to uint 32
uint32 SockAddrToUint32(struct sockaddr * a);

// Convert a numeric IP address into its string representation
void Inet_NtoA(uint32 addr, char * ipbuf);
