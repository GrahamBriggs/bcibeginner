/************************************************************************************************
 * This wrapper class was written by Daniel J Riches for Gordon Hendersons WiringPi C library   *
 * I take no responsibility for this wrapper class providing proper functionality and give no   *
 * warranty of any kind, nor it's use or fitness for any purpose. You use this wrapper at your  *
 * own risk.                                                                                    *
 *                                                                                              *
 * This code is released as Open Source under GNU GPL license, please ensure that you have a    *
 * copy of the license and understand the usage terms and conditions.                           *
 *                                                                                              *
 * I take no credit for the underlying functionality that this wrapper provides.                *
 * Authored: 29/04/2013                                                                         *
 ************************************************************************************************
 * Changelog
 * Date         Changed By          Details of change
 * 08 May 2013  Daniel Riches       Corrected c library mappings for I2C and SPI, added this header
 * 
 ************************************************************************************************
 * Changelog
 * Date         Changed By          Details of change  
 * 23 Nov 2013  Gerhard de Clercq   Changed digitalread to return int and implemented wiringPiISR
 * 
 ************************************************************************************************
 * Changelog
 * Date         Changed By          Details of change  
 * 18 Jan 2016  Marcus Lum          Updated imported methods to current wiringPi 
 * 
 ************************************************************************************************
 ************************************************************************************************
 * Changelog
 * Date         Changed By          Details of change
 * December 2016  Graham Briggs     Forked this wrapper and refactored to work with libwiringPi
 *
 ************************************************************************************************/

//  Comment change on Pi version using Visual Studio on PC

using System;
using System.Runtime.InteropServices;


namespace WiringPiWrapper
{
    public static class Constants
    {
        public static double RPiPwmClockSpeed = 19.2e6;
        public static int RPiPwmFrequency = 50;
    }

    public enum WiringPiPinMode
    {
        Input = 0,
        Output = 1,
        PWMOutput = 2,
        GPIOClock = 3,
        SoftPwmOutput,
        SoftToneOutput,
        PwmToneOutput,
    }

    public enum WiringPiPinValue
    {
        Low = 0,
        High = 1
    }

    public enum WiringPiPullUpDownValue
    {
        Off = 0,
        Down = 1,
        Up = 2
    }

    public enum WiringPiInterruptLevels
    {
        INT_EDGE_SETUP = 0,
        INT_EDGE_FALLING = 1,
        INT_EDGE_RISING = 2,
        INT_EDGE_BOTH = 3
    }





    /// <summary>
    /// Initialization
    /// Raw functions can be used to initialise Gordon's library if you only want to use raw functions from wiringPi
    /// If you are using the extension, you should call SetupWiringPiExtension
    /// </summary>
    public class Setup
    {
        [DllImport("libwiringPi.so", EntryPoint = "wiringPiSetup")]
        public static extern int wiringPiSetup();

        [DllImport("libwiringPi.so", EntryPoint = "wiringPiSetupGpio")]
        public static extern int wiringPiSetupGpio();

        [DllImport("libwiringPi.so", EntryPoint = "wiringPiSetupSys")]
        public static extern int wiringPiSetupSys();

        [DllImport("libwiringPi.so", EntryPoint = "wiringPiSetupPhys")]
        public static extern int wiringPiSetupPhys();
    }


    /// <summary>
    /// GPIO pin functions
    /// </summary>
    public class GPIO
    {
        [DllImport("libwiringPi.so", EntryPoint = "pinMode")]           
        public static extern void pinMode(int pin, WiringPiPinMode mode);

        [DllImport("libwiringPi.so", EntryPoint = "pinModeAlt")]
        public static extern void pinModeAlt(int pin, WiringPiPinMode mode);

        [DllImport("libwiringPi.so", EntryPoint = "digitalWrite")]      
        public static extern void digitalWrite(int pin, WiringPiPinValue value);

        [DllImport("libwiringPi.so", EntryPoint = "digitalWriteByte")]      
        public static extern void digitalWriteByte(int value);

        [DllImport("libwiringPi.so", EntryPoint = "digitalRead")]
        public static extern int digitalRead(int pin);

        [DllImport("libwiringPi.so", EntryPoint = "analogRead")]           
        public static extern int analogRead(int pin);

        [DllImport("libwiringPi.so", EntryPoint = "analogWrite")]
        public static extern int analogWrite(int pin, int value);
        
        [DllImport("libwiringPi.so", EntryPoint = "pullUpDnControl")]         
        public static extern void pullUpDnControl(int pin, WiringPiPullUpDownValue pud);

        //This pwm mode cannot be used when using GpioSys mode!!
        [DllImport("libwiringPi.so", EntryPoint = "pwmWrite")]              
        public static extern void pwmWrite(int pin, int value);

        [DllImport("libwiringPi.so", EntryPoint = "pwmSetMode")]             
        public static extern void pwmSetMode(WiringPiPinMode mode);

        [DllImport("libwiringPi.so", EntryPoint = "pwmSetRange")]             
        public static extern void pwmSetRange(uint range);

        [DllImport("libwiringPi.so", EntryPoint = "pwmSetClock")]             
        public static extern void pwmSetClock(int divisor);

        [DllImport("libwiringPi.so", EntryPoint = "gpioClockSet")]              
        public static extern void gpioClockSet(int pin, int freq);

        //  Software PWM
        //
        [DllImport("libwiringPi.so", EntryPoint = "softPwmCreate")]
        public static extern int softPwmCreate(int pin, int value, int range);

        [DllImport("libwiringPi.so", EntryPoint = "softPwmWrite")]
        public static extern void softPwmWrite(int pin, int value);

        [DllImport("libwiringPi.so", EntryPoint = "softPwmStop")]
        public static extern void softPwmStop(int pin);
    }

    
    

    /// <summary>
    /// Provides use of the Timing functions such as delays
    /// </summary>
    public class Timing
    {
        [DllImport("libwiringPi.so", EntryPoint = "millis")]
        public static extern uint millis();

        [DllImport("libwiringPi.so", EntryPoint = "micros")]
        public static extern uint micros();

        [DllImport("libwiringPi.so", EntryPoint = "delay")]
        public static extern void delay(uint howLong);

        [DllImport("libwiringPi.so", EntryPoint = "delayMicroseconds")]
        public static extern void delayMicroseconds(uint howLong);
    }


    /// <summary>
    /// Provides access to the Thread priority and interrupts for IO
    /// </summary>
    //
    public delegate void ISRCallback();
    //
    public class Interrupts
    {
        [DllImport("libwiringPi.so", EntryPoint = "piHiPri")]
        public static extern int piHiPri(int priority);

        [DllImport("libwiringPi.so", EntryPoint = "waitForInterrupt")]
        public static extern int waitForInterrupt(int pin, int timeout);
        
        [DllImport("libwiringPi.so", EntryPoint = "wiringPiISR")]
        public static extern int wiringPiISR(int pin, int mode, ISRCallback method);

    }

    public class PiBoard
    {
        [DllImport("libwiringPi.so", EntryPoint = "piBoardRev")]
        public static extern int piBoardRev();

        [DllImport("libwiringPi.so", EntryPoint = "wpiPinToGpio")]
        public static extern int wpiPinToGpio(int wPiPin);

        [DllImport("libwiringPi.so", EntryPoint = "physPinToGpio")]
        public static extern int physPinToGpio(int physPin);

        [DllImport("libwiringPi.so", EntryPoint = "setPadDrive")]
        public static extern void setPadDrive(int group, int value);
    }

    /// <summary>
    /// Provides SPI port functionality
    /// </summary>
    public class SPI
    {
        /// <summary>
        /// Configures the SPI channel specified on the Raspberry Pi
        /// </summary>
        /// <param name="channel">Selects either Channel 0 or 1 for use</param>
        /// <param name="speed">Selects speed, 500,000 to 32,000,000</param>
        /// <returns>-1 for an error, or the linux file descriptor the channel uses</returns>
        [DllImport("libwiringPi.so", EntryPoint = "wiringPiSPISetup")]
        public static extern int wiringPiSPISetup(int channel, int speed);

        /// <summary>
        /// Read and Write data over the SPI bus, don't forget to configure it first
        /// </summary>
        /// <param name="channel">Selects Channel 0 or Channel 1 for this operation</param>
        /// <param name="data">signed byte array pointer which holds the data to send and will then hold the received data</param>
        /// <param name="len">How many bytes to write and read</param>
        /// <returns>-1 for an error, or the linux file descriptor the channel uses</returns>
        [DllImport("libwiringPi.so", EntryPoint = "wiringPiSPIDataRW")]
        public static unsafe extern int wiringPiSPIDataRW(int channel, byte* data, int len);  //char is a signed byte
    }

    /// <summary>
    /// Provides access to the I2C port
    /// </summary>
    public class I2C
    {
        [DllImport("libwiringPi.so", EntryPoint = "wiringPiI2CSetup")]
        public static extern int wiringPiI2CSetup(int devId);

        [DllImport("libwiringPi.so", EntryPoint = "wiringPiI2CRead")]
        public static extern int wiringPiI2CRead(int fd);

        [DllImport("libwiringPi.so", EntryPoint = "wiringPiI2CWrite")]
        public static extern int wiringPiI2CWrite(int fd, int data);

        [DllImport("libwiringPi.so", EntryPoint = "wiringPiI2CWriteReg8")]
        public static extern int wiringPiI2CWriteReg8(int fd, int reg, int data);

        [DllImport("libwiringPi.so", EntryPoint = "wiringPiI2CWriteReg16")]
        public static extern int wiringPiI2CWriteReg16(int fd, int reg, int data);

        [DllImport("libwiringPi.so", EntryPoint = "wiringPiI2CReadReg8")]
        public static extern int wiringPiI2CReadReg8(int fd, int reg);

        [DllImport("libwiringPi.so", EntryPoint = "wringPiI2CReadReg16")]
        public static extern int wiringPiI2CReadReg16(int fd, int reg);
    }




}