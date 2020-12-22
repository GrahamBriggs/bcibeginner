using BrainflowDataProcessing;
using BrainflowInterfaces;
using BrainHatNetwork;
using LoggingInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace brainHatLit
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            PlatformHelper.PlatformHelper.GetLibraryEnvironment();

            HostName = GpioPinManager.LoadFromConfigFile();
            GpioPinManager.SetupGpio();

            textBoxHostName.Text = HostName;
            ConnectToServer = false;
            ConnectedServer = null;

            EnableMotor = true;
            EnableLights = true;

            checkBoxHapticMotor.Checked = true;
            checkBoxLightsAuto.Checked = true;

            

            ServersMonitor = new HatServersMonitor();
            ServersMonitor.Log += OnLog;
            ServersMonitor.HatConnectionStatusUpdate += OnHatStatusUpdate;
            ServersMonitor.HatConnectionChanged += OnHatConnectionChanged;

            BlinkDetector = new BlinkDetector();
            BlinkDetector.DetectedBlink += OnDetectedBlink;

            SeekingAlpha = new AlphaWaveDetector();
            SeekingAlpha.DetectedBrainWave += OnDetectedBrainWave;


            _ = Task.Run(async () =>
            {
                //await GpioPinManager.TestConfig();

                await ServersMonitor.StartMonitorAsync();
                await SeekingAlpha.StartDetectorAsync();
                
                await StartLightFlash();
            });


        }

        private async void OnHatStatusUpdate(object sender, BrainHatStatusEventArgs e)
        {
            if (ConnectToServer && ! IsConnected)
            {
                if (!IsConnected && e.Status.HostName == HostName)
                {
                    if (StartMonitorForServer(e.Status))
                    {
                        buttonStart.Invoke(new Action(() => buttonStart.Enabled = true));
                        labelConnectionStatus.Invoke(new Action(() => labelConnectionStatus.Text = $"Connected to {HostName}."));
                        await StartLightSequence();
                    }
                }
            }
        }

        //  Servers Monitor
        HatServersMonitor ServersMonitor { get; set; }

        BrainflowDataProcessor DataProcessor { get; set; }
        BlinkDetector BlinkDetector { get; set; }
        AlphaWaveDetector SeekingAlpha { get; set; }

        bool ConnectToServer { get; set; }
        HatClient ConnectedServer { get; set; }
        bool IsConnected => ConnectedServer != null;

        string HostName { get; set; }

        bool EnableMotor { get; set; }
        bool EnableLights { get; set; }


        private async Task StartLightFlash()
        {
            if (EnableLights)
                await GpioPinManager.LightStringMaster.StartFlashAsync(333, 111, 3);
        }

        private async Task StartLightSequence()
        {
            if ( EnableLights )
                await GpioPinManager.LightStringMaster.StartSequenceAsync(222, 111, true);
        }

        async void OnHatConnectionChanged(object sender, HatConnectionEventArgs e)
        {
            if (IsConnected)
            {
                switch (e.State)
                {
                    case HatConnectionState.Lost:
                        {
                            if (e.HostName == HostName)
                            {
                                await StopMonitorForServer();
                                await StartLightFlash();
                            }
                        }
                        break;
                }
            }
        }

        private async void buttonStart_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = false;

            if ( IsConnected )
            {
                ConnectToServer = false;

                await StopMonitorForServer();

                buttonStart.Text = "Start";
                buttonStart.Enabled = true;
                labelConnectionStatus.Text = "Not connected.";
            }
            else
            {
                HostName = textBoxHostName.Text;
                ConnectToServer = true;
                buttonStart.Text = "Stop";
                labelConnectionStatus.Text = $"Connecting to {HostName} ...";
            }
        }


        private bool StartMonitorForServer(BrainHatServerStatus status)
        {
            ConnectedServer = ServersMonitor.GetServer(HostName);
            if (ConnectedServer != null)
            {
                DataProcessor = new BrainflowDataProcessor(HostName, status.BoardId, status.SampleRate);
                SetBandPowerCalculator();

                BlinkDetector.GetData = DataProcessor.GetRawData;
                BlinkDetector.GetStdDevMedians = DataProcessor.GetStdDevianMedians;
                ConnectedServer.RawDataReceived += DataProcessor.AddDataToProcessor;
                SeekingAlpha.GetBandPower = DataProcessor.GetBandPower;

                DataProcessor.NewSample += BlinkDetector.OnNewSample;

                _ = Task.Run(async () =>
                {
                    await DataProcessor.StartDataProcessorAsync();
                    await DataProcessor.StartBandPowerMonitorAsync();
                    await ConnectedServer.StartReadingFromLslAsync();
                });

                return true;
            }

            return false;
        }

        private async Task StopMonitorForServer()
        {
            if (ConnectedServer != null)
            {
                ConnectedServer.RawDataReceived -= DataProcessor.AddDataToProcessor;
                await ConnectedServer.StopReadingFromLslAsync();
            }
            ConnectedServer = null;
            
            if (DataProcessor != null)
            {
                DataProcessor.NewSample -= BlinkDetector.OnNewSample;
                await DataProcessor.StopDataProcessorAsync();
            }
            DataProcessor = null;
        }

        private void OnDetectedBlink(object sender, DetectedBlinkEventArgs e)
        {
            if (EnableLights)
            {
                switch (e.Eye)
                {
                    case Eyes.Left:
                        {
                            switch (e.State)
                            {
                                case WinkState.Rising:
                                    GpioPinManager.LightLeftRising();
                                    break;
                                case WinkState.Falling:
                                    GpioPinManager.LightLeftFalling();
                                    break;
                                case WinkState.Wink:
                                    GpioPinManager.LightLeftBlink();
                                    break;
                            }
                        }
                        break;

                    case Eyes.Right:
                        {
                            switch (e.State)
                            {
                                case WinkState.Rising:
                                    GpioPinManager.LightRightRising();
                                    break;
                                case WinkState.Falling:
                                    GpioPinManager.LightRightFalling();
                                    break;
                                case WinkState.Wink:
                                    GpioPinManager.LightRightBlink();
                                    break;
                            }
                        }
                        break;

                }
            }
        }


        private async void OnDetectedBrainWave(object sender, DetectedBrainWaveEventArgs e)
        {
            switch (e.Type)
            {
                case BrainWave.Alpha:
                    StartAlphaDetected();
                    break;

                case BrainWave.None:
                    await StopAlphaDetected();
                    break;
            }
        }

        CancellationTokenSource AlphaCancelTokenSource { get; set; }
        Task AlphaDetectedTask { get; set; }


        void StartAlphaDetected()
        {
            if (AlphaCancelTokenSource == null)
            {
                AlphaCancelTokenSource = new CancellationTokenSource();
                AlphaDetectedTask = RunAlphaDetectedTask(AlphaCancelTokenSource.Token);
            }
        }

        private async Task RunAlphaDetectedTask(CancellationToken cancelToken)
        {
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    if (EnableLights)
                        await GpioPinManager.LightStringMaster.SetLevel(Math.Min(timer.Elapsed.Seconds, 5));

                    if (EnableMotor && timer.Elapsed.Seconds > 0)
                    {
                        GpioPinManager.HapticMotorEnable(true);
                        if (timer.Elapsed.Seconds < 5)
                        {
                            await Task.Delay(100 * timer.Elapsed.Seconds, cancelToken);
                            GpioPinManager.HapticMotorEnable(false);
                            await Task.Delay(1000 - (100 * timer.Elapsed.Seconds), cancelToken);
                        }
                    }

                    await Task.Delay(1);
                }
            }
            catch (OperationCanceledException)
            { }
        }

        async Task StopAlphaDetected()
        {
            if ( AlphaCancelTokenSource != null )
            {
                AlphaCancelTokenSource.Cancel();
                await AlphaDetectedTask;
                GpioPinManager.HapticMotorEnable(false);
                AlphaCancelTokenSource = null;

                await StartLightSequence();
            }
        }

        void SetBandPowerCalculator()
        {
            //  create a list of tuples for your band power ranges
            var rangeList = new List<Tuple<double, double>>() {
                    new Tuple<double, double>(7.0,9.0),
                    new Tuple<double, double>(9.0,11.0),
                    new Tuple<double, double>(11.0,13.0),

                    new Tuple<double, double>(17.0,19.0),
                    new Tuple<double, double>(19.0,21.0),
                    new Tuple<double, double>(21.0,23.0),
                };

            DataProcessor.SetBandPowerRangeList(rangeList);
        }


        void OnLog(object sender, LogEventArgs e)
        {
            Console.WriteLine(e.FormatLogForConsole());
        }

        private void checkBoxHapticMotor_CheckedChanged(object sender, EventArgs e)
        {
            EnableMotor = checkBoxHapticMotor.Checked;
            if (!EnableMotor)
                GpioPinManager.HapticMotorEnable(false);
        }

        private async void checkBoxLightsAuto_CheckedChanged(object sender, EventArgs e)
        {
            EnableLights = checkBoxLightsAuto.Checked;
            if (EnableLights)
            {
                if (IsConnected)
                    await StartLightSequence();
                else
                    await StartLightFlash();
            }
            else
            {
                GpioPinManager.AllLightsOff();
            }
        }
    }
}
