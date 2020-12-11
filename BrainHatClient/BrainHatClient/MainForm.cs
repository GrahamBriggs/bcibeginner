using BrainflowDataProcessing;
using BrainHatNetwork;
using BrainHatServersMonitor;
using LoggingInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrainHatClient
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            BrainHatNetworkAddresses.Channel1 = false;

            //  create the program logging object
            Logger = new Logging();


            InitializeComponent();

            SetupDevicesList();

            //  start logging thread
            StartLogging();

            // create the hat servers monitor
            BrainHatServers = new HatServersMonitor();
            // hook up hat monitor events
            BrainHatServers.Log += OnLog;
            BrainHatServers.HatStatusUpdate += OnHatStatusUpdate;
            BrainHatServers.HatConnectionChanged += OnHatConnectionChanged;

            //  we will create a data processor and blink detector for each server
            DataProcessors = new ConcurrentDictionary<string, BrainflowDataProcessor>();
            BlinkDetectors = new ConcurrentDictionary<string, BlinkDetector>();

            OpenForms = new ConcurrentDictionary<string, Form1>();

            //  start the brainHat servers mointor off the UI thread
            _ = Task.Run(async () =>
            {
                await BrainHatServers.StartMonitorAsync();
            });



        }

        //  Logging
        public static Logging Logger { get; protected set; }

        //  Servers Monitor
        public static HatServersMonitor BrainHatServers { get; protected set; }

        //  Brainflow Data Processing
        public static ConcurrentDictionary<string, BrainflowDataProcessor> DataProcessors { get; protected set; }
        public static ConcurrentDictionary<string, BlinkDetector> BlinkDetectors { get; protected set; }

        //  Collection of open server forms
        protected ConcurrentDictionary<string, Form1> OpenForms { get; set; }

        /// <summary>
        /// Setup the devices list view
        /// </summary>
        private void SetupDevicesList()
        {
            listViewDevices.Columns.Add("Device", 100);
            listViewDevices.Columns.Add("IP Address", 100);
            listViewDevices.Columns.Add("Board Id", 100);
            listViewDevices.Columns.Add("Sample Rate", 100);
            listViewDevices.MultiSelect = false;
            listViewDevices.FullRowSelect = true;
            listViewDevices.DoubleClick += ListViewDevices_DoubleClick;
        }


        /// <summary>
        /// Devices list view double click
        /// </summary>
        private void ListViewDevices_DoubleClick(object sender, EventArgs e)
        {
            var hostName = listViewDevices.SelectedItems[0].Tag.ToString();

            if (OpenForms.ContainsKey(hostName))
            {
                OpenForms[hostName].WindowState = FormWindowState.Minimized;
                OpenForms[hostName].Show();
                OpenForms[hostName].WindowState = FormWindowState.Normal;
            }
            else
            {
                var server = BrainHatServers.ConnectedServers.Where(x => x.HostName == hostName).FirstOrDefault();
                if (server != null)
                {
                    var newForm = new Form1(server.HostName, server.IpAddress, server.BoardId, server.SampleRate);
                    newForm.Text = $"brainHat {server.HostName}";
                    newForm.FormClosing += NewForm_FormClosing;
                    newForm.Show();
                    OpenForms.TryAdd(hostName, newForm);
                }
            }
        }


        /// <summary>
        /// Remove the form from collection when it closes
        /// </summary>
        private void NewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var form = (Form1)sender;
            OpenForms.TryRemove(form.HostName, out var discard);
        }


        /// <summary>
        /// Hat servers connection state changed
        /// </summary>
        private async void OnHatConnectionChanged(object sender, HatConnectionEventArgs e)
        {
            // Logger.AddLog(new LogEventArgs(this, "OnHatConnectionChanged", $"Hat connection changed {e.HostName} {e.State}.", LogLevel.INFO));


            switch (e.State)
            {
                case HatConnectionState.Discovered:
                    {
                        if (!DataProcessors.ContainsKey(e.HostName))
                        {
                            CreateDataObjectsForNewServer(BrainHatServers.GetServer(e.HostName));

                            var newListViewItem = new ListViewItem(new string[4] { e.HostName, e.IpAddress, e.BoardId.ToString(), e.SampleRate.ToString() });
                            newListViewItem.Tag = e.HostName;
                            Invoke(new Action(() => listViewDevices.Items.Add(newListViewItem)));
                        }
                    }
                    break;

                case HatConnectionState.Lost:
                    {
                        var server = BrainHatServers.GetServer(e.HostName);
                        if (server != null)
                        {
                            if (OpenForms.ContainsKey(e.HostName))
                            {
                                OpenForms.TryRemove(e.HostName, out var form);
                                Invoke(new Action(() => form.Close()));
                            }
                            Invoke(new Action(() =>
                            {
                                foreach (var nextItem in listViewDevices.Items)
                                {
                                    if (nextItem is ListViewItem item && item.Tag.ToString() == e.HostName)
                                    {
                                        listViewDevices.Items.Remove(item);
                                        break;
                                    }
                                }
                            }));

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



        private void OnHatStatusUpdate(object sender, BrainHatStatusEventArgs e)
        {
            //  TODO
            //if (IsConnected && e.Status.HostName == ConnectedServer.HostName)
            //{
            //    string statusString = $"host: {e.Status.HostName}\n";
            //    statusString += $"eth0:  {e.Eth0Description}\n";
            //    statusString += $"wlan0: {e.Wlan0Description}\n";
            //    statusString += $"ping speed: {e.Status.PingSpeed.TotalMilliseconds.ToString("N3")} ms.";
            //    ConnectionStatusLastUpdateTime = DateTimeOffset.UtcNow;

            //    labelConnectionStatus.Invoke(new Action(() => { labelConnectionStatus.Text = statusString; }));
            //}
        }


        /// <summary>
        /// We discovered a new server, create data processor objects to receive data
        /// </summary>
        private async void CreateDataObjectsForNewServer(HatServer server)
        {
            var dataProcessor = new BrainflowDataProcessor(server.HostName, server.BoardId, server.SampleRate);
            dataProcessor.Log += OnLog;
            server.RawDataReceived += dataProcessor.AddDataToProcessor;
            DataProcessors.TryAdd(server.HostName, dataProcessor);

            var blinkDetector = new BlinkDetector();
            blinkDetector.Log += OnLog;
            dataProcessor.NewSample += blinkDetector.OnNewSample;
            blinkDetector.GetData = dataProcessor.GetRawData;
            blinkDetector.GetStdDevMedians = dataProcessor.GetStdDevianMedians;
            BlinkDetectors.TryAdd(server.HostName, blinkDetector);

            await dataProcessor.StartDataProcessorAsync();
        }

        /// <summary>
        /// We lost connection to a server, shut down the objects connected to this server
        /// </summary>
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
                removedProcessor.NewSample -= removedDetector.OnNewSample;
            }
            catch (Exception ex)
            {
                Logger.AddLog(new LogEventArgs(this, "OnHatConnectionChanged", ex, LogLevel.ERROR));
            }
        }



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
