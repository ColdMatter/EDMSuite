
﻿namespace MOTMaster
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
            this.PatternPathTextBox = new System.Windows.Forms.TextBox();
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
            this.ReplicateScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.stopButton = new System.Windows.Forms.Button();
            this.iterationsBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PatternPathTextBox
            // 
            this.PatternPathTextBox.Location = new System.Drawing.Point(129, 58);
            this.PatternPathTextBox.Name = "PatternPathTextBox";
            this.PatternPathTextBox.ReadOnly = true;
            this.PatternPathTextBox.Size = new System.Drawing.Size(454, 20);
            this.PatternPathTextBox.TabIndex = 4;
            // 
            // scriptListComboBox
            // 
            this.scriptListComboBox.FormattingEnabled = true;
            this.scriptListComboBox.Location = new System.Drawing.Point(16, 24);
            this.scriptListComboBox.MaxDropDownItems = 32;
            this.scriptListComboBox.Name = "scriptListComboBox";
            this.scriptListComboBox.Size = new System.Drawing.Size(566, 21);
            this.scriptListComboBox.Sorted = true;
            this.scriptListComboBox.TabIndex = 6;
            // 
            // lookupScriptsButton
            // 
            this.lookupScriptsButton.Location = new System.Drawing.Point(589, 24);
            this.lookupScriptsButton.Name = "lookupScriptsButton";
            this.lookupScriptsButton.Size = new System.Drawing.Size(137, 23);
            this.lookupScriptsButton.TabIndex = 7;
            this.lookupScriptsButton.Text = "Refresh Script List";
            this.lookupScriptsButton.UseVisualStyleBackColor = true;
            this.lookupScriptsButton.Click += new System.EventHandler(this.lookupScriptsButton_Click);
            // 
            // runButton
            // 
            this.runButton.Location = new System.Drawing.Point(589, 83);
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
            this.saveExperimentCheckBox.Location = new System.Drawing.Point(12, 109);
            this.saveExperimentCheckBox.Name = "saveExperimentCheckBox";
            this.saveExperimentCheckBox.Size = new System.Drawing.Size(148, 17);
            this.saveExperimentCheckBox.TabIndex = 9;
            this.saveExperimentCheckBox.Text = "Save Experiment to batch";
            this.saveExperimentCheckBox.UseVisualStyleBackColor = true;
            this.saveExperimentCheckBox.CheckedChanged += new System.EventHandler(this.saveExperimentCheckBox_CheckedChanged);
            // 
            // saveBatchTextBox
            // 
            this.saveBatchTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.saveBatchTextBox.Location = new System.Drawing.Point(166, 107);
            this.saveBatchTextBox.Name = "saveBatchTextBox";
            this.saveBatchTextBox.Size = new System.Drawing.Size(49, 20);
            this.saveBatchTextBox.TabIndex = 10;
            this.saveBatchTextBox.Text = "0";
            // 
            // selectScriptButton
            // 
            this.selectScriptButton.Location = new System.Drawing.Point(589, 55);
            this.selectScriptButton.Name = "selectScriptButton";
            this.selectScriptButton.Size = new System.Drawing.Size(137, 23);
            this.selectScriptButton.TabIndex = 12;
            this.selectScriptButton.Text = "Select Script";
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
            this.menuStrip1.Size = new System.Drawing.Size(731, 24);
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
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(97, 22);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // patternsToolStripMenuItem
            // 
            this.patternsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newPatternToolStripMenuItem,
            this.ReplicateScriptToolStripMenuItem});
            this.patternsToolStripMenuItem.Name = "patternsToolStripMenuItem";
            this.patternsToolStripMenuItem.Size = new System.Drawing.Size(62, 20);
            this.patternsToolStripMenuItem.Text = "Patterns";
            // 
            // newPatternToolStripMenuItem
            // 
            this.newPatternToolStripMenuItem.Name = "newPatternToolStripMenuItem";
            this.newPatternToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.newPatternToolStripMenuItem.Text = "Select script";
            this.newPatternToolStripMenuItem.Click += new System.EventHandler(this.newPatternToolStripMenuItem_Click);
            // 
            // ReplicateScriptToolStripMenuItem
            // 
            this.ReplicateScriptToolStripMenuItem.Name = "ReplicateScriptToolStripMenuItem";
            this.ReplicateScriptToolStripMenuItem.ShowShortcutKeys = false;
            this.ReplicateScriptToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.ReplicateScriptToolStripMenuItem.Text = "Replicate saved run";
            this.ReplicateScriptToolStripMenuItem.Click += new System.EventHandler(this.ReplicateScriptToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Selected Script:";
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(588, 112);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(136, 25);
            this.stopButton.TabIndex = 15;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // iterationsBox
            // 
            this.iterationsBox.Location = new System.Drawing.Point(129, 84);
            this.iterationsBox.Name = "iterationsBox";
            this.iterationsBox.Size = new System.Drawing.Size(46, 20);
            this.iterationsBox.TabIndex = 16;
            this.iterationsBox.Text = "1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(45, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Iterations:";
            // 
            // ControllerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(731, 218);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.iterationsBox);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.selectScriptButton);
            this.Controls.Add(this.saveBatchTextBox);
            this.Controls.Add(this.saveExperimentCheckBox);
            this.Controls.Add(this.runButton);
            this.Controls.Add(this.lookupScriptsButton);
            this.Controls.Add(this.scriptListComboBox);
            this.Controls.Add(this.PatternPathTextBox);
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

        public System.Windows.Forms.TextBox PatternPathTextBox;
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
        private System.Windows.Forms.ToolStripMenuItem ReplicateScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.TextBox iterationsBox;
        private System.Windows.Forms.Label label2;
    }
}

