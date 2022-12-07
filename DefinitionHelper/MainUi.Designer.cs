namespace LiveSplit.UI.Components
{
    partial class MainUI
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.listSplits = new System.Windows.Forms.ListView();
            this.label1 = new System.Windows.Forms.Label();
            this.OpenGame = new System.Windows.Forms.Button();
            this.NewGame = new System.Windows.Forms.Button();
            this.SaveGame = new System.Windows.Forms.Button();
            this.TitleLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.splitNameTextBox = new System.Windows.Forms.TextBox();
            this.addressTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.typeComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.subSplitLabel = new System.Windows.Forms.Label();
            this.addNextButton = new System.Windows.Forms.Button();
            this.addMoreButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.subSplitAddress = new System.Windows.Forms.TextBox();
            this.subSplitTypeComboBox = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.addSplitButton = new System.Windows.Forms.Button();
            this.delSplitButton = new System.Windows.Forms.Button();
            this.usb2snesLabel = new System.Windows.Forms.Label();
            this.checkButton = new System.Windows.Forms.Button();
            this.splitOkButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.valueTextBox = new System.Windows.Forms.TextBox();
            this.subSplitValue = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.checkStatusLabel = new System.Windows.Forms.Label();
            this.subSplitView = new System.Windows.Forms.DataGridView();
            this.subSplitName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.subSplitStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label10 = new System.Windows.Forms.Label();
            this.valueDecTextBox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.subSplitValueDecTextBox = new System.Windows.Forms.TextBox();
            this.buttonOrderUp = new System.Windows.Forms.Button();
            this.buttonOrderDown = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.subSplitView)).BeginInit();
            this.SuspendLayout();
            // 
            // listSplits
            // 
            this.listSplits.HideSelection = false;
            this.listSplits.Location = new System.Drawing.Point(37, 142);
            this.listSplits.MultiSelect = false;
            this.listSplits.Name = "listSplits";
            this.listSplits.Size = new System.Drawing.Size(193, 264);
            this.listSplits.TabIndex = 0;
            this.listSplits.UseCompatibleStateImageBehavior = false;
            this.listSplits.View = System.Windows.Forms.View.List;
            this.listSplits.SelectedIndexChanged += new System.EventHandler(this.listSplits_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(69, 113);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "List of Definitions";
            // 
            // OpenGame
            // 
            this.OpenGame.Location = new System.Drawing.Point(467, 12);
            this.OpenGame.Name = "OpenGame";
            this.OpenGame.Size = new System.Drawing.Size(75, 23);
            this.OpenGame.TabIndex = 2;
            this.OpenGame.Text = "Open";
            this.OpenGame.UseVisualStyleBackColor = true;
            this.OpenGame.Click += new System.EventHandler(this.OpenGame_Click);
            // 
            // NewGame
            // 
            this.NewGame.Location = new System.Drawing.Point(386, 12);
            this.NewGame.Name = "NewGame";
            this.NewGame.Size = new System.Drawing.Size(75, 23);
            this.NewGame.TabIndex = 3;
            this.NewGame.Text = "New";
            this.NewGame.UseVisualStyleBackColor = true;
            this.NewGame.Click += new System.EventHandler(this.NewGame_Click);
            // 
            // SaveGame
            // 
            this.SaveGame.Location = new System.Drawing.Point(48, 12);
            this.SaveGame.Name = "SaveGame";
            this.SaveGame.Size = new System.Drawing.Size(75, 23);
            this.SaveGame.TabIndex = 4;
            this.SaveGame.Text = "Save";
            this.SaveGame.UseVisualStyleBackColor = true;
            this.SaveGame.Click += new System.EventHandler(this.SaveGame_Click);
            // 
            // TitleLabel
            // 
            this.TitleLabel.AutoSize = true;
            this.TitleLabel.Location = new System.Drawing.Point(197, 69);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Size = new System.Drawing.Size(66, 13);
            this.TitleLabel.TabIndex = 5;
            this.TitleLabel.Text = "Game Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(257, 158);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Name :";
            // 
            // splitNameTextBox
            // 
            this.splitNameTextBox.AcceptsReturn = true;
            this.splitNameTextBox.Location = new System.Drawing.Point(312, 155);
            this.splitNameTextBox.Name = "splitNameTextBox";
            this.splitNameTextBox.Size = new System.Drawing.Size(216, 20);
            this.splitNameTextBox.TabIndex = 7;
            this.splitNameTextBox.TextChanged += new System.EventHandler(this.splitNameTextBox_TextChanged);
            // 
            // addressTextBox
            // 
            this.addressTextBox.AcceptsReturn = true;
            this.addressTextBox.Location = new System.Drawing.Point(312, 191);
            this.addressTextBox.Name = "addressTextBox";
            this.addressTextBox.Size = new System.Drawing.Size(100, 20);
            this.addressTextBox.TabIndex = 8;
            this.addressTextBox.TextChanged += new System.EventHandler(this.addressTextBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(253, 194);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Address :";
            // 
            // typeComboBox
            // 
            this.typeComboBox.FormattingEnabled = true;
            this.typeComboBox.Location = new System.Drawing.Point(386, 248);
            this.typeComboBox.Name = "typeComboBox";
            this.typeComboBox.Size = new System.Drawing.Size(121, 21);
            this.typeComboBox.TabIndex = 10;
            this.typeComboBox.SelectedIndexChanged += new System.EventHandler(this.typeComboBox_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(281, 251);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Comparaison Type";
            // 
            // subSplitLabel
            // 
            this.subSplitLabel.AutoSize = true;
            this.subSplitLabel.Location = new System.Drawing.Point(589, 123);
            this.subSplitLabel.Name = "subSplitLabel";
            this.subSplitLabel.Size = new System.Drawing.Size(81, 13);
            this.subSplitLabel.TabIndex = 13;
            this.subSplitLabel.Text = "Next Sequence";
            // 
            // addNextButton
            // 
            this.addNextButton.Location = new System.Drawing.Point(549, 375);
            this.addNextButton.Name = "addNextButton";
            this.addNextButton.Size = new System.Drawing.Size(75, 23);
            this.addNextButton.TabIndex = 14;
            this.addNextButton.Text = "Add Next";
            this.addNextButton.UseVisualStyleBackColor = true;
            this.addNextButton.Click += new System.EventHandler(this.addNextButton_Click);
            // 
            // addMoreButton
            // 
            this.addMoreButton.Location = new System.Drawing.Point(645, 375);
            this.addMoreButton.Name = "addMoreButton";
            this.addMoreButton.Size = new System.Drawing.Size(75, 23);
            this.addMoreButton.TabIndex = 15;
            this.addMoreButton.Text = "Add More";
            this.addMoreButton.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(345, 312);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "------------------------";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(261, 341);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(45, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "Address";
            // 
            // subSplitAddress
            // 
            this.subSplitAddress.Location = new System.Drawing.Point(345, 338);
            this.subSplitAddress.Name = "subSplitAddress";
            this.subSplitAddress.Size = new System.Drawing.Size(100, 20);
            this.subSplitAddress.TabIndex = 18;
            this.subSplitAddress.TextChanged += new System.EventHandler(this.subSplitAddress_TextChanged);
            // 
            // subSplitTypeComboBox
            // 
            this.subSplitTypeComboBox.FormattingEnabled = true;
            this.subSplitTypeComboBox.Location = new System.Drawing.Point(348, 363);
            this.subSplitTypeComboBox.Name = "subSplitTypeComboBox";
            this.subSplitTypeComboBox.Size = new System.Drawing.Size(121, 21);
            this.subSplitTypeComboBox.TabIndex = 19;
            this.subSplitTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.subSplitTypeComboBox_SelectedIndexChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(247, 366);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(95, 13);
            this.label8.TabIndex = 20;
            this.label8.Text = "Comparaison Type";
            // 
            // addSplitButton
            // 
            this.addSplitButton.Location = new System.Drawing.Point(37, 427);
            this.addSplitButton.Name = "addSplitButton";
            this.addSplitButton.Size = new System.Drawing.Size(86, 23);
            this.addSplitButton.TabIndex = 21;
            this.addSplitButton.Text = "Add Definition";
            this.addSplitButton.UseVisualStyleBackColor = true;
            this.addSplitButton.Click += new System.EventHandler(this.addSplitButton_Click);
            // 
            // delSplitButton
            // 
            this.delSplitButton.Location = new System.Drawing.Point(131, 427);
            this.delSplitButton.Name = "delSplitButton";
            this.delSplitButton.Size = new System.Drawing.Size(99, 23);
            this.delSplitButton.TabIndex = 22;
            this.delSplitButton.Text = "Delete Definition";
            this.delSplitButton.UseVisualStyleBackColor = true;
            this.delSplitButton.Click += new System.EventHandler(this.delSplit_Click);
            // 
            // usb2snesLabel
            // 
            this.usb2snesLabel.AutoSize = true;
            this.usb2snesLabel.Location = new System.Drawing.Point(585, 69);
            this.usb2snesLabel.Name = "usb2snesLabel";
            this.usb2snesLabel.Size = new System.Drawing.Size(85, 13);
            this.usb2snesLabel.TabIndex = 23;
            this.usb2snesLabel.Text = "Usb2snes status";
            // 
            // checkButton
            // 
            this.checkButton.Enabled = false;
            this.checkButton.Location = new System.Drawing.Point(561, 426);
            this.checkButton.Name = "checkButton";
            this.checkButton.Size = new System.Drawing.Size(102, 23);
            this.checkButton.TabIndex = 24;
            this.checkButton.Text = "Check Split";
            this.checkButton.UseVisualStyleBackColor = true;
            this.checkButton.Click += new System.EventHandler(this.checkButton_Click);
            // 
            // splitOkButton
            // 
            this.splitOkButton.BackColor = System.Drawing.Color.Silver;
            this.splitOkButton.Location = new System.Drawing.Point(669, 426);
            this.splitOkButton.Name = "splitOkButton";
            this.splitOkButton.Size = new System.Drawing.Size(75, 23);
            this.splitOkButton.TabIndex = 25;
            this.splitOkButton.Text = "None";
            this.splitOkButton.UseVisualStyleBackColor = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(247, 292);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 13);
            this.label5.TabIndex = 26;
            this.label5.Text = "Value Hex:";
            // 
            // valueTextBox
            // 
            this.valueTextBox.Location = new System.Drawing.Point(312, 289);
            this.valueTextBox.Name = "valueTextBox";
            this.valueTextBox.Size = new System.Drawing.Size(70, 20);
            this.valueTextBox.TabIndex = 27;
            this.valueTextBox.TextChanged += new System.EventHandler(this.valueTextBox_TextChanged);
            // 
            // subSplitValue
            // 
            this.subSplitValue.Location = new System.Drawing.Point(312, 394);
            this.subSplitValue.Name = "subSplitValue";
            this.subSplitValue.Size = new System.Drawing.Size(70, 20);
            this.subSplitValue.TabIndex = 28;
            this.subSplitValue.TextChanged += new System.EventHandler(this.subSplitValue_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(257, 394);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(59, 13);
            this.label9.TabIndex = 29;
            this.label9.Text = "Value Hex:";
            // 
            // checkStatusLabel
            // 
            this.checkStatusLabel.AutoSize = true;
            this.checkStatusLabel.Location = new System.Drawing.Point(549, 475);
            this.checkStatusLabel.Name = "checkStatusLabel";
            this.checkStatusLabel.Size = new System.Drawing.Size(87, 13);
            this.checkStatusLabel.TabIndex = 30;
            this.checkStatusLabel.Text = "what is checking";
            // 
            // subSplitView
            // 
            this.subSplitView.AllowUserToAddRows = false;
            this.subSplitView.AllowUserToDeleteRows = false;
            this.subSplitView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.subSplitView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.subSplitName,
            this.subSplitStatus});
            this.subSplitView.Location = new System.Drawing.Point(549, 158);
            this.subSplitView.MultiSelect = false;
            this.subSplitView.Name = "subSplitView";
            this.subSplitView.ReadOnly = true;
            this.subSplitView.RowHeadersVisible = false;
            this.subSplitView.Size = new System.Drawing.Size(218, 187);
            this.subSplitView.TabIndex = 31;
            this.subSplitView.SelectionChanged += new System.EventHandler(this.subSplitView_SelectionChanged);
            // 
            // subSplitName
            // 
            this.subSplitName.HeaderText = "Sub Split";
            this.subSplitName.Name = "subSplitName";
            this.subSplitName.ReadOnly = true;
            // 
            // subSplitStatus
            // 
            this.subSplitStatus.HeaderText = "Status";
            this.subSplitStatus.Name = "subSplitStatus";
            this.subSplitStatus.ReadOnly = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(388, 292);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(60, 13);
            this.label10.TabIndex = 32;
            this.label10.Text = "Value Dec:";
            // 
            // valueDecTextBox
            // 
            this.valueDecTextBox.Location = new System.Drawing.Point(453, 289);
            this.valueDecTextBox.Name = "valueDecTextBox";
            this.valueDecTextBox.Size = new System.Drawing.Size(75, 20);
            this.valueDecTextBox.TabIndex = 33;
            this.valueDecTextBox.TextChanged += new System.EventHandler(this.valueDecTextBox_TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(388, 397);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(57, 13);
            this.label11.TabIndex = 34;
            this.label11.Text = "Value Dec";
            // 
            // subSplitValueDecTextBox
            // 
            this.subSplitValueDecTextBox.Location = new System.Drawing.Point(454, 394);
            this.subSplitValueDecTextBox.Name = "subSplitValueDecTextBox";
            this.subSplitValueDecTextBox.Size = new System.Drawing.Size(74, 20);
            this.subSplitValueDecTextBox.TabIndex = 35;
            this.subSplitValueDecTextBox.TextChanged += new System.EventHandler(this.subSplitValueDecTextBox_TextChanged);
            // 
            // buttonOrderUp
            // 
            this.buttonOrderUp.Location = new System.Drawing.Point(12, 209);
            this.buttonOrderUp.Name = "buttonOrderUp";
            this.buttonOrderUp.Size = new System.Drawing.Size(18, 40);
            this.buttonOrderUp.TabIndex = 36;
            this.buttonOrderUp.Text = "^";
            this.buttonOrderUp.UseVisualStyleBackColor = true;
            this.buttonOrderUp.Click += new System.EventHandler(this.buttonOderUp_Click);
            // 
            // buttonOrderDown
            // 
            this.buttonOrderDown.Location = new System.Drawing.Point(13, 256);
            this.buttonOrderDown.Name = "buttonOrderDown";
            this.buttonOrderDown.Size = new System.Drawing.Size(18, 57);
            this.buttonOrderDown.TabIndex = 37;
            this.buttonOrderDown.Text = "V";
            this.buttonOrderDown.UseVisualStyleBackColor = true;
            this.buttonOrderDown.Click += new System.EventHandler(this.buttonOrderDown_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(261, 232);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(273, 13);
            this.label12.TabIndex = 38;
            this.label12.Text = "Byte: one byte get checked. Word: 2 bytes get checked";
            // 
            // MainUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(797, 500);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.buttonOrderDown);
            this.Controls.Add(this.buttonOrderUp);
            this.Controls.Add(this.subSplitValueDecTextBox);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.valueDecTextBox);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.subSplitView);
            this.Controls.Add(this.checkStatusLabel);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.subSplitValue);
            this.Controls.Add(this.valueTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.splitOkButton);
            this.Controls.Add(this.checkButton);
            this.Controls.Add(this.usb2snesLabel);
            this.Controls.Add(this.delSplitButton);
            this.Controls.Add(this.addSplitButton);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.subSplitTypeComboBox);
            this.Controls.Add(this.subSplitAddress);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.addMoreButton);
            this.Controls.Add(this.addNextButton);
            this.Controls.Add(this.subSplitLabel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.typeComboBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.addressTextBox);
            this.Controls.Add(this.splitNameTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TitleLabel);
            this.Controls.Add(this.SaveGame);
            this.Controls.Add(this.NewGame);
            this.Controls.Add(this.OpenGame);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listSplits);
            this.Name = "MainUI";
            this.Text = "Definition Helper";
            ((System.ComponentModel.ISupportInitialize)(this.subSplitView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listSplits;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button OpenGame;
        private System.Windows.Forms.Button NewGame;
        private System.Windows.Forms.Button SaveGame;
        private System.Windows.Forms.Label TitleLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox splitNameTextBox;
        private System.Windows.Forms.TextBox addressTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox typeComboBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label subSplitLabel;
        private System.Windows.Forms.Button addNextButton;
        private System.Windows.Forms.Button addMoreButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox subSplitAddress;
        private System.Windows.Forms.ComboBox subSplitTypeComboBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button addSplitButton;
        private System.Windows.Forms.Button delSplitButton;
        private System.Windows.Forms.Label usb2snesLabel;
        private System.Windows.Forms.Button checkButton;
        private System.Windows.Forms.Button splitOkButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox valueTextBox;
        private System.Windows.Forms.TextBox subSplitValue;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label checkStatusLabel;
        private System.Windows.Forms.DataGridView subSplitView;
        private System.Windows.Forms.DataGridViewTextBoxColumn subSplitColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn subSplitColumnStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn subSplitName;
        private System.Windows.Forms.DataGridViewTextBoxColumn subSplitStatus;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox valueDecTextBox;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox subSplitValueDecTextBox;
        private System.Windows.Forms.Button buttonOrderUp;
        private System.Windows.Forms.Button buttonOrderDown;
        private System.Windows.Forms.Label label12;
    }
}

