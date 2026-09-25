
namespace AlFHardwareControl
{
    partial class MOTMasterStuff
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
            this.DataTabs = new System.Windows.Forms.TabControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cameraEnable = new System.Windows.Forms.CheckBox();
            this.saveEnable = new System.Windows.Forms.CheckBox();
            this.fixY = new System.Windows.Forms.CheckBox();
            this.fixX = new System.Windows.Forms.CheckBox();
            this.armToF = new System.Windows.Forms.CheckBox();
            this.sampNum = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbSamplingRate = new System.Windows.Forms.ComboBox();
            this.scanCtrl = new System.Windows.Forms.GroupBox();
            this.startPattern = new System.Windows.Forms.Button();
            this.reloadPatterns = new System.Windows.Forms.Button();
            this.PatternPicker = new System.Windows.Forms.ComboBox();
            this.scanPointProgress = new System.Windows.Forms.Label();
            this.repeatScan = new System.Windows.Forms.CheckBox();
            this.RejectionStatus = new System.Windows.Forms.TextBox();
            this.save_data = new System.Windows.Forms.Button();
            this.clear_data = new System.Windows.Forms.Button();
            this.ScanParameterButton = new System.Windows.Forms.Button();
            this.PluginSelector = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.stopScan = new System.Windows.Forms.Button();
            this.startScan = new System.Windows.Forms.Button();
            this.scanGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scanLowCursor = new NationalInstruments.UI.XYCursor();
            this.scatterPlot3 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.xyCursor2 = new NationalInstruments.UI.XYCursor();
            this.scatterPlot4 = new NationalInstruments.UI.ScatterPlot();
            this.integralView = new System.Windows.Forms.DataGridView();
            this.groupBox1.SuspendLayout();
            this.scanCtrl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scanGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scanLowCursor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.integralView)).BeginInit();
            this.SuspendLayout();
            // 
            // DataTabs
            // 
            this.DataTabs.Location = new System.Drawing.Point(3, 4);
            this.DataTabs.Name = "DataTabs";
            this.DataTabs.SelectedIndex = 0;
            this.DataTabs.Size = new System.Drawing.Size(604, 404);
            this.DataTabs.TabIndex = 1;
            this.DataTabs.SelectedIndexChanged += new System.EventHandler(this.DataTabs_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cameraEnable);
            this.groupBox1.Controls.Add(this.saveEnable);
            this.groupBox1.Controls.Add(this.fixY);
            this.groupBox1.Controls.Add(this.fixX);
            this.groupBox1.Controls.Add(this.armToF);
            this.groupBox1.Controls.Add(this.sampNum);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.cmbSamplingRate);
            this.groupBox1.Location = new System.Drawing.Point(614, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(247, 134);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // cameraEnable
            // 
            this.cameraEnable.AutoSize = true;
            this.cameraEnable.Location = new System.Drawing.Point(138, 88);
            this.cameraEnable.Name = "cameraEnable";
            this.cameraEnable.Size = new System.Drawing.Size(98, 17);
            this.cameraEnable.TabIndex = 9;
            this.cameraEnable.Text = "Enable Camera";
            this.cameraEnable.UseVisualStyleBackColor = true;
            // 
            // saveEnable
            // 
            this.saveEnable.AutoSize = true;
            this.saveEnable.Location = new System.Drawing.Point(138, 65);
            this.saveEnable.Name = "saveEnable";
            this.saveEnable.Size = new System.Drawing.Size(87, 17);
            this.saveEnable.TabIndex = 5;
            this.saveEnable.Text = "Enable Save";
            this.saveEnable.UseVisualStyleBackColor = true;
            // 
            // fixY
            // 
            this.fixY.AutoSize = true;
            this.fixY.Location = new System.Drawing.Point(9, 88);
            this.fixY.Name = "fixY";
            this.fixY.Size = new System.Drawing.Size(79, 17);
            this.fixY.TabIndex = 3;
            this.fixY.Text = "Fix Y range";
            this.fixY.UseVisualStyleBackColor = true;
            this.fixY.CheckedChanged += new System.EventHandler(this.fixY_CheckedChanged);
            // 
            // fixX
            // 
            this.fixX.AutoSize = true;
            this.fixX.Location = new System.Drawing.Point(9, 65);
            this.fixX.Name = "fixX";
            this.fixX.Size = new System.Drawing.Size(79, 17);
            this.fixX.TabIndex = 2;
            this.fixX.Text = "Fix X range";
            this.fixX.UseVisualStyleBackColor = true;
            this.fixX.CheckedChanged += new System.EventHandler(this.fixX_CheckedChanged);
            // 
            // armToF
            // 
            this.armToF.AutoSize = true;
            this.armToF.Location = new System.Drawing.Point(9, 111);
            this.armToF.Name = "armToF";
            this.armToF.Size = new System.Drawing.Size(66, 17);
            this.armToF.TabIndex = 4;
            this.armToF.Text = "Arm ToF";
            this.armToF.UseVisualStyleBackColor = true;
            this.armToF.CheckedChanged += new System.EventHandler(this.armToF_CheckedChanged);
            // 
            // sampNum
            // 
            this.sampNum.Location = new System.Drawing.Point(138, 41);
            this.sampNum.Name = "sampNum";
            this.sampNum.Size = new System.Drawing.Size(103, 20);
            this.sampNum.TabIndex = 1;
            this.sampNum.Text = "1000";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Sample length:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 13);
            this.label5.TabIndex = 7;
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
            this.cmbSamplingRate.Location = new System.Drawing.Point(138, 13);
            this.cmbSamplingRate.Name = "cmbSamplingRate";
            this.cmbSamplingRate.Size = new System.Drawing.Size(103, 21);
            this.cmbSamplingRate.TabIndex = 0;
            this.cmbSamplingRate.Text = "100000";
            // 
            // scanCtrl
            // 
            this.scanCtrl.Controls.Add(this.startPattern);
            this.scanCtrl.Controls.Add(this.reloadPatterns);
            this.scanCtrl.Controls.Add(this.PatternPicker);
            this.scanCtrl.Controls.Add(this.scanPointProgress);
            this.scanCtrl.Controls.Add(this.repeatScan);
            this.scanCtrl.Controls.Add(this.RejectionStatus);
            this.scanCtrl.Controls.Add(this.save_data);
            this.scanCtrl.Controls.Add(this.clear_data);
            this.scanCtrl.Controls.Add(this.ScanParameterButton);
            this.scanCtrl.Controls.Add(this.PluginSelector);
            this.scanCtrl.Controls.Add(this.label2);
            this.scanCtrl.Controls.Add(this.stopScan);
            this.scanCtrl.Controls.Add(this.startScan);
            this.scanCtrl.Enabled = false;
            this.scanCtrl.Location = new System.Drawing.Point(868, 6);
            this.scanCtrl.Name = "scanCtrl";
            this.scanCtrl.Size = new System.Drawing.Size(309, 132);
            this.scanCtrl.TabIndex = 3;
            this.scanCtrl.TabStop = false;
            this.scanCtrl.Text = "Scan Control";
            // 
            // startPattern
            // 
            this.startPattern.Location = new System.Drawing.Point(248, 74);
            this.startPattern.Name = "startPattern";
            this.startPattern.Size = new System.Drawing.Size(23, 23);
            this.startPattern.TabIndex = 17;
            this.startPattern.Text = "+";
            this.startPattern.UseVisualStyleBackColor = true;
            this.startPattern.Click += new System.EventHandler(this.startPattern_Click);
            // 
            // reloadPatterns
            // 
            this.reloadPatterns.Location = new System.Drawing.Point(219, 74);
            this.reloadPatterns.Name = "reloadPatterns";
            this.reloadPatterns.Size = new System.Drawing.Size(23, 23);
            this.reloadPatterns.TabIndex = 16;
            this.reloadPatterns.Text = "R";
            this.reloadPatterns.UseVisualStyleBackColor = true;
            this.reloadPatterns.Click += new System.EventHandler(this.reloadPatterns_Click);
            // 
            // PatternPicker
            // 
            this.PatternPicker.FormattingEnabled = true;
            this.PatternPicker.Location = new System.Drawing.Point(10, 74);
            this.PatternPicker.Name = "PatternPicker";
            this.PatternPicker.Size = new System.Drawing.Size(203, 21);
            this.PatternPicker.TabIndex = 15;
            this.PatternPicker.SelectedIndexChanged += new System.EventHandler(this.PatternPicker_SelectedIndexChanged);
            // 
            // scanPointProgress
            // 
            this.scanPointProgress.AutoSize = true;
            this.scanPointProgress.Location = new System.Drawing.Point(81, 109);
            this.scanPointProgress.Name = "scanPointProgress";
            this.scanPointProgress.Size = new System.Drawing.Size(24, 13);
            this.scanPointProgress.TabIndex = 14;
            this.scanPointProgress.Text = "0/0";
            // 
            // repeatScan
            // 
            this.repeatScan.AutoSize = true;
            this.repeatScan.Location = new System.Drawing.Point(210, 109);
            this.repeatScan.Name = "repeatScan";
            this.repeatScan.Size = new System.Drawing.Size(61, 17);
            this.repeatScan.TabIndex = 13;
            this.repeatScan.Text = "Repeat";
            this.repeatScan.UseVisualStyleBackColor = true;
            this.repeatScan.CheckedChanged += new System.EventHandler(this.repeatScan_CheckedChanged);
            // 
            // RejectionStatus
            // 
            this.RejectionStatus.Enabled = false;
            this.RejectionStatus.Location = new System.Drawing.Point(7, 106);
            this.RejectionStatus.Name = "RejectionStatus";
            this.RejectionStatus.Size = new System.Drawing.Size(67, 20);
            this.RejectionStatus.TabIndex = 12;
            // 
            // save_data
            // 
            this.save_data.Location = new System.Drawing.Point(188, 44);
            this.save_data.Name = "save_data";
            this.save_data.Size = new System.Drawing.Size(83, 23);
            this.save_data.TabIndex = 9;
            this.save_data.Text = "Save Data";
            this.save_data.UseVisualStyleBackColor = true;
            this.save_data.Click += new System.EventHandler(this.save_data_Click);
            // 
            // clear_data
            // 
            this.clear_data.Location = new System.Drawing.Point(99, 44);
            this.clear_data.Name = "clear_data";
            this.clear_data.Size = new System.Drawing.Size(83, 23);
            this.clear_data.TabIndex = 8;
            this.clear_data.Text = "Clear Data";
            this.clear_data.UseVisualStyleBackColor = true;
            this.clear_data.Click += new System.EventHandler(this.clear_data_Click);
            // 
            // ScanParameterButton
            // 
            this.ScanParameterButton.Location = new System.Drawing.Point(10, 44);
            this.ScanParameterButton.Name = "ScanParameterButton";
            this.ScanParameterButton.Size = new System.Drawing.Size(83, 23);
            this.ScanParameterButton.TabIndex = 7;
            this.ScanParameterButton.Text = "Parameters";
            this.ScanParameterButton.UseVisualStyleBackColor = true;
            this.ScanParameterButton.Click += new System.EventHandler(this.ScanParameterButton_Click);
            // 
            // PluginSelector
            // 
            this.PluginSelector.FormattingEnabled = true;
            this.PluginSelector.Location = new System.Drawing.Point(80, 17);
            this.PluginSelector.Name = "PluginSelector";
            this.PluginSelector.Size = new System.Drawing.Size(191, 21);
            this.PluginSelector.TabIndex = 6;
            this.PluginSelector.SelectedIndexChanged += new System.EventHandler(this.PluginSelector_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Scan Plugin:";
            // 
            // stopScan
            // 
            this.stopScan.Enabled = false;
            this.stopScan.Location = new System.Drawing.Point(280, 75);
            this.stopScan.Name = "stopScan";
            this.stopScan.Size = new System.Drawing.Size(23, 51);
            this.stopScan.TabIndex = 11;
            this.stopScan.Text = "-";
            this.stopScan.UseVisualStyleBackColor = true;
            this.stopScan.Click += new System.EventHandler(this.stopScan_Click);
            // 
            // startScan
            // 
            this.startScan.Location = new System.Drawing.Point(280, 14);
            this.startScan.Name = "startScan";
            this.startScan.Size = new System.Drawing.Size(23, 55);
            this.startScan.TabIndex = 10;
            this.startScan.Text = "+";
            this.startScan.UseVisualStyleBackColor = true;
            this.startScan.Click += new System.EventHandler(this.startScan_Click);
            // 
            // scanGraph
            // 
            this.scanGraph.Cursors.AddRange(new NationalInstruments.UI.XYCursor[] {
            this.scanLowCursor,
            this.xyCursor2});
            this.scanGraph.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.scanGraph.Location = new System.Drawing.Point(614, 144);
            this.scanGraph.Name = "scanGraph";
            this.scanGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot3,
            this.scatterPlot4});
            this.scanGraph.Size = new System.Drawing.Size(359, 264);
            this.scanGraph.TabIndex = 15;
            this.scanGraph.UseColorGenerator = true;
            this.scanGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.scanGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // scanLowCursor
            // 
            this.scanLowCursor.Color = System.Drawing.Color.Cyan;
            this.scanLowCursor.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.scanLowCursor.LabelDisplay = NationalInstruments.UI.XYCursorLabelDisplay.ShowX;
            this.scanLowCursor.Plot = this.scatterPlot3;
            this.scanLowCursor.PointStyle = NationalInstruments.UI.PointStyle.Cross;
            this.scanLowCursor.XPosition = 0D;
            // 
            // scatterPlot3
            // 
            this.scatterPlot3.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.scatterPlot3.PointColor = System.Drawing.Color.Lime;
            this.scatterPlot3.PointStyle = NationalInstruments.UI.PointStyle.Cross;
            this.scatterPlot3.XAxis = this.xAxis1;
            this.scatterPlot3.YAxis = this.yAxis1;
            // 
            // xyCursor2
            // 
            this.xyCursor2.Color = System.Drawing.Color.Crimson;
            this.xyCursor2.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor2.LabelDisplay = NationalInstruments.UI.XYCursorLabelDisplay.ShowX;
            this.xyCursor2.Plot = this.scatterPlot3;
            this.xyCursor2.PointStyle = NationalInstruments.UI.PointStyle.Cross;
            this.xyCursor2.XPosition = 10D;
            // 
            // scatterPlot4
            // 
            this.scatterPlot4.LineColor = System.Drawing.Color.Lime;
            this.scatterPlot4.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.scatterPlot4.XAxis = this.xAxis1;
            this.scatterPlot4.YAxis = this.yAxis1;
            // 
            // integralView
            // 
            this.integralView.AllowUserToAddRows = false;
            this.integralView.AllowUserToDeleteRows = false;
            this.integralView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.integralView.Location = new System.Drawing.Point(980, 145);
            this.integralView.Name = "integralView";
            this.integralView.ReadOnly = true;
            this.integralView.Size = new System.Drawing.Size(191, 263);
            this.integralView.TabIndex = 16;
            // 
            // MOTMasterStuff
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.integralView);
            this.Controls.Add(this.scanGraph);
            this.Controls.Add(this.scanCtrl);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.DataTabs);
            this.Name = "MOTMasterStuff";
            this.Size = new System.Drawing.Size(1180, 411);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.scanCtrl.ResumeLayout(false);
            this.scanCtrl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scanGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.scanLowCursor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.integralView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl DataTabs;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbSamplingRate;
        private System.Windows.Forms.TextBox sampNum;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox armToF;
        private System.Windows.Forms.GroupBox scanCtrl;
        private NationalInstruments.UI.WindowsForms.ScatterGraph scanGraph;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private System.Windows.Forms.Button stopScan;
        private System.Windows.Forms.Button startScan;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox PluginSelector;
        private System.Windows.Forms.Button ScanParameterButton;
        private System.Windows.Forms.Button save_data;
        private System.Windows.Forms.Button clear_data;
        private System.Windows.Forms.CheckBox fixY;
        private System.Windows.Forms.CheckBox fixX;
        private System.Windows.Forms.CheckBox saveEnable;
        private System.Windows.Forms.TextBox RejectionStatus;
        private System.Windows.Forms.CheckBox repeatScan;
        private System.Windows.Forms.Label scanPointProgress;
        private System.Windows.Forms.Button startPattern;
        private System.Windows.Forms.Button reloadPatterns;
        private System.Windows.Forms.ComboBox PatternPicker;
        private NationalInstruments.UI.ScatterPlot scatterPlot3;
        private NationalInstruments.UI.ScatterPlot scatterPlot4;
        private System.Windows.Forms.CheckBox cameraEnable;
        private NationalInstruments.UI.XYCursor scanLowCursor;
        private NationalInstruments.UI.XYCursor xyCursor2;
        private System.Windows.Forms.DataGridView integralView;
    }
}
