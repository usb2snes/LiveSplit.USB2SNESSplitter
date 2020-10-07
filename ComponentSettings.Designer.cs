namespace LiveSplit.UI.Components
{
    partial class ComponentSettings
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label lblDevice;
            System.Windows.Forms.Label lblConfig;
            System.Windows.Forms.Button btnBrowse;
            System.Windows.Forms.Button btnDetect;
            this.txtDevice = new System.Windows.Forms.TextBox();
            this.txtConfigFile = new System.Windows.Forms.TextBox();
            this.chkReset = new System.Windows.Forms.CheckBox();
            this.errorPanel = new System.Windows.Forms.TableLayoutPanel();
            this.errorIcon = new System.Windows.Forms.PictureBox();
            this.errorMessage = new System.Windows.Forms.Label();
            this.chkStatus = new System.Windows.Forms.CheckBox();
            lblDevice = new System.Windows.Forms.Label();
            lblConfig = new System.Windows.Forms.Label();
            btnBrowse = new System.Windows.Forms.Button();
            btnDetect = new System.Windows.Forms.Button();
            this.errorPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // lblDevice
            // 
            lblDevice.AutoSize = true;
            lblDevice.Location = new System.Drawing.Point(10, 20);
            lblDevice.Name = "lblDevice";
            lblDevice.Size = new System.Drawing.Size(44, 13);
            lblDevice.TabIndex = 0;
            lblDevice.Text = "Device:";
            // 
            // lblConfig
            // 
            lblConfig.AutoSize = true;
            lblConfig.Location = new System.Drawing.Point(10, 46);
            lblConfig.Name = "lblConfig";
            lblConfig.Size = new System.Drawing.Size(56, 13);
            lblConfig.TabIndex = 0;
            lblConfig.Text = "Config file:";
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new System.Drawing.Point(433, 43);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new System.Drawing.Size(33, 20);
            btnBrowse.TabIndex = 4;
            btnBrowse.Text = "...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnDetect
            // 
            btnDetect.Location = new System.Drawing.Point(178, 15);
            btnDetect.Name = "btnDetect";
            btnDetect.Size = new System.Drawing.Size(75, 23);
            btnDetect.TabIndex = 1;
            btnDetect.Text = "Autodetect";
            btnDetect.UseVisualStyleBackColor = true;
            btnDetect.Click += new System.EventHandler(this.btnDetect_Click);
            // 
            // txtDevice
            // 
            this.txtDevice.Location = new System.Drawing.Point(72, 17);
            this.txtDevice.Name = "txtDevice";
            this.txtDevice.Size = new System.Drawing.Size(100, 20);
            this.txtDevice.TabIndex = 0;
            // 
            // txtConfigFile
            // 
            this.txtConfigFile.Location = new System.Drawing.Point(72, 43);
            this.txtConfigFile.Name = "txtConfigFile";
            this.txtConfigFile.Size = new System.Drawing.Size(355, 20);
            this.txtConfigFile.TabIndex = 3;
            // 
            // chkReset
            // 
            this.chkReset.AutoSize = true;
            this.chkReset.Location = new System.Drawing.Point(72, 69);
            this.chkReset.Name = "chkReset";
            this.chkReset.Size = new System.Drawing.Size(156, 17);
            this.chkReset.TabIndex = 5;
            this.chkReset.Text = "Reset SNES on Timer reset";
            this.chkReset.UseVisualStyleBackColor = true;
            // 
            // errorPanel
            // 
            this.errorPanel.AutoSize = true;
            this.errorPanel.ColumnCount = 2;
            this.errorPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.errorPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.errorPanel.Controls.Add(this.errorIcon, 0, 0);
            this.errorPanel.Controls.Add(this.errorMessage, 1, 0);
            this.errorPanel.Location = new System.Drawing.Point(13, 115);
            this.errorPanel.Name = "errorPanel";
            this.errorPanel.RowCount = 1;
            this.errorPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.errorPanel.Size = new System.Drawing.Size(453, 38);
            this.errorPanel.TabIndex = 0;
            this.errorPanel.Visible = false;
            // 
            // errorIcon
            // 
            this.errorIcon.Location = new System.Drawing.Point(3, 3);
            this.errorIcon.Name = "errorIcon";
            this.errorIcon.Size = new System.Drawing.Size(32, 32);
            this.errorIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.errorIcon.TabIndex = 2;
            this.errorIcon.TabStop = false;
            // 
            // errorMessage
            // 
            this.errorMessage.AutoSize = true;
            this.errorMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.errorMessage.Location = new System.Drawing.Point(41, 0);
            this.errorMessage.MaximumSize = new System.Drawing.Size(409, 0);
            this.errorMessage.Name = "errorMessage";
            this.errorMessage.Size = new System.Drawing.Size(409, 38);
            this.errorMessage.TabIndex = 0;
            this.errorMessage.Text = "Error Message";
            this.errorMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkStatus
            // 
            this.chkStatus.AutoSize = true;
            this.chkStatus.Location = new System.Drawing.Point(72, 92);
            this.chkStatus.Name = "chkStatus";
            this.chkStatus.Size = new System.Drawing.Size(132, 17);
            this.chkStatus.TabIndex = 6;
            this.chkStatus.Text = "Show Status Message";
            this.chkStatus.UseVisualStyleBackColor = true;
            // 
            // ComponentSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(lblDevice);
            this.Controls.Add(this.txtDevice);
            this.Controls.Add(btnDetect);
            this.Controls.Add(lblConfig);
            this.Controls.Add(this.txtConfigFile);
            this.Controls.Add(btnBrowse);
            this.Controls.Add(this.chkReset);
            this.Controls.Add(this.chkStatus);
            this.Controls.Add(this.errorPanel);
            this.Name = "ComponentSettings";
            this.Padding = new System.Windows.Forms.Padding(7);
            this.Size = new System.Drawing.Size(476, 512);
            this.errorPanel.ResumeLayout(false);
            this.errorPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtDevice;
        private System.Windows.Forms.TextBox txtConfigFile;
        private System.Windows.Forms.CheckBox chkReset;
        private System.Windows.Forms.CheckBox chkStatus;
        private System.Windows.Forms.TableLayoutPanel errorPanel;
        private System.Windows.Forms.Label errorMessage;
        private System.Windows.Forms.PictureBox errorIcon;
    }
}
