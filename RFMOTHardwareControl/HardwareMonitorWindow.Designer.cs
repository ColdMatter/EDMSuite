namespace RFMOTHardwareControl
{
    partial class HardwareMonitorWindow
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.startAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.monitorFrequenciesCheckBox = new System.Windows.Forms.CheckBox();
            this.laser1FrequencyTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.fieldGradientTB = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.fieldGradientCB = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(222, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startAllToolStripMenuItem,
            this.stopAllToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(62, 20);
            this.toolStripMenuItem1.Text = "Monitor";
            // 
            // startAllToolStripMenuItem
            // 
            this.startAllToolStripMenuItem.Name = "startAllToolStripMenuItem";
            this.startAllToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.startAllToolStripMenuItem.Text = "Start All";
            this.startAllToolStripMenuItem.Click += new System.EventHandler(this.startAllToolStripMenuItem_Click);
            // 
            // stopAllToolStripMenuItem
            // 
            this.stopAllToolStripMenuItem.Name = "stopAllToolStripMenuItem";
            this.stopAllToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.stopAllToolStripMenuItem.Text = "Stop All";
            this.stopAllToolStripMenuItem.Click += new System.EventHandler(this.stopAllToolStripMenuItem_Click);
            // 
            // monitorFrequenciesCheckBox
            // 
            this.monitorFrequenciesCheckBox.AutoSize = true;
            this.monitorFrequenciesCheckBox.Location = new System.Drawing.Point(6, 18);
            this.monitorFrequenciesCheckBox.Name = "monitorFrequenciesCheckBox";
            this.monitorFrequenciesCheckBox.Size = new System.Drawing.Size(61, 17);
            this.monitorFrequenciesCheckBox.TabIndex = 5;
            this.monitorFrequenciesCheckBox.Text = "Monitor";
            this.monitorFrequenciesCheckBox.UseVisualStyleBackColor = true;
            this.monitorFrequenciesCheckBox.CheckedChanged += new System.EventHandler(this.monitorFrequenciesCheckBox_CheckedChanged);
            // 
            // laser1FrequencyTextBox
            // 
            this.laser1FrequencyTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.laser1FrequencyTextBox.Location = new System.Drawing.Point(6, 38);
            this.laser1FrequencyTextBox.Name = "laser1FrequencyTextBox";
            this.laser1FrequencyTextBox.Size = new System.Drawing.Size(117, 32);
            this.laser1FrequencyTextBox.TabIndex = 7;
            this.laser1FrequencyTextBox.TextChanged += new System.EventHandler(this.laser1FrequencyTextBox_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(129, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 26);
            this.label2.TabIndex = 5;
            this.label2.Text = "MHz";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // fieldGradientTB
            // 
            this.fieldGradientTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fieldGradientTB.Location = new System.Drawing.Point(7, 45);
            this.fieldGradientTB.Name = "fieldGradientTB";
            this.fieldGradientTB.Size = new System.Drawing.Size(117, 32);
            this.fieldGradientTB.TabIndex = 8;
            this.fieldGradientTB.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(130, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 26);
            this.label3.TabIndex = 9;
            this.label3.Text = "G/cm";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.fieldGradientCB);
            this.groupBox1.Controls.Add(this.fieldGradientTB);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(9, 135);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(203, 89);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "MOT Field Gradient";
            // 
            // fieldGradientCB
            // 
            this.fieldGradientCB.AutoSize = true;
            this.fieldGradientCB.Location = new System.Drawing.Point(7, 22);
            this.fieldGradientCB.Name = "fieldGradientCB";
            this.fieldGradientCB.Size = new System.Drawing.Size(61, 17);
            this.fieldGradientCB.TabIndex = 10;
            this.fieldGradientCB.Text = "Monitor";
            this.fieldGradientCB.UseVisualStyleBackColor = true;
            this.fieldGradientCB.CheckedChanged += new System.EventHandler(this.fieldGradientCB_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.laser1FrequencyTextBox);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.monitorFrequenciesCheckBox);
            this.groupBox2.Location = new System.Drawing.Point(12, 36);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 78);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Frequency Counter";
            // 
            // HardwareMonitorWindow
            // 
            this.ClientSize = new System.Drawing.Size(222, 236);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "HardwareMonitorWindow";
            this.Text = "Hardware Monitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HardwareMonitorWindow_Close);
            this.Load += new System.EventHandler(this.HardwareMonitorWindow_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem startAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopAllToolStripMenuItem;
        private System.Windows.Forms.CheckBox monitorFrequenciesCheckBox;
        private System.Windows.Forms.TextBox laser1FrequencyTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox fieldGradientTB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox fieldGradientCB;
        private System.Windows.Forms.GroupBox groupBox2;

    }
}