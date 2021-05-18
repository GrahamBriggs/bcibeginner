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
#include "BFSampleImplementation.h"
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
ContecDataReader::ContecDataReader(ConnectionChangedCallbackFn connectionChangedFn, NewSampleCallbackFn newSampleFn):
	SampleIndex((int)BrainhatBoardIds::CONTEC_KT88)
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
	BoardDataSource::Init();
	
	BoardComPortFd = -1;
	BoardOn = true;
	IsConnected = false;
	StreamRunning = false;
	ConnectionChangedCallback  = NULL;
	ConnectionChangedDelegate = NULL;
	InvalidSampleCounter = 0;
	
	RawConsoleEnabled = false;
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
	
	Thread::Start();
	
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
bool ContecDataReader::ReadSerialPortResponse()
{
	vector<int> responses;
	int bytesRead = 0;
	bool response = false;
	auto readChar = serialGetchar(BoardComPortFd);
	while (readChar > -1 && bytesRead < 256)
	{
		bytesRead++;
		response = true;
		responses.push_back(readChar);
		readChar = serialGetchar(BoardComPortFd);
	}
		
	if (response)
	{
		string responseString = "";
		for (int i = 0; i < responses.size(); i++)
			responseString += format("%02x ", responses[i]);
		
		Logging.AddLog("ContecDataReader", "ReadSerialPortResponse", format("Response: %s", responseString.c_str()), LogLevelDebug);
	}
		
	return response;
}


//  Start board streaming
//
bool ContecDataReader::StartStreaming()
{
	if (!StreamRunning)
	{
		StreamRunning = false;
		
		Logging.AddLog("ContecDataReader", "StartStreaming", "Sending >> 0x90 0x09", LogLevelDebug);
		serialPutchar(BoardComPortFd, 0x90);
		serialPutchar(BoardComPortFd, 0x09);
		//  get response
		ReadSerialPortResponse();
			
		Logging.AddLog("ContecDataReader", "StartStreaming", "Sending >> 0x90 0x03", LogLevelDebug);
		serialPutchar(BoardComPortFd, 0x90);
		serialPutchar(BoardComPortFd, 0x03);
		//  get response
		ReadSerialPortResponse();
			
		Logging.AddLog("ContecDataReader", "StartStreaming", "Sending >> 0x90 0x06", LogLevelDebug);
		serialPutchar(BoardComPortFd, 0x90);
		serialPutchar(BoardComPortFd, 0x06);
		//  get response
		ReadSerialPortResponse();
		
		Logging.AddLog("ContecDataReader", "StartStreaming", "Sending >> 0x90 0x01", LogLevelDebug);
		serialPutchar(BoardComPortFd, 0x90);
		serialPutchar(BoardComPortFd, 0x01);
		//  get the expected response, the next two bytes before stream starts
		auto response1 = serialGetchar(BoardComPortFd);
		auto response2 = serialGetchar(BoardComPortFd);
		if (response1 == -1 || response2 == -1)
		{
			return false;
		}
		
		Logging.AddLog("ContecDataReader", "StartStreaming", format("Response << %d %d",response1, response2), LogLevelDebug);
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
		Logging.AddLog("ContecDataReader", "StopStreaming", "Sending >> 0x90 0x02", LogLevelDebug);
		serialPutchar(BoardComPortFd, 0x90);
		serialPutchar(BoardComPortFd, 0x02);
			
		StreamRunning = false;
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
	Logging.AddLog("ContecDataReader", "FindHeader", "Looking for header.", LogLevelInfo);
	
	auto readChar = serialGetchar(BoardComPortFd);
	int invalidReads = 0;
	while (readChar != 0xA0)
	{
		readChar = serialGetchar(BoardComPortFd);
		Logging.AddLog("ContecDataReader", "FindHeader", format("Did not find header. Read byte: %02x", readChar), LogLevelWarn);
		
		if (readChar == -1)
		{
			invalidReads++;
			if (invalidReads > 3)
			{
				Logging.AddLog("ContecDataReader", "FindHeader", "Three seconds without reading data. Releasing board.", LogLevelError);
				ReleaseBoard();
				return;
			}
		}
	}	
	
	Logging.AddLog("ContecDataReader", "FindHeader", "Found header", LogLevelInfo);
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
			cout << "  " << setw(6) << setfill(' ') << channelData[i];
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
						Logging.AddLog("ContecDataReader", "RunFunction", "Three seconds without reading data. Releasing board.", LogLevelError);
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
			else if ( BoardReady() )
			{
				cout << "Lost sync " << bytesRead << endl;
				for (int i = 0; i < bytesRead; i++)
				{
					string responsesString = "";
					responsesString += format("%02x ", readBuffer[i]);
					Logging.AddLog("ContecDataReader", "RunFunction", format("Lost sync. Bytes: %s", responsesString.c_str()), LogLevelWarn);
				}
					
				readingInSync = false;	//  failed to sync with last frame
			}
			bytesRead = 0;
			memset(readBuffer, 0x00, 32);
		}
	}
}


void GetNumbersFromTripleOctet(char* readBuffer, int pos, double& value1, double& value2)
{
	//  first number, set first 8 bits
	uint16_t num1 = readBuffer[pos];
	//  add three more bits from the next byte
	num1 += ((readBuffer[pos + 1] & 0x07) * 256);
	value1 = num1;
	//  sign bit
	if (readBuffer[pos + 1] & 0x08)
		value1 *= -1;
	
	//  set the first four bits of the second number using last four bits of second byte
	uint16_t num2 = ((readBuffer[pos + 1] & 0xF0) >> 4);
	//  set the remaining bits of the second number using the third byte
	num2 += (16* (readBuffer[pos + 2] & 0x7F));
	value2 = num2;
	//  sign bit
	if (readBuffer[pos + 2] & 0x08)
		value2 *= -1;
}


// Process a chunk of data read from the board
// send to broadcast thread and logging if enabled
//
void ContecDataReader::ProcessData(char *readBuffer)
{
	double channelData[20] = { MISSING_VALUE };
	int channelCount = 0;
	
	//  process the array of 31 bytes into numbers
	//  we assume that every triple octet yields two 12 bit numbers (11 bits plus sign)
	//  there is one extra byte, assumed to be at beginning if startAt = 1, or end if startAt = 0
	int startAtByte = 0;
	for(int i = startAtByte; i < 30 ; i += 3)
	{
		GetNumbersFromTripleOctet(readBuffer, i, channelData[channelCount], channelData[channelCount+1]);
		channelCount += 2;
	}
	
	//  create a sample with 16 EEG plus 4 other channels
	Sample* newSample = new Sample(16, 0, 4, 0);
	
	// set sample index and time stamp
	newSample->SampleIndex = SampleIndex.GetNextSampleIndex();
	newSample->TimeStamp = (double)GetUnixTimeMilliseconds() / 1000.0;
	
	//  set the parsed data to the data structure
	//  TODO - if there is to be any math done on channelData before booking it as EEG data,
	//  then we would do this here on the channelData[] elements
	//
	//  for example, to set EXG channel zero with the result of contec channel zero minus contec channel 7 would look like the following
	//	newSample->SetExg(0, channelData[0] - channelData[7]);
	
	//  initial implementation, first 16 numbers go into EEG channels
	//  put first 16 numbers into EXG channels
	for (int i = 0; i < 16; i++)
		newSample->SetExg(i, channelData[i]);
	
	//  put next four numbers into Other channels
	for (int i = 0; i < 4; i++)
		newSample->SetOther(i, channelData[i + 16]);
	
	InspectDataStream(newSample);
	
	NewSampleCallback(newSample);
	
	//  report the current reading to the console three times a second
	if(timer.ElapsedMilliseconds() > 333)
	{
		if ( RawConsoleEnabled )
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


