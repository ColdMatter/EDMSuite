using NationalInstruments.UI;

namespace CaFBECHadwareController.Controls
{
    partial class SourceTabView
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
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.readButton = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chkAutoScale = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbSamplingRate = new System.Windows.Forms.ComboBox();
            this.chkToF = new System.Windows.Forms.CheckBox();
            this.tempGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot1 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.chkSaveTrace = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label14 = new System.Windows.Forms.Label();
            this.numFlowTimeout = new System.Windows.Forms.NumericUpDown();
            this.chkAutoValveControl = new System.Windows.Forms.CheckBox();
            this.chkHeValve = new System.Windows.Forms.CheckBox();
            this.chkSF6Valve = new System.Windows.Forms.CheckBox();
            this.chkAutoFlowControl = new System.Windows.Forms.CheckBox();
            this.chkAO1Enable = new System.Windows.Forms.CheckBox();
            this.chkAO0Enable = new System.Windows.Forms.CheckBox();
            this.lblAO1 = new System.Windows.Forms.Label();
            this.lblAO0 = new System.Windows.Forms.Label();
            this.numAO1 = new System.Windows.Forms.NumericUpDown();
            this.numAO0 = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblheflow = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblsf6flow = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel6.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tempGraph)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFlowTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAO1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAO0)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 1;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Controls.Add(this.readButton, 0, 2);
            this.tableLayoutPanel6.Controls.Add(this.groupBox3, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.groupBox4, 0, 1);
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 3;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 64.51613F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35.48387F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(687, 607);
            this.tableLayoutPanel6.TabIndex = 3;
            // 
            // readButton
            // 
            this.readButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.readButton.Location = new System.Drawing.Point(294, 565);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(99, 34);
            this.readButton.TabIndex = 3;
            this.readButton.Text = "Start Reading";
            this.readButton.UseVisualStyleBackColor = true;
            this.readButton.Click += new System.EventHandler(this.toggleReading);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chkAutoScale);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.cmbSamplingRate);
            this.groupBox3.Controls.Add(this.chkToF);
            this.groupBox3.Controls.Add(this.tempGraph);
            this.groupBox3.Controls.Add(this.chkSaveTrace);
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(681, 354);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "PMT - Time of flight";
            // 
            // chkAutoScale
            // 
            this.chkAutoScale.AutoSize = true;
            this.chkAutoScale.Location = new System.Drawing.Point(174, 327);
            this.chkAutoScale.Name = "chkAutoScale";
            this.chkAutoScale.Size = new System.Drawing.Size(78, 17);
            this.chkAutoScale.TabIndex = 7;
            this.chkAutoScale.Text = "Auto Scale";
            this.chkAutoScale.UseVisualStyleBackColor = true;
            this.chkAutoScale.CheckedChanged += new System.EventHandler(this.chkAutoScale_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(251, 329);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Sampling rate (Hz):";
            this.label5.Click += new System.EventHandler(this.label5_Click);
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
            this.cmbSamplingRate.Location = new System.Drawing.Point(353, 326);
            this.cmbSamplingRate.Name = "cmbSamplingRate";
            this.cmbSamplingRate.Size = new System.Drawing.Size(72, 21);
            this.cmbSamplingRate.TabIndex = 4;
            this.cmbSamplingRate.Text = "100000";
            this.cmbSamplingRate.SelectedIndexChanged += new System.EventHandler(this.samplingRateSelect);
            // 
            // chkToF
            // 
            this.chkToF.AutoSize = true;
            this.chkToF.Location = new System.Drawing.Point(7, 327);
            this.chkToF.Name = "chkToF";
            this.chkToF.Size = new System.Drawing.Size(80, 17);
            this.chkToF.TabIndex = 3;
            this.chkToF.Text = "Show trace";
            this.chkToF.UseVisualStyleBackColor = true;
            // 
            // tempGraph
            // 
            this.tempGraph.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.tempGraph.Location = new System.Drawing.Point(6, 19);
            this.tempGraph.Name = "tempGraph";
            this.tempGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot1});
            this.tempGraph.Size = new System.Drawing.Size(669, 301);
            this.tempGraph.TabIndex = 0;
            this.tempGraph.UseColorGenerator = true;
            this.tempGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.tempGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            this.tempGraph.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.tempGraph_PlotDataChanged);
            // 
            // scatterPlot1
            // 
            this.scatterPlot1.XAxis = this.xAxis1;
            this.scatterPlot1.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.Caption = "Time (ms)";
            // 
            // yAxis1
            // 
            this.yAxis1.Caption = "Voltage (V)";
            this.yAxis1.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.yAxis1.Range = new NationalInstruments.UI.Range(0D, 1D);
            // 
            // chkSaveTrace
            // 
            this.chkSaveTrace.AutoSize = true;
            this.chkSaveTrace.Location = new System.Drawing.Point(93, 327);
            this.chkSaveTrace.Name = "chkSaveTrace";
            this.chkSaveTrace.Size = new System.Drawing.Size(79, 17);
            this.chkSaveTrace.TabIndex = 2;
            this.chkSaveTrace.Text = "Save to file";
            this.chkSaveTrace.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label14);
            this.groupBox4.Controls.Add(this.numFlowTimeout);
            this.groupBox4.Controls.Add(this.chkAutoValveControl);
            this.groupBox4.Controls.Add(this.chkHeValve);
            this.groupBox4.Controls.Add(this.chkSF6Valve);
            this.groupBox4.Controls.Add(this.chkAutoFlowControl);
            this.groupBox4.Controls.Add(this.chkAO1Enable);
            this.groupBox4.Controls.Add(this.chkAO0Enable);
            this.groupBox4.Controls.Add(this.lblAO1);
            this.groupBox4.Controls.Add(this.lblAO0);
            this.groupBox4.Controls.Add(this.numAO1);
            this.groupBox4.Controls.Add(this.numAO0);
            this.groupBox4.Controls.Add(this.label10);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Controls.Add(this.lblheflow);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.lblsf6flow);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.Location = new System.Drawing.Point(3, 363);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(675, 192);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Flow Controllers";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(10, 171);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(73, 13);
            this.label14.TabIndex = 19;
            this.label14.Text = "Timeout (ms) :";
            // 
            // numFlowTimeout
            // 
            this.numFlowTimeout.Location = new System.Drawing.Point(89, 169);
            this.numFlowTimeout.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numFlowTimeout.Minimum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numFlowTimeout.Name = "numFlowTimeout";
            this.numFlowTimeout.Size = new System.Drawing.Size(58, 20);
            this.numFlowTimeout.TabIndex = 18;
            this.numFlowTimeout.Value = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.numFlowTimeout.ValueChanged += new System.EventHandler(this.numFlowTimeout_ValueChanged);
            // 
            // chkAutoValveControl
            // 
            this.chkAutoValveControl.AutoSize = true;
            this.chkAutoValveControl.Location = new System.Drawing.Point(160, 148);
            this.chkAutoValveControl.Name = "chkAutoValveControl";
            this.chkAutoValveControl.Size = new System.Drawing.Size(139, 17);
            this.chkAutoValveControl.TabIndex = 17;
            this.chkAutoValveControl.Text = "Automatic Valve Control";
            this.chkAutoValveControl.UseVisualStyleBackColor = true;
            // 
            // chkHeValve
            // 
            this.chkHeValve.AutoSize = true;
            this.chkHeValve.Location = new System.Drawing.Point(321, 112);
            this.chkHeValve.Name = "chkHeValve";
            this.chkHeValve.Size = new System.Drawing.Size(82, 17);
            this.chkHeValve.TabIndex = 16;
            this.chkHeValve.Text = "Valve Open";
            this.chkHeValve.UseVisualStyleBackColor = true;
            this.chkHeValve.CheckedChanged += new System.EventHandler(this.chkHeValve_CheckedChanged);
            // 
            // chkSF6Valve
            // 
            this.chkSF6Valve.AutoSize = true;
            this.chkSF6Valve.Location = new System.Drawing.Point(83, 111);
            this.chkSF6Valve.Name = "chkSF6Valve";
            this.chkSF6Valve.Size = new System.Drawing.Size(82, 17);
            this.chkSF6Valve.TabIndex = 15;
            this.chkSF6Valve.Text = "Valve Open";
            this.chkSF6Valve.UseVisualStyleBackColor = true;
            this.chkSF6Valve.CheckedChanged += new System.EventHandler(this.chkSF6Valve_CheckedChanged);
            // 
            // chkAutoFlowControl
            // 
            this.chkAutoFlowControl.AutoSize = true;
            this.chkAutoFlowControl.Location = new System.Drawing.Point(10, 148);
            this.chkAutoFlowControl.Name = "chkAutoFlowControl";
            this.chkAutoFlowControl.Size = new System.Drawing.Size(134, 17);
            this.chkAutoFlowControl.TabIndex = 14;
            this.chkAutoFlowControl.Text = "Automatic Flow Control";
            this.chkAutoFlowControl.UseVisualStyleBackColor = true;
            // 
            // chkAO1Enable
            // 
            this.chkAO1Enable.AutoSize = true;
            this.chkAO1Enable.Location = new System.Drawing.Point(248, 112);
            this.chkAO1Enable.Name = "chkAO1Enable";
            this.chkAO1Enable.Size = new System.Drawing.Size(67, 17);
            this.chkAO1Enable.TabIndex = 13;
            this.chkAO1Enable.Text = "Flow ON";
            this.chkAO1Enable.UseVisualStyleBackColor = true;
            
            // 
            // chkAO0Enable
            // 
            this.chkAO0Enable.AutoSize = true;
            this.chkAO0Enable.Location = new System.Drawing.Point(10, 111);
            this.chkAO0Enable.Name = "chkAO0Enable";
            this.chkAO0Enable.Size = new System.Drawing.Size(67, 17);
            this.chkAO0Enable.TabIndex = 12;
            this.chkAO0Enable.Text = "Flow ON";
            this.chkAO0Enable.UseVisualStyleBackColor = true;
            
            // 
            // lblAO1
            // 
            this.lblAO1.AutoSize = true;
            this.lblAO1.Location = new System.Drawing.Point(335, 87);
            this.lblAO1.Name = "lblAO1";
            this.lblAO1.Size = new System.Drawing.Size(50, 13);
            this.lblAO1.TabIndex = 11;
            this.lblAO1.Text = "( 0.00 V )";
            // 
            // lblAO0
            // 
            this.lblAO0.AutoSize = true;
            this.lblAO0.Location = new System.Drawing.Point(94, 88);
            this.lblAO0.Name = "lblAO0";
            this.lblAO0.Size = new System.Drawing.Size(53, 13);
            this.lblAO0.TabIndex = 10;
            this.lblAO0.Text = "( 0. 00 V )";
            // 
            // numAO1
            // 
            this.numAO1.DecimalPlaces = 2;
            this.numAO1.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numAO1.Location = new System.Drawing.Point(248, 84);
            this.numAO1.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numAO1.Name = "numAO1";
            this.numAO1.Size = new System.Drawing.Size(77, 20);
            this.numAO1.TabIndex = 9;
            
            // 
            // numAO0
            // 
            this.numAO0.DecimalPlaces = 2;
            this.numAO0.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numAO0.Location = new System.Drawing.Point(10, 84);
            this.numAO0.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numAO0.Name = "numAO0";
            this.numAO0.Size = new System.Drawing.Size(77, 20);
            this.numAO0.TabIndex = 8;
            
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(245, 67);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(128, 13);
            this.label10.TabIndex = 7;
            this.label10.Text = "He flow set point in sccm:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 67);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(133, 13);
            this.label9.TabIndex = 6;
            this.label9.Text = "SF6 flow set point in sccm:";
            // 
            // lblheflow
            // 
            this.lblheflow.AutoSize = true;
            this.lblheflow.Location = new System.Drawing.Point(300, 27);
            this.lblheflow.Name = "lblheflow";
            this.lblheflow.Size = new System.Drawing.Size(62, 13);
            this.lblheflow.TabIndex = 5;
            this.lblheflow.Text = "0.000 sccm";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(245, 27);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "He Flow:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 27);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "SF6 Flow:";
            // 
            // lblsf6flow
            // 
            this.lblsf6flow.AutoSize = true;
            this.lblsf6flow.Location = new System.Drawing.Point(67, 27);
            this.lblsf6flow.Name = "lblsf6flow";
            this.lblsf6flow.Size = new System.Drawing.Size(62, 13);
            this.lblsf6flow.TabIndex = 4;
            this.lblsf6flow.Text = "0.000 sccm";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 98.79699F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1.203007F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel6, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 77.16049F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22.83951F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(702, 810);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // SourceTabView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SourceTabView";
            this.Size = new System.Drawing.Size(702, 810);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tempGraph)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFlowTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAO1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAO0)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Button readButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkAutoScale;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbSamplingRate;
        private System.Windows.Forms.CheckBox chkToF;
        private NationalInstruments.UI.WindowsForms.ScatterGraph tempGraph;
        private ScatterPlot scatterPlot1;
        private XAxis xAxis1;
        private YAxis yAxis1;
        private System.Windows.Forms.CheckBox chkSaveTrace;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown numFlowTimeout;
        private System.Windows.Forms.CheckBox chkAutoValveControl;
        private System.Windows.Forms.CheckBox chkHeValve;
        private System.Windows.Forms.CheckBox chkSF6Valve;
        private System.Windows.Forms.CheckBox chkAutoFlowControl;
        private System.Windows.Forms.CheckBox chkAO1Enable;
        private System.Windows.Forms.CheckBox chkAO0Enable;
        private System.Windows.Forms.Label lblAO1;
        private System.Windows.Forms.Label lblAO0;
        private System.Windows.Forms.NumericUpDown numAO1;
        private System.Windows.Forms.NumericUpDown numAO0;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblheflow;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblsf6flow;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
