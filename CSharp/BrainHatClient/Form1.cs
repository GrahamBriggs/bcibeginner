using BrainflowDataProcessing;
using LoggingInterface;
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
using OpenBCIInterfaces;
using System.Windows.Threading;

namespace BrainHatClient
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Form1()
        {

            HatConnection.MonoVersion = true;

            //  create the program logging object
            Logger = new Logging();
            Logger.LoggedEvents += OnLoggedEvents;

            InitializeComponent();

            //  start logging thread
            StartLogging();

            //  init UI begin state
            SetupFormUi();


            // create the hat monitor
            BrainHatServer = new HatServerMonitor();
            // hook up hat monitor events
            BrainHatServer.Log += OnLog;
            BrainHatServer.HatConnectionStatusChanged += OnHatConnectionStatusChanged;
            BrainHatServer.RawDataReceived += OnRawDataReceived;
            BrainHatServer.HatConnectionChanged += OnHatConnectionChanged;

            DataProcessor = new BrainflowDataProcessor();
            DataProcessor.Log += OnLog;
            DataProcessor.CurrentDataStateReported += OnHatDataProcessorCurrentState;
            DataProcessor.SetBoard(0);  //  using only Cyton 8 for now

            BlinkDetection = new BlinkDetector();
            BlinkDetection.Log += OnLog;
            BlinkDetection.DetectedBlink += OnBlinkDetected;
            DataProcessor.NewReading += BlinkDetection.OnNewReading;
            BlinkDetection.GetData = DataProcessor.GetRawData;
            BlinkDetection.GetStdDevMedians = DataProcessor.GetStdDevianMedians;


            //  start the hat monitor not on the UI thread
            _ = Task.Run(async () =>
            {
                await BrainHatServer.StartMonitorAsync();
                await DataProcessor.StartDataProcessorAsync();
            }) ;

            //  create a file writer to record raw data
            FileWriter = new OpenBCIGuiRawFileWriter();
            BrainHatServer.RawDataReceived += FileWriter.AddData;
        }

      



        private void OnRawDataReceived(object sender, HatRawDataReceivedEventArgs e)
        {
            DataProcessor.AddDataToProcessor(e.Data);
        }


        //  Object to monitor receive and process data coming from the hat
        HatServerMonitor BrainHatServer;
        //  Brainflow Data Processing
        BrainflowDataProcessor DataProcessor { get; set; }
        BlinkDetector BlinkDetection { get; set; }

        //  File writer
        OpenBCIGuiRawFileWriter FileWriter;

        //  Connection flags

        private DateTimeOffset ConnectionStatusLastUpdateTime;

        /// <summary>
        /// Form closing event
        /// wait for the update UI task to exit
        /// </summary>
        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //// TODO - this is hacky way to wait for cleanup events on form closing, is there a proper way
            //e.Cancel = true;

            //await TheHat.StopMonitorAsync();
            //UpdateUiCancelToken.Cancel();
            //await UpdateUiTask;
            //await Logger.StopLoggingAsync();

            //this.FormClosing -= Form1_FormClosing;
            //Close();
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
            labelConnectionStatus.Text = "Not connected.";

            UpdateUiCancelToken = new CancellationTokenSource();
            UpdateUiTask = UpdateUiAsync(UpdateUiCancelToken.Token);
            ConnectionStatusLastUpdateTime = DateTimeOffset.UtcNow;

            comboBoxConnectedDevice.Items.Add(DisconnectedString);
            comboBoxConnectedDevice.SelectedItem = DisconnectedString;

            comboBoxConnectedDevice.SelectedIndexChanged += comboBoxConnectedDevice_SelectedIndexChanged;
        }

        string DisconnectedString = "- Disconnected -";


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
                if (BrainHatServer.IsConnected && e.ValidData)
                {
                    label = "";
                    label += $"            {string.Format("{0,9}", "Read mV")}    {string.Format("{0,9}", "Dev uV")}      {string.Format("{0,9}", "Noise uV")}      {string.Format("{0,9}", "Pwr 10Hz")}   {string.Format("{0,9}", "10/8")}      {string.Format("{0,9}", "10/12")}\n";
                    label += $"            {string.Format("{0,9}", "-------")}     {string.Format("{0,9}", "-------")}     {string.Format("{0,9}", "-------")}     {string.Format("{0,9}", "-------")}     {string.Format("{0,9}", "-------")}      {string.Format("{0,9}", "-------")}\n";
                    label += $"Channel 0: {string.Format("{0,9}", (e.CurrentReading.ExgCh0 / 1000.0).ToString("N3"))}     {string.Format("{0,9}", e.CurrentDeviation.ExgCh0.ToString("N3"))}     {string.Format("{0,9}", e.CurrentDevMedian.ExgCh0.ToString("N3"))}     {string.Format("{0,9}", e.CurrentBandPower10.ExgCh0.ToString("N3"))}     {string.Format("{0,9}", (e.CurrentBandPower10.ExgCh0 / e.CurrentBandPower08.ExgCh0).ToString("N3"))}      {string.Format("{0,9}", (e.CurrentBandPower10.ExgCh0 / e.CurrentBandPower12.ExgCh0).ToString("N3"))}\n";
                    label += $"Channel 1: {string.Format("{0,9}", (e.CurrentReading.ExgCh1 / 1000.0).ToString("N3"))}     {string.Format("{0,9}", e.CurrentDeviation.ExgCh1.ToString("N3"))}     {string.Format("{0,9}", e.CurrentDevMedian.ExgCh1.ToString("N3"))}     {string.Format("{0,9}", e.CurrentBandPower10.ExgCh1.ToString("N3"))}     {string.Format("{0,9}", (e.CurrentBandPower10.ExgCh1 / e.CurrentBandPower08.ExgCh1).ToString("N3"))}      {string.Format("{0,9}", (e.CurrentBandPower10.ExgCh1 / e.CurrentBandPower12.ExgCh1).ToString("N3"))}\n";
                    label += $"Channel 2: {string.Format("{0,9}", (e.CurrentReading.ExgCh2 / 1000.0).ToString("N3"))}     {string.Format("{0,9}", e.CurrentDeviation.ExgCh2.ToString("N3"))}     {string.Format("{0,9}", e.CurrentDevMedian.ExgCh2.ToString("N3"))}     {string.Format("{0,9}", e.CurrentBandPower10.ExgCh2.ToString("N3"))}     {string.Format("{0,9}", (e.CurrentBandPower10.ExgCh2 / e.CurrentBandPower08.ExgCh2).ToString("N3"))}      {string.Format("{0,9}", (e.CurrentBandPower10.ExgCh2 / e.CurrentBandPower12.ExgCh2).ToString("N3"))}\n";
                    label += $"Channel 3: {string.Format("{0,9}", (e.CurrentReading.ExgCh3 / 1000.0).ToString("N3"))}     {string.Format("{0,9}", e.CurrentDeviation.ExgCh3.ToString("N3"))}     {string.Format("{0,9}", e.CurrentDevMedian.ExgCh3.ToString("N3"))}     {string.Format("{0,9}", e.CurrentBandPower10.ExgCh3.ToString("N3"))}     {string.Format("{0,9}", (e.CurrentBandPower10.ExgCh3 / e.CurrentBandPower08.ExgCh3).ToString("N3"))}      {string.Format("{0,9}", (e.CurrentBandPower10.ExgCh3 / e.CurrentBandPower12.ExgCh3).ToString("N3"))}\n";
                    label += $"Channel 4: {string.Format("{0,9}", (e.CurrentReading.ExgCh4 / 1000.0).ToString("N3"))}     {string.Format("{0,9}", e.CurrentDeviation.ExgCh4.ToString("N3"))}     {string.Format("{0,9}", e.CurrentDevMedian.ExgCh4.ToString("N3"))}     {string.Format("{0,9}", e.CurrentBandPower10.ExgCh4.ToString("N3"))}     {string.Format("{0,9}", (e.CurrentBandPower10.ExgCh4 / e.CurrentBandPower08.ExgCh4).ToString("N3"))}      {string.Format("{0,9}", (e.CurrentBandPower10.ExgCh4 / e.CurrentBandPower12.ExgCh4).ToString("N3"))}\n";
                    label += $"Channel 5: {string.Format("{0,9}", (e.CurrentReading.ExgCh5 / 1000.0).ToString("N3"))}     {string.Format("{0,9}", e.CurrentDeviation.ExgCh5.ToString("N3"))}     {string.Format("{0,9}", e.CurrentDevMedian.ExgCh5.ToString("N3"))}     {string.Format("{0,9}", e.CurrentBandPower10.ExgCh5.ToString("N3"))}     {string.Format("{0,9}", (e.CurrentBandPower10.ExgCh5 / e.CurrentBandPower08.ExgCh5).ToString("N3"))}      {string.Format("{0,9}", (e.CurrentBandPower10.ExgCh5 / e.CurrentBandPower12.ExgCh5).ToString("N3"))}\n";
                    label += $"Channel 6: {string.Format("{0,9}", (e.CurrentReading.ExgCh6 / 1000.0).ToString("N3"))}     {string.Format("{0,9}", e.CurrentDeviation.ExgCh6.ToString("N3"))}     {string.Format("{0,9}", e.CurrentDevMedian.ExgCh6.ToString("N3"))}     {string.Format("{0,9}", e.CurrentBandPower10.ExgCh6.ToString("N3"))}     {string.Format("{0,9}", (e.CurrentBandPower10.ExgCh6 / e.CurrentBandPower08.ExgCh6).ToString("N3"))}      {string.Format("{0,9}", (e.CurrentBandPower10.ExgCh6 / e.CurrentBandPower12.ExgCh6).ToString("N3"))}\n";
                    label += $"Channel 7: {string.Format("{0,9}", (e.CurrentReading.ExgCh7 / 1000.0).ToString("N3"))}     {string.Format("{0,9}", e.CurrentDeviation.ExgCh7.ToString("N3"))}     {string.Format("{0,9}", e.CurrentDevMedian.ExgCh7.ToString("N3"))}     {string.Format("{0,9}", e.CurrentBandPower10.ExgCh7.ToString("N3"))}     {string.Format("{0,9}", (e.CurrentBandPower10.ExgCh7 / e.CurrentBandPower08.ExgCh7).ToString("N3"))}      {string.Format("{0,9}", (e.CurrentBandPower10.ExgCh7 / e.CurrentBandPower12.ExgCh7).ToString("N3"))}\n";
                    label += $"\n\nTime stamp: {e.CurrentReading.TimeStamp.ToString("N6")}\n";
                    label += $"Observation time: {DateTimeOffset.FromUnixTimeMilliseconds((long)(e.CurrentReading.TimeStamp * 1000.0)).ToLocalTime().ToString("HH:mm:ss.fff")}";
                }

                labelExgData.Invoke(new Action(() => { labelExgData.Text = label; }));
            }
            catch (Exception ex)
            {
                Logger.AddLog(new LogEventArgs(this, "UpdateExgDataLabel", ex, LogLevel.ERROR));
            }
        }


        /// <summary>
        /// Update the accelerometer label
        /// </summary>
        private void UpdateAccelerometerLabel(ProcessorCurrentStateReportEventArgs e)
        {
            try
            {
                string label = "Not receiving data from the sensor.";
                if (BrainHatServer.IsConnected && e.ValidData)
                {
                    label = "";
                    label += $"Acel 0: {e.CurrentReading.AcelCh0.ToString("N6")}\n";
                    label += $"Acel 1: {e.CurrentReading.AcelCh1.ToString("N6")}\n";
                    label += $"Acel 2: {e.CurrentReading.AcelCh2.ToString("N6")}\n";
                }

                labelAcelData.Invoke(new Action(() => { labelAcelData.Text = label; }));
            }
            catch (Exception)
            {
                Logger.AddLog(new LogEventArgs(this, "OnTheHatDataProcessorCurrentState", e, LogLevel.ERROR));
            }
        }



        private async void OnHatConnectionChanged(object sender, HatConnectionEventArgs e)
        {
            Logger.AddLog(new LogEventArgs(this, "OnHatConnectionChanged", $"Hat connection changed {e.HostName} {e.State}.", LogLevel.INFO));


            switch ( e.State )
            {
                case HatConnectionState.Discovered:
                    comboBoxConnectedDevice.Invoke(new Action(() => {
                        comboBoxConnectedDevice.SelectedIndexChanged -= comboBoxConnectedDevice_SelectedIndexChanged; 
                        comboBoxConnectedDevice.Items.Add(e.HostName);
                        comboBoxConnectedDevice.SelectedIndexChanged += comboBoxConnectedDevice_SelectedIndexChanged;
                    } ));
                    break;

                case HatConnectionState.Lost:
                    
                    comboBoxConnectedDevice.Invoke(new Action(() => {
                        if (comboBoxConnectedDevice.Items.Contains(e.HostName))
                        {
                            comboBoxConnectedDevice.SelectedIndexChanged -= comboBoxConnectedDevice_SelectedIndexChanged;
                            comboBoxConnectedDevice.Items.Remove(e.HostName); 
                            comboBoxConnectedDevice.SelectedIndexChanged += comboBoxConnectedDevice_SelectedIndexChanged;
                        }
                    }));
                    break;

                case HatConnectionState.Connected:
                    LogLevel syncLogLevel = LogLevel.OFF;

                    comboBoxConnectedDevice.Invoke(new Action(() => {
                        comboBoxConnectedDevice.SelectedIndexChanged -= comboBoxConnectedDevice_SelectedIndexChanged;
                        comboBoxConnectedDevice.SelectedItem = e.HostName;
                        comboBoxConnectedDevice.SelectedIndexChanged += comboBoxConnectedDevice_SelectedIndexChanged;

                        //  sync the log level
                        syncLogLevel = (LogLevel)comboBoxLogLevelRemote.SelectedItem;
                    }));


                    Logger.AddLog(new LogEventArgs(this, "OnCheckDataStreamConnection", $"Receiving new data stream from hat. Setting remote log level {syncLogLevel}", LogLevel.DEBUG));

                    await SetRemoteLogLevel(syncLogLevel);

                    break;

                case HatConnectionState.Disconnected:
                    comboBoxConnectedDevice.Invoke(new Action(() =>
                    {
                        comboBoxConnectedDevice.SelectedIndexChanged -= comboBoxConnectedDevice_SelectedIndexChanged;
                        comboBoxConnectedDevice.SelectedItem = DisconnectedString;
                        comboBoxConnectedDevice.SelectedIndexChanged += comboBoxConnectedDevice_SelectedIndexChanged;
                    }));
                    break;
            }
        }


      

        /// <summary>
        /// Process hat connection status update
        /// </summary>
        private void OnHatConnectionStatusChanged(object sender, HatConnectionStatusEventArgs e)
        {
            string statusString = $"host: {e.Connection.HostName}\n";
            statusString += $"eth0:  {e.Connection.Eth0Connection}\n";
            statusString += $"wlan0: {e.Connection.WLan0Connection}\n";
            statusString += $"ping speed: {e.PingSpeed.TotalMilliseconds.ToString("N3")} ms.";
            ConnectionStatusLastUpdateTime = DateTimeOffset.UtcNow;

            labelConnectionStatus.Invoke(new Action(() => { labelConnectionStatus.Text = statusString;  }));
        }


        private async void comboBoxConnectedDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (BrainHatServer.IsConnected)
            {
                var selection = comboBoxConnectedDevice.SelectedItem.ToString();
                if (selection == DisconnectedString)
                {
                    await DataProcessor.StopDataProcessorAsync();
                    BrainHatServer.ChangeConnectedServer(null);
                    labelExgData.Text = "Not receiving data from the sensor.";
                    labelAcelData.Text = "Not receiving data from the sensor.";
                }
                else if (selection != BrainHatServer.ConnectedServer.HostName)
                {
                    await DataProcessor.StopDataProcessorAsync();
                    labelExgData.Text = "Not receiving data from the sensor.";
                    labelAcelData.Text = "Not receiving data from the sensor.";

                    BrainHatServer.ChangeConnectedServer(comboBoxConnectedDevice.SelectedItem.ToString());

                    //  start the hat monitor not on the UI thread
                    _ = Task.Run(async () =>
                    {
                        await DataProcessor.StartDataProcessorAsync();
                    });
                }
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

            UpdateBlinkUi();
        }


        /// <summary>
        /// Update the blink counter UI
        /// </summary>
        private void UpdateBlinkUi()
        {
            labelBlinkDetector.Invoke(new Action(() => { labelBlinkDetector.Text = $"Left: {BlinkLeftCount}\nRight: {BlinkRightCount}"; }));
        }


        /// <summary>
        /// Reset blink counter button
        /// </summary>
        private void buttonResetBlinkCounter_Click(object sender, EventArgs e)
        {
            BlinkLeftCount = 0;
            BlinkRightCount = 0;

            UpdateBlinkUi();
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
                await FileWriter.StartWritingToFileAsync(textBoxRecordingName.Text);
                buttonStartRecording.Text = "Stop Recording";
                RecordingStartTime = DateTimeOffset.UtcNow;
            }

            buttonStartRecording.Enabled = true;
        }


        /// <summary>
        /// Update the UI state when recording or to detect disconnected status
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

                    if ((DateTimeOffset.UtcNow - ConnectionStatusLastUpdateTime).TotalSeconds > 10.0)
                        labelConnectionStatus.Text = "Not connected.";
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Logger.AddLog(this, new LogEventArgs(this, "UpdateFileLoggingUi", e, LogLevel.ERROR));
            }
        }



        // Logging
        //

        /// <summary>
        /// Kick off the logging task
        /// </summary>
        private async void StartLogging()
        {
            await Logger.StartLogging();
        }


        /// <summary>
        /// Component log handler
        /// </summary>
        private void OnLog(object sender, LogEventArgs e)
        {
            Logger.AddLog(e);
        }



    }
}
