
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
            this.stopScan = new System.Windows.Forms.Button();
            this.startScan = new System.Windows.Forms.Button();
            this.scanTabs = new System.Windows.Forms.TabControl();
            this.ParamScan = new System.Windows.Forms.TabPage();
            this.pScanDir = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.pShots = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pSteps = new System.Windows.Forms.TextBox();
            this.pEnd = new System.Windows.Forms.TextBox();
            this.pStart = new System.Windows.Forms.TextBox();
            this.pParam = new System.Windows.Forms.TextBox();
            this.WMLScan = new System.Windows.Forms.TabPage();
            this.WMLScanDir = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.WMLShots = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.WMLSteps = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.WMLEnd = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.WMLStart = new System.Windows.Forms.TextBox();
            this.WMLOffset = new System.Windows.Forms.TextBox();
            this.WMLServer = new System.Windows.Forms.TextBox();
            this.WMLLaser = new System.Windows.Forms.TextBox();
            this.scanGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot1 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.groupBox1.SuspendLayout();
            this.scanCtrl.SuspendLayout();
            this.scanTabs.SuspendLayout();
            this.ParamScan.SuspendLayout();
            this.WMLScan.SuspendLayout();
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
            this.armToF.Location = new System.Drawing.Point(6, 111);
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
            this.scanCtrl.Controls.Add(this.stopScan);
            this.scanCtrl.Controls.Add(this.startScan);
            this.scanCtrl.Controls.Add(this.scanTabs);
            this.scanCtrl.Enabled = false;
            this.scanCtrl.Location = new System.Drawing.Point(868, 6);
            this.scanCtrl.Name = "scanCtrl";
            this.scanCtrl.Size = new System.Drawing.Size(309, 132);
            this.scanCtrl.TabIndex = 3;
            this.scanCtrl.TabStop = false;
            this.scanCtrl.Text = "Scan Control";
            // 
            // stopScan
            // 
            this.stopScan.Enabled = false;
            this.stopScan.Location = new System.Drawing.Point(280, 87);
            this.stopScan.Name = "stopScan";
            this.stopScan.Size = new System.Drawing.Size(23, 39);
            this.stopScan.TabIndex = 7;
            this.stopScan.Text = "-";
            this.stopScan.UseVisualStyleBackColor = true;
            this.stopScan.Click += new System.EventHandler(this.stopScan_Click);
            // 
            // startScan
            // 
            this.startScan.Location = new System.Drawing.Point(280, 39);
            this.startScan.Name = "startScan";
            this.startScan.Size = new System.Drawing.Size(23, 39);
            this.startScan.TabIndex = 6;
            this.startScan.Text = "+";
            this.startScan.UseVisualStyleBackColor = true;
            this.startScan.Click += new System.EventHandler(this.startScan_Click);
            // 
            // scanTabs
            // 
            this.scanTabs.Controls.Add(this.ParamScan);
            this.scanTabs.Controls.Add(this.WMLScan);
            this.scanTabs.Location = new System.Drawing.Point(7, 20);
            this.scanTabs.Name = "scanTabs";
            this.scanTabs.SelectedIndex = 0;
            this.scanTabs.Size = new System.Drawing.Size(271, 106);
            this.scanTabs.TabIndex = 5;
            this.scanTabs.SelectedIndexChanged += new System.EventHandler(this.scanTabs_SelectedIndexChanged);
            // 
            // ParamScan
            // 
            this.ParamScan.AutoScroll = true;
            this.ParamScan.BackColor = System.Drawing.SystemColors.Control;
            this.ParamScan.Controls.Add(this.pScanDir);
            this.ParamScan.Controls.Add(this.label14);
            this.ParamScan.Controls.Add(this.label13);
            this.ParamScan.Controls.Add(this.pShots);
            this.ParamScan.Controls.Add(this.label6);
            this.ParamScan.Controls.Add(this.label4);
            this.ParamScan.Controls.Add(this.label3);
            this.ParamScan.Controls.Add(this.label2);
            this.ParamScan.Controls.Add(this.pSteps);
            this.ParamScan.Controls.Add(this.pEnd);
            this.ParamScan.Controls.Add(this.pStart);
            this.ParamScan.Controls.Add(this.pParam);
            this.ParamScan.Location = new System.Drawing.Point(4, 22);
            this.ParamScan.Name = "ParamScan";
            this.ParamScan.Padding = new System.Windows.Forms.Padding(3);
            this.ParamScan.Size = new System.Drawing.Size(263, 80);
            this.ParamScan.TabIndex = 0;
            this.ParamScan.Text = "Parameter";
            // 
            // pScanDir
            // 
            this.pScanDir.FormattingEnabled = true;
            this.pScanDir.Items.AddRange(new object[] {
            "up",
            "down",
            "updown",
            "downup",
            "random"});
            this.pScanDir.Location = new System.Drawing.Point(106, 126);
            this.pScanDir.Name = "pScanDir";
            this.pScanDir.Size = new System.Drawing.Size(127, 21);
            this.pScanDir.TabIndex = 16;
            this.pScanDir.Text = "up";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 130);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(80, 13);
            this.label14.TabIndex = 15;
            this.label14.Text = "Scan Direction:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 105);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(43, 13);
            this.label13.TabIndex = 13;
            this.label13.Text = "nShots:";
            // 
            // pShots
            // 
            this.pShots.Location = new System.Drawing.Point(106, 102);
            this.pShots.Name = "pShots";
            this.pShots.Size = new System.Drawing.Size(127, 20);
            this.pShots.TabIndex = 12;
            this.pShots.Text = "1";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 81);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "nSteps:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 57);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "End:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Start:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Parameter:";
            // 
            // pSteps
            // 
            this.pSteps.Location = new System.Drawing.Point(106, 78);
            this.pSteps.Name = "pSteps";
            this.pSteps.Size = new System.Drawing.Size(127, 20);
            this.pSteps.TabIndex = 9;
            this.pSteps.Text = "100";
            // 
            // pEnd
            // 
            this.pEnd.Location = new System.Drawing.Point(106, 54);
            this.pEnd.Name = "pEnd";
            this.pEnd.Size = new System.Drawing.Size(127, 20);
            this.pEnd.TabIndex = 8;
            // 
            // pStart
            // 
            this.pStart.Location = new System.Drawing.Point(106, 31);
            this.pStart.Name = "pStart";
            this.pStart.Size = new System.Drawing.Size(127, 20);
            this.pStart.TabIndex = 7;
            // 
            // pParam
            // 
            this.pParam.Location = new System.Drawing.Point(106, 7);
            this.pParam.Name = "pParam";
            this.pParam.Size = new System.Drawing.Size(127, 20);
            this.pParam.TabIndex = 6;
            // 
            // WMLScan
            // 
            this.WMLScan.AutoScroll = true;
            this.WMLScan.BackColor = System.Drawing.SystemColors.Control;
            this.WMLScan.Controls.Add(this.WMLScanDir);
            this.WMLScan.Controls.Add(this.label15);
            this.WMLScan.Controls.Add(this.label16);
            this.WMLScan.Controls.Add(this.WMLShots);
            this.WMLScan.Controls.Add(this.label12);
            this.WMLScan.Controls.Add(this.WMLSteps);
            this.WMLScan.Controls.Add(this.label11);
            this.WMLScan.Controls.Add(this.WMLEnd);
            this.WMLScan.Controls.Add(this.label7);
            this.WMLScan.Controls.Add(this.label8);
            this.WMLScan.Controls.Add(this.label9);
            this.WMLScan.Controls.Add(this.label10);
            this.WMLScan.Controls.Add(this.WMLStart);
            this.WMLScan.Controls.Add(this.WMLOffset);
            this.WMLScan.Controls.Add(this.WMLServer);
            this.WMLScan.Controls.Add(this.WMLLaser);
            this.WMLScan.Location = new System.Drawing.Point(4, 22);
            this.WMLScan.Name = "WMLScan";
            this.WMLScan.Padding = new System.Windows.Forms.Padding(3);
            this.WMLScan.Size = new System.Drawing.Size(263, 80);
            this.WMLScan.TabIndex = 1;
            this.WMLScan.Text = "WML";
            // 
            // WMLScanDir
            // 
            this.WMLScanDir.FormattingEnabled = true;
            this.WMLScanDir.Items.AddRange(new object[] {
            "up",
            "down",
            "updown",
            "downup",
            "random"});
            this.WMLScanDir.Location = new System.Drawing.Point(108, 171);
            this.WMLScanDir.Name = "WMLScanDir";
            this.WMLScanDir.Size = new System.Drawing.Size(127, 21);
            this.WMLScanDir.TabIndex = 27;
            this.WMLScanDir.Text = "up";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(8, 175);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(80, 13);
            this.label15.TabIndex = 26;
            this.label15.Text = "Scan Direction:";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(8, 150);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(43, 13);
            this.label16.TabIndex = 25;
            this.label16.Text = "nShots:";
            // 
            // WMLShots
            // 
            this.WMLShots.Location = new System.Drawing.Point(108, 147);
            this.WMLShots.Name = "WMLShots";
            this.WMLShots.Size = new System.Drawing.Size(127, 20);
            this.WMLShots.TabIndex = 24;
            this.WMLShots.Text = "1";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(8, 127);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(43, 13);
            this.label12.TabIndex = 23;
            this.label12.Text = "nSteps:";
            // 
            // WMLSteps
            // 
            this.WMLSteps.Location = new System.Drawing.Point(108, 124);
            this.WMLSteps.Name = "WMLSteps";
            this.WMLSteps.Size = new System.Drawing.Size(127, 20);
            this.WMLSteps.TabIndex = 22;
            this.WMLSteps.Text = "100";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(8, 103);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(59, 13);
            this.label11.TabIndex = 21;
            this.label11.Text = "End [GHz]:";
            // 
            // WMLEnd
            // 
            this.WMLEnd.Location = new System.Drawing.Point(108, 100);
            this.WMLEnd.Name = "WMLEnd";
            this.WMLEnd.Size = new System.Drawing.Size(127, 20);
            this.WMLEnd.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 80);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 13);
            this.label7.TabIndex = 19;
            this.label7.Text = "Start [GHz]:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 56);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(67, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "Offset [THz]:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 33);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(70, 13);
            this.label9.TabIndex = 17;
            this.label9.Text = "WML Server:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(8, 9);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(36, 13);
            this.label10.TabIndex = 16;
            this.label10.Text = "Laser:";
            // 
            // WMLStart
            // 
            this.WMLStart.Location = new System.Drawing.Point(108, 77);
            this.WMLStart.Name = "WMLStart";
            this.WMLStart.Size = new System.Drawing.Size(127, 20);
            this.WMLStart.TabIndex = 13;
            // 
            // WMLOffset
            // 
            this.WMLOffset.Location = new System.Drawing.Point(108, 53);
            this.WMLOffset.Name = "WMLOffset";
            this.WMLOffset.Size = new System.Drawing.Size(127, 20);
            this.WMLOffset.TabIndex = 12;
            // 
            // WMLServer
            // 
            this.WMLServer.Location = new System.Drawing.Point(108, 30);
            this.WMLServer.Name = "WMLServer";
            this.WMLServer.Size = new System.Drawing.Size(127, 20);
            this.WMLServer.TabIndex = 11;
            // 
            // WMLLaser
            // 
            this.WMLLaser.Location = new System.Drawing.Point(108, 6);
            this.WMLLaser.Name = "WMLLaser";
            this.WMLLaser.Size = new System.Drawing.Size(127, 20);
            this.WMLLaser.TabIndex = 10;
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
            this.scatterPlot1});
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
            this.scatterPlot1.PointColor = System.Drawing.Color.DarkOrange;
            this.scatterPlot1.XAxis = this.xAxis1;
            this.scatterPlot1.YAxis = this.yAxis1;
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
            this.scanTabs.ResumeLayout(false);
            this.ParamScan.ResumeLayout(false);
            this.ParamScan.PerformLayout();
            this.WMLScan.ResumeLayout(false);
            this.WMLScan.PerformLayout();
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
        private System.Windows.Forms.TabControl scanTabs;
        private System.Windows.Forms.TabPage ParamScan;
        private System.Windows.Forms.TabPage WMLScan;
        private NationalInstruments.UI.WindowsForms.ScatterGraph scanGraph;
        private NationalInstruments.UI.ScatterPlot scatterPlot1;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox pSteps;
        private System.Windows.Forms.TextBox pEnd;
        private System.Windows.Forms.TextBox pStart;
        private System.Windows.Forms.TextBox pParam;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox WMLStart;
        private System.Windows.Forms.TextBox WMLOffset;
        private System.Windows.Forms.TextBox WMLServer;
        private System.Windows.Forms.TextBox WMLLaser;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox WMLEnd;
        private System.Windows.Forms.Button stopScan;
        private System.Windows.Forms.Button startScan;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox WMLSteps;
        private System.Windows.Forms.ComboBox pScanDir;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox pShots;
        private System.Windows.Forms.ComboBox WMLScanDir;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox WMLShots;
    }
}
