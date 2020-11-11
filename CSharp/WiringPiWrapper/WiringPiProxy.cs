

namespace WiringPiWrapper
{

    /// <summary>
    /// GpioManager
    /// Handles GPIO pin state for the program
    /// also wraps up pin handling in ifdef so you can compile and run on windows with dummy pin objects
    /// </summary>
    public static class WiringPiProxy
    {

#if MONO
        public static int WiringPiSetup()
        {
            return Setup.wiringPiSetup();

        }

        public static int WiringPiSetupPhys()
        {
            return Setup.wiringPiSetupPhys();
        }

        public static int WiringPiSetupGpio()
        {
            return Setup.wiringPiSetupGpio();
        }

        public static int WiringPiSetupSys()
        {
            return Setup.wiringPiSetupSys();
        }

		public static void PinMode(int pin, WiringPiPinMode mode)
        {
            GPIO.pinMode(pin, mode);
        }

		public static void PinModeAlt(int pin, WiringPiPinMode mode)
        {
            GPIO.pinModeAlt(pin, mode);
        }

		public static void DigitalWrite(int pin, WiringPiPinValue value)
        {
            GPIO.digitalWrite(pin, value);
        }

        public static void DigitalWriteByte(int value)
        {
            GPIO.digitalWriteByte(value);
        }

        public static int DigitalRead(int pin)
        {
            return GPIO.digitalRead(pin);
        }

        public static int AnalogRead(int pin)
        {
            return GPIO.analogRead(pin);
        }

        public static int AnalogWrite(int pin, int value)
        {
            return GPIO.analogWrite(pin, value);
        }

		public static void PullUpDnControl(int pin, WiringPiPullUpDownValue pud)
        {
            GPIO.pullUpDnControl(pin, pud);
        }

        public static void PwmWrite(int pin, int value)
        {
            GPIO.pwmWrite(pin, value);
        }

		public static void PwmSetMode(WiringPiPinMode mode)
        {
            GPIO.pwmSetMode(mode);
        }

        public static void PwmSetRange(uint range)
        {
            GPIO.pwmSetRange(range);
        }

        public static void PwmSetClock(int divisor)
        {
            GPIO.pwmSetClock(divisor);
        }

        public static void GpioClockSet(int pin, int freq)
        {
            GPIO.gpioClockSet(pin, freq);
        }

        //  Software PWM
        //
        public static int SoftPwmCreate(int pin, int value, int range)
        {
            return GPIO.softPwmCreate(pin, value, range);
        }

        public static void SoftPwmWrite(int pin, int value)
        {
            GPIO.softPwmWrite(pin, value);
        }

        public static void SoftPwmStop(int pin)
        {
            GPIO.softPwmStop(pin);
        }
#else

        public static int WiringPiSetup()
        {
            return 0;

        }

        public static int WiringPiSetupPhys()
        {
            return 0;
        }

        public static int WiringPiSetupGpio()
        {
            return 0;
        }

        public static int WiringPiSetupSys()
        {
            return 0;
        }

        public static void PinMode(int pin, WiringPiPinMode mode)
        {
            
        }

        public static void PinModeAlt(int pin, WiringPiPinMode mode)
        {
           
        }

        public static void DigitalWrite(int pin, WiringPiPinValue value)
        {
           
        }

        public static void DigitalWriteByte(int value)
        {
           
        }

        public static int DigitalRead(int pin)
        {
            return 0;
        }

        public static int AnalogRead(int pin)
        {
            return 0;
        }

        public static int AnalogWrite(int pin, int value)
        {
            return 0;
        }

        public static void PullUpDnControl(int pin, WiringPiPullUpDownValue pud)
        {
            
        }

        public static void PwmWrite(int pin, int value)
        {
           
        }

        public static void PwmSetMode(WiringPiPinMode mode)
        {
           
        }

        public static void PwmSetRange(uint range)
        {
           
        }

        public static void PwmSetClock(int divisor)
        {
         
        }

        public static void GpioClockSet(int pin, int freq)
        {
           
        }

        //  Software PWM
        //
        public static int SoftPwmCreate(int pin, int value, int range)
        {
            return 0;
        }

        public static void SoftPwmWrite(int pin, int value)
        {

        }

        public static void SoftPwmStop(int pin)
        {

        }

#endif


    }
}
