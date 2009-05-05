namespace EDMBlockHead
{
    partial class LiveViewer
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
            this.statusText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.clusterStatusText = new System.Windows.Forms.TextBox();
            this.sigScatterGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.sigPlot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.bScatterGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.bPlot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis2 = new NationalInstruments.UI.XAxis();
            this.yAxis2 = new NationalInstruments.UI.YAxis();
            this.resetRunningMeans = new System.Windows.Forms.Button();
            this.dbScatterGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.dbPlot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis3 = new NationalInstruments.UI.XAxis();
            this.yAxis3 = new NationalInstruments.UI.YAxis();
            this.edmErrorScatterGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.edmErrorPlot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis4 = new NationalInstruments.UI.XAxis();
            this.yAxis4 = new NationalInstruments.UI.YAxis();
            this.edmNormedErrorPlot = new NationalInstruments.UI.ScatterPlot();
            this.sigSigmaHi = new NationalInstruments.UI.ScatterPlot();
            this.sigSigmaLo = new NationalInstruments.UI.ScatterPlot();
            this.bSigmaHi = new NationalInstruments.UI.ScatterPlot();
            this.bSigmaLo = new NationalInstruments.UI.ScatterPlot();
            this.dbSigmaHi = new NationalInstruments.UI.ScatterPlot();
            this.dbSigmaLo = new NationalInstruments.UI.ScatterPlot();
            ((System.ComponentModel.ISupportInitialize)(this.sigScatterGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bScatterGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbScatterGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.edmErrorScatterGraph)).BeginInit();
            this.SuspendLayout();
            // 
            // statusText
            // 
            this.statusText.BackColor = System.Drawing.Color.Black;
            this.statusText.ForeColor = System.Drawing.Color.Lime;
            this.statusText.Location = new System.Drawing.Point(12, 54);
            this.statusText.Multiline = true;
            this.statusText.Name = "statusText";
            this.statusText.ReadOnly = true;
            this.statusText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.statusText.Size = new System.Drawing.Size(328, 316);
            this.statusText.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Block Analysis";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 394);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Cluster Information";
            // 
            // clusterStatusText
            // 
            this.clusterStatusText.BackColor = System.Drawing.Color.Black;
            this.clusterStatusText.ForeColor = System.Drawing.Color.Lime;
            this.clusterStatusText.Location = new System.Drawing.Point(12, 419);
            this.clusterStatusText.Multiline = true;
            this.clusterStatusText.Name = "clusterStatusText";
            this.clusterStatusText.ReadOnly = true;
            this.clusterStatusText.Size = new System.Drawing.Size(328, 43);
            this.clusterStatusText.TabIndex = 3;
            // 
            // sigScatterGraph
            // 
            this.sigScatterGraph.Caption = "SIG";
            this.sigScatterGraph.CaptionBackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.sigScatterGraph.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sigScatterGraph.CaptionPosition = NationalInstruments.UI.CaptionPosition.Left;
            this.sigScatterGraph.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sigScatterGraph.Location = new System.Drawing.Point(346, 54);
            this.sigScatterGraph.Name = "sigScatterGraph";
            this.sigScatterGraph.PlotAreaBorder = NationalInstruments.UI.Border.Etched;
            this.sigScatterGraph.PlotAreaColor = System.Drawing.Color.White;
            this.sigScatterGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.sigPlot,
            this.sigSigmaHi,
            this.sigSigmaLo});
            this.sigScatterGraph.Size = new System.Drawing.Size(259, 203);
            this.sigScatterGraph.TabIndex = 5;
            this.sigScatterGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.sigScatterGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // sigPlot
            // 
            this.sigPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.sigPlot.PointColor = System.Drawing.Color.DodgerBlue;
            this.sigPlot.PointSize = new System.Drawing.Size(3, 3);
            this.sigPlot.PointStyle = NationalInstruments.UI.PointStyle.SolidCircle;
            this.sigPlot.XAxis = this.xAxis1;
            this.sigPlot.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xAxis1.CaptionPosition = NationalInstruments.UI.XAxisPosition.Top;
            this.xAxis1.EditRangeNumericFormatMode = NationalInstruments.UI.NumericFormatMode.CreateGenericMode("F0");
            this.xAxis1.MajorDivisions.LabelFont = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // yAxis1
            // 
            this.yAxis1.MajorDivisions.LabelFont = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // bScatterGraph
            // 
            this.bScatterGraph.Caption = "B";
            this.bScatterGraph.CaptionBackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.bScatterGraph.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bScatterGraph.CaptionPosition = NationalInstruments.UI.CaptionPosition.Left;
            this.bScatterGraph.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bScatterGraph.Location = new System.Drawing.Point(615, 54);
            this.bScatterGraph.Name = "bScatterGraph";
            this.bScatterGraph.PlotAreaBorder = NationalInstruments.UI.Border.Etched;
            this.bScatterGraph.PlotAreaColor = System.Drawing.Color.White;
            this.bScatterGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.bPlot,
            this.bSigmaHi,
            this.bSigmaLo});
            this.bScatterGraph.Size = new System.Drawing.Size(259, 203);
            this.bScatterGraph.TabIndex = 6;
            this.bScatterGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis2});
            this.bScatterGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis2});
            // 
            // bPlot
            // 
            this.bPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.bPlot.PointColor = System.Drawing.Color.DodgerBlue;
            this.bPlot.PointSize = new System.Drawing.Size(3, 3);
            this.bPlot.PointStyle = NationalInstruments.UI.PointStyle.SolidCircle;
            this.bPlot.XAxis = this.xAxis2;
            this.bPlot.YAxis = this.yAxis2;
            // 
            // xAxis2
            // 
            this.xAxis2.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xAxis2.CaptionPosition = NationalInstruments.UI.XAxisPosition.Top;
            this.xAxis2.EditRangeNumericFormatMode = NationalInstruments.UI.NumericFormatMode.CreateGenericMode("F0");
            this.xAxis2.MajorDivisions.LabelFont = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // yAxis2
            // 
            this.yAxis2.MajorDivisions.LabelFont = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.yAxis2.OriginLineVisible = true;
            // 
            // resetRunningMeans
            // 
            this.resetRunningMeans.Location = new System.Drawing.Point(12, 468);
            this.resetRunningMeans.Name = "resetRunningMeans";
            this.resetRunningMeans.Size = new System.Drawing.Size(96, 23);
            this.resetRunningMeans.TabIndex = 24;
            this.resetRunningMeans.Text = "Reset";
            this.resetRunningMeans.Click += new System.EventHandler(this.resetRunningMeans_Click);
            // 
            // dbScatterGraph
            // 
            this.dbScatterGraph.Caption = "DB";
            this.dbScatterGraph.CaptionBackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.dbScatterGraph.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dbScatterGraph.CaptionPosition = NationalInstruments.UI.CaptionPosition.Left;
            this.dbScatterGraph.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dbScatterGraph.Location = new System.Drawing.Point(346, 282);
            this.dbScatterGraph.Name = "dbScatterGraph";
            this.dbScatterGraph.PlotAreaBorder = NationalInstruments.UI.Border.Etched;
            this.dbScatterGraph.PlotAreaColor = System.Drawing.Color.White;
            this.dbScatterGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.dbPlot,
            this.dbSigmaHi,
            this.dbSigmaLo});
            this.dbScatterGraph.Size = new System.Drawing.Size(259, 203);
            this.dbScatterGraph.TabIndex = 25;
            this.dbScatterGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis3});
            this.dbScatterGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis3});
            // 
            // dbPlot
            // 
            this.dbPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.dbPlot.PointColor = System.Drawing.Color.DodgerBlue;
            this.dbPlot.PointSize = new System.Drawing.Size(3, 3);
            this.dbPlot.PointStyle = NationalInstruments.UI.PointStyle.SolidCircle;
            this.dbPlot.XAxis = this.xAxis3;
            this.dbPlot.YAxis = this.yAxis3;
            // 
            // xAxis3
            // 
            this.xAxis3.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xAxis3.CaptionPosition = NationalInstruments.UI.XAxisPosition.Top;
            this.xAxis3.EditRangeNumericFormatMode = NationalInstruments.UI.NumericFormatMode.CreateGenericMode("F0");
            this.xAxis3.MajorDivisions.LabelFont = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // yAxis3
            // 
            this.yAxis3.MajorDivisions.LabelFont = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // edmErrorScatterGraph
            // 
            this.edmErrorScatterGraph.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.edmErrorScatterGraph.Caption = "edm Error (10 ^ -26)";
            this.edmErrorScatterGraph.CaptionBackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.edmErrorScatterGraph.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.edmErrorScatterGraph.CaptionPosition = NationalInstruments.UI.CaptionPosition.Left;
            this.edmErrorScatterGraph.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.edmErrorScatterGraph.Location = new System.Drawing.Point(615, 282);
            this.edmErrorScatterGraph.Name = "edmErrorScatterGraph";
            this.edmErrorScatterGraph.PlotAreaBorder = NationalInstruments.UI.Border.Etched;
            this.edmErrorScatterGraph.PlotAreaColor = System.Drawing.Color.White;
            this.edmErrorScatterGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.edmErrorPlot,
            this.edmNormedErrorPlot});
            this.edmErrorScatterGraph.Size = new System.Drawing.Size(259, 203);
            this.edmErrorScatterGraph.TabIndex = 26;
            this.edmErrorScatterGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis4});
            this.edmErrorScatterGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis4});
            // 
            // edmErrorPlot
            // 
            this.edmErrorPlot.PointColor = System.Drawing.Color.DodgerBlue;
            this.edmErrorPlot.PointStyle = NationalInstruments.UI.PointStyle.Cross;
            this.edmErrorPlot.XAxis = this.xAxis4;
            this.edmErrorPlot.YAxis = this.yAxis4;
            // 
            // xAxis4
            // 
            this.xAxis4.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xAxis4.CaptionPosition = NationalInstruments.UI.XAxisPosition.Top;
            this.xAxis4.EditRangeNumericFormatMode = NationalInstruments.UI.NumericFormatMode.CreateGenericMode("F0");
            this.xAxis4.MajorDivisions.LabelFont = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // yAxis4
            // 
            this.yAxis4.MajorDivisions.LabelFont = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // edmNormedErrorPlot
            // 
            this.edmNormedErrorPlot.LineColor = System.Drawing.Color.RoyalBlue;
            this.edmNormedErrorPlot.PointColor = System.Drawing.Color.Red;
            this.edmNormedErrorPlot.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.edmNormedErrorPlot.XAxis = this.xAxis4;
            this.edmNormedErrorPlot.YAxis = this.yAxis4;
            // 
            // sigSigmaHi
            // 
            this.sigSigmaHi.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.sigSigmaHi.PointColor = System.Drawing.Color.DodgerBlue;
            this.sigSigmaHi.PointSize = new System.Drawing.Size(5, 0);
            this.sigSigmaHi.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.sigSigmaHi.XAxis = this.xAxis1;
            this.sigSigmaHi.YAxis = this.yAxis1;
            // 
            // sigSigmaLo
            // 
            this.sigSigmaLo.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.sigSigmaLo.PointColor = System.Drawing.Color.DodgerBlue;
            this.sigSigmaLo.PointSize = new System.Drawing.Size(5, 0);
            this.sigSigmaLo.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.sigSigmaLo.XAxis = this.xAxis1;
            this.sigSigmaLo.YAxis = this.yAxis1;
            // 
            // bSigmaHi
            // 
            this.bSigmaHi.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.bSigmaHi.PointColor = System.Drawing.Color.DodgerBlue;
            this.bSigmaHi.PointSize = new System.Drawing.Size(5, 0);
            this.bSigmaHi.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.bSigmaHi.XAxis = this.xAxis2;
            this.bSigmaHi.YAxis = this.yAxis2;
            // 
            // bSigmaLo
            // 
            this.bSigmaLo.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.bSigmaLo.PointColor = System.Drawing.Color.DodgerBlue;
            this.bSigmaLo.PointSize = new System.Drawing.Size(5, 0);
            this.bSigmaLo.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.bSigmaLo.XAxis = this.xAxis2;
            this.bSigmaLo.YAxis = this.yAxis2;
            // 
            // dbSigmaHi
            // 
            this.dbSigmaHi.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.dbSigmaHi.PointColor = System.Drawing.Color.DodgerBlue;
            this.dbSigmaHi.PointSize = new System.Drawing.Size(5, 0);
            this.dbSigmaHi.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.dbSigmaHi.XAxis = this.xAxis3;
            this.dbSigmaHi.YAxis = this.yAxis3;
            // 
            // dbSigmaLo
            // 
            this.dbSigmaLo.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.dbSigmaLo.PointColor = System.Drawing.Color.DodgerBlue;
            this.dbSigmaLo.PointSize = new System.Drawing.Size(5, 0);
            this.dbSigmaLo.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.dbSigmaLo.XAxis = this.xAxis3;
            this.dbSigmaLo.YAxis = this.yAxis3;
            // 
            // LiveViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(886, 528);
            this.Controls.Add(this.edmErrorScatterGraph);
            this.Controls.Add(this.dbScatterGraph);
            this.Controls.Add(this.resetRunningMeans);
            this.Controls.Add(this.bScatterGraph);
            this.Controls.Add(this.sigScatterGraph);
            this.Controls.Add(this.clusterStatusText);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusText);
            this.MaximizeBox = false;
            this.Name = "LiveViewer";
            this.Text = "LiveViewer";
            ((System.ComponentModel.ISupportInitialize)(this.sigScatterGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bScatterGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbScatterGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.edmErrorScatterGraph)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox statusText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox clusterStatusText;
        private NationalInstruments.UI.WindowsForms.ScatterGraph sigScatterGraph;
        private NationalInstruments.UI.ScatterPlot sigPlot;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private NationalInstruments.UI.WindowsForms.ScatterGraph bScatterGraph;
        private NationalInstruments.UI.ScatterPlot bPlot;
        private NationalInstruments.UI.XAxis xAxis2;
        private NationalInstruments.UI.YAxis yAxis2;
        public System.Windows.Forms.Button resetRunningMeans;
        private NationalInstruments.UI.WindowsForms.ScatterGraph dbScatterGraph;
        private NationalInstruments.UI.ScatterPlot dbPlot;
        private NationalInstruments.UI.XAxis xAxis3;
        private NationalInstruments.UI.YAxis yAxis3;
        private NationalInstruments.UI.WindowsForms.ScatterGraph edmErrorScatterGraph;
        private NationalInstruments.UI.ScatterPlot edmErrorPlot;
        private NationalInstruments.UI.XAxis xAxis4;
        private NationalInstruments.UI.YAxis yAxis4;
        private NationalInstruments.UI.ScatterPlot edmNormedErrorPlot;
        private NationalInstruments.UI.ScatterPlot sigSigmaHi;
        private NationalInstruments.UI.ScatterPlot sigSigmaLo;
        private NationalInstruments.UI.ScatterPlot bSigmaHi;
        private NationalInstruments.UI.ScatterPlot bSigmaLo;
        private NationalInstruments.UI.ScatterPlot dbSigmaHi;
        private NationalInstruments.UI.ScatterPlot dbSigmaLo;

    }
}