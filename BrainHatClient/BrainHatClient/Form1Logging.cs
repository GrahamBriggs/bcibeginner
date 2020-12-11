using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BrainHatNetwork;
using BrainHatServersMonitor;
using LoggingInterfaces;

namespace BrainHatClient
{
    public partial class Form1
    {


        LogLevel LogLevelDisplay;

        protected void SetupLoggingUi()
        {
            //  log display list view
            listViewLogs.View = View.Details;
            listViewLogs.Columns.Add("", listViewLogs.Width, HorizontalAlignment.Left);
            listViewLogs.HeaderStyle = ColumnHeaderStyle.None;
            listViewLogs.Resize += listViewLogs_Resize;

            // log settings combo box
            comboBoxLogLevel.DataSource = Enum.GetValues(typeof(LogLevel));
            LogLevelDisplay = MainForm.Logger.LogLevelDisplay;
            comboBoxLogLevel.SelectedItem = LogLevel.INFO;
            comboBoxLogLevel.SelectedIndexChanged += comboBoxLogLevel_SelectedIndexChanged;
            
            

            //  remote log settings combo box
            comboBoxLogLevelRemote.DataSource = Enum.GetValues(typeof(LogLevel));
            comboBoxLogLevelRemote.SelectedItem = LogLevel.TRACE;
            comboBoxLogLevelRemote.SelectedIndexChanged += comboBoxLogLevelRemote_SelectedIndexChanged;
        }


        /// <summary>
        /// Program log function, will queue the log up for processing
        /// </summary>
        public void OnProgramLog(object sender, LogEventArgs e)
        {
            MainForm.Logger.AddLog(e);
        }


        /// <summary>
        /// Logging function, update the UI with the log and send it to the log4 appenders
        /// </summary>
        public void OnLoggedEvents(object sender, IEnumerable<LogEventArgs> logs)
        {
            var logsToDisplay = logs.Where(x => x.HostName == HostName && x.Level >= LogLevelDisplay);

            if (logsToDisplay.Count() > 0)
            {
                listViewLogs.BeginUpdate();

                foreach (var nextLog in logsToDisplay)
                {
                    if (nextLog.Level >= MainForm.Logger.LogLevelDisplay)
                    {
                        var item = listViewLogs.Items.Insert(0, nextLog.FormatLogForConsole());
                        item.ForeColor = nextLog.Level.LogColour();
                        item.BackColor = nextLog.Level.BackgrondColour(nextLog.Remote);
                    }
                }

                while (listViewLogs.Items.Count > 500)
                    listViewLogs.Items.RemoveAt(listViewLogs.Items.Count - 1);

                listViewLogs.EndUpdate();
            }
        }



        private void listViewLogs_Resize(object sender, EventArgs e)
        {

            listViewLogs.Columns[0].Width = listViewLogs.Width + 10;

        }



        private void comboBoxLogLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
           LogLevelDisplay =  MainForm.Logger.LogLevelDisplay = (LogLevel)comboBoxLogLevel.SelectedItem;
        }

        private async void comboBoxLogLevelRemote_SelectedIndexChanged(object sender, EventArgs e)
        {   
            try
            {
                var level = (LogLevel)comboBoxLogLevelRemote.SelectedItem;

                await SetRemoteLogLevel(level);
            }
            catch (Exception ex)
            {
                OnProgramLog(this, new LogEventArgs(this, "comboBoxRemoteLogLevel_SelectedIndexChanged", ex, LogLevel.ERROR));
            }
        }

        private async System.Threading.Tasks.Task SetRemoteLogLevel(LogLevel level)
        {
            OnProgramLog(this, new LogEventArgs(this, "comboBoxRemoteLogLevel_SelectedIndexChanged", $"Setting remote log level {level}.", LogLevel.INFO));
            var response = await Tcpip.GetTcpResponse(IpAddress, BrainHatNetworkAddresses.ServerPort, $"loglevel?object=a&level={(int)level}");
            if (!response.CheckHatResponse())
            {
                OnProgramLog(this, new LogEventArgs(this, "comboBoxRemoteLogLevel_SelectedIndexChanged", $"Received an invalid response {response}.", LogLevel.ERROR));
            }
        }
    }
}
