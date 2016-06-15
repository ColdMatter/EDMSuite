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
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.propLockEnable = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.reverseCheckBox = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.setPointNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.propGainNumeric = new System.Windows.Forms.NumericUpDown();
            this.intGainNumeric = new System.Windows.Forms.NumericUpDown();
            this.outputVoltageNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.logCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.errorSigGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.setPointNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.propGainNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.intGainNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputVoltageNumericUpDown)).BeginInit();
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
            this.label1.Location = new System.Drawing.Point(15, 183);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 23);
            this.label1.TabIndex = 48;
            this.label1.Text = "Set point";
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
            this.errorSigPlot.HistoryCapacity = 10;
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
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(15, 206);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 23);
            this.label5.TabIndex = 72;
            this.label5.Text = "Proportional gain";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(15, 228);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(78, 23);
            this.label6.TabIndex = 74;
            this.label6.Text = "Integral gain";
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
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(15, 274);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(101, 23);
            this.label7.TabIndex = 78;
            this.label7.Text = "Output voltage";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(233, 274);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(48, 23);
            this.label8.TabIndex = 80;
            this.label8.Text = "V";
            // 
            // reverseCheckBox
            // 
            this.reverseCheckBox.AutoSize = true;
            this.reverseCheckBox.Location = new System.Drawing.Point(116, 252);
            this.reverseCheckBox.Name = "reverseCheckBox";
            this.reverseCheckBox.Size = new System.Drawing.Size(66, 17);
            this.reverseCheckBox.TabIndex = 82;
            this.reverseCheckBox.Text = "Reverse";
            this.reverseCheckBox.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(15, 251);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(78, 23);
            this.label9.TabIndex = 83;
            this.label9.Text = "Lock direction";
            // 
            // setPointNumericUpDown
            // 
            this.setPointNumericUpDown.Location = new System.Drawing.Point(116, 181);
            this.setPointNumericUpDown.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.setPointNumericUpDown.Name = "setPointNumericUpDown";
            this.setPointNumericUpDown.Size = new System.Drawing.Size(111, 20);
            this.setPointNumericUpDown.TabIndex = 84;
            this.setPointNumericUpDown.Value = new decimal(new int[] {
            14000,
            0,
            0,
            0});
            // 
            // propGainNumeric
            // 
            this.propGainNumeric.DecimalPlaces = 2;
            this.propGainNumeric.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.propGainNumeric.Location = new System.Drawing.Point(116, 204);
            this.propGainNumeric.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.propGainNumeric.Name = "propGainNumeric";
            this.propGainNumeric.Size = new System.Drawing.Size(47, 20);
            this.propGainNumeric.TabIndex = 85;
            this.propGainNumeric.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // intGainNumeric
            // 
            this.intGainNumeric.DecimalPlaces = 2;
            this.intGainNumeric.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.intGainNumeric.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.intGainNumeric.Location = new System.Drawing.Point(116, 226);
            this.intGainNumeric.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.intGainNumeric.Name = "intGainNumeric";
            this.intGainNumeric.Size = new System.Drawing.Size(47, 20);
            this.intGainNumeric.TabIndex = 86;
            this.intGainNumeric.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // outputVoltageNumericUpDown
            // 
            this.outputVoltageNumericUpDown.DecimalPlaces = 3;
            this.outputVoltageNumericUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.outputVoltageNumericUpDown.Location = new System.Drawing.Point(116, 272);
            this.outputVoltageNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.outputVoltageNumericUpDown.Name = "outputVoltageNumericUpDown";
            this.outputVoltageNumericUpDown.Size = new System.Drawing.Size(111, 20);
            this.outputVoltageNumericUpDown.TabIndex = 87;
            // 
            // logCheckBox
            // 
            this.logCheckBox.AutoSize = true;
            this.logCheckBox.Location = new System.Drawing.Point(278, 184);
            this.logCheckBox.Name = "logCheckBox";
            this.logCheckBox.Size = new System.Drawing.Size(71, 17);
            this.logCheckBox.TabIndex = 88;
            this.logCheckBox.Text = "Log Lock";
            this.logCheckBox.UseVisualStyleBackColor = true;
            // 
            // ControlWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(361, 302);
            this.Controls.Add(this.logCheckBox);
            this.Controls.Add(this.outputVoltageNumericUpDown);
            this.Controls.Add(this.intGainNumeric);
            this.Controls.Add(this.propGainNumeric);
            this.Controls.Add(this.setPointNumericUpDown);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.reverseCheckBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.propLockEnable);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.intLockEnable);
            this.Controls.Add(this.pollPeriodTextBox);
            this.Controls.Add(this.stopPollButton);
            this.Controls.Add(this.label80);
            this.Controls.Add(this.startPollButton);
            this.Controls.Add(this.errorSigGraph);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.freqCounterTextBox);
            this.Controls.Add(this.counterFreqUpdateButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Name = "ControlWindow";
            this.Text = "VCOLock";
            this.Load += new System.EventHandler(this.ControlWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorSigGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.setPointNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.propGainNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.intGainNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputVoltageNumericUpDown)).EndInit();
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
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        public System.Windows.Forms.TextBox pollPeriodTextBox;
        public System.Windows.Forms.Button stopPollButton;
        private System.Windows.Forms.Label label80;
        public System.Windows.Forms.Button startPollButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        public NationalInstruments.UI.WaveformPlot errorSigPlot;
        public NationalInstruments.UI.WindowsForms.WaveformGraph errorSigGraph;
        public System.Windows.Forms.CheckBox intLockEnable;
        public System.Windows.Forms.CheckBox propLockEnable;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.CheckBox reverseCheckBox;
        private System.Windows.Forms.Label label9;
        public System.Windows.Forms.NumericUpDown setPointNumericUpDown;
        public System.Windows.Forms.NumericUpDown propGainNumeric;
        public System.Windows.Forms.NumericUpDown intGainNumeric;
        public System.Windows.Forms.NumericUpDown outputVoltageNumericUpDown;
        public System.Windows.Forms.CheckBox logCheckBox;
    }
}

