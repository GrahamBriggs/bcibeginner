
using static PlatformHelper.PlatformHelper;

namespace WiringPiWrapper
{

    /// <summary>
    /// GpioManager
    /// Handles GPIO pin state for the program
    /// also wraps up pin handling in ifdef so you can compile and run on windows with dummy pin objects
    /// </summary>
    public static class WiringPiProxy
    {
        public static int WiringPiSetup()
        {
            if (Linux)
                return Setup.wiringPiSetup();
            else
                return 1;
        }

        public static int WiringPiSetupPhys()
        {
            if (Linux)
                return Setup.wiringPiSetupPhys();
            else
                return 1;
        }

        public static int WiringPiSetupGpio()
        {
            if (Linux)
                return Setup.wiringPiSetupGpio();
            else
                return 1;
        }

        public static int WiringPiSetupSys()
        {
            if (Linux)
                return Setup.wiringPiSetupSys();
            else
                return 1;
        }

		public static void PinMode(int pin, WiringPiPinMode mode)
        {
            if (Linux)
                GPIO.pinMode(pin, mode);
        }

		public static void PinModeAlt(int pin, WiringPiPinMode mode)
        {
            if (Linux)
                GPIO.pinModeAlt(pin, mode);
        }

		public static void DigitalWrite(int pin, WiringPiPinValue value)
        {
            if (Linux)
                GPIO.digitalWrite(pin, value);
        }

        public static void DigitalWriteByte(int value)
        {
            if (Linux)
                GPIO.digitalWriteByte(value);
        }

        public static int DigitalRead(int pin)
        {
            if (Linux)
                return GPIO.digitalRead(pin);
            else
                return 1;
        }

        public static int AnalogRead(int pin)
        {
            if (Linux)
                return GPIO.analogRead(pin);
            else
                return 1;
        }

        public static int AnalogWrite(int pin, int value)
        {
            if (Linux)
                return GPIO.analogWrite(pin, value);
            else
                return 1;
        }

		public static void PullUpDnControl(int pin, WiringPiPullUpDownValue pud)
        {
            if (Linux)
                GPIO.pullUpDnControl(pin, pud);
        }

        public static void PwmWrite(int pin, int value)
        {
            if (Linux)
                GPIO.pwmWrite(pin, value);
        }

		public static void PwmSetMode(WiringPiPinMode mode)
        {
            if (Linux)
                GPIO.pwmSetMode(mode);
        }

        public static void PwmSetRange(uint range)
        {
            if (Linux)
                GPIO.pwmSetRange(range);
        }

        public static void PwmSetClock(int divisor)
        {
            if (Linux)
                GPIO.pwmSetClock(divisor);
        }

        public static void GpioClockSet(int pin, int freq)
        {
            if (Linux)
                GPIO.gpioClockSet(pin, freq);
        }

        //  Software PWM
        //
        public static int SoftPwmCreate(int pin, int value, int range)
        {
            if (Linux)
                return GPIO.softPwmCreate(pin, value, range);
            else
                return 1;
        }

        public static void SoftPwmWrite(int pin, int value)
        {
            if (Linux)
                GPIO.softPwmWrite(pin, value);
        }

        public static void SoftPwmStop(int pin)
        {
            if (Linux)
                GPIO.softPwmStop(pin);
        }



    }
}
