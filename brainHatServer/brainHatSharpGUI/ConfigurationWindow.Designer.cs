
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
            this.groupBoxConfiguration = new System.Windows.Forms.GroupBox();
            this.listViewConfig = new System.Windows.Forms.ListView();
            this.buttonSignalTest = new System.Windows.Forms.Button();
            this.comboBoxSignalTest = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonChannelDefaults = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBoxConfiguration.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxConfiguration
            // 
            this.groupBoxConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxConfiguration.Controls.Add(this.listViewConfig);
            this.groupBoxConfiguration.Location = new System.Drawing.Point(13, 13);
            this.groupBoxConfiguration.Name = "groupBoxConfiguration";
            this.groupBoxConfiguration.Size = new System.Drawing.Size(388, 500);
            this.groupBoxConfiguration.TabIndex = 0;
            this.groupBoxConfiguration.TabStop = false;
            this.groupBoxConfiguration.Text = "Current Configuration";
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
            this.listViewConfig.Size = new System.Drawing.Size(376, 475);
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
            this.buttonSignalTest.Text = "Start Test Signal";
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
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.buttonChannelDefaults);
            this.groupBox1.Location = new System.Drawing.Point(420, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(473, 500);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Channel Configuration";
            // 
            // buttonChannelDefaults
            // 
            this.buttonChannelDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChannelDefaults.Location = new System.Drawing.Point(353, 471);
            this.buttonChannelDefaults.Name = "buttonChannelDefaults";
            this.buttonChannelDefaults.Size = new System.Drawing.Size(110, 23);
            this.buttonChannelDefaults.TabIndex = 4;
            this.buttonChannelDefaults.Text = "Reset to Default";
            this.buttonChannelDefaults.UseVisualStyleBackColor = true;
            this.buttonChannelDefaults.Click += new System.EventHandler(this.buttonChannelDefaults_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.buttonSignalTest);
            this.groupBox2.Controls.Add(this.comboBoxSignalTest);
            this.groupBox2.Location = new System.Drawing.Point(13, 529);
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
            // ConfigurationWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(905, 629);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxConfiguration);
            this.Name = "ConfigurationWindow";
            this.Text = "ConfigurationWindow";
            this.groupBoxConfiguration.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
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
    }
}