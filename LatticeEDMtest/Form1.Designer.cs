
namespace LatticeHardwareControl
{
    partial class Form1
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
        public void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title2 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartHeliumFlow = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btStopFlowActMonitor = new System.Windows.Forms.Button();
            this.btStartFlowActMonitor = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.tbFlowActPollPeriod = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btSetNewHeliumFlowSetpoint = new System.Windows.Forms.Button();
            this.tbHeliumFlowActual = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbNewHeliumFlowSetPoint = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbHeliumFlowSetpoint = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tbSF6FlowActual = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btSetNewSF6FlowSetpoint = new System.Windows.Forms.Button();
            this.tbNewSF6FlowSetPoint = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbSF6FlowSetpoint = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartHeliumFlow)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(189, 597);
            this.panel1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Font = new System.Drawing.Font("Arial Rounded MT Bold", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(197, 9);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(770, 575);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tabPage1.Controls.Add(this.chart1);
            this.tabPage1.Controls.Add(this.chartHeliumFlow);
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Location = new System.Drawing.Point(4, 21);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(762, 550);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Flow Controllers";
            // 
            // chart1
            // 
            this.chart1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea1.AxisX.IsLabelAutoFit = false;
            chartArea1.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial Rounded MT Bold", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisX.LabelStyle.ForeColor = System.Drawing.Color.White;
            chartArea1.AxisX.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.MajorTickMark.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.MinorGrid.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.MinorTickMark.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.Title = "Time";
            chartArea1.AxisX.TitleFont = new System.Drawing.Font("Arial Rounded MT Bold", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisX.TitleForeColor = System.Drawing.Color.White;
            chartArea1.AxisY.IsLabelAutoFit = false;
            chartArea1.AxisY.LabelStyle.Font = new System.Drawing.Font("Arial Rounded MT Bold", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisY.LabelStyle.ForeColor = System.Drawing.Color.White;
            chartArea1.AxisY.LineColor = System.Drawing.Color.White;
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.White;
            chartArea1.AxisY.MajorTickMark.LineColor = System.Drawing.Color.White;
            chartArea1.AxisY.MinorGrid.LineColor = System.Drawing.Color.White;
            chartArea1.AxisY.MinorTickMark.LineColor = System.Drawing.Color.White;
            chartArea1.AxisY.Title = "Flow Rate (sccm)";
            chartArea1.AxisY.TitleFont = new System.Drawing.Font("Arial Rounded MT Bold", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea1.AxisY.TitleForeColor = System.Drawing.Color.White;
            chartArea1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea1.Name = "HeliumFlowChartArea";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Enabled = false;
            legend1.Name = "HeliumFlowLegend";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(6, 315);
            this.chart1.Name = "chart1";
            series1.ChartArea = "HeliumFlowChartArea";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Legend = "HeliumFlowLegend";
            series1.Name = "SF6 Flow";
            series1.YValuesPerPoint = 2;
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(430, 199);
            this.chart1.TabIndex = 4;
            this.chart1.Text = "chart1";
            title1.Font = new System.Drawing.Font("Arial Rounded MT Bold", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            title1.ForeColor = System.Drawing.Color.White;
            title1.Name = "SF6 Flow";
            title1.Text = "SF6 Flow";
            this.chart1.Titles.Add(title1);
            // 
            // chartHeliumFlow
            // 
            this.chartHeliumFlow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea2.AxisX.IsStartedFromZero = false;
            chartArea2.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial Rounded MT Bold", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea2.AxisX.LabelStyle.ForeColor = System.Drawing.Color.White;
            chartArea2.AxisX.LabelStyle.Format = "00:00";
            chartArea2.AxisX.LineColor = System.Drawing.Color.White;
            chartArea2.AxisX.MajorGrid.LineColor = System.Drawing.Color.White;
            chartArea2.AxisX.MajorTickMark.LineColor = System.Drawing.Color.White;
            chartArea2.AxisX.MinorGrid.LineColor = System.Drawing.Color.White;
            chartArea2.AxisX.MinorTickMark.LineColor = System.Drawing.Color.White;
            chartArea2.AxisX.Title = "Time";
            chartArea2.AxisX.TitleFont = new System.Drawing.Font("Arial Rounded MT Bold", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea2.AxisX.TitleForeColor = System.Drawing.Color.White;
            chartArea2.AxisY.IsLabelAutoFit = false;
            chartArea2.AxisY.LabelStyle.Font = new System.Drawing.Font("Arial Rounded MT Bold", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea2.AxisY.LabelStyle.ForeColor = System.Drawing.Color.White;
            chartArea2.AxisY.LineColor = System.Drawing.Color.White;
            chartArea2.AxisY.MajorGrid.LineColor = System.Drawing.Color.White;
            chartArea2.AxisY.MajorTickMark.LineColor = System.Drawing.Color.White;
            chartArea2.AxisY.MinorGrid.LineColor = System.Drawing.Color.White;
            chartArea2.AxisY.MinorTickMark.LineColor = System.Drawing.Color.White;
            chartArea2.AxisY.Title = "Flow Rate (sccm)";
            chartArea2.AxisY.TitleFont = new System.Drawing.Font("Arial Rounded MT Bold", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            chartArea2.AxisY.TitleForeColor = System.Drawing.Color.White;
            chartArea2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea2.Name = "HeliumFlowChartArea";
            this.chartHeliumFlow.ChartAreas.Add(chartArea2);
            legend2.Enabled = false;
            legend2.Name = "HeliumFlowLegend";
            this.chartHeliumFlow.Legends.Add(legend2);
            this.chartHeliumFlow.Location = new System.Drawing.Point(6, 88);
            this.chartHeliumFlow.Name = "chartHeliumFlow";
            series2.ChartArea = "HeliumFlowChartArea";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Legend = "HeliumFlowLegend";
            series2.Name = "Helium Flow";
            this.chartHeliumFlow.Series.Add(series2);
            this.chartHeliumFlow.Size = new System.Drawing.Size(430, 199);
            this.chartHeliumFlow.TabIndex = 3;
            this.chartHeliumFlow.Text = "chartHeliumFlow";
            title2.Font = new System.Drawing.Font("Arial Rounded MT Bold", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            title2.ForeColor = System.Drawing.Color.White;
            title2.Name = "HeliumFlowTitle";
            title2.Text = "Helium Flow";
            this.chartHeliumFlow.Titles.Add(title2);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btStopFlowActMonitor);
            this.groupBox3.Controls.Add(this.btStartFlowActMonitor);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.tbFlowActPollPeriod);
            this.groupBox3.Controls.Add(this.groupBox1);
            this.groupBox3.Controls.Add(this.groupBox2);
            this.groupBox3.Location = new System.Drawing.Point(451, 11);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(297, 522);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Monitor Flow Controls";
            // 
            // btStopFlowActMonitor
            // 
            this.btStopFlowActMonitor.Location = new System.Drawing.Point(184, 56);
            this.btStopFlowActMonitor.Name = "btStopFlowActMonitor";
            this.btStopFlowActMonitor.Size = new System.Drawing.Size(100, 27);
            this.btStopFlowActMonitor.TabIndex = 5;
            this.btStopFlowActMonitor.Text = "Stop";
            this.btStopFlowActMonitor.UseVisualStyleBackColor = true;
            this.btStopFlowActMonitor.Click += new System.EventHandler(this.btStopFlowActMonitor_Click_1);
            // 
            // btStartFlowActMonitor
            // 
            this.btStartFlowActMonitor.Location = new System.Drawing.Point(77, 56);
            this.btStartFlowActMonitor.Name = "btStartFlowActMonitor";
            this.btStartFlowActMonitor.Size = new System.Drawing.Size(101, 27);
            this.btStartFlowActMonitor.TabIndex = 4;
            this.btStartFlowActMonitor.Text = "Start";
            this.btStartFlowActMonitor.UseVisualStyleBackColor = true;
            this.btStartFlowActMonitor.Click += new System.EventHandler(this.btStartFlowActMonitor_Click_1);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(75, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 12);
            this.label5.TabIndex = 3;
            this.label5.Text = "Poll Period";
            // 
            // tbFlowActPollPeriod
            // 
            this.tbFlowActPollPeriod.Location = new System.Drawing.Point(145, 30);
            this.tbFlowActPollPeriod.Name = "tbFlowActPollPeriod";
            this.tbFlowActPollPeriod.Size = new System.Drawing.Size(139, 20);
            this.tbFlowActPollPeriod.TabIndex = 2;
            this.tbFlowActPollPeriod.Text = "1000";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btSetNewHeliumFlowSetpoint);
            this.groupBox1.Controls.Add(this.tbHeliumFlowActual);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.tbNewHeliumFlowSetPoint);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.tbHeliumFlowSetpoint);
            this.groupBox1.Location = new System.Drawing.Point(6, 103);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(285, 173);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Helium Flow Control";
            // 
            // btSetNewHeliumFlowSetpoint
            // 
            this.btSetNewHeliumFlowSetpoint.Location = new System.Drawing.Point(201, 110);
            this.btSetNewHeliumFlowSetpoint.Name = "btSetNewHeliumFlowSetpoint";
            this.btSetNewHeliumFlowSetpoint.Size = new System.Drawing.Size(77, 20);
            this.btSetNewHeliumFlowSetpoint.TabIndex = 8;
            this.btSetNewHeliumFlowSetpoint.Text = "Set Setpoint";
            this.btSetNewHeliumFlowSetpoint.UseVisualStyleBackColor = true;
            this.btSetNewHeliumFlowSetpoint.Click += new System.EventHandler(this.btSetNewHeliumFlowSetpoint_Click);
            // 
            // tbHeliumFlowActual
            // 
            this.tbHeliumFlowActual.Location = new System.Drawing.Point(72, 67);
            this.tbHeliumFlowActual.Name = "tbHeliumFlowActual";
            this.tbHeliumFlowActual.ReadOnly = true;
            this.tbHeliumFlowActual.Size = new System.Drawing.Size(116, 20);
            this.tbHeliumFlowActual.TabIndex = 6;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 70);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 12);
            this.label6.TabIndex = 5;
            this.label6.Text = "Flow Rate";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 113);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "Input Setpoint";
            // 
            // tbNewHeliumFlowSetPoint
            // 
            this.tbNewHeliumFlowSetPoint.Location = new System.Drawing.Point(94, 110);
            this.tbNewHeliumFlowSetPoint.Name = "tbNewHeliumFlowSetPoint";
            this.tbNewHeliumFlowSetPoint.Size = new System.Drawing.Size(85, 20);
            this.tbNewHeliumFlowSetPoint.TabIndex = 2;
            this.tbNewHeliumFlowSetPoint.Text = "0.0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "Setpoint";
            // 
            // tbHeliumFlowSetpoint
            // 
            this.tbHeliumFlowSetpoint.Location = new System.Drawing.Point(72, 33);
            this.tbHeliumFlowSetpoint.Name = "tbHeliumFlowSetpoint";
            this.tbHeliumFlowSetpoint.ReadOnly = true;
            this.tbHeliumFlowSetpoint.Size = new System.Drawing.Size(116, 20);
            this.tbHeliumFlowSetpoint.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tbSF6FlowActual);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.btSetNewSF6FlowSetpoint);
            this.groupBox2.Controls.Add(this.tbNewSF6FlowSetPoint);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.tbSF6FlowSetpoint);
            this.groupBox2.Location = new System.Drawing.Point(6, 323);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(285, 180);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "SF6 Flow Control";
            // 
            // tbSF6FlowActual
            // 
            this.tbSF6FlowActual.Location = new System.Drawing.Point(72, 67);
            this.tbSF6FlowActual.Name = "tbSF6FlowActual";
            this.tbSF6FlowActual.ReadOnly = true;
            this.tbSF6FlowActual.Size = new System.Drawing.Size(116, 20);
            this.tbSF6FlowActual.TabIndex = 7;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 70);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 12);
            this.label7.TabIndex = 7;
            this.label7.Text = "Flow Rate";
            // 
            // btSetNewSF6FlowSetpoint
            // 
            this.btSetNewSF6FlowSetpoint.Location = new System.Drawing.Point(201, 105);
            this.btSetNewSF6FlowSetpoint.Name = "btSetNewSF6FlowSetpoint";
            this.btSetNewSF6FlowSetpoint.Size = new System.Drawing.Size(77, 20);
            this.btSetNewSF6FlowSetpoint.TabIndex = 5;
            this.btSetNewSF6FlowSetpoint.Text = "Set Setpoint";
            this.btSetNewSF6FlowSetpoint.UseVisualStyleBackColor = true;
            // 
            // tbNewSF6FlowSetPoint
            // 
            this.tbNewSF6FlowSetPoint.Location = new System.Drawing.Point(94, 106);
            this.tbNewSF6FlowSetPoint.Name = "tbNewSF6FlowSetPoint";
            this.tbNewSF6FlowSetPoint.Size = new System.Drawing.Size(85, 20);
            this.tbNewSF6FlowSetPoint.TabIndex = 4;
            this.tbNewSF6FlowSetPoint.Text = "0.0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 109);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "Input Setpoint";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "Setpoint";
            // 
            // tbSF6FlowSetpoint
            // 
            this.tbSF6FlowSetpoint.Location = new System.Drawing.Point(72, 33);
            this.tbSF6FlowSetpoint.Name = "tbSF6FlowSetpoint";
            this.tbSF6FlowSetpoint.ReadOnly = true;
            this.tbSF6FlowSetpoint.Size = new System.Drawing.Size(116, 20);
            this.tbSF6FlowSetpoint.TabIndex = 1;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 21);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(762, 550);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "YAG Control";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 21);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(762, 550);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Refresh";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(58)))), ((int)(((byte)(54)))));
            this.ClientSize = new System.Drawing.Size(975, 597);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Lattice EDM Controller";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartHeliumFlow)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox tbNewSF6FlowSetPoint;
        public System.Windows.Forms.TextBox tbSF6FlowSetpoint;
        public System.Windows.Forms.TextBox tbNewHeliumFlowSetPoint;
        public System.Windows.Forms.TextBox tbHeliumFlowSetpoint;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.Button btStartFlowActMonitor;
        public System.Windows.Forms.Button btStopFlowActMonitor;
        public System.Windows.Forms.TextBox tbHeliumFlowActual;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.TextBox tbSF6FlowActual;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.Button btSetNewSF6FlowSetpoint;
        public System.Windows.Forms.TextBox tbFlowActPollPeriod;
        public System.Windows.Forms.Button btSetNewHeliumFlowSetpoint;
        public System.Windows.Forms.DataVisualization.Charting.Chart chartHeliumFlow;
        public System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.TabPage tabPage3;
    }
}

