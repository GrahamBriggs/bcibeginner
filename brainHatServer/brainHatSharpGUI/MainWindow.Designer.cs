
namespace brainHatSharpGUI
{
    partial class MainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.buttonStart = new System.Windows.Forms.Button();
            this.comboBoxComPort = new System.Windows.Forms.ComboBox();
            this.groupBoxBoard = new System.Windows.Forms.GroupBox();
            this.checkBoxSRB = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxIpPort = new System.Windows.Forms.TextBox();
            this.textBoxIpAddress = new System.Windows.Forms.TextBox();
            this.checkBoxUseBFStream = new System.Windows.Forms.CheckBox();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.radioButtonDaisy = new System.Windows.Forms.RadioButton();
            this.radioButtonCyton = new System.Windows.Forms.RadioButton();
            this.buttonViewLogs = new System.Windows.Forms.Button();
            this.buttonConfigureBoard = new System.Windows.Forms.Button();
            this.labelVersion = new System.Windows.Forms.Label();
            this.checkBoxLogToFile = new System.Windows.Forms.CheckBox();
            this.groupBoxRunStatus = new System.Windows.Forms.GroupBox();
            this.pictureBoxStatus = new System.Windows.Forms.PictureBox();
            this.labelDataStatus = new System.Windows.Forms.Label();
            this.labelSrbStatus = new System.Windows.Forms.Label();
            this.labelRunStatus = new System.Windows.Forms.Label();
            this.groupBoxBoard.SuspendLayout();
            this.groupBoxRunStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonStart
            // 
            this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStart.Location = new System.Drawing.Point(382, 44);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(109, 23);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start Server";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // comboBoxComPort
            // 
            this.comboBoxComPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxComPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxComPort.FormattingEnabled = true;
            this.comboBoxComPort.Location = new System.Drawing.Point(187, 19);
            this.comboBoxComPort.Name = "comboBoxComPort";
            this.comboBoxComPort.Size = new System.Drawing.Size(165, 21);
            this.comboBoxComPort.TabIndex = 1;
            // 
            // groupBoxBoard
            // 
            this.groupBoxBoard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxBoard.Controls.Add(this.checkBoxSRB);
            this.groupBoxBoard.Controls.Add(this.label3);
            this.groupBoxBoard.Controls.Add(this.label2);
            this.groupBoxBoard.Controls.Add(this.textBoxIpPort);
            this.groupBoxBoard.Controls.Add(this.textBoxIpAddress);
            this.groupBoxBoard.Controls.Add(this.checkBoxUseBFStream);
            this.groupBoxBoard.Controls.Add(this.buttonRefresh);
            this.groupBoxBoard.Controls.Add(this.label1);
            this.groupBoxBoard.Controls.Add(this.radioButtonDaisy);
            this.groupBoxBoard.Controls.Add(this.radioButtonCyton);
            this.groupBoxBoard.Controls.Add(this.comboBoxComPort);
            this.groupBoxBoard.Location = new System.Drawing.Point(10, 12);
            this.groupBoxBoard.Name = "groupBoxBoard";
            this.groupBoxBoard.Size = new System.Drawing.Size(362, 168);
            this.groupBoxBoard.TabIndex = 2;
            this.groupBoxBoard.TabStop = false;
            this.groupBoxBoard.Text = "Connect to Board";
            // 
            // checkBoxSRB
            // 
            this.checkBoxSRB.AutoSize = true;
            this.checkBoxSRB.Location = new System.Drawing.Point(28, 74);
            this.checkBoxSRB.Name = "checkBoxSRB";
            this.checkBoxSRB.Size = new System.Drawing.Size(156, 17);
            this.checkBoxSRB.TabIndex = 11;
            this.checkBoxSRB.Text = "Start with SRB1 Connected";
            this.checkBoxSRB.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(205, 142);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Port";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(173, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "IP Address";
            // 
            // textBoxIpPort
            // 
            this.textBoxIpPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxIpPort.Location = new System.Drawing.Point(246, 139);
            this.textBoxIpPort.Name = "textBoxIpPort";
            this.textBoxIpPort.Size = new System.Drawing.Size(106, 20);
            this.textBoxIpPort.TabIndex = 8;
            // 
            // textBoxIpAddress
            // 
            this.textBoxIpAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxIpAddress.Location = new System.Drawing.Point(246, 113);
            this.textBoxIpAddress.Name = "textBoxIpAddress";
            this.textBoxIpAddress.Size = new System.Drawing.Size(106, 20);
            this.textBoxIpAddress.TabIndex = 7;
            // 
            // checkBoxUseBFStream
            // 
            this.checkBoxUseBFStream.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxUseBFStream.AutoSize = true;
            this.checkBoxUseBFStream.Location = new System.Drawing.Point(6, 113);
            this.checkBoxUseBFStream.Name = "checkBoxUseBFStream";
            this.checkBoxUseBFStream.Size = new System.Drawing.Size(155, 17);
            this.checkBoxUseBFStream.TabIndex = 6;
            this.checkBoxUseBFStream.Text = "Enable Brainflow Streaming";
            this.checkBoxUseBFStream.UseVisualStyleBackColor = true;
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRefresh.Location = new System.Drawing.Point(257, 46);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(95, 23);
            this.buttonRefresh.TabIndex = 5;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(128, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "COM Port";
            // 
            // radioButtonDaisy
            // 
            this.radioButtonDaisy.AutoSize = true;
            this.radioButtonDaisy.Location = new System.Drawing.Point(6, 46);
            this.radioButtonDaisy.Name = "radioButtonDaisy";
            this.radioButtonDaisy.Size = new System.Drawing.Size(84, 17);
            this.radioButtonDaisy.TabIndex = 3;
            this.radioButtonDaisy.TabStop = true;
            this.radioButtonDaisy.Text = "Cyton+Daisy";
            this.radioButtonDaisy.UseVisualStyleBackColor = true;
            // 
            // radioButtonCyton
            // 
            this.radioButtonCyton.AutoSize = true;
            this.radioButtonCyton.Location = new System.Drawing.Point(6, 20);
            this.radioButtonCyton.Name = "radioButtonCyton";
            this.radioButtonCyton.Size = new System.Drawing.Size(52, 17);
            this.radioButtonCyton.TabIndex = 2;
            this.radioButtonCyton.TabStop = true;
            this.radioButtonCyton.Text = "Cyton";
            this.radioButtonCyton.UseVisualStyleBackColor = true;
            // 
            // buttonViewLogs
            // 
            this.buttonViewLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonViewLogs.Location = new System.Drawing.Point(382, 108);
            this.buttonViewLogs.Name = "buttonViewLogs";
            this.buttonViewLogs.Size = new System.Drawing.Size(109, 23);
            this.buttonViewLogs.TabIndex = 3;
            this.buttonViewLogs.Text = "View Logs";
            this.buttonViewLogs.UseVisualStyleBackColor = true;
            this.buttonViewLogs.Click += new System.EventHandler(this.buttonViewLogs_Click);
            // 
            // buttonConfigureBoard
            // 
            this.buttonConfigureBoard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConfigureBoard.Location = new System.Drawing.Point(382, 76);
            this.buttonConfigureBoard.Name = "buttonConfigureBoard";
            this.buttonConfigureBoard.Size = new System.Drawing.Size(109, 23);
            this.buttonConfigureBoard.TabIndex = 4;
            this.buttonConfigureBoard.Text = "Configure Board";
            this.buttonConfigureBoard.UseVisualStyleBackColor = true;
            this.buttonConfigureBoard.Click += new System.EventHandler(this.buttonConfigureBoard_Click);
            // 
            // labelVersion
            // 
            this.labelVersion.Location = new System.Drawing.Point(7, 183);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(309, 23);
            this.labelVersion.TabIndex = 5;
            this.labelVersion.Text = "label4";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // checkBoxLogToFile
            // 
            this.checkBoxLogToFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxLogToFile.AutoSize = true;
            this.checkBoxLogToFile.Location = new System.Drawing.Point(382, 137);
            this.checkBoxLogToFile.Name = "checkBoxLogToFile";
            this.checkBoxLogToFile.Size = new System.Drawing.Size(75, 17);
            this.checkBoxLogToFile.TabIndex = 6;
            this.checkBoxLogToFile.Text = "Log to File";
            this.checkBoxLogToFile.UseVisualStyleBackColor = true;
            // 
            // groupBoxRunStatus
            // 
            this.groupBoxRunStatus.BackColor = System.Drawing.Color.Transparent;
            this.groupBoxRunStatus.Controls.Add(this.pictureBoxStatus);
            this.groupBoxRunStatus.Controls.Add(this.labelDataStatus);
            this.groupBoxRunStatus.Controls.Add(this.labelSrbStatus);
            this.groupBoxRunStatus.Controls.Add(this.labelRunStatus);
            this.groupBoxRunStatus.Location = new System.Drawing.Point(10, 12);
            this.groupBoxRunStatus.Name = "groupBoxRunStatus";
            this.groupBoxRunStatus.Size = new System.Drawing.Size(362, 168);
            this.groupBoxRunStatus.TabIndex = 7;
            this.groupBoxRunStatus.TabStop = false;
            // 
            // pictureBoxStatus
            // 
            this.pictureBoxStatus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBoxStatus.Image = global::brainHatSharpGUI.Properties.Resources.yellowLight;
            this.pictureBoxStatus.InitialImage = null;
            this.pictureBoxStatus.Location = new System.Drawing.Point(10, 21);
            this.pictureBoxStatus.Name = "pictureBoxStatus";
            this.pictureBoxStatus.Size = new System.Drawing.Size(48, 48);
            this.pictureBoxStatus.TabIndex = 3;
            this.pictureBoxStatus.TabStop = false;
            // 
            // labelDataStatus
            // 
            this.labelDataStatus.AutoSize = true;
            this.labelDataStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDataStatus.Location = new System.Drawing.Point(93, 71);
            this.labelDataStatus.Name = "labelDataStatus";
            this.labelDataStatus.Size = new System.Drawing.Size(56, 20);
            this.labelDataStatus.TabIndex = 2;
            this.labelDataStatus.Text = "Status";
            // 
            // labelSrbStatus
            // 
            this.labelSrbStatus.AutoSize = true;
            this.labelSrbStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSrbStatus.Location = new System.Drawing.Point(93, 109);
            this.labelSrbStatus.Name = "labelSrbStatus";
            this.labelSrbStatus.Size = new System.Drawing.Size(56, 20);
            this.labelSrbStatus.TabIndex = 1;
            this.labelSrbStatus.Text = "Status";
            // 
            // labelRunStatus
            // 
            this.labelRunStatus.AutoSize = true;
            this.labelRunStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRunStatus.Location = new System.Drawing.Point(64, 32);
            this.labelRunStatus.Name = "labelRunStatus";
            this.labelRunStatus.Size = new System.Drawing.Size(60, 24);
            this.labelRunStatus.TabIndex = 0;
            this.labelRunStatus.Text = "Status";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 206);
            this.Controls.Add(this.groupBoxRunStatus);
            this.Controls.Add(this.checkBoxLogToFile);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.buttonConfigureBoard);
            this.Controls.Add(this.buttonViewLogs);
            this.Controls.Add(this.groupBoxBoard);
            this.Controls.Add(this.buttonStart);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(517, 245);
            this.MinimumSize = new System.Drawing.Size(517, 245);
            this.Name = "MainWindow";
            this.Text = "brainHat Server";
            this.groupBoxBoard.ResumeLayout(false);
            this.groupBoxBoard.PerformLayout();
            this.groupBoxRunStatus.ResumeLayout(false);
            this.groupBoxRunStatus.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStatus)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.ComboBox comboBoxComPort;
        private System.Windows.Forms.GroupBox groupBoxBoard;
        private System.Windows.Forms.RadioButton radioButtonDaisy;
        private System.Windows.Forms.RadioButton radioButtonCyton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonViewLogs;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.Button buttonConfigureBoard;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxIpPort;
        private System.Windows.Forms.TextBox textBoxIpAddress;
        private System.Windows.Forms.CheckBox checkBoxUseBFStream;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.CheckBox checkBoxLogToFile;
        private System.Windows.Forms.GroupBox groupBoxRunStatus;
        private System.Windows.Forms.CheckBox checkBoxSRB;
        private System.Windows.Forms.Label labelRunStatus;
        private System.Windows.Forms.Label labelDataStatus;
        private System.Windows.Forms.Label labelSrbStatus;
        private System.Windows.Forms.PictureBox pictureBoxStatus;
    }
}

