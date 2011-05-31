namespace MOTMaster
{
    partial class ControllerWindow
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
            this.PatternSourceTextBox = new System.Windows.Forms.TextBox();
            this.scriptListComboBox = new System.Windows.Forms.ComboBox();
            this.lookupScriptsButton = new System.Windows.Forms.Button();
            this.runButton = new System.Windows.Forms.Button();
            this.saveExperimentCheckBox = new System.Windows.Forms.CheckBox();
            this.saveBatchTextBox = new System.Windows.Forms.TextBox();
            this.SelectBinaryButton = new System.Windows.Forms.Button();
            this.selectScriptButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // PatternSourceTextBox
            // 
            this.PatternSourceTextBox.Location = new System.Drawing.Point(589, 41);
            this.PatternSourceTextBox.Name = "PatternSourceTextBox";
            this.PatternSourceTextBox.ReadOnly = true;
            this.PatternSourceTextBox.Size = new System.Drawing.Size(355, 20);
            this.PatternSourceTextBox.TabIndex = 4;
            // 
            // scriptListComboBox
            // 
            this.scriptListComboBox.FormattingEnabled = true;
            this.scriptListComboBox.Location = new System.Drawing.Point(4, 12);
            this.scriptListComboBox.MaxDropDownItems = 32;
            this.scriptListComboBox.Name = "scriptListComboBox";
            this.scriptListComboBox.Size = new System.Drawing.Size(940, 21);
            this.scriptListComboBox.Sorted = true;
            this.scriptListComboBox.TabIndex = 6;
            // 
            // lookupScriptsButton
            // 
            this.lookupScriptsButton.Location = new System.Drawing.Point(4, 39);
            this.lookupScriptsButton.Name = "lookupScriptsButton";
            this.lookupScriptsButton.Size = new System.Drawing.Size(137, 23);
            this.lookupScriptsButton.TabIndex = 7;
            this.lookupScriptsButton.Text = "Refresh Script List";
            this.lookupScriptsButton.UseVisualStyleBackColor = true;
            this.lookupScriptsButton.Click += new System.EventHandler(this.lookupScriptsButton_Click);
            // 
            // runButton
            // 
            this.runButton.Location = new System.Drawing.Point(433, 39);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(137, 23);
            this.runButton.TabIndex = 8;
            this.runButton.Text = "Run";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // saveExperimentCheckBox
            // 
            this.saveExperimentCheckBox.AutoSize = true;
            this.saveExperimentCheckBox.Location = new System.Drawing.Point(294, 70);
            this.saveExperimentCheckBox.Name = "saveExperimentCheckBox";
            this.saveExperimentCheckBox.Size = new System.Drawing.Size(148, 17);
            this.saveExperimentCheckBox.TabIndex = 9;
            this.saveExperimentCheckBox.Text = "Save Experiment to batch";
            this.saveExperimentCheckBox.UseVisualStyleBackColor = true;
            this.saveExperimentCheckBox.CheckedChanged += new System.EventHandler(this.saveExperimentCheckBox_CheckedChanged);
            // 
            // saveBatchTextBox
            // 
            this.saveBatchTextBox.Enabled = false;
            this.saveBatchTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.saveBatchTextBox.Location = new System.Drawing.Point(463, 72);
            this.saveBatchTextBox.Name = "saveBatchTextBox";
            this.saveBatchTextBox.Size = new System.Drawing.Size(49, 20);
            this.saveBatchTextBox.TabIndex = 10;
            this.saveBatchTextBox.Text = "0";
            // 
            // SelectBinaryButton
            // 
            this.SelectBinaryButton.Location = new System.Drawing.Point(290, 39);
            this.SelectBinaryButton.Name = "SelectBinaryButton";
            this.SelectBinaryButton.Size = new System.Drawing.Size(137, 23);
            this.SelectBinaryButton.TabIndex = 11;
            this.SelectBinaryButton.Text = "Select prebuilt pattern";
            this.SelectBinaryButton.UseVisualStyleBackColor = true;
            this.SelectBinaryButton.Click += new System.EventHandler(this.SelectBinaryButton_Click);
            // 
            // selectScriptButton
            // 
            this.selectScriptButton.Location = new System.Drawing.Point(147, 39);
            this.selectScriptButton.Name = "selectScriptButton";
            this.selectScriptButton.Size = new System.Drawing.Size(137, 23);
            this.selectScriptButton.TabIndex = 12;
            this.selectScriptButton.Text = "Select script";
            this.selectScriptButton.UseVisualStyleBackColor = true;
            this.selectScriptButton.Click += new System.EventHandler(this.selectScriptButton_Click);
            // 
            // ControllerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(953, 105);
            this.Controls.Add(this.selectScriptButton);
            this.Controls.Add(this.SelectBinaryButton);
            this.Controls.Add(this.saveBatchTextBox);
            this.Controls.Add(this.saveExperimentCheckBox);
            this.Controls.Add(this.runButton);
            this.Controls.Add(this.lookupScriptsButton);
            this.Controls.Add(this.scriptListComboBox);
            this.Controls.Add(this.PatternSourceTextBox);
            this.Name = "ControllerWindow";
            this.Text = "MOTMaster Main Window";
            this.Load += new System.EventHandler(this.ControllerWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox PatternSourceTextBox;
        private System.Windows.Forms.ComboBox scriptListComboBox;
        private System.Windows.Forms.Button lookupScriptsButton;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.CheckBox saveExperimentCheckBox;
        private System.Windows.Forms.TextBox saveBatchTextBox;
        private System.Windows.Forms.Button SelectBinaryButton;
        private System.Windows.Forms.Button selectScriptButton;
    }
}

