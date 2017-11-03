namespace PaddlePolStabiliser
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
            this.comboDetectorDevice = new System.Windows.Forms.ComboBox();
            this.combDetectorChannel = new System.Windows.Forms.ComboBox();
            this.combController = new System.Windows.Forms.ComboBox();
            this.labDetector = new System.Windows.Forms.Label();
            this.labController = new System.Windows.Forms.Label();
            this.butRun = new System.Windows.Forms.Button();
            this.labStatus = new System.Windows.Forms.Label();
            this.waveformGraph1 = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.waveformPlotDetector = new NationalInstruments.UI.WaveformPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.slideLockPoint = new NationalInstruments.UI.WindowsForms.Slide();
            this.labLockPoint = new System.Windows.Forms.Label();
            this.checkLock = new System.Windows.Forms.CheckBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.butStop = new System.Windows.Forms.Button();
            this.waveformPlotLock = new NationalInstruments.UI.WaveformPlot();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.textGain = new System.Windows.Forms.TextBox();
            this.labGain = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.waveformGraph1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.slideLockPoint)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboDetectorDevice
            // 
            this.comboDetectorDevice.FormattingEnabled = true;
            this.comboDetectorDevice.Location = new System.Drawing.Point(108, 13);
            this.comboDetectorDevice.Name = "comboDetectorDevice";
            this.comboDetectorDevice.Size = new System.Drawing.Size(121, 21);
            this.comboDetectorDevice.TabIndex = 0;
            this.comboDetectorDevice.SelectedIndexChanged += new System.EventHandler(this.comboDetectorDevice_SelectedIndexChanged);
            // 
            // combDetectorChannel
            // 
            this.combDetectorChannel.FormattingEnabled = true;
            this.combDetectorChannel.Location = new System.Drawing.Point(236, 13);
            this.combDetectorChannel.Name = "combDetectorChannel";
            this.combDetectorChannel.Size = new System.Drawing.Size(121, 21);
            this.combDetectorChannel.TabIndex = 1;
            this.combDetectorChannel.SelectedIndexChanged += new System.EventHandler(this.combDetectorChannel_SelectedIndexChanged);
            // 
            // combController
            // 
            this.combController.FormattingEnabled = true;
            this.combController.Location = new System.Drawing.Point(108, 41);
            this.combController.Name = "combController";
            this.combController.Size = new System.Drawing.Size(121, 21);
            this.combController.TabIndex = 2;
            this.combController.SelectedIndexChanged += new System.EventHandler(this.combController_SelectedIndexChanged);
            // 
            // labDetector
            // 
            this.labDetector.AutoSize = true;
            this.labDetector.Location = new System.Drawing.Point(13, 13);
            this.labDetector.Name = "labDetector";
            this.labDetector.Size = new System.Drawing.Size(48, 13);
            this.labDetector.TabIndex = 3;
            this.labDetector.Text = "Detector";
            // 
            // labController
            // 
            this.labController.AutoSize = true;
            this.labController.Location = new System.Drawing.Point(13, 41);
            this.labController.Name = "labController";
            this.labController.Size = new System.Drawing.Size(51, 13);
            this.labController.TabIndex = 4;
            this.labController.Text = "Controller";
            // 
            // butRun
            // 
            this.butRun.Location = new System.Drawing.Point(16, 254);
            this.butRun.Name = "butRun";
            this.butRun.Size = new System.Drawing.Size(75, 23);
            this.butRun.TabIndex = 5;
            this.butRun.Text = "Run";
            this.butRun.UseVisualStyleBackColor = true;
            this.butRun.Click += new System.EventHandler(this.butRun_Click);
            // 
            // labStatus
            // 
            this.labStatus.AutoSize = true;
            this.labStatus.Location = new System.Drawing.Point(279, 259);
            this.labStatus.Name = "labStatus";
            this.labStatus.Size = new System.Drawing.Size(35, 13);
            this.labStatus.TabIndex = 6;
            this.labStatus.Text = "Error: ";
            // 
            // waveformGraph1
            // 
            this.waveformGraph1.Location = new System.Drawing.Point(13, 71);
            this.waveformGraph1.Name = "waveformGraph1";
            this.waveformGraph1.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.waveformPlotDetector,
            this.waveformPlotLock});
            this.waveformGraph1.Size = new System.Drawing.Size(272, 168);
            this.waveformGraph1.TabIndex = 7;
            this.waveformGraph1.UseColorGenerator = true;
            this.waveformGraph1.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.waveformGraph1.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // waveformPlotDetector
            // 
            this.waveformPlotDetector.XAxis = this.xAxis1;
            this.waveformPlotDetector.YAxis = this.yAxis1;
            // 
            // slideLockPoint
            // 
            this.slideLockPoint.Location = new System.Drawing.Point(291, 69);
            this.slideLockPoint.Name = "slideLockPoint";
            this.slideLockPoint.Size = new System.Drawing.Size(54, 156);
            this.slideLockPoint.TabIndex = 8;
            this.slideLockPoint.AfterChangeValue += new NationalInstruments.UI.AfterChangeNumericValueEventHandler(this.slideLockPoint_AfterChangeValue);
            // 
            // labLockPoint
            // 
            this.labLockPoint.AutoSize = true;
            this.labLockPoint.Location = new System.Drawing.Point(299, 226);
            this.labLockPoint.Name = "labLockPoint";
            this.labLockPoint.Size = new System.Drawing.Size(58, 13);
            this.labLockPoint.TabIndex = 9;
            this.labLockPoint.Text = "Lock Point";
            // 
            // checkLock
            // 
            this.checkLock.AutoSize = true;
            this.checkLock.Location = new System.Drawing.Point(111, 258);
            this.checkLock.Name = "checkLock";
            this.checkLock.Size = new System.Drawing.Size(90, 17);
            this.checkLock.TabIndex = 10;
            this.checkLock.Text = "Engage Lock";
            this.checkLock.UseVisualStyleBackColor = true;
            this.checkLock.CheckedChanged += new System.EventHandler(this.checkLock_CheckedChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 319);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(451, 22);
            this.statusStrip1.TabIndex = 11;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // butStop
            // 
            this.butStop.ForeColor = System.Drawing.Color.Red;
            this.butStop.Location = new System.Drawing.Point(16, 283);
            this.butStop.Name = "butStop";
            this.butStop.Size = new System.Drawing.Size(75, 23);
            this.butStop.TabIndex = 12;
            this.butStop.Text = "STOP";
            this.butStop.UseVisualStyleBackColor = true;
            this.butStop.Click += new System.EventHandler(this.butStop_Click);
            // 
            // waveformPlotLock
            // 
            this.waveformPlotLock.XAxis = this.xAxis1;
            this.waveformPlotLock.YAxis = this.yAxis1;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(39, 17);
            this.toolStripStatusLabel1.Text = "Status";
            this.toolStripStatusLabel1.Click += new System.EventHandler(this.toolStripStatusLabel1_Click);
            // 
            // textGain
            // 
            this.textGain.Location = new System.Drawing.Point(351, 84);
            this.textGain.Name = "textGain";
            this.textGain.Size = new System.Drawing.Size(56, 20);
            this.textGain.TabIndex = 13;
            this.textGain.TextChanged += new System.EventHandler(this.textGain_TextChanged);
            // 
            // labGain
            // 
            this.labGain.AutoSize = true;
            this.labGain.Location = new System.Drawing.Point(352, 65);
            this.labGain.Name = "labGain";
            this.labGain.Size = new System.Drawing.Size(29, 13);
            this.labGain.TabIndex = 14;
            this.labGain.Text = "Gain";
            this.labGain.Click += new System.EventHandler(this.label1_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(451, 341);
            this.Controls.Add(this.labGain);
            this.Controls.Add(this.textGain);
            this.Controls.Add(this.butStop);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.checkLock);
            this.Controls.Add(this.labLockPoint);
            this.Controls.Add(this.slideLockPoint);
            this.Controls.Add(this.waveformGraph1);
            this.Controls.Add(this.labStatus);
            this.Controls.Add(this.butRun);
            this.Controls.Add(this.labController);
            this.Controls.Add(this.labDetector);
            this.Controls.Add(this.combController);
            this.Controls.Add(this.combDetectorChannel);
            this.Controls.Add(this.comboDetectorDevice);
            this.Name = "MainForm";
            this.Text = "Paddle Polarisation Stabiliser";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.waveformGraph1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.slideLockPoint)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboDetectorDevice;
        private System.Windows.Forms.ComboBox combDetectorChannel;
        private System.Windows.Forms.ComboBox combController;
        private System.Windows.Forms.Label labDetector;
        private System.Windows.Forms.Label labController;
        private System.Windows.Forms.Button butRun;
        private System.Windows.Forms.Label labStatus;
        private NationalInstruments.UI.WindowsForms.WaveformGraph waveformGraph1;
        private NationalInstruments.UI.WaveformPlot waveformPlotDetector;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private NationalInstruments.UI.WindowsForms.Slide slideLockPoint;
        private System.Windows.Forms.Label labLockPoint;
        private System.Windows.Forms.CheckBox checkLock;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Button butStop;
        private NationalInstruments.UI.WaveformPlot waveformPlotLock;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TextBox textGain;
        private System.Windows.Forms.Label labGain;
    }
}

