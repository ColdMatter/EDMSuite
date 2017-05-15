namespace MicrocavityScanner.GUI
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.acquireToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scatterGraph1 = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot1 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.CurrentScanLine = new System.Windows.Forms.GroupBox();
            this.SuperScanGraph = new NationalInstruments.UI.WindowsForms.IntensityGraph();
            this.colorScale1 = new NationalInstruments.UI.ColorScale();
            this.intensityPlot1 = new NationalInstruments.UI.IntensityPlot();
            this.intensityXAxis1 = new NationalInstruments.UI.IntensityXAxis();
            this.intensityYAxis1 = new NationalInstruments.UI.IntensityYAxis();
            this.SuperScanCont = new System.Windows.Forms.GroupBox();
            this.FastAxis = new System.Windows.Forms.GroupBox();
            this.FastAxisEndLabel = new System.Windows.Forms.Label();
            this.FastAxisStartLabel = new System.Windows.Forms.Label();
            this.FastAxisSelectCombo = new System.Windows.Forms.ComboBox();
            this.FastAxisStartVoltLabel = new System.Windows.Forms.Label();
            this.FastAxisEndVoltLabel = new System.Windows.Forms.Label();
            this.SlowAxisCont = new System.Windows.Forms.GroupBox();
            this.SlowAxisEndVoltLabel = new System.Windows.Forms.Label();
            this.SlowAxisStartVoltLabel = new System.Windows.Forms.Label();
            this.SlowAxisEndLabel = new System.Windows.Forms.Label();
            this.SlowAxisStartLabel = new System.Windows.Forms.Label();
            this.SlowAxisSelectCombo = new System.Windows.Forms.ComboBox();
            this.TimingCont = new System.Windows.Forms.GroupBox();
            this.FastAxisResLabel = new System.Windows.Forms.Label();
            this.SlowAxisResLabel = new System.Windows.Forms.Label();
            this.ExposureLabel = new System.Windows.Forms.Label();
            this.ExposureUnitsLabel = new System.Windows.Forms.Label();
            this.Exposure = new System.Windows.Forms.TextBox();
            this.SlowAxisRes = new System.Windows.Forms.TextBox();
            this.SlowAxisEnd = new System.Windows.Forms.TextBox();
            this.SlowAxisStart = new System.Windows.Forms.TextBox();
            this.FastAxisRes = new System.Windows.Forms.TextBox();
            this.FastAxisEnd = new System.Windows.Forms.TextBox();
            this.FastAxisStart = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scatterGraph1)).BeginInit();
            this.CurrentScanLine.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SuperScanGraph)).BeginInit();
            this.SuperScanCont.SuspendLayout();
            this.FastAxis.SuspendLayout();
            this.SlowAxisCont.SuspendLayout();
            this.TimingCont.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.acquireToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(758, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // acquireToolStripMenuItem
            // 
            this.acquireToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.stopToolStripMenuItem});
            this.acquireToolStripMenuItem.Name = "acquireToolStripMenuItem";
            this.acquireToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.acquireToolStripMenuItem.Text = "Acquire";
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.startToolStripMenuItem.Text = "Start";
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.stopToolStripMenuItem.Text = "Stop";
            // 
            // scatterGraph1
            // 
            this.scatterGraph1.Location = new System.Drawing.Point(6, 19);
            this.scatterGraph1.Name = "scatterGraph1";
            this.scatterGraph1.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot1});
            this.scatterGraph1.Size = new System.Drawing.Size(520, 134);
            this.scatterGraph1.TabIndex = 1;
            this.scatterGraph1.UseColorGenerator = true;
            this.scatterGraph1.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.scatterGraph1.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // scatterPlot1
            // 
            this.scatterPlot1.XAxis = this.xAxis1;
            this.scatterPlot1.YAxis = this.yAxis1;
            // 
            // CurrentScanLine
            // 
            this.CurrentScanLine.Controls.Add(this.scatterGraph1);
            this.CurrentScanLine.Location = new System.Drawing.Point(214, 27);
            this.CurrentScanLine.Name = "CurrentScanLine";
            this.CurrentScanLine.Size = new System.Drawing.Size(532, 158);
            this.CurrentScanLine.TabIndex = 2;
            this.CurrentScanLine.TabStop = false;
            this.CurrentScanLine.Text = "Current Scan Line";
            this.CurrentScanLine.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // SuperScanGraph
            // 
            this.SuperScanGraph.ColorScales.AddRange(new NationalInstruments.UI.ColorScale[] {
            this.colorScale1});
            this.SuperScanGraph.Location = new System.Drawing.Point(6, 19);
            this.SuperScanGraph.Name = "SuperScanGraph";
            this.SuperScanGraph.Plots.AddRange(new NationalInstruments.UI.IntensityPlot[] {
            this.intensityPlot1});
            this.SuperScanGraph.Size = new System.Drawing.Size(520, 310);
            this.SuperScanGraph.TabIndex = 3;
            this.SuperScanGraph.XAxes.AddRange(new NationalInstruments.UI.IntensityXAxis[] {
            this.intensityXAxis1});
            this.SuperScanGraph.YAxes.AddRange(new NationalInstruments.UI.IntensityYAxis[] {
            this.intensityYAxis1});
            // 
            // intensityPlot1
            // 
            this.intensityPlot1.ColorScale = this.colorScale1;
            this.intensityPlot1.XAxis = this.intensityXAxis1;
            this.intensityPlot1.YAxis = this.intensityYAxis1;
            // 
            // SuperScanCont
            // 
            this.SuperScanCont.Controls.Add(this.SuperScanGraph);
            this.SuperScanCont.Location = new System.Drawing.Point(214, 186);
            this.SuperScanCont.Name = "SuperScanCont";
            this.SuperScanCont.Size = new System.Drawing.Size(532, 335);
            this.SuperScanCont.TabIndex = 4;
            this.SuperScanCont.TabStop = false;
            this.SuperScanCont.Text = "SuperScan";
            // 
            // FastAxis
            // 
            this.FastAxis.Controls.Add(this.FastAxisRes);
            this.FastAxis.Controls.Add(this.FastAxisResLabel);
            this.FastAxis.Controls.Add(this.FastAxisEndVoltLabel);
            this.FastAxis.Controls.Add(this.FastAxisStartVoltLabel);
            this.FastAxis.Controls.Add(this.FastAxisEnd);
            this.FastAxis.Controls.Add(this.FastAxisEndLabel);
            this.FastAxis.Controls.Add(this.FastAxisStart);
            this.FastAxis.Controls.Add(this.FastAxisStartLabel);
            this.FastAxis.Controls.Add(this.FastAxisSelectCombo);
            this.FastAxis.Location = new System.Drawing.Point(8, 27);
            this.FastAxis.Name = "FastAxis";
            this.FastAxis.Size = new System.Drawing.Size(200, 128);
            this.FastAxis.TabIndex = 5;
            this.FastAxis.TabStop = false;
            this.FastAxis.Text = "Fast Axis";
            // 
            // FastAxisEndLabel
            // 
            this.FastAxisEndLabel.AutoSize = true;
            this.FastAxisEndLabel.Location = new System.Drawing.Point(103, 43);
            this.FastAxisEndLabel.Name = "FastAxisEndLabel";
            this.FastAxisEndLabel.Size = new System.Drawing.Size(26, 13);
            this.FastAxisEndLabel.TabIndex = 9;
            this.FastAxisEndLabel.Text = "End";
            this.FastAxisEndLabel.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // FastAxisStartLabel
            // 
            this.FastAxisStartLabel.AutoSize = true;
            this.FastAxisStartLabel.Location = new System.Drawing.Point(6, 43);
            this.FastAxisStartLabel.Name = "FastAxisStartLabel";
            this.FastAxisStartLabel.Size = new System.Drawing.Size(29, 13);
            this.FastAxisStartLabel.TabIndex = 7;
            this.FastAxisStartLabel.Text = "Start";
            this.FastAxisStartLabel.Click += new System.EventHandler(this.label1_Click);
            // 
            // FastAxisSelectCombo
            // 
            this.FastAxisSelectCombo.FormattingEnabled = true;
            this.FastAxisSelectCombo.Location = new System.Drawing.Point(4, 19);
            this.FastAxisSelectCombo.Name = "FastAxisSelectCombo";
            this.FastAxisSelectCombo.Size = new System.Drawing.Size(188, 21);
            this.FastAxisSelectCombo.TabIndex = 1;
            this.FastAxisSelectCombo.SelectedIndexChanged += new System.EventHandler(this.FastAxisSelectCombo_SelectedIndexChanged);
            // 
            // FastAxisStartVoltLabel
            // 
            this.FastAxisStartVoltLabel.AutoSize = true;
            this.FastAxisStartVoltLabel.Location = new System.Drawing.Point(83, 62);
            this.FastAxisStartVoltLabel.Name = "FastAxisStartVoltLabel";
            this.FastAxisStartVoltLabel.Size = new System.Drawing.Size(14, 13);
            this.FastAxisStartVoltLabel.TabIndex = 11;
            this.FastAxisStartVoltLabel.Text = "V";
            // 
            // FastAxisEndVoltLabel
            // 
            this.FastAxisEndVoltLabel.AutoSize = true;
            this.FastAxisEndVoltLabel.Location = new System.Drawing.Point(183, 62);
            this.FastAxisEndVoltLabel.Name = "FastAxisEndVoltLabel";
            this.FastAxisEndVoltLabel.Size = new System.Drawing.Size(14, 13);
            this.FastAxisEndVoltLabel.TabIndex = 12;
            this.FastAxisEndVoltLabel.Text = "V";
            // 
            // SlowAxisCont
            // 
            this.SlowAxisCont.Controls.Add(this.SlowAxisRes);
            this.SlowAxisCont.Controls.Add(this.SlowAxisResLabel);
            this.SlowAxisCont.Controls.Add(this.SlowAxisEndVoltLabel);
            this.SlowAxisCont.Controls.Add(this.SlowAxisStartVoltLabel);
            this.SlowAxisCont.Controls.Add(this.SlowAxisEnd);
            this.SlowAxisCont.Controls.Add(this.SlowAxisEndLabel);
            this.SlowAxisCont.Controls.Add(this.SlowAxisStart);
            this.SlowAxisCont.Controls.Add(this.SlowAxisStartLabel);
            this.SlowAxisCont.Controls.Add(this.SlowAxisSelectCombo);
            this.SlowAxisCont.Location = new System.Drawing.Point(8, 161);
            this.SlowAxisCont.Name = "SlowAxisCont";
            this.SlowAxisCont.Size = new System.Drawing.Size(200, 128);
            this.SlowAxisCont.TabIndex = 6;
            this.SlowAxisCont.TabStop = false;
            this.SlowAxisCont.Text = "Slow Axis";
            this.SlowAxisCont.Enter += new System.EventHandler(this.groupBox1_Enter_1);
            // 
            // SlowAxisEndVoltLabel
            // 
            this.SlowAxisEndVoltLabel.AutoSize = true;
            this.SlowAxisEndVoltLabel.Location = new System.Drawing.Point(183, 62);
            this.SlowAxisEndVoltLabel.Name = "SlowAxisEndVoltLabel";
            this.SlowAxisEndVoltLabel.Size = new System.Drawing.Size(14, 13);
            this.SlowAxisEndVoltLabel.TabIndex = 12;
            this.SlowAxisEndVoltLabel.Text = "V";
            // 
            // SlowAxisStartVoltLabel
            // 
            this.SlowAxisStartVoltLabel.AutoSize = true;
            this.SlowAxisStartVoltLabel.Location = new System.Drawing.Point(83, 62);
            this.SlowAxisStartVoltLabel.Name = "SlowAxisStartVoltLabel";
            this.SlowAxisStartVoltLabel.Size = new System.Drawing.Size(14, 13);
            this.SlowAxisStartVoltLabel.TabIndex = 11;
            this.SlowAxisStartVoltLabel.Text = "V";
            // 
            // SlowAxisEndLabel
            // 
            this.SlowAxisEndLabel.AutoSize = true;
            this.SlowAxisEndLabel.Location = new System.Drawing.Point(103, 43);
            this.SlowAxisEndLabel.Name = "SlowAxisEndLabel";
            this.SlowAxisEndLabel.Size = new System.Drawing.Size(26, 13);
            this.SlowAxisEndLabel.TabIndex = 9;
            this.SlowAxisEndLabel.Text = "End";
            // 
            // SlowAxisStartLabel
            // 
            this.SlowAxisStartLabel.AutoSize = true;
            this.SlowAxisStartLabel.Location = new System.Drawing.Point(6, 43);
            this.SlowAxisStartLabel.Name = "SlowAxisStartLabel";
            this.SlowAxisStartLabel.Size = new System.Drawing.Size(29, 13);
            this.SlowAxisStartLabel.TabIndex = 7;
            this.SlowAxisStartLabel.Text = "Start";
            // 
            // SlowAxisSelectCombo
            // 
            this.SlowAxisSelectCombo.FormattingEnabled = true;
            this.SlowAxisSelectCombo.Location = new System.Drawing.Point(4, 19);
            this.SlowAxisSelectCombo.Name = "SlowAxisSelectCombo";
            this.SlowAxisSelectCombo.Size = new System.Drawing.Size(188, 21);
            this.SlowAxisSelectCombo.TabIndex = 5;
            this.SlowAxisSelectCombo.SelectedIndexChanged += new System.EventHandler(this.SlowAxisSelectCombo_SelectedIndexChanged);
            // 
            // TimingCont
            // 
            this.TimingCont.Controls.Add(this.ExposureUnitsLabel);
            this.TimingCont.Controls.Add(this.Exposure);
            this.TimingCont.Controls.Add(this.ExposureLabel);
            this.TimingCont.Location = new System.Drawing.Point(8, 295);
            this.TimingCont.Name = "TimingCont";
            this.TimingCont.Size = new System.Drawing.Size(200, 60);
            this.TimingCont.TabIndex = 7;
            this.TimingCont.TabStop = false;
            this.TimingCont.Text = "Timing";
            // 
            // FastAxisResLabel
            // 
            this.FastAxisResLabel.AutoSize = true;
            this.FastAxisResLabel.Location = new System.Drawing.Point(6, 82);
            this.FastAxisResLabel.Name = "FastAxisResLabel";
            this.FastAxisResLabel.Size = new System.Drawing.Size(57, 13);
            this.FastAxisResLabel.TabIndex = 13;
            this.FastAxisResLabel.Text = "Resolution";
            // 
            // SlowAxisResLabel
            // 
            this.SlowAxisResLabel.AutoSize = true;
            this.SlowAxisResLabel.Location = new System.Drawing.Point(6, 82);
            this.SlowAxisResLabel.Name = "SlowAxisResLabel";
            this.SlowAxisResLabel.Size = new System.Drawing.Size(57, 13);
            this.SlowAxisResLabel.TabIndex = 15;
            this.SlowAxisResLabel.Text = "Resolution";
            // 
            // ExposureLabel
            // 
            this.ExposureLabel.AutoSize = true;
            this.ExposureLabel.Location = new System.Drawing.Point(6, 17);
            this.ExposureLabel.Name = "ExposureLabel";
            this.ExposureLabel.Size = new System.Drawing.Size(51, 13);
            this.ExposureLabel.TabIndex = 0;
            this.ExposureLabel.Text = "Exposure";
            // 
            // ExposureUnitsLabel
            // 
            this.ExposureUnitsLabel.AutoSize = true;
            this.ExposureUnitsLabel.Location = new System.Drawing.Point(110, 36);
            this.ExposureUnitsLabel.Name = "ExposureUnitsLabel";
            this.ExposureUnitsLabel.Size = new System.Drawing.Size(20, 13);
            this.ExposureUnitsLabel.TabIndex = 2;
            this.ExposureUnitsLabel.Text = "ms";
            this.ExposureUnitsLabel.Click += new System.EventHandler(this.label1_Click_2);
            // 
            // Exposure
            // 
            this.Exposure.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::MicrocavityScanner.Properties.Settings.Default, "ExposureBind", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.Exposure.Location = new System.Drawing.Point(4, 33);
            this.Exposure.Name = "Exposure";
            this.Exposure.Size = new System.Drawing.Size(100, 20);
            this.Exposure.TabIndex = 9;
            this.Exposure.Text = global::MicrocavityScanner.Properties.Settings.Default.ExposureBind;
            this.Exposure.TextChanged += new System.EventHandler(this.Exposure_TextChanged);
            // 
            // SlowAxisRes
            // 
            this.SlowAxisRes.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::MicrocavityScanner.Properties.Settings.Default, "SlowAxisResBind", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.SlowAxisRes.Location = new System.Drawing.Point(4, 98);
            this.SlowAxisRes.Name = "SlowAxisRes";
            this.SlowAxisRes.Size = new System.Drawing.Size(88, 20);
            this.SlowAxisRes.TabIndex = 8;
            this.SlowAxisRes.Text = global::MicrocavityScanner.Properties.Settings.Default.SlowAxisResBind;
            this.SlowAxisRes.TextChanged += new System.EventHandler(this.SlowAxisRes_TextChanged);
            // 
            // SlowAxisEnd
            // 
            this.SlowAxisEnd.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::MicrocavityScanner.Properties.Settings.Default, "SlowAxisEndBind", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.SlowAxisEnd.Location = new System.Drawing.Point(106, 59);
            this.SlowAxisEnd.Name = "SlowAxisEnd";
            this.SlowAxisEnd.Size = new System.Drawing.Size(71, 20);
            this.SlowAxisEnd.TabIndex = 7;
            this.SlowAxisEnd.Text = global::MicrocavityScanner.Properties.Settings.Default.SlowAxisEndBind;
            this.SlowAxisEnd.TextChanged += new System.EventHandler(this.SlowAxisEnd_TextChanged);
            // 
            // SlowAxisStart
            // 
            this.SlowAxisStart.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::MicrocavityScanner.Properties.Settings.Default, "SlowAxisStart", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.SlowAxisStart.Location = new System.Drawing.Point(4, 59);
            this.SlowAxisStart.Name = "SlowAxisStart";
            this.SlowAxisStart.Size = new System.Drawing.Size(71, 20);
            this.SlowAxisStart.TabIndex = 6;
            this.SlowAxisStart.Text = global::MicrocavityScanner.Properties.Settings.Default.SlowAxisStart;
            this.SlowAxisStart.TextChanged += new System.EventHandler(this.SlowAxisStart_TextChanged);
            // 
            // FastAxisRes
            // 
            this.FastAxisRes.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::MicrocavityScanner.Properties.Settings.Default, "FastAxisResBind", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.FastAxisRes.Location = new System.Drawing.Point(4, 98);
            this.FastAxisRes.Name = "FastAxisRes";
            this.FastAxisRes.Size = new System.Drawing.Size(88, 20);
            this.FastAxisRes.TabIndex = 4;
            this.FastAxisRes.Text = global::MicrocavityScanner.Properties.Settings.Default.FastAxisResBind;
            this.FastAxisRes.TextChanged += new System.EventHandler(this.FastAxisRes_TextChanged);
            // 
            // FastAxisEnd
            // 
            this.FastAxisEnd.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::MicrocavityScanner.Properties.Settings.Default, "FastAxisEndBind", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.FastAxisEnd.Location = new System.Drawing.Point(106, 59);
            this.FastAxisEnd.Name = "FastAxisEnd";
            this.FastAxisEnd.Size = new System.Drawing.Size(71, 20);
            this.FastAxisEnd.TabIndex = 3;
            this.FastAxisEnd.Text = global::MicrocavityScanner.Properties.Settings.Default.FastAxisEndBind;
            this.FastAxisEnd.TextChanged += new System.EventHandler(this.FastAxisEnd_TextChanged);
            // 
            // FastAxisStart
            // 
            this.FastAxisStart.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::MicrocavityScanner.Properties.Settings.Default, "FastAxisStartBind", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.FastAxisStart.Location = new System.Drawing.Point(4, 59);
            this.FastAxisStart.Name = "FastAxisStart";
            this.FastAxisStart.Size = new System.Drawing.Size(71, 20);
            this.FastAxisStart.TabIndex = 2;
            this.FastAxisStart.Text = global::MicrocavityScanner.Properties.Settings.Default.FastAxisStartBind;
            this.FastAxisStart.TextChanged += new System.EventHandler(this.FastAxisStart_TextChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(758, 533);
            this.Controls.Add(this.TimingCont);
            this.Controls.Add(this.SlowAxisCont);
            this.Controls.Add(this.FastAxis);
            this.Controls.Add(this.SuperScanCont);
            this.Controls.Add(this.CurrentScanLine);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "MicrocavityScanner";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scatterGraph1)).EndInit();
            this.CurrentScanLine.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SuperScanGraph)).EndInit();
            this.SuperScanCont.ResumeLayout(false);
            this.FastAxis.ResumeLayout(false);
            this.FastAxis.PerformLayout();
            this.SlowAxisCont.ResumeLayout(false);
            this.SlowAxisCont.PerformLayout();
            this.TimingCont.ResumeLayout(false);
            this.TimingCont.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem acquireToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private NationalInstruments.UI.WindowsForms.ScatterGraph scatterGraph1;
        private NationalInstruments.UI.ScatterPlot scatterPlot1;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private System.Windows.Forms.GroupBox CurrentScanLine;
        private NationalInstruments.UI.WindowsForms.IntensityGraph SuperScanGraph;
        private NationalInstruments.UI.ColorScale colorScale1;
        private NationalInstruments.UI.IntensityPlot intensityPlot1;
        private NationalInstruments.UI.IntensityXAxis intensityXAxis1;
        private NationalInstruments.UI.IntensityYAxis intensityYAxis1;
        private System.Windows.Forms.GroupBox SuperScanCont;
        private System.Windows.Forms.GroupBox FastAxis;
        private System.Windows.Forms.Label FastAxisStartLabel;
        private System.Windows.Forms.ComboBox FastAxisSelectCombo;
        private System.Windows.Forms.TextBox FastAxisEnd;
        private System.Windows.Forms.Label FastAxisEndLabel;
        private System.Windows.Forms.TextBox FastAxisStart;
        private System.Windows.Forms.Label FastAxisEndVoltLabel;
        private System.Windows.Forms.Label FastAxisStartVoltLabel;
        private System.Windows.Forms.GroupBox SlowAxisCont;
        private System.Windows.Forms.Label SlowAxisEndVoltLabel;
        private System.Windows.Forms.Label SlowAxisStartVoltLabel;
        private System.Windows.Forms.TextBox SlowAxisEnd;
        private System.Windows.Forms.Label SlowAxisEndLabel;
        private System.Windows.Forms.TextBox SlowAxisStart;
        private System.Windows.Forms.Label SlowAxisStartLabel;
        private System.Windows.Forms.ComboBox SlowAxisSelectCombo;
        private System.Windows.Forms.TextBox FastAxisRes;
        private System.Windows.Forms.Label FastAxisResLabel;
        private System.Windows.Forms.TextBox SlowAxisRes;
        private System.Windows.Forms.Label SlowAxisResLabel;
        private System.Windows.Forms.GroupBox TimingCont;
        private System.Windows.Forms.Label ExposureUnitsLabel;
        private System.Windows.Forms.TextBox Exposure;
        private System.Windows.Forms.Label ExposureLabel;
    }
}

