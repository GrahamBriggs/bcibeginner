using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WiringPiWrapper.WiringPiProxy;
using WiringPiWrapper;
using System.Xml.Linq;

namespace brainHatLit
{
    public static class GpioPinManager
    {
        //  Define pins used
        static int PinRightRising = 0;
        static int PinRightFalling = 0;
        static int PinRightBlink = 0;

        static int PinLeftRising = 0;
        static int PinLeftFalling = 0;
        static int PinLeftBlink = 0;

        static int PinLightStringA5 = 0;
        static int PinLightStringA4 = 0;
        static int PinLightStringA3 = 0;
        static int PinLightStringA2 = 0;
        static int PinLightStringA1 = 0;

        static int PinLightStringB5 = 0;
        static int PinLightStringB4 = 0;
        static int PinLightStringB3 = 0;
        static int PinLightStringB2 = 0;
        static int PinLightStringB1 = 0;

        static int PinHapticMotor = 0;



        /// <summary>
        /// Setup wiringPi and initialize our pins for output
        /// </summary>
        public static void SetupGpio()
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

            foreach (var nextPin in PinsInUse)
                PinMode(nextPin, WiringPiPinMode.Output);

            LightStringRight = new LightString(new int[] { PinLightStringB1, PinLightStringB2, PinLightStringB3, PinLightStringB4, PinLightStringB5 });
            LightStringLeft = new LightString(new int[] { PinLightStringA1, PinLightStringA2, PinLightStringA3, PinLightStringA4, PinLightStringA5 }, LightStringRight);
            LightStringMaster = LightStringLeft;

            AllOff();

        }

        public static async Task TestConfig()
        {
            foreach (var nextPin in PinsInUse)
            {
                DigitalWrite(nextPin, WiringPiPinValue.High);
                await Task.Delay(333);
                DigitalWrite(nextPin, WiringPiPinValue.Low);
            }

          

            //  test lights
            await LightStringMaster.StartFlashAsync(333, 111, 3);
            await Task.Delay(5000);
            await LightStringMaster.StartSequenceAsync(333, 111, false);
            await Task.Delay(5000);
            await LightStringMaster.Stop();
            await Task.Delay(1000);
            await LightStringMaster.StartSequenceAsync(333, 111, true);
            await Task.Delay(5000);
            await LightStringMaster.Stop();
            await Task.Delay(1000);

            await LightStringLeft.SetPins(new int[] { 1, 0, 1, 0, 1 });
            await LightStringRight.SetPins(new int[] { 0, 1, 0, 1, 0 });
            await Task.Delay(2000);

            //  kick off default sequence for scanning state
            await LightStringMaster.StartSequenceAsync(333, 111, true);
        }



        public static string LoadFromConfigFile()
        {
            Console.WriteLine("Loading from config file");

            string hostName = "";
            try
            {
                XDocument xdoc = XDocument.Load("./config.xml");

                hostName = xdoc.Element("Config")?.Element("HostName")?.Value;
                if (hostName == null)
                    hostName = "";


                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinLeftRising").Value, out PinLeftRising);
                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinLeftFalling").Value, out PinLeftFalling);
                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinLeftBlink").Value, out PinLeftBlink);

                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinRightRising").Value, out PinRightRising);
                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinRightFalling").Value, out PinRightFalling);
                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinRightBlink").Value, out PinRightBlink);

                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinLightStringA5").Value, out PinLightStringA5);
                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinLightStringA4").Value, out PinLightStringA4);
                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinLightStringA3").Value, out PinLightStringA3);
                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinLightStringA2").Value, out PinLightStringA2);
                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinLightStringA1").Value, out PinLightStringA1);

                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinLightStringB5").Value, out PinLightStringB5);
                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinLightStringB4").Value, out PinLightStringB4);
                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinLightStringB3").Value, out PinLightStringB3);
                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinLightStringB2").Value, out PinLightStringB2);
                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinLightStringB1").Value, out PinLightStringB1);

                int.TryParse(xdoc.Element("Config")?.Element("Pins")?.Element("PinHapticMotor").Value, out PinHapticMotor);
            }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }

            return hostName;
        }



        public static void AllOff()
        {
            foreach (var nextPin in PinsInUse)
                DigitalWrite(nextPin, WiringPiPinValue.Low);
        }

        public static void AllLightsOff()
        {
            foreach (var nextPin in PinsInUse)
            {
                if (nextPin == PinHapticMotor)
                    continue;

                DigitalWrite(nextPin, WiringPiPinValue.Low);
            }
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
            await Task.Delay(333);
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
            await Task.Delay(333);
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

        public static void HapticMotorEnable(bool enable)
        {
            if (enable)
                DigitalWrite(PinHapticMotor, WiringPiPinValue.High);
            else if (!enable)
                DigitalWrite(PinHapticMotor, WiringPiPinValue.Low);
        }


        static List<int> PinsInUse;
        public static LightString LightStringMaster;
        static LightString LightStringLeft;
        static LightString LightStringRight;
    }
}
