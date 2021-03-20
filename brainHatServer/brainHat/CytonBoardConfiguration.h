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



//  Helper Functions
//

//  Get the index of the bit number or register report comma delimited string
inline int IndexOfBit(int bitNumber) {	return 10 - bitNumber; }


//  Get the channel gain from register report row data
inline ChannelGain GetChannelGain(std::vector<std::string> value)
{
	if (value.size() == 11)
	{
		int channelGain = 0;
		channelGain += value[IndexOfBit(4)] == "1" ? 1 : 0;
		channelGain += value[IndexOfBit(5)] == "1" ? 2 : 0;
		channelGain += value[IndexOfBit(6)] == "1" ? 4 : 0;

		return (ChannelGain)channelGain;
	}

	return ChannelGain::x1;
}


//  Get the channel input type from register report row data
inline AdsChannelInputType GetChannelInputType(std::vector<std::string> value)
{
	if (value.size() == 11)
	{
		int channelInputType = 0;
		channelInputType += value[IndexOfBit(0)] == "1" ? 1 : 0;
		channelInputType += value[IndexOfBit(1)] == "1" ? 2 : 0;
		channelInputType += value[IndexOfBit(2)] == "1" ? 4 : 0;

		return (AdsChannelInputType)channelInputType;
	}

	return AdsChannelInputType::Normal;
}


inline std::string ChannelSetCharacter(int value)
{
	switch (value)
	{
	case 1:
		return "1";
	case 2:
		return "2";
	case 3:
		return "3";
	case 4:
		return "4";
	case 5:
		return "5";
	case 6:
		return "6";
	case 7:
		return "7";
	case 8:
		return "8";
	case 9:
		return "Q";
	case 10:
		return "W";
	case 11:
		return "E";
	case 12:
		return "R";
	case 13:
		return "T";
	case 14:
		return "Y";
	case 15:
		return "U";
	case 16:
		return "I";
	default:
		return "";
	}
}

inline std::string BoolCharacter(bool value)
{
	return value ? "1" : "0";
}