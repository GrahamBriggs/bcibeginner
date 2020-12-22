using BrainHatNetwork;
using LoggingInterfaces;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrainHatClient
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            Logger = new Logging();
           
            InitializeComponent();

            Logger.LogToFile = true;
            checkBoxLogging.Checked = true;

            SetupDevicesList();

            StartLogging();

            BrainHatServers = new HatServersMonitor();
            BrainHatServers.Log += OnLog;
            BrainHatServers.HatConnectionStatusUpdate += OnHatStatusUpdate;
            BrainHatServers.HatConnectionChanged += OnHatConnectionChanged;

            OpenForms = new ConcurrentDictionary<string, Form1>();

            //  start the brainHat servers mointor off the UI thread
            _ = Task.Run(async () =>
            {
                await BrainHatServers.StartMonitorAsync();
            });
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            var task = Task.Run(async () => await BrainHatServers.StopMonitorAsync());
            task.Wait();
 
            base.OnFormClosing(e);
        }

        //  Logging
        public static Logging Logger { get; protected set; }

        //  Servers Monitor
        public static HatServersMonitor BrainHatServers { get; protected set; }


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
                //  window for this device is open already, bring it to the foreground
                OpenForms[hostName].WindowState = FormWindowState.Minimized;
                OpenForms[hostName].Show();
                OpenForms[hostName].WindowState = FormWindowState.Normal;
            }
            else
            {
                //  open a new window for this device
                var server = BrainHatServers.ConnectedServers.Where(x => x.HostName == hostName).FirstOrDefault();
                if (server != null)
                {
                    var newForm = new Form1(server);
                    newForm.Text = $"{server.HostName}";
                    newForm.FormClosing += NewForm_FormClosing;
                    newForm.Show();
                    OpenForms.TryAdd(hostName, newForm);
                }
                else
                {
                    MessageBox.Show("Error. That server does not exist", "brainHat", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        /// <summary>
        /// Remove the form from collection when it closes
        /// </summary>
        private async void NewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var form = (Form1)sender;
            var server = BrainHatServers.GetServer(form.ConnectedServer.HostName);
            if (server != null)
            {
                await server.StopReadingFromLslAsync();
            }
            OpenForms.TryRemove(form.ConnectedServer.HostName, out var discard);
        }
        


        bool DevicesListContainsServer(string hostName)
        {
            return (bool)listViewDevices.Invoke(new Func<bool>(() =>
            {
                foreach (var nextItem in listViewDevices.Items)
                {
                    if (nextItem is ListViewItem listViewItem)
                    {
                        if ((string)listViewItem.Tag == hostName)
                            return true;
                    }
                }
                return false;
            }));

            
        }


        /// <summary>
        /// Hat servers connection state changed
        /// </summary>
        private  void OnHatConnectionChanged(object sender, HatConnectionEventArgs e)
        {
            switch (e.State)
            {
                case HatConnectionState.Discovered:
                    {
                        if (!DevicesListContainsServer(e.HostName) )
                        {
              
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

        private async void checkBoxLogging_CheckedChanged(object sender, EventArgs e)
        {
            Logger.LogToFile = true;
            Logger.AddLog(new LogEventArgs(this, "checkBoxLogging_CheckChanged", $"Logging to file {(checkBoxLogging.Checked ? "enabled" : "disabled")}.", LogLevel.INFO));
            await Task.Delay(1000);
            Logger.LogToFile = checkBoxLogging.Checked;
        }
    }
}
