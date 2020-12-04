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
        Logging Logger;
       


        protected void SetupLoggingUi()
        {
            //  log display list view
            listViewLogs.View = View.Details;
            listViewLogs.Columns.Add("", listViewLogs.Width, HorizontalAlignment.Left);
            listViewLogs.HeaderStyle = ColumnHeaderStyle.None;
            listViewLogs.Resize += listViewLogs_Resize;

            // log settings combo box
            comboBoxLogLevel.DataSource = Enum.GetValues(typeof(LogLevel));
            Logger.LogLevelDisplay = LogLevel.TRACE; 
            comboBoxLogLevel.SelectedItem = LogLevel.TRACE;
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
            Logger.AddLog(e);
        }


        /// <summary>
        /// Logging function, update the UI with the log and send it to the log4 appenders
        /// </summary>
        public void OnLoggedEvents(object sender, IEnumerable<LogEventArgs> logs)
        {
            var logsToDisplay = logs.Where(x => x.Level >= Logger.LogLevelDisplay);

            if (logsToDisplay.Count() > 0)
            {
                listViewLogs.BeginUpdate();

                foreach (var nextLog in logsToDisplay)
                {
                    if (nextLog.Level >= Logger.LogLevelDisplay)
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
            Logger.LogLevelDisplay = (LogLevel)comboBoxLogLevel.SelectedItem;
        }

        private async void comboBoxLogLevelRemote_SelectedIndexChanged(object sender, EventArgs e)
        {   
            try
            {
                var level = (LogLevel)comboBoxLogLevelRemote.SelectedItem;

                if (!IsConnected)
                {
                    OnProgramLog(this, new LogEventArgs(this, "comboBoxRemoteLogLevel_SelectedIndexChanged", $"Hat is not connected.", LogLevel.WARN));
                    return;
                }

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
            var response = await Tcpip.GetTcpResponse(ConnectedServer.IpAddress, BrainHatNetworkAddresses.ServerPort, $"loglevel?object=a&level={(int)level}");
            if (!response.CheckHatResponse())
            {
                OnProgramLog(this, new LogEventArgs(this, "comboBoxRemoteLogLevel_SelectedIndexChanged", $"Received an invalid response {response}.", LogLevel.ERROR));
            }
        }
    }
}
