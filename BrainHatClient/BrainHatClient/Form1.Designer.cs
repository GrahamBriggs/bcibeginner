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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
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
            this.labelAlpha = new System.Windows.Forms.Label();
            this.labelDataProcessing = new System.Windows.Forms.Label();
            this.buttonResetBlinkCounter = new System.Windows.Forms.Button();
            this.labelBlinkDetector = new System.Windows.Forms.Label();
            this.groupBoxOtherData = new System.Windows.Forms.GroupBox();
            this.labelOtherData = new System.Windows.Forms.Label();
            this.groupBoxLogging.SuspendLayout();
            this.groupBoxExgChannels.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBoxBlinkDetector.SuspendLayout();
            this.groupBoxOtherData.SuspendLayout();
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
            this.groupBoxLogging.Location = new System.Drawing.Point(12, 379);
            this.groupBoxLogging.Name = "groupBoxLogging";
            this.groupBoxLogging.Size = new System.Drawing.Size(1150, 341);
            this.groupBoxLogging.TabIndex = 0;
            this.groupBoxLogging.TabStop = false;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(885, 313);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Remote Log Level";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(621, 310);
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
            this.comboBoxLogLevelRemote.Location = new System.Drawing.Point(985, 310);
            this.comboBoxLogLevelRemote.Name = "comboBoxLogLevelRemote";
            this.comboBoxLogLevelRemote.Size = new System.Drawing.Size(147, 21);
            this.comboBoxLogLevelRemote.TabIndex = 2;
            // 
            // comboBoxLogLevel
            // 
            this.comboBoxLogLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLogLevel.FormattingEnabled = true;
            this.comboBoxLogLevel.Location = new System.Drawing.Point(692, 310);
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
            this.listViewLogs.Size = new System.Drawing.Size(1138, 292);
            this.listViewLogs.TabIndex = 0;
            this.listViewLogs.UseCompatibleStateImageBehavior = false;
            // 
            // groupBoxExgChannels
            // 
            this.groupBoxExgChannels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxExgChannels.Controls.Add(this.labelRecordingDuration);
            this.groupBoxExgChannels.Controls.Add(this.textBoxRecordingName);
            this.groupBoxExgChannels.Controls.Add(this.buttonStartRecording);
            this.groupBoxExgChannels.Controls.Add(this.labelExgData);
            this.groupBoxExgChannels.Location = new System.Drawing.Point(12, 12);
            this.groupBoxExgChannels.Name = "groupBoxExgChannels";
            this.groupBoxExgChannels.Size = new System.Drawing.Size(496, 361);
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
            this.labelExgData.Size = new System.Drawing.Size(80, 17);
            this.labelExgData.TabIndex = 0;
            this.labelExgData.Text = "EXG Data";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.labelAcelData);
            this.groupBox3.Location = new System.Drawing.Point(514, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(203, 94);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Accelerometer";
            // 
            // labelAcelData
            // 
            this.labelAcelData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAcelData.AutoSize = true;
            this.labelAcelData.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAcelData.Location = new System.Drawing.Point(6, 16);
            this.labelAcelData.Name = "labelAcelData";
            this.labelAcelData.Size = new System.Drawing.Size(125, 17);
            this.labelAcelData.TabIndex = 0;
            this.labelAcelData.Text = "Accelerometer";
            // 
            // groupBoxBlinkDetector
            // 
            this.groupBoxBlinkDetector.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxBlinkDetector.Controls.Add(this.labelAlpha);
            this.groupBoxBlinkDetector.Controls.Add(this.labelDataProcessing);
            this.groupBoxBlinkDetector.Controls.Add(this.buttonResetBlinkCounter);
            this.groupBoxBlinkDetector.Controls.Add(this.labelBlinkDetector);
            this.groupBoxBlinkDetector.Location = new System.Drawing.Point(723, 12);
            this.groupBoxBlinkDetector.Name = "groupBoxBlinkDetector";
            this.groupBoxBlinkDetector.Size = new System.Drawing.Size(439, 361);
            this.groupBoxBlinkDetector.TabIndex = 3;
            this.groupBoxBlinkDetector.TabStop = false;
            this.groupBoxBlinkDetector.Text = "Data Processing";
            // 
            // labelAlpha
            // 
            this.labelAlpha.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAlpha.AutoSize = true;
            this.labelAlpha.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAlpha.Location = new System.Drawing.Point(271, 16);
            this.labelAlpha.Name = "labelAlpha";
            this.labelAlpha.Size = new System.Drawing.Size(161, 17);
            this.labelAlpha.TabIndex = 3;
            this.labelAlpha.Text = "Seeking Alpha ...";
            // 
            // labelDataProcessing
            // 
            this.labelDataProcessing.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDataProcessing.AutoSize = true;
            this.labelDataProcessing.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDataProcessing.Location = new System.Drawing.Point(6, 68);
            this.labelDataProcessing.Name = "labelDataProcessing";
            this.labelDataProcessing.Size = new System.Drawing.Size(143, 17);
            this.labelDataProcessing.TabIndex = 2;
            this.labelDataProcessing.Text = "Data Processing";
            // 
            // buttonResetBlinkCounter
            // 
            this.buttonResetBlinkCounter.Location = new System.Drawing.Point(182, 14);
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
            this.labelBlinkDetector.Location = new System.Drawing.Point(6, 17);
            this.labelBlinkDetector.Name = "labelBlinkDetector";
            this.labelBlinkDetector.Size = new System.Drawing.Size(134, 17);
            this.labelBlinkDetector.TabIndex = 0;
            this.labelBlinkDetector.Text = "Blink Detector";
            // 
            // groupBoxOtherData
            // 
            this.groupBoxOtherData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxOtherData.Controls.Add(this.labelOtherData);
            this.groupBoxOtherData.Location = new System.Drawing.Point(517, 112);
            this.groupBoxOtherData.Name = "groupBoxOtherData";
            this.groupBoxOtherData.Size = new System.Drawing.Size(200, 261);
            this.groupBoxOtherData.TabIndex = 4;
            this.groupBoxOtherData.TabStop = false;
            this.groupBoxOtherData.Text = "Other Sample Properties";
            // 
            // labelOtherData
            // 
            this.labelOtherData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.labelOtherData.AutoSize = true;
            this.labelOtherData.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOtherData.Location = new System.Drawing.Point(6, 26);
            this.labelOtherData.Name = "labelOtherData";
            this.labelOtherData.Size = new System.Drawing.Size(98, 17);
            this.labelOtherData.TabIndex = 1;
            this.labelOtherData.Text = "Other Data";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1174, 732);
            this.Controls.Add(this.groupBoxOtherData);
            this.Controls.Add(this.groupBoxBlinkDetector);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBoxExgChannels);
            this.Controls.Add(this.groupBoxLogging);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "brainHat Data Viewer for Windows";
            this.groupBoxLogging.ResumeLayout(false);
            this.groupBoxLogging.PerformLayout();
            this.groupBoxExgChannels.ResumeLayout(false);
            this.groupBoxExgChannels.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBoxBlinkDetector.ResumeLayout(false);
            this.groupBoxBlinkDetector.PerformLayout();
            this.groupBoxOtherData.ResumeLayout(false);
            this.groupBoxOtherData.PerformLayout();
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
        private System.Windows.Forms.Label labelDataProcessing;
        private System.Windows.Forms.GroupBox groupBoxOtherData;
        private System.Windows.Forms.Label labelOtherData;
        private System.Windows.Forms.Label labelAlpha;
    }
}

