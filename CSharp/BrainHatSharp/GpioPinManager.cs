using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WiringPiWrapper.WiringPiProxy;
using WiringPiWrapper;

namespace BrainHatSharp
{
    public static class GpioPinManager
    {
        //  Define pins used

        //  Hard Hat
        static int PinRightRising = 37;
        static int PinRightFalling = 38;
        static int PinRightBlink = 36;

        static int PinLeftRising = 7;
        static int PinLeftFalling = 11;
        static int PinLeftBlink = 13;


        static int PinLightStringA5 = 35;
        static int PinLightStringA4 = 32;
        static int PinLightStringA3 = 31;
        static int PinLightStringA2 = 29;
        static int PinLightStringA1 = 33;

        static int PinLightStringB5 = 23;
        static int PinLightStringB4 = 21;
        static int PinLightStringB3 = 24;
        static int PinLightStringB2 = 22;
        static int PinLightStringB1 = 26;

        static int PinHapticMotor = 12;

        static int PinPowerSwitch = 40;



        //  Proto board
        //static int PinRightRising = 40;
        //static int PinRightFalling = 38;
        //static int PinRightBlink = 36;

        //static int PinLeftRising = 7;
        //static int PinLeftFalling = 11;
        //static int PinLeftBlink = 13;



        //static int PinLightStringA5 = 29;
        //static int PinLightStringA4 = 31;
        //static int PinLightStringA3 = 32;
        //static int PinLightStringA2 = 33;
        //static int PinLightStringA1 = 35;

        //static int PinLightStringB5 = 26;
        //static int PinLightStringB4 = 23;
        //static int PinLightStringB3 = 24;
        //static int PinLightStringB2 = 21;
        //static int PinLightStringB1 = 19;

        //static int PinHapticMotor = 16;

        //  List of all pins in use (used for all off)
        static List<int> PinsInUse;

        //  Light strings
        public static LightString LightStringMaster;
        static LightString LightStringLeft;
        static LightString LightStringRight;


        /// <summary>
        /// Setup wiringPi and initialize our pins for output
        /// </summary>
        public static async Task SetupGpio()
        {

            WiringPiSetupPhys();
        
            PinsInUse = new List<int>();
            PinsInUse.Add(PinLeftRising);
            PinsInUse.Add(PinLeftFalling);
            PinsInUse.Add(PinLeftBlink);

            PinsInUse.Add(PinRightRising);
            PinsInUse.Add(PinRightFalling);
            PinsInUse.Add(PinRightBlink);

            PinsInUse.Add(PinLightStringA1);
            PinsInUse.Add(PinLightStringA2);
            PinsInUse.Add(PinLightStringA3);
            PinsInUse.Add(PinLightStringA4);
            PinsInUse.Add(PinLightStringA5);
            
            PinsInUse.Add(PinLightStringB1);
            PinsInUse.Add(PinLightStringB2);
            PinsInUse.Add(PinLightStringB3);
            PinsInUse.Add(PinLightStringB4);
            PinsInUse.Add(PinLightStringB5);

            PinsInUse.Add(PinHapticMotor);
            PinsInUse.Add(PinPowerSwitch);

            foreach (var nextPin in PinsInUse)
                PinMode(nextPin, WiringPiPinMode.Output);

            AllOff();

            foreach (var nextPin in PinsInUse)
            {
                if (nextPin == PinPowerSwitch)
                    continue;

                DigitalWrite(nextPin, WiringPiPinValue.High);
                await Task.Delay(333);
                DigitalWrite(nextPin, WiringPiPinValue.Low);
            }

            LightStringRight = new LightString(new int[] { PinLightStringB1, PinLightStringB2, PinLightStringB3, PinLightStringB4, PinLightStringB5 });
            LightStringLeft = new LightString(new int[] { PinLightStringA1, PinLightStringA2, PinLightStringA3, PinLightStringA4, PinLightStringA5 }, LightStringRight);
            LightStringMaster = LightStringLeft;

            //  test lights
            await LightStringMaster.StartFlashAsync(111, 333, 3);
            await Task.Delay(3000);
            await LightStringMaster.StartSequenceAsync(111, 333, false);
            await Task.Delay(5000);
            await LightStringMaster.Stop();
            await Task.Delay(1000);
            await LightStringMaster.StartSequenceAsync(111, 333, true);
            await Task.Delay(5000);
            await LightStringMaster.Stop();
            await Task.Delay(1000);

            LightStringLeft.SetPins(new int[] { 1, 0, 1, 0, 1 });
            LightStringRight.SetPins(new int[] { 0, 1, 0, 1, 0 });
            await Task.Delay(2000);

            BuzzMotor(0, 2000, 1);
            await Task.Delay(1000);
            BuzzMotor(111, 222, 3);

            await LightStringMaster.StartSequenceAsync(111, 333, true);
        }



        public static void AllOff()
        {
            foreach (var nextPin in PinsInUse)
                DigitalWrite(nextPin, WiringPiPinValue.Low);
        }


        public static async void CommandMode(bool on)
        {
            if (on)
            {
                BuzzMotor(333, 111, 3);
                await LightStringMaster.StartFlashAsync(111, 333, 3);
            }
            else
            {
                BuzzMotor(333, 0, 1);
                await LightStringMaster.StartSequenceAsync(111, 333, true);
            }
        }


        public static async void BuzzMotor(int intervalBetween, int duration, int times)
        {
            for(int i = 0; i < times; i++)
            {
                if (i > 0)
                    await Task.Delay(intervalBetween);
                DigitalWrite(PinHapticMotor, WiringPiPinValue.High);
                await Task.Delay(duration);
                DigitalWrite(PinHapticMotor, WiringPiPinValue.Low);
            }
        }

        public static void CommandTrigger2()
        {

            BuzzMotor(333, 111, 2);
        }

        public static async void LightLeftRising()
        {
            DigitalWrite(PinLeftRising, WiringPiPinValue.High);
            await Task.Delay(333);
            DigitalWrite(PinLeftRising, WiringPiPinValue.Low);
        }

        public static async void LightLeftFalling()
        {
            DigitalWrite(PinLeftFalling, WiringPiPinValue.High);
            await Task.Delay(222);
            DigitalWrite(PinLeftFalling, WiringPiPinValue.Low);
        }

        public static async void LightLeftBlink()
        {
            DigitalWrite(PinLeftFalling, WiringPiPinValue.High);
            DigitalWrite(PinLeftBlink, WiringPiPinValue.High);
            await Task.Delay(333);
            DigitalWrite(PinLeftFalling, WiringPiPinValue.Low);
            DigitalWrite(PinLeftBlink, WiringPiPinValue.Low);
        }

        public static async void LightRightRising()
        {
            DigitalWrite(PinRightRising, WiringPiPinValue.High);
            await Task.Delay(333);
            DigitalWrite(PinRightRising, WiringPiPinValue.Low);
        }

        public static async void LightRightFalling()
        {
            DigitalWrite(PinRightFalling, WiringPiPinValue.High);
            await Task.Delay(222);
            DigitalWrite(PinRightFalling, WiringPiPinValue.Low);
        }

        public static async void LightRightBlink()
        {
            DigitalWrite(PinRightFalling, WiringPiPinValue.High);
            DigitalWrite(PinRightBlink, WiringPiPinValue.High);
            await Task.Delay(333);
            DigitalWrite(PinRightFalling, WiringPiPinValue.Low);
            DigitalWrite(PinRightBlink, WiringPiPinValue.Low);
        }

        public static void EnableHapticMotor(bool enable)
        {
            if (enable)
                DigitalWrite(PinHapticMotor, WiringPiPinValue.High);
            else
                DigitalWrite(PinHapticMotor, WiringPiPinValue.Low);

        }

        public static void BrainBoardPower(bool on)
        {
            DigitalWrite(PinPowerSwitch, on ? WiringPiPinValue.High : WiringPiPinValue.Low);
        }




    }
}
