
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
            this.groupBoxChannelConfig = new System.Windows.Forms.GroupBox();
            this.buttonImpedance = new System.Windows.Forms.Button();
            this.labelDaisySrbStatus = new System.Windows.Forms.Label();
            this.buttonDaisySrb = new System.Windows.Forms.Button();
            this.labelDaisySrb = new System.Windows.Forms.Label();
            this.labelCytonSrbStatus = new System.Windows.Forms.Label();
            this.buttonCytonSrb = new System.Windows.Forms.Button();
            this.labelCytonSrb = new System.Windows.Forms.Label();
            this.buttonSetChannels = new System.Windows.Forms.Button();
            this.listViewChannels = new System.Windows.Forms.ListView();
            this.buttonChannelDefaults = new System.Windows.Forms.Button();
            this.groupBoxTestSignal = new System.Windows.Forms.GroupBox();
            this.labelTestMode = new System.Windows.Forms.Label();
            this.buttonStartStream = new System.Windows.Forms.Button();
            this.buttonStopStream = new System.Windows.Forms.Button();
            this.groupBoxDataStream = new System.Windows.Forms.GroupBox();
            this.groupBoxConfiguration.SuspendLayout();
            this.groupBoxChannelConfig.SuspendLayout();
            this.groupBoxTestSignal.SuspendLayout();
            this.groupBoxDataStream.SuspendLayout();
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
            this.groupBoxConfiguration.Size = new System.Drawing.Size(378, 580);
            this.groupBoxConfiguration.TabIndex = 0;
            this.groupBoxConfiguration.TabStop = false;
            this.groupBoxConfiguration.Text = "Configuración actual";
            // 
            // buttonReload
            // 
            this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReload.Location = new System.Drawing.Point(262, 551);
            this.buttonReload.Name = "buttonReload";
            this.buttonReload.Size = new System.Drawing.Size(110, 23);
            this.buttonReload.TabIndex = 7;
            this.buttonReload.Text = "Recargar";
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
            this.listViewConfig.Size = new System.Drawing.Size(366, 527);
            this.listViewConfig.TabIndex = 6;
            this.listViewConfig.UseCompatibleStateImageBehavior = false;
            // 
            // buttonSignalTest
            // 
            this.buttonSignalTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSignalTest.Location = new System.Drawing.Point(97, 50);
            this.buttonSignalTest.Name = "buttonSignalTest";
            this.buttonSignalTest.Size = new System.Drawing.Size(163, 23);
            this.buttonSignalTest.TabIndex = 1;
            this.buttonSignalTest.Text = "Establecer modo de prueba";
            this.buttonSignalTest.UseVisualStyleBackColor = true;
            this.buttonSignalTest.Click += new System.EventHandler(this.buttonSignalTest_Click);
            // 
            // comboBoxSignalTest
            // 
            this.comboBoxSignalTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxSignalTest.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSignalTest.FormattingEnabled = true;
            this.comboBoxSignalTest.Location = new System.Drawing.Point(97, 23);
            this.comboBoxSignalTest.Name = "comboBoxSignalTest";
            this.comboBoxSignalTest.Size = new System.Drawing.Size(164, 21);
            this.comboBoxSignalTest.TabIndex = 2;
            // 
            // groupBoxChannelConfig
            // 
            this.groupBoxChannelConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxChannelConfig.Controls.Add(this.buttonImpedance);
            this.groupBoxChannelConfig.Controls.Add(this.labelDaisySrbStatus);
            this.groupBoxChannelConfig.Controls.Add(this.buttonDaisySrb);
            this.groupBoxChannelConfig.Controls.Add(this.labelDaisySrb);
            this.groupBoxChannelConfig.Controls.Add(this.labelCytonSrbStatus);
            this.groupBoxChannelConfig.Controls.Add(this.buttonCytonSrb);
            this.groupBoxChannelConfig.Controls.Add(this.labelCytonSrb);
            this.groupBoxChannelConfig.Controls.Add(this.buttonSetChannels);
            this.groupBoxChannelConfig.Controls.Add(this.listViewChannels);
            this.groupBoxChannelConfig.Controls.Add(this.buttonChannelDefaults);
            this.groupBoxChannelConfig.Location = new System.Drawing.Point(397, 117);
            this.groupBoxChannelConfig.Name = "groupBoxChannelConfig";
            this.groupBoxChannelConfig.Size = new System.Drawing.Size(540, 476);
            this.groupBoxChannelConfig.TabIndex = 3;
            this.groupBoxChannelConfig.TabStop = false;
            this.groupBoxChannelConfig.Text = "Configuración de canal";
            // 
            // buttonImpedance
            // 
            this.buttonImpedance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonImpedance.Location = new System.Drawing.Point(6, 344);
            this.buttonImpedance.Name = "buttonImpedance";
            this.buttonImpedance.Size = new System.Drawing.Size(156, 23);
            this.buttonImpedance.TabIndex = 13;
            this.buttonImpedance.Text = "Establecer impedancia(s)";
            this.buttonImpedance.UseVisualStyleBackColor = true;
            this.buttonImpedance.Click += new System.EventHandler(this.buttonImpedance_Click);
            // 
            // labelDaisySrbStatus
            // 
            this.labelDaisySrbStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDaisySrbStatus.Location = new System.Drawing.Point(346, 411);
            this.labelDaisySrbStatus.Name = "labelDaisySrbStatus";
            this.labelDaisySrbStatus.Size = new System.Drawing.Size(167, 23);
            this.labelDaisySrbStatus.TabIndex = 12;
            this.labelDaisySrbStatus.Text = "label4";
            // 
            // buttonDaisySrb
            // 
            this.buttonDaisySrb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDaisySrb.Location = new System.Drawing.Point(348, 441);
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
            this.labelDaisySrb.Location = new System.Drawing.Point(346, 393);
            this.labelDaisySrb.Name = "labelDaisySrb";
            this.labelDaisySrb.Size = new System.Drawing.Size(74, 13);
            this.labelDaisySrb.TabIndex = 10;
            this.labelDaisySrb.Text = "Daisy SRB1";
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
            this.labelCytonSrb.Location = new System.Drawing.Point(11, 393);
            this.labelCytonSrb.Name = "labelCytonSrb";
            this.labelCytonSrb.Size = new System.Drawing.Size(88, 13);
            this.labelCytonSrb.TabIndex = 7;
            this.labelCytonSrb.Text = "Equipos SRB1";
            // 
            // buttonSetChannels
            // 
            this.buttonSetChannels.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSetChannels.Location = new System.Drawing.Point(190, 344);
            this.buttonSetChannels.Name = "buttonSetChannels";
            this.buttonSetChannels.Size = new System.Drawing.Size(156, 23);
            this.buttonSetChannels.TabIndex = 6;
            this.buttonSetChannels.Text = "Establecer canal(es)";
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
            this.listViewChannels.Size = new System.Drawing.Size(524, 319);
            this.listViewChannels.TabIndex = 5;
            this.listViewChannels.UseCompatibleStateImageBehavior = false;
            this.listViewChannels.View = System.Windows.Forms.View.Details;
            // 
            // buttonChannelDefaults
            // 
            this.buttonChannelDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChannelDefaults.Location = new System.Drawing.Point(374, 344);
            this.buttonChannelDefaults.Name = "buttonChannelDefaults";
            this.buttonChannelDefaults.Size = new System.Drawing.Size(156, 23);
            this.buttonChannelDefaults.TabIndex = 4;
            this.buttonChannelDefaults.Text = "Establecer predeterminado";
            this.buttonChannelDefaults.UseVisualStyleBackColor = true;
            this.buttonChannelDefaults.Click += new System.EventHandler(this.buttonChannelDefaults_Click);
            // 
            // groupBoxTestSignal
            // 
            this.groupBoxTestSignal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTestSignal.Controls.Add(this.labelTestMode);
            this.groupBoxTestSignal.Controls.Add(this.buttonSignalTest);
            this.groupBoxTestSignal.Controls.Add(this.comboBoxSignalTest);
            this.groupBoxTestSignal.Location = new System.Drawing.Point(649, 13);
            this.groupBoxTestSignal.Name = "groupBoxTestSignal";
            this.groupBoxTestSignal.Size = new System.Drawing.Size(288, 88);
            this.groupBoxTestSignal.TabIndex = 4;
            this.groupBoxTestSignal.TabStop = false;
            this.groupBoxTestSignal.Text = "Señal de prueba";
            // 
            // labelTestMode
            // 
            this.labelTestMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTestMode.AutoSize = true;
            this.labelTestMode.Location = new System.Drawing.Point(22, 26);
            this.labelTestMode.Name = "labelTestMode";
            this.labelTestMode.Size = new System.Drawing.Size(34, 13);
            this.labelTestMode.TabIndex = 3;
            this.labelTestMode.Text = "Modo";
            // 
            // buttonStartStream
            // 
            this.buttonStartStream.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStartStream.Location = new System.Drawing.Point(55, 22);
            this.buttonStartStream.Name = "buttonStartStream";
            this.buttonStartStream.Size = new System.Drawing.Size(137, 23);
            this.buttonStartStream.TabIndex = 8;
            this.buttonStartStream.Text = "Iniciar transmisión";
            this.buttonStartStream.UseVisualStyleBackColor = true;
            this.buttonStartStream.Click += new System.EventHandler(this.buttonStartStream_Click);
            // 
            // buttonStopStream
            // 
            this.buttonStopStream.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStopStream.Location = new System.Drawing.Point(55, 51);
            this.buttonStopStream.Name = "buttonStopStream";
            this.buttonStopStream.Size = new System.Drawing.Size(137, 23);
            this.buttonStopStream.TabIndex = 9;
            this.buttonStopStream.Text = "Detener transmisión";
            this.buttonStopStream.UseVisualStyleBackColor = true;
            this.buttonStopStream.Click += new System.EventHandler(this.buttonStopStream_Click);
            // 
            // groupBoxDataStream
            // 
            this.groupBoxDataStream.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDataStream.Controls.Add(this.buttonStartStream);
            this.groupBoxDataStream.Controls.Add(this.buttonStopStream);
            this.groupBoxDataStream.Location = new System.Drawing.Point(397, 12);
            this.groupBoxDataStream.Name = "groupBoxDataStream";
            this.groupBoxDataStream.Size = new System.Drawing.Size(246, 88);
            this.groupBoxDataStream.TabIndex = 10;
            this.groupBoxDataStream.TabStop = false;
            this.groupBoxDataStream.Text = "Transmisión de datos";
            // 
            // ConfigurationWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(949, 611);
            this.Controls.Add(this.groupBoxDataStream);
            this.Controls.Add(this.groupBoxTestSignal);
            this.Controls.Add(this.groupBoxChannelConfig);
            this.Controls.Add(this.groupBoxConfiguration);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ConfigurationWindow";
            this.Text = "ConfigurationWindow";
            this.groupBoxConfiguration.ResumeLayout(false);
            this.groupBoxChannelConfig.ResumeLayout(false);
            this.groupBoxChannelConfig.PerformLayout();
            this.groupBoxTestSignal.ResumeLayout(false);
            this.groupBoxTestSignal.PerformLayout();
            this.groupBoxDataStream.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxConfiguration;
        private System.Windows.Forms.ListView listViewConfig;
        private System.Windows.Forms.Button buttonSignalTest;
        private System.Windows.Forms.ComboBox comboBoxSignalTest;
        private System.Windows.Forms.GroupBox groupBoxChannelConfig;
        private System.Windows.Forms.Button buttonChannelDefaults;
        private System.Windows.Forms.GroupBox groupBoxTestSignal;
        private System.Windows.Forms.Label labelTestMode;
        private System.Windows.Forms.ListView listViewChannels;
        private System.Windows.Forms.Button buttonReload;
        private System.Windows.Forms.Button buttonSetChannels;
        private System.Windows.Forms.Button buttonStartStream;
        private System.Windows.Forms.Button buttonStopStream;
        private System.Windows.Forms.GroupBox groupBoxDataStream;
        private System.Windows.Forms.Label labelDaisySrbStatus;
        private System.Windows.Forms.Button buttonDaisySrb;
        private System.Windows.Forms.Label labelDaisySrb;
        private System.Windows.Forms.Label labelCytonSrbStatus;
        private System.Windows.Forms.Button buttonCytonSrb;
        private System.Windows.Forms.Label labelCytonSrb;
        private System.Windows.Forms.Button buttonImpedance;
    }
}