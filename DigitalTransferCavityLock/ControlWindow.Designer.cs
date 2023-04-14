
namespace DigitalTransferCavityLock
{
    partial class ControlWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlWindow));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.GUIDisable = new System.Windows.Forms.CheckBox();
            this.LockRate = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.StartRamp = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.RampAmplitude = new System.Windows.Forms.TextBox();
            this.RampOffset = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.RampFreq = new System.Windows.Forms.TextBox();
            this.labelrampfreq = new System.Windows.Forms.Label();
            this.CavityTabs = new System.Windows.Forms.TabControl();
            this.PeakPlot = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.rampPlot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.refPeak = new NationalInstruments.UI.ScatterPlot();
            this.slavePeak = new NationalInstruments.UI.ScatterPlot();
            this.groupBox1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PeakPlot)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.GUIDisable);
            this.groupBox1.Controls.Add(this.LockRate);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(288, 70);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "DAQ Control";
            // 
            // GUIDisable
            // 
            this.GUIDisable.AutoSize = true;
            this.GUIDisable.Location = new System.Drawing.Point(9, 44);
            this.GUIDisable.Name = "GUIDisable";
            this.GUIDisable.Size = new System.Drawing.Size(141, 17);
            this.GUIDisable.TabIndex = 1;
            this.GUIDisable.Text = "Limit GUI Update to 1Hz";
            this.GUIDisable.UseVisualStyleBackColor = true;
            // 
            // LockRate
            // 
            this.LockRate.Enabled = false;
            this.LockRate.Location = new System.Drawing.Point(182, 13);
            this.LockRate.Name = "LockRate";
            this.LockRate.Size = new System.Drawing.Size(100, 20);
            this.LockRate.TabIndex = 0;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 16);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(74, 13);
            this.label8.TabIndex = 17;
            this.label8.Text = "Lock rate [Hz]";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.StartRamp);
            this.groupBox4.Controls.Add(this.label11);
            this.groupBox4.Controls.Add(this.RampAmplitude);
            this.groupBox4.Controls.Add(this.RampOffset);
            this.groupBox4.Controls.Add(this.label12);
            this.groupBox4.Controls.Add(this.RampFreq);
            this.groupBox4.Controls.Add(this.labelrampfreq);
            this.groupBox4.Location = new System.Drawing.Point(13, 90);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(288, 111);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Ramp Control";
            // 
            // StartRamp
            // 
            this.StartRamp.AutoSize = true;
            this.StartRamp.Location = new System.Drawing.Point(10, 89);
            this.StartRamp.Name = "StartRamp";
            this.StartRamp.Size = new System.Drawing.Size(58, 17);
            this.StartRamp.TabIndex = 5;
            this.StartRamp.Text = "Output";
            this.StartRamp.UseVisualStyleBackColor = true;
            this.StartRamp.CheckedChanged += new System.EventHandler(this.StartRamp_CheckedChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(7, 60);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(100, 13);
            this.label11.TabIndex = 22;
            this.label11.Text = "Ramp Amplitude [V]";
            // 
            // RampAmplitude
            // 
            this.RampAmplitude.Location = new System.Drawing.Point(182, 58);
            this.RampAmplitude.Name = "RampAmplitude";
            this.RampAmplitude.Size = new System.Drawing.Size(100, 20);
            this.RampAmplitude.TabIndex = 4;
            this.RampAmplitude.Text = "0";
            this.RampAmplitude.TextChanged += new System.EventHandler(this.RampAmplitude_TextChanged);
            // 
            // RampOffset
            // 
            this.RampOffset.Location = new System.Drawing.Point(182, 35);
            this.RampOffset.Name = "RampOffset";
            this.RampOffset.Size = new System.Drawing.Size(100, 20);
            this.RampOffset.TabIndex = 3;
            this.RampOffset.Text = "0";
            this.RampOffset.TextChanged += new System.EventHandler(this.RampOffset_TextChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 38);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(82, 13);
            this.label12.TabIndex = 18;
            this.label12.Text = "Ramp Offset [V]";
            // 
            // RampFreq
            // 
            this.RampFreq.Location = new System.Drawing.Point(182, 13);
            this.RampFreq.Name = "RampFreq";
            this.RampFreq.Size = new System.Drawing.Size(100, 20);
            this.RampFreq.TabIndex = 2;
            this.RampFreq.Text = "200";
            this.RampFreq.TextChanged += new System.EventHandler(this.RampFreq_TextChanged);
            this.RampFreq.Leave += new System.EventHandler(this.RampFreq_Leave);
            // 
            // labelrampfreq
            // 
            this.labelrampfreq.AutoSize = true;
            this.labelrampfreq.Location = new System.Drawing.Point(7, 16);
            this.labelrampfreq.Name = "labelrampfreq";
            this.labelrampfreq.Size = new System.Drawing.Size(110, 13);
            this.labelrampfreq.TabIndex = 17;
            this.labelrampfreq.Text = "Ramp Frequency [Hz]";
            // 
            // CavityTabs
            // 
            this.CavityTabs.Location = new System.Drawing.Point(13, 326);
            this.CavityTabs.Name = "CavityTabs";
            this.CavityTabs.SelectedIndex = 0;
            this.CavityTabs.Size = new System.Drawing.Size(288, 584);
            this.CavityTabs.TabIndex = 6;
            // 
            // PeakPlot
            // 
            this.PeakPlot.Location = new System.Drawing.Point(13, 207);
            this.PeakPlot.Name = "PeakPlot";
            this.PeakPlot.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.rampPlot,
            this.refPeak,
            this.slavePeak});
            this.PeakPlot.Size = new System.Drawing.Size(288, 113);
            this.PeakPlot.TabIndex = 7;
            this.PeakPlot.UseColorGenerator = true;
            this.PeakPlot.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.PeakPlot.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // rampPlot
            // 
            this.rampPlot.HistoryCapacity = 10000;
            this.rampPlot.XAxis = this.xAxis1;
            this.rampPlot.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.AutoMinorDivisionFrequency = 1000;
            this.xAxis1.MinorDivisions.Interval = 0.001D;
            this.xAxis1.Mode = NationalInstruments.UI.AxisMode.AutoScaleExact;
            // 
            // yAxis1
            // 
            this.yAxis1.Mode = NationalInstruments.UI.AxisMode.AutoScaleExact;
            // 
            // refPeak
            // 
            this.refPeak.XAxis = this.xAxis1;
            this.refPeak.YAxis = this.yAxis1;
            // 
            // slavePeak
            // 
            this.slavePeak.XAxis = this.xAxis1;
            this.slavePeak.YAxis = this.yAxis1;
            // 
            // ControlWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(313, 922);
            this.Controls.Add(this.PeakPlot);
            this.Controls.Add(this.CavityTabs);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ControlWindow";
            this.Text = "Digital Transfer Cavity Lock";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ControlWindow_FormClosing);
            this.Load += new System.EventHandler(this.ControlWindow_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PeakPlot)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.TextBox LockRate;
        public System.Windows.Forms.CheckBox GUIDisable;
        private System.Windows.Forms.GroupBox groupBox4;
        public System.Windows.Forms.CheckBox StartRamp;
        private System.Windows.Forms.Label label11;
        public System.Windows.Forms.TextBox RampAmplitude;
        public System.Windows.Forms.TextBox RampOffset;
        private System.Windows.Forms.Label label12;
        public System.Windows.Forms.TextBox RampFreq;
        private System.Windows.Forms.Label labelrampfreq;
        public System.Windows.Forms.TabControl CavityTabs;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        public NationalInstruments.UI.ScatterPlot refPeak;
        public NationalInstruments.UI.ScatterPlot slavePeak;
        public NationalInstruments.UI.WindowsForms.ScatterGraph PeakPlot;
        public NationalInstruments.UI.ScatterPlot rampPlot;
    }
}

