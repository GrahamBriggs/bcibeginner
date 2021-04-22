#include <iostream>
#include "SerialPort.h"
#include <unistd.h>
#include <vector>
#include <sstream>
#include <fstream>

using namespace std;

int fd;
bool streamRunning;
int indexCounter = 0;
ifstream dataFile;

void SimulateDeviceReadCommand();
void SimulateDeviceStream();

int main(int argc, char *argv[])
{
	streamRunning = false;
	
	fd = serialOpen("/dev/serial0", 921600);
	if (fd < 0)
	{
		return -1;
	}
	
	dataFile.open("ContecDataRaw.txt");

	if (! dataFile.is_open())
	{
		return -1;
	}
	
	
		
	while (true)
	{
		if (serialDataAvail(fd) >= 2)
		{
			SimulateDeviceReadCommand();
		}
			
		if (streamRunning)
		{
			SimulateDeviceStream();
		
//			indexCounter++;
//			
//			serialPutchar(fd, 0xA0);
//		
//			serialPutchar(fd, indexCounter);
//			serialPrintf(fd, "BCDEFGHIJKLMNOPQRSTUVWXYZABCDE");
//			if (indexCounter == 256)
//				indexCounter = 0;
		}
		
		usleep(7000);
	}
	
	
	return 0;
}



//  Simulate the device response 
//  (based on the com port tracing from device to controller software interaction
//
void SimulateDeviceReadCommand()
{
	char readBuff[2];
	
	if (read(fd, readBuff, 2) == 2)
	{
		if (readBuff[0] == 0x90 && readBuff[1] == 0x09)
		{
			streamRunning = false;
			
			cout << "Respond to 0x90 0x09" << endl;
			serialPutchar(fd, 0xE0);
			serialPutchar(fd, 0x09);
			serialPutchar(fd, 0xB0);
			serialPutchar(fd, 0x3F);
			for (int i = 0; i < 37; i++)
				serialPutchar(fd, 0x7F);
		}
		else if (readBuff[0] == 0x90 && readBuff[1] == 0x03)
		{
			cout << "Respond to 0x90 0x03" << endl;
			serialPutchar(fd, 0xE0);
			serialPutchar(fd, 0x03);
		}
		else if (readBuff[0] == 0x90 && readBuff[1] == 0x06)
		{
			cout << "Respond to 0x90 0x06" << endl;
			serialPutchar(fd, 0xE0);
			serialPutchar(fd, 0x06);
		}
		else if (readBuff[0] == 0x90 && readBuff[1] == 0x01)
		{
			cout << "Respond to 0x90 0x01" << endl;
			serialPutchar(fd, 0xE0);
			serialPutchar(fd, 0x01);
			streamRunning = true;
		}
		else if(readBuff[0] == 0x90 && readBuff[1] == 0x02)
		{
			cout << "Respond to 0x90 0x02" << endl;
			serialPutchar(fd, 0xE0);
			serialPutchar(fd, 0x02);
			streamRunning = false;
		}
	}
	else
	{
		serialFlush(fd);
	}
}


void SimulateDeviceStream()
{
	string readLine;

	if (!dataFile.eof())
	{
		getline(dataFile, readLine);
		if (readLine.size() != 0)
		{
			int number = 0;
			stringstream ss(readLine);         //convert my_string into string stream

			istringstream hex_chars_stream(readLine);
			unsigned int c;
			while (hex_chars_stream >> std::hex >> c)
			{
				serialPutchar(fd, c);	
			}	
		}
	}
	
	else
	{	
		dataFile.clear();
		dataFile.seekg(0);
		cout << "Reading from file" << endl;
	}
}