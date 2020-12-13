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

namespace BrainHatClient
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Form1(string hostName, string ipAddress, int boardId, int sampleRate)
        {
            InitializeComponent();

            HostName = hostName;
            BoardId = boardId;
            SampleRate = sampleRate;
            IpAddress = ipAddress;

            BlinkLeftCount = 0;
            BlinkRightCount = 0;

            //  create a file writer to record raw data
            FileWriter = new OpenBCIGuiFormatRawFileWriter();

            //  init UI begin state
            SetupFormUi();

            //  hook up the events to UI
            ConnectCurrentServerToUiEvents();
        }

        //  brainHat server info
        public string HostName { get; protected set; }
        int BoardId;
        int SampleRate;
        string IpAddress;

        //  File writer
        OpenBCIGuiFormatRawFileWriter FileWriter;


        /// <summary>
        /// Form closing event
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisconnectCurrentServerFromUiEvents();
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
            UpdateAccelerometerLabel(e);
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
                    label += $"            {string.Format("{0,9}", "Read mV")}    {string.Format("{0,9}", "Dev uV")}      {string.Format("{0,9}", "Noise uV")}      {string.Format("{0,9}", "Pwr 10Hz")}   {string.Format("{0,9}", "10/8")}      {string.Format("{0,9}", "10/12")}\n";
                    label += $"            {string.Format("{0,9}", "-------")}     {string.Format("{0,9}", "-------")}     {string.Format("{0,9}", "-------")}     {string.Format("{0,9}", "-------")}     {string.Format("{0,9}", "-------")}      {string.Format("{0,9}", "-------")}\n";

                    for (int i = 0; i < e.CurrentSample.NumberExgChannels; i++)
                    {
                        label += $"Channel {i:D2}: {string.Format("{0,9}", (e.CurrentSample.GetExgDataForChannel(i) / 1000.0).ToString("N3"))}     {string.Format("{0,9}", e.CurrentDeviation.GetExgDataForChannel(i).ToString("N3"))}     {string.Format("{0,9}", e.CurrentDevMedian.GetExgDataForChannel(i).ToString("N3"))}     {string.Format("{0,9}", e.CurrentBandPower10.GetExgDataForChannel(i).ToString("N3"))}     {string.Format("{0,9}", (e.CurrentBandPower10.GetExgDataForChannel(i) / e.CurrentBandPower08.GetExgDataForChannel(i)).ToString("N3"))}      {string.Format("{0,9}", (e.CurrentBandPower10.GetExgDataForChannel(i) / e.CurrentBandPower12.GetExgDataForChannel(i)).ToString("N3"))}\n";
                    }
                }

                labelExgData.Invoke(new Action(() => { labelExgData.Text = label; }));
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


        /// <summary>
        /// Connect a server to UI events
        /// </summary>
        private void ConnectCurrentServerToUiEvents()
        {
            MainForm.DataProcessors[HostName].NewSample += FileWriter.AddData;
            MainForm.DataProcessors[HostName].CurrentDataStateReported += OnHatDataProcessorCurrentState;

            MainForm.BlinkDetectors[HostName].DetectedBlink += OnBlinkDetected;

            MainForm.Logger.LoggedEvents += OnLoggedEvents;
        }


        /// <summary>
        /// Disconnect a server from the UI events
        /// </summary>
        private void DisconnectCurrentServerFromUiEvents()
        {
            MainForm.DataProcessors[HostName].NewSample -= FileWriter.AddData;
            MainForm.DataProcessors[HostName].CurrentDataStateReported -= OnHatDataProcessorCurrentState;

            MainForm.BlinkDetectors[HostName].DetectedBlink -= OnBlinkDetected;

            MainForm.Logger.LoggedEvents -= OnLoggedEvents;
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

            labelBlinkDetector.Invoke(new Action(() => { labelBlinkDetector.Text = $"Left: {BlinkLeftCount}\nRight: {BlinkRightCount}"; }));
        }



        /// <summary>
        /// Reset blink counter button
        /// </summary>
        private void buttonResetBlinkCounter_Click(object sender, EventArgs e)
        {
            BlinkLeftCount = 0;
            BlinkRightCount = 0;

            labelBlinkDetector.Invoke(new Action(() => { labelBlinkDetector.Text = $"Left: {BlinkLeftCount}\nRight: {BlinkRightCount}"; }));
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
                await FileWriter.StartWritingToFileAsync(textBoxRecordingName.Text, BoardId, SampleRate);
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



    }
}
