#pragma once

struct OpenBciData
{
	OpenBciData(double** rawData, int point)
	{
		SampleIndex = rawData[0][point];
		ExgCh0 = rawData[1][point];
		ExgCh1 = rawData[2][point];
		ExgCh2 = rawData[3][point];
		ExgCh3 = rawData[4][point];
		ExgCh4 = rawData[5][point];
		ExgCh5 = rawData[6][point];
		ExgCh6 = rawData[7][point];
		ExgCh7 = rawData[8][point];
		AcelCh0 = rawData[9][point];
		AcelCh1 = rawData[10][point];
		AcelCh2 = rawData[11][point];
		Other0 = rawData[12][point];
		Other1 = rawData[13][point];
		Other2 = rawData[14][point];
		Other3 = rawData[15][point];
		Other4 = rawData[16][point];
		Other5 = rawData[17][point];
		Other6 = rawData[18][point];
		AngCh0 = rawData[19][point];
		AngCh1 = rawData[20][point];
		AngCh2 = rawData[21][point];
		TimeStamp = rawData[22][point];
	}
	
	double SampleIndex;
	double ExgCh0;
	double ExgCh1;
	double ExgCh2;
	double ExgCh3;
	double ExgCh4;
	double ExgCh5;
	double ExgCh6;
	double ExgCh7;
	double AcelCh0;
	double AcelCh1;
	double AcelCh2;
	double Other0;
	double Other1;
	double Other2;
	double Other3;
	double Other4;
	double Other5;
	double Other6;
	double AngCh0;
	double AngCh1;
	double AngCh2;
	double TimeStamp;
};