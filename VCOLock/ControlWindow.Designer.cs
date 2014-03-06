namespace VCOLock
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
            this.freqCounterTextBox = new System.Windows.Forms.TextBox();
            this.counterFreqUpdateButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.freqSetpointTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.errorSigGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.errorSigPlot = new NationalInstruments.UI.WaveformPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.pollPeriodTextBox = new System.Windows.Forms.TextBox();
            this.stopPollButton = new System.Windows.Forms.Button();
            this.label80 = new System.Windows.Forms.Label();
            this.startPollButton = new System.Windows.Forms.Button();
            this.intLockEnable = new System.Windows.Forms.CheckBox();
            this.propGainTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.intGainTextBox = new System.Windows.Forms.TextBox();
            this.propLockEnable = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.errorSigGraph)).BeginInit();
            this.SuspendLayout();
            // 
            // freqCounterTextBox
            // 
            this.freqCounterTextBox.BackColor = System.Drawing.Color.Black;
            this.freqCounterTextBox.ForeColor = System.Drawing.Color.Chartreuse;
            this.freqCounterTextBox.Location = new System.Drawing.Point(114, 159);
            this.freqCounterTextBox.Name = "freqCounterTextBox";
            this.freqCounterTextBox.ReadOnly = true;
            this.freqCounterTextBox.Size = new System.Drawing.Size(113, 20);
            this.freqCounterTextBox.TabIndex = 47;
            this.freqCounterTextBox.Text = "0";
            // 
            // counterFreqUpdateButton
            // 
            this.counterFreqUpdateButton.Location = new System.Drawing.Point(274, 157);
            this.counterFreqUpdateButton.Name = "counterFreqUpdateButton";
            this.counterFreqUpdateButton.Size = new System.Drawing.Size(75, 23);
            this.counterFreqUpdateButton.TabIndex = 46;
            this.counterFreqUpdateButton.Text = "Update";
            this.counterFreqUpdateButton.UseVisualStyleBackColor = true;
            this.counterFreqUpdateButton.Click += new System.EventHandler(this.counterFreqUpdateButton_Click);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(233, 162);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 23);
            this.label4.TabIndex = 45;
            this.label4.Text = "MHz";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(15, 162);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 23);
            this.label3.TabIndex = 44;
            this.label3.Text = "Counter frequency";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(15, 185);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 23);
            this.label1.TabIndex = 48;
            this.label1.Text = "Set point";
            // 
            // freqSetpointTextBox
            // 
            this.freqSetpointTextBox.Location = new System.Drawing.Point(114, 182);
            this.freqSetpointTextBox.Name = "freqSetpointTextBox";
            this.freqSetpointTextBox.Size = new System.Drawing.Size(113, 20);
            this.freqSetpointTextBox.TabIndex = 49;
            this.freqSetpointTextBox.Text = "14000";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(233, 185);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 23);
            this.label2.TabIndex = 50;
            this.label2.Text = "MHz";
            // 
            // errorSigGraph
            // 
            this.errorSigGraph.Location = new System.Drawing.Point(12, 12);
            this.errorSigGraph.Name = "errorSigGraph";
            this.errorSigGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.errorSigPlot});
            this.errorSigGraph.Size = new System.Drawing.Size(336, 117);
            this.errorSigGraph.TabIndex = 52;
            this.errorSigGraph.UseColorGenerator = true;
            this.errorSigGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.errorSigGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // errorSigPlot
            // 
            this.errorSigPlot.XAxis = this.xAxis1;
            this.errorSigPlot.YAxis = this.yAxis1;
            // 
            // pollPeriodTextBox
            // 
            this.pollPeriodTextBox.Location = new System.Drawing.Point(114, 136);
            this.pollPeriodTextBox.Name = "pollPeriodTextBox";
            this.pollPeriodTextBox.Size = new System.Drawing.Size(73, 20);
            this.pollPeriodTextBox.TabIndex = 68;
            this.pollPeriodTextBox.Text = "50";
            // 
            // stopPollButton
            // 
            this.stopPollButton.Enabled = false;
            this.stopPollButton.Location = new System.Drawing.Point(274, 134);
            this.stopPollButton.Name = "stopPollButton";
            this.stopPollButton.Size = new System.Drawing.Size(75, 23);
            this.stopPollButton.TabIndex = 70;
            this.stopPollButton.Text = "Stop poll";
            this.stopPollButton.UseVisualStyleBackColor = true;
            this.stopPollButton.Click += new System.EventHandler(this.stopPollButton_Click);
            // 
            // label80
            // 
            this.label80.Location = new System.Drawing.Point(15, 139);
            this.label80.Name = "label80";
            this.label80.Size = new System.Drawing.Size(101, 23);
            this.label80.TabIndex = 67;
            this.label80.Text = "Poll period (ms)";
            // 
            // startPollButton
            // 
            this.startPollButton.Location = new System.Drawing.Point(193, 134);
            this.startPollButton.Name = "startPollButton";
            this.startPollButton.Size = new System.Drawing.Size(75, 23);
            this.startPollButton.TabIndex = 69;
            this.startPollButton.Text = "Start poll";
            this.startPollButton.UseVisualStyleBackColor = true;
            this.startPollButton.Click += new System.EventHandler(this.startPollButton_Click);
            // 
            // intLockEnable
            // 
            this.intLockEnable.AutoSize = true;
            this.intLockEnable.Location = new System.Drawing.Point(177, 227);
            this.intLockEnable.Name = "intLockEnable";
            this.intLockEnable.Size = new System.Drawing.Size(59, 17);
            this.intLockEnable.TabIndex = 71;
            this.intLockEnable.Text = "Enable";
            this.intLockEnable.UseVisualStyleBackColor = true;
            // 
            // propGainTextBox
            // 
            this.propGainTextBox.Location = new System.Drawing.Point(114, 202);
            this.propGainTextBox.Name = "propGainTextBox";
            this.propGainTextBox.Size = new System.Drawing.Size(49, 20);
            this.propGainTextBox.TabIndex = 73;
            this.propGainTextBox.Text = "1";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(15, 205);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 23);
            this.label5.TabIndex = 72;
            this.label5.Text = "Proportional Gain";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(15, 228);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(78, 23);
            this.label6.TabIndex = 74;
            this.label6.Text = "Integral Gain";
            // 
            // intGainTextBox
            // 
            this.intGainTextBox.Location = new System.Drawing.Point(114, 225);
            this.intGainTextBox.Name = "intGainTextBox";
            this.intGainTextBox.Size = new System.Drawing.Size(49, 20);
            this.intGainTextBox.TabIndex = 75;
            this.intGainTextBox.Text = "1";
            // 
            // propLockEnable
            // 
            this.propLockEnable.AutoSize = true;
            this.propLockEnable.Location = new System.Drawing.Point(177, 204);
            this.propLockEnable.Name = "propLockEnable";
            this.propLockEnable.Size = new System.Drawing.Size(59, 17);
            this.propLockEnable.TabIndex = 77;
            this.propLockEnable.Text = "Enable";
            this.propLockEnable.UseVisualStyleBackColor = true;
            // 
            // ControlWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(361, 302);
            this.Controls.Add(this.propLockEnable);
            this.Controls.Add(this.intGainTextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.propGainTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.intLockEnable);
            this.Controls.Add(this.pollPeriodTextBox);
            this.Controls.Add(this.stopPollButton);
            this.Controls.Add(this.label80);
            this.Controls.Add(this.startPollButton);
            this.Controls.Add(this.errorSigGraph);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.freqSetpointTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.freqCounterTextBox);
            this.Controls.Add(this.counterFreqUpdateButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Name = "ControlWindow";
            this.Text = "VCOLock";
            ((System.ComponentModel.ISupportInitialize)(this.errorSigGraph)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox freqCounterTextBox;
        public System.Windows.Forms.Button counterFreqUpdateButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox freqSetpointTextBox;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        public System.Windows.Forms.TextBox pollPeriodTextBox;
        public System.Windows.Forms.Button stopPollButton;
        private System.Windows.Forms.Label label80;
        public System.Windows.Forms.Button startPollButton;
        public System.Windows.Forms.TextBox propGainTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.TextBox intGainTextBox;
        public NationalInstruments.UI.WaveformPlot errorSigPlot;
        public NationalInstruments.UI.WindowsForms.WaveformGraph errorSigGraph;
        public System.Windows.Forms.CheckBox intLockEnable;
        public System.Windows.Forms.CheckBox propLockEnable;
    }
}

