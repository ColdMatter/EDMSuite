namespace SympatheticHardwareControl
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
            this.chamber1PressureCheckBox = new System.Windows.Forms.CheckBox();
            this.chamber1PressureTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.laserLockErrorThresholdTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.laserErrorLED = new NationalInstruments.UI.WindowsForms.Led();
            this.laserErrorMonitorTextbox = new System.Windows.Forms.TextBox();
            this.laserErrorMonitorCheckBox = new System.Windows.Forms.CheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.monitorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.laserErrorLED)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chamber1PressureCheckBox
            // 
            this.chamber1PressureCheckBox.AutoSize = true;
            this.chamber1PressureCheckBox.Location = new System.Drawing.Point(6, 19);
            this.chamber1PressureCheckBox.Name = "chamber1PressureCheckBox";
            this.chamber1PressureCheckBox.Size = new System.Drawing.Size(115, 17);
            this.chamber1PressureCheckBox.TabIndex = 0;
            this.chamber1PressureCheckBox.Text = "Chamber 1 voltage";
            this.chamber1PressureCheckBox.UseVisualStyleBackColor = true;
            this.chamber1PressureCheckBox.CheckedChanged += new System.EventHandler(this.chamber1PressureCheckBox_CheckedChanged);
            // 
            // chamber1PressureTextBox
            // 
            this.chamber1PressureTextBox.Location = new System.Drawing.Point(155, 17);
            this.chamber1PressureTextBox.Name = "chamber1PressureTextBox";
            this.chamber1PressureTextBox.ReadOnly = true;
            this.chamber1PressureTextBox.Size = new System.Drawing.Size(305, 20);
            this.chamber1PressureTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(466, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "mbar";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chamber1PressureCheckBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.chamber1PressureTextBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 80);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(502, 133);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Pressure Gauges";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.laserLockErrorThresholdTextBox);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.laserErrorLED);
            this.groupBox2.Controls.Add(this.laserErrorMonitorTextbox);
            this.groupBox2.Controls.Add(this.laserErrorMonitorCheckBox);
            this.groupBox2.Location = new System.Drawing.Point(12, 28);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(507, 46);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Laser error signal";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(293, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Threshold";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(459, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "V";
            // 
            // laserLockErrorThresholdTextBox
            // 
            this.laserLockErrorThresholdTextBox.Location = new System.Drawing.Point(353, 17);
            this.laserLockErrorThresholdTextBox.Name = "laserLockErrorThresholdTextBox";
            this.laserLockErrorThresholdTextBox.Size = new System.Drawing.Size(100, 20);
            this.laserLockErrorThresholdTextBox.TabIndex = 5;
            this.laserLockErrorThresholdTextBox.Text = "0.1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(261, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "V";
            // 
            // laserErrorLED
            // 
            this.laserErrorLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.laserErrorLED.Location = new System.Drawing.Point(471, 10);
            this.laserErrorLED.Name = "laserErrorLED";
            this.laserErrorLED.Size = new System.Drawing.Size(31, 30);
            this.laserErrorLED.TabIndex = 4;
            // 
            // laserErrorMonitorTextbox
            // 
            this.laserErrorMonitorTextbox.Location = new System.Drawing.Point(155, 17);
            this.laserErrorMonitorTextbox.Name = "laserErrorMonitorTextbox";
            this.laserErrorMonitorTextbox.ReadOnly = true;
            this.laserErrorMonitorTextbox.Size = new System.Drawing.Size(100, 20);
            this.laserErrorMonitorTextbox.TabIndex = 3;
            // 
            // laserErrorMonitorCheckBox
            // 
            this.laserErrorMonitorCheckBox.AutoSize = true;
            this.laserErrorMonitorCheckBox.Location = new System.Drawing.Point(6, 19);
            this.laserErrorMonitorCheckBox.Name = "laserErrorMonitorCheckBox";
            this.laserErrorMonitorCheckBox.Size = new System.Drawing.Size(143, 17);
            this.laserErrorMonitorCheckBox.TabIndex = 0;
            this.laserErrorMonitorCheckBox.Text = "Monitor laser Error Signal";
            this.laserErrorMonitorCheckBox.UseVisualStyleBackColor = true;
            this.laserErrorMonitorCheckBox.CheckedChanged += new System.EventHandler(this.laserErrorMonitorCheckBox_CheckedChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.monitorToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(522, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // monitorToolStripMenuItem
            // 
            this.monitorToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startAllToolStripMenuItem,
            this.stopAllToolStripMenuItem});
            this.monitorToolStripMenuItem.Name = "monitorToolStripMenuItem";
            this.monitorToolStripMenuItem.Size = new System.Drawing.Size(62, 20);
            this.monitorToolStripMenuItem.Text = "Monitor";
            // 
            // startAllToolStripMenuItem
            // 
            this.startAllToolStripMenuItem.Name = "startAllToolStripMenuItem";
            this.startAllToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.startAllToolStripMenuItem.Text = "Start All";
            this.startAllToolStripMenuItem.Click += new System.EventHandler(this.startAllToolStripMenuItem_Click);
            // 
            // stopAllToolStripMenuItem
            // 
            this.stopAllToolStripMenuItem.Name = "stopAllToolStripMenuItem";
            this.stopAllToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.stopAllToolStripMenuItem.Text = "Stop All";
            this.stopAllToolStripMenuItem.Click += new System.EventHandler(this.stopAllToolStripMenuItem_Click);
            // 
            // HardwareMonitorWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(522, 221);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "HardwareMonitorWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Hardware Monitor Window";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HardwareMonitorWindow_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.laserErrorLED)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chamber1PressureCheckBox;
        private System.Windows.Forms.TextBox chamber1PressureTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox laserErrorMonitorTextbox;
        private NationalInstruments.UI.WindowsForms.Led laserErrorLED;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox laserErrorMonitorCheckBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox laserLockErrorThresholdTextBox;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem monitorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopAllToolStripMenuItem;
    }
}