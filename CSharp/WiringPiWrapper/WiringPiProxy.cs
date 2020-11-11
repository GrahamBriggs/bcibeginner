
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
            if (Windows)
                return Setup.wiringPiSetup();
            else
                return 1;
        }

        public static int WiringPiSetupPhys()
        {
            if (Windows)
                return Setup.wiringPiSetupPhys();
            else
                return 1;
        }

        public static int WiringPiSetupGpio()
        {
            if (Windows)
                return Setup.wiringPiSetupGpio();
            else
                return 1;
        }

        public static int WiringPiSetupSys()
        {
            if (Windows)
                return Setup.wiringPiSetupSys();
            else
                return 1;
        }

		public static void PinMode(int pin, WiringPiPinMode mode)
        {
            GPIO.pinMode(pin, mode);
        }

		public static void PinModeAlt(int pin, WiringPiPinMode mode)
        {
            if (Windows)
                GPIO.pinModeAlt(pin, mode);
        }

		public static void DigitalWrite(int pin, WiringPiPinValue value)
        {
            GPIO.digitalWrite(pin, value);
        }

        public static void DigitalWriteByte(int value)
        {
            if (Windows)
                GPIO.digitalWriteByte(value);
        }

        public static int DigitalRead(int pin)
        {
            if (Windows)
                return GPIO.digitalRead(pin);
            else
                return 1;
        }

        public static int AnalogRead(int pin)
        {
            if (Windows)
                return GPIO.analogRead(pin);
            else
                return 1;
        }

        public static int AnalogWrite(int pin, int value)
        {
            if (Windows)
                return GPIO.analogWrite(pin, value);
            else
                return 1;
        }

		public static void PullUpDnControl(int pin, WiringPiPullUpDownValue pud)
        {
            if (Windows)
                GPIO.pullUpDnControl(pin, pud);
        }

        public static void PwmWrite(int pin, int value)
        {
            if (Windows)
                GPIO.pwmWrite(pin, value);
        }

		public static void PwmSetMode(WiringPiPinMode mode)
        {
            if (Windows)
                GPIO.pwmSetMode(mode);
        }

        public static void PwmSetRange(uint range)
        {
            GPIO.pwmSetRange(range);
        }

        public static void PwmSetClock(int divisor)
        {
            if (Windows)
                GPIO.pwmSetClock(divisor);
        }

        public static void GpioClockSet(int pin, int freq)
        {
            if (Windows)
                GPIO.gpioClockSet(pin, freq);
        }

        //  Software PWM
        //
        public static int SoftPwmCreate(int pin, int value, int range)
        {
            if (Windows)
                return GPIO.softPwmCreate(pin, value, range);
            else
                return 1;
        }

        public static void SoftPwmWrite(int pin, int value)
        {
            if (Windows)
                GPIO.softPwmWrite(pin, value);
        }

        public static void SoftPwmStop(int pin)
        {
            if (Windows)
                GPIO.softPwmStop(pin);
        }



    }
}
