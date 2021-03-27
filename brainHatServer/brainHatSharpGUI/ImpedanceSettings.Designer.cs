
namespace brainHatSharpGUI
{
    partial class ImpedanceSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImpedanceSettings));
            this.comboBoxLlofP = new System.Windows.Forms.ComboBox();
            this.comboBoxLlofN = new System.Windows.Forms.ComboBox();
            this.labelChannel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonSetImpedance = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBoxLlofP
            // 
            this.comboBoxLlofP.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLlofP.FormattingEnabled = true;
            this.comboBoxLlofP.Location = new System.Drawing.Point(119, 78);
            this.comboBoxLlofP.Name = "comboBoxLlofP";
            this.comboBoxLlofP.Size = new System.Drawing.Size(227, 21);
            this.comboBoxLlofP.TabIndex = 1;
            // 
            // comboBoxLlofN
            // 
            this.comboBoxLlofN.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLlofN.FormattingEnabled = true;
            this.comboBoxLlofN.Location = new System.Drawing.Point(119, 121);
            this.comboBoxLlofN.Name = "comboBoxLlofN";
            this.comboBoxLlofN.Size = new System.Drawing.Size(227, 21);
            this.comboBoxLlofN.TabIndex = 2;
            // 
            // labelChannel
            // 
            this.labelChannel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelChannel.Location = new System.Drawing.Point(21, 16);
            this.labelChannel.Name = "labelChannel";
            this.labelChannel.Size = new System.Drawing.Size(284, 48);
            this.labelChannel.TabIndex = 7;
            this.labelChannel.Text = "Channel";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "LLOF_SENSP";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 124);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "LLOF_SENSN";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // buttonSetImpedance
            // 
            this.buttonSetImpedance.Location = new System.Drawing.Point(152, 164);
            this.buttonSetImpedance.Name = "buttonSetImpedance";
            this.buttonSetImpedance.Size = new System.Drawing.Size(194, 23);
            this.buttonSetImpedance.TabIndex = 13;
            this.buttonSetImpedance.Text = "Set Impedance(s)";
            this.buttonSetImpedance.UseVisualStyleBackColor = true;
            this.buttonSetImpedance.Click += new System.EventHandler(this.buttonSetImpedance_Click);
            // 
            // ImpedanceSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(373, 261);
            this.Controls.Add(this.buttonSetImpedance);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelChannel);
            this.Controls.Add(this.comboBoxLlofN);
            this.Controls.Add(this.comboBoxLlofP);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(389, 300);
            this.MinimumSize = new System.Drawing.Size(389, 300);
            this.Name = "ImpedanceSettings";
            this.Text = "Impedance Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxLlofP;
        private System.Windows.Forms.ComboBox comboBoxLlofN;
        private System.Windows.Forms.Label labelChannel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonSetImpedance;
    }
}