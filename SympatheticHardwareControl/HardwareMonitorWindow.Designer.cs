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
            this.laserErrorMonitorTextbox = new System.Windows.Forms.TextBox();
            this.laserErrorLED = new NationalInstruments.UI.WindowsForms.Led();
            this.label2 = new System.Windows.Forms.Label();
            this.laserErrorMonitorCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.laserErrorLED)).BeginInit();
            this.SuspendLayout();
            // 
            // chamber1PressureCheckBox
            // 
            this.chamber1PressureCheckBox.AutoSize = true;
            this.chamber1PressureCheckBox.Checked = true;
            this.chamber1PressureCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chamber1PressureCheckBox.Location = new System.Drawing.Point(6, 19);
            this.chamber1PressureCheckBox.Name = "chamber1PressureCheckBox";
            this.chamber1PressureCheckBox.Size = new System.Drawing.Size(120, 17);
            this.chamber1PressureCheckBox.TabIndex = 0;
            this.chamber1PressureCheckBox.Text = "Chamber 1 pressure";
            this.chamber1PressureCheckBox.UseVisualStyleBackColor = true;
            // 
            // chamber1PressureTextBox
            // 
            this.chamber1PressureTextBox.Location = new System.Drawing.Point(155, 17);
            this.chamber1PressureTextBox.Name = "chamber1PressureTextBox";
            this.chamber1PressureTextBox.ReadOnly = true;
            this.chamber1PressureTextBox.Size = new System.Drawing.Size(100, 20);
            this.chamber1PressureTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(261, 20);
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
            this.groupBox1.Location = new System.Drawing.Point(12, 64);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(319, 100);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Pressure Gauges";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.laserErrorLED);
            this.groupBox2.Controls.Add(this.laserErrorMonitorTextbox);
            this.groupBox2.Controls.Add(this.laserErrorMonitorCheckBox);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(319, 46);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Laser error signal";
            // 
            // laserErrorMonitorTextbox
            // 
            this.laserErrorMonitorTextbox.Location = new System.Drawing.Point(155, 17);
            this.laserErrorMonitorTextbox.Name = "laserErrorMonitorTextbox";
            this.laserErrorMonitorTextbox.ReadOnly = true;
            this.laserErrorMonitorTextbox.Size = new System.Drawing.Size(100, 20);
            this.laserErrorMonitorTextbox.TabIndex = 3;
            // 
            // laserErrorLED
            // 
            this.laserErrorLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.laserErrorLED.Location = new System.Drawing.Point(280, 12);
            this.laserErrorLED.Name = "laserErrorLED";
            this.laserErrorLED.Size = new System.Drawing.Size(31, 30);
            this.laserErrorLED.TabIndex = 4;
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
            // HardwareMonitorWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(790, 209);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "HardwareMonitorWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "HardwareMonitorWindow";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.laserErrorLED)).EndInit();
            this.ResumeLayout(false);

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
    }
}