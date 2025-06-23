using NationalInstruments.UI;

namespace CaFBECHardwareController.Controls
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
            this.scatterPlot2 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis2 = new NationalInstruments.UI.XAxis();
            this.yAxis2 = new NationalInstruments.UI.YAxis();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.graphAbs = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot3 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis3 = new NationalInstruments.UI.XAxis();
            this.yAxis3 = new NationalInstruments.UI.YAxis();
            this.chkAutoScale = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbSamplingRate = new System.Windows.Forms.ComboBox();
            this.chkToF = new System.Windows.Forms.CheckBox();
            this.graphPMT = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot1 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.chkSaveTrace = new System.Windows.Forms.CheckBox();
            this.readButton = new System.Windows.Forms.Button();
            this.chkAutoScaleAbs = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbSamplingRateAbs = new System.Windows.Forms.ComboBox();
            this.chkToFAbs = new System.Windows.Forms.CheckBox();
            this.chkSaveTraceAbs = new System.Windows.Forms.CheckBox();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.graphAbs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.graphPMT)).BeginInit();
            this.SuspendLayout();
            // 
            // scatterPlot2
            // 
            this.scatterPlot2.XAxis = this.xAxis2;
            this.scatterPlot2.YAxis = this.yAxis2;
            // 
            // xAxis2
            // 
            this.xAxis2.Caption = "Time (ms)";
            // 
            // yAxis2
            // 
            this.yAxis2.Caption = "Voltage (V)";
            this.yAxis2.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.yAxis2.Range = new NationalInstruments.UI.Range(0D, 1D);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chkAutoScaleAbs);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.cmbSamplingRateAbs);
            this.groupBox3.Controls.Add(this.chkToFAbs);
            this.groupBox3.Controls.Add(this.chkSaveTraceAbs);
            this.groupBox3.Controls.Add(this.graphAbs);
            this.groupBox3.Controls.Add(this.chkAutoScale);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.cmbSamplingRate);
            this.groupBox3.Controls.Add(this.chkToF);
            this.groupBox3.Controls.Add(this.graphPMT);
            this.groupBox3.Controls.Add(this.chkSaveTrace);
            this.groupBox3.Location = new System.Drawing.Point(3, 7);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(681, 692);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Time of flight";
            // 
            // graphAbs
            // 
            this.graphAbs.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.graphAbs.Location = new System.Drawing.Point(6, 349);
            this.graphAbs.Name = "graphAbs";
            this.graphAbs.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot3});
            this.graphAbs.Size = new System.Drawing.Size(669, 301);
            this.graphAbs.TabIndex = 9;
            this.graphAbs.UseColorGenerator = true;
            this.graphAbs.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis3});
            this.graphAbs.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis3});
            // 
            // scatterPlot3
            // 
            this.scatterPlot3.XAxis = this.xAxis3;
            this.scatterPlot3.YAxis = this.yAxis3;
            // 
            // xAxis3
            // 
            this.xAxis3.Caption = "Time (ms)";
            this.xAxis3.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.xAxis3.Range = new NationalInstruments.UI.Range(0D, 1D);
            // 
            // yAxis3
            // 
            this.yAxis3.Caption = "Absorption Photodiode Voltage (V)";
            this.yAxis3.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.yAxis3.Range = new NationalInstruments.UI.Range(0D, 1D);
            // 
            // chkAutoScale
            // 
            this.chkAutoScale.AutoSize = true;
            this.chkAutoScale.Location = new System.Drawing.Point(173, 325);
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
            this.label5.Location = new System.Drawing.Point(250, 327);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 13);
            this.label5.TabIndex = 5;
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
            this.cmbSamplingRate.Location = new System.Drawing.Point(352, 324);
            this.cmbSamplingRate.Name = "cmbSamplingRate";
            this.cmbSamplingRate.Size = new System.Drawing.Size(72, 21);
            this.cmbSamplingRate.TabIndex = 4;
            this.cmbSamplingRate.Text = "80000";
            this.cmbSamplingRate.SelectedIndexChanged += new System.EventHandler(this.samplingRateSelect);
            // 
            // chkToF
            // 
            this.chkToF.AutoSize = true;
            this.chkToF.Location = new System.Drawing.Point(6, 325);
            this.chkToF.Name = "chkToF";
            this.chkToF.Size = new System.Drawing.Size(80, 17);
            this.chkToF.TabIndex = 3;
            this.chkToF.Text = "Show trace";
            this.chkToF.UseVisualStyleBackColor = true;
            this.chkToF.CheckedChanged += new System.EventHandler(this.chkToF_CheckedChanged);
            // 
            // graphPMT
            // 
            this.graphPMT.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.graphPMT.Location = new System.Drawing.Point(6, 19);
            this.graphPMT.Name = "graphPMT";
            this.graphPMT.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot1});
            this.graphPMT.Size = new System.Drawing.Size(669, 301);
            this.graphPMT.TabIndex = 0;
            this.graphPMT.UseColorGenerator = true;
            this.graphPMT.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.graphPMT.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
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
            this.yAxis1.Caption = "PMT Voltage (V)";
            this.yAxis1.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.yAxis1.Range = new NationalInstruments.UI.Range(0D, 1D);
            // 
            // chkSaveTrace
            // 
            this.chkSaveTrace.AutoSize = true;
            this.chkSaveTrace.Location = new System.Drawing.Point(92, 325);
            this.chkSaveTrace.Name = "chkSaveTrace";
            this.chkSaveTrace.Size = new System.Drawing.Size(79, 17);
            this.chkSaveTrace.TabIndex = 2;
            this.chkSaveTrace.Text = "Save to file";
            this.chkSaveTrace.UseVisualStyleBackColor = true;
            // 
            // readButton
            // 
            this.readButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.readButton.Location = new System.Drawing.Point(308, 722);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(99, 24);
            this.readButton.TabIndex = 8;
            this.readButton.Text = "Start Reading";
            this.readButton.UseVisualStyleBackColor = true;
            this.readButton.Click += new System.EventHandler(this.toggleReading);
            // 
            // chkAutoScaleAbs
            // 
            this.chkAutoScaleAbs.AutoSize = true;
            this.chkAutoScaleAbs.Location = new System.Drawing.Point(173, 657);
            this.chkAutoScaleAbs.Name = "chkAutoScaleAbs";
            this.chkAutoScaleAbs.Size = new System.Drawing.Size(78, 17);
            this.chkAutoScaleAbs.TabIndex = 14;
            this.chkAutoScaleAbs.Text = "Auto Scale";
            this.chkAutoScaleAbs.UseVisualStyleBackColor = true;
            this.chkAutoScaleAbs.CheckedChanged += new System.EventHandler(this.chkAutoScaleAbs_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(250, 659);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Sampling rate (Hz):";
            // 
            // cmbSamplingRateAbs
            // 
            this.cmbSamplingRateAbs.FormattingEnabled = true;
            this.cmbSamplingRateAbs.Items.AddRange(new object[] {
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
            this.cmbSamplingRateAbs.Location = new System.Drawing.Point(352, 656);
            this.cmbSamplingRateAbs.Name = "cmbSamplingRateAbs";
            this.cmbSamplingRateAbs.Size = new System.Drawing.Size(72, 21);
            this.cmbSamplingRateAbs.TabIndex = 12;
            this.cmbSamplingRateAbs.Text = "250000";
            this.cmbSamplingRateAbs.SelectedIndexChanged += new System.EventHandler(this.samplingRateAbsSelect);
            // 
            // chkToFAbs
            // 
            this.chkToFAbs.AutoSize = true;
            this.chkToFAbs.Location = new System.Drawing.Point(6, 657);
            this.chkToFAbs.Name = "chkToFAbs";
            this.chkToFAbs.Size = new System.Drawing.Size(80, 17);
            this.chkToFAbs.TabIndex = 11;
            this.chkToFAbs.Text = "Show trace";
            this.chkToFAbs.UseVisualStyleBackColor = true;
            this.chkToFAbs.CheckedChanged += new System.EventHandler(this.chkToFAbs_CheckedChanged);
            // 
            // chkSaveTraceAbs
            // 
            this.chkSaveTraceAbs.AutoSize = true;
            this.chkSaveTraceAbs.Location = new System.Drawing.Point(92, 657);
            this.chkSaveTraceAbs.Name = "chkSaveTraceAbs";
            this.chkSaveTraceAbs.Size = new System.Drawing.Size(79, 17);
            this.chkSaveTraceAbs.TabIndex = 10;
            this.chkSaveTraceAbs.Text = "Save to file";
            this.chkSaveTraceAbs.UseVisualStyleBackColor = true;
            // 
            // SourceTabView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.readButton);
            this.Controls.Add(this.groupBox3);
            this.Name = "SourceTabView";
            this.Size = new System.Drawing.Size(702, 810);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.graphAbs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.graphPMT)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private ScatterPlot scatterPlot2;
        private XAxis xAxis2;
        private YAxis yAxis2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkAutoScale;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbSamplingRate;
        private System.Windows.Forms.CheckBox chkToF;
        private NationalInstruments.UI.WindowsForms.ScatterGraph graphPMT;
        private ScatterPlot scatterPlot1;
        private XAxis xAxis1;
        private YAxis yAxis1;
        private System.Windows.Forms.CheckBox chkSaveTrace;
        private System.Windows.Forms.Button readButton;
        private NationalInstruments.UI.WindowsForms.ScatterGraph graphAbs;
        private ScatterPlot scatterPlot3;
        private XAxis xAxis3;
        private YAxis yAxis3;
        private System.Windows.Forms.CheckBox chkAutoScaleAbs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbSamplingRateAbs;
        private System.Windows.Forms.CheckBox chkToFAbs;
        private System.Windows.Forms.CheckBox chkSaveTraceAbs;
    }
}
