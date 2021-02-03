
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
            this.label1 = new System.Windows.Forms.Label();
            this.radioButtonDaisy = new System.Windows.Forms.RadioButton();
            this.radioButtonCyton = new System.Windows.Forms.RadioButton();
            this.buttonViewLogs = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.groupBoxBoard.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonStart
            // 
            this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStart.Location = new System.Drawing.Point(370, 26);
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
            this.comboBoxComPort.Location = new System.Drawing.Point(102, 78);
            this.comboBoxComPort.Name = "comboBoxComPort";
            this.comboBoxComPort.Size = new System.Drawing.Size(124, 21);
            this.comboBoxComPort.TabIndex = 1;
            // 
            // groupBoxBoard
            // 
            this.groupBoxBoard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxBoard.Controls.Add(this.buttonRefresh);
            this.groupBoxBoard.Controls.Add(this.label1);
            this.groupBoxBoard.Controls.Add(this.radioButtonDaisy);
            this.groupBoxBoard.Controls.Add(this.radioButtonCyton);
            this.groupBoxBoard.Controls.Add(this.comboBoxComPort);
            this.groupBoxBoard.Location = new System.Drawing.Point(12, 12);
            this.groupBoxBoard.Name = "groupBoxBoard";
            this.groupBoxBoard.Size = new System.Drawing.Size(331, 109);
            this.groupBoxBoard.TabIndex = 2;
            this.groupBoxBoard.TabStop = false;
            this.groupBoxBoard.Text = "Connect to Board";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 81);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "COM Port";
            // 
            // radioButtonDaisy
            // 
            this.radioButtonDaisy.AutoSize = true;
            this.radioButtonDaisy.Location = new System.Drawing.Point(18, 46);
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
            this.radioButtonCyton.Location = new System.Drawing.Point(18, 20);
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
            this.buttonViewLogs.Location = new System.Drawing.Point(370, 58);
            this.buttonViewLogs.Name = "buttonViewLogs";
            this.buttonViewLogs.Size = new System.Drawing.Size(109, 23);
            this.buttonViewLogs.TabIndex = 3;
            this.buttonViewLogs.Text = "View Logs";
            this.buttonViewLogs.UseVisualStyleBackColor = true;
            this.buttonViewLogs.Click += new System.EventHandler(this.buttonViewLogs_Click);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRefresh.Location = new System.Drawing.Point(241, 76);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(71, 23);
            this.buttonRefresh.TabIndex = 5;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(505, 126);
            this.Controls.Add(this.buttonViewLogs);
            this.Controls.Add(this.groupBoxBoard);
            this.Controls.Add(this.buttonStart);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.Text = "brainHat Server";
            this.groupBoxBoard.ResumeLayout(false);
            this.groupBoxBoard.PerformLayout();
            this.ResumeLayout(false);

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
    }
}

