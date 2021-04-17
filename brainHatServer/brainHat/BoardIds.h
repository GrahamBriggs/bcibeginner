#pragma once

//  BrainhatBoardIds
//  wrapper around brainflow board ids to support additional hardware external to brainflow board shim

//  BoardIds
//  This enum must mirror brainflow_constants for values > -50
enum class BrainhatBoardIds : int
{
	UNDEFINED = -99,
	CONTEC_KT88 = -50,
	//
	//  brainflow_constants.h
	PLAYBACK_FILE_BOARD = -3,
	STREAMING_BOARD = -2,
	SYNTHETIC_BOARD = -1,
	CYTON_BOARD = 0,
	GANGLION_BOARD = 1,
	CYTON_DAISY_BOARD = 2,
	GALEA_BOARD = 3,
	GANGLION_WIFI_BOARD = 4,
	CYTON_WIFI_BOARD = 5,
	CYTON_DAISY_WIFI_BOARD = 6,
	BRAINBIT_BOARD = 7,
	UNICORN_BOARD = 8,
	CALLIBRI_EEG_BOARD = 9,
	CALLIBRI_EMG_BOARD = 10,
	CALLIBRI_ECG_BOARD = 11,
	FASCIA_BOARD = 12,
	NOTION_1_BOARD = 13,
	NOTION_2_BOARD = 14,
	IRONBCI_BOARD = 15,
	GFORCE_PRO_BOARD = 16,
	FREEEEG32_BOARD = 17,
	// use it to iterate
	FIRST = UNDEFINED,
	LAST = FREEEEG32_BOARD
};


int getNumberOfExgChannels(int boardId);
int getNumberOfAccelChannels(int boardId);
int getNumberOfOtherChannels(int boardId);
int getNumberOfAnalogChannels(int boardId);
