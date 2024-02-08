
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
            this.armToF = new System.Windows.Forms.CheckBox();
            this.sampNum = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbSamplingRate = new System.Windows.Forms.ComboBox();
            this.scanCtrl = new System.Windows.Forms.GroupBox();
            this.save_data = new System.Windows.Forms.Button();
            this.clear_data = new System.Windows.Forms.Button();
            this.ScanParameterButton = new System.Windows.Forms.Button();
            this.PluginSelector = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.stopScan = new System.Windows.Forms.Button();
            this.startScan = new System.Windows.Forms.Button();
            this.scanGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot1 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.scatterPlot2 = new NationalInstruments.UI.ScatterPlot();
            this.fixY = new System.Windows.Forms.CheckBox();
            this.fixX = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.scanCtrl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scanGraph)).BeginInit();
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
            this.sampNum.TabIndex = 3;
            this.sampNum.Text = "10000";
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
            this.cmbSamplingRate.TabIndex = 2;
            this.cmbSamplingRate.Text = "1000000";
            // 
            // scanCtrl
            // 
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
            // save_data
            // 
            this.save_data.Location = new System.Drawing.Point(188, 44);
            this.save_data.Name = "save_data";
            this.save_data.Size = new System.Drawing.Size(83, 23);
            this.save_data.TabIndex = 12;
            this.save_data.Text = "Save Data";
            this.save_data.UseVisualStyleBackColor = true;
            this.save_data.Click += new System.EventHandler(this.save_data_Click);
            // 
            // clear_data
            // 
            this.clear_data.Location = new System.Drawing.Point(99, 44);
            this.clear_data.Name = "clear_data";
            this.clear_data.Size = new System.Drawing.Size(83, 23);
            this.clear_data.TabIndex = 11;
            this.clear_data.Text = "Clear Data";
            this.clear_data.UseVisualStyleBackColor = true;
            this.clear_data.Click += new System.EventHandler(this.clear_data_Click);
            // 
            // ScanParameterButton
            // 
            this.ScanParameterButton.Location = new System.Drawing.Point(10, 44);
            this.ScanParameterButton.Name = "ScanParameterButton";
            this.ScanParameterButton.Size = new System.Drawing.Size(83, 23);
            this.ScanParameterButton.TabIndex = 10;
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
            this.PluginSelector.TabIndex = 9;
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
            this.stopScan.TabIndex = 7;
            this.stopScan.Text = "-";
            this.stopScan.UseVisualStyleBackColor = true;
            this.stopScan.Click += new System.EventHandler(this.stopScan_Click);
            // 
            // startScan
            // 
            this.startScan.Location = new System.Drawing.Point(280, 14);
            this.startScan.Name = "startScan";
            this.startScan.Size = new System.Drawing.Size(23, 55);
            this.startScan.TabIndex = 6;
            this.startScan.Text = "+";
            this.startScan.UseVisualStyleBackColor = true;
            this.startScan.Click += new System.EventHandler(this.startScan_Click);
            // 
            // scanGraph
            // 
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
            this.scatterPlot1,
            this.scatterPlot2});
            this.scanGraph.Size = new System.Drawing.Size(563, 264);
            this.scanGraph.TabIndex = 15;
            this.scanGraph.UseColorGenerator = true;
            this.scanGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.scanGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // scatterPlot1
            // 
            this.scatterPlot1.LineColor = System.Drawing.Color.DarkOrange;
            this.scatterPlot1.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.scatterPlot1.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.scatterPlot1.PointColor = System.Drawing.Color.DarkOrange;
            this.scatterPlot1.PointStyle = NationalInstruments.UI.PointStyle.Cross;
            this.scatterPlot1.XAxis = this.xAxis1;
            this.scatterPlot1.YAxis = this.yAxis1;
            // 
            // scatterPlot2
            // 
            this.scatterPlot2.LineColor = System.Drawing.Color.OrangeRed;
            this.scatterPlot2.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.scatterPlot2.XAxis = this.xAxis1;
            this.scatterPlot2.YAxis = this.yAxis1;
            // 
            // fixY
            // 
            this.fixY.AutoSize = true;
            this.fixY.Location = new System.Drawing.Point(9, 88);
            this.fixY.Name = "fixY";
            this.fixY.Size = new System.Drawing.Size(79, 17);
            this.fixY.TabIndex = 10;
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
            this.fixX.TabIndex = 9;
            this.fixX.Text = "Fix X range";
            this.fixX.UseVisualStyleBackColor = true;
            this.fixX.CheckedChanged += new System.EventHandler(this.fixX_CheckedChanged);
            // 
            // MOTMasterStuff
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
        private NationalInstruments.UI.ScatterPlot scatterPlot1;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private System.Windows.Forms.Button stopScan;
        private System.Windows.Forms.Button startScan;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox PluginSelector;
        private System.Windows.Forms.Button ScanParameterButton;
        private NationalInstruments.UI.ScatterPlot scatterPlot2;
        private System.Windows.Forms.Button save_data;
        private System.Windows.Forms.Button clear_data;
        private System.Windows.Forms.CheckBox fixY;
        private System.Windows.Forms.CheckBox fixX;
    }
}
