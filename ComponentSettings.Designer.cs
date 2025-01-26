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
            System.Windows.Forms.TableLayoutPanel mainPanel;
            System.Windows.Forms.FlowLayoutPanel topPanel;
            System.Windows.Forms.Panel devicePanel;
            System.Windows.Forms.Panel configPanel;
            System.Windows.Forms.Panel resetPanel;
            System.Windows.Forms.Panel statusPanel;
            System.Windows.Forms.GroupBox splitsGroup;
            this.txtDevice = new System.Windows.Forms.TextBox();
            this.txtConfigFile = new System.Windows.Forms.TextBox();
            this.chkReset = new System.Windows.Forms.CheckBox();
            this.legacyPort = new System.Windows.Forms.CheckBox();
            this.chkStatus = new System.Windows.Forms.CheckBox();
            this.errorPanel = new System.Windows.Forms.TableLayoutPanel();
            this.errorIcon = new System.Windows.Forms.PictureBox();
            this.errorMessage = new System.Windows.Forms.Label();
            this.splitsPanel = new System.Windows.Forms.TableLayoutPanel();
            lblDevice = new System.Windows.Forms.Label();
            lblConfig = new System.Windows.Forms.Label();
            btnBrowse = new System.Windows.Forms.Button();
            btnDetect = new System.Windows.Forms.Button();
            mainPanel = new System.Windows.Forms.TableLayoutPanel();
            topPanel = new System.Windows.Forms.FlowLayoutPanel();
            devicePanel = new System.Windows.Forms.Panel();
            configPanel = new System.Windows.Forms.Panel();
            resetPanel = new System.Windows.Forms.Panel();
            statusPanel = new System.Windows.Forms.Panel();
            splitsGroup = new System.Windows.Forms.GroupBox();
            mainPanel.SuspendLayout();
            topPanel.SuspendLayout();
            devicePanel.SuspendLayout();
            configPanel.SuspendLayout();
            resetPanel.SuspendLayout();
            statusPanel.SuspendLayout();
            this.errorPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorIcon)).BeginInit();
            splitsGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDevice
            // 
            lblDevice.AutoSize = true;
            lblDevice.Location = new System.Drawing.Point(3, 7);
            lblDevice.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
            lblDevice.Name = "lblDevice";
            lblDevice.Size = new System.Drawing.Size(44, 13);
            lblDevice.TabIndex = 0;
            lblDevice.Text = "Device:";
            // 
            // lblConfig
            // 
            lblConfig.AutoSize = true;
            lblConfig.Location = new System.Drawing.Point(3, 8);
            lblConfig.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
            lblConfig.Name = "lblConfig";
            lblConfig.Size = new System.Drawing.Size(56, 13);
            lblConfig.TabIndex = 0;
            lblConfig.Text = "Config file:";
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new System.Drawing.Point(410, 3);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new System.Drawing.Size(33, 23);
            btnBrowse.TabIndex = 4;
            btnBrowse.Text = "...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnDetect
            // 
            btnDetect.Location = new System.Drawing.Point(172, 2);
            btnDetect.Name = "btnDetect";
            btnDetect.Size = new System.Drawing.Size(75, 23);
            btnDetect.TabIndex = 1;
            btnDetect.Text = "Autodetect";
            btnDetect.UseVisualStyleBackColor = true;
            btnDetect.Click += new System.EventHandler(this.btnDetect_Click);
            // 
            // mainPanel
            // 
            mainPanel.AutoScroll = true;
            mainPanel.AutoSize = true;
            mainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            mainPanel.ColumnCount = 1;
            mainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            mainPanel.Controls.Add(topPanel, 0, 0);
            mainPanel.Controls.Add(splitsGroup, 0, 1);
            mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            mainPanel.Location = new System.Drawing.Point(7, 7);
            mainPanel.Margin = new System.Windows.Forms.Padding(0);
            mainPanel.Name = "mainPanel";
            mainPanel.RowCount = 2;
            mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            mainPanel.Size = new System.Drawing.Size(462, 179);
            mainPanel.TabIndex = 8;
            // 
            // topPanel
            // 
            topPanel.AutoSize = true;
            topPanel.Controls.Add(devicePanel);
            topPanel.Controls.Add(configPanel);
            topPanel.Controls.Add(resetPanel);
            topPanel.Controls.Add(statusPanel);
            topPanel.Controls.Add(this.errorPanel);
            topPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            topPanel.Location = new System.Drawing.Point(0, 0);
            topPanel.Margin = new System.Windows.Forms.Padding(0);
            topPanel.Name = "topPanel";
            topPanel.Size = new System.Drawing.Size(446, 147);
            topPanel.TabIndex = 0;
            // 
            // devicePanel
            // 
            devicePanel.AutoSize = true;
            devicePanel.Controls.Add(lblDevice);
            devicePanel.Controls.Add(this.txtDevice);
            devicePanel.Controls.Add(btnDetect);
            devicePanel.Controls.Add(this.legacyPort);
            devicePanel.Location = new System.Drawing.Point(0, 0);
            devicePanel.Margin = new System.Windows.Forms.Padding(0);
            devicePanel.Name = "devicePanel";
            devicePanel.Size = new System.Drawing.Size(250, 28);
            devicePanel.TabIndex = 0;
            // 
            // txtDevice
            // 
            this.txtDevice.Location = new System.Drawing.Point(66, 3);
            this.txtDevice.Name = "txtDevice";
            this.txtDevice.Size = new System.Drawing.Size(100, 20);
            this.txtDevice.TabIndex = 0;
            this.txtDevice.TextChanged += new System.EventHandler(this.txtDevice_TextChanged);
            //
            // legacyPort
            //
            this.legacyPort.AutoSize = true;
            this.legacyPort.Location = new System.Drawing.Point(256, 5);
            this.legacyPort.Name = "legacyPort";
            this.legacyPort.Size = new System.Drawing.Size(150, 17);
            this.legacyPort.TabIndex = 1;
            this.legacyPort.Text = "Use port 8080 (legacy)";
            this.legacyPort.UseVisualStyleBackColor = true;
            this.legacyPort.CheckedChanged += new System.EventHandler(this.legacyPort_CheckedChanged);
            // 
            // configPanel
            // 
            configPanel.AutoSize = true;
            configPanel.Controls.Add(lblConfig);
            configPanel.Controls.Add(this.txtConfigFile);
            configPanel.Controls.Add(btnBrowse);
            configPanel.Location = new System.Drawing.Point(0, 28);
            configPanel.Margin = new System.Windows.Forms.Padding(0);
            configPanel.Name = "configPanel";
            configPanel.Size = new System.Drawing.Size(446, 29);
            configPanel.TabIndex = 1;
            // 
            // txtConfigFile
            // 
            this.txtConfigFile.Location = new System.Drawing.Point(66, 4);
            this.txtConfigFile.Name = "txtConfigFile";
            this.txtConfigFile.Size = new System.Drawing.Size(340, 20);
            this.txtConfigFile.TabIndex = 3;
            // 
            // resetPanel
            // 
            resetPanel.AutoSize = true;
            resetPanel.Controls.Add(this.chkReset);
            resetPanel.Location = new System.Drawing.Point(0, 57);
            resetPanel.Margin = new System.Windows.Forms.Padding(0);
            resetPanel.Name = "resetPanel";
            resetPanel.Size = new System.Drawing.Size(225, 23);
            resetPanel.TabIndex = 7;
            // 
            // chkReset
            // 
            this.chkReset.AutoSize = true;
            this.chkReset.Location = new System.Drawing.Point(66, 3);
            this.chkReset.Name = "chkReset";
            this.chkReset.Size = new System.Drawing.Size(156, 17);
            this.chkReset.TabIndex = 5;
            this.chkReset.Text = "Reset SNES on Timer reset";
            this.chkReset.UseVisualStyleBackColor = true;
            // 
            // statusPanel
            // 
            statusPanel.AutoSize = true;
            statusPanel.Controls.Add(this.chkStatus);
            statusPanel.Location = new System.Drawing.Point(0, 80);
            statusPanel.Margin = new System.Windows.Forms.Padding(0);
            statusPanel.Name = "statusPanel";
            statusPanel.Size = new System.Drawing.Size(201, 23);
            statusPanel.TabIndex = 8;
            // 
            // chkStatus
            // 
            this.chkStatus.AutoSize = true;
            this.chkStatus.Location = new System.Drawing.Point(66, 3);
            this.chkStatus.Name = "chkStatus";
            this.chkStatus.Size = new System.Drawing.Size(132, 17);
            this.chkStatus.TabIndex = 6;
            this.chkStatus.Text = "Show Status Message";
            this.chkStatus.UseVisualStyleBackColor = true;
            // 
            // errorPanel
            // 
            this.errorPanel.AutoSize = true;
            this.errorPanel.ColumnCount = 2;
            this.errorPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.errorPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.errorPanel.Controls.Add(this.errorIcon, 0, 0);
            this.errorPanel.Controls.Add(this.errorMessage, 1, 0);
            this.errorPanel.Location = new System.Drawing.Point(3, 106);
            this.errorPanel.Name = "errorPanel";
            this.errorPanel.RowCount = 1;
            this.errorPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.errorPanel.Size = new System.Drawing.Size(119, 38);
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
            this.errorMessage.Size = new System.Drawing.Size(75, 38);
            this.errorMessage.TabIndex = 0;
            this.errorMessage.Text = "Error Message";
            this.errorMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.errorMessage.TextChanged += new System.EventHandler(this.errorMessage_TextChanged);
            // 
            // splitsGroup
            // 
            splitsGroup.AutoSize = true;
            splitsGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            splitsGroup.Controls.Add(this.splitsPanel);
            splitsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            splitsGroup.Location = new System.Drawing.Point(0, 147);
            splitsGroup.Margin = new System.Windows.Forms.Padding(0);
            splitsGroup.Name = "splitsGroup";
            splitsGroup.Size = new System.Drawing.Size(462, 32);
            splitsGroup.TabIndex = 9;
            splitsGroup.TabStop = false;
            splitsGroup.Text = "Splits";
            // 
            // splitsPanel
            // 
            this.splitsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitsPanel.AutoSize = true;
            this.splitsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.splitsPanel.ColumnCount = 2;
            this.splitsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.splitsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.splitsPanel.Location = new System.Drawing.Point(3, 16);
            this.splitsPanel.Margin = new System.Windows.Forms.Padding(0);
            this.splitsPanel.Name = "splitsPanel";
            this.splitsPanel.RowCount = 1;
            this.splitsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.splitsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
            this.splitsPanel.Size = new System.Drawing.Size(456, 0);
            this.splitsPanel.TabIndex = 1;
            // 
            // ComponentSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(mainPanel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ComponentSettings";
            this.Padding = new System.Windows.Forms.Padding(7);
            this.Size = new System.Drawing.Size(476, 193);
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            devicePanel.ResumeLayout(false);
            devicePanel.PerformLayout();
            configPanel.ResumeLayout(false);
            configPanel.PerformLayout();
            resetPanel.ResumeLayout(false);
            resetPanel.PerformLayout();
            statusPanel.ResumeLayout(false);
            statusPanel.PerformLayout();
            this.errorPanel.ResumeLayout(false);
            this.errorPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorIcon)).EndInit();
            splitsGroup.ResumeLayout(false);
            splitsGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtDevice;
        private System.Windows.Forms.TextBox txtConfigFile;
        private System.Windows.Forms.CheckBox chkReset;
        private System.Windows.Forms.CheckBox legacyPort;
        private System.Windows.Forms.CheckBox chkStatus;
        private System.Windows.Forms.TableLayoutPanel errorPanel;
        private System.Windows.Forms.Label errorMessage;
        private System.Windows.Forms.PictureBox errorIcon;
        private System.Windows.Forms.TableLayoutPanel splitsPanel;
    }
}
