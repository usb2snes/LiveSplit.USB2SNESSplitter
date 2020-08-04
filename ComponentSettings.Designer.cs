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
            this.label1 = new System.Windows.Forms.Label();
            this.txtComPort = new System.Windows.Forms.TextBox();
            this.txtConfigFile = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnDetect = new System.Windows.Forms.Button();
            this.chkReset = new System.Windows.Forms.CheckBox();
            this.errorPanel = new System.Windows.Forms.TableLayoutPanel();
            this.errorMessage = new System.Windows.Forms.Label();
            this.errorIcon = new System.Windows.Forms.PictureBox();
            this.errorPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Device:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // txtComPort
            // 
            this.txtComPort.Location = new System.Drawing.Point(72, 17);
            this.txtComPort.Name = "txtComPort";
            this.txtComPort.Size = new System.Drawing.Size(100, 20);
            this.txtComPort.TabIndex = 1;
            // 
            // txtConfigFile
            // 
            this.txtConfigFile.Location = new System.Drawing.Point(72, 43);
            this.txtConfigFile.Name = "txtConfigFile";
            this.txtConfigFile.Size = new System.Drawing.Size(355, 20);
            this.txtConfigFile.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Config file:";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(433, 43);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(33, 20);
            this.btnBrowse.TabIndex = 4;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnDetect
            // 
            this.btnDetect.Location = new System.Drawing.Point(178, 15);
            this.btnDetect.Name = "btnDetect";
            this.btnDetect.Size = new System.Drawing.Size(75, 23);
            this.btnDetect.TabIndex = 5;
            this.btnDetect.Text = "Autodetect";
            this.btnDetect.UseVisualStyleBackColor = true;
            this.btnDetect.Click += new System.EventHandler(this.btnDetect_Click);
            // 
            // chkReset
            // 
            this.chkReset.AutoSize = true;
            this.chkReset.Location = new System.Drawing.Point(72, 69);
            this.chkReset.Name = "chkReset";
            this.chkReset.Size = new System.Drawing.Size(156, 17);
            this.chkReset.TabIndex = 6;
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
            this.errorPanel.Location = new System.Drawing.Point(13, 92);
            this.errorPanel.Name = "errorPanel";
            this.errorPanel.RowCount = 1;
            this.errorPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.errorPanel.Size = new System.Drawing.Size(453, 38);
            this.errorPanel.TabIndex = 7;
            this.errorPanel.Visible = false;
            // 
            // errorMessage
            // 
            this.errorMessage.AutoSize = true;
            this.errorMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.errorMessage.Location = new System.Drawing.Point(41, 0);
            this.errorMessage.MaximumSize = new System.Drawing.Size(409, 0);
            this.errorMessage.Name = "errorMessage";
            this.errorMessage.Size = new System.Drawing.Size(409, 38);
            this.errorMessage.TabIndex = 1;
            this.errorMessage.Text = "Error Message";
            this.errorMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            // ComponentSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.errorPanel);
            this.Controls.Add(this.chkReset);
            this.Controls.Add(this.btnDetect);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtConfigFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtComPort);
            this.Controls.Add(this.label1);
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

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtComPort;
        private System.Windows.Forms.TextBox txtConfigFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnDetect;
        private System.Windows.Forms.CheckBox chkReset;
        private System.Windows.Forms.TableLayoutPanel errorPanel;
        private System.Windows.Forms.Label errorMessage;
        private System.Windows.Forms.PictureBox errorIcon;
    }
}
