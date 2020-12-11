namespace BrainHatClient
{
    partial class Form1
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
            this.groupBoxLogging = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxLogLevelRemote = new System.Windows.Forms.ComboBox();
            this.comboBoxLogLevel = new System.Windows.Forms.ComboBox();
            this.listViewLogs = new System.Windows.Forms.ListView();
            this.groupBoxExgChannels = new System.Windows.Forms.GroupBox();
            this.labelRecordingDuration = new System.Windows.Forms.Label();
            this.textBoxRecordingName = new System.Windows.Forms.TextBox();
            this.buttonStartRecording = new System.Windows.Forms.Button();
            this.labelExgData = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.labelAcelData = new System.Windows.Forms.Label();
            this.groupBoxBlinkDetector = new System.Windows.Forms.GroupBox();
            this.buttonResetBlinkCounter = new System.Windows.Forms.Button();
            this.labelBlinkDetector = new System.Windows.Forms.Label();
            this.groupBoxLogging.SuspendLayout();
            this.groupBoxExgChannels.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBoxBlinkDetector.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxLogging
            // 
            this.groupBoxLogging.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLogging.Controls.Add(this.label3);
            this.groupBoxLogging.Controls.Add(this.label2);
            this.groupBoxLogging.Controls.Add(this.comboBoxLogLevelRemote);
            this.groupBoxLogging.Controls.Add(this.comboBoxLogLevel);
            this.groupBoxLogging.Controls.Add(this.listViewLogs);
            this.groupBoxLogging.Location = new System.Drawing.Point(12, 367);
            this.groupBoxLogging.Name = "groupBoxLogging";
            this.groupBoxLogging.Size = new System.Drawing.Size(1069, 341);
            this.groupBoxLogging.TabIndex = 0;
            this.groupBoxLogging.TabStop = false;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(804, 313);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Remote Log Level";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(540, 310);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Log Level";
            // 
            // comboBoxLogLevelRemote
            // 
            this.comboBoxLogLevelRemote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLogLevelRemote.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLogLevelRemote.FormattingEnabled = true;
            this.comboBoxLogLevelRemote.Location = new System.Drawing.Point(904, 310);
            this.comboBoxLogLevelRemote.Name = "comboBoxLogLevelRemote";
            this.comboBoxLogLevelRemote.Size = new System.Drawing.Size(147, 21);
            this.comboBoxLogLevelRemote.TabIndex = 2;
            // 
            // comboBoxLogLevel
            // 
            this.comboBoxLogLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLogLevel.FormattingEnabled = true;
            this.comboBoxLogLevel.Location = new System.Drawing.Point(611, 310);
            this.comboBoxLogLevel.Name = "comboBoxLogLevel";
            this.comboBoxLogLevel.Size = new System.Drawing.Size(147, 21);
            this.comboBoxLogLevel.TabIndex = 1;
            // 
            // listViewLogs
            // 
            this.listViewLogs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewLogs.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewLogs.HideSelection = false;
            this.listViewLogs.Location = new System.Drawing.Point(6, 12);
            this.listViewLogs.MultiSelect = false;
            this.listViewLogs.Name = "listViewLogs";
            this.listViewLogs.Size = new System.Drawing.Size(1045, 292);
            this.listViewLogs.TabIndex = 0;
            this.listViewLogs.UseCompatibleStateImageBehavior = false;
            // 
            // groupBoxExgChannels
            // 
            this.groupBoxExgChannels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxExgChannels.Controls.Add(this.labelRecordingDuration);
            this.groupBoxExgChannels.Controls.Add(this.textBoxRecordingName);
            this.groupBoxExgChannels.Controls.Add(this.buttonStartRecording);
            this.groupBoxExgChannels.Controls.Add(this.labelExgData);
            this.groupBoxExgChannels.Location = new System.Drawing.Point(12, 12);
            this.groupBoxExgChannels.Name = "groupBoxExgChannels";
            this.groupBoxExgChannels.Size = new System.Drawing.Size(786, 349);
            this.groupBoxExgChannels.TabIndex = 1;
            this.groupBoxExgChannels.TabStop = false;
            this.groupBoxExgChannels.Text = "Exg Channel Data";
            // 
            // labelRecordingDuration
            // 
            this.labelRecordingDuration.AutoSize = true;
            this.labelRecordingDuration.Location = new System.Drawing.Point(249, 31);
            this.labelRecordingDuration.Name = "labelRecordingDuration";
            this.labelRecordingDuration.Size = new System.Drawing.Size(88, 13);
            this.labelRecordingDuration.TabIndex = 3;
            this.labelRecordingDuration.Text = "recDurationLabel";
            // 
            // textBoxRecordingName
            // 
            this.textBoxRecordingName.Location = new System.Drawing.Point(115, 28);
            this.textBoxRecordingName.Name = "textBoxRecordingName";
            this.textBoxRecordingName.Size = new System.Drawing.Size(128, 20);
            this.textBoxRecordingName.TabIndex = 2;
            // 
            // buttonStartRecording
            // 
            this.buttonStartRecording.Location = new System.Drawing.Point(9, 26);
            this.buttonStartRecording.Name = "buttonStartRecording";
            this.buttonStartRecording.Size = new System.Drawing.Size(100, 23);
            this.buttonStartRecording.TabIndex = 1;
            this.buttonStartRecording.Text = "Start Recording";
            this.buttonStartRecording.UseVisualStyleBackColor = true;
            this.buttonStartRecording.Click += new System.EventHandler(this.buttonStartRecording_Click);
            // 
            // labelExgData
            // 
            this.labelExgData.AutoSize = true;
            this.labelExgData.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelExgData.Location = new System.Drawing.Point(6, 68);
            this.labelExgData.Name = "labelExgData";
            this.labelExgData.Size = new System.Drawing.Size(62, 17);
            this.labelExgData.TabIndex = 0;
            this.labelExgData.Text = "label1";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.labelAcelData);
            this.groupBox3.Location = new System.Drawing.Point(810, 195);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(271, 166);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Accelerometer Data";
            // 
            // labelAcelData
            // 
            this.labelAcelData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAcelData.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAcelData.Location = new System.Drawing.Point(6, 16);
            this.labelAcelData.Name = "labelAcelData";
            this.labelAcelData.Size = new System.Drawing.Size(259, 147);
            this.labelAcelData.TabIndex = 0;
            this.labelAcelData.Text = "label4";
            // 
            // groupBoxBlinkDetector
            // 
            this.groupBoxBlinkDetector.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxBlinkDetector.Controls.Add(this.buttonResetBlinkCounter);
            this.groupBoxBlinkDetector.Controls.Add(this.labelBlinkDetector);
            this.groupBoxBlinkDetector.Location = new System.Drawing.Point(810, 12);
            this.groupBoxBlinkDetector.Name = "groupBoxBlinkDetector";
            this.groupBoxBlinkDetector.Size = new System.Drawing.Size(271, 177);
            this.groupBoxBlinkDetector.TabIndex = 3;
            this.groupBoxBlinkDetector.TabStop = false;
            this.groupBoxBlinkDetector.Text = "BlinkDetector";
            // 
            // buttonResetBlinkCounter
            // 
            this.buttonResetBlinkCounter.Location = new System.Drawing.Point(190, 138);
            this.buttonResetBlinkCounter.Name = "buttonResetBlinkCounter";
            this.buttonResetBlinkCounter.Size = new System.Drawing.Size(75, 23);
            this.buttonResetBlinkCounter.TabIndex = 1;
            this.buttonResetBlinkCounter.Text = "Reset";
            this.buttonResetBlinkCounter.UseVisualStyleBackColor = true;
            this.buttonResetBlinkCounter.Click += new System.EventHandler(this.buttonResetBlinkCounter_Click);
            // 
            // labelBlinkDetector
            // 
            this.labelBlinkDetector.AutoSize = true;
            this.labelBlinkDetector.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBlinkDetector.Location = new System.Drawing.Point(6, 16);
            this.labelBlinkDetector.Name = "labelBlinkDetector";
            this.labelBlinkDetector.Size = new System.Drawing.Size(62, 17);
            this.labelBlinkDetector.TabIndex = 0;
            this.labelBlinkDetector.Text = "label4";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1093, 720);
            this.Controls.Add(this.groupBoxBlinkDetector);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBoxExgChannels);
            this.Controls.Add(this.groupBoxLogging);
            this.Name = "Form1";
            this.Text = "brainHat Data Viewer for Windows";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.groupBoxLogging.ResumeLayout(false);
            this.groupBoxLogging.PerformLayout();
            this.groupBoxExgChannels.ResumeLayout(false);
            this.groupBoxExgChannels.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBoxBlinkDetector.ResumeLayout(false);
            this.groupBoxBlinkDetector.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxLogging;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxLogLevelRemote;
        private System.Windows.Forms.ComboBox comboBoxLogLevel;
        private System.Windows.Forms.ListView listViewLogs;
        private System.Windows.Forms.GroupBox groupBoxExgChannels;
        private System.Windows.Forms.Label labelExgData;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label labelAcelData;
        private System.Windows.Forms.GroupBox groupBoxBlinkDetector;
        private System.Windows.Forms.Label labelBlinkDetector;
        private System.Windows.Forms.Button buttonResetBlinkCounter;
        private System.Windows.Forms.TextBox textBoxRecordingName;
        private System.Windows.Forms.Button buttonStartRecording;
        private System.Windows.Forms.Label labelRecordingDuration;
    }
}

