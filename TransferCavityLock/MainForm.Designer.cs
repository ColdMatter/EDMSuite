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
            this.label3 = new System.Windows.Forms.Label();
            this.interGuessBox = new System.Windows.Forms.TextBox();
            this.fitEnableCheck = new System.Windows.Forms.CheckBox();
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
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.interval1 = new System.Windows.Forms.TextBox();
            this.gp1 = new System.Windows.Forms.TextBox();
            this.fitResultsP2 = new System.Windows.Forms.GroupBox();
            this.gp2 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.interval2 = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.plotFitsWindow = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot3 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis3 = new NationalInstruments.UI.XAxis();
            this.yAxis3 = new NationalInstruments.UI.YAxis();
            this.plotFitsWindow2 = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot4 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis4 = new NationalInstruments.UI.XAxis();
            this.yAxis4 = new NationalInstruments.UI.YAxis();
            this.voltageRampControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rampLED)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.p1Intensity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.p2Intensity)).BeginInit();
            this.fitResultsP1.SuspendLayout();
            this.fitResultsP2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.plotFitsWindow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.plotFitsWindow2)).BeginInit();
            this.SuspendLayout();
            // 
            // voltageRampControl
            // 
            this.voltageRampControl.Controls.Add(this.label3);
            this.voltageRampControl.Controls.Add(this.interGuessBox);
            this.voltageRampControl.Controls.Add(this.fitEnableCheck);
            this.voltageRampControl.Controls.Add(this.triggerMenu);
            this.voltageRampControl.Controls.Add(this.rampLED);
            this.voltageRampControl.Controls.Add(this.rampStopButton);
            this.voltageRampControl.Controls.Add(this.rampStartButton);
            this.voltageRampControl.Location = new System.Drawing.Point(554, 3);
            this.voltageRampControl.Name = "voltageRampControl";
            this.voltageRampControl.Size = new System.Drawing.Size(174, 130);
            this.voltageRampControl.TabIndex = 2;
            this.voltageRampControl.TabStop = false;
            this.voltageRampControl.Text = "Voltage Ramp";
            this.voltageRampControl.Enter += new System.EventHandler(this.voltageRampControl_Enter);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "inter guess";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // interGuessBox
            // 
            this.interGuessBox.Location = new System.Drawing.Point(68, 104);
            this.interGuessBox.Name = "interGuessBox";
            this.interGuessBox.Size = new System.Drawing.Size(100, 20);
            this.interGuessBox.TabIndex = 10;
            this.interGuessBox.TextChanged += new System.EventHandler(this.interGuessBox_TextChanged);
            // 
            // fitEnableCheck
            // 
            this.fitEnableCheck.AutoSize = true;
            this.fitEnableCheck.Location = new System.Drawing.Point(134, 77);
            this.fitEnableCheck.Name = "fitEnableCheck";
            this.fitEnableCheck.Size = new System.Drawing.Size(34, 17);
            this.fitEnableCheck.TabIndex = 9;
            this.fitEnableCheck.Text = "fit";
            this.fitEnableCheck.UseVisualStyleBackColor = true;
            this.fitEnableCheck.CheckedChanged += new System.EventHandler(this.fitEnableCheck_CheckedChanged);
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
            this.textBox.Location = new System.Drawing.Point(554, 139);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(174, 20);
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
            this.p2Intensity.Location = new System.Drawing.Point(0, 237);
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
            this.fitResultsP1.Controls.Add(this.label2);
            this.fitResultsP1.Controls.Add(this.label1);
            this.fitResultsP1.Controls.Add(this.interval1);
            this.fitResultsP1.Controls.Add(this.gp1);
            this.fitResultsP1.Location = new System.Drawing.Point(734, 3);
            this.fitResultsP1.Name = "fitResultsP1";
            this.fitResultsP1.Size = new System.Drawing.Size(174, 75);
            this.fitResultsP1.TabIndex = 9;
            this.fitResultsP1.TabStop = false;
            this.fitResultsP1.Text = "Fit Results for Laser";
            this.fitResultsP1.Enter += new System.EventHandler(this.fitResultsP1_Enter);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "P. Interval";
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
            // interval1
            // 
            this.interval1.Location = new System.Drawing.Point(68, 45);
            this.interval1.Name = "interval1";
            this.interval1.Size = new System.Drawing.Size(100, 20);
            this.interval1.TabIndex = 12;
            this.interval1.TextChanged += new System.EventHandler(this.interval1_TextChanged);
            // 
            // gp1
            // 
            this.gp1.Location = new System.Drawing.Point(68, 19);
            this.gp1.Name = "gp1";
            this.gp1.Size = new System.Drawing.Size(100, 20);
            this.gp1.TabIndex = 11;
            this.gp1.TextChanged += new System.EventHandler(this.gp1_TextChanged);
            // 
            // fitResultsP2
            // 
            this.fitResultsP2.Controls.Add(this.gp2);
            this.fitResultsP2.Controls.Add(this.label9);
            this.fitResultsP2.Controls.Add(this.interval2);
            this.fitResultsP2.Controls.Add(this.label10);
            this.fitResultsP2.Location = new System.Drawing.Point(734, 84);
            this.fitResultsP2.Name = "fitResultsP2";
            this.fitResultsP2.Size = new System.Drawing.Size(174, 75);
            this.fitResultsP2.TabIndex = 10;
            this.fitResultsP2.TabStop = false;
            this.fitResultsP2.Text = "Fit Results for Reference Laser";
            this.fitResultsP2.Enter += new System.EventHandler(this.fitResultsP2_Enter);
            // 
            // gp2
            // 
            this.gp2.Location = new System.Drawing.Point(67, 21);
            this.gp2.Name = "gp2";
            this.gp2.Size = new System.Drawing.Size(100, 20);
            this.gp2.TabIndex = 16;
            this.gp2.TextChanged += new System.EventHandler(this.gp2_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(10, 52);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(55, 13);
            this.label9.TabIndex = 22;
            this.label9.Text = "P. Interval";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // interval2
            // 
            this.interval2.Location = new System.Drawing.Point(67, 47);
            this.interval2.Name = "interval2";
            this.interval2.Size = new System.Drawing.Size(100, 20);
            this.interval2.TabIndex = 17;
            this.interval2.TextChanged += new System.EventHandler(this.interval2_TextChanged);
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
            // plotFitsWindow
            // 
            this.plotFitsWindow.Location = new System.Drawing.Point(0, 119);
            this.plotFitsWindow.Name = "plotFitsWindow";
            this.plotFitsWindow.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot3});
            this.plotFitsWindow.Size = new System.Drawing.Size(548, 112);
            this.plotFitsWindow.TabIndex = 11;
            this.plotFitsWindow.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis3});
            this.plotFitsWindow.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis3});
            this.plotFitsWindow.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.plotFitsWindow_PlotDataChanged);
            // 
            // scatterPlot3
            // 
            this.scatterPlot3.XAxis = this.xAxis3;
            this.scatterPlot3.YAxis = this.yAxis3;
            // 
            // plotFitsWindow2
            // 
            this.plotFitsWindow2.Location = new System.Drawing.Point(0, 355);
            this.plotFitsWindow2.Name = "plotFitsWindow2";
            this.plotFitsWindow2.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot4});
            this.plotFitsWindow2.Size = new System.Drawing.Size(548, 112);
            this.plotFitsWindow2.TabIndex = 12;
            this.plotFitsWindow2.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis4});
            this.plotFitsWindow2.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis4});
            this.plotFitsWindow2.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.plotFitsWindow2_PlotDataChanged);
            // 
            // scatterPlot4
            // 
            this.scatterPlot4.XAxis = this.xAxis4;
            this.scatterPlot4.YAxis = this.yAxis4;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(913, 476);
            this.Controls.Add(this.plotFitsWindow2);
            this.Controls.Add(this.plotFitsWindow);
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
            this.voltageRampControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rampLED)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.p1Intensity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.p2Intensity)).EndInit();
            this.fitResultsP1.ResumeLayout(false);
            this.fitResultsP1.PerformLayout();
            this.fitResultsP2.ResumeLayout(false);
            this.fitResultsP2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.plotFitsWindow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.plotFitsWindow2)).EndInit();
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
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox interval1;
        private System.Windows.Forms.TextBox gp2;
        private System.Windows.Forms.TextBox interval2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox fitEnableCheck;
        private NationalInstruments.UI.WindowsForms.ScatterGraph plotFitsWindow;
        private NationalInstruments.UI.ScatterPlot scatterPlot3;
        private NationalInstruments.UI.XAxis xAxis3;
        private NationalInstruments.UI.YAxis yAxis3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox interGuessBox;
        private NationalInstruments.UI.WindowsForms.ScatterGraph plotFitsWindow2;
        private NationalInstruments.UI.ScatterPlot scatterPlot4;
        private NationalInstruments.UI.XAxis xAxis4;
        private NationalInstruments.UI.YAxis yAxis4;
    }
}

