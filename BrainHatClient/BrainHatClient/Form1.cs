using BrainflowDataProcessing;
using LoggingInterfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrainflowInterfaces;
using BrainHatServersMonitor;
using BrainHatNetwork;
using System.Collections.Concurrent;
using System.Media;

namespace BrainHatClient
{
    public partial class Form1 : Form
    {
        //  brainHat server info
        public HatServer Server { get; protected set; }

        BrainflowDataProcessor DataProcessor;
        BlinkDetector BlinkDetector;
        AlphaWaveDetector AlphaDetector;


        //  File writer
        OpenBCIGuiFormatRawFileWriter FileWriter;

        /// <summary>
        /// Constructor
        /// </summary>
        public Form1(HatServer server)
        {
            InitializeComponent();

            //  reference to the server we are connecting to
            Server = server;

            //  create the data processor
            DataProcessor = new BrainflowDataProcessor(server.HostName, server.BoardId, server.SampleRate);
            DataProcessor.Log += OnLog;
            server.RawDataReceived += DataProcessor.AddDataToProcessor;
          
            //  create the blink detector
            BlinkDetector = new BlinkDetector();
            BlinkDetector.Log += OnLog;
            DataProcessor.NewSample += BlinkDetector.OnNewSample;
            BlinkDetector.GetData = DataProcessor.GetRawData;
            BlinkDetector.GetStdDevMedians = DataProcessor.GetStdDevianMedians;
       
            //  create the alpha wave detector
            AlphaDetector = new AlphaWaveDetector();
            AlphaDetector.GetBandPower = DataProcessor.GetBandPower;
            AlphaDetector.Log += OnLog;
            AlphaDetector.DetectedBrainWave += OnAlphaDetectorDetectedBrainWave;

            //  create a file writer to record raw data
            FileWriter = new OpenBCIGuiFormatRawFileWriter();

            //  init the blink counter
            BlinkLeftCount = 0;
            BlinkRightCount = 0;

            //  init UI begin state
            SetupFormUi();

            //  start processes
            Start();
        }


        /// <summary>
        /// Start the data processors and begin reading data from the server
        /// </summary>
        private async void Start()
        {
            await Task.Run(async () =>
           {
               await DataProcessor.StartDataProcessorAsync();
               await AlphaDetector.StartDetectorAsync();
               await Server.StartReadingFromLslAsync();
           });

            ConnectToUiEvents();
        }


        /// <summary>
        /// Form closing
        /// Stop the data processors and stop the server data stream
        /// </summary>
        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            //  can not close when we are logging
            if ( FileWriter.IsLogging )
            {
                MessageBox.Show("You must end recording to the file before you can close this window.", "brainHat", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
                return;
            }

            DisconnectFromUiEvents();

            await DataProcessor.StopDataProcessorAsync();
            await AlphaDetector.StopDetectorAsync();
            await Server.StopReadingFromLslAsync();
            await StopBeep();

            base.OnFormClosing(e);
        }


        /// <summary>
        /// Connect a server to UI events
        /// </summary>
        private void ConnectToUiEvents()
        {
            DataProcessor.NewSample += FileWriter.AddData;
            DataProcessor.CurrentDataStateReported += OnHatDataProcessorCurrentState;
            BlinkDetector.DetectedBlink += OnBlinkDetected;

            MainForm.BrainHatServers.HatConnectionStatusUpdate += OnHatConnectionStatusUpdate;
            MainForm.Logger.LoggedEvents += OnLoggedEvents;
        }


        /// <summary>
        /// Disconnect a server from the UI events
        /// </summary>
        private void DisconnectFromUiEvents()
        {
            DataProcessor.NewSample -= FileWriter.AddData;
            DataProcessor.CurrentDataStateReported -= OnHatDataProcessorCurrentState;
            BlinkDetector.DetectedBlink -= OnBlinkDetected;

            MainForm.BrainHatServers.HatConnectionStatusUpdate += OnHatConnectionStatusUpdate;
            MainForm.Logger.LoggedEvents -= OnLoggedEvents;
        }


        /// <summary>
        /// Setup form UI for program startup
        /// </summary>
        private void SetupFormUi()
        {
            SetupLoggingUi();
            labelBlinkDetector.Text = "";
            labelRecordingDuration.Text = "";
            labelAcelData.Text = "No data.";
            labelExgData.Text = "No data.";
            labelOtherData.Text = "No data.";
            labelDataProcessing.Text = "No data.";

            labelBlinkDetector.Text = $"Left: {BlinkLeftCount}\nRight: {BlinkRightCount}";

            UpdateUiCancelToken = new CancellationTokenSource();
            UpdateUiTask = UpdateUiAsync(UpdateUiCancelToken.Token);
        }


        /// <summary>
        /// Update the UI when we get status updates from the processor
        /// </summary>
        private void OnHatDataProcessorCurrentState(object sender, BrainflowDataProcessing.ProcessorCurrentStateReportEventArgs e)
        {
            UpdateExgDataLabel(e);
            UpdateDataProcessingLabel(e);
            UpdateAccelerometerLabel(e);
            UpdateOtherPropertiesLabel(e);
        }


        /// <summary>
        /// Update the EXG data label
        /// </summary>
        private void UpdateExgDataLabel(ProcessorCurrentStateReportEventArgs e)
        {
            try
            {
                string label = "Not receiving data from the sensor.";
                if (e.ValidData)
                {
                    label = "";
                    label += $"Time stamp: {e.CurrentSample.TimeStamp.ToString("N6")}\n";
                    label += $"Observation time: {DateTimeOffset.FromUnixTimeMilliseconds((long)(e.CurrentSample.TimeStamp * 1000.0)).ToLocalTime().ToString("HH:mm:ss.fff")}\n\n";
                    label += $"     {string.Format("{0,14}", "Read mV")}{string.Format("{0,14}", "Dev uV")}{string.Format("{0,14}", "Noise uV")}\n";
                    label += $"     {string.Format("{0,14}", "-------")}{string.Format("{0,14}", "-------")}{string.Format("{0,14}", "-------")}\n";

                    for (int i = 0; i < e.CurrentSample.NumberExgChannels; i++)
                    {
                        label += $"Exg {i:D2}: {(e.CurrentSample.GetExgDataForChannel(i) / 1000.0),11:N3}{e.CurrentDeviation.GetExgDataForChannel(i),14:N3}{e.CurrentDevMedian.GetExgDataForChannel(i),14:N3}\n";
                    }
                }

                labelExgData.Invoke(new Action(() => { labelExgData.Text = label; }));
            }
            catch (Exception)
            { }
        }


        /// <summary>
        /// Update the data processing label
        /// </summary>
        private void UpdateDataProcessingLabel(ProcessorCurrentStateReportEventArgs e)
        {
            try
            {
                string label = "Not receiving data from the sensor.";
                if (e.ValidData)
                {
                    label = "";
                    label += $"Network offset time: {LatestOffsetTime.TotalSeconds:N4} s.\n";
                    label += $"Raw data latency: {LatestRawLatency:N4}\n";
                    label += "\n";
                    
                    label += $"Band Power{string.Format("{0,9}", "8Hz")}{string.Format("{0,9}", "10Hz")}{string.Format("{0,9}", "12Hz")}{string.Format("{0,9}", "18Hz")}{string.Format("{0,9}", "20Hz")}{string.Format("{0,9}", "22Hz")}\n";
                    label += $"             {string.Format("{0,9}", "-------")}{string.Format("{0,9}", "-------")}{string.Format("{0,9}", "-------")}{string.Format("{0,9}", "-------")}{string.Format("{0,9}", "-------")}{string.Format("{0,9}", "-------")}\n";

                    for (int i = 0; i < e.CurrentSample.NumberExgChannels; i++)
                    {
                        label += $"Channel {i:D2}:{Math.Log10(e.CurrentBandPower08.GetExgDataForChannel(i)),9:N3}{Math.Log10(e.CurrentBandPower10.GetExgDataForChannel(i)),9:N3}{Math.Log10(e.CurrentBandPower12.GetExgDataForChannel(i)),9:N3}{Math.Log10(e.CurrentBandPower18.GetExgDataForChannel(i)),9:N3}{Math.Log10(e.CurrentBandPower20.GetExgDataForChannel(i)),9:N3}{Math.Log10(e.CurrentBandPower22.GetExgDataForChannel(i)),9:N3}\n";
                    }
                }

                labelDataProcessing.Invoke(new Action(() => { labelDataProcessing.Text = label; }));
            }
            catch (Exception)
            { }
        }


        /// <summary>
        /// Update the other properties label
        /// </summary>
        private void UpdateOtherPropertiesLabel(ProcessorCurrentStateReportEventArgs e)
        {
            try
            {
                string label = "Not receiving data from the sensor.";
                if (e.ValidData)
                {
                    label = "";
                    for ( int i = 0; i < e.CurrentSample.NumberOtherChannels; i++)
                    {
                        label += $"Other {i:D2}:  {e.CurrentSample.GetOtherDataForChannel(i):N1}\n";
                    }

                    label += "\n";

                    for (int i = 0; i < e.CurrentSample.NumberAnalogChannels; i++)
                    {
                        label += $"Anlg {i:D2}:   {e.CurrentSample.GetAnalogDataForChannel(i):N1}\n";
                    }

                }

                labelOtherData.Invoke(new Action(() => { labelOtherData.Text = label; }));
            }
            catch (Exception)
            { }
        }


        /// <summary>
        /// Update the accelerometer label
        /// </summary>
        private void UpdateAccelerometerLabel(ProcessorCurrentStateReportEventArgs e)
        {
            try
            {
                string label = "Not receiving data from the sensor.";
                if (e.ValidData)
                {
                    label = "";
                    label += $"Acel 0: {e.CurrentSample.GetAccelDataForChannel(0).ToString("N6")}\n";
                    label += $"Acel 1: {e.CurrentSample.GetAccelDataForChannel(1).ToString("N6")}\n";
                    label += $"Acel 2: {e.CurrentSample.GetAccelDataForChannel(2).ToString("N6")}\n";
                }

                labelAcelData.Invoke(new Action(() => { labelAcelData.Text = label; }));
            }
            catch (Exception)
            { }
        }



        private TimeSpan LatestOffsetTime = TimeSpan.FromSeconds(-1);
        private double LatestRawLatency = -1.0;
        /// <summary>
        /// Connection status handler
        /// </summary>
        private void OnHatConnectionStatusUpdate(object sender, BrainHatStatusEventArgs e)
        {
            if ( e.Status.HostName == Server.HostName )
            {
                LatestOffsetTime = e.Status.OffsetTime;
                LatestRawLatency = e.Status.RawLatency;
            }
        }


        //  Blink Detection
        //
        int BlinkLeftCount = 0;
        int BlinkRightCount = 0;

        /// <summary>
        /// Blink detection event handler
        /// </summary>
        private void OnBlinkDetected(object sender, DetectedBlinkEventArgs e)
        {
            try
            {
                switch (e.State)
                {
                    case WinkState.Wink:
                        {
                            switch (e.Eye)
                            {
                                case Eyes.Left:
                                    BlinkLeftCount++;
                                    break;

                                case Eyes.Right:
                                    BlinkRightCount++;
                                    break;
                            }
                        }
                        break;
                }

                labelBlinkDetector.Invoke(new Action(() => { labelBlinkDetector.Text = $"Blink Left: {BlinkLeftCount}\nBlink Right: {BlinkRightCount}"; }));
            }
            catch (Exception)
            { }
        }



        /// <summary>
        /// Reset blink counter button
        /// </summary>
        private void buttonResetBlinkCounter_Click(object sender, EventArgs e)
        {
            BlinkLeftCount = 0;
            BlinkRightCount = 0;

            labelBlinkDetector.Text = $"Left: {BlinkLeftCount}\nRight: {BlinkRightCount}";
        }


        /// <summary>
        /// Alpha wave detector handling
        /// </summary>
        private async void OnAlphaDetectorDetectedBrainWave(object sender, DetectedBrainWaveEventArgs e)
        {
            try
            {
                switch (e.Type)
                {
                    case BrainWave.Alpha:
                        labelAlpha.Invoke(new Action(() => labelAlpha.Text = "Alpha wave detected."));
                        StartBeep();
                        break;

                    case BrainWave.None:
                        labelAlpha.Invoke(new Action(() => labelAlpha.Text = "Seeking alpha ..."));
                        await StopBeep();
                        break;
                }
            }
            catch (Exception)
            { }
        }


        CancellationTokenSource BeepCancel;
        Task BeepFunction;
        /// <summary>
        /// Start beep for alpha wave
        /// </summary>
        void StartBeep()
        {
            if ( BeepCancel == null )
            {
                BeepCancel = new CancellationTokenSource();
                BeepFunction = RunBeep(BeepCancel.Token);
            }
        }


        /// <summary>
        /// Stop beep for alpha 
        /// </summary>
        /// <returns></returns>
        async Task StopBeep()
        {
            if ( BeepCancel != null)
            {
                BeepCancel.Cancel();
                await BeepFunction;
                BeepCancel = null;
                BeepFunction = null;
            }
        }


        /// <summary>
        /// Run beep
        /// </summary>
        private async Task RunBeep(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    SystemSounds.Asterisk.Play();
                    await Task.Delay(1500);
                }
            }
            catch (OperationCanceledException)
            { }
            
        }


        //  Recording data to a file
        //
        DateTimeOffset? RecordingStartTime { get; set; }
        Task UpdateUiTask { get; set; }
        CancellationTokenSource UpdateUiCancelToken;


        /// <summary>
        /// Start / Stop recording button handler
        /// </summary>
        private async void buttonStartRecording_Click(object sender, EventArgs e)
        {
            buttonStartRecording.Enabled = false;

            if (FileWriter.IsLogging)
            {
                await FileWriter.StopWritingToFileAsync();
                buttonStartRecording.Text = "Start Recording";
                labelRecordingDuration.Text = "";
            }
            else
            {
                await FileWriter.StartWritingToFileAsync(textBoxRecordingName.Text, Server.BoardId, Server.SampleRate);
                buttonStartRecording.Text = "Stop Recording";
                RecordingStartTime = DateTimeOffset.UtcNow;
            }

            buttonStartRecording.Enabled = true;
        }


        /// <summary>
        /// Update the UI state for the recording timer display
        /// </summary>
        private async Task UpdateUiAsync(CancellationToken cancelToken)
        {
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    await Task.Delay(250, cancelToken);

                    if (FileWriter.IsLogging)
                        labelRecordingDuration.Text = $"Logging {(DateTimeOffset.UtcNow - RecordingStartTime).Value.TotalSeconds.ToString("N2")} seconds.";
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                MainForm.Logger.AddLog(this, new LogEventArgs(this, "UpdateFileLoggingUi", e, LogLevel.ERROR));
            }
        }


        private void OnLog(object sender, LogEventArgs e)
        {
            MainForm.Logger.AddLog(e);
        }
    }
}
