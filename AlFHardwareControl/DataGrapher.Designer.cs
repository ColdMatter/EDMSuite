
namespace AlFHardwareControl
{
    partial class DataGrapher
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
            this.DataGraph = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.SaveTempData = new System.Windows.Forms.Button();
            this.TempDataSaveLoc = new System.Windows.Forms.TextBox();
            this.Settings = new System.Windows.Forms.GroupBox();
            this.logYAxis = new System.Windows.Forms.CheckBox();
            this.DataClear = new System.Windows.Forms.Button();
            this.DataControl = new System.Windows.Forms.GroupBox();
            this.MaximumDatapointNumber = new System.Windows.Forms.TextBox();
            this.MaximumDataEnable = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.DataGraph)).BeginInit();
            this.Settings.SuspendLayout();
            this.DataControl.SuspendLayout();
            this.SuspendLayout();
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
            chartArea1.AxisY.Title = "Temperature (K)";
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
            this.DataGraph.Location = new System.Drawing.Point(3, 3);
            this.DataGraph.Name = "DataGraph";
            this.DataGraph.Size = new System.Drawing.Size(836, 372);
            this.DataGraph.TabIndex = 26;
            this.DataGraph.Text = "chart2";
            title1.BackColor = System.Drawing.Color.Black;
            title1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            title1.ForeColor = System.Drawing.Color.White;
            title1.Name = "chartTitle2";
            title1.Text = "Temperature";
            this.DataGraph.Titles.Add(title1);
            // 
            // SaveTempData
            // 
            this.SaveTempData.Location = new System.Drawing.Point(733, 381);
            this.SaveTempData.Name = "SaveTempData";
            this.SaveTempData.Size = new System.Drawing.Size(106, 20);
            this.SaveTempData.TabIndex = 24;
            this.SaveTempData.Text = "Save Data";
            this.SaveTempData.UseVisualStyleBackColor = true;
            this.SaveTempData.Click += new System.EventHandler(this.SaveTempData_Click);
            // 
            // TempDataSaveLoc
            // 
            this.TempDataSaveLoc.Location = new System.Drawing.Point(3, 382);
            this.TempDataSaveLoc.Name = "TempDataSaveLoc";
            this.TempDataSaveLoc.Size = new System.Drawing.Size(724, 20);
            this.TempDataSaveLoc.TabIndex = 25;
            this.TempDataSaveLoc.Text = "C:\\Users\\alfultra\\OneDrive - Imperial College London\\Cooling_logs\\Default.csv";
            // 
            // Settings
            // 
            this.Settings.Controls.Add(this.logYAxis);
            this.Settings.Location = new System.Drawing.Point(845, 3);
            this.Settings.Name = "Settings";
            this.Settings.Size = new System.Drawing.Size(332, 137);
            this.Settings.TabIndex = 27;
            this.Settings.TabStop = false;
            this.Settings.Text = "Settings";
            this.Settings.Enter += new System.EventHandler(this.Settings_Enter);
            // 
            // logYAxis
            // 
            this.logYAxis.AutoSize = true;
            this.logYAxis.Location = new System.Drawing.Point(6, 19);
            this.logYAxis.Name = "logYAxis";
            this.logYAxis.Size = new System.Drawing.Size(110, 17);
            this.logYAxis.TabIndex = 0;
            this.logYAxis.Text = "Logarithmic y Axis";
            this.logYAxis.UseVisualStyleBackColor = true;
            this.logYAxis.CheckedChanged += new System.EventHandler(this.logYAxis_CheckedChanged);
            // 
            // DataClear
            // 
            this.DataClear.Location = new System.Drawing.Point(7, 46);
            this.DataClear.Name = "DataClear";
            this.DataClear.Size = new System.Drawing.Size(75, 23);
            this.DataClear.TabIndex = 1;
            this.DataClear.Text = "Clear Data";
            this.DataClear.UseVisualStyleBackColor = true;
            this.DataClear.Click += new System.EventHandler(this.DataClear_Click);
            // 
            // DataControl
            // 
            this.DataControl.Controls.Add(this.MaximumDatapointNumber);
            this.DataControl.Controls.Add(this.DataClear);
            this.DataControl.Controls.Add(this.MaximumDataEnable);
            this.DataControl.Location = new System.Drawing.Point(845, 146);
            this.DataControl.Name = "DataControl";
            this.DataControl.Size = new System.Drawing.Size(332, 137);
            this.DataControl.TabIndex = 28;
            this.DataControl.TabStop = false;
            this.DataControl.Text = "Data Control";
            // 
            // MaximumDatapointNumber
            // 
            this.MaximumDatapointNumber.Enabled = false;
            this.MaximumDatapointNumber.Location = new System.Drawing.Point(7, 20);
            this.MaximumDatapointNumber.Name = "MaximumDatapointNumber";
            this.MaximumDatapointNumber.Size = new System.Drawing.Size(100, 20);
            this.MaximumDatapointNumber.TabIndex = 1;
            this.MaximumDatapointNumber.Text = "50000";
            // 
            // MaximumDataEnable
            // 
            this.MaximumDataEnable.AutoSize = true;
            this.MaximumDataEnable.Checked = true;
            this.MaximumDataEnable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MaximumDataEnable.Location = new System.Drawing.Point(113, 22);
            this.MaximumDataEnable.Name = "MaximumDataEnable";
            this.MaximumDataEnable.Size = new System.Drawing.Size(132, 17);
            this.MaximumDataEnable.TabIndex = 0;
            this.MaximumDataEnable.Text = "Limit datapoint number";
            this.MaximumDataEnable.UseVisualStyleBackColor = true;
            this.MaximumDataEnable.CheckedChanged += new System.EventHandler(this.MaximumDataEnable_CheckedChanged);
            // 
            // DataGrapher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DataControl);
            this.Controls.Add(this.Settings);
            this.Controls.Add(this.DataGraph);
            this.Controls.Add(this.SaveTempData);
            this.Controls.Add(this.TempDataSaveLoc);
            this.Name = "DataGrapher";
            this.Size = new System.Drawing.Size(1180, 411);
            ((System.ComponentModel.ISupportInitialize)(this.DataGraph)).EndInit();
            this.Settings.ResumeLayout(false);
            this.Settings.PerformLayout();
            this.DataControl.ResumeLayout(false);
            this.DataControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.DataVisualization.Charting.Chart DataGraph;
        public System.Windows.Forms.Button SaveTempData;
        public System.Windows.Forms.TextBox TempDataSaveLoc;
        private System.Windows.Forms.GroupBox Settings;
        private System.Windows.Forms.CheckBox logYAxis;
        private System.Windows.Forms.Button DataClear;
        private System.Windows.Forms.GroupBox DataControl;
        private System.Windows.Forms.TextBox MaximumDatapointNumber;
        private System.Windows.Forms.CheckBox MaximumDataEnable;
    }
}
