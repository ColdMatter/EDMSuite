using NationalInstruments.UI;

namespace CaFBECHardwareController.Controls
{
    partial class SourceTabView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Title title2 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.xAxis2 = new NationalInstruments.UI.XAxis();
            this.yAxis2 = new NationalInstruments.UI.YAxis();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.GraphAbs = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.GraphPMT = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chkAutoScaleAbs = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbSamplingRateAbs = new System.Windows.Forms.ComboBox();
            this.chkToFAbs = new System.Windows.Forms.CheckBox();
            this.chkSaveTraceAbs = new System.Windows.Forms.CheckBox();
            this.chkAutoScale = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbSamplingRate = new System.Windows.Forms.ComboBox();
            this.chkToF = new System.Windows.Forms.CheckBox();
            this.chkSaveTrace = new System.Windows.Forms.CheckBox();
            this.xAxis3 = new NationalInstruments.UI.XAxis();
            this.yAxis3 = new NationalInstruments.UI.YAxis();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.readButton = new System.Windows.Forms.Button();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GraphAbs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GraphPMT)).BeginInit();
            this.SuspendLayout();
            // 
            // xAxis2
            // 
            this.xAxis2.Caption = "Time (ms)";
            // 
            // yAxis2
            // 
            this.yAxis2.Caption = "Voltage (V)";
            this.yAxis2.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.yAxis2.Range = new NationalInstruments.UI.Range(0D, 1D);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.GraphAbs);
            this.groupBox3.Controls.Add(this.GraphPMT);
            this.groupBox3.Controls.Add(this.chkAutoScaleAbs);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.cmbSamplingRateAbs);
            this.groupBox3.Controls.Add(this.chkToFAbs);
            this.groupBox3.Controls.Add(this.chkSaveTraceAbs);
            this.groupBox3.Controls.Add(this.chkAutoScale);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.cmbSamplingRate);
            this.groupBox3.Controls.Add(this.chkToF);
            this.groupBox3.Controls.Add(this.chkSaveTrace);
            this.groupBox3.Location = new System.Drawing.Point(3, 7);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(681, 692);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Time of flight";
            // 
            // GraphAbs
            // 
            this.GraphAbs.BackColor = System.Drawing.Color.Black;
            this.GraphAbs.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartArea1.AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.AxisX.InterlacedColor = System.Drawing.Color.Black;
            chartArea1.AxisX.IsStartedFromZero = false;
            chartArea1.AxisX.LabelStyle.ForeColor = System.Drawing.Color.White;
            chartArea1.AxisX.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.MajorGrid.Enabled = false;
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.MajorTickMark.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.MinorTickMark.Enabled = true;
            chartArea1.AxisX.MinorTickMark.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.Title = "Time (ms)";
            chartArea1.AxisX.TitleForeColor = System.Drawing.Color.White;
            chartArea1.AxisY.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.AxisY.InterlacedColor = System.Drawing.Color.Black;
            chartArea1.AxisY.IsStartedFromZero = false;
            chartArea1.AxisY.LabelStyle.ForeColor = System.Drawing.Color.White;
            chartArea1.AxisY.LineColor = System.Drawing.Color.White;
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea1.AxisY.MajorTickMark.LineColor = System.Drawing.Color.White;
            chartArea1.AxisY.MinorTickMark.Enabled = true;
            chartArea1.AxisY.MinorTickMark.LineColor = System.Drawing.Color.White;
            chartArea1.AxisY.Title = "Photodiode voltage (V)";
            chartArea1.AxisY.TitleForeColor = System.Drawing.Color.White;
            chartArea1.BackColor = System.Drawing.Color.Black;
            chartArea1.BackImageTransparentColor = System.Drawing.Color.Black;
            chartArea1.BackSecondaryColor = System.Drawing.Color.White;
            chartArea1.BorderColor = System.Drawing.Color.White;
            chartArea1.CursorX.Interval = 100D;
            chartArea1.CursorX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Milliseconds;
            chartArea1.CursorX.IsUserEnabled = true;
            chartArea1.CursorX.IsUserSelectionEnabled = true;
            chartArea1.CursorY.Interval = 0.01D;
            chartArea1.CursorY.IsUserEnabled = true;
            chartArea1.CursorY.IsUserSelectionEnabled = true;
            chartArea1.Name = "ChartArea2";
            this.GraphAbs.ChartAreas.Add(chartArea1);
            legend1.BackColor = System.Drawing.Color.Black;
            legend1.Enabled = false;
            legend1.ForeColor = System.Drawing.Color.White;
            legend1.Name = "LegendChart2";
            this.GraphAbs.Legends.Add(legend1);
            this.GraphAbs.Location = new System.Drawing.Point(6, 358);
            this.GraphAbs.Name = "GraphAbs";
            this.GraphAbs.Size = new System.Drawing.Size(658, 292);
            this.GraphAbs.TabIndex = 30;
            this.GraphAbs.Text = "graphAbs";
            title1.BackColor = System.Drawing.Color.Black;
            title1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            title1.ForeColor = System.Drawing.Color.White;
            title1.Name = "graphAbs";
            title1.Text = "Absorption ToF";
            this.GraphAbs.Titles.Add(title1);
            // 
            // GraphPMT
            // 
            this.GraphPMT.BackColor = System.Drawing.Color.Black;
            this.GraphPMT.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartArea2.AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea2.AxisX.InterlacedColor = System.Drawing.Color.Black;
            chartArea2.AxisX.IsStartedFromZero = false;
            chartArea2.AxisX.LabelStyle.ForeColor = System.Drawing.Color.White;
            chartArea2.AxisX.LineColor = System.Drawing.Color.White;
            chartArea2.AxisX.MajorGrid.Enabled = false;
            chartArea2.AxisX.MajorGrid.LineColor = System.Drawing.Color.White;
            chartArea2.AxisX.MajorTickMark.LineColor = System.Drawing.Color.White;
            chartArea2.AxisX.MinorTickMark.Enabled = true;
            chartArea2.AxisX.MinorTickMark.LineColor = System.Drawing.Color.White;
            chartArea2.AxisX.Title = "Time (ms)";
            chartArea2.AxisX.TitleForeColor = System.Drawing.Color.White;
            chartArea2.AxisY.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea2.AxisY.InterlacedColor = System.Drawing.Color.Black;
            chartArea2.AxisY.IsStartedFromZero = false;
            chartArea2.AxisY.LabelStyle.ForeColor = System.Drawing.Color.White;
            chartArea2.AxisY.LineColor = System.Drawing.Color.White;
            chartArea2.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea2.AxisY.MajorTickMark.LineColor = System.Drawing.Color.White;
            chartArea2.AxisY.MinorTickMark.Enabled = true;
            chartArea2.AxisY.MinorTickMark.LineColor = System.Drawing.Color.White;
            chartArea2.AxisY.Title = "PMT voltage (V)";
            chartArea2.AxisY.TitleForeColor = System.Drawing.Color.White;
            chartArea2.BackColor = System.Drawing.Color.Black;
            chartArea2.BackImageTransparentColor = System.Drawing.Color.Black;
            chartArea2.BackSecondaryColor = System.Drawing.Color.White;
            chartArea2.BorderColor = System.Drawing.Color.White;
            chartArea2.CursorX.Interval = 100D;
            chartArea2.CursorX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Milliseconds;
            chartArea2.CursorX.IsUserEnabled = true;
            chartArea2.CursorX.IsUserSelectionEnabled = true;
            chartArea2.CursorY.Interval = 0.01D;
            chartArea2.CursorY.IsUserEnabled = true;
            chartArea2.CursorY.IsUserSelectionEnabled = true;
            chartArea2.Name = "ChartArea2";
            this.GraphPMT.ChartAreas.Add(chartArea2);
            legend2.BackColor = System.Drawing.Color.Black;
            legend2.Enabled = false;
            legend2.ForeColor = System.Drawing.Color.White;
            legend2.Name = "LegendChart2";
            this.GraphPMT.Legends.Add(legend2);
            this.GraphPMT.Location = new System.Drawing.Point(6, 19);
            this.GraphPMT.Name = "GraphPMT";
            this.GraphPMT.Size = new System.Drawing.Size(658, 292);
            this.GraphPMT.TabIndex = 29;
            this.GraphPMT.Text = "graphPMT";
            title2.BackColor = System.Drawing.Color.Black;
            title2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            title2.ForeColor = System.Drawing.Color.White;
            title2.Name = "chartTitle2";
            title2.Text = "PMT ToF";
            this.GraphPMT.Titles.Add(title2);
            // 
            // chkAutoScaleAbs
            // 
            this.chkAutoScaleAbs.AutoSize = true;
            this.chkAutoScaleAbs.Enabled = false;
            this.chkAutoScaleAbs.Location = new System.Drawing.Point(173, 657);
            this.chkAutoScaleAbs.Name = "chkAutoScaleAbs";
            this.chkAutoScaleAbs.Size = new System.Drawing.Size(78, 17);
            this.chkAutoScaleAbs.TabIndex = 14;
            this.chkAutoScaleAbs.Text = "Auto Scale";
            this.chkAutoScaleAbs.UseVisualStyleBackColor = true;
            this.chkAutoScaleAbs.CheckedChanged += new System.EventHandler(this.chkAutoScaleAbs_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(250, 659);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Sampling rate (Hz):";
            // 
            // cmbSamplingRateAbs
            // 
            this.cmbSamplingRateAbs.FormattingEnabled = true;
            this.cmbSamplingRateAbs.Items.AddRange(new object[] {
            "100",
            "500",
            "1000",
            "5000",
            "10000",
            "20000",
            "40000",
            "60000",
            "80000",
            "100000",
            "150000",
            "200000",
            "250000"});
            this.cmbSamplingRateAbs.Location = new System.Drawing.Point(352, 656);
            this.cmbSamplingRateAbs.Name = "cmbSamplingRateAbs";
            this.cmbSamplingRateAbs.Size = new System.Drawing.Size(72, 21);
            this.cmbSamplingRateAbs.TabIndex = 12;
            this.cmbSamplingRateAbs.Text = "250000";
            this.cmbSamplingRateAbs.SelectedIndexChanged += new System.EventHandler(this.samplingRateAbsSelect);
            // 
            // chkToFAbs
            // 
            this.chkToFAbs.AutoSize = true;
            this.chkToFAbs.Location = new System.Drawing.Point(6, 657);
            this.chkToFAbs.Name = "chkToFAbs";
            this.chkToFAbs.Size = new System.Drawing.Size(80, 17);
            this.chkToFAbs.TabIndex = 11;
            this.chkToFAbs.Text = "Show trace";
            this.chkToFAbs.UseVisualStyleBackColor = true;
            this.chkToFAbs.CheckedChanged += new System.EventHandler(this.chkToFAbs_CheckedChanged);
            // 
            // chkSaveTraceAbs
            // 
            this.chkSaveTraceAbs.AutoSize = true;
            this.chkSaveTraceAbs.Location = new System.Drawing.Point(92, 657);
            this.chkSaveTraceAbs.Name = "chkSaveTraceAbs";
            this.chkSaveTraceAbs.Size = new System.Drawing.Size(79, 17);
            this.chkSaveTraceAbs.TabIndex = 10;
            this.chkSaveTraceAbs.Text = "Save to file";
            this.chkSaveTraceAbs.UseVisualStyleBackColor = true;
            // 
            // chkAutoScale
            // 
            this.chkAutoScale.AutoSize = true;
            this.chkAutoScale.Enabled = false;
            this.chkAutoScale.Location = new System.Drawing.Point(173, 325);
            this.chkAutoScale.Name = "chkAutoScale";
            this.chkAutoScale.Size = new System.Drawing.Size(78, 17);
            this.chkAutoScale.TabIndex = 7;
            this.chkAutoScale.Text = "Auto Scale";
            this.chkAutoScale.UseVisualStyleBackColor = true;
            this.chkAutoScale.CheckedChanged += new System.EventHandler(this.chkAutoScale_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(250, 327);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Sampling rate (Hz):";
            // 
            // cmbSamplingRate
            // 
            this.cmbSamplingRate.FormattingEnabled = true;
            this.cmbSamplingRate.Items.AddRange(new object[] {
            "100",
            "500",
            "1000",
            "5000",
            "10000",
            "20000",
            "40000",
            "60000",
            "80000",
            "100000",
            "150000",
            "200000",
            "250000"});
            this.cmbSamplingRate.Location = new System.Drawing.Point(352, 324);
            this.cmbSamplingRate.Name = "cmbSamplingRate";
            this.cmbSamplingRate.Size = new System.Drawing.Size(72, 21);
            this.cmbSamplingRate.TabIndex = 4;
            this.cmbSamplingRate.Text = "80000";
            this.cmbSamplingRate.SelectedIndexChanged += new System.EventHandler(this.samplingRateSelect);
            // 
            // chkToF
            // 
            this.chkToF.AutoSize = true;
            this.chkToF.Location = new System.Drawing.Point(6, 325);
            this.chkToF.Name = "chkToF";
            this.chkToF.Size = new System.Drawing.Size(80, 17);
            this.chkToF.TabIndex = 3;
            this.chkToF.Text = "Show trace";
            this.chkToF.UseVisualStyleBackColor = true;
            this.chkToF.CheckedChanged += new System.EventHandler(this.chkToF_CheckedChanged);
            // 
            // chkSaveTrace
            // 
            this.chkSaveTrace.AutoSize = true;
            this.chkSaveTrace.Location = new System.Drawing.Point(92, 325);
            this.chkSaveTrace.Name = "chkSaveTrace";
            this.chkSaveTrace.Size = new System.Drawing.Size(79, 17);
            this.chkSaveTrace.TabIndex = 2;
            this.chkSaveTrace.Text = "Save to file";
            this.chkSaveTrace.UseVisualStyleBackColor = true;
            // 
            // readButton
            // 
            this.readButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.readButton.Location = new System.Drawing.Point(308, 722);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(99, 24);
            this.readButton.TabIndex = 8;
            this.readButton.Text = "Start Reading";
            this.readButton.UseVisualStyleBackColor = true;
            this.readButton.Click += new System.EventHandler(this.toggleReading);
            // 
            // SourceTabView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.readButton);
            this.Controls.Add(this.groupBox3);
            this.Name = "SourceTabView";
            this.Size = new System.Drawing.Size(702, 810);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GraphAbs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GraphPMT)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private XAxis xAxis2;
        private YAxis yAxis2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkAutoScale;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbSamplingRate;
        private System.Windows.Forms.CheckBox chkToF;
        private XAxis xAxis1;
        private YAxis yAxis1;
        private System.Windows.Forms.CheckBox chkSaveTrace;
        private System.Windows.Forms.Button readButton;
        private XAxis xAxis3;
        private YAxis yAxis3;
        private System.Windows.Forms.CheckBox chkAutoScaleAbs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbSamplingRateAbs;
        private System.Windows.Forms.CheckBox chkToFAbs;
        private System.Windows.Forms.CheckBox chkSaveTraceAbs;
        public System.Windows.Forms.DataVisualization.Charting.Chart GraphPMT;
        public System.Windows.Forms.DataVisualization.Charting.Chart GraphAbs;
    }
}
