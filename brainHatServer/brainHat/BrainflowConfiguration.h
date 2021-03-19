#pragma once


typedef enum TestSignalMode
{
	InternalGround,
	Signal1Slow,
	Signal1Fast,
	DcSignal,
	Signal2Slow,
	Signal2Fast,
} TestSignalMode;

typedef enum ChannelGain
{
	x1,
	x2,
	x4,
	x6,
	x8,
	x12,
	x24,
} ChannelGain;

typedef enum AdsChannelInputType
{
	Normal,
	Shorted,
	BiasMeas,
	Mvdd,
	Temp,
	Testsig,
	BiasDrp,
	BiasDrn,
} AdsChannelInputType;
