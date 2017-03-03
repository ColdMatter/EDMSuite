namespace NavigatorHardwareControl
{
    partial class ImageAnalysisWindow
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
            this.waveformGraph1 = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.waveformPlot1 = new NationalInstruments.UI.WaveformPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.waveformGraph2 = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.waveformPlot2 = new NationalInstruments.UI.WaveformPlot();
            this.xAxis2 = new NationalInstruments.UI.XAxis();
            this.yAxis2 = new NationalInstruments.UI.YAxis();
            this.average_pixelTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.waveformGraph4 = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.waveformPlot4 = new NationalInstruments.UI.WaveformPlot();
            this.xAxis4 = new NationalInstruments.UI.XAxis();
            this.yAxis4 = new NationalInstruments.UI.YAxis();
            this.waveformGraph3 = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.waveformPlot3 = new NationalInstruments.UI.WaveformPlot();
            this.xAxis3 = new NationalInstruments.UI.XAxis();
            this.yAxis3 = new NationalInstruments.UI.YAxis();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.waveformOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoScaleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.waveformGraph1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.waveformGraph2)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.waveformGraph4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.waveformGraph3)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // waveformGraph1
            // 
            this.waveformGraph1.Location = new System.Drawing.Point(49, 50);
            this.waveformGraph1.Name = "waveformGraph1";
            this.waveformGraph1.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.waveformPlot1});
            this.waveformGraph1.Size = new System.Drawing.Size(581, 151);
            this.waveformGraph1.TabIndex = 0;
            this.waveformGraph1.UseColorGenerator = true;
            this.waveformGraph1.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.waveformGraph1.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            this.waveformGraph1.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.waveformGraph1_PlotDataChanged);
            // 
            // waveformPlot1
            // 
            this.waveformPlot1.LineColor = System.Drawing.Color.White;
            this.waveformPlot1.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.waveformPlot1.XAxis = this.xAxis1;
            this.waveformPlot1.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.Caption = "Pixel";
            this.xAxis1.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.xAxis1.Range = new NationalInstruments.UI.Range(0D, 1038D);
            // 
            // yAxis1
            // 
            this.yAxis1.Caption = "Pixel Value";
            this.yAxis1.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.yAxis1.Range = new NationalInstruments.UI.Range(0D, 255D);
            // 
            // waveformGraph2
            // 
            this.waveformGraph2.Location = new System.Drawing.Point(49, 216);
            this.waveformGraph2.Name = "waveformGraph2";
            this.waveformGraph2.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.waveformPlot2});
            this.waveformGraph2.Size = new System.Drawing.Size(581, 153);
            this.waveformGraph2.TabIndex = 1;
            this.waveformGraph2.UseColorGenerator = true;
            this.waveformGraph2.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis2});
            this.waveformGraph2.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis2});
            // 
            // waveformPlot2
            // 
            this.waveformPlot2.LineColor = System.Drawing.Color.White;
            this.waveformPlot2.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.waveformPlot2.XAxis = this.xAxis2;
            this.waveformPlot2.YAxis = this.yAxis2;
            // 
            // xAxis2
            // 
            this.xAxis2.Caption = "Pixel";
            // 
            // yAxis2
            // 
            this.yAxis2.Caption = "Pixel Value";
            this.yAxis2.Range = new NationalInstruments.UI.Range(0D, 255D);
            // 
            // average_pixelTextBox
            // 
            this.average_pixelTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.average_pixelTextBox.Location = new System.Drawing.Point(295, 189);
            this.average_pixelTextBox.Name = "average_pixelTextBox";
            this.average_pixelTextBox.Size = new System.Drawing.Size(180, 38);
            this.average_pixelTextBox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(167, 192);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 31);
            this.label1.TabIndex = 3;
            this.label1.Text = "Average ";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.waveformGraph4);
            this.groupBox1.Controls.Add(this.waveformGraph3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.average_pixelTextBox);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(49, 367);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(581, 238);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ROI";
            // 
            // waveformGraph4
            // 
            this.waveformGraph4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.waveformGraph4.Location = new System.Drawing.Point(24, 28);
            this.waveformGraph4.Name = "waveformGraph4";
            this.waveformGraph4.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.waveformPlot4});
            this.waveformGraph4.Size = new System.Drawing.Size(265, 153);
            this.waveformGraph4.TabIndex = 5;
            this.waveformGraph4.UseColorGenerator = true;
            this.waveformGraph4.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis4});
            this.waveformGraph4.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis4});
            // 
            // waveformPlot4
            // 
            this.waveformPlot4.CanScaleYAxis = false;
            this.waveformPlot4.LineColor = System.Drawing.Color.White;
            this.waveformPlot4.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.waveformPlot4.XAxis = this.xAxis4;
            this.waveformPlot4.YAxis = this.yAxis4;
            // 
            // xAxis4
            // 
            this.xAxis4.Caption = "Pixel";
            this.xAxis4.MajorDivisions.GridColor = System.Drawing.Color.Silver;
            this.xAxis4.MajorDivisions.GridVisible = true;
            // 
            // yAxis4
            // 
            this.yAxis4.Caption = "Integrated Pixel Count";
            this.yAxis4.MajorDivisions.GridColor = System.Drawing.Color.Silver;
            this.yAxis4.MajorDivisions.GridVisible = true;
            this.yAxis4.MinorDivisions.GridColor = System.Drawing.Color.Gray;
            this.yAxis4.MinorDivisions.GridLineStyle = NationalInstruments.UI.LineStyle.Dash;
            this.yAxis4.MinorDivisions.GridVisible = true;
            this.yAxis4.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.yAxis4.Range = new NationalInstruments.UI.Range(0D, 200D);
            // 
            // waveformGraph3
            // 
            this.waveformGraph3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.waveformGraph3.Location = new System.Drawing.Point(296, 28);
            this.waveformGraph3.Name = "waveformGraph3";
            this.waveformGraph3.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.waveformPlot3});
            this.waveformGraph3.Size = new System.Drawing.Size(265, 153);
            this.waveformGraph3.TabIndex = 4;
            this.waveformGraph3.UseColorGenerator = true;
            this.waveformGraph3.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis3});
            this.waveformGraph3.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis3});
            // 
            // waveformPlot3
            // 
            this.waveformPlot3.CanScaleYAxis = false;
            this.waveformPlot3.LineColor = System.Drawing.Color.White;
            this.waveformPlot3.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.waveformPlot3.XAxis = this.xAxis3;
            this.waveformPlot3.YAxis = this.yAxis3;
            // 
            // xAxis3
            // 
            this.xAxis3.Caption = "Pixel";
            this.xAxis3.MajorDivisions.GridColor = System.Drawing.Color.Silver;
            this.xAxis3.MajorDivisions.GridVisible = true;
            // 
            // yAxis3
            // 
            this.yAxis3.MajorDivisions.GridColor = System.Drawing.Color.Silver;
            this.yAxis3.MajorDivisions.GridVisible = true;
            this.yAxis3.MinorDivisions.GridColor = System.Drawing.Color.Silver;
            this.yAxis3.MinorDivisions.GridLineStyle = NationalInstruments.UI.LineStyle.Dash;
            this.yAxis3.MinorDivisions.GridVisible = true;
            this.yAxis3.Mode = NationalInstruments.UI.AxisMode.Fixed;
            this.yAxis3.Range = new NationalInstruments.UI.Range(0D, 200D);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.waveformOptionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(691, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // waveformOptionsToolStripMenuItem
            // 
            this.waveformOptionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoScaleToolStripMenuItem});
            this.waveformOptionsToolStripMenuItem.Name = "waveformOptionsToolStripMenuItem";
            this.waveformOptionsToolStripMenuItem.Size = new System.Drawing.Size(119, 20);
            this.waveformOptionsToolStripMenuItem.Text = "Waveform Options";
            // 
            // autoScaleToolStripMenuItem
            // 
            this.autoScaleToolStripMenuItem.Checked = true;
            this.autoScaleToolStripMenuItem.CheckOnClick = true;
            this.autoScaleToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoScaleToolStripMenuItem.Name = "autoScaleToolStripMenuItem";
            this.autoScaleToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.autoScaleToolStripMenuItem.Text = "Auto Scale";
            this.autoScaleToolStripMenuItem.Click += new System.EventHandler(this.autoScaleToolStripMenuItem_Click);
            // 
            // ImageAnalysisWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(691, 635);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.waveformGraph2);
            this.Controls.Add(this.waveformGraph1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ImageAnalysisWindow";
            this.Text = "ImageAnalysis";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImageAnalysisWindow_FormClosing);
            this.Load += new System.EventHandler(this.ImageAnalysisWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.waveformGraph1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.waveformGraph2)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.waveformGraph4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.waveformGraph3)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private NationalInstruments.UI.WindowsForms.WaveformGraph waveformGraph1;
        private NationalInstruments.UI.WaveformPlot waveformPlot1;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private NationalInstruments.UI.WindowsForms.WaveformGraph waveformGraph2;
        private NationalInstruments.UI.WaveformPlot waveformPlot2;
        private NationalInstruments.UI.XAxis xAxis2;
        private NationalInstruments.UI.YAxis yAxis2;
        private System.Windows.Forms.TextBox average_pixelTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private NationalInstruments.UI.WindowsForms.WaveformGraph waveformGraph4;
        private NationalInstruments.UI.WaveformPlot waveformPlot4;
        private NationalInstruments.UI.XAxis xAxis4;
        private NationalInstruments.UI.YAxis yAxis4;
        private NationalInstruments.UI.WindowsForms.WaveformGraph waveformGraph3;
        private NationalInstruments.UI.WaveformPlot waveformPlot3;
        private NationalInstruments.UI.XAxis xAxis3;
        private NationalInstruments.UI.YAxis yAxis3;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem waveformOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoScaleToolStripMenuItem;
    }
}