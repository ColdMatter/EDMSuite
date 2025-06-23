using NationalInstruments.UI;

namespace CaFBECHardwareController.Controls
{
    partial class TTabView
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea10 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend10 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Title title10 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.TempA = new System.Windows.Forms.Label();
            this.TempB = new System.Windows.Forms.Label();
            this.TempC = new System.Windows.Forms.Label();
            this.TempD = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.CycleButton = new System.Windows.Forms.Button();
            this.CryoON = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.TempE = new System.Windows.Forms.Label();
            this.TempF = new System.Windows.Forms.Label();
            this.TempG = new System.Windows.Forms.Label();
            this.TempH = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.HeaterON = new System.Windows.Forms.Button();
            this.StartReading = new System.Windows.Forms.Button();
            this.led1 = new NationalInstruments.UI.WindowsForms.Led();
            this.led2 = new NationalInstruments.UI.WindowsForms.Led();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.DataGraph = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.PollPeriod = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.SaveTempData = new System.Windows.Forms.Button();
            this.TempDataSaveLoc = new System.Windows.Forms.TextBox();
            this.DataClear = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.led1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.led2)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGraph)).BeginInit();
            this.SuspendLayout();
            // 
            // TempA
            // 
            this.TempA.AutoSize = true;
            this.TempA.Location = new System.Drawing.Point(114, 19);
            this.TempA.Name = "TempA";
            this.TempA.Size = new System.Drawing.Size(41, 13);
            this.TempA.TabIndex = 0;
            this.TempA.Text = "TempA";
            // 
            // TempB
            // 
            this.TempB.AutoSize = true;
            this.TempB.Location = new System.Drawing.Point(114, 43);
            this.TempB.Name = "TempB";
            this.TempB.Size = new System.Drawing.Size(41, 13);
            this.TempB.TabIndex = 1;
            this.TempB.Text = "TempB";
            // 
            // TempC
            // 
            this.TempC.AutoSize = true;
            this.TempC.Location = new System.Drawing.Point(114, 67);
            this.TempC.Name = "TempC";
            this.TempC.Size = new System.Drawing.Size(41, 13);
            this.TempC.TabIndex = 2;
            this.TempC.Text = "TempC";
            // 
            // TempD
            // 
            this.TempD.AutoSize = true;
            this.TempD.Location = new System.Drawing.Point(114, 91);
            this.TempD.Name = "TempD";
            this.TempD.Size = new System.Drawing.Size(42, 13);
            this.TempD.TabIndex = 3;
            this.TempD.Text = "TempD";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(24, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Cell";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "4K Stage";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "SF6 4K";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 91);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "40K Stage";
            // 
            // CycleButton
            // 
            this.CycleButton.Location = new System.Drawing.Point(41, 105);
            this.CycleButton.Name = "CycleButton";
            this.CycleButton.Size = new System.Drawing.Size(117, 37);
            this.CycleButton.TabIndex = 16;
            this.CycleButton.Text = "Cycle Source";
            this.CycleButton.UseVisualStyleBackColor = true;
            this.CycleButton.Click += new System.EventHandler(this.CycleButton_Click);
            // 
            // CryoON
            // 
            this.CryoON.Location = new System.Drawing.Point(83, 65);
            this.CryoON.Name = "CryoON";
            this.CryoON.Size = new System.Drawing.Size(75, 28);
            this.CryoON.TabIndex = 18;
            this.CryoON.Text = "Cryo";
            this.CryoON.UseVisualStyleBackColor = true;
            this.CryoON.Click += new System.EventHandler(this.CryoON_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.TempD);
            this.groupBox1.Controls.Add(this.TempC);
            this.groupBox1.Controls.Add(this.TempB);
            this.groupBox1.Controls.Add(this.TempA);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.TempE);
            this.groupBox1.Location = new System.Drawing.Point(10, 25);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(197, 148);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source Chamber Temps (K)";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 116);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "SF6 40K";
            // 
            // TempE
            // 
            this.TempE.AutoSize = true;
            this.TempE.Location = new System.Drawing.Point(114, 116);
            this.TempE.Name = "TempE";
            this.TempE.Size = new System.Drawing.Size(41, 13);
            this.TempE.TabIndex = 4;
            this.TempE.Text = "TempE";
            // 
            // TempF
            // 
            this.TempF.AutoSize = true;
            this.TempF.Location = new System.Drawing.Point(112, 19);
            this.TempF.Name = "TempF";
            this.TempF.Size = new System.Drawing.Size(40, 13);
            this.TempF.TabIndex = 5;
            this.TempF.Text = "TempF";
            // 
            // TempG
            // 
            this.TempG.AutoSize = true;
            this.TempG.Location = new System.Drawing.Point(112, 43);
            this.TempG.Name = "TempG";
            this.TempG.Size = new System.Drawing.Size(42, 13);
            this.TempG.TabIndex = 6;
            this.TempG.Text = "TempG";
            // 
            // TempH
            // 
            this.TempH.AutoSize = true;
            this.TempH.Location = new System.Drawing.Point(112, 67);
            this.TempH.Name = "TempH";
            this.TempH.Size = new System.Drawing.Size(42, 13);
            this.TempH.TabIndex = 7;
            this.TempH.Text = "TempH";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 19);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(46, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Top Coil";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 43);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(39, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Jacket";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(5, 67);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(84, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Cold Head MOT";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.TempH);
            this.groupBox2.Controls.Add(this.TempG);
            this.groupBox2.Controls.Add(this.TempF);
            this.groupBox2.Location = new System.Drawing.Point(251, 26);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(196, 98);
            this.groupBox2.TabIndex = 21;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Science Chamber Temps (K)";
            // 
            // HeaterON
            // 
            this.HeaterON.Location = new System.Drawing.Point(83, 20);
            this.HeaterON.Name = "HeaterON";
            this.HeaterON.Size = new System.Drawing.Size(75, 28);
            this.HeaterON.TabIndex = 22;
            this.HeaterON.Text = "Heater";
            this.HeaterON.UseVisualStyleBackColor = true;
            this.HeaterON.Click += new System.EventHandler(this.HeaterON_Click);
            // 
            // StartReading
            // 
            this.StartReading.Location = new System.Drawing.Point(306, 136);
            this.StartReading.Name = "StartReading";
            this.StartReading.Size = new System.Drawing.Size(83, 37);
            this.StartReading.TabIndex = 24;
            this.StartReading.Text = "Start";
            this.StartReading.UseVisualStyleBackColor = true;
            this.StartReading.Click += new System.EventHandler(this.StartReading_Click);
            // 
            // led1
            // 
            this.led1.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.led1.Location = new System.Drawing.Point(36, 19);
            this.led1.Name = "led1";
            this.led1.Size = new System.Drawing.Size(30, 30);
            this.led1.TabIndex = 25;
            // 
            // led2
            // 
            this.led2.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.led2.Location = new System.Drawing.Point(36, 64);
            this.led2.Name = "led2";
            this.led2.Size = new System.Drawing.Size(30, 30);
            this.led2.TabIndex = 26;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.led2);
            this.groupBox3.Controls.Add(this.led1);
            this.groupBox3.Controls.Add(this.HeaterON);
            this.groupBox3.Controls.Add(this.CryoON);
            this.groupBox3.Controls.Add(this.CycleButton);
            this.groupBox3.Location = new System.Drawing.Point(486, 25);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(182, 148);
            this.groupBox3.TabIndex = 27;
            this.groupBox3.TabStop = false;
            // 
            // DataGraph
            // 
            this.DataGraph.BackColor = System.Drawing.Color.Black;
            this.DataGraph.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartArea10.AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea10.AxisX.InterlacedColor = System.Drawing.Color.Black;
            chartArea10.AxisX.IsStartedFromZero = false;
            chartArea10.AxisX.LabelStyle.ForeColor = System.Drawing.Color.White;
            chartArea10.AxisX.LabelStyle.Format = "HH:mm:ss";
            chartArea10.AxisX.LineColor = System.Drawing.Color.White;
            chartArea10.AxisX.MajorGrid.Enabled = false;
            chartArea10.AxisX.MajorGrid.LineColor = System.Drawing.Color.White;
            chartArea10.AxisX.MajorTickMark.LineColor = System.Drawing.Color.White;
            chartArea10.AxisX.MinorTickMark.Enabled = true;
            chartArea10.AxisX.MinorTickMark.LineColor = System.Drawing.Color.White;
            chartArea10.AxisX.Title = "Time";
            chartArea10.AxisX.TitleForeColor = System.Drawing.Color.White;
            chartArea10.AxisY.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea10.AxisY.InterlacedColor = System.Drawing.Color.Black;
            chartArea10.AxisY.IsStartedFromZero = false;
            chartArea10.AxisY.LabelStyle.ForeColor = System.Drawing.Color.White;
            chartArea10.AxisY.LineColor = System.Drawing.Color.White;
            chartArea10.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea10.AxisY.MajorTickMark.LineColor = System.Drawing.Color.White;
            chartArea10.AxisY.MinorTickMark.Enabled = true;
            chartArea10.AxisY.MinorTickMark.LineColor = System.Drawing.Color.White;
            chartArea10.AxisY.Title = "Temperature (K)";
            chartArea10.AxisY.TitleForeColor = System.Drawing.Color.White;
            chartArea10.BackColor = System.Drawing.Color.Black;
            chartArea10.BackImageTransparentColor = System.Drawing.Color.Black;
            chartArea10.BackSecondaryColor = System.Drawing.Color.White;
            chartArea10.BorderColor = System.Drawing.Color.White;
            chartArea10.CursorX.Interval = 100D;
            chartArea10.CursorX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Milliseconds;
            chartArea10.CursorX.IsUserEnabled = true;
            chartArea10.CursorX.IsUserSelectionEnabled = true;
            chartArea10.CursorY.Interval = 0.01D;
            chartArea10.CursorY.IsUserEnabled = true;
            chartArea10.CursorY.IsUserSelectionEnabled = true;
            chartArea10.Name = "ChartArea2";
            this.DataGraph.ChartAreas.Add(chartArea10);
            legend10.BackColor = System.Drawing.Color.Black;
            legend10.ForeColor = System.Drawing.Color.White;
            legend10.Name = "LegendChart2";
            this.DataGraph.Legends.Add(legend10);
            this.DataGraph.Location = new System.Drawing.Point(10, 223);
            this.DataGraph.Name = "DataGraph";
            this.DataGraph.Size = new System.Drawing.Size(658, 372);
            this.DataGraph.TabIndex = 28;
            this.DataGraph.Text = "chart2";
            title10.BackColor = System.Drawing.Color.Black;
            title10.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            title10.ForeColor = System.Drawing.Color.White;
            title10.Name = "chartTitle2";
            title10.Text = "Temperature";
            this.DataGraph.Titles.Add(title10);
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
            // SaveTempData
            // 
            this.SaveTempData.Location = new System.Drawing.Point(562, 600);
            this.SaveTempData.Name = "SaveTempData";
            this.SaveTempData.Size = new System.Drawing.Size(106, 20);
            this.SaveTempData.TabIndex = 29;
            this.SaveTempData.Text = "Save Data";
            this.SaveTempData.UseVisualStyleBackColor = true;
            this.SaveTempData.Click += new System.EventHandler(this.SaveTempData_Click);
            // 
            // TempDataSaveLoc
            // 
            this.TempDataSaveLoc.Location = new System.Drawing.Point(10, 601);
            this.TempDataSaveLoc.Name = "TempDataSaveLoc";
            this.TempDataSaveLoc.Size = new System.Drawing.Size(542, 20);
            this.TempDataSaveLoc.TabIndex = 30;
            this.TempDataSaveLoc.Text = "C:\\Users\\cafmot\\OneDrive - Imperial College London\\cafbec\\logs\\defaultTLog.csv";
            // 
            // DataClear
            // 
            this.DataClear.Location = new System.Drawing.Point(194, 195);
            this.DataClear.Name = "DataClear";
            this.DataClear.Size = new System.Drawing.Size(75, 23);
            this.DataClear.TabIndex = 31;
            this.DataClear.Text = "Clear Data";
            this.DataClear.UseVisualStyleBackColor = true;
            this.DataClear.Click += new System.EventHandler(this.DataClear_Click);
            // 
            // TPTabView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.DataClear);
            this.Controls.Add(this.TempDataSaveLoc);
            this.Controls.Add(this.SaveTempData);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.PollPeriod);
            this.Controls.Add(this.DataGraph);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.StartReading);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "TPTabView";
            this.Load += new System.EventHandler(this.TPTabView_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.led1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.led2)).EndInit();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataGraph)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label TempA;
        public System.Windows.Forms.Label TempB;
        public System.Windows.Forms.Label TempC;
        public System.Windows.Forms.Label TempD;
        public System.Windows.Forms.Button CycleButton;
        public System.Windows.Forms.Button CryoON;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.Label TempE;
        public System.Windows.Forms.Label TempF;
        public System.Windows.Forms.Label TempG;
        public System.Windows.Forms.Label TempH;
        private System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.Button HeaterON;
        public System.Windows.Forms.Button StartReading;
        public NationalInstruments.UI.WindowsForms.Led led1;
        public NationalInstruments.UI.WindowsForms.Led led2;
        private System.Windows.Forms.GroupBox groupBox3;
        public System.Windows.Forms.DataVisualization.Charting.Chart DataGraph;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label label4;
        public System.Windows.Forms.Label label5;
        public System.Windows.Forms.Label label6;
        public System.Windows.Forms.Label label7;
        public System.Windows.Forms.Label label8;
        public System.Windows.Forms.Label label9;
        public System.Windows.Forms.TextBox PollPeriod;
        public System.Windows.Forms.Button SaveTempData;
        public System.Windows.Forms.TextBox TempDataSaveLoc;
        private System.Windows.Forms.Button DataClear;
    }
}
