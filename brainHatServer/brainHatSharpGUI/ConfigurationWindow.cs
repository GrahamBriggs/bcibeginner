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

            Load += ConfigurationWindow_Load;
            FormClosing += ConfigurationWindow_FormClosing;
            
            SetupUi();
        }


        /// <summary>
        /// Setup the user interface
        /// </summary>
        private void SetupUi()
        {
            SetupConfigurationListView();

            SetupSignalTestComboBox();

            ShowSrb1Ui(false);
        }

        private void ShowSrb1Ui(bool visible)
        {
            labelCytonSrb.Visible = visible;
            labelCytonSrbStatus.Visible = visible;
            buttonCytonSrb.Visible = visible;

            labelDaisySrb.Visible = visible;
            labelDaisySrbStatus.Visible = visible;
            buttonDaisySrb.Visible = visible;
        }


        /// <summary>
        /// Setup signal test combo box
        /// </summary>
        private void SetupSignalTestComboBox()
        {
            comboBoxSignalTest.Items.Add(new ComboBoxItem() { Text = "Ground", Value = TestSignalMode.InternalGround });
            comboBoxSignalTest.Items.Add(new ComboBoxItem() { Text = "1x Slow", Value = TestSignalMode.Signal1Slow });
            comboBoxSignalTest.Items.Add(new ComboBoxItem() { Text = "1x Fast", Value = TestSignalMode.Signal1Fast });
            comboBoxSignalTest.Items.Add(new ComboBoxItem() { Text = "DC Signal", Value = TestSignalMode.DcSignal });
            comboBoxSignalTest.Items.Add(new ComboBoxItem() { Text = "2x Slow", Value = TestSignalMode.Signal2Slow });
            comboBoxSignalTest.Items.Add(new ComboBoxItem() { Text = "2x Fast", Value = TestSignalMode.Signal2Fast });
            comboBoxSignalTest.SelectedIndex = 0;
        }


        /// <summary>
        /// Setup Configuraiton list view
        /// </summary>
        private void SetupConfigurationListView()
        {
            listViewConfig.View = View.Details;
            listViewConfig.Columns.Add("", listViewConfig.Width, HorizontalAlignment.Left);
            listViewConfig.HeaderStyle = ColumnHeaderStyle.None;

            listViewConfig.Resize += listViewConfig_Resize;

            listViewChannels.Columns.Add("Channel", 60);
            listViewChannels.Columns.Add("Power Down", 75);
            listViewChannels.Columns.Add("Gain", 50);
            listViewChannels.Columns.Add("Input Type", 75);
            listViewChannels.Columns.Add("Bias Set", 60);
            listViewChannels.Columns.Add("SRB2", 60);
            listViewChannels.MultiSelect = true;
            listViewChannels.FullRowSelect = true;

            UpdateListViewChannels();
        }
        //
        private void listViewConfig_Resize(object sender, EventArgs e)
        {
            listViewConfig.Columns[0].Width = listViewConfig.Width + 10;
        }


        

        private async void ConfigurationWindow_Load(object sender, EventArgs e)
        {
            EnableSettingsButtons(false);

            UpdateListViewRawConfiguration("Querying board parameters ...");

            await Board.StopStreamAndEmptyBufferAsnyc();
            await Task.Delay(2000);

            try
            {
                var config = await GetBoardConfig();
                UpdateUiWithBoardConfiguration(config);
            }
            catch
            {
                UpdateListViewRawConfiguration("Failed to get valid board configuration!");
            }
            EnableSettingsButtons(true);
        }


        private async void ConfigurationWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            await Board.StartStreamAsync();
        }


        /// <summary>
        /// Load the board configuration string into the settings object
        /// </summary>
        private void LoadBoardSettings(string configString)
        {
            try
            {
                BoardSettings = new BrainHatBoardSettingsImplementation(configString);
            }
            catch (Exception)
            {
                BoardSettings = null;
            }
        }

        

        BoardDataReader Board;
        BrainHatBoardSettingsImplementation BoardSettings;

       

        /// <summary>
        /// Update the list view with latest config for channels
        /// </summary>
        private void UpdateListViewChannels()
        {
            if (BoardSettings == null)
                return;

            listViewChannels.BeginUpdate();

            listViewChannels.Items.Clear();

            foreach ( var nextBoard in BoardSettings.Boards )
            {
                foreach (var nextChannel in nextBoard.Channels)
                {
                    var newItem = listViewChannels.Items.Add( new ListViewItem(new string[6] { nextChannel.ChannelNumber.ToString(), nextChannel.PowerDown.ToString(), nextChannel.Gain.ToString(), nextChannel.InputType.ToString(), nextChannel.Bias.ToString(), nextChannel.Srb2.ToString() }));
                    newItem.Tag = nextChannel;
                }
            }

            listViewChannels.EndUpdate();
        }


        /// <summary>
        /// Update the raw config list view
        /// </summary>
        protected void UpdateListViewRawConfiguration(string config)
        {
            listViewConfig.BeginUpdate();

            listViewConfig.Items.Clear();

            foreach (var nextString in config.Split('\n').Reverse())
            {
                var item = listViewConfig.Items.Insert(0, nextString);
            }

            listViewConfig.EndUpdate();
        }

        private void UpdateSrbButtons()
        {
            if (BoardSettings == null)
                return;

            if (BoardSettings.Boards.Count() > 0)
            {
                labelCytonSrb.Visible = true;
                labelCytonSrbStatus.Text = BoardSettings.Boards.First().Srb1Set ? "SRB1 Connected" : "SRB1 Disconnected (default)";
                buttonCytonSrb.Text = BoardSettings.Boards.First().Srb1Set ? "Disconnect SRB1" : "Connect SRB1";

                labelCytonSrb.Visible = true;
                labelCytonSrbStatus.Visible = true;
                buttonCytonSrb.Visible = true;
            }

            if(BoardSettings.Boards.Count() > 1)
            {
                labelDaisySrb.Visible = true;
                labelDaisySrbStatus.Text = BoardSettings.Boards.Last().Srb1Set ? "SRB1 Connected" : "SRB1 Disconnected (default)";
                buttonDaisySrb.Text = BoardSettings.Boards.Last().Srb1Set ? "Disconnect SRB1" : "Connect SRB1";

                labelDaisySrb.Visible = true;
                labelDaisySrbStatus.Visible = true;
                buttonDaisySrb.Visible = true;

            }
        }

        /// <summary>
        /// Update the UI with current board state
        /// </summary>
        private void UpdateUiWithBoardConfiguration(string config)
        {
            LoadBoardSettings(config);
            UpdateListViewRawConfiguration(config);
            UpdateListViewChannels();
            UpdateSrbButtons();
        }


        /// <summary>
        /// Blank out the board channel UI before settings operation will update it
        /// </summary>
        private void BlankBoardUi(string message = "")
        {
            listViewChannels.Items.Clear();
            listViewConfig.Items.Clear();
            UpdateListViewRawConfiguration(message);
            ShowSrb1Ui(false);
        }


        /// <summary>
        /// Enable or disable the settings buttons
        /// </summary>
        private void EnableSettingsButtons(bool enable)
        {
            buttonChannelDefaults.Enabled = enable;
            buttonReload.Enabled = enable;
            buttonSetChannels.Enabled = enable;
            buttonSignalTest.Enabled = enable;
            comboBoxSignalTest.Enabled = enable;
            buttonStartStream.Enabled = enable;
            buttonStopStream.Enabled = enable;
            buttonCytonSrb.Enabled = enable;
            buttonDaisySrb.Enabled = enable;
        }


        /// <summary>
        /// Get the current board config and update the UI
        /// </summary>
        private async Task GetBoardConfigAndUpdateUi()
        {
            try
            {
                var config = await GetBoardConfig();
                UpdateUiWithBoardConfiguration(config);
            }
            catch (Exception)
            {
                UpdateListViewRawConfiguration("Failed to get board configuration.");
            }
        }


        /// <summary>
        /// Get current board config
        /// </summary>
        private async Task<string> GetBoardConfig()
        {
            int retries = 0;
            while (retries < 5)
            {
                try
                {
                    await Task.Delay(1000);
                    var config = await Board.GetBoardConfigurationAsync();
                    return config;
                }
                catch (Exception)
                {
                    UpdateListViewRawConfiguration("Failed to get valid board configuration! Retrying ...");
                    await Task.Delay(3000);
                    retries++;
                }
            }
            throw new Exception("Unable to get valid board config");
        }


        /// <summary>
        /// Check board for streaming before executing a command
        /// </summary>
        private bool IsSafeToExecuteCommand()
        {
            if (Board.IsStreaming)
            {
                MessageBox.Show("You must stop streaming before sending command.", "brainHat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }


        /// <summary>
        /// Start stream button
        /// </summary>
        private async void buttonStartStream_Click(object sender, EventArgs e)
        {
            EnableSettingsButtons(false);
            await Board.StartStreamAsync();
            EnableSettingsButtons(true);
        }


        /// <summary>
        /// Stop stream button
        /// </summary>
        private async void buttonStopStream_Click(object sender, EventArgs e)
        {
            EnableSettingsButtons(false);
            await Board.StopStreamAndEmptyBufferAsnyc();
            EnableSettingsButtons(true);
        }


        /// <summary>
        /// Reload config button
        /// </summary>
        private async void buttonReload_Click(object sender, EventArgs e)
        {
            if (!IsSafeToExecuteCommand())
                return;

            EnableSettingsButtons(false);
            BlankBoardUi();

            await GetBoardConfigAndUpdateUi();

            EnableSettingsButtons(true);
        }


        /// <summary>
        /// Set channels button
        /// </summary>
        private async void buttonSetChannels_Click(object sender, EventArgs e)
        {
            if (!IsSafeToExecuteCommand())
                return;

            var selected = listViewChannels.SelectedItems;

            if (selected.Count > 0)
            {
                List<int> selectedChannels = new List<int>();
                for (int i = 0; i < listViewChannels.SelectedItems.Count; i++)
                {
                    selectedChannels.Add(((ICytonChannelSettings)(listViewChannels.SelectedItems[i]).Tag).ChannelNumber);
                }

                var settingsWindow = new ChannelSettings(selectedChannels, (ICytonChannelSettings)selected[0].Tag);
                if (settingsWindow.ShowDialog() == DialogResult.OK)
                {
                    EnableSettingsButtons(false);
                    BlankBoardUi("Setting channels ...");

                    await Board.SetBoardChannelAsync(settingsWindow.ChannelsToSet, settingsWindow);
                    await Task.Delay(3000);

                    await GetBoardConfigAndUpdateUi();
                    EnableSettingsButtons(true);
                }
            }
        }


        /// <summary>
        /// Set channel defaults button
        /// </summary>
        private async void buttonChannelDefaults_Click(object sender, EventArgs e)
        {
            if (!IsSafeToExecuteCommand())
                return;

            EnableSettingsButtons(false);
            BlankBoardUi("Setting channels to default ...");

            await Board.ResetChannelsToDefaultAsync();
            await Task.Delay(3000);

            await GetBoardConfigAndUpdateUi();
            EnableSettingsButtons(true);
        }


        /// <summary>
        /// Signal testing
        /// </summary>
        private async void buttonSignalTest_Click(object sender, EventArgs e)
        {
            if (!IsSafeToExecuteCommand())
                return;

            EnableSettingsButtons(false);
            BlankBoardUi("Setting signal test mode ...");

            var item = (ComboBoxItem)comboBoxSignalTest.SelectedItem;
            await Board.SetSignalTestModeAsync((TestSignalMode)item.Value);
            await Task.Delay(3000);

            await GetBoardConfigAndUpdateUi();
            EnableSettingsButtons(true);
        }

       


        /// <summary>
        /// Set SRB on Cyton board button
        /// </summary>
        private async void buttonCytonSrb_Click(object sender, EventArgs e)
        {
            if (!IsSafeToExecuteCommand() || BoardSettings.Boards.Count() < 1)
                return;

            var isSet = BoardSettings.Boards.First().Srb1Set;
            var firstChannel = BoardSettings.Boards.First().Channels.First();
            if (firstChannel != null)
            {
                EnableSettingsButtons(false);
                BlankBoardUi();

                await Board.SetSrb1Async(firstChannel, !isSet);
                await Task.Delay(3000);

                await GetBoardConfigAndUpdateUi();
                EnableSettingsButtons(true);
            }
        }

        /// <summary>
        /// Set SRB on Daisy board button
        /// </summary>
        private async void buttonDaisySrb_Click(object sender, EventArgs e)
        {
            if (!IsSafeToExecuteCommand() || BoardSettings.Boards.Count() < 2)
                return;

            var isSet = BoardSettings.Boards.Last().Srb1Set;
            var firstChannel = BoardSettings.Boards.Last().Channels.First();
            if (firstChannel != null)
            {
                EnableSettingsButtons(false);
                BlankBoardUi();

                await Board.SetSrb1Async(firstChannel, ! isSet);
                await Task.Delay(3000);

                await GetBoardConfigAndUpdateUi();
                EnableSettingsButtons(true);
            }
        }
       
    }
}
