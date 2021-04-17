#include <iostream>
#include "SerialPort.h"
#include <unistd.h>


using namespace std;

int fd;
bool streamRunning;
int indexCounter = 0;

void SimulateDeviceReadCommand();

int main(int argc, char *argv[])
{
	streamRunning = false;
	
	fd = serialOpen("/dev/serial0", 921600);
	if (fd < 0)
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
			indexCounter++;
			
			serialPutchar(fd, 0xA0);
		
			serialPutchar(fd, indexCounter);
			serialPrintf(fd, "BCDEFGHIJKLMNOPQRSTUVWXYZABCDE");
			if (indexCounter == 256)
				indexCounter = 0;
		}
		
		usleep(10000);
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
		streamRunning = false;
		
		if (readBuff[0] == 0x90 && readBuff[1] == 0x09)
		{
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
		// I made this last one up, if the one above is start stream, this one might be stop stream?
		else if(readBuff[0] == 0x90 && readBuff[1] == 0x00)
		{
			cout << "Respond to 0x90 0x00" << endl;
			serialPutchar(fd, 0xE0);
			serialPutchar(fd, 0x00);
			streamRunning = false;
		}
	}
	else
	{
		serialFlush(fd);
	}
}