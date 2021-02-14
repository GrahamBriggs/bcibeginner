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
    public partial class ChannelSettings : Form, ICytonChannelSettings
    {
        public ChannelSettings(IEnumerable<int> channels, ICytonChannelSettings settings)
        {
            InitializeComponent();
            ChannelsToSet = channels;

            var labelTitle = channels.Count() > 1 ? "Set Channels" : "Set Channel";

            labelChannel.Text = $"{labelTitle} {string.Join(", ", channels)}";

            comboBoxPowerDown.Items.Add(new ComboBoxItem("false (default)", false));
            comboBoxPowerDown.Items.Add(new ComboBoxItem("true", true));
            comboBoxPowerDown.SelectedIndex = settings.PowerDown ? 1 : 0;

            comboBoxGain.Items.Add(new ComboBoxItem("1x", ChannelGain.x1));
            comboBoxGain.Items.Add(new ComboBoxItem("2x", ChannelGain.x2));
            comboBoxGain.Items.Add(new ComboBoxItem("4x", ChannelGain.x4));
            comboBoxGain.Items.Add(new ComboBoxItem("6x", ChannelGain.x6));
            comboBoxGain.Items.Add(new ComboBoxItem("8x", ChannelGain.x8));
            comboBoxGain.Items.Add(new ComboBoxItem("12x", ChannelGain.x12));
            comboBoxGain.Items.Add(new ComboBoxItem("24x (default)", ChannelGain.x24));
            comboBoxGain.SelectedIndex = (int)settings.Gain;

            comboBoxInputType.Items.Add(new ComboBoxItem("Normal (default)", AdsChannelInputType.Normal));
            comboBoxInputType.Items.Add(new ComboBoxItem("Shorted", AdsChannelInputType.Shorted));
            comboBoxInputType.Items.Add(new ComboBoxItem("Bias Measured", AdsChannelInputType.BiasMeas));
            comboBoxInputType.Items.Add(new ComboBoxItem("MVDD", AdsChannelInputType.Mvdd));
            comboBoxInputType.Items.Add(new ComboBoxItem("Temp", AdsChannelInputType.Temp));
            comboBoxInputType.Items.Add(new ComboBoxItem("Testing", AdsChannelInputType.Testsig));
            comboBoxInputType.Items.Add(new ComboBoxItem("Bias Drp", AdsChannelInputType.BiasDrp));
            comboBoxInputType.Items.Add(new ComboBoxItem("Bias Drn", AdsChannelInputType.BiasDrn));
            comboBoxInputType.SelectedIndex = (int)settings.InputType;

            comboBoxBias.Items.Add(new ComboBoxItem("Remove from BIAS", false));
            comboBoxBias.Items.Add(new ComboBoxItem("Include in BIAS (default)", true));
            comboBoxBias.SelectedIndex = settings.Bias ? 1 : 0;

            comboBoxSrb2.Items.Add(new ComboBoxItem("Disconnected from SRB2", false));
            comboBoxSrb2.Items.Add(new ComboBoxItem("Connected to SRB2 (default)", true));
            comboBoxSrb2.SelectedIndex = settings.Srb2 ? 1 : 0;
        }


        public override string ToString()
        {
            return this.ChannelSettingsToString();
        }


        public IEnumerable<int> ChannelsToSet { get; protected set; }

        public bool PowerDown
        {
            get
            {
                return (bool)((ComboBoxItem)(comboBoxPowerDown.SelectedItem)).Value;
            }
            set
            {
                comboBoxPowerDown.SelectedIndex = value ? 1 : 0;
            }
        }

        public ChannelGain Gain
        {
            get
            {
                return (ChannelGain)((ComboBoxItem)(comboBoxGain.SelectedItem)).Value;
            }
            set
            {
                comboBoxGain.SelectedIndex = (int)value;
            }
        }


        public AdsChannelInputType InputType
        {
            get
            {
                return (AdsChannelInputType)((ComboBoxItem)(comboBoxInputType.SelectedItem)).Value;
            }
            set
            {
                comboBoxInputType.SelectedIndex = (int)value;
            }
        }


        public bool Bias
        {
            get
            {
                return (bool)((ComboBoxItem)(comboBoxBias.SelectedItem)).Value;
            }
            set
            {
                comboBoxBias.SelectedIndex = value ? 1 : 0;
            }
        }

        public bool Srb2
        {
            get
            {
                return (bool)((ComboBoxItem)(comboBoxSrb2.SelectedItem)).Value;
            }
            set
            {
                comboBoxSrb2.SelectedIndex = value ? 1 : 0;
            }
        }

        public int ChannelNumber { get; set; }

        private void buttonSetChannels_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
