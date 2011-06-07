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
            this.selectScriptButton = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.patternsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newPatternToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenOtherScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PatternSourceTextBox
            // 
            this.PatternSourceTextBox.Location = new System.Drawing.Point(96, 85);
            this.PatternSourceTextBox.Name = "PatternSourceTextBox";
            this.PatternSourceTextBox.ReadOnly = true;
            this.PatternSourceTextBox.Size = new System.Drawing.Size(474, 20);
            this.PatternSourceTextBox.TabIndex = 4;
            // 
            // scriptListComboBox
            // 
            this.scriptListComboBox.FormattingEnabled = true;
            this.scriptListComboBox.Location = new System.Drawing.Point(4, 26);
            this.scriptListComboBox.MaxDropDownItems = 32;
            this.scriptListComboBox.Name = "scriptListComboBox";
            this.scriptListComboBox.Size = new System.Drawing.Size(566, 21);
            this.scriptListComboBox.Sorted = true;
            this.scriptListComboBox.TabIndex = 6;
            // 
            // lookupScriptsButton
            // 
            this.lookupScriptsButton.Location = new System.Drawing.Point(576, 24);
            this.lookupScriptsButton.Name = "lookupScriptsButton";
            this.lookupScriptsButton.Size = new System.Drawing.Size(137, 23);
            this.lookupScriptsButton.TabIndex = 7;
            this.lookupScriptsButton.Text = "Refresh Script List";
            this.lookupScriptsButton.UseVisualStyleBackColor = true;
            this.lookupScriptsButton.Click += new System.EventHandler(this.lookupScriptsButton_Click);
            // 
            // runButton
            // 
            this.runButton.Location = new System.Drawing.Point(576, 83);
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
            this.saveExperimentCheckBox.Location = new System.Drawing.Point(147, 55);
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
            this.saveBatchTextBox.Location = new System.Drawing.Point(301, 53);
            this.saveBatchTextBox.Name = "saveBatchTextBox";
            this.saveBatchTextBox.Size = new System.Drawing.Size(49, 20);
            this.saveBatchTextBox.TabIndex = 10;
            this.saveBatchTextBox.Text = "0";
            // 
            // selectScriptButton
            // 
            this.selectScriptButton.Location = new System.Drawing.Point(4, 51);
            this.selectScriptButton.Name = "selectScriptButton";
            this.selectScriptButton.Size = new System.Drawing.Size(137, 23);
            this.selectScriptButton.TabIndex = 12;
            this.selectScriptButton.Text = "Select script";
            this.selectScriptButton.UseVisualStyleBackColor = true;
            this.selectScriptButton.Click += new System.EventHandler(this.selectScriptButton_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.patternsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(722, 24);
            this.menuStrip1.TabIndex = 13;
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // patternsToolStripMenuItem
            // 
            this.patternsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newPatternToolStripMenuItem,
            this.OpenOtherScriptToolStripMenuItem});
            this.patternsToolStripMenuItem.Name = "patternsToolStripMenuItem";
            this.patternsToolStripMenuItem.Size = new System.Drawing.Size(62, 20);
            this.patternsToolStripMenuItem.Text = "Patterns";
            // 
            // newPatternToolStripMenuItem
            // 
            this.newPatternToolStripMenuItem.Name = "newPatternToolStripMenuItem";
            this.newPatternToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.newPatternToolStripMenuItem.Text = "Build from selected script";
            this.newPatternToolStripMenuItem.Click += new System.EventHandler(this.newPatternToolStripMenuItem_Click);
            // 
            // OpenOtherScriptToolStripMenuItem
            // 
            this.OpenOtherScriptToolStripMenuItem.Name = "OpenOtherScriptToolStripMenuItem";
            this.OpenOtherScriptToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.OpenOtherScriptToolStripMenuItem.Text = "Open other patterns";
            this.OpenOtherScriptToolStripMenuItem.Click += new System.EventHandler(this.prebuiltPatternToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 88);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Selected Pattern:";
            // 
            // ControllerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(722, 111);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.selectScriptButton);
            this.Controls.Add(this.saveBatchTextBox);
            this.Controls.Add(this.saveExperimentCheckBox);
            this.Controls.Add(this.runButton);
            this.Controls.Add(this.lookupScriptsButton);
            this.Controls.Add(this.scriptListComboBox);
            this.Controls.Add(this.PatternSourceTextBox);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ControllerWindow";
            this.Text = "MOTMaster Main Window";
            this.Load += new System.EventHandler(this.ControllerWindow_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
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
        private System.Windows.Forms.Button selectScriptButton;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem patternsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newPatternToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OpenOtherScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.Label label1;
    }
}

