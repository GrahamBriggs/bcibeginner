
namespace brainHatLit
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
            this.textBoxHostName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonStart = new System.Windows.Forms.Button();
            this.labelConnectionStatus = new System.Windows.Forms.Label();
            this.checkBoxLightsAuto = new System.Windows.Forms.CheckBox();
            this.checkBoxHapticMotor = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxHostName
            // 
            this.textBoxHostName.Location = new System.Drawing.Point(208, 14);
            this.textBoxHostName.Name = "textBoxHostName";
            this.textBoxHostName.Size = new System.Drawing.Size(109, 20);
            this.textBoxHostName.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(161, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Server:";
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(333, 11);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 2;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // labelConnectionStatus
            // 
            this.labelConnectionStatus.AutoSize = true;
            this.labelConnectionStatus.Location = new System.Drawing.Point(154, 46);
            this.labelConnectionStatus.Name = "labelConnectionStatus";
            this.labelConnectionStatus.Size = new System.Drawing.Size(78, 13);
            this.labelConnectionStatus.TabIndex = 3;
            this.labelConnectionStatus.Text = "Not connected";
            // 
            // checkBoxLightsAuto
            // 
            this.checkBoxLightsAuto.AutoSize = true;
            this.checkBoxLightsAuto.Location = new System.Drawing.Point(198, 107);
            this.checkBoxLightsAuto.Name = "checkBoxLightsAuto";
            this.checkBoxLightsAuto.Size = new System.Drawing.Size(104, 17);
            this.checkBoxLightsAuto.TabIndex = 4;
            this.checkBoxLightsAuto.Text = "Automatic Lights";
            this.checkBoxLightsAuto.UseVisualStyleBackColor = true;
            this.checkBoxLightsAuto.CheckedChanged += new System.EventHandler(this.checkBoxLightsAuto_CheckedChanged);
            // 
            // checkBoxHapticMotor
            // 
            this.checkBoxHapticMotor.AutoSize = true;
            this.checkBoxHapticMotor.Location = new System.Drawing.Point(198, 130);
            this.checkBoxHapticMotor.Name = "checkBoxHapticMotor";
            this.checkBoxHapticMotor.Size = new System.Drawing.Size(87, 17);
            this.checkBoxHapticMotor.TabIndex = 5;
            this.checkBoxHapticMotor.Text = "Haptic Motor";
            this.checkBoxHapticMotor.UseVisualStyleBackColor = true;
            this.checkBoxHapticMotor.CheckedChanged += new System.EventHandler(this.checkBoxHapticMotor_CheckedChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.InitialImage")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 14);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(133, 133);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 159);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.checkBoxHapticMotor);
            this.Controls.Add(this.checkBoxLightsAuto);
            this.Controls.Add(this.labelConnectionStatus);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxHostName);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "brainHat Lit";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxHostName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Label labelConnectionStatus;
        private System.Windows.Forms.CheckBox checkBoxLightsAuto;
        private System.Windows.Forms.CheckBox checkBoxHapticMotor;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

