#include <stdlib.h>
#include <iostream>
#include <unistd.h>
#include <sstream>
#include <iomanip>
#include <sys/time.h>
#include <math.h>
#include <chrono>


#include "brainHat.h"
#include "ContecDataReader.h"
#include "StringExtensions.h"
#include "TimeExtensions.h"
#include "BFSampleGeneric.h"
#include "CytonBoardConfiguration.h"
#include "board_controller.h"
#include "SerialPort.h"

#define SENSOR_SLEEP (50)

using namespace std;
using namespace chrono;


//  Contec Data Reader, reads data from Contec device
//  Construct with callback functions:
//    -  ConnectionChanged will be called on first discovery of board parameters, then on connect / disconnect state
//    -  NewSample will be called when new data is read from the board
//
ContecDataReader::ContecDataReader(ConnectionChangedCallbackFn connectionChangedFn, NewSampleCallbackFn newSampleFn)
{
	Init();

	ConnectionChangedCallback = connectionChangedFn;
	NewSampleCallback = newSampleFn;
}


//  Destructor
//
ContecDataReader::~ContecDataReader()
{
	Cancel();
}


//  Initialize properties
//
void ContecDataReader::Init()
{
	BoardComPortFd = -1;
	BoardOn = true;
	IsConnected = false;
	StreamRunning = false;
	ConnectionChangedCallback  = NULL;
	ConnectionChangedDelegate = NULL;
	InvalidSampleCounter = 0;
	
	BoardDataSource::Init();
}


//  Describe the source of the data
//
string ContecDataReader::ReportSource()
{
	return format("Contec %d at %d Hz.", BoardId, SampleRate);
}


//  Board object is created and session is prepared
//
bool ContecDataReader::BoardReady()
{
	return BoardComPortFd >= 0;
}


//  Thread Start
//
int ContecDataReader::Start(int board_id, struct BrainFlowInputParams params, bool srb1On)
{

	BoardId = board_id;
	ComPort = params.serial_port;
	
	LastSampleIndex = -1;
	
	BoardDataSource::Start();
	
	return 0;
}


//  Thread Cancel
//  
void ContecDataReader::Cancel()
{
	Thread::Cancel();
	
	StopStreaming();
	
	ReleaseBoard();
}



//  Public function to get current SRB1 state for specified board
//
int ContecDataReader::GetSrb1(int board)
{
	return -1;
}




//  Public funciton to toggle streaming
bool ContecDataReader::RequestEnableStreaming(bool enable)
{
	if ( (enable && ! StreamRunning) || (!enable && StreamRunning))
		RequestToggleStreaming = true;
	
	return true;
}




//  Initialize Board
//  creates a new Brainflow board object and starts streaming data
int ContecDataReader::InitializeBoard()
{
	int res = 0;
	
	ReleaseBoard();
	RequestToggleStreaming = false;
		
	BoardComPortFd = serialOpen(ComPort.c_str(), 921600, 10);
	
	if (BoardComPortFd < 0)
		return -1;
	
	serialFlush(BoardComPortFd);
		
			
	bool newConnection = SampleRate < 0;
	SampleRate = 100;	
		
	if (!StartStreaming())
	{
		cout << "Failed to start streaming" << endl;
		ReleaseBoard();
		return -1;
	}
	
	InitializeDataReadCounters();
	
	ConnectionChanged(newConnection ? New : Connected, BoardId, SampleRate);
		
	IsConnected = true;
	Logging.AddLog("ContecDataReader", "InitializeBoard", format("Connected to board %d. Sample rate %d", BoardId, SampleRate), LogLevelInfo);
	
	return res;
}



// Initialize the data reading monitor counters
//
void ContecDataReader::InitializeDataReadCounters()
{
	LastSampleIndex = -1;
	LastTimeStampSync = -1;
	ReadTimer.Start();
	InspectDataStreamLogTimer.Start(); 
}


//  Release Board
//  stops the session and deletes the board if it is initialized
//
void ContecDataReader::ReleaseBoard()
{
	if (BoardComPortFd >= 0)
		close(BoardComPortFd);
	BoardComPortFd = -1;
	
	StreamRunning = false;
	IsConnected = false;
}


//  Keep reading from the serial port until read times out
bool ReadSerialPortResponse(int fd)
{
	int bytesRead = 0;
	bool response = false;
	auto readChar = serialGetchar(fd);
	while (readChar > -1 && bytesRead < 256)
	{
		bytesRead++;
		response = true;
		cout << readChar;
		readChar = serialGetchar(fd);
	}
		
	if(response)
		cout << endl;
	
	return response;
}


//  Start board streaming
//
bool ContecDataReader::StartStreaming()
{
	if (!StreamRunning)
	{
		cout << "Start streaming 0x90 0x09" << endl;
		serialPutchar(BoardComPortFd, 0x90);
		serialPutchar(BoardComPortFd, 0x09);
		
		//  get response
		ReadSerialPortResponse(BoardComPortFd);
//		if(!ReadSerialPortResponse(BoardComPortFd))
//			return false;
			
		cout << "Start streaming send 0x90 0x03" << endl;
		serialPutchar(BoardComPortFd, 0x90);
		serialPutchar(BoardComPortFd, 0x03);
		//  get response
		ReadSerialPortResponse(BoardComPortFd);
//		if(!ReadSerialPortResponse(BoardComPortFd))
//			return false;
			
		cout << "Start streaming send 0x90 0x06" << endl;
		serialPutchar(BoardComPortFd, 0x90);
		serialPutchar(BoardComPortFd, 0x06);
		//  get response
		ReadSerialPortResponse(BoardComPortFd);
//		if(!ReadSerialPortResponse(BoardComPortFd))
//			return false;
			
		cout << "Start streaming send 0x90 0x01" << endl;
		serialPutchar(BoardComPortFd, 0x90);
		serialPutchar(BoardComPortFd, 0x01);
		//  get the expected response, the next two bytes before stream starts
		auto response1 = serialGetchar(BoardComPortFd);
		auto response2 = serialGetchar(BoardComPortFd);
//		if (response1 == -1 || response2 == -1)
//			return false;
			
		cout << "Streaming started: " << response1 << " " << response2 << endl;
		StreamRunning = true;
	}
	return StreamRunning;
}


//  Stop board streaming
//
void ContecDataReader::StopStreaming()
{
	if (StreamRunning)
	{
		try
		{
			cout << "Start streaming send 0x90 0x02" << endl;
			serialPutchar(BoardComPortFd, 0x90);
			serialPutchar(BoardComPortFd, 0x02);
			
			StreamRunning = false;
		}
		catch (const BrainFlowException &err)
		{
			Logging.AddLog("ContecDataReader", "StopStreaming", format("Failed to release to board. Error %d %s.", err.exit_code, err.what()), LogLevelError);
		}
	}
}



//  Reconnect to Board
//  tries to restart board streaming
//
void ContecDataReader::EstablishConnectionWithBoard()
{
	if (!BoardReady())
	{
		if (InitializeBoard() != 0)
			usleep(1*USLEEP_SEC);
	}
}
	

//  Check conditions are OK to read data from the board
//
bool ContecDataReader::PreparedToReadBoard()
{
	if (!BoardOn)
	{
		usleep(1*USLEEP_SEC);
		return false;
	}
	
	//  make sure we are connected to the board
	EstablishConnectionWithBoard();
			
	if (!BoardReady())
	{
		usleep(1*USLEEP_SEC);      
		return false;
	}
	else if (RequestToggleStreaming)
	{
		if (StreamRunning)
			StopStreaming();
		else
			StartStreaming();
		
		RequestToggleStreaming = false;
		return false;
	}
	else if (!StreamRunning)
	{
		usleep(1*USLEEP_SEC);
		return false;
	}
	else if ((InvalidSampleCounter * SENSOR_SLEEP) > 3000)
	{
		//  have not received fresh samples in three seconds, release board and reinitialize
		Logging.AddLog("ContecDataReader", "PreparedToReadBoard", "Too long without valid sample. Reconnecting to board.", LogLevelError);
		ReleaseBoard();
		usleep(1*USLEEP_SEC);
		return false;
	}
	
	return true;
}


//  Lock onto the header byte at the start of the stream
//
void ContecDataReader::FindHeader()
{
	cout << "Looking for header ..." <<  endl;
	auto readChar = serialGetchar(BoardComPortFd);
	int invalidReads = 0;
	while (readChar != 0xA0)
	{
		readChar = serialGetchar(BoardComPortFd);
		cout << "Read byte: " << readChar << endl;
		
		if (readChar == -1)
		{
			invalidReads++;
			if (invalidReads > 3)
			{
				cout << "Three seconds without reading data. Releasing board " << readChar << endl;
				ReleaseBoard();
				return;
			}
		}
	}	
	cout << "Found header ..." <<  endl;
}


//  Output the current raw read buffer to the console
//
void OutputBinaryToConsole(char* readBuffer, int readCounter, double* channelData, int elapsedMs)
{
	printf("\033[2J\033[1;1H");
	
	timeval tv;
	gettimeofday(&tv, NULL);
	tm* timeNow = localtime(&(tv.tv_sec));
	
	cout << "Read " << readCounter << " samples in " << setfill('0') << setw(3) << elapsedMs << " ms | " << setfill('0') << setw(2) << timeNow->tm_hour << ":" << setfill('0') << setw(2) << timeNow->tm_min  << ":" << setfill('0') << setw(2) << timeNow->tm_sec << endl;
	for (int i = 0; i < 32; i++)
	{
		//  trace out each of the 32 bytes
		cout <<  "Byte" << setw(2) << setfill('0') << i << "   "  << format(BYTE_TO_BINARY_PATTERN, BYTE_TO_BINARY(readBuffer[i])) << "   "  << format("%02x", readBuffer[i]) << "   "  << setfill('0') << setw(3) << format("%d", readBuffer[i]);
		//  in the first 20 rows, put the calculated channel data
		if (i < 20)
		{
			cout << "  " << setw(6) << channelData[i];
		}
		cout << endl;
	}
}


ChronoTimer timer;
int readCounter = 0;

//  Board reading / control thread run function
//
void ContecDataReader::RunFunction()
{
	double *sample = NULL;
	int res = 0;
	int bytesRead = 0;
	char readBuffer[32] = "";
	int invalidReads = 0;

	timer.Start();
	readCounter = 0;
	
	while (ThreadRunning)
	{
		//  check to see that port is open and stream is running
		if (!PreparedToReadBoard())
			continue;
		
		//  lock onto the start of data frame
		FindHeader();
		
		bool readingInSync = true;
		invalidReads = 0;
		
		//  keep reading while we are in sync and stream is running
		while (readingInSync && PreparedToReadBoard())
		{
			//  each pass of this loop we will keep reading until we have 32 bytes read from the port
			while (bytesRead < 32 )
			{
				int readBytes = read(BoardComPortFd, readBuffer + bytesRead, 32 - bytesRead);
				if (readBytes > 0)
				{
					bytesRead += readBytes;
					invalidReads = 0;
				}
				else
				{
					invalidReads++;
					if (invalidReads > 3)
					{
						cout << "More than 3 seconds without reading from the port. Releasing board" << endl;
						ReleaseBoard();
						break;
					}
				}
			}
			
			//  check that we have a complete frame, and that the end byte is the start of the next frame
			if (bytesRead == 32 && readBuffer[31] == 0xA0)
			{
				readCounter++;	//  count this complete sample reading
				ProcessData(readBuffer);
			}
			else
			{
				cout << "Lost sync " << bytesRead << endl;
				for (int i = 0; i < bytesRead; i++)
				{
					cout << "Lost sync byte " << i << " " << (int)readBuffer[i] << endl;
				}
					
				readingInSync = false;	//  failed to sync with last frame
			}
			bytesRead = 0;
			memset(readBuffer, 0x00, 32);
		}
	}
}


// Process a chunk of data read from the board
// send to broadcast thread and logging if enabled
//
void ContecDataReader::ProcessData(char *readBuffer)
{
	double channelData[20] = { MISSING_VALUE };
	
	int channelCount = 0;
	//  make 11 bit numbers with one sign bit from the array of bytes
	for(int i = 0 ; i < 24 ; i += 3)
	{
		//  first number, set first 8 bits
		uint16_t num1 = readBuffer[i];
		//  add three more bits from the next byte
		num1 += ((readBuffer[i + 1] & 0x07) * 256);
		//  sign bit
		int value1 = num1;
		if (readBuffer[i + 1] & 0x08)
			value1 *= -1;
		channelData[channelCount++] = value1;

		//  set the first four bits of the second number using last four bits of second byte
		uint16_t num2 = ((readBuffer[i + 1] & 0xF0) >> 4);
		//  set the remaining bits of the second number using the third byte
		num2 += (16* (readBuffer[i + 2] & 0x7F));
		//  sign bit
		int value2 = num2;
		if (readBuffer[i + 2] & 0x08)
			value2 *= -1;
		channelData[channelCount++] = value2;
	}
	
	channelCount = 0;
	for (int i = 24; i < 30; i += 3)
	{
		//  first number, set first 8 bits
		uint16_t num1 = readBuffer[i];
		//  add three more bits from the next byte
		num1 += ((readBuffer[i + 1] & 0x07) * 256);
		//  sign bit
		int value1 = num1;
		if (readBuffer[i + 1] & 0x08)
			value1 *= -1;
		channelData[channelCount++] = value1;

		//  set the first four bits of the second number using last four bits of second byte
		uint16_t num2 = ((readBuffer[i + 1] & 0xF0) >> 4);
		//  set the remaining bits of the second number using the third byte
		num2 += (16* (readBuffer[i + 2] & 0x7F));
		//  sign bit
		int value2 = num2;
		if (readBuffer[i + 2] & 0x08)
			value2 *= -1;
		channelData[channelCount++] = value2;
	}
	
	//  fill the sample with 16 EEG plus 4 other channels
	GenericSample* newSample = new GenericSample(16, 0, 4, 1);
	
	//  set the parsed data to the data structure
	//  TODO - if there is to be any math done on channelData before booking it as EEG data,
	//  then we would do this here on the channelData[] elements
	//
	
	//  put first 16 numbers into EXG channels
	for (int i = 0; i < 16; i++)
		newSample->SetExg(i, channelData[i]);
	
	//  put next four numbers into Other channels
	for (int i = 0; i < 4; i++)
		newSample->SetOther(i, channelData[i + 16]);
	
	NewSampleCallback(newSample);
	
	//  report the current reading to the console three times a second
	if(timer.ElapsedMilliseconds() > 333)
	{
		OutputBinaryToConsole(readBuffer, readCounter, channelData, timer.ElapsedMilliseconds());
		readCounter = 0;
		timer.Reset();
	}
	
}



//  Parse the raw data and create a sample type for this board
//
BFSample* ContecDataReader::ParseRawData(double * sample)
{
	//return new ContecSample(sample);
	return NULL;
	
}


//  Calculate the reading time this chunk
//  used to smooth out the sample times
//
void ContecDataReader::CalculateReadingTimeThisChunk(double** chunk, int samples, double& period, double& oldestSampleTime)
{
	auto timeNow = (chrono::duration_cast< milliseconds >(system_clock::now().time_since_epoch()).count() / 1000.0);	
		
	if (LastTimeStampSync > 0)
	{
		oldestSampleTime = LastTimeStampSync;
	}
	else
	{
		oldestSampleTime = timeNow - (samples / SampleRate);
	}
	
	LastTimeStampSync = timeNow;
		
	period = (timeNow - oldestSampleTime) / samples;
	
}


