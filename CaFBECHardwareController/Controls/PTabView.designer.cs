using NationalInstruments.UI;

namespace CaFBECHardwareController.Controls
{
    partial class PTabView
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
            this.PressureA = new System.Windows.Forms.Label();
            this.PressureB = new System.Windows.Forms.Label();
            this.PressureC = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.PressureF = new System.Windows.Forms.Label();
            this.PressureE = new System.Windows.Forms.Label();
            this.PressureD = new System.Windows.Forms.Label();
            this.StartReading = new System.Windows.Forms.Button();
            this.DataGraph = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.PollPeriod = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.TempDataSaveLoc = new System.Windows.Forms.TextBox();
            this.SaveTempData = new System.Windows.Forms.Button();
            this.DataClear = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGraph)).BeginInit();
            this.SuspendLayout();
            // 
            // PressureA
            // 
            this.PressureA.AutoSize = true;
            this.PressureA.Location = new System.Drawing.Point(114, 19);
            this.PressureA.Name = "PressureA";
            this.PressureA.Size = new System.Drawing.Size(55, 13);
            this.PressureA.TabIndex = 0;
            this.PressureA.Text = "PressureA";
            // 
            // PressureB
            // 
            this.PressureB.AutoSize = true;
            this.PressureB.Location = new System.Drawing.Point(114, 43);
            this.PressureB.Name = "PressureB";
            this.PressureB.Size = new System.Drawing.Size(55, 13);
            this.PressureB.TabIndex = 1;
            this.PressureB.Text = "PressureB";
            // 
            // PressureC
            // 
            this.PressureC.AutoSize = true;
            this.PressureC.Location = new System.Drawing.Point(114, 67);
            this.PressureC.Name = "PressureC";
            this.PressureC.Size = new System.Drawing.Size(55, 13);
            this.PressureC.TabIndex = 2;
            this.PressureC.Text = "PressureC";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Source backing";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Sci chamber backing";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Not used";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.PressureF);
            this.groupBox1.Controls.Add(this.PressureE);
            this.groupBox1.Controls.Add(this.PressureC);
            this.groupBox1.Controls.Add(this.PressureD);
            this.groupBox1.Controls.Add(this.PressureB);
            this.groupBox1.Controls.Add(this.PressureA);
            this.groupBox1.Location = new System.Drawing.Point(10, 25);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(401, 93);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Pressures (mbar)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(218, 67);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(66, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Sci chamber";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(218, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Beamline";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(218, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Source";
            // 
            // PressureF
            // 
            this.PressureF.AutoSize = true;
            this.PressureF.Location = new System.Drawing.Point(325, 67);
            this.PressureF.Name = "PressureF";
            this.PressureF.Size = new System.Drawing.Size(54, 13);
            this.PressureF.TabIndex = 7;
            this.PressureF.Text = "PressureF";
            // 
            // PressureE
            // 
            this.PressureE.AutoSize = true;
            this.PressureE.Location = new System.Drawing.Point(325, 43);
            this.PressureE.Name = "PressureE";
            this.PressureE.Size = new System.Drawing.Size(55, 13);
            this.PressureE.TabIndex = 6;
            this.PressureE.Text = "PressureE";
            // 
            // PressureD
            // 
            this.PressureD.AutoSize = true;
            this.PressureD.Location = new System.Drawing.Point(325, 19);
            this.PressureD.Name = "PressureD";
            this.PressureD.Size = new System.Drawing.Size(56, 13);
            this.PressureD.TabIndex = 5;
            this.PressureD.Text = "PressureD";
            // 
            // StartReading
            // 
            this.StartReading.Location = new System.Drawing.Point(166, 135);
            this.StartReading.Name = "StartReading";
            this.StartReading.Size = new System.Drawing.Size(83, 37);
            this.StartReading.TabIndex = 24;
            this.StartReading.Text = "Start";
            this.StartReading.UseVisualStyleBackColor = true;
            this.StartReading.Click += new System.EventHandler(this.StartReading_Click);
            // 
            // DataGraph
            // 
            this.DataGraph.BackColor = System.Drawing.Color.Black;
            this.DataGraph.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartArea1.AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.AxisX.InterlacedColor = System.Drawing.Color.Black;
            chartArea1.AxisX.IsStartedFromZero = false;
            chartArea1.AxisX.LabelStyle.ForeColor = System.Drawing.Color.White;
            chartArea1.AxisX.LabelStyle.Format = "HH:mm:ss";
            chartArea1.AxisX.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.MajorGrid.Enabled = false;
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.MajorTickMark.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.MinorTickMark.Enabled = true;
            chartArea1.AxisX.MinorTickMark.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.Title = "Time";
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
            chartArea1.AxisY.Title = "Pressure (mbar)";
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
            this.DataGraph.ChartAreas.Add(chartArea1);
            legend1.BackColor = System.Drawing.Color.Black;
            legend1.ForeColor = System.Drawing.Color.White;
            legend1.Name = "LegendChart2";
            this.DataGraph.Legends.Add(legend1);
            this.DataGraph.Location = new System.Drawing.Point(10, 223);
            this.DataGraph.Name = "DataGraph";
            this.DataGraph.Size = new System.Drawing.Size(658, 372);
            this.DataGraph.TabIndex = 28;
            this.DataGraph.Text = "chart2";
            title1.BackColor = System.Drawing.Color.Black;
            title1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            title1.ForeColor = System.Drawing.Color.White;
            title1.Name = "Chart2";
            title1.Text = "Pressure";
            this.DataGraph.Titles.Add(title1);
            // 
            // PollPeriod
            // 
            this.PollPeriod.Location = new System.Drawing.Point(105, 197);
            this.PollPeriod.Name = "PollPeriod";
            this.PollPeriod.Size = new System.Drawing.Size(66, 20);
            this.PollPeriod.TabIndex = 16;
            this.PollPeriod.Text = "1000";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(17, 200);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(78, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Poll period (ms)";
            // 
            // TempDataSaveLoc
            // 
            this.TempDataSaveLoc.Location = new System.Drawing.Point(10, 601);
            this.TempDataSaveLoc.Name = "TempDataSaveLoc";
            this.TempDataSaveLoc.Size = new System.Drawing.Size(542, 20);
            this.TempDataSaveLoc.TabIndex = 31;
            this.TempDataSaveLoc.Text = "C:\\Users\\cafmot\\OneDrive - Imperial College London\\cafbec\\logs\\defaultPLog.csv";
            // 
            // SaveTempData
            // 
            this.SaveTempData.Location = new System.Drawing.Point(562, 601);
            this.SaveTempData.Name = "SaveTempData";
            this.SaveTempData.Size = new System.Drawing.Size(106, 20);
            this.SaveTempData.TabIndex = 32;
            this.SaveTempData.Text = "Save Data";
            this.SaveTempData.UseVisualStyleBackColor = true;
            this.SaveTempData.Click += new System.EventHandler(this.SaveTempData_Click);
            // 
            // DataClear
            // 
            this.DataClear.Location = new System.Drawing.Point(194, 195);
            this.DataClear.Name = "DataClear";
            this.DataClear.Size = new System.Drawing.Size(75, 23);
            this.DataClear.TabIndex = 33;
            this.DataClear.Text = "Clear Data";
            this.DataClear.UseVisualStyleBackColor = true;
            this.DataClear.Click += new System.EventHandler(this.DataClear_Click);
            // 
            // PTabView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.DataClear);
            this.Controls.Add(this.SaveTempData);
            this.Controls.Add(this.TempDataSaveLoc);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.PollPeriod);
            this.Controls.Add(this.DataGraph);
            this.Controls.Add(this.StartReading);
            this.Controls.Add(this.groupBox1);
            this.Name = "PTabView";
            this.Load += new System.EventHandler(this.TPTabView_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGraph)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label PressureA;
        public System.Windows.Forms.Label PressureB;
        public System.Windows.Forms.Label PressureC;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.Label PressureD;
        public System.Windows.Forms.Label PressureE;
        public System.Windows.Forms.Label PressureF;
        public System.Windows.Forms.Button StartReading;
        public System.Windows.Forms.DataVisualization.Charting.Chart DataGraph;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label label4;
        public System.Windows.Forms.Label label5;
        public System.Windows.Forms.Label label6;
        public System.Windows.Forms.Label label9;
        public System.Windows.Forms.TextBox PollPeriod;
        public System.Windows.Forms.TextBox TempDataSaveLoc;
        public System.Windows.Forms.Button SaveTempData;
        private System.Windows.Forms.Button DataClear;
    }
}
