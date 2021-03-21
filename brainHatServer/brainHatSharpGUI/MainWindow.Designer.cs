
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
            this.groupBoxBoard.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonStart
            // 
            this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStart.Location = new System.Drawing.Point(376, 44);
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
            this.comboBoxComPort.FormattingEnabled = true;
            this.comboBoxComPort.Location = new System.Drawing.Point(187, 19);
            this.comboBoxComPort.Name = "comboBoxComPort";
            this.comboBoxComPort.Size = new System.Drawing.Size(159, 21);
            this.comboBoxComPort.TabIndex = 1;
            // 
            // groupBoxBoard
            // 
            this.groupBoxBoard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.groupBoxBoard.Location = new System.Drawing.Point(12, 12);
            this.groupBoxBoard.Name = "groupBoxBoard";
            this.groupBoxBoard.Size = new System.Drawing.Size(356, 145);
            this.groupBoxBoard.TabIndex = 2;
            this.groupBoxBoard.TabStop = false;
            this.groupBoxBoard.Text = "Connect to Board";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(199, 116);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(167, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "IP Address";
            // 
            // textBoxIpPort
            // 
            this.textBoxIpPort.Location = new System.Drawing.Point(240, 113);
            this.textBoxIpPort.Name = "textBoxIpPort";
            this.textBoxIpPort.Size = new System.Drawing.Size(106, 20);
            this.textBoxIpPort.TabIndex = 8;
            // 
            // textBoxIpAddress
            // 
            this.textBoxIpAddress.Location = new System.Drawing.Point(240, 82);
            this.textBoxIpAddress.Name = "textBoxIpAddress";
            this.textBoxIpAddress.Size = new System.Drawing.Size(106, 20);
            this.textBoxIpAddress.TabIndex = 7;
            // 
            // checkBoxUseBFStream
            // 
            this.checkBoxUseBFStream.AutoSize = true;
            this.checkBoxUseBFStream.Location = new System.Drawing.Point(6, 84);
            this.checkBoxUseBFStream.Name = "checkBoxUseBFStream";
            this.checkBoxUseBFStream.Size = new System.Drawing.Size(155, 17);
            this.checkBoxUseBFStream.TabIndex = 6;
            this.checkBoxUseBFStream.Text = "Enable Brainflow Streaming";
            this.checkBoxUseBFStream.UseVisualStyleBackColor = true;
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRefresh.Location = new System.Drawing.Point(251, 46);
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
            this.buttonViewLogs.Location = new System.Drawing.Point(376, 108);
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
            this.buttonConfigureBoard.Location = new System.Drawing.Point(376, 76);
            this.buttonConfigureBoard.Name = "buttonConfigureBoard";
            this.buttonConfigureBoard.Size = new System.Drawing.Size(109, 23);
            this.buttonConfigureBoard.TabIndex = 4;
            this.buttonConfigureBoard.Text = "Configure Board";
            this.buttonConfigureBoard.UseVisualStyleBackColor = true;
            this.buttonConfigureBoard.Click += new System.EventHandler(this.buttonConfigureBoard_Click);
            // 
            // labelVersion
            // 
            this.labelVersion.Location = new System.Drawing.Point(9, 158);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(309, 23);
            this.labelVersion.TabIndex = 5;
            this.labelVersion.Text = "label4";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // checkBoxLogToFile
            // 
            this.checkBoxLogToFile.AutoSize = true;
            this.checkBoxLogToFile.Location = new System.Drawing.Point(376, 138);
            this.checkBoxLogToFile.Name = "checkBoxLogToFile";
            this.checkBoxLogToFile.Size = new System.Drawing.Size(75, 17);
            this.checkBoxLogToFile.TabIndex = 6;
            this.checkBoxLogToFile.Text = "Log to File";
            this.checkBoxLogToFile.UseVisualStyleBackColor = true;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(495, 182);
            this.Controls.Add(this.checkBoxLogToFile);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.buttonConfigureBoard);
            this.Controls.Add(this.buttonViewLogs);
            this.Controls.Add(this.groupBoxBoard);
            this.Controls.Add(this.buttonStart);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.Text = "brainHat Server";
            this.groupBoxBoard.ResumeLayout(false);
            this.groupBoxBoard.PerformLayout();
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
    }
}

