using BrainflowInterfaces;
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
    public partial class ImpedanceSettings : Form, ICytonChannelSettings
    {
        public ImpedanceSettings(IEnumerable<int> channels, ICytonChannelSettings settings)
        {
            InitializeComponent();
            ChannelsToSet = channels;

            var labelTitle = channels.Count() > 1 ? "Set Channels" : "Set Channel";

            labelChannel.Text = $"{labelTitle} {string.Join(", ", channels)}";

            comboBoxLlofP.Items.Add(new ComboBoxItem("Test signal not applied (default)", false));
            comboBoxLlofP.Items.Add(new ComboBoxItem("Test signal applied", true));
            comboBoxLlofP.SelectedIndex = settings.LlofP ? 1 : 0;

            comboBoxLlofN.Items.Add(new ComboBoxItem("Test signal not applied (default)", false));
            comboBoxLlofN.Items.Add(new ComboBoxItem("Test signal applied", true));
            comboBoxLlofN.SelectedIndex = settings.LlofN ? 1 : 0;



        }


        public override string ToString()
        {
            return this.ChannelSettingsToString();
        }


        public IEnumerable<int> ChannelsToSet { get; protected set; }

       

        public bool LlofP
        {
            get
            {
                return (bool)((ComboBoxItem)(comboBoxLlofP.SelectedItem)).Value;
            }
            set
            {
                comboBoxLlofP.SelectedIndex = value ? 1 : 0;
            }
        }

        

        public bool LlofN
        {
            get
            {
                return (bool)((ComboBoxItem)(comboBoxLlofN.SelectedItem)).Value;
            }
            set
            {
                comboBoxLlofN.SelectedIndex = value ? 1 : 0;
            }
        }


        public AdsChannelInputType InputType { get; set; }

        public bool PowerDown { get; set; }
        public ChannelGain Gain { get; set; }
        public bool Bias { get; set; }

        public bool Srb2 { get; set; }

        public int ChannelNumber { get; set; }
       
       

        private void buttonSetImpedance_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
