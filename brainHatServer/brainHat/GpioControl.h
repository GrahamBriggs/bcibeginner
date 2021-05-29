#pragma once

#include "PinController.h"





void StartGpioController(int pinConnection, int pinRecording);
void StopGpioController();

void ConnectionLightShowConnecting();
void ConnectionLightShowReady();
void ConnectionLightShowConnected();
void ConnectionLightShowPaused();

void RecordingLight(bool enable);
