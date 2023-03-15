namespace TransferCavityLock2023
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadProfileSetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveProfileSetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.CavityVoltageReadScatterGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.cavityDataPlot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis3 = new NationalInstruments.UI.XAxis();
            this.yAxis3 = new NationalInstruments.UI.YAxis();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.voltageRampControl = new System.Windows.Forms.GroupBox();
            this.rampLED = new NationalInstruments.UI.WindowsForms.Led();
            this.label8 = new System.Windows.Forms.Label();
            this.rampStopButton = new System.Windows.Forms.Button();
            this.updateRateTextBox = new System.Windows.Forms.TextBox();
            this.rampStartButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.NumberOfScanpointsTextBox = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.scanAvCheckBox = new System.Windows.Forms.CheckBox();
            this.axisCheckBox = new System.Windows.Forms.CheckBox();
            this.logCheckBox = new System.Windows.Forms.CheckBox();
            this.dissableGUIupdateCheckBox = new System.Windows.Forms.CheckBox();
            this.fastFitCheckBox = new System.Windows.Forms.CheckBox();
            this.cavitiesTab = new System.Windows.Forms.TabControl();
            this.menuStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CavityVoltageReadScatterGraph)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            this.voltageRampControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rampLED)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1026, 24);
            this.menuStrip1.TabIndex = 59;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadProfileSetToolStripMenuItem,
            this.saveProfileSetToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadProfileSetToolStripMenuItem
            // 
            this.loadProfileSetToolStripMenuItem.Name = "loadProfileSetToolStripMenuItem";
            this.loadProfileSetToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.loadProfileSetToolStripMenuItem.Text = "Load parameters";
            this.loadProfileSetToolStripMenuItem.Click += new System.EventHandler(this.loadProfileSetToolStripMenuItem_Click);
            // 
            // saveProfileSetToolStripMenuItem
            // 
            this.saveProfileSetToolStripMenuItem.Name = "saveProfileSetToolStripMenuItem";
            this.saveProfileSetToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.saveProfileSetToolStripMenuItem.Text = "Save parameters";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cavitiesTab, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 27.06044F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 72.93956F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1026, 751);
            this.tableLayoutPanel1.TabIndex = 60;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.5567F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 41.4433F));
            this.tableLayoutPanel2.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1020, 197);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.CavityVoltageReadScatterGraph);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(591, 191);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cavity Scan Voltage";
            // 
            // CavityVoltageReadScatterGraph
            // 
            this.CavityVoltageReadScatterGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CavityVoltageReadScatterGraph.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.CavityVoltageReadScatterGraph.Location = new System.Drawing.Point(3, 16);
            this.CavityVoltageReadScatterGraph.Name = "CavityVoltageReadScatterGraph";
            this.CavityVoltageReadScatterGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.cavityDataPlot});
            this.CavityVoltageReadScatterGraph.Size = new System.Drawing.Size(585, 172);
            this.CavityVoltageReadScatterGraph.TabIndex = 13;
            this.CavityVoltageReadScatterGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis3});
            this.CavityVoltageReadScatterGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis3});
            this.CavityVoltageReadScatterGraph.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.CavityVoltageReadScatterGraph_PlotDataChanged);
            // 
            // cavityDataPlot
            // 
            this.cavityDataPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.cavityDataPlot.PointSize = new System.Drawing.Size(2, 2);
            this.cavityDataPlot.PointStyle = NationalInstruments.UI.PointStyle.SolidCircle;
            this.cavityDataPlot.XAxis = this.xAxis3;
            this.cavityDataPlot.YAxis = this.yAxis3;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.voltageRampControl, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.groupBox3, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(600, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 52.48619F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 47.51381F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(417, 191);
            this.tableLayoutPanel3.TabIndex = 6;
            // 
            // voltageRampControl
            // 
            this.voltageRampControl.Controls.Add(this.rampLED);
            this.voltageRampControl.Controls.Add(this.label8);
            this.voltageRampControl.Controls.Add(this.rampStopButton);
            this.voltageRampControl.Controls.Add(this.updateRateTextBox);
            this.voltageRampControl.Controls.Add(this.rampStartButton);
            this.voltageRampControl.Controls.Add(this.label5);
            this.voltageRampControl.Controls.Add(this.NumberOfScanpointsTextBox);
            this.voltageRampControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.voltageRampControl.Location = new System.Drawing.Point(3, 103);
            this.voltageRampControl.Name = "voltageRampControl";
            this.voltageRampControl.Size = new System.Drawing.Size(411, 85);
            this.voltageRampControl.TabIndex = 7;
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
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(102, 51);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(117, 13);
            this.label8.TabIndex = 23;
            this.label8.Text = "Lock Update Rate (Hz)";
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
            // updateRateTextBox
            // 
            this.updateRateTextBox.Enabled = false;
            this.updateRateTextBox.Location = new System.Drawing.Point(237, 48);
            this.updateRateTextBox.Name = "updateRateTextBox";
            this.updateRateTextBox.Size = new System.Drawing.Size(69, 20);
            this.updateRateTextBox.TabIndex = 56;
            this.updateRateTextBox.Text = "1";
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
            this.label5.Location = new System.Drawing.Point(129, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(90, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "Number of points:";
            // 
            // NumberOfScanpointsTextBox
            // 
            this.NumberOfScanpointsTextBox.Location = new System.Drawing.Point(237, 21);
            this.NumberOfScanpointsTextBox.Name = "NumberOfScanpointsTextBox";
            this.NumberOfScanpointsTextBox.Size = new System.Drawing.Size(69, 20);
            this.NumberOfScanpointsTextBox.TabIndex = 22;
            this.NumberOfScanpointsTextBox.TextChanged += new System.EventHandler(this.NumberOfScanpointsTextBox_TextChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tableLayoutPanel5);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(411, 94);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Fit Settings";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42.4F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 57.6F));
            this.tableLayoutPanel5.Controls.Add(this.scanAvCheckBox, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.axisCheckBox, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.logCheckBox, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.dissableGUIupdateCheckBox, 1, 1);
            this.tableLayoutPanel5.Controls.Add(this.fastFitCheckBox, 1, 2);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 3;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(405, 75);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // scanAvCheckBox
            // 
            this.scanAvCheckBox.AutoSize = true;
            this.scanAvCheckBox.Location = new System.Drawing.Point(3, 3);
            this.scanAvCheckBox.Name = "scanAvCheckBox";
            this.scanAvCheckBox.Size = new System.Drawing.Size(133, 17);
            this.scanAvCheckBox.TabIndex = 55;
            this.scanAvCheckBox.Text = "Average Scan Voltage";
            this.scanAvCheckBox.UseVisualStyleBackColor = true;
            // 
            // axisCheckBox
            // 
            this.axisCheckBox.AutoSize = true;
            this.axisCheckBox.Location = new System.Drawing.Point(174, 3);
            this.axisCheckBox.Name = "axisCheckBox";
            this.axisCheckBox.Size = new System.Drawing.Size(118, 17);
            this.axisCheckBox.TabIndex = 56;
            this.axisCheckBox.Text = "Disable axis update";
            this.axisCheckBox.UseVisualStyleBackColor = true;
            this.axisCheckBox.CheckedChanged += new System.EventHandler(this.axisCheckBox_CheckedChanged);
            // 
            // logCheckBox
            // 
            this.logCheckBox.AutoSize = true;
            this.logCheckBox.Location = new System.Drawing.Point(3, 28);
            this.logCheckBox.Name = "logCheckBox";
            this.logCheckBox.Size = new System.Drawing.Size(124, 17);
            this.logCheckBox.TabIndex = 57;
            this.logCheckBox.Text = "Log laser parameters";
            this.logCheckBox.UseVisualStyleBackColor = true;
            this.logCheckBox.CheckedChanged += new System.EventHandler(this.logCheckBox_CheckedChanged);
            // 
            // dissableGUIupdateCheckBox
            // 
            this.dissableGUIupdateCheckBox.AutoSize = true;
            this.dissableGUIupdateCheckBox.Location = new System.Drawing.Point(174, 28);
            this.dissableGUIupdateCheckBox.Name = "dissableGUIupdateCheckBox";
            this.dissableGUIupdateCheckBox.Size = new System.Drawing.Size(188, 17);
            this.dissableGUIupdateCheckBox.TabIndex = 58;
            this.dissableGUIupdateCheckBox.Text = "Disable GUI updates when locked";
            this.dissableGUIupdateCheckBox.UseVisualStyleBackColor = true;
            // 
            // fastFitCheckBox
            // 
            this.fastFitCheckBox.AutoSize = true;
            this.fastFitCheckBox.Location = new System.Drawing.Point(174, 53);
            this.fastFitCheckBox.Name = "fastFitCheckBox";
            this.fastFitCheckBox.Size = new System.Drawing.Size(183, 17);
            this.fastFitCheckBox.TabIndex = 59;
            this.fastFitCheckBox.Text = "Restrict fit to points close to peak";
            this.fastFitCheckBox.UseVisualStyleBackColor = true;
            // 
            // cavitiesTab
            // 
            this.cavitiesTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cavitiesTab.Location = new System.Drawing.Point(3, 206);
            this.cavitiesTab.Name = "cavitiesTab";
            this.cavitiesTab.SelectedIndex = 0;
            this.cavitiesTab.Size = new System.Drawing.Size(1020, 542);
            this.cavitiesTab.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1026, 775);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Transfer Cavity Lock 2012";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CavityVoltageReadScatterGraph)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.voltageRampControl.ResumeLayout(false);
            this.voltageRampControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rampLED)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadProfileSetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveProfileSetToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        public System.Windows.Forms.CheckBox scanAvCheckBox;
        public System.Windows.Forms.CheckBox axisCheckBox;
        public System.Windows.Forms.CheckBox logCheckBox;
        public System.Windows.Forms.CheckBox dissableGUIupdateCheckBox;
        public System.Windows.Forms.CheckBox fastFitCheckBox;
        private System.Windows.Forms.GroupBox voltageRampControl;
        private NationalInstruments.UI.WindowsForms.Led rampLED;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button rampStopButton;
        private System.Windows.Forms.TextBox updateRateTextBox;
        private System.Windows.Forms.Button rampStartButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox NumberOfScanpointsTextBox;
        private System.Windows.Forms.GroupBox groupBox1;
        public NationalInstruments.UI.WindowsForms.ScatterGraph CavityVoltageReadScatterGraph;
        public NationalInstruments.UI.ScatterPlot cavityDataPlot;
        private NationalInstruments.UI.XAxis xAxis3;
        private NationalInstruments.UI.YAxis yAxis3;
        private System.Windows.Forms.TabControl cavitiesTab;
    }
}

