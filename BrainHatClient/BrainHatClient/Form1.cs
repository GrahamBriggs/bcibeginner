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
        public Form1()
        {

            BrainHatNetworkAddresses.Channel1 = false;

            //  create the program logging object
            Logger = new Logging();
            Logger.LoggedEvents += OnLoggedEvents;

            InitializeComponent();

            //  start logging thread
            StartLogging();

            //  init UI begin state
            SetupFormUi();


            // create the hat monitor
            BrainHatServers = new HatServersMonitor();
            // hook up hat monitor events
            BrainHatServers.Log += OnLog;
            BrainHatServers.HatStatusUpdate += OnHatStatusUpdate;

            BrainHatServers.HatConnectionChanged += OnHatConnectionChanged;

            DataProcessors = new ConcurrentDictionary<string, BrainflowDataProcessor>();

            

            BlinkDetectors = new ConcurrentDictionary<string, BlinkDetector>();

            _ = Task.Run(async () =>
            {
                await BrainHatServers.StartMonitorAsync();
            });

            //  create a file writer to record raw data
            FileWriter = new OpenBCIGuiFormatRawFileWriter();

        }





       


        //  Object to monitor receive and process data coming from the hat
        HatServersMonitor BrainHatServers;
        //  Brainflow Data Processing
        ConcurrentDictionary<string, BrainflowDataProcessor> DataProcessors { get; set; }
        ConcurrentDictionary<string, BlinkDetector> BlinkDetectors { get; set; }

        //  File writer
        OpenBCIGuiFormatRawFileWriter FileWriter;

        //  Connection flags

        private DateTimeOffset ConnectionStatusLastUpdateTime;

        /// <summary>
        /// Form closing event
        /// wait for the update UI task to exit
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

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
            comboBoxConnectedDevice.SelectedItem = "";

            comboBoxConnectedDevice.SelectedIndexChanged += comboBoxConnectedDevice_SelectedIndexChanged;
        }

        string DisconnectedString = "- Disconnected -";

        HatServer ConnectedServer = null;

        bool IsConnected => ConnectedServer != null;

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
                if (IsConnected && e.ValidData)
                {
                    label = "";
                    label += $"Time stamp: {e.CurrentReading.TimeStamp.ToString("N6")}\n";
                    label += $"Observation time: {DateTimeOffset.FromUnixTimeMilliseconds((long)(e.CurrentReading.TimeStamp * 1000.0)).ToLocalTime().ToString("HH:mm:ss.fff")}\n\n";
                    label += $"            {string.Format("{0,9}", "Read mV")}    {string.Format("{0,9}", "Dev uV")}      {string.Format("{0,9}", "Noise uV")}      {string.Format("{0,9}", "Pwr 10Hz")}   {string.Format("{0,9}", "10/8")}      {string.Format("{0,9}", "10/12")}\n";
                    label += $"            {string.Format("{0,9}", "-------")}     {string.Format("{0,9}", "-------")}     {string.Format("{0,9}", "-------")}     {string.Format("{0,9}", "-------")}     {string.Format("{0,9}", "-------")}      {string.Format("{0,9}", "-------")}\n";
                    
                    for( int i = 0; i < e.CurrentReading.NumberExgChannels; i++)
                    {
                        label += $"Channel {i}: {string.Format("{0,9}", (e.CurrentReading.GetExgDataForChannel(i) / 1000.0).ToString("N3"))}     {string.Format("{0,9}", e.CurrentDeviation.GetExgDataForChannel(i).ToString("N3"))}     {string.Format("{0,9}", e.CurrentDevMedian.GetExgDataForChannel(i).ToString("N3"))}     {string.Format("{0,9}", e.CurrentBandPower10.GetExgDataForChannel(i).ToString("N3"))}     {string.Format("{0,9}", (e.CurrentBandPower10.GetExgDataForChannel(i) / e.CurrentBandPower08.GetExgDataForChannel(i)).ToString("N3"))}      {string.Format("{0,9}", (e.CurrentBandPower10.GetExgDataForChannel(i) / e.CurrentBandPower12.GetExgDataForChannel(i)).ToString("N3"))}\n";
                    }
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
                if (IsConnected && e.ValidData)
                {
                    label = "";
                    label += $"Acel 0: {e.CurrentReading.GetAccelDataForChannel(0).ToString("N6")}\n";
                    label += $"Acel 1: {e.CurrentReading.GetAccelDataForChannel(1).ToString("N6")}\n";
                    label += $"Acel 2: {e.CurrentReading.GetAccelDataForChannel(2).ToString("N6")}\n";
                }

                labelAcelData.Invoke(new Action(() => { labelAcelData.Text = label; }));
            }
            catch (Exception)
            {
                Logger.AddLog(new LogEventArgs(this, "OnTheHatDataProcessorCurrentState", e, LogLevel.ERROR));
            }
        }

        private void CreateDataObjectsForNewServer(HatServer server)
        {
            var dataProcessor = new BrainflowDataProcessor(server.HostName, server.BoardId, server.SampleRate);
            dataProcessor.Log += OnLog;
            server.RawDataReceived += dataProcessor.AddDataToProcessor;
            DataProcessors.TryAdd(server.HostName, dataProcessor);

            var blinkDetector = new BlinkDetector();
            blinkDetector.Log += OnLog;
            dataProcessor.NewReading += blinkDetector.OnNewReading;
            blinkDetector.GetData = dataProcessor.GetRawData;
            blinkDetector.GetStdDevMedians = dataProcessor.GetStdDevianMedians;
            BlinkDetectors.TryAdd(server.HostName, blinkDetector);

           
            _ = Task.Run(async () =>
            {
                await dataProcessor.StartDataProcessorAsync();
            });
        }

        private async Task ShutDownDataObjectsForServer(HatServer server)
        {
            if (!DataProcessors.ContainsKey(server.HostName) && !BlinkDetectors.ContainsKey(server.HostName))
                return;

            try
            {
                DataProcessors.TryRemove(server.HostName, out var removedProcessor);
                await removedProcessor.StopDataProcessorAsync();
                removedProcessor.Log -= OnLog;
                server.RawDataReceived -= removedProcessor.AddDataToProcessor;

                BlinkDetectors.TryRemove(server.HostName, out var removedDetector);
                removedDetector.Log -= OnLog;
                removedProcessor.NewReading -= removedDetector.OnNewReading;
            }
            catch (Exception ex)
            {
                Logger.AddLog(new LogEventArgs(this, "OnHatConnectionChanged", ex, LogLevel.ERROR));
            }
        }

        private void DisconnectCurrentServerFromUiEvents()
        {
            var processor = GetProcessor(ConnectedServer.HostName);

            processor.NewReading -= FileWriter.AddData;
            processor.CurrentDataStateReported -= OnHatDataProcessorCurrentState;

            var blinkDetector = GetBlinkDetector(ConnectedServer.HostName);
            blinkDetector.DetectedBlink -= OnBlinkDetected;
        }

        private void HookUpCurrentServerToUiEvents()
        {
            var processor = GetProcessor(ConnectedServer.HostName);

            processor.NewReading += FileWriter.AddData;
            processor.CurrentDataStateReported += OnHatDataProcessorCurrentState;

            var blinkDetector = GetBlinkDetector(ConnectedServer.HostName);
            blinkDetector.DetectedBlink += OnBlinkDetected;
        }




        private async void OnHatConnectionChanged(object sender, HatConnectionEventArgs e)
        {
            Logger.AddLog(new LogEventArgs(this, "OnHatConnectionChanged", $"Hat connection changed {e.HostName} {e.State}.", LogLevel.INFO));


            switch (e.State)
            {
                case HatConnectionState.Discovered:
                    {
                        string selectedServer = "";
                        comboBoxConnectedDevice.Invoke(new Action(() =>
                        {
                            comboBoxConnectedDevice.SelectedIndexChanged -= comboBoxConnectedDevice_SelectedIndexChanged;
                            comboBoxConnectedDevice.Items.Add(e.HostName);
                            var currentSelection = comboBoxConnectedDevice.SelectedItem;
                            if ( currentSelection != null )
                                selectedServer = currentSelection.ToString();

                            comboBoxConnectedDevice.SelectedIndexChanged += comboBoxConnectedDevice_SelectedIndexChanged;
                        }));

                        CreateDataObjectsForNewServer(BrainHatServers.GetServer(e.HostName));

                        if ( ConnectedServer == null && selectedServer != DisconnectedString)
                        {
                            ConnectedServer = BrainHatServers.GetServer(e.HostName);
                            HookUpCurrentServerToUiEvents();
                            comboBoxConnectedDevice.Invoke(new Action(() =>
                            {
                                comboBoxConnectedDevice.SelectedIndexChanged -= comboBoxConnectedDevice_SelectedIndexChanged;
                                comboBoxConnectedDevice.SelectedItem = e.HostName;
                                comboBoxConnectedDevice.SelectedIndexChanged += comboBoxConnectedDevice_SelectedIndexChanged;
                            }));

                        }
                    }
                    break;

                case HatConnectionState.Lost:
                    {
                        comboBoxConnectedDevice.Invoke(new Action(() =>
                        {
                            if (comboBoxConnectedDevice.Items.Contains(e.HostName))
                            {
                                comboBoxConnectedDevice.SelectedIndexChanged -= comboBoxConnectedDevice_SelectedIndexChanged;
                                comboBoxConnectedDevice.Items.Remove(e.HostName);
                                comboBoxConnectedDevice.SelectedIndexChanged += comboBoxConnectedDevice_SelectedIndexChanged;
                            }
                        }));
                        var server = BrainHatServers.GetServer(e.HostName);
                        if (server != null)
                        {
                            await ShutDownDataObjectsForServer(server);
                        }
                        else
                        {
                            //  todo  log it
                        }
                    }
                    break;
            }
        }





        /// <summary>
        /// Process hat connection status update
        /// </summary>
        private void OnHatStatusUpdate(object sender, BrainHatStatusEventArgs e)
        {
            if (IsConnected && e.Status.HostName == ConnectedServer.HostName)
            {
                string statusString = $"host: {e.Status.HostName}\n";
                statusString += $"eth0:  {e.Eth0Description}\n";
                statusString += $"wlan0: {e.Wlan0Description}\n";
                statusString += $"ping speed: {e.Status.PingSpeed.TotalMilliseconds.ToString("N3")} ms.";
                ConnectionStatusLastUpdateTime = DateTimeOffset.UtcNow;

                labelConnectionStatus.Invoke(new Action(() => { labelConnectionStatus.Text = statusString; }));
            }
        }

        BrainflowDataProcessor GetProcessor(string hostName)
        {
            if (DataProcessors.ContainsKey(hostName))
                return DataProcessors[hostName];
            
            return null;
        }

        BlinkDetector GetBlinkDetector(string hostName)
        {
            if (BlinkDetectors.ContainsKey(hostName))
                return BlinkDetectors[hostName];

            return null;
        }


        private void comboBoxConnectedDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            labelExgData.Text = "Not receiving data from the sensor.";
            labelAcelData.Text = "Not receiving data from the sensor.";

            var selection = comboBoxConnectedDevice.SelectedItem.ToString();
            if (ConnectedServer != null)
            {
                if (ConnectedServer.HostName == selection)
                    return;

                DisconnectCurrentServerFromUiEvents();

                ConnectedServer = null;
            }

            if ( selection != DisconnectedString )
            {
                ConnectedServer = BrainHatServers.GetServer(selection);
                if ( ConnectedServer != null )
                {
                    HookUpCurrentServerToUiEvents();
                }
            }
            else
            {
                ConnectedServer = null;
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
