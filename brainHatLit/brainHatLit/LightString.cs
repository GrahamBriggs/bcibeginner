using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static WiringPiWrapper.WiringPiProxy;
using WiringPiWrapper;

namespace brainHatLit
{
    public class LightString
    {
        public List<int> Pins { get; set; }

        public void SetupLightString(IEnumerable<int> pins, LightString slave = null)
        {
            Pins = new List<int>();
            Pins.AddRange(pins);
            SlaveString = slave;
        }


        public async Task StartFlashAsync(int intervalBetween, int durationFlash, int numberFlashes)
        {
            await Stop();

            if (intervalBetween < 1)
                intervalBetween = 1;
            if (durationFlash < 1)
                durationFlash = 1;

            IntervalBetween = TimeSpan.FromMilliseconds(intervalBetween);
            DurationFlash = TimeSpan.FromMilliseconds(durationFlash);
            NumberOfFlashes = numberFlashes;

            CancelTokenSource = new CancellationTokenSource();
            RunTask = RunFlashLightsAsync(CancelTokenSource.Token);
        }


        public async Task StartSequenceAsync(int intervalBetween, int durationFlash, bool biDirection)
        {
            await Stop();

            if (intervalBetween < 1)
                intervalBetween = 1;
            if (durationFlash < 1)
                durationFlash = 1;

            IntervalBetween = TimeSpan.FromMilliseconds(intervalBetween);
            DurationFlash = TimeSpan.FromMilliseconds(durationFlash);
            BiDirectionalSequence = biDirection;

            CancelTokenSource = new CancellationTokenSource();
            RunTask = RunLightsInSequenceAsync(CancelTokenSource.Token);
        }


        public async Task SetPins(IEnumerable<int> values)
        {
            await Stop();

            if (values.Count() != Pins.Count)
                return;

            int i = 0;
            foreach (var nextValue in values)
                DigitalWrite(Pins[i++], (WiringPiWrapper.WiringPiPinValue)nextValue);

        }

        public async Task SetLevel(int level)
        {
            await Stop();

            if (level > Pins.Count)
                return;

            for (int i = 0; i < Pins.Count; i++)
            {
                DigitalWrite(Pins[i], i > level ? WiringPiPinValue.Low : WiringPiPinValue.High);
                if (SlaveString != null)
                    DigitalWrite(SlaveString.Pins[i], i > level ? WiringPiPinValue.Low : WiringPiPinValue.High);
            }
        }


        public async Task Stop()
        {
            if (CancelTokenSource != null)
            {
                CancelTokenSource.Cancel();
                if (RunTask != null)
                    await RunTask;

                CancelTokenSource = null;
                RunTask = null;
            }
        }

        private void AllOff()
        {
            foreach (var nextPin in Pins)
                DigitalWrite(nextPin, WiringPiPinValue.Low);
            if (SlaveString != null)
            {
                foreach (var nextPin in SlaveString.Pins)
                    DigitalWrite(nextPin, WiringPiPinValue.Low);
            }
        }

        private void AllOn()
        {
            foreach (var nextPin in Pins)
                DigitalWrite(nextPin, WiringPiPinValue.High);
            if (SlaveString != null)
            {
                foreach (var nextPin in SlaveString.Pins)
                    DigitalWrite(nextPin, WiringPiPinValue.High);
            }
        }

        public LightString()
        {

        }

        public LightString(IEnumerable<int> pins, LightString slave = null)
        {
            SetupLightString(pins, slave);
        }

        private TimeSpan IntervalBetween { get; set; }
        private TimeSpan DurationFlash { get; set; }
        private int NumberOfFlashes { get; set; }
        private bool BiDirectionalSequence { get; set; }
        LightString SlaveString { get; set; }


        CancellationTokenSource CancelTokenSource;
        Task RunTask;


        private async Task RunFlashLightsAsync(CancellationToken cancelToken)
        {
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    for (int j = 0; j < NumberOfFlashes; j++)
                    {
                        if (j > 0)
                            await Task.Delay(DurationFlash, cancelToken);

                        AllOn();

                        await Task.Delay(DurationFlash, cancelToken);

                        AllOff();
                    }

                    await Task.Delay(IntervalBetween, cancelToken);
                }
            }
            catch (OperationCanceledException)
            {

            }
        }

        private async Task RunLightsInSequenceAsync(CancellationToken cancelToken)
        {
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    for (int i = 0; i < Pins.Count; i++)
                    {
                        DigitalWrite(Pins[i], WiringPiPinValue.High);
                        if (SlaveString != null)
                            DigitalWrite(SlaveString.Pins[i], WiringPiPinValue.High);

                        await Task.Delay(DurationFlash, cancelToken);

                        DigitalWrite(Pins[i], WiringPiPinValue.Low);
                        if (SlaveString != null)
                            DigitalWrite(SlaveString.Pins[i], WiringPiPinValue.Low);

                        await Task.Delay(IntervalBetween, cancelToken);
                    }
                    if (BiDirectionalSequence)
                    {
                        for (int i = Pins.Count - 2; i >= 1; i--)
                        {
                            DigitalWrite(Pins[i], WiringPiPinValue.High);
                            if (SlaveString != null)
                                DigitalWrite(SlaveString.Pins[i], WiringPiPinValue.High);

                            await Task.Delay(DurationFlash, cancelToken);

                            DigitalWrite(Pins[i], WiringPiPinValue.Low);
                            if (SlaveString != null)
                                DigitalWrite(SlaveString.Pins[i], WiringPiPinValue.Low);

                            await Task.Delay(IntervalBetween, cancelToken);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }
        }



    }
}
