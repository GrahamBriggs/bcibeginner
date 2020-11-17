using LoggingInterface;
using OpenBCIInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainHatSharp
{
    class CommandStateMonitor
    {
        public event LogEventDelegate Log;
        public event BsCommandEventDelegate CommandStateChanged;


        public async void OnBlinkDetected(object sender, DetectedBlinkEventArgs e)
        {
            try
            {
                if (e.State != WinkState.Wink)
                    return;

                if (e.Eye == Eyes.Left)
                {
                    BlinksDetectedLeft.Enqueue(e.TimeStamp);
                }
                if (e.Eye == Eyes.Right)
                {
                    BlinksDetectedRight.Enqueue(e.TimeStamp);
                }

                var timeNow = DateTimeOffset.UtcNow.ToUnixTimeInDoubleSeconds();
               
                switch (CurrentMode)
                {
                    case BsCommand.None:
                    case BsCommand.Off:
                        {
                            var leftBlinks = BlinksDetectedLeft.Where(x => timeNow - x < 3.0);
                            var rightBlinks = BlinksDetectedRight.Where(x => timeNow - x < 3.0);

                            if (leftBlinks.Count() == 3 || rightBlinks.Count() == 3)
                            {
                                CurrentMode = BsCommand.On;
                                CommandStateChanged(this, new BsCommandEventArgs(BsCommand.On));
                                CleraBlinks();
                            }
                        }
                        break;

                    case BsCommand.On:
                        {
                            CommandModeOnProcessBlink(timeNow);
                        }                   
                        break;
                }

                while (BlinksDetectedLeft.Count > 0 && (timeNow - BlinksDetectedLeft.First()) > 3.0)
                    BlinksDetectedLeft.TryDequeue(out var discard);

                while (BlinksDetectedRight.Count > 0 && (timeNow - BlinksDetectedRight.First()) > 3.0)
                    BlinksDetectedRight.TryDequeue(out var discard);
            }
            catch (Exception ex )
            {
                Log?.Invoke(this, new LogEventArgs(this, "OnBlinkDetected", ex, LogLevel.ERROR));
            }

        }

        Task ProcessCommandModeBlinkTask;

        private void CommandModeOnProcessBlink(double timeNow)
        {
            if (ProcessCommandModeBlinkTask == null || ProcessCommandModeBlinkTask.IsCompleted)
            {
                ProcessCommandModeBlinkTask = ProcessCommandModeBlink(timeNow);
            }

        }

        private async Task ProcessCommandModeBlink(double timeNow)
        {
            var leftBlinks = BlinksDetectedLeft.Where(x => timeNow - x < 3.0);
            var rightBlinks = BlinksDetectedRight.Where(x => timeNow - x < 3.0);
            Log?.Invoke(this, new LogEventArgs(this, "ProcessCommandModeBlink", $">>>   Before delay {leftBlinks} {rightBlinks}.", LogLevel.INFO));
            if (leftBlinks.Count() > 1 || rightBlinks.Count() > 1)
            {
                await Task.Delay(1333);

                timeNow = DateTimeOffset.UtcNow.ToUnixTimeInDoubleSeconds();
                leftBlinks = BlinksDetectedLeft.Where(x => timeNow - x < 3.0);
                rightBlinks = BlinksDetectedRight.Where(x => timeNow - x < 3.0);
                Log?.Invoke(this, new LogEventArgs(this, "ProcessCommandModeBlink", $">>>   After delay {leftBlinks} {rightBlinks}.", LogLevel.INFO));

                if (leftBlinks.Count() == 3 || rightBlinks.Count() == 3)
                {
                    CurrentMode = BsCommand.Off;
                    CommandStateChanged(this, new BsCommandEventArgs(BsCommand.Off));
                    Log?.Invoke(this, new LogEventArgs(this, "ProcessCommandModeBlink", $">>>   Three Blinks Processed", LogLevel.INFO));
                    CleraBlinks();
                }
                else if (leftBlinks.Count() == 2 || rightBlinks.Count() == 2)
                {
                    CommandStateChanged(this, new BsCommandEventArgs(BsCommand.Trigger2));
                    Log?.Invoke(this, new LogEventArgs(this, "ProcessCommandModeBlink", $"--   Two Blinks Processed", LogLevel.INFO));
                    CleraBlinks();
                }
            }
        }



        private void CleraBlinks()
        {
            BlinksDetectedLeft.RemoveAll();
            BlinksDetectedRight.RemoveAll();
        }

        public CommandStateMonitor()
        {
            BlinksDetectedLeft = new ConcurrentQueue<double>();
            BlinksDetectedRight = new ConcurrentQueue<double>();
        }


        private ConcurrentQueue<double> BlinksDetectedLeft { get; set; }
        private ConcurrentQueue<double> BlinksDetectedRight { get; set; }

        private BsCommand CurrentMode { get; set; }

    }
}
