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
            this.SynthTab = new System.Windows.Forms.TabPage();
            this.synthSettingsUpdateButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.synthOnCheck = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.synthOnAmpBox = new System.Windows.Forms.TextBox();
            this.synthOnFreqBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.DeceleratorTab = new System.Windows.Forms.TabPage();
            this.EFieldTab = new System.Windows.Forms.TabPage();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.LaserTab = new System.Windows.Forms.TabPage();
            this.laserBlockCheckBox = new System.Windows.Forms.CheckBox();
            this.diodeSaturation = new NationalInstruments.UI.WindowsForms.Led();
            this.aomfrequency_label = new System.Windows.Forms.Label();
            this.AomVoltageBox = new System.Windows.Forms.NumericUpDown();
            this.LaserLockCheckBox = new System.Windows.Forms.CheckBox();
            this.MonitoringTab = new System.Windows.Forms.TabPage();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.monitorColdPlate = new System.Windows.Forms.TextBox();
            this.monitorShield = new System.Windows.Forms.TextBox();
            this.monitor10KTherm30KPlate = new System.Windows.Forms.TextBox();
            this.monitorRoughVacuum = new System.Windows.Forms.TextBox();
            this.GetData = new System.Windows.Forms.Button();
            this.monitorPressureSourceChamber = new System.Windows.Forms.TextBox();
            this.PressureSourceChamber = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.FlowController = new System.Windows.Forms.TabPage();
            this.ReadFlow = new System.Windows.Forms.Button();
            this.SetFlow = new System.Windows.Forms.Button();
            this.CommandBox = new System.Windows.Forms.TextBox();
            this.ReturnBox = new System.Windows.Forms.TextBox();
            this.SynthTab.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.LaserTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.diodeSaturation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AomVoltageBox)).BeginInit();
            this.MonitoringTab.SuspendLayout();
            this.FlowController.SuspendLayout();
            this.SuspendLayout();
            // 
            // SynthTab
            // 
            this.SynthTab.Controls.Add(this.synthSettingsUpdateButton);
            this.SynthTab.Controls.Add(this.label2);
            this.SynthTab.Controls.Add(this.label1);
            this.SynthTab.Controls.Add(this.synthOnCheck);
            this.SynthTab.Controls.Add(this.label7);
            this.SynthTab.Controls.Add(this.synthOnAmpBox);
            this.SynthTab.Controls.Add(this.synthOnFreqBox);
            this.SynthTab.Controls.Add(this.label8);
            this.SynthTab.Location = new System.Drawing.Point(4, 22);
            this.SynthTab.Name = "SynthTab";
            this.SynthTab.Padding = new System.Windows.Forms.Padding(3);
            this.SynthTab.Size = new System.Drawing.Size(347, 225);
            this.SynthTab.TabIndex = 3;
            this.SynthTab.Text = "Synth";
            this.SynthTab.UseVisualStyleBackColor = true;
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
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(225, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 23);
            this.label2.TabIndex = 31;
            this.label2.Text = "dBm";
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
            // synthOnFreqBox
            // 
            this.synthOnFreqBox.Location = new System.Drawing.Point(156, 12);
            this.synthOnFreqBox.Name = "synthOnFreqBox";
            this.synthOnFreqBox.Size = new System.Drawing.Size(64, 20);
            this.synthOnFreqBox.TabIndex = 24;
            this.synthOnFreqBox.Text = "14";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(7, 15);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(122, 23);
            this.label8.TabIndex = 27;
            this.label8.Text = "Frequency";
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
            // tabControl
            // 
            this.tabControl.Controls.Add(this.LaserTab);
            this.tabControl.Controls.Add(this.EFieldTab);
            this.tabControl.Controls.Add(this.DeceleratorTab);
            this.tabControl.Controls.Add(this.SynthTab);
            this.tabControl.Controls.Add(this.MonitoringTab);
            this.tabControl.Controls.Add(this.FlowController);
            this.tabControl.Location = new System.Drawing.Point(1, 3);
            this.tabControl.Multiline = true;
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
            this.LaserTab.Location = new System.Drawing.Point(4, 40);
            this.LaserTab.Name = "LaserTab";
            this.LaserTab.Padding = new System.Windows.Forms.Padding(3);
            this.LaserTab.Size = new System.Drawing.Size(347, 207);
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
            this.aomfrequency_label.Size = new System.Drawing.Size(0, 13);
            this.aomfrequency_label.TabIndex = 1;
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
            // MonitoringTab
            // 
            this.MonitoringTab.Controls.Add(this.label12);
            this.MonitoringTab.Controls.Add(this.label11);
            this.MonitoringTab.Controls.Add(this.label14);
            this.MonitoringTab.Controls.Add(this.label13);
            this.MonitoringTab.Controls.Add(this.label10);
            this.MonitoringTab.Controls.Add(this.label9);
            this.MonitoringTab.Controls.Add(this.label6);
            this.MonitoringTab.Controls.Add(this.label5);
            this.MonitoringTab.Controls.Add(this.label4);
            this.MonitoringTab.Controls.Add(this.monitorColdPlate);
            this.MonitoringTab.Controls.Add(this.monitorShield);
            this.MonitoringTab.Controls.Add(this.monitor10KTherm30KPlate);
            this.MonitoringTab.Controls.Add(this.monitorRoughVacuum);
            this.MonitoringTab.Controls.Add(this.GetData);
            this.MonitoringTab.Controls.Add(this.monitorPressureSourceChamber);
            this.MonitoringTab.Controls.Add(this.PressureSourceChamber);
            this.MonitoringTab.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.MonitoringTab.Location = new System.Drawing.Point(4, 22);
            this.MonitoringTab.Name = "MonitoringTab";
            this.MonitoringTab.Size = new System.Drawing.Size(347, 225);
            this.MonitoringTab.TabIndex = 4;
            this.MonitoringTab.Text = "Monitor";
            this.MonitoringTab.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Symbol", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.label12.Location = new System.Drawing.Point(297, 150);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(28, 27);
            this.label12.TabIndex = 17;
            this.label12.Text = "W";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Symbol", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.label11.Location = new System.Drawing.Point(297, 174);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(28, 27);
            this.label11.TabIndex = 16;
            this.label11.Text = "W";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(288, 87);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(53, 24);
            this.label14.TabIndex = 15;
            this.label14.Text = "mbar";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(288, 58);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(53, 24);
            this.label13.TabIndex = 14;
            this.label13.Text = "mbar";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Symbol", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.label10.Location = new System.Drawing.Point(297, 123);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(28, 27);
            this.label10.TabIndex = 11;
            this.label10.Text = "W";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(11, 182);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(55, 16);
            this.label9.TabIndex = 10;
            this.label9.Text = "4K Cell :";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(11, 158);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 16);
            this.label6.TabIndex = 9;
            this.label6.Text = "30K Shield :";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(10, 131);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(162, 16);
            this.label5.TabIndex = 8;
            this.label5.Text = "10K Thermistor 30K Plate :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(10, 60);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(106, 16);
            this.label4.TabIndex = 7;
            this.label4.Text = "Rough Vacuum :";
            // 
            // monitorColdPlate
            // 
            this.monitorColdPlate.Location = new System.Drawing.Point(182, 180);
            this.monitorColdPlate.Name = "monitorColdPlate";
            this.monitorColdPlate.Size = new System.Drawing.Size(100, 20);
            this.monitorColdPlate.TabIndex = 6;
            // 
            // monitorShield
            // 
            this.monitorShield.Location = new System.Drawing.Point(182, 152);
            this.monitorShield.Name = "monitorShield";
            this.monitorShield.Size = new System.Drawing.Size(100, 20);
            this.monitorShield.TabIndex = 5;
            // 
            // monitor10KTherm30KPlate
            // 
            this.monitor10KTherm30KPlate.Location = new System.Drawing.Point(182, 125);
            this.monitor10KTherm30KPlate.Name = "monitor10KTherm30KPlate";
            this.monitor10KTherm30KPlate.Size = new System.Drawing.Size(100, 20);
            this.monitor10KTherm30KPlate.TabIndex = 4;
            // 
            // monitorRoughVacuum
            // 
            this.monitorRoughVacuum.Location = new System.Drawing.Point(182, 58);
            this.monitorRoughVacuum.Name = "monitorRoughVacuum";
            this.monitorRoughVacuum.Size = new System.Drawing.Size(100, 20);
            this.monitorRoughVacuum.TabIndex = 3;
            // 
            // GetData
            // 
            this.GetData.Location = new System.Drawing.Point(127, 16);
            this.GetData.Name = "GetData";
            this.GetData.Size = new System.Drawing.Size(75, 23);
            this.GetData.TabIndex = 2;
            this.GetData.Text = "READ";
            this.GetData.UseVisualStyleBackColor = true;
            this.GetData.Click += new System.EventHandler(this.GetData_Click);
            // 
            // monitorPressureSourceChamber
            // 
            this.monitorPressureSourceChamber.Location = new System.Drawing.Point(182, 87);
            this.monitorPressureSourceChamber.Name = "monitorPressureSourceChamber";
            this.monitorPressureSourceChamber.Size = new System.Drawing.Size(100, 20);
            this.monitorPressureSourceChamber.TabIndex = 1;
            // 
            // PressureSourceChamber
            // 
            this.PressureSourceChamber.AutoSize = true;
            this.PressureSourceChamber.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PressureSourceChamber.Location = new System.Drawing.Point(8, 91);
            this.PressureSourceChamber.Name = "PressureSourceChamber";
            this.PressureSourceChamber.Size = new System.Drawing.Size(172, 16);
            this.PressureSourceChamber.TabIndex = 0;
            this.PressureSourceChamber.Text = "Pressure Source Chamber :";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 23);
            this.label3.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            // 
            // FlowController
            // 
            this.FlowController.Controls.Add(this.ReturnBox);
            this.FlowController.Controls.Add(this.CommandBox);
            this.FlowController.Controls.Add(this.SetFlow);
            this.FlowController.Controls.Add(this.ReadFlow);
            this.FlowController.Location = new System.Drawing.Point(4, 40);
            this.FlowController.Name = "FlowController";
            this.FlowController.Padding = new System.Windows.Forms.Padding(3);
            this.FlowController.Size = new System.Drawing.Size(347, 207);
            this.FlowController.TabIndex = 5;
            this.FlowController.Text = " Flow Controller";
            this.FlowController.UseVisualStyleBackColor = true;
            // 
            // ReadFlow
            // 
            this.ReadFlow.Location = new System.Drawing.Point(73, 19);
            this.ReadFlow.Name = "ReadFlow";
            this.ReadFlow.Size = new System.Drawing.Size(64, 28);
            this.ReadFlow.TabIndex = 0;
            this.ReadFlow.Text = "Read";
            this.ReadFlow.UseVisualStyleBackColor = true;
            this.ReadFlow.Click += new System.EventHandler(this.ReadFlow_Click);
            // 
            // SetFlow
            // 
            this.SetFlow.Location = new System.Drawing.Point(205, 19);
            this.SetFlow.Name = "SetFlow";
            this.SetFlow.Size = new System.Drawing.Size(64, 28);
            this.SetFlow.TabIndex = 1;
            this.SetFlow.Text = "Set";
            this.SetFlow.UseVisualStyleBackColor = true;
            this.SetFlow.Click += new System.EventHandler(this.SetFlow_Click);
            // 
            // CommandBox
            // 
            this.CommandBox.Location = new System.Drawing.Point(201, 67);
            this.CommandBox.Name = "CommandBox";
            this.CommandBox.Size = new System.Drawing.Size(95, 20);
            this.CommandBox.TabIndex = 2;
            // 
            // ReturnBox
            // 
            this.ReturnBox.Location = new System.Drawing.Point(56, 67);
            this.ReturnBox.Name = "ReturnBox";
            this.ReturnBox.Size = new System.Drawing.Size(95, 20);
            this.ReturnBox.TabIndex = 3;
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
            this.SynthTab.ResumeLayout(false);
            this.SynthTab.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.LaserTab.ResumeLayout(false);
            this.LaserTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.diodeSaturation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AomVoltageBox)).EndInit();
            this.MonitoringTab.ResumeLayout(false);
            this.MonitoringTab.PerformLayout();
            this.FlowController.ResumeLayout(false);
            this.FlowController.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage SynthTab;
        public System.Windows.Forms.Button synthSettingsUpdateButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.CheckBox synthOnCheck;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.TextBox synthOnAmpBox;
        public System.Windows.Forms.TextBox synthOnFreqBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TabPage DeceleratorTab;
        private System.Windows.Forms.TabPage EFieldTab;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage LaserTab;
        private System.Windows.Forms.CheckBox laserBlockCheckBox;
        public NationalInstruments.UI.WindowsForms.Led diodeSaturation;
        private System.Windows.Forms.Label aomfrequency_label;
        public System.Windows.Forms.NumericUpDown AomVoltageBox;
        public System.Windows.Forms.CheckBox LaserLockCheckBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabPage MonitoringTab;
        private System.Windows.Forms.Button GetData;
        public System.Windows.Forms.TextBox monitorPressureSourceChamber;
        private System.Windows.Forms.Label PressureSourceChamber;
        public System.Windows.Forms.TextBox monitorColdPlate;
        public System.Windows.Forms.TextBox monitorShield;
        public System.Windows.Forms.TextBox monitor10KTherm30KPlate;
        public System.Windows.Forms.TextBox monitorRoughVacuum;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TabPage FlowController;
        private System.Windows.Forms.Button SetFlow;
        private System.Windows.Forms.Button ReadFlow;
        public System.Windows.Forms.TextBox ReturnBox;
        public System.Windows.Forms.TextBox CommandBox;

    }
}

