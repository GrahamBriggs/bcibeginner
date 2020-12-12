#pragma once
#include <string>

#include <sys/stat.h>




inline bool MakePath(std::string path)
{
	bool bSuccess = false;
	int nRC =:: mkdir(path.c_str(), 0775);
	if (nRC == -1)
	{
		switch (errno)
		{
		case ENOENT:
			//parent didn't exist, try to create it
			if(MakePath(path.substr(0, path.find_last_of('/'))))
			    //Now, try to create again.
			    bSuccess = 0 == ::mkdir(path.c_str(), 0775);
			else
			    bSuccess = false;
			break;
		case EEXIST:
			//Done!
			bSuccess = true;
			break;
		default:
			bSuccess = false;
			break;
		}
	}
	else
		bSuccess = true;
	return bSuccess;
}