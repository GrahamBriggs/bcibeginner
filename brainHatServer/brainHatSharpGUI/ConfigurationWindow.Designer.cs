
namespace brainHatSharpGUI
{
    partial class ConfigurationWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigurationWindow));
            this.groupBoxConfiguration = new System.Windows.Forms.GroupBox();
            this.buttonReload = new System.Windows.Forms.Button();
            this.listViewConfig = new System.Windows.Forms.ListView();
            this.buttonSignalTest = new System.Windows.Forms.Button();
            this.comboBoxSignalTest = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelDaisySrbStatus = new System.Windows.Forms.Label();
            this.buttonDaisySrb = new System.Windows.Forms.Button();
            this.labelDaisySrb = new System.Windows.Forms.Label();
            this.labelCytonSrbStatus = new System.Windows.Forms.Label();
            this.buttonCytonSrb = new System.Windows.Forms.Button();
            this.labelCytonSrb = new System.Windows.Forms.Label();
            this.buttonSetChannels = new System.Windows.Forms.Button();
            this.listViewChannels = new System.Windows.Forms.ListView();
            this.buttonChannelDefaults = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonStartStream = new System.Windows.Forms.Button();
            this.buttonStopStream = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBoxConfiguration.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxConfiguration
            // 
            this.groupBoxConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxConfiguration.Controls.Add(this.buttonReload);
            this.groupBoxConfiguration.Controls.Add(this.listViewConfig);
            this.groupBoxConfiguration.Location = new System.Drawing.Point(13, 13);
            this.groupBoxConfiguration.Name = "groupBoxConfiguration";
            this.groupBoxConfiguration.Size = new System.Drawing.Size(388, 580);
            this.groupBoxConfiguration.TabIndex = 0;
            this.groupBoxConfiguration.TabStop = false;
            this.groupBoxConfiguration.Text = "Current Configuration";
            // 
            // buttonReload
            // 
            this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReload.Location = new System.Drawing.Point(272, 551);
            this.buttonReload.Name = "buttonReload";
            this.buttonReload.Size = new System.Drawing.Size(110, 23);
            this.buttonReload.TabIndex = 7;
            this.buttonReload.Text = "Reload";
            this.buttonReload.UseVisualStyleBackColor = true;
            this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
            // 
            // listViewConfig
            // 
            this.listViewConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewConfig.BackColor = System.Drawing.Color.White;
            this.listViewConfig.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewConfig.ForeColor = System.Drawing.Color.Black;
            this.listViewConfig.HideSelection = false;
            this.listViewConfig.Location = new System.Drawing.Point(6, 19);
            this.listViewConfig.MultiSelect = false;
            this.listViewConfig.Name = "listViewConfig";
            this.listViewConfig.Size = new System.Drawing.Size(376, 527);
            this.listViewConfig.TabIndex = 6;
            this.listViewConfig.UseCompatibleStateImageBehavior = false;
            // 
            // buttonSignalTest
            // 
            this.buttonSignalTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSignalTest.Location = new System.Drawing.Point(94, 50);
            this.buttonSignalTest.Name = "buttonSignalTest";
            this.buttonSignalTest.Size = new System.Drawing.Size(110, 23);
            this.buttonSignalTest.TabIndex = 1;
            this.buttonSignalTest.Text = "Set Test Mode";
            this.buttonSignalTest.UseVisualStyleBackColor = true;
            this.buttonSignalTest.Click += new System.EventHandler(this.buttonSignalTest_Click);
            // 
            // comboBoxSignalTest
            // 
            this.comboBoxSignalTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxSignalTest.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSignalTest.FormattingEnabled = true;
            this.comboBoxSignalTest.Location = new System.Drawing.Point(83, 23);
            this.comboBoxSignalTest.Name = "comboBoxSignalTest";
            this.comboBoxSignalTest.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSignalTest.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.labelDaisySrbStatus);
            this.groupBox1.Controls.Add(this.buttonDaisySrb);
            this.groupBox1.Controls.Add(this.labelDaisySrb);
            this.groupBox1.Controls.Add(this.labelCytonSrbStatus);
            this.groupBox1.Controls.Add(this.buttonCytonSrb);
            this.groupBox1.Controls.Add(this.labelCytonSrb);
            this.groupBox1.Controls.Add(this.buttonSetChannels);
            this.groupBox1.Controls.Add(this.listViewChannels);
            this.groupBox1.Controls.Add(this.buttonChannelDefaults);
            this.groupBox1.Location = new System.Drawing.Point(407, 117);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(442, 476);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Channel Configuration";
            // 
            // labelDaisySrbStatus
            // 
            this.labelDaisySrbStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDaisySrbStatus.Location = new System.Drawing.Point(253, 411);
            this.labelDaisySrbStatus.Name = "labelDaisySrbStatus";
            this.labelDaisySrbStatus.Size = new System.Drawing.Size(167, 23);
            this.labelDaisySrbStatus.TabIndex = 12;
            this.labelDaisySrbStatus.Text = "label4";
            // 
            // buttonDaisySrb
            // 
            this.buttonDaisySrb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDaisySrb.Location = new System.Drawing.Point(250, 441);
            this.buttonDaisySrb.Name = "buttonDaisySrb";
            this.buttonDaisySrb.Size = new System.Drawing.Size(164, 23);
            this.buttonDaisySrb.TabIndex = 11;
            this.buttonDaisySrb.Text = "SRB1";
            this.buttonDaisySrb.UseVisualStyleBackColor = true;
            this.buttonDaisySrb.Click += new System.EventHandler(this.buttonDaisySrb_Click);
            // 
            // labelDaisySrb
            // 
            this.labelDaisySrb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDaisySrb.AutoSize = true;
            this.labelDaisySrb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDaisySrb.Location = new System.Drawing.Point(248, 393);
            this.labelDaisySrb.Name = "labelDaisySrb";
            this.labelDaisySrb.Size = new System.Drawing.Size(111, 13);
            this.labelDaisySrb.TabIndex = 10;
            this.labelDaisySrb.Text = "Daisy Board SRB1";
            // 
            // labelCytonSrbStatus
            // 
            this.labelCytonSrbStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelCytonSrbStatus.Location = new System.Drawing.Point(11, 411);
            this.labelCytonSrbStatus.Name = "labelCytonSrbStatus";
            this.labelCytonSrbStatus.Size = new System.Drawing.Size(167, 23);
            this.labelCytonSrbStatus.TabIndex = 9;
            this.labelCytonSrbStatus.Text = "SRB1 Disconnected";
            // 
            // buttonCytonSrb
            // 
            this.buttonCytonSrb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCytonSrb.Location = new System.Drawing.Point(9, 441);
            this.buttonCytonSrb.Name = "buttonCytonSrb";
            this.buttonCytonSrb.Size = new System.Drawing.Size(164, 23);
            this.buttonCytonSrb.TabIndex = 8;
            this.buttonCytonSrb.Text = "SRB1";
            this.buttonCytonSrb.UseVisualStyleBackColor = true;
            this.buttonCytonSrb.Click += new System.EventHandler(this.buttonCytonSrb_Click);
            // 
            // labelCytonSrb
            // 
            this.labelCytonSrb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelCytonSrb.AutoSize = true;
            this.labelCytonSrb.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCytonSrb.Location = new System.Drawing.Point(6, 393);
            this.labelCytonSrb.Name = "labelCytonSrb";
            this.labelCytonSrb.Size = new System.Drawing.Size(112, 13);
            this.labelCytonSrb.TabIndex = 7;
            this.labelCytonSrb.Text = "Cyton Board SRB1";
            // 
            // buttonSetChannels
            // 
            this.buttonSetChannels.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSetChannels.Location = new System.Drawing.Point(206, 344);
            this.buttonSetChannels.Name = "buttonSetChannels";
            this.buttonSetChannels.Size = new System.Drawing.Size(110, 23);
            this.buttonSetChannels.TabIndex = 6;
            this.buttonSetChannels.Text = "Set Channel(s)";
            this.buttonSetChannels.UseVisualStyleBackColor = true;
            this.buttonSetChannels.Click += new System.EventHandler(this.buttonSetChannels_Click);
            // 
            // listViewChannels
            // 
            this.listViewChannels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewChannels.HideSelection = false;
            this.listViewChannels.Location = new System.Drawing.Point(6, 19);
            this.listViewChannels.Name = "listViewChannels";
            this.listViewChannels.Size = new System.Drawing.Size(426, 319);
            this.listViewChannels.TabIndex = 5;
            this.listViewChannels.UseCompatibleStateImageBehavior = false;
            this.listViewChannels.View = System.Windows.Forms.View.Details;
            // 
            // buttonChannelDefaults
            // 
            this.buttonChannelDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChannelDefaults.Location = new System.Drawing.Point(322, 344);
            this.buttonChannelDefaults.Name = "buttonChannelDefaults";
            this.buttonChannelDefaults.Size = new System.Drawing.Size(110, 23);
            this.buttonChannelDefaults.TabIndex = 4;
            this.buttonChannelDefaults.Text = "Reset to Default";
            this.buttonChannelDefaults.UseVisualStyleBackColor = true;
            this.buttonChannelDefaults.Click += new System.EventHandler(this.buttonChannelDefaults_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.buttonSignalTest);
            this.groupBox2.Controls.Add(this.comboBoxSignalTest);
            this.groupBox2.Location = new System.Drawing.Point(633, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(216, 88);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Test Signal";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Test Mode";
            // 
            // buttonStartStream
            // 
            this.buttonStartStream.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStartStream.Location = new System.Drawing.Point(37, 22);
            this.buttonStartStream.Name = "buttonStartStream";
            this.buttonStartStream.Size = new System.Drawing.Size(110, 23);
            this.buttonStartStream.TabIndex = 8;
            this.buttonStartStream.Text = "Start Stream";
            this.buttonStartStream.UseVisualStyleBackColor = true;
            this.buttonStartStream.Click += new System.EventHandler(this.buttonStartStream_Click);
            // 
            // buttonStopStream
            // 
            this.buttonStopStream.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStopStream.Location = new System.Drawing.Point(37, 51);
            this.buttonStopStream.Name = "buttonStopStream";
            this.buttonStopStream.Size = new System.Drawing.Size(110, 23);
            this.buttonStopStream.TabIndex = 9;
            this.buttonStopStream.Text = "Stop Stream";
            this.buttonStopStream.UseVisualStyleBackColor = true;
            this.buttonStopStream.Click += new System.EventHandler(this.buttonStopStream_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.buttonStartStream);
            this.groupBox3.Controls.Add(this.buttonStopStream);
            this.groupBox3.Location = new System.Drawing.Point(413, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(193, 88);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Data Stream";
            // 
            // ConfigurationWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(874, 611);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxConfiguration);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ConfigurationWindow";
            this.Text = "ConfigurationWindow";
            this.groupBoxConfiguration.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxConfiguration;
        private System.Windows.Forms.ListView listViewConfig;
        private System.Windows.Forms.Button buttonSignalTest;
        private System.Windows.Forms.ComboBox comboBoxSignalTest;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonChannelDefaults;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listViewChannels;
        private System.Windows.Forms.Button buttonReload;
        private System.Windows.Forms.Button buttonSetChannels;
        private System.Windows.Forms.Button buttonStartStream;
        private System.Windows.Forms.Button buttonStopStream;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label labelDaisySrbStatus;
        private System.Windows.Forms.Button buttonDaisySrb;
        private System.Windows.Forms.Label labelDaisySrb;
        private System.Windows.Forms.Label labelCytonSrbStatus;
        private System.Windows.Forms.Button buttonCytonSrb;
        private System.Windows.Forms.Label labelCytonSrb;
    }
}