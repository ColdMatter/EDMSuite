namespace DecelerationHardwareControl
{
    partial class ControlWindow
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.LaserTab = new System.Windows.Forms.TabPage();
            this.laserBlockCheckBox = new System.Windows.Forms.CheckBox();
            this.diodeSaturation = new NationalInstruments.UI.WindowsForms.Led();
            this.aomfrequency_label = new System.Windows.Forms.Label();
            this.AomVoltageBox = new System.Windows.Forms.NumericUpDown();
            this.LaserLockCheckBox = new System.Windows.Forms.CheckBox();
            this.EFieldTab = new System.Windows.Forms.TabPage();
            this.DeceleratorTab = new System.Windows.Forms.TabPage();
            this.SynthTab = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.synthOnCheck = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.synthOnAmpBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.synthOnFreqBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.synthSettingsUpdateButton = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.LaserTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.diodeSaturation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AomVoltageBox)).BeginInit();
            this.SynthTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.LaserTab);
            this.tabControl.Controls.Add(this.EFieldTab);
            this.tabControl.Controls.Add(this.DeceleratorTab);
            this.tabControl.Controls.Add(this.SynthTab);
            this.tabControl.Location = new System.Drawing.Point(1, 3);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(355, 251);
            this.tabControl.TabIndex = 0;
            // 
            // LaserTab
            // 
            this.LaserTab.Controls.Add(this.laserBlockCheckBox);
            this.LaserTab.Controls.Add(this.diodeSaturation);
            this.LaserTab.Controls.Add(this.aomfrequency_label);
            this.LaserTab.Controls.Add(this.AomVoltageBox);
            this.LaserTab.Controls.Add(this.LaserLockCheckBox);
            this.LaserTab.Location = new System.Drawing.Point(4, 22);
            this.LaserTab.Name = "LaserTab";
            this.LaserTab.Padding = new System.Windows.Forms.Padding(3);
            this.LaserTab.Size = new System.Drawing.Size(347, 225);
            this.LaserTab.TabIndex = 0;
            this.LaserTab.Text = "Laser";
            this.LaserTab.UseVisualStyleBackColor = true;
            // 
            // laserBlockCheckBox
            // 
            this.laserBlockCheckBox.AutoSize = true;
            this.laserBlockCheckBox.Location = new System.Drawing.Point(17, 44);
            this.laserBlockCheckBox.Name = "laserBlockCheckBox";
            this.laserBlockCheckBox.Size = new System.Drawing.Size(65, 17);
            this.laserBlockCheckBox.TabIndex = 1;
            this.laserBlockCheckBox.Text = "Blocked";
            this.laserBlockCheckBox.UseVisualStyleBackColor = true;
            this.laserBlockCheckBox.CheckedChanged += new System.EventHandler(this.laserBlockCheckBox_CheckedChanged);
            // 
            // diodeSaturation
            // 
            this.diodeSaturation.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.diodeSaturation.Location = new System.Drawing.Point(277, 3);
            this.diodeSaturation.Name = "diodeSaturation";
            this.diodeSaturation.OnColor = System.Drawing.Color.Crimson;
            this.diodeSaturation.Size = new System.Drawing.Size(64, 64);
            this.diodeSaturation.TabIndex = 2;
            // 
            // aomfrequency_label
            // 
            this.aomfrequency_label.AutoSize = true;
            this.aomfrequency_label.Location = new System.Drawing.Point(85, 73);
            this.aomfrequency_label.Name = "aomfrequency_label";
            this.aomfrequency_label.Size = new System.Drawing.Size(115, 13);
            this.aomfrequency_label.TabIndex = 1;
            this.aomfrequency_label.Text = "AOM Frequency (MHz)";
            // 
            // AomVoltageBox
            // 
            this.AomVoltageBox.DecimalPlaces = 2;
            this.AomVoltageBox.Location = new System.Drawing.Point(17, 73);
            this.AomVoltageBox.Maximum = new decimal(new int[] {
            192,
            0,
            0,
            0});
            this.AomVoltageBox.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.AomVoltageBox.Name = "AomVoltageBox";
            this.AomVoltageBox.Size = new System.Drawing.Size(62, 20);
            this.AomVoltageBox.TabIndex = 1;
            this.AomVoltageBox.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.AomVoltageBox.ValueChanged += new System.EventHandler(this.AomVoltageBox_ValueChanged);
            // 
            // LaserLockCheckBox
            // 
            this.LaserLockCheckBox.AutoSize = true;
            this.LaserLockCheckBox.Location = new System.Drawing.Point(17, 21);
            this.LaserLockCheckBox.Name = "LaserLockCheckBox";
            this.LaserLockCheckBox.Size = new System.Drawing.Size(62, 17);
            this.LaserLockCheckBox.TabIndex = 0;
            this.LaserLockCheckBox.Text = "Locked";
            this.LaserLockCheckBox.UseVisualStyleBackColor = true;
            // 
            // EFieldTab
            // 
            this.EFieldTab.Location = new System.Drawing.Point(4, 22);
            this.EFieldTab.Name = "EFieldTab";
            this.EFieldTab.Padding = new System.Windows.Forms.Padding(3);
            this.EFieldTab.Size = new System.Drawing.Size(347, 225);
            this.EFieldTab.TabIndex = 1;
            this.EFieldTab.Text = "Electric Fields";
            this.EFieldTab.UseVisualStyleBackColor = true;
            // 
            // DeceleratorTab
            // 
            this.DeceleratorTab.Location = new System.Drawing.Point(4, 22);
            this.DeceleratorTab.Name = "DeceleratorTab";
            this.DeceleratorTab.Padding = new System.Windows.Forms.Padding(3);
            this.DeceleratorTab.Size = new System.Drawing.Size(347, 225);
            this.DeceleratorTab.TabIndex = 2;
            this.DeceleratorTab.Text = "Decelerator";
            this.DeceleratorTab.UseVisualStyleBackColor = true;
            // 
            // SynthTab
            // 
            this.SynthTab.Controls.Add(this.synthSettingsUpdateButton);
            this.SynthTab.Controls.Add(this.label2);
            this.SynthTab.Controls.Add(this.label1);
            this.SynthTab.Controls.Add(this.synthOnCheck);
            this.SynthTab.Controls.Add(this.label7);
            this.SynthTab.Controls.Add(this.synthOnAmpBox);
            this.SynthTab.Controls.Add(this.label8);
            this.SynthTab.Controls.Add(this.synthOnFreqBox);
            this.SynthTab.Location = new System.Drawing.Point(4, 22);
            this.SynthTab.Name = "SynthTab";
            this.SynthTab.Padding = new System.Windows.Forms.Padding(3);
            this.SynthTab.Size = new System.Drawing.Size(347, 225);
            this.SynthTab.TabIndex = 3;
            this.SynthTab.Text = "Synth";
            this.SynthTab.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(225, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 23);
            this.label1.TabIndex = 30;
            this.label1.Text = "GHz";
            // 
            // synthOnCheck
            // 
            this.synthOnCheck.Location = new System.Drawing.Point(10, 73);
            this.synthOnCheck.Name = "synthOnCheck";
            this.synthOnCheck.Size = new System.Drawing.Size(104, 24);
            this.synthOnCheck.TabIndex = 29;
            this.synthOnCheck.Text = "On/Off";
            this.synthOnCheck.CheckedChanged += new System.EventHandler(this.synthOnCheck_CheckedChanged);
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(7, 47);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(122, 23);
            this.label7.TabIndex = 28;
            this.label7.Text = "Amplitude";
            // 
            // synthOnAmpBox
            // 
            this.synthOnAmpBox.Location = new System.Drawing.Point(156, 44);
            this.synthOnAmpBox.Name = "synthOnAmpBox";
            this.synthOnAmpBox.Size = new System.Drawing.Size(64, 20);
            this.synthOnAmpBox.TabIndex = 25;
            this.synthOnAmpBox.Text = "-6";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(7, 15);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(122, 23);
            this.label8.TabIndex = 27;
            this.label8.Text = "Frequency";
            // 
            // synthOnFreqBox
            // 
            this.synthOnFreqBox.Location = new System.Drawing.Point(156, 12);
            this.synthOnFreqBox.Name = "synthOnFreqBox";
            this.synthOnFreqBox.Size = new System.Drawing.Size(64, 20);
            this.synthOnFreqBox.TabIndex = 24;
            this.synthOnFreqBox.Text = "14";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(225, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 23);
            this.label2.TabIndex = 31;
            this.label2.Text = "dBm";
            // 
            // synthSettingsUpdateButton
            // 
            this.synthSettingsUpdateButton.Location = new System.Drawing.Point(266, 30);
            this.synthSettingsUpdateButton.Name = "synthSettingsUpdateButton";
            this.synthSettingsUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.synthSettingsUpdateButton.TabIndex = 32;
            this.synthSettingsUpdateButton.Text = "Update";
            this.synthSettingsUpdateButton.UseVisualStyleBackColor = true;
            this.synthSettingsUpdateButton.Click += new System.EventHandler(this.synthSettingsUpdateButton_Click);
            // 
            // ControlWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 266);
            this.Controls.Add(this.tabControl);
            this.MaximizeBox = false;
            this.Name = "ControlWindow";
            this.Text = "Decelerator Control";
            this.tabControl.ResumeLayout(false);
            this.LaserTab.ResumeLayout(false);
            this.LaserTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.diodeSaturation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AomVoltageBox)).EndInit();
            this.SynthTab.ResumeLayout(false);
            this.SynthTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage LaserTab;
        private System.Windows.Forms.TabPage EFieldTab;
        private System.Windows.Forms.TabPage DeceleratorTab;
        public System.Windows.Forms.CheckBox LaserLockCheckBox;
        public System.Windows.Forms.NumericUpDown AomVoltageBox;
        private System.Windows.Forms.Label aomfrequency_label;
        public NationalInstruments.UI.WindowsForms.Led diodeSaturation;
        private System.Windows.Forms.CheckBox laserBlockCheckBox;
        private System.Windows.Forms.TabPage SynthTab;
        public System.Windows.Forms.CheckBox synthOnCheck;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.TextBox synthOnAmpBox;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.TextBox synthOnFreqBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.Button synthSettingsUpdateButton;
    }
}

