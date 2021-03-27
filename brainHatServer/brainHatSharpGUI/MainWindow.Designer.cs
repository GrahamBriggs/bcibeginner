
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
            this.groupBoxRunStatus = new System.Windows.Forms.GroupBox();
            this.pictureBoxStatus = new System.Windows.Forms.PictureBox();
            this.labelDataStatus = new System.Windows.Forms.Label();
            this.labelSrbStatus = new System.Windows.Forms.Label();
            this.labelRunStatus = new System.Windows.Forms.Label();
            this.checkBoxSRB = new System.Windows.Forms.CheckBox();
            this.labelIpPort = new System.Windows.Forms.Label();
            this.labelIpAddress = new System.Windows.Forms.Label();
            this.textBoxIpPort = new System.Windows.Forms.TextBox();
            this.textBoxIpAddress = new System.Windows.Forms.TextBox();
            this.checkBoxUseBFStream = new System.Windows.Forms.CheckBox();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.labelComPort = new System.Windows.Forms.Label();
            this.radioButtonDaisy = new System.Windows.Forms.RadioButton();
            this.radioButtonCyton = new System.Windows.Forms.RadioButton();
            this.buttonViewLogs = new System.Windows.Forms.Button();
            this.buttonConfigureBoard = new System.Windows.Forms.Button();
            this.labelVersion = new System.Windows.Forms.Label();
            this.checkBoxLogToFile = new System.Windows.Forms.CheckBox();
            this.groupBoxBoard.SuspendLayout();
            this.groupBoxRunStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonStart
            // 
            this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonStart.Location = new System.Drawing.Point(432, 32);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(136, 29);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Iniciar servidor";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // comboBoxComPort
            // 
            this.comboBoxComPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxComPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxComPort.FormattingEnabled = true;
            this.comboBoxComPort.Location = new System.Drawing.Point(107, 26);
            this.comboBoxComPort.Name = "comboBoxComPort";
            this.comboBoxComPort.Size = new System.Drawing.Size(183, 24);
            this.comboBoxComPort.TabIndex = 1;
            // 
            // groupBoxBoard
            // 
            this.groupBoxBoard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxBoard.Controls.Add(this.checkBoxSRB);
            this.groupBoxBoard.Controls.Add(this.labelIpPort);
            this.groupBoxBoard.Controls.Add(this.labelIpAddress);
            this.groupBoxBoard.Controls.Add(this.textBoxIpPort);
            this.groupBoxBoard.Controls.Add(this.textBoxIpAddress);
            this.groupBoxBoard.Controls.Add(this.checkBoxUseBFStream);
            this.groupBoxBoard.Controls.Add(this.buttonRefresh);
            this.groupBoxBoard.Controls.Add(this.labelComPort);
            this.groupBoxBoard.Controls.Add(this.radioButtonDaisy);
            this.groupBoxBoard.Controls.Add(this.radioButtonCyton);
            this.groupBoxBoard.Controls.Add(this.comboBoxComPort);
            this.groupBoxBoard.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxBoard.Location = new System.Drawing.Point(10, 12);
            this.groupBoxBoard.Name = "groupBoxBoard";
            this.groupBoxBoard.Size = new System.Drawing.Size(405, 182);
            this.groupBoxBoard.TabIndex = 2;
            this.groupBoxBoard.TabStop = false;
            this.groupBoxBoard.Text = "Configuración de conexión";
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
            this.groupBoxRunStatus.Size = new System.Drawing.Size(405, 182);
            this.groupBoxRunStatus.TabIndex = 7;
            this.groupBoxRunStatus.TabStop = false;
            // 
            // pictureBoxStatus
            // 
            this.pictureBoxStatus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBoxStatus.Image = global::brainHatSharpGUI.Properties.Resources.yellowLight;
            this.pictureBoxStatus.InitialImage = null;
            this.pictureBoxStatus.Location = new System.Drawing.Point(14, 20);
            this.pictureBoxStatus.Name = "pictureBoxStatus";
            this.pictureBoxStatus.Size = new System.Drawing.Size(48, 48);
            this.pictureBoxStatus.TabIndex = 3;
            this.pictureBoxStatus.TabStop = false;
            // 
            // labelDataStatus
            // 
            this.labelDataStatus.AutoSize = true;
            this.labelDataStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDataStatus.Location = new System.Drawing.Point(87, 67);
            this.labelDataStatus.Name = "labelDataStatus";
            this.labelDataStatus.Size = new System.Drawing.Size(56, 20);
            this.labelDataStatus.TabIndex = 2;
            this.labelDataStatus.Text = "Status";
            // 
            // labelSrbStatus
            // 
            this.labelSrbStatus.AutoSize = true;
            this.labelSrbStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSrbStatus.Location = new System.Drawing.Point(87, 99);
            this.labelSrbStatus.Name = "labelSrbStatus";
            this.labelSrbStatus.Size = new System.Drawing.Size(56, 20);
            this.labelSrbStatus.TabIndex = 1;
            this.labelSrbStatus.Text = "Status";
            // 
            // labelRunStatus
            // 
            this.labelRunStatus.AutoSize = true;
            this.labelRunStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRunStatus.Location = new System.Drawing.Point(74, 33);
            this.labelRunStatus.Name = "labelRunStatus";
            this.labelRunStatus.Size = new System.Drawing.Size(60, 24);
            this.labelRunStatus.TabIndex = 0;
            this.labelRunStatus.Text = "Status";
            // 
            // checkBoxSRB
            // 
            this.checkBoxSRB.AutoSize = true;
            this.checkBoxSRB.Location = new System.Drawing.Point(107, 64);
            this.checkBoxSRB.Name = "checkBoxSRB";
            this.checkBoxSRB.Size = new System.Drawing.Size(217, 20);
            this.checkBoxSRB.TabIndex = 11;
            this.checkBoxSRB.Text = "Comienza con SRB1 conectado";
            this.checkBoxSRB.UseVisualStyleBackColor = true;
            // 
            // labelIpPort
            // 
            this.labelIpPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelIpPort.Location = new System.Drawing.Point(158, 155);
            this.labelIpPort.Name = "labelIpPort";
            this.labelIpPort.Size = new System.Drawing.Size(125, 19);
            this.labelIpPort.TabIndex = 10;
            this.labelIpPort.Text = "Puerto";
            this.labelIpPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelIpAddress
            // 
            this.labelIpAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelIpAddress.Location = new System.Drawing.Point(155, 129);
            this.labelIpAddress.Name = "labelIpAddress";
            this.labelIpAddress.Size = new System.Drawing.Size(125, 19);
            this.labelIpAddress.TabIndex = 9;
            this.labelIpAddress.Text = "Dirección IP";
            this.labelIpAddress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxIpPort
            // 
            this.textBoxIpPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxIpPort.Location = new System.Drawing.Point(289, 153);
            this.textBoxIpPort.Name = "textBoxIpPort";
            this.textBoxIpPort.Size = new System.Drawing.Size(106, 22);
            this.textBoxIpPort.TabIndex = 8;
            // 
            // textBoxIpAddress
            // 
            this.textBoxIpAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxIpAddress.Location = new System.Drawing.Point(289, 127);
            this.textBoxIpAddress.Name = "textBoxIpAddress";
            this.textBoxIpAddress.Size = new System.Drawing.Size(106, 22);
            this.textBoxIpAddress.TabIndex = 7;
            // 
            // checkBoxUseBFStream
            // 
            this.checkBoxUseBFStream.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxUseBFStream.AutoSize = true;
            this.checkBoxUseBFStream.Location = new System.Drawing.Point(14, 104);
            this.checkBoxUseBFStream.Name = "checkBoxUseBFStream";
            this.checkBoxUseBFStream.Size = new System.Drawing.Size(191, 20);
            this.checkBoxUseBFStream.TabIndex = 6;
            this.checkBoxUseBFStream.Text = "Enable Brainflow Streaming";
            this.checkBoxUseBFStream.UseVisualStyleBackColor = true;
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRefresh.Location = new System.Drawing.Point(300, 23);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(95, 29);
            this.buttonRefresh.TabIndex = 5;
            this.buttonRefresh.Text = "Actualizar";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // labelComPort
            // 
            this.labelComPort.Location = new System.Drawing.Point(17, 29);
            this.labelComPort.Name = "labelComPort";
            this.labelComPort.Size = new System.Drawing.Size(90, 20);
            this.labelComPort.TabIndex = 4;
            this.labelComPort.Text = "Puerto COM";
            // 
            // radioButtonDaisy
            // 
            this.radioButtonDaisy.AutoSize = true;
            this.radioButtonDaisy.Location = new System.Drawing.Point(50, 148);
            this.radioButtonDaisy.Name = "radioButtonDaisy";
            this.radioButtonDaisy.Size = new System.Drawing.Size(102, 20);
            this.radioButtonDaisy.TabIndex = 3;
            this.radioButtonDaisy.TabStop = true;
            this.radioButtonDaisy.Text = "Cyton+Daisy";
            this.radioButtonDaisy.UseVisualStyleBackColor = true;
            this.radioButtonDaisy.Visible = false;
            // 
            // radioButtonCyton
            // 
            this.radioButtonCyton.AutoSize = true;
            this.radioButtonCyton.Location = new System.Drawing.Point(50, 122);
            this.radioButtonCyton.Name = "radioButtonCyton";
            this.radioButtonCyton.Size = new System.Drawing.Size(60, 20);
            this.radioButtonCyton.TabIndex = 2;
            this.radioButtonCyton.TabStop = true;
            this.radioButtonCyton.Text = "Cyton";
            this.radioButtonCyton.UseVisualStyleBackColor = true;
            this.radioButtonCyton.Visible = false;
            // 
            // buttonViewLogs
            // 
            this.buttonViewLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonViewLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonViewLogs.Location = new System.Drawing.Point(432, 121);
            this.buttonViewLogs.Name = "buttonViewLogs";
            this.buttonViewLogs.Size = new System.Drawing.Size(136, 29);
            this.buttonViewLogs.TabIndex = 3;
            this.buttonViewLogs.Text = "Ver consola";
            this.buttonViewLogs.UseVisualStyleBackColor = true;
            this.buttonViewLogs.Click += new System.EventHandler(this.buttonViewLogs_Click);
            // 
            // buttonConfigureBoard
            // 
            this.buttonConfigureBoard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConfigureBoard.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonConfigureBoard.Location = new System.Drawing.Point(432, 76);
            this.buttonConfigureBoard.Name = "buttonConfigureBoard";
            this.buttonConfigureBoard.Size = new System.Drawing.Size(136, 29);
            this.buttonConfigureBoard.TabIndex = 4;
            this.buttonConfigureBoard.Text = "Configurar equipo";
            this.buttonConfigureBoard.UseVisualStyleBackColor = true;
            this.buttonConfigureBoard.Click += new System.EventHandler(this.buttonConfigureBoard_Click);
            // 
            // labelVersion
            // 
            this.labelVersion.Location = new System.Drawing.Point(7, 201);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(309, 23);
            this.labelVersion.TabIndex = 5;
            this.labelVersion.Text = "label4";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // checkBoxLogToFile
            // 
            this.checkBoxLogToFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxLogToFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxLogToFile.Location = new System.Drawing.Point(432, 156);
            this.checkBoxLogToFile.Name = "checkBoxLogToFile";
            this.checkBoxLogToFile.Size = new System.Drawing.Size(134, 43);
            this.checkBoxLogToFile.TabIndex = 6;
            this.checkBoxLogToFile.Text = "Guardar consola en archivo";
            this.checkBoxLogToFile.UseVisualStyleBackColor = true;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(578, 231);
            this.Controls.Add(this.groupBoxRunStatus);
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
            this.groupBoxRunStatus.ResumeLayout(false);
            this.groupBoxRunStatus.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStatus)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.ComboBox comboBoxComPort;
        private System.Windows.Forms.GroupBox groupBoxBoard;
        private System.Windows.Forms.RadioButton radioButtonDaisy;
        private System.Windows.Forms.RadioButton radioButtonCyton;
        private System.Windows.Forms.Label labelComPort;
        private System.Windows.Forms.Button buttonViewLogs;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.Button buttonConfigureBoard;
        private System.Windows.Forms.Label labelIpPort;
        private System.Windows.Forms.Label labelIpAddress;
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

