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


        public void OnBlinkDetected(object sender, DetectedBlinkEventArgs e)
        {
            try
            {
                if (e.State == WinkState.Wink && e.Eye == Eyes.Left)
                {
                    BlinksDetectedLeft.Enqueue(e.TimeStamp);
                }
                if (e.State == WinkState.Wink && e.Eye == Eyes.Right)
                {
                    BlinksDetectedRight.Enqueue(e.TimeStamp);
                }

                var timeNow = DateTimeOffset.UtcNow.ToUnixTimeInDoubleSeconds();
                var leftBlinks = BlinksDetectedLeft.Where(x => timeNow - x < 3.0);
                var rightBlinks = BlinksDetectedRight.Where(x => timeNow - x < 3.0);

                switch (CurrentMode)
                {
                    case BsCommand.None:
                    case BsCommand.Off:
                        if (leftBlinks.Count() == 3 || rightBlinks.Count() == 3)
                        {
                            CurrentMode = BsCommand.On;
                            CommandStateChanged(this, new BsCommandEventArgs(BsCommand.On));
                            CleraBlinks();
                        }
                        break;

                    case BsCommand.On:
                        if (leftBlinks.Count() == 2 || rightBlinks.Count() == 2)
                        {
                            CommandStateChanged(this, new BsCommandEventArgs(BsCommand.Trigger2));
                            CleraBlinks();
                        }
                        else if (leftBlinks.Count() == 3 || rightBlinks.Count() == 3)
                        {
                            CurrentMode = BsCommand.Off;
                            CommandStateChanged(this, new BsCommandEventArgs(BsCommand.Off));
                            CleraBlinks();
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
