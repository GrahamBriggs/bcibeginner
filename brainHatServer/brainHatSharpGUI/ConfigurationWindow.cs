using BrainflowInterfaces;
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
   

    public partial class ConfigurationWindow : Form
    {
        public LogEventDelegate Log;

        public ConfigurationWindow(BoardDataReader board)
        {
            InitializeComponent();

            Board = board;
          
            FormClosing += OnFormClosing;
            SetupUi();

            UpdateConfigurationListView(Board.BoardConfigurationString);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        BoardDataReader Board;

        private void SetupUi()
        {
            //  log display list view
            listViewConfig.View = View.Details;
            listViewConfig.Columns.Add("", listViewConfig.Width, HorizontalAlignment.Left);
            listViewConfig.HeaderStyle = ColumnHeaderStyle.None;
            
            listViewConfig.Resize += listViewConfig_Resize;

            comboBoxSignalTest.Items.Add(new ComboBoxItem() { Text = "Ground", Value = TestSignalMode.InternalGround });
            comboBoxSignalTest.Items.Add(new ComboBoxItem() { Text = "1x Slow", Value = TestSignalMode.Signal1Slow });
            comboBoxSignalTest.Items.Add(new ComboBoxItem() { Text = "1x Fast", Value = TestSignalMode.Signal1Fast});
            comboBoxSignalTest.Items.Add(new ComboBoxItem() { Text = "DC Signal", Value = TestSignalMode.DcSignal });
            comboBoxSignalTest.Items.Add(new ComboBoxItem() { Text = "2x Slow", Value = TestSignalMode.Signal2Slow });
            comboBoxSignalTest.Items.Add(new ComboBoxItem() { Text = "2x Fast", Value = TestSignalMode.Signal2Fast });
            comboBoxSignalTest.SelectedIndex = 0;

        }

        private void listViewConfig_Resize(object sender, EventArgs e)
        {
            listViewConfig.Columns[0].Width = listViewConfig.Width + 10;
        }

        protected void UpdateConfigurationListView(string config)
        {
            listViewConfig.BeginUpdate();

            listViewConfig.Items.Clear();

            foreach (var nextString in config.Split('\n').Reverse())
            {
                var item = listViewConfig.Items.Insert(0, nextString);
            }

            listViewConfig.EndUpdate();
        }

        bool SignalTesting = false;

        private async void buttonSignalTest_Click(object sender, EventArgs e)
        {
            buttonSignalTest.Enabled = false;

            if ( SignalTesting )
            {
                SignalTesting = false;
                buttonSignalTest.Text = "Start Signal Test";

                await Board.ResetSessionAsync();

                comboBoxSignalTest.Enabled = true;
            }
            else
            {
                SignalTesting = true;
                buttonSignalTest.Text = "Stop Signal Test";
                comboBoxSignalTest.Enabled = false;

                var item = (ComboBoxItem)comboBoxSignalTest.SelectedItem;
                await Board.StartSignalTestAsync((TestSignalMode)item.Value);
            }

            buttonSignalTest.Enabled = true;
        }

        private async void buttonChannelDefaults_Click(object sender, EventArgs e)
        {
            buttonChannelDefaults.Enabled = false;

            await Board.ResetChannelsToDefaultAsync();

            UpdateConfigurationListView(Board.BoardConfigurationString);

            buttonChannelDefaults.Enabled = true;
        }
    }
}
