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
            this.resultsTextBox = new System.Windows.Forms.TextBox();
            this.scriptListComboBox = new System.Windows.Forms.ComboBox();
            this.lookupScriptsButton = new System.Windows.Forms.Button();
            this.compileAndRunButton = new System.Windows.Forms.Button();
            this.saveExperimentCheckBox = new System.Windows.Forms.CheckBox();
            this.saveBatchTextBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // resultsTextBox
            // 
            this.resultsTextBox.Location = new System.Drawing.Point(564, 41);
            this.resultsTextBox.Name = "resultsTextBox";
            this.resultsTextBox.ReadOnly = true;
            this.resultsTextBox.Size = new System.Drawing.Size(355, 20);
            this.resultsTextBox.TabIndex = 4;
            // 
            // scriptListComboBox
            // 
            this.scriptListComboBox.FormattingEnabled = true;
            this.scriptListComboBox.Location = new System.Drawing.Point(4, 12);
            this.scriptListComboBox.MaxDropDownItems = 32;
            this.scriptListComboBox.Name = "scriptListComboBox";
            this.scriptListComboBox.Size = new System.Drawing.Size(915, 21);
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
            // compileAndRunButton
            // 
            this.compileAndRunButton.Location = new System.Drawing.Point(147, 39);
            this.compileAndRunButton.Name = "compileAndRunButton";
            this.compileAndRunButton.Size = new System.Drawing.Size(137, 23);
            this.compileAndRunButton.TabIndex = 8;
            this.compileAndRunButton.Text = "Compile and Run";
            this.compileAndRunButton.UseVisualStyleBackColor = true;
            this.compileAndRunButton.Click += new System.EventHandler(this.compileAndInitializeButton_Click);
            // 
            // saveExperimentCheckBox
            // 
            this.saveExperimentCheckBox.AutoSize = true;
            this.saveExperimentCheckBox.Location = new System.Drawing.Point(290, 43);
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
            this.saveBatchTextBox.Location = new System.Drawing.Point(444, 41);
            this.saveBatchTextBox.Name = "saveBatchTextBox";
            this.saveBatchTextBox.Size = new System.Drawing.Size(49, 20);
            this.saveBatchTextBox.TabIndex = 10;
            this.saveBatchTextBox.Text = "0";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(28, 70);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 11;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ControllerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(931, 105);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.saveBatchTextBox);
            this.Controls.Add(this.saveExperimentCheckBox);
            this.Controls.Add(this.compileAndRunButton);
            this.Controls.Add(this.lookupScriptsButton);
            this.Controls.Add(this.scriptListComboBox);
            this.Controls.Add(this.resultsTextBox);
            this.Name = "ControllerWindow";
            this.Text = "MOTMaster Main Window";
            this.Load += new System.EventHandler(this.ControllerWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox resultsTextBox;
        private System.Windows.Forms.ComboBox scriptListComboBox;
        private System.Windows.Forms.Button lookupScriptsButton;
        private System.Windows.Forms.Button compileAndRunButton;
        private System.Windows.Forms.CheckBox saveExperimentCheckBox;
        private System.Windows.Forms.TextBox saveBatchTextBox;
        private System.Windows.Forms.Button button1;
    }
}

