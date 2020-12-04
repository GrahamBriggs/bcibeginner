#include <stdio.h>      
#include <sys/types.h>
#include <ifaddrs.h>
#include <netinet/in.h> 
#include <string.h> 
#include <arpa/inet.h>
#include <unistd.h>

#include "NetworkExtensions.h"

using namespace std;


list<tuple<string, string>> GetNetworkIp4Addresses()
{
	list<tuple<string, string>> addressList;
	
	struct ifaddrs * ifAddrStruct = NULL;
	struct ifaddrs * ifa = NULL;
	void * tmpAddrPtr = NULL;

	getifaddrs(&ifAddrStruct);

	for (ifa = ifAddrStruct; ifa != NULL; ifa = ifa->ifa_next) {
		if (!ifa->ifa_addr) {
			continue;
		}
		if (ifa->ifa_addr->sa_family == AF_INET) {
			// check it is IP4
		   tmpAddrPtr = &((struct sockaddr_in *)ifa->ifa_addr)->sin_addr;
			char addressBuffer[INET_ADDRSTRLEN];
			inet_ntop(AF_INET, tmpAddrPtr, addressBuffer, INET_ADDRSTRLEN);
			addressList.push_back(make_tuple(string(ifa->ifa_name), string(addressBuffer)));
		}
	}
	if (ifAddrStruct != NULL) freeifaddrs(ifAddrStruct);
	
	return addressList;
}


list<tuple<string, string>> GetNetworkIp6Addresses()
{
	list<tuple<string, string>> addressList;
	
	struct ifaddrs * ifAddrStruct = NULL;
	struct ifaddrs * ifa = NULL;
	void * tmpAddrPtr = NULL;

	getifaddrs(&ifAddrStruct);

	for (ifa = ifAddrStruct; ifa != NULL; ifa = ifa->ifa_next) 
	{
		if (!ifa->ifa_addr) 
			continue;

		if (ifa->ifa_addr->sa_family == AF_INET6) 
		{
		   // is a valid IP6 Address
		   tmpAddrPtr = &((struct sockaddr_in6 *)ifa->ifa_addr)->sin6_addr;
			char addressBuffer[INET6_ADDRSTRLEN];
			inet_ntop(AF_INET6, tmpAddrPtr, addressBuffer, INET6_ADDRSTRLEN);
			addressList.push_back(make_tuple(string(ifa->ifa_name), string(addressBuffer)));
		} 
	}
	if (ifAddrStruct != NULL) freeifaddrs(ifAddrStruct);
	
	return addressList;
}


string GetHostName()
{
	char host[1024];
	gethostname(host, 1024);
	return string(host);
}