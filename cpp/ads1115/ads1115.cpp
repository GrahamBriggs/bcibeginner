// asd1115.c read TMP37 temperature sensor ANC0
// operates in continuous mode
// pull up resistors in module caused problems
// used level translator - operated ADS1115 at 5V
// by Lewis Loflin lewis@bvu.net
// www.bristolwatch.com
// http://www.bristolwatch.com/rpi/ads1115.html

#include <stdio.h>
#include <sys/ioctl.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <unistd.h>    // read/write usleep
#include <stdlib.h>    // exit function
#include <inttypes.h>  // uint8_t, etc
#include <linux/i2c-dev.h> // I2C bus definitions
#include <chrono>

using namespace std::chrono;

// Connect ADDR to GRD.
// Setup to use ADC0 single ended

int fd;
// Note PCF8591 defaults to 0x48!
int asd_address = 0x48;
int16_t val;
uint8_t writeBuf[3];
uint8_t readBuf[2];
float myfloat;

const float VPS = 4.096 / 32768.0;  // volts per step

/*
The resolution of the ADC in single ended mode 
we have 15 bit rather than 16 bit resolution, 
the 16th bit being the sign of the differential 
reading.
*/


std::chrono::steady_clock::time_point LastLoggedTime;
int RecordsLogged;


int main() {

	// open device on /dev/i2c-1 the default on Raspberry Pi B
	if((fd = open("/dev/i2c-1", O_RDWR)) < 0) {
		printf("Error: Couldn't open device! %d\n", fd);
		exit(1);
	}

	// connect to ADS1115 as i2c slave
	if(ioctl(fd, I2C_SLAVE, asd_address) < 0) {
		printf("Error: Couldn't find device on address!\n");
		exit(1);
	}

	// set config register and start conversion
	// AIN0 and GND, 4.096v, 128s/s
	// Refer to page 19 area of spec sheet
	writeBuf[0] = 1;  // config register is 1
	writeBuf[1] = 0b11000010;  // 0xC2 single shot off
	// bit 15 flag bit for single shot not used here
	// Bits 14-12 input selection:
	// 100 ANC0; 101 ANC1; 110 ANC2; 111 ANC3
	// Bits 11-9 Amp gain. Default to 010 here 001 P19
	// Bit 8 Operational mode of the ADS1115.
	// 0 : Continuous conversion mode
	// 1 : Power-down single-shot mode (default)

	writeBuf[2] = 0b10000101;  // bits 7-0  0x85
	// Bits 7-5 data rate default to 100 for 128SPS
	// Bits 4-0  comparator functions see spec sheet.

	// begin conversion
	if(write(fd, writeBuf, 3) != 3) {
		perror("Write to register 1");
		exit(1);
	}

	sleep(1);

	printf("ASD1115 Demo will take five readings.\n");

 
	// set pointer to 0
	readBuf[0] = 0;
	if (write(fd, readBuf, 1) != 1) {
		perror("Write register select");
		exit(-1);
	}
  
	// take 5000 readings:

	int count = 1;

	while (1) {

		// read conversion register
		if(read(fd, readBuf, 2) != 2) {
			perror("Read conversion");
			exit(-1);
		}

		// could also multiply by 256 then add readBuf[1]
		val = readBuf[0] << 8 | readBuf[1];

		// with +- LSB sometimes generates very low neg number.
		if(val < 0)   val = 0;

		myfloat = val * VPS;  // convert to voltage
		
		RecordsLogged++;

		//  update the display once a second
			if(duration_cast<milliseconds>(steady_clock::now() - LastLoggedTime).count() > 1000)
		{
			
		
			printf("Conversion number %d HEX 0x%02x DEC %d %4.3f volts. Records %d\n",
				count,
				val,
				val,
				myfloat, RecordsLogged);
			// TMP37 20mV per deg C
			printf("Temp. C = %4.2f \n", myfloat / 0.02);
			printf("Temp. F = %4.2f \n", myfloat / 0.02 * 9 / 5 + 32);
			
			LastLoggedTime = steady_clock::now();
			
		}

		/* Output:
		 Conversion number 1 HEX 0x1113 DEC 4371 0.546 volts.
		 Temp. C = 27.32
		 Temp. F = 81.17
		 */
     
		usleep(4*1000);

		count++;  // inc count
		if(count > 10000)   break;

	} // end while loop

	// power down ASD1115
	writeBuf[0] = 1;     // config register is 1
	writeBuf[1] = 0b11000011;  // bit 15-8 0xC3 single shot on
	writeBuf[2] = 0b10000101;  // bits 7-0  0x85
	if(write(fd, writeBuf, 3) != 3) {
		perror("Write to register 1");
		exit(1);
	}

	close(fd);

	return 0;
}