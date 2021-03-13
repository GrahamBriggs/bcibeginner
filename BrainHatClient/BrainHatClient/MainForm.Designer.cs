
namespace BrainHatClient
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.listViewDevices = new System.Windows.Forms.ListView();
            this.checkBoxLogging = new System.Windows.Forms.CheckBox();
            this.labelVersion = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // listViewDevices
            // 
            this.listViewDevices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewDevices.HideSelection = false;
            this.listViewDevices.Location = new System.Drawing.Point(12, 12);
            this.listViewDevices.Name = "listViewDevices";
            this.listViewDevices.Size = new System.Drawing.Size(399, 431);
            this.listViewDevices.TabIndex = 0;
            this.listViewDevices.UseCompatibleStateImageBehavior = false;
            this.listViewDevices.View = System.Windows.Forms.View.Details;
            // 
            // checkBoxLogging
            // 
            this.checkBoxLogging.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxLogging.AutoSize = true;
            this.checkBoxLogging.Location = new System.Drawing.Point(12, 457);
            this.checkBoxLogging.Name = "checkBoxLogging";
            this.checkBoxLogging.Size = new System.Drawing.Size(152, 17);
            this.checkBoxLogging.TabIndex = 2;
            this.checkBoxLogging.Text = "Log program activity to file.";
            this.checkBoxLogging.UseVisualStyleBackColor = true;
            this.checkBoxLogging.CheckedChanged += new System.EventHandler(this.checkBoxLogging_CheckedChanged);
            // 
            // labelVersion
            // 
            this.labelVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelVersion.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.labelVersion.Location = new System.Drawing.Point(170, 454);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(241, 23);
            this.labelVersion.TabIndex = 3;
            this.labelVersion.Text = "label1";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 486);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.checkBoxLogging);
            this.Controls.Add(this.listViewDevices);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "brainHat Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewDevices;
        private System.Windows.Forms.CheckBox checkBoxLogging;
        private System.Windows.Forms.Label labelVersion;
    }
}