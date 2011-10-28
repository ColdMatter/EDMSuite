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
            this.lockEnableCheck = new System.Windows.Forms.CheckBox();
            this.SlaveLaserIntensityScatterGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.SlaveDataPlot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.SlaveFitPlot = new NationalInstruments.UI.ScatterPlot();
            this.MasterLaserIntensityScatterGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.MasterDataPlot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis2 = new NationalInstruments.UI.XAxis();
            this.yAxis2 = new NationalInstruments.UI.YAxis();
            this.MasterFitPlot = new NationalInstruments.UI.ScatterPlot();
            this.lockParams = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.setPointIncrementBox = new System.Windows.Forms.TextBox();
            this.GainTextbox = new System.Windows.Forms.TextBox();
            this.VoltageToLaserTextBox = new System.Windows.Forms.TextBox();
            this.setPointAdjustMinusButton = new System.Windows.Forms.Button();
            this.setPointAdjustPlusButton = new System.Windows.Forms.Button();
            this.LaserSetPointTextBox = new System.Windows.Forms.TextBox();
            this.NumberOfScanpointsTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.CavityVoltageReadScatterGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.cavityDataPlot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis3 = new NationalInstruments.UI.XAxis();
            this.yAxis3 = new NationalInstruments.UI.YAxis();
            this.voltageRampControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rampLED)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SlaveLaserIntensityScatterGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MasterLaserIntensityScatterGraph)).BeginInit();
            this.lockParams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CavityVoltageReadScatterGraph)).BeginInit();
            this.SuspendLayout();
            // 
            // voltageRampControl
            // 
            this.voltageRampControl.Controls.Add(this.rampLED);
            this.voltageRampControl.Controls.Add(this.rampStopButton);
            this.voltageRampControl.Controls.Add(this.rampStartButton);
            this.voltageRampControl.Location = new System.Drawing.Point(554, 3);
            this.voltageRampControl.Name = "voltageRampControl";
            this.voltageRampControl.Size = new System.Drawing.Size(355, 76);
            this.voltageRampControl.TabIndex = 2;
            this.voltageRampControl.TabStop = false;
            this.voltageRampControl.Text = "Analog Inputs";
            // 
            // rampLED
            // 
            this.rampLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.rampLED.Location = new System.Drawing.Point(157, 13);
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
            // lockEnableCheck
            // 
            this.lockEnableCheck.AutoSize = true;
            this.lockEnableCheck.Location = new System.Drawing.Point(285, 30);
            this.lockEnableCheck.Name = "lockEnableCheck";
            this.lockEnableCheck.Size = new System.Drawing.Size(50, 17);
            this.lockEnableCheck.TabIndex = 9;
            this.lockEnableCheck.Text = "Lock";
            this.lockEnableCheck.UseVisualStyleBackColor = true;
            this.lockEnableCheck.CheckedChanged += new System.EventHandler(this.lockEnableCheck_CheckedChanged);
            // 
            // SlaveLaserIntensityScatterGraph
            // 
            this.SlaveLaserIntensityScatterGraph.Location = new System.Drawing.Point(0, 19);
            this.SlaveLaserIntensityScatterGraph.Name = "SlaveLaserIntensityScatterGraph";
            this.SlaveLaserIntensityScatterGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.SlaveDataPlot,
            this.SlaveFitPlot});
            this.SlaveLaserIntensityScatterGraph.Size = new System.Drawing.Size(548, 132);
            this.SlaveLaserIntensityScatterGraph.TabIndex = 4;
            this.SlaveLaserIntensityScatterGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.SlaveLaserIntensityScatterGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            //this.SlaveLaserIntensityScatterGraph.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.SlaveLaserIntensityScatterGraph_PlotDataChanged);
            // 
            // SlaveDataPlot
            // 
            this.SlaveDataPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.SlaveDataPlot.PointSize = new System.Drawing.Size(3, 3);
            this.SlaveDataPlot.PointStyle = NationalInstruments.UI.PointStyle.SolidCircle;
            this.SlaveDataPlot.XAxis = this.xAxis1;
            this.SlaveDataPlot.YAxis = this.yAxis1;
            // 
            // SlaveFitPlot
            // 
            this.SlaveFitPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.SlaveFitPlot.PointColor = System.Drawing.Color.LawnGreen;
            this.SlaveFitPlot.PointStyle = NationalInstruments.UI.PointStyle.EmptyTriangleUp;
            this.SlaveFitPlot.XAxis = this.xAxis1;
            this.SlaveFitPlot.YAxis = this.yAxis1;
            // 
            // MasterLaserIntensityScatterGraph
            // 
            this.MasterLaserIntensityScatterGraph.Location = new System.Drawing.Point(3, 168);
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
            // lockParams
            // 
            this.lockParams.Controls.Add(this.label10);
            this.lockParams.Controls.Add(this.setPointIncrementBox);
            this.lockParams.Controls.Add(this.GainTextbox);
            this.lockParams.Controls.Add(this.VoltageToLaserTextBox);
            this.lockParams.Controls.Add(this.setPointAdjustMinusButton);
            this.lockParams.Controls.Add(this.setPointAdjustPlusButton);
            this.lockParams.Controls.Add(this.LaserSetPointTextBox);
            this.lockParams.Controls.Add(this.lockEnableCheck);
            this.lockParams.Controls.Add(this.NumberOfScanpointsTextBox);
            this.lockParams.Controls.Add(this.label5);
            this.lockParams.Controls.Add(this.label4);
            this.lockParams.Controls.Add(this.label2);
            this.lockParams.Controls.Add(this.label3);
            this.lockParams.Location = new System.Drawing.Point(554, 85);
            this.lockParams.Name = "lockParams";
            this.lockParams.Size = new System.Drawing.Size(355, 213);
            this.lockParams.TabIndex = 10;
            this.lockParams.TabStop = false;
            this.lockParams.Text = "Lock Parameters";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 90);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(123, 13);
            this.label10.TabIndex = 33;
            this.label10.Text = "Set Point Increment Size";
            // 
            // setPointIncrementBox
            // 
            this.setPointIncrementBox.Location = new System.Drawing.Point(168, 87);
            this.setPointIncrementBox.Name = "setPointIncrementBox";
            this.setPointIncrementBox.Size = new System.Drawing.Size(55, 20);
            this.setPointIncrementBox.TabIndex = 32;
            this.setPointIncrementBox.TextChanged += new System.EventHandler(this.setPointIncrementBox_TextChanged);
            // 
            // GainTextbox
            // 
            this.GainTextbox.Location = new System.Drawing.Point(167, 28);
            this.GainTextbox.Name = "GainTextbox";
            this.GainTextbox.Size = new System.Drawing.Size(81, 20);
            this.GainTextbox.TabIndex = 31;
            this.GainTextbox.Text = "0";
            this.GainTextbox.TextChanged += new System.EventHandler(this.GainChanged);
            // 
            // VoltageToLaserTextBox
            // 
            this.VoltageToLaserTextBox.Location = new System.Drawing.Point(167, 113);
            this.VoltageToLaserTextBox.Name = "VoltageToLaserTextBox";
            this.VoltageToLaserTextBox.Size = new System.Drawing.Size(100, 20);
            this.VoltageToLaserTextBox.TabIndex = 30;
            this.VoltageToLaserTextBox.Text = "0";
            this.VoltageToLaserTextBox.TextChanged += new System.EventHandler(this.VoltageToLaserChanged);
            // 
            // setPointAdjustMinusButton
            // 
            this.setPointAdjustMinusButton.Location = new System.Drawing.Point(124, 61);
            this.setPointAdjustMinusButton.Name = "setPointAdjustMinusButton";
            this.setPointAdjustMinusButton.Size = new System.Drawing.Size(37, 23);
            this.setPointAdjustMinusButton.TabIndex = 29;
            this.setPointAdjustMinusButton.Text = "-";
            this.setPointAdjustMinusButton.UseVisualStyleBackColor = true;
            this.setPointAdjustMinusButton.Click += new System.EventHandler(this.setPointAdjustMinusButton_Click);
            // 
            // setPointAdjustPlusButton
            // 
            this.setPointAdjustPlusButton.Location = new System.Drawing.Point(81, 61);
            this.setPointAdjustPlusButton.Name = "setPointAdjustPlusButton";
            this.setPointAdjustPlusButton.Size = new System.Drawing.Size(37, 23);
            this.setPointAdjustPlusButton.TabIndex = 28;
            this.setPointAdjustPlusButton.Text = "+";
            this.setPointAdjustPlusButton.UseVisualStyleBackColor = true;
            this.setPointAdjustPlusButton.Click += new System.EventHandler(this.setPointAdjustPlusButton_Click);
            // 
            // LaserSetPointTextBox
            // 
            this.LaserSetPointTextBox.AcceptsReturn = true;
            this.LaserSetPointTextBox.Location = new System.Drawing.Point(167, 63);
            this.LaserSetPointTextBox.Name = "LaserSetPointTextBox";
            this.LaserSetPointTextBox.Size = new System.Drawing.Size(57, 20);
            this.LaserSetPointTextBox.TabIndex = 27;
            // 
            // NumberOfScanpointsTextBox
            // 
            this.NumberOfScanpointsTextBox.Location = new System.Drawing.Point(167, 142);
            this.NumberOfScanpointsTextBox.Name = "NumberOfScanpointsTextBox";
            this.NumberOfScanpointsTextBox.Size = new System.Drawing.Size(57, 20);
            this.NumberOfScanpointsTextBox.TabIndex = 22;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 145);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(90, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "Number of points:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 13);
            this.label4.TabIndex = 20;
            this.label4.Text = "Gain";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(122, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Voltage sent to laser (V):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Set Point (V):";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 3);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Ti:Sapphire";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 152);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(38, 13);
            this.label9.TabIndex = 12;
            this.label9.Text = "He-Ne";
            // 
            // CavityVoltageReadScatterGraph
            // 
            this.CavityVoltageReadScatterGraph.Location = new System.Drawing.Point(3, 304);
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
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(921, 442);
            this.Controls.Add(this.CavityVoltageReadScatterGraph);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.lockParams);
            this.Controls.Add(this.MasterLaserIntensityScatterGraph);
            this.Controls.Add(this.SlaveLaserIntensityScatterGraph);
            this.Controls.Add(this.voltageRampControl);
            this.Name = "MainForm";
            this.Text = "Transfer Cavity Lock 2012";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.voltageRampControl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.rampLED)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SlaveLaserIntensityScatterGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MasterLaserIntensityScatterGraph)).EndInit();
            this.lockParams.ResumeLayout(false);
            this.lockParams.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CavityVoltageReadScatterGraph)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox voltageRampControl;
        private System.Windows.Forms.Button rampStartButton;
        private System.Windows.Forms.Button rampStopButton;
        private NationalInstruments.UI.WindowsForms.Led rampLED;
        public NationalInstruments.UI.WindowsForms.ScatterGraph SlaveLaserIntensityScatterGraph;
        public NationalInstruments.UI.ScatterPlot SlaveDataPlot;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        public NationalInstruments.UI.WindowsForms.ScatterGraph MasterLaserIntensityScatterGraph;
        public NationalInstruments.UI.ScatterPlot MasterDataPlot;
        private NationalInstruments.UI.XAxis xAxis2;
        private NationalInstruments.UI.YAxis yAxis2;
        private System.Windows.Forms.CheckBox lockEnableCheck;
        private System.Windows.Forms.GroupBox lockParams;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button setPointAdjustMinusButton;
        private System.Windows.Forms.Button setPointAdjustPlusButton;
        private System.Windows.Forms.TextBox NumberOfScanpointsTextBox;
        private System.Windows.Forms.TextBox LaserSetPointTextBox;
        private System.Windows.Forms.TextBox GainTextbox;
        private System.Windows.Forms.TextBox VoltageToLaserTextBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox setPointIncrementBox;
        public NationalInstruments.UI.ScatterPlot SlaveFitPlot;
        public NationalInstruments.UI.ScatterPlot MasterFitPlot;
        public NationalInstruments.UI.WindowsForms.ScatterGraph CavityVoltageReadScatterGraph;
        public NationalInstruments.UI.ScatterPlot cavityDataPlot;
        private NationalInstruments.UI.XAxis xAxis3;
        private NationalInstruments.UI.YAxis yAxis3;
    }
}

