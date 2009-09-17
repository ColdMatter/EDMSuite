namespace TransferCavityLock
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
            this.voltageRampControl = new System.Windows.Forms.GroupBox();
            this.triggerMenu = new System.Windows.Forms.ComboBox();
            this.rampLED = new NationalInstruments.UI.WindowsForms.Led();
            this.rampStopButton = new System.Windows.Forms.Button();
            this.rampStartButton = new System.Windows.Forms.Button();
            this.textBox = new System.Windows.Forms.TextBox();
            this.p1Intensity = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot1 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.p2Intensity = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot2 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis2 = new NationalInstruments.UI.XAxis();
            this.yAxis2 = new NationalInstruments.UI.YAxis();
            this.fitResultsP1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.background1 = new System.Windows.Forms.TextBox();
            this.amplitude1 = new System.Windows.Forms.TextBox();
            this.finesse1 = new System.Windows.Forms.TextBox();
            this.freq1 = new System.Windows.Forms.TextBox();
            this.gp1 = new System.Windows.Forms.TextBox();
            this.fitResultsP2 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.background2 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.gp2 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.amplitude2 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.freq2 = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.finesse2 = new System.Windows.Forms.TextBox();
            this.voltageRampControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rampLED)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.p1Intensity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.p2Intensity)).BeginInit();
            this.fitResultsP1.SuspendLayout();
            this.fitResultsP2.SuspendLayout();
            this.SuspendLayout();
            // 
            // voltageRampControl
            // 
            this.voltageRampControl.Controls.Add(this.triggerMenu);
            this.voltageRampControl.Controls.Add(this.rampLED);
            this.voltageRampControl.Controls.Add(this.rampStopButton);
            this.voltageRampControl.Controls.Add(this.rampStartButton);
            this.voltageRampControl.Location = new System.Drawing.Point(554, 3);
            this.voltageRampControl.Name = "voltageRampControl";
            this.voltageRampControl.Size = new System.Drawing.Size(159, 103);
            this.voltageRampControl.TabIndex = 2;
            this.voltageRampControl.TabStop = false;
            this.voltageRampControl.Text = "Voltage Ramp";
            this.voltageRampControl.Enter += new System.EventHandler(this.voltageRampControl_Enter);
            // 
            // triggerMenu
            // 
            this.triggerMenu.FormattingEnabled = true;
            this.triggerMenu.Items.AddRange(new object[] {
            "int",
            "ext"});
            this.triggerMenu.Location = new System.Drawing.Point(6, 75);
            this.triggerMenu.MaxDropDownItems = 2;
            this.triggerMenu.Name = "triggerMenu";
            this.triggerMenu.Size = new System.Drawing.Size(121, 21);
            this.triggerMenu.TabIndex = 8;
            this.triggerMenu.Text = "Select Trigger";
            this.triggerMenu.SelectedIndexChanged += new System.EventHandler(this.triggerMenu_SelectedIndexChanged);
            // 
            // rampLED
            // 
            this.rampLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.rampLED.Location = new System.Drawing.Point(123, 19);
            this.rampLED.Name = "rampLED";
            this.rampLED.OffColor = System.Drawing.Color.Red;
            this.rampLED.Size = new System.Drawing.Size(31, 29);
            this.rampLED.TabIndex = 7;
            // 
            // rampStopButton
            // 
            this.rampStopButton.Location = new System.Drawing.Point(6, 46);
            this.rampStopButton.Name = "rampStopButton";
            this.rampStopButton.Size = new System.Drawing.Size(111, 23);
            this.rampStopButton.TabIndex = 6;
            this.rampStopButton.Text = "Stop ramping";
            this.rampStopButton.UseVisualStyleBackColor = true;
            this.rampStopButton.Click += new System.EventHandler(this.rampStopButton_Click);
            // 
            // rampStartButton
            // 
            this.rampStartButton.Location = new System.Drawing.Point(6, 19);
            this.rampStartButton.Name = "rampStartButton";
            this.rampStartButton.Size = new System.Drawing.Size(111, 23);
            this.rampStartButton.TabIndex = 2;
            this.rampStartButton.Text = "Start ramping";
            this.rampStartButton.UseVisualStyleBackColor = true;
            this.rampStartButton.Click += new System.EventHandler(this.rampStartButton_Click);
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(12, 239);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(499, 20);
            this.textBox.TabIndex = 3;
            this.textBox.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // p1Intensity
            // 
            this.p1Intensity.Location = new System.Drawing.Point(0, 3);
            this.p1Intensity.Name = "p1Intensity";
            this.p1Intensity.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot1});
            this.p1Intensity.Size = new System.Drawing.Size(548, 112);
            this.p1Intensity.TabIndex = 4;
            this.p1Intensity.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.p1Intensity.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            this.p1Intensity.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.p1Intensity_PlotDataChanged);
            // 
            // scatterPlot1
            // 
            this.scatterPlot1.XAxis = this.xAxis1;
            this.scatterPlot1.YAxis = this.yAxis1;
            // 
            // p2Intensity
            // 
            this.p2Intensity.Location = new System.Drawing.Point(0, 121);
            this.p2Intensity.Name = "p2Intensity";
            this.p2Intensity.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot2});
            this.p2Intensity.Size = new System.Drawing.Size(548, 112);
            this.p2Intensity.TabIndex = 5;
            this.p2Intensity.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis2});
            this.p2Intensity.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis2});
            this.p2Intensity.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.p2Intensity_PlotDataChanged);
            // 
            // scatterPlot2
            // 
            this.scatterPlot2.XAxis = this.xAxis2;
            this.scatterPlot2.YAxis = this.yAxis2;
            // 
            // fitResultsP1
            // 
            this.fitResultsP1.Controls.Add(this.label5);
            this.fitResultsP1.Controls.Add(this.label4);
            this.fitResultsP1.Controls.Add(this.label3);
            this.fitResultsP1.Controls.Add(this.label2);
            this.fitResultsP1.Controls.Add(this.label1);
            this.fitResultsP1.Controls.Add(this.background1);
            this.fitResultsP1.Controls.Add(this.amplitude1);
            this.fitResultsP1.Controls.Add(this.finesse1);
            this.fitResultsP1.Controls.Add(this.freq1);
            this.fitResultsP1.Controls.Add(this.gp1);
            this.fitResultsP1.Location = new System.Drawing.Point(554, 112);
            this.fitResultsP1.Name = "fitResultsP1";
            this.fitResultsP1.Size = new System.Drawing.Size(174, 147);
            this.fitResultsP1.TabIndex = 9;
            this.fitResultsP1.TabStop = false;
            this.fitResultsP1.Text = "Fit Results for Laser";
            this.fitResultsP1.Enter += new System.EventHandler(this.fitResultsP1_Enter);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 126);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(22, 13);
            this.label5.TabIndex = 20;
            this.label5.Text = "BG";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 104);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "Amplitude";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "Finesse";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Freq";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "G. Phase";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // background1
            // 
            this.background1.Location = new System.Drawing.Point(68, 123);
            this.background1.Name = "background1";
            this.background1.Size = new System.Drawing.Size(100, 20);
            this.background1.TabIndex = 15;
            this.background1.TextChanged += new System.EventHandler(this.background1_TextChanged);
            // 
            // amplitude1
            // 
            this.amplitude1.Location = new System.Drawing.Point(68, 97);
            this.amplitude1.Name = "amplitude1";
            this.amplitude1.Size = new System.Drawing.Size(100, 20);
            this.amplitude1.TabIndex = 14;
            this.amplitude1.TextChanged += new System.EventHandler(this.amplitude1_TextChanged);
            // 
            // finesse1
            // 
            this.finesse1.Location = new System.Drawing.Point(68, 71);
            this.finesse1.Name = "finesse1";
            this.finesse1.Size = new System.Drawing.Size(100, 20);
            this.finesse1.TabIndex = 13;
            this.finesse1.TextChanged += new System.EventHandler(this.finesse1_TextChanged);
            // 
            // freq1
            // 
            this.freq1.Location = new System.Drawing.Point(68, 45);
            this.freq1.Name = "freq1";
            this.freq1.Size = new System.Drawing.Size(100, 20);
            this.freq1.TabIndex = 12;
            this.freq1.TextChanged += new System.EventHandler(this.freq1_TextChanged);
            // 
            // gp1
            // 
            this.gp1.Location = new System.Drawing.Point(68, 19);
            this.gp1.Name = "gp1";
            this.gp1.Size = new System.Drawing.Size(100, 20);
            this.gp1.TabIndex = 11;
            // 
            // fitResultsP2
            // 
            this.fitResultsP2.Controls.Add(this.label6);
            this.fitResultsP2.Controls.Add(this.background2);
            this.fitResultsP2.Controls.Add(this.label7);
            this.fitResultsP2.Controls.Add(this.gp2);
            this.fitResultsP2.Controls.Add(this.label8);
            this.fitResultsP2.Controls.Add(this.amplitude2);
            this.fitResultsP2.Controls.Add(this.label9);
            this.fitResultsP2.Controls.Add(this.freq2);
            this.fitResultsP2.Controls.Add(this.label10);
            this.fitResultsP2.Controls.Add(this.finesse2);
            this.fitResultsP2.Location = new System.Drawing.Point(734, 112);
            this.fitResultsP2.Name = "fitResultsP2";
            this.fitResultsP2.Size = new System.Drawing.Size(174, 147);
            this.fitResultsP2.TabIndex = 10;
            this.fitResultsP2.TabStop = false;
            this.fitResultsP2.Text = "Fit Results for Reference Laser";
            this.fitResultsP2.Enter += new System.EventHandler(this.fitResultsP2_Enter);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 126);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(22, 13);
            this.label6.TabIndex = 25;
            this.label6.Text = "BG";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // background2
            // 
            this.background2.Location = new System.Drawing.Point(67, 125);
            this.background2.Name = "background2";
            this.background2.Size = new System.Drawing.Size(100, 20);
            this.background2.TabIndex = 20;
            this.background2.TextChanged += new System.EventHandler(this.background2_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 104);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 24;
            this.label7.Text = "Amplitude";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // gp2
            // 
            this.gp2.Location = new System.Drawing.Point(67, 21);
            this.gp2.Name = "gp2";
            this.gp2.Size = new System.Drawing.Size(100, 20);
            this.gp2.TabIndex = 16;
            this.gp2.TextChanged += new System.EventHandler(this.gp2_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(10, 78);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(43, 13);
            this.label8.TabIndex = 23;
            this.label8.Text = "Finesse";
            this.label8.Click += new System.EventHandler(this.label8_Click);
            // 
            // amplitude2
            // 
            this.amplitude2.Location = new System.Drawing.Point(67, 99);
            this.amplitude2.Name = "amplitude2";
            this.amplitude2.Size = new System.Drawing.Size(100, 20);
            this.amplitude2.TabIndex = 19;
            this.amplitude2.TextChanged += new System.EventHandler(this.amplitude2_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(10, 52);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(28, 13);
            this.label9.TabIndex = 22;
            this.label9.Text = "Freq";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // freq2
            // 
            this.freq2.Location = new System.Drawing.Point(67, 47);
            this.freq2.Name = "freq2";
            this.freq2.Size = new System.Drawing.Size(100, 20);
            this.freq2.TabIndex = 17;
            this.freq2.TextChanged += new System.EventHandler(this.freq2_TextChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(10, 28);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(51, 13);
            this.label10.TabIndex = 21;
            this.label10.Text = "G. Phase";
            this.label10.Click += new System.EventHandler(this.label10_Click);
            // 
            // finesse2
            // 
            this.finesse2.Location = new System.Drawing.Point(67, 73);
            this.finesse2.Name = "finesse2";
            this.finesse2.Size = new System.Drawing.Size(100, 20);
            this.finesse2.TabIndex = 18;
            this.finesse2.TextChanged += new System.EventHandler(this.finesse2_TextChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(913, 271);
            this.Controls.Add(this.fitResultsP2);
            this.Controls.Add(this.fitResultsP1);
            this.Controls.Add(this.p2Intensity);
            this.Controls.Add(this.p1Intensity);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.voltageRampControl);
            this.Name = "MainForm";
            this.Text = "Transfer Cavity Lock";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.voltageRampControl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.rampLED)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.p1Intensity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.p2Intensity)).EndInit();
            this.fitResultsP1.ResumeLayout(false);
            this.fitResultsP1.PerformLayout();
            this.fitResultsP2.ResumeLayout(false);
            this.fitResultsP2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox voltageRampControl;
        private System.Windows.Forms.Button rampStartButton;
        private System.Windows.Forms.Button rampStopButton;
        private NationalInstruments.UI.WindowsForms.Led rampLED;
        private System.Windows.Forms.TextBox textBox;
        private System.Windows.Forms.ComboBox triggerMenu;
        private NationalInstruments.UI.WindowsForms.ScatterGraph p1Intensity;
        private NationalInstruments.UI.ScatterPlot scatterPlot1;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private NationalInstruments.UI.WindowsForms.ScatterGraph p2Intensity;
        private NationalInstruments.UI.ScatterPlot scatterPlot2;
        private NationalInstruments.UI.XAxis xAxis2;
        private NationalInstruments.UI.YAxis yAxis2;
        private System.Windows.Forms.GroupBox fitResultsP1;
        private System.Windows.Forms.TextBox gp1;
        private System.Windows.Forms.GroupBox fitResultsP2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox background1;
        private System.Windows.Forms.TextBox amplitude1;
        private System.Windows.Forms.TextBox finesse1;
        private System.Windows.Forms.TextBox freq1;
        private System.Windows.Forms.TextBox background2;
        private System.Windows.Forms.TextBox gp2;
        private System.Windows.Forms.TextBox amplitude2;
        private System.Windows.Forms.TextBox freq2;
        private System.Windows.Forms.TextBox finesse2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
    }
}

