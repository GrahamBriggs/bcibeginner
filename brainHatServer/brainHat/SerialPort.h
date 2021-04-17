#pragma once

/*
 *  wiringSerial.h:
 *	Handle a serial port
 ***********************************************************************
 * This file was modified from wiringPi:
 *	https://projects.drogon.net/raspberry-pi/wiringpi/
 *
 */

#ifdef __cplusplus
extern "C" {
#endif

	extern int   serialOpen(const char *device, const int baud, const int timeout);
	extern void  serialClose(const int fd);
	extern void  serialFlush(const int fd);
	extern void  serialPutchar(const int fd, const unsigned char c);
	extern void  serialPuts(const int fd, const char *s);
	extern void  serialPrintf(const int fd, const char *message, ...);
	extern int   serialDataAvail(const int fd);
	extern int   serialGetchar(const int fd);

#ifdef __cplusplus
}
#endif