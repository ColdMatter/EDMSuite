namespace TransferCavityLock2012
{
    partial class MainForm
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
            this.voltageRampControl = new System.Windows.Forms.GroupBox();
            this.rampLED = new NationalInstruments.UI.WindowsForms.Led();
            this.rampStopButton = new System.Windows.Forms.Button();
            this.rampStartButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.NumberOfScanpointsTextBox = new System.Windows.Forms.TextBox();
            this.MasterLaserIntensityScatterGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.MasterDataPlot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis2 = new NationalInstruments.UI.XAxis();
            this.yAxis2 = new NationalInstruments.UI.YAxis();
            this.MasterFitPlot = new NationalInstruments.UI.ScatterPlot();
            this.CavityVoltageReadScatterGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.cavityDataPlot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis3 = new NationalInstruments.UI.XAxis();
            this.yAxis3 = new NationalInstruments.UI.YAxis();
            this.slaveLasersTab = new System.Windows.Forms.TabControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.logCheckBox = new System.Windows.Forms.CheckBox();
            this.MasterSetPointTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.masterLockEnableCheck = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.MasterGainTextBox = new System.Windows.Forms.TextBox();
            this.VToOffsetTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.MasterFitTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.CavLockVoltageTrackBar = new System.Windows.Forms.TrackBar();
            this.label7 = new System.Windows.Forms.Label();
            this.voltageRampControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rampLED)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MasterLaserIntensityScatterGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CavityVoltageReadScatterGraph)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CavLockVoltageTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // voltageRampControl
            // 
            this.voltageRampControl.Controls.Add(this.rampLED);
            this.voltageRampControl.Controls.Add(this.rampStopButton);
            this.voltageRampControl.Controls.Add(this.rampStartButton);
            this.voltageRampControl.Controls.Add(this.label5);
            this.voltageRampControl.Controls.Add(this.NumberOfScanpointsTextBox);
            this.voltageRampControl.Location = new System.Drawing.Point(580, 12);
            this.voltageRampControl.Name = "voltageRampControl";
            this.voltageRampControl.Size = new System.Drawing.Size(355, 76);
            this.voltageRampControl.TabIndex = 2;
            this.voltageRampControl.TabStop = false;
            this.voltageRampControl.Text = "Analog Inputs";
            // 
            // rampLED
            // 
            this.rampLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.rampLED.Location = new System.Drawing.Point(318, 13);
            this.rampLED.Name = "rampLED";
            this.rampLED.OffColor = System.Drawing.Color.Red;
            this.rampLED.Size = new System.Drawing.Size(31, 29);
            this.rampLED.TabIndex = 7;
            // 
            // rampStopButton
            // 
            this.rampStopButton.Location = new System.Drawing.Point(6, 46);
            this.rampStopButton.Name = "rampStopButton";
            this.rampStopButton.Size = new System.Drawing.Size(87, 23);
            this.rampStopButton.TabIndex = 6;
            this.rampStopButton.Text = "Stop reading";
            this.rampStopButton.UseVisualStyleBackColor = true;
            this.rampStopButton.Click += new System.EventHandler(this.rampStopButton_Click);
            // 
            // rampStartButton
            // 
            this.rampStartButton.Location = new System.Drawing.Point(6, 19);
            this.rampStartButton.Name = "rampStartButton";
            this.rampStartButton.Size = new System.Drawing.Size(87, 23);
            this.rampStartButton.TabIndex = 2;
            this.rampStartButton.Text = "Start reading";
            this.rampStartButton.UseVisualStyleBackColor = true;
            this.rampStartButton.Click += new System.EventHandler(this.rampStartButton_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(99, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(90, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "Number of points:";
            // 
            // NumberOfScanpointsTextBox
            // 
            this.NumberOfScanpointsTextBox.Location = new System.Drawing.Point(195, 21);
            this.NumberOfScanpointsTextBox.Name = "NumberOfScanpointsTextBox";
            this.NumberOfScanpointsTextBox.Size = new System.Drawing.Size(57, 20);
            this.NumberOfScanpointsTextBox.TabIndex = 22;
            // 
            // MasterLaserIntensityScatterGraph
            // 
            this.MasterLaserIntensityScatterGraph.Location = new System.Drawing.Point(6, 19);
            this.MasterLaserIntensityScatterGraph.Name = "MasterLaserIntensityScatterGraph";
            this.MasterLaserIntensityScatterGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.MasterDataPlot,
            this.MasterFitPlot});
            this.MasterLaserIntensityScatterGraph.Size = new System.Drawing.Size(548, 130);
            this.MasterLaserIntensityScatterGraph.TabIndex = 5;
            this.MasterLaserIntensityScatterGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis2});
            this.MasterLaserIntensityScatterGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis2});
            // 
            // MasterDataPlot
            // 
            this.MasterDataPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.MasterDataPlot.PointSize = new System.Drawing.Size(2, 2);
            this.MasterDataPlot.PointStyle = NationalInstruments.UI.PointStyle.SolidCircle;
            this.MasterDataPlot.XAxis = this.xAxis2;
            this.MasterDataPlot.YAxis = this.yAxis2;
            // 
            // MasterFitPlot
            // 
            this.MasterFitPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.MasterFitPlot.PointColor = System.Drawing.Color.LawnGreen;
            this.MasterFitPlot.PointStyle = NationalInstruments.UI.PointStyle.EmptyTriangleUp;
            this.MasterFitPlot.XAxis = this.xAxis2;
            this.MasterFitPlot.YAxis = this.yAxis2;
            // 
            // CavityVoltageReadScatterGraph
            // 
            this.CavityVoltageReadScatterGraph.Location = new System.Drawing.Point(6, 19);
            this.CavityVoltageReadScatterGraph.Name = "CavityVoltageReadScatterGraph";
            this.CavityVoltageReadScatterGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.cavityDataPlot});
            this.CavityVoltageReadScatterGraph.Size = new System.Drawing.Size(548, 130);
            this.CavityVoltageReadScatterGraph.TabIndex = 13;
            this.CavityVoltageReadScatterGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis3});
            this.CavityVoltageReadScatterGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis3});
            // 
            // cavityDataPlot
            // 
            this.cavityDataPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.cavityDataPlot.PointSize = new System.Drawing.Size(2, 2);
            this.cavityDataPlot.PointStyle = NationalInstruments.UI.PointStyle.SolidCircle;
            this.cavityDataPlot.XAxis = this.xAxis3;
            this.cavityDataPlot.YAxis = this.yAxis3;
            // 
            // slaveLasersTab
            // 
            this.slaveLasersTab.Location = new System.Drawing.Point(12, 338);
            this.slaveLasersTab.Name = "slaveLasersTab";
            this.slaveLasersTab.SelectedIndex = 0;
            this.slaveLasersTab.Size = new System.Drawing.Size(952, 334);
            this.slaveLasersTab.TabIndex = 15;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.CavityVoltageReadScatterGraph);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(562, 158);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cavity Scan Voltage";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.MasterLaserIntensityScatterGraph);
            this.groupBox2.Location = new System.Drawing.Point(12, 176);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(562, 156);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Reference laser";
            // 
            // logCheckBox
            // 
            this.logCheckBox.AutoSize = true;
            this.logCheckBox.Location = new System.Drawing.Point(586, 315);
            this.logCheckBox.Name = "logCheckBox";
            this.logCheckBox.Size = new System.Drawing.Size(124, 17);
            this.logCheckBox.TabIndex = 18;
            this.logCheckBox.Text = "Log laser parameters";
            this.logCheckBox.UseVisualStyleBackColor = true;
            this.logCheckBox.CheckedChanged += new System.EventHandler(this.logCheckBox_CheckedChanged);
            // 
            // MasterSetPointTextBox
            // 
            this.MasterSetPointTextBox.AcceptsReturn = true;
            this.MasterSetPointTextBox.Location = new System.Drawing.Point(658, 193);
            this.MasterSetPointTextBox.Name = "MasterSetPointTextBox";
            this.MasterSetPointTextBox.Size = new System.Drawing.Size(57, 20);
            this.MasterSetPointTextBox.TabIndex = 29;
            this.MasterSetPointTextBox.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(583, 196);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 28;
            this.label3.Text = "Set Point (V):";
            // 
            // masterLockEnableCheck
            // 
            this.masterLockEnableCheck.AutoSize = true;
            this.masterLockEnableCheck.Location = new System.Drawing.Point(721, 195);
            this.masterLockEnableCheck.Name = "masterLockEnableCheck";
            this.masterLockEnableCheck.Size = new System.Drawing.Size(50, 17);
            this.masterLockEnableCheck.TabIndex = 35;
            this.masterLockEnableCheck.Text = "Lock";
            this.masterLockEnableCheck.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(583, 221);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 37;
            this.label1.Text = "Gain:";
            // 
            // MasterGainTextBox
            // 
            this.MasterGainTextBox.AcceptsReturn = true;
            this.MasterGainTextBox.Location = new System.Drawing.Point(658, 221);
            this.MasterGainTextBox.Name = "MasterGainTextBox";
            this.MasterGainTextBox.Size = new System.Drawing.Size(57, 20);
            this.MasterGainTextBox.TabIndex = 38;
            this.MasterGainTextBox.Text = "0";
            // 
            // VToOffsetTextBox
            // 
            this.VToOffsetTextBox.CausesValidation = false;
            this.VToOffsetTextBox.Location = new System.Drawing.Point(871, 221);
            this.VToOffsetTextBox.Name = "VToOffsetTextBox";
            this.VToOffsetTextBox.Size = new System.Drawing.Size(93, 20);
            this.VToOffsetTextBox.TabIndex = 39;
            //this.VToOffsetTextBox.TextChanged += new System.EventHandler(this.VToOffsetTextBox_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(773, 224);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 40;
            this.label2.Text = "Summed Voltage";
            // 
            // MasterFitTextBox
            // 
            this.MasterFitTextBox.CausesValidation = false;
            this.MasterFitTextBox.Location = new System.Drawing.Point(871, 193);
            this.MasterFitTextBox.Name = "MasterFitTextBox";
            this.MasterFitTextBox.ReadOnly = true;
            this.MasterFitTextBox.Size = new System.Drawing.Size(93, 20);
            this.MasterFitTextBox.TabIndex = 41;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(773, 187);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 13);
            this.label4.TabIndex = 42;
            this.label4.Text = "Voltage into Cavity";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(773, 200);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 13);
            this.label6.TabIndex = 43;
            this.label6.Text = "at Ref Peak";
            // 
            // CavLockVoltageTrackBar
            // 
            this.CavLockVoltageTrackBar.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CavLockVoltageTrackBar.Location = new System.Drawing.Point(580, 263);
            this.CavLockVoltageTrackBar.Maximum = 1000;
            this.CavLockVoltageTrackBar.Minimum = -1000;
            this.CavLockVoltageTrackBar.Name = "CavLockVoltageTrackBar";
            this.CavLockVoltageTrackBar.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.CavLockVoltageTrackBar.Size = new System.Drawing.Size(384, 45);
            this.CavLockVoltageTrackBar.TabIndex = 52;
            this.CavLockVoltageTrackBar.Scroll += new System.EventHandler(this.CavLockVoltageTrackBar_Scroll);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(586, 247);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(87, 13);
            this.label7.TabIndex = 53;
            this.label7.Text = "Summed Voltage";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(976, 677);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.CavLockVoltageTrackBar);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.MasterFitTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.VToOffsetTextBox);
            this.Controls.Add(this.MasterGainTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.masterLockEnableCheck);
            this.Controls.Add(this.MasterSetPointTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.logCheckBox);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.slaveLasersTab);
            this.Controls.Add(this.voltageRampControl);
            this.Name = "MainForm";
            this.Text = "Transfer Cavity Lock 2012";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.voltageRampControl.ResumeLayout(false);
            this.voltageRampControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rampLED)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MasterLaserIntensityScatterGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CavityVoltageReadScatterGraph)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CavLockVoltageTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox voltageRampControl;
        private System.Windows.Forms.Button rampStartButton;
        private System.Windows.Forms.Button rampStopButton;
        private NationalInstruments.UI.WindowsForms.Led rampLED;
        public NationalInstruments.UI.WindowsForms.ScatterGraph MasterLaserIntensityScatterGraph;
        public NationalInstruments.UI.ScatterPlot MasterDataPlot;
        private NationalInstruments.UI.XAxis xAxis2;
        private NationalInstruments.UI.YAxis yAxis2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox NumberOfScanpointsTextBox;
        public NationalInstruments.UI.ScatterPlot MasterFitPlot;
        public NationalInstruments.UI.WindowsForms.ScatterGraph CavityVoltageReadScatterGraph;
        public NationalInstruments.UI.ScatterPlot cavityDataPlot;
        private NationalInstruments.UI.XAxis xAxis3;
        private NationalInstruments.UI.YAxis yAxis3;
        private System.Windows.Forms.TabControl slaveLasersTab;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.CheckBox logCheckBox;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.CheckBox masterLockEnableCheck;
        public System.Windows.Forms.TextBox MasterSetPointTextBox;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox MasterGainTextBox;
        public System.Windows.Forms.TextBox VToOffsetTextBox;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox MasterFitTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.TrackBar CavLockVoltageTrackBar;
        private System.Windows.Forms.Label label7;
    }
}

