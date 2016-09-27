namespace RFMOTHardwareControl
{
    partial class voltageLogger
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
            this.voltageInGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.waveformPlot1 = new NationalInstruments.UI.WaveformPlot();
            this.channelNamesComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.sampleRateTB = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.samplesTB = new System.Windows.Forms.TextBox();
            this.saveToTB = new System.Windows.Forms.TextBox();
            this.saveCB = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.filenameTB = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.logBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.voltageInGraph)).BeginInit();
            this.SuspendLayout();
            // 
            // voltageInGraph
            // 
            this.voltageInGraph.Border = NationalInstruments.UI.Border.None;
            this.voltageInGraph.Location = new System.Drawing.Point(-2, 72);
            this.voltageInGraph.Name = "voltageInGraph";
            this.voltageInGraph.PlotAreaBorder = NationalInstruments.UI.Border.None;
            this.voltageInGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.waveformPlot1});
            this.voltageInGraph.Size = new System.Drawing.Size(630, 188);
            this.voltageInGraph.TabIndex = 0;
            this.voltageInGraph.UseColorGenerator = true;
            this.voltageInGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.voltageInGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // xAxis1
            // 
            this.xAxis1.Caption = "Sample";
            // 
            // yAxis1
            // 
            this.yAxis1.Caption = "Voltage";
            // 
            // waveformPlot1
            // 
            this.waveformPlot1.XAxis = this.xAxis1;
            this.waveformPlot1.YAxis = this.yAxis1;
            // 
            // channelNamesComboBox
            // 
            this.channelNamesComboBox.FormattingEnabled = true;
            this.channelNamesComboBox.Location = new System.Drawing.Point(78, 12);
            this.channelNamesComboBox.Name = "channelNamesComboBox";
            this.channelNamesComboBox.Size = new System.Drawing.Size(121, 21);
            this.channelNamesComboBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Channel";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(232, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Sample Rate:";
            // 
            // sampleRateTB
            // 
            this.sampleRateTB.Location = new System.Drawing.Point(307, 12);
            this.sampleRateTB.Name = "sampleRateTB";
            this.sampleRateTB.Size = new System.Drawing.Size(100, 20);
            this.sampleRateTB.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(413, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Hz";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(455, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Samples:";
            // 
            // samplesTB
            // 
            this.samplesTB.Location = new System.Drawing.Point(508, 13);
            this.samplesTB.Name = "samplesTB";
            this.samplesTB.Size = new System.Drawing.Size(100, 20);
            this.samplesTB.TabIndex = 7;
            // 
            // saveToTB
            // 
            this.saveToTB.Location = new System.Drawing.Point(145, 43);
            this.saveToTB.Name = "saveToTB";
            this.saveToTB.Size = new System.Drawing.Size(218, 20);
            this.saveToTB.TabIndex = 8;
            // 
            // saveCB
            // 
            this.saveCB.AutoSize = true;
            this.saveCB.Location = new System.Drawing.Point(29, 45);
            this.saveCB.Name = "saveCB";
            this.saveCB.Size = new System.Drawing.Size(51, 17);
            this.saveCB.TabIndex = 9;
            this.saveCB.Text = "Save";
            this.saveCB.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(88, 46);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Save To:";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(412, 45);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Filename:";
            // 
            // filenameTB
            // 
            this.filenameTB.Location = new System.Drawing.Point(467, 42);
            this.filenameTB.Name = "filenameTB";
            this.filenameTB.Size = new System.Drawing.Size(131, 20);
            this.filenameTB.TabIndex = 11;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(602, 45);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(21, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = ".txt";
            // 
            // logBtn
            // 
            this.logBtn.Location = new System.Drawing.Point(288, 261);
            this.logBtn.Name = "logBtn";
            this.logBtn.Size = new System.Drawing.Size(75, 23);
            this.logBtn.TabIndex = 14;
            this.logBtn.Text = "Log!";
            this.logBtn.UseVisualStyleBackColor = true;
            this.logBtn.Click += new System.EventHandler(this.logBtn_Click);
            // 
            // voltageLogger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(648, 296);
            this.Controls.Add(this.logBtn);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.filenameTB);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.saveCB);
            this.Controls.Add(this.saveToTB);
            this.Controls.Add(this.samplesTB);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.sampleRateTB);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.channelNamesComboBox);
            this.Controls.Add(this.voltageInGraph);
            this.Name = "voltageLogger";
            this.Text = "voltageLogger";
            ((System.ComponentModel.ISupportInitialize)(this.voltageInGraph)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private NationalInstruments.UI.WindowsForms.WaveformGraph voltageInGraph;
        private NationalInstruments.UI.WaveformPlot waveformPlot1;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private System.Windows.Forms.ComboBox channelNamesComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox sampleRateTB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox samplesTB;
        private System.Windows.Forms.TextBox saveToTB;
        private System.Windows.Forms.CheckBox saveCB;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox filenameTB;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button logBtn;
    }
}