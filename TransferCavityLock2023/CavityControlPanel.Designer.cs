namespace TransferCavityLock2023
{
    partial class CavityControlPanel
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.MasterLaserIntensityScatterGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.MasterDataPlot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis2 = new NationalInstruments.UI.XAxis();
            this.yAxis2 = new NationalInstruments.UI.YAxis();
            this.MasterFitPlot = new NationalInstruments.UI.ScatterPlot();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.CavLockVoltageTrackBar = new System.Windows.Forms.TrackBar();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.VoltageIntoCavityTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SummedVoltageTextBox = new System.Windows.Forms.TextBox();
            this.MasterGainTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.masterLockEnableCheck = new System.Windows.Forms.CheckBox();
            this.MasterSetPointTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.slaveLasersTab = new System.Windows.Forms.TabControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MasterLaserIntensityScatterGraph)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CavLockVoltageTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.slaveLasersTab, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(992, 510);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.62069F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 41.37931F));
            this.tableLayoutPanel2.Controls.Add(this.groupBox2, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.groupBox4, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 169F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 169F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 169F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 169F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 169F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 169F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 169F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 169F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 169F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(986, 169);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.MasterLaserIntensityScatterGraph);
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(562, 156);
            this.groupBox2.TabIndex = 67;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Reference laser";
            // 
            // MasterLaserIntensityScatterGraph
            // 
            this.MasterLaserIntensityScatterGraph.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.MasterLaserIntensityScatterGraph.Location = new System.Drawing.Point(6, 19);
            this.MasterLaserIntensityScatterGraph.Name = "MasterLaserIntensityScatterGraph";
            this.MasterLaserIntensityScatterGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.MasterDataPlot,
            this.MasterFitPlot});
            this.MasterLaserIntensityScatterGraph.Size = new System.Drawing.Size(548, 130);
            this.MasterLaserIntensityScatterGraph.TabIndex = 5;
            this.MasterLaserIntensityScatterGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis2});
            this.MasterLaserIntensityScatterGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis2});
            // 
            // MasterDataPlot
            // 
            this.MasterDataPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.MasterDataPlot.PointSize = new System.Drawing.Size(2, 2);
            this.MasterDataPlot.PointStyle = NationalInstruments.UI.PointStyle.SolidCircle;
            this.MasterDataPlot.XAxis = this.xAxis2;
            this.MasterDataPlot.YAxis = this.yAxis2;
            // 
            // MasterFitPlot
            // 
            this.MasterFitPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.MasterFitPlot.PointColor = System.Drawing.Color.LawnGreen;
            this.MasterFitPlot.PointStyle = NationalInstruments.UI.PointStyle.EmptyTriangleUp;
            this.MasterFitPlot.XAxis = this.xAxis2;
            this.MasterFitPlot.YAxis = this.yAxis2;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.CavLockVoltageTrackBar);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.VoltageIntoCavityTextBox);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.SummedVoltageTextBox);
            this.groupBox4.Controls.Add(this.MasterGainTextBox);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Controls.Add(this.masterLockEnableCheck);
            this.groupBox4.Controls.Add(this.MasterSetPointTextBox);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(580, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(403, 163);
            this.groupBox4.TabIndex = 6;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Cavity Settings";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 76);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(87, 13);
            this.label7.TabIndex = 65;
            this.label7.Text = "Summed Voltage";
            // 
            // CavLockVoltageTrackBar
            // 
            this.CavLockVoltageTrackBar.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CavLockVoltageTrackBar.Location = new System.Drawing.Point(6, 112);
            this.CavLockVoltageTrackBar.Maximum = 1000;
            this.CavLockVoltageTrackBar.Minimum = -1000;
            this.CavLockVoltageTrackBar.Name = "CavLockVoltageTrackBar";
            this.CavLockVoltageTrackBar.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.CavLockVoltageTrackBar.Size = new System.Drawing.Size(384, 45);
            this.CavLockVoltageTrackBar.TabIndex = 64;
            this.CavLockVoltageTrackBar.Scroll += new System.EventHandler(this.CavLockVoltageTrackBar_Scroll);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(199, 28);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 13);
            this.label6.TabIndex = 63;
            this.label6.Text = "at Ref Peak";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(199, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 13);
            this.label4.TabIndex = 62;
            this.label4.Text = "Voltage into Cavity";
            // 
            // VoltageIntoCavityTextBox
            // 
            this.VoltageIntoCavityTextBox.CausesValidation = false;
            this.VoltageIntoCavityTextBox.Location = new System.Drawing.Point(297, 21);
            this.VoltageIntoCavityTextBox.Name = "VoltageIntoCavityTextBox";
            this.VoltageIntoCavityTextBox.ReadOnly = true;
            this.VoltageIntoCavityTextBox.Size = new System.Drawing.Size(93, 20);
            this.VoltageIntoCavityTextBox.TabIndex = 61;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(199, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 60;
            this.label2.Text = "Summed Voltage";
            // 
            // SummedVoltageTextBox
            // 
            this.SummedVoltageTextBox.CausesValidation = false;
            this.SummedVoltageTextBox.Location = new System.Drawing.Point(297, 49);
            this.SummedVoltageTextBox.Name = "SummedVoltageTextBox";
            this.SummedVoltageTextBox.Size = new System.Drawing.Size(93, 20);
            this.SummedVoltageTextBox.TabIndex = 59;
            this.SummedVoltageTextBox.TextChanged += new System.EventHandler(this.SummedVoltageTextBox_TextChanged);
            // 
            // MasterGainTextBox
            // 
            this.MasterGainTextBox.AcceptsReturn = true;
            this.MasterGainTextBox.Location = new System.Drawing.Point(84, 49);
            this.MasterGainTextBox.Name = "MasterGainTextBox";
            this.MasterGainTextBox.Size = new System.Drawing.Size(57, 20);
            this.MasterGainTextBox.TabIndex = 58;
            this.MasterGainTextBox.Text = "1";
            this.MasterGainTextBox.TextChanged += new System.EventHandler(this.MasterGainTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 57;
            this.label1.Text = "Gain:";
            // 
            // masterLockEnableCheck
            // 
            this.masterLockEnableCheck.AutoSize = true;
            this.masterLockEnableCheck.Location = new System.Drawing.Point(147, 23);
            this.masterLockEnableCheck.Name = "masterLockEnableCheck";
            this.masterLockEnableCheck.Size = new System.Drawing.Size(50, 17);
            this.masterLockEnableCheck.TabIndex = 56;
            this.masterLockEnableCheck.Text = "Lock";
            this.masterLockEnableCheck.UseVisualStyleBackColor = true;
            this.masterLockEnableCheck.CheckedChanged += new System.EventHandler(this.masterLockEnableCheck_CheckedChanged);
            // 
            // MasterSetPointTextBox
            // 
            this.MasterSetPointTextBox.AcceptsReturn = true;
            this.MasterSetPointTextBox.Location = new System.Drawing.Point(84, 21);
            this.MasterSetPointTextBox.Name = "MasterSetPointTextBox";
            this.MasterSetPointTextBox.Size = new System.Drawing.Size(57, 20);
            this.MasterSetPointTextBox.TabIndex = 55;
            this.MasterSetPointTextBox.Text = "0";
            this.MasterSetPointTextBox.TextChanged += new System.EventHandler(this.MasterSetPointTextBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 54;
            this.label3.Text = "Set Point (V):";
            // 
            // slaveLasersTab
            // 
            this.slaveLasersTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.slaveLasersTab.Location = new System.Drawing.Point(3, 178);
            this.slaveLasersTab.Name = "slaveLasersTab";
            this.slaveLasersTab.SelectedIndex = 0;
            this.slaveLasersTab.Size = new System.Drawing.Size(986, 329);
            this.slaveLasersTab.TabIndex = 1;
            // 
            // CavityControlPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "CavityControlPanel";
            this.Size = new System.Drawing.Size(992, 510);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MasterLaserIntensityScatterGraph)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CavLockVoltageTrackBar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox groupBox2;
        public NationalInstruments.UI.WindowsForms.ScatterGraph MasterLaserIntensityScatterGraph;
        public NationalInstruments.UI.ScatterPlot MasterDataPlot;
        private NationalInstruments.UI.XAxis xAxis2;
        private NationalInstruments.UI.YAxis yAxis2;
        public NationalInstruments.UI.ScatterPlot MasterFitPlot;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.TrackBar CavLockVoltageTrackBar;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox VoltageIntoCavityTextBox;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox SummedVoltageTextBox;
        public System.Windows.Forms.TextBox MasterGainTextBox;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.CheckBox masterLockEnableCheck;
        public System.Windows.Forms.TextBox MasterSetPointTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabControl slaveLasersTab;

    }
}
