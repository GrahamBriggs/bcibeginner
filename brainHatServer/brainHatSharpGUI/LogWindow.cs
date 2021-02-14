using LoggingInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace brainHatSharpGUI
{
    public partial class LogWindow : Form
    {
        public LogWindow(Logging logger, IEnumerable<LogEventArgs> buffer)
        {
            InitializeComponent();

            Logger = logger;

            SetupLoggingUi(buffer);
        }

        Logging Logger;

        protected void SetupLoggingUi(IEnumerable<LogEventArgs> buffer)
        {
            //  log display list view
            listViewLogs.View = View.Details;
            listViewLogs.Columns.Add("", listViewLogs.Width, HorizontalAlignment.Left);
            listViewLogs.HeaderStyle = ColumnHeaderStyle.None;
            listViewLogs.Resize += listViewLogs_Resize;

            AddLogsToListView(buffer);

            // log settings combo box
            comboBoxLogLevel.DataSource = Enum.GetValues(typeof(LogLevel));
            comboBoxLogLevel.SelectedItem = Logger.LogLevelDisplay;
            comboBoxLogLevel.SelectedIndexChanged += comboBoxLogLevel_SelectedIndexChanged;
        }

        /// <summary>
        /// Logging function, update the UI with the log and send it to the log4 appenders
        /// </summary>
        public void OnLoggedEvents(object sender, IEnumerable<LogEventArgs> logs)
        {
            listViewLogs.Invoke(new Action( () =>
            {
                if (logs.Count() > 0)
                {
                    AddLogsToListView(logs);
                }
            }));
        }

        private void AddLogsToListView(IEnumerable<LogEventArgs> logs)
        {
            listViewLogs.BeginUpdate();

            foreach (var nextLog in logs)
            {
                if (nextLog.Level >= Logger.LogLevelDisplay)
                {
                    System.Diagnostics.Debug.WriteLine($"Next Log in UI: {nextLog.FormatLogForConsole()}");
                    var item = listViewLogs.Items.Insert(0, nextLog.FormatLogForConsole());
                    item.ForeColor = nextLog.Level.LogColour();
                    item.BackColor = nextLog.Level.BackgrondColour(nextLog.Remote);
                }
            }

            while (listViewLogs.Items.Count > 500)
                listViewLogs.Items.RemoveAt(listViewLogs.Items.Count - 1);

            listViewLogs.EndUpdate();
        }

        public void ChangeLogLevel()
        {
            comboBoxLogLevel.Invoke( new Action( () =>    
            {
                comboBoxLogLevel.SelectedItem = Logger.LogLevelDisplay;
            }));
        }

        private void comboBoxLogLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            Logger.LogLevelDisplay = (LogLevel)comboBoxLogLevel.SelectedItem;
        }

        private void listViewLogs_Resize(object sender, EventArgs e)
        {
            listViewLogs.Columns[0].Width = listViewLogs.Width + 10;
        }
    }
}
