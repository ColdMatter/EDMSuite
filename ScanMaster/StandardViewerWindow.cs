using System;
using System.Collections;
using System.Windows.Forms;

using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;

using Data;

namespace ScanMaster.GUI
{

	/// <summary>
	/// </summary>
	public class StandardViewerWindow : System.Windows.Forms.Form
	{

		// this windows associated Viewer
		private StandardViewer viewer;
		private System.ComponentModel.Container components = null;
		private NationalInstruments.UI.WindowsForms.ScatterGraph analog1Graph;
		private NationalInstruments.UI.XAxis xAxis1;
		private NationalInstruments.UI.YAxis yAxis1;
		private NationalInstruments.UI.WindowsForms.ScatterGraph analog2Graph;
		private NationalInstruments.UI.XAxis xAxis2;
		private NationalInstruments.UI.YAxis yAxis2;
		private System.Windows.Forms.StatusBar statusBar1;
		private NationalInstruments.UI.ScatterPlot analog1Plot;
		private NationalInstruments.UI.ScatterPlot analog2Plot;
		private NationalInstruments.UI.WindowsForms.ScatterGraph pmtGraph;
		private NationalInstruments.UI.ScatterPlot pmtOnPlot;
		private NationalInstruments.UI.XAxis pmtXAxis;
		private NationalInstruments.UI.YAxis pmtYAxis;
		private NationalInstruments.UI.XAxis xAxis3;
		private NationalInstruments.UI.ScatterPlot pmtOffPlot;
		private NationalInstruments.UI.XYCursor pmtLowCursor;
		private NationalInstruments.UI.XAxis xAxis5;
		private NationalInstruments.UI.WindowsForms.ScatterGraph differenceGraph;
		private NationalInstruments.UI.WindowsForms.WaveformGraph tofGraph;
		private NationalInstruments.UI.XAxis xAxis4;
		private NationalInstruments.UI.WaveformPlot tofOnPlot;
		private NationalInstruments.UI.WaveformPlot tofOffPlot;
		private NationalInstruments.UI.ScatterPlot differencePlot;
		private NationalInstruments.UI.ScatterPlot differenceAvgPlot;
		private NationalInstruments.UI.XYCursor tofLowCursor;
		private NationalInstruments.UI.XYCursor tofHighCursor;
		private NationalInstruments.UI.WaveformPlot tofOnAveragePlot;
		private NationalInstruments.UI.WaveformPlot tofOffAveragePlot;
		private NationalInstruments.UI.ScatterPlot pmtOnAvgPlot;
		private NationalInstruments.UI.ScatterPlot pmtOffAvgPlot;
		private NationalInstruments.UI.YAxis differenceYAxis;
		private NationalInstruments.UI.YAxis tofYAxis;
		private NationalInstruments.UI.YAxis tofAvgYAxis;
		private NationalInstruments.UI.ScatterPlot pmtFitPlot;
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.ComboBox tofFitFunctionCombo;
		private System.Windows.Forms.Label label2;
		public ComboBox tofFitModeCombo;
		public System.Windows.Forms.ComboBox spectrumFitFunctionCombo;
		public ComboBox spectrumFitModeCombo;
		public System.Windows.Forms.Label tofFitResultsLabel;
		public System.Windows.Forms.Label spectrumFitResultsLabel;
		private WaveformPlot tofFitPlot;
		private Button updateTOFFitButton;
        private Button updateSpectrumFitButton;
        public ComboBox tofFitDataSelectCombo;
		private NationalInstruments.UI.XYCursor pmtHighCursor;

		public StandardViewerWindow(StandardViewer viewer)
		{
			this.viewer = viewer;
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StandardViewerWindow));
            this.analog1Graph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.analog1Plot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.analog2Graph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.analog2Plot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis2 = new NationalInstruments.UI.XAxis();
            this.yAxis2 = new NationalInstruments.UI.YAxis();
            this.pmtGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.pmtLowCursor = new NationalInstruments.UI.XYCursor();
            this.pmtOnAvgPlot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis3 = new NationalInstruments.UI.XAxis();
            this.pmtYAxis = new NationalInstruments.UI.YAxis();
            this.pmtHighCursor = new NationalInstruments.UI.XYCursor();
            this.pmtOnPlot = new NationalInstruments.UI.ScatterPlot();
            this.pmtOffPlot = new NationalInstruments.UI.ScatterPlot();
            this.pmtOffAvgPlot = new NationalInstruments.UI.ScatterPlot();
            this.pmtFitPlot = new NationalInstruments.UI.ScatterPlot();
            this.pmtXAxis = new NationalInstruments.UI.XAxis();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.xAxis5 = new NationalInstruments.UI.XAxis();
            this.differenceYAxis = new NationalInstruments.UI.YAxis();
            this.differenceGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.differencePlot = new NationalInstruments.UI.ScatterPlot();
            this.differenceAvgPlot = new NationalInstruments.UI.ScatterPlot();
            this.tofGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.tofLowCursor = new NationalInstruments.UI.XYCursor();
            this.tofOnAveragePlot = new NationalInstruments.UI.WaveformPlot();
            this.xAxis4 = new NationalInstruments.UI.XAxis();
            this.tofAvgYAxis = new NationalInstruments.UI.YAxis();
            this.tofHighCursor = new NationalInstruments.UI.XYCursor();
            this.tofOnPlot = new NationalInstruments.UI.WaveformPlot();
            this.tofYAxis = new NationalInstruments.UI.YAxis();
            this.tofOffPlot = new NationalInstruments.UI.WaveformPlot();
            this.tofOffAveragePlot = new NationalInstruments.UI.WaveformPlot();
            this.tofFitPlot = new NationalInstruments.UI.WaveformPlot();
            this.tofFitModeCombo = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tofFitFunctionCombo = new System.Windows.Forms.ComboBox();
            this.spectrumFitFunctionCombo = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.spectrumFitModeCombo = new System.Windows.Forms.ComboBox();
            this.tofFitResultsLabel = new System.Windows.Forms.Label();
            this.spectrumFitResultsLabel = new System.Windows.Forms.Label();
            this.updateTOFFitButton = new System.Windows.Forms.Button();
            this.updateSpectrumFitButton = new System.Windows.Forms.Button();
            this.tofFitDataSelectCombo = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.analog1Graph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.analog2Graph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pmtGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pmtLowCursor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pmtHighCursor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.differenceGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofLowCursor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofHighCursor)).BeginInit();
            this.SuspendLayout();
            // 
            // analog1Graph
            // 
            this.analog1Graph.Location = new System.Drawing.Point(376, 8);
            this.analog1Graph.Name = "analog1Graph";
            this.analog1Graph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.analog1Plot});
            this.analog1Graph.Size = new System.Drawing.Size(584, 136);
            this.analog1Graph.TabIndex = 0;
            this.analog1Graph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.analog1Graph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // analog1Plot
            // 
            this.analog1Plot.LineColor = System.Drawing.Color.Red;
            this.analog1Plot.PointColor = System.Drawing.Color.MintCream;
            this.analog1Plot.XAxis = this.xAxis1;
            this.analog1Plot.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // analog2Graph
            // 
            this.analog2Graph.Location = new System.Drawing.Point(376, 152);
            this.analog2Graph.Name = "analog2Graph";
            this.analog2Graph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.analog2Plot});
            this.analog2Graph.Size = new System.Drawing.Size(584, 136);
            this.analog2Graph.TabIndex = 1;
            this.analog2Graph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis2});
            this.analog2Graph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis2});
            // 
            // analog2Plot
            // 
            this.analog2Plot.LineColor = System.Drawing.Color.Blue;
            this.analog2Plot.PointColor = System.Drawing.Color.Fuchsia;
            this.analog2Plot.XAxis = this.xAxis2;
            this.analog2Plot.YAxis = this.yAxis2;
            // 
            // xAxis2
            // 
            this.xAxis2.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // pmtGraph
            // 
            this.pmtGraph.Cursors.AddRange(new NationalInstruments.UI.XYCursor[] {
            this.pmtLowCursor,
            this.pmtHighCursor});
            this.pmtGraph.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY)
                        | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint)
                        | NationalInstruments.UI.GraphInteractionModes.PanX)
                        | NationalInstruments.UI.GraphInteractionModes.PanY)
                        | NationalInstruments.UI.GraphInteractionModes.DragCursor)
                        | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption)
                        | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.pmtGraph.Location = new System.Drawing.Point(376, 304);
            this.pmtGraph.Name = "pmtGraph";
            this.pmtGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.pmtOnPlot,
            this.pmtOffPlot,
            this.pmtOnAvgPlot,
            this.pmtOffAvgPlot,
            this.pmtFitPlot});
            this.pmtGraph.Size = new System.Drawing.Size(584, 280);
            this.pmtGraph.TabIndex = 9;
            this.pmtGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis3});
            this.pmtGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.pmtYAxis});
            // 
            // pmtLowCursor
            // 
            this.pmtLowCursor.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.pmtLowCursor.LabelVisible = true;
            this.pmtLowCursor.Plot = this.pmtOnAvgPlot;
            this.pmtLowCursor.SnapMode = NationalInstruments.UI.CursorSnapMode.NearestPoint;
            this.pmtLowCursor.AfterMove += new NationalInstruments.UI.AfterMoveXYCursorEventHandler(this.PMTCursorMoved);
            // 
            // pmtOnAvgPlot
            // 
            this.pmtOnAvgPlot.LineColor = System.Drawing.Color.Red;
            this.pmtOnAvgPlot.XAxis = this.xAxis3;
            this.pmtOnAvgPlot.YAxis = this.pmtYAxis;
            // 
            // xAxis3
            // 
            this.xAxis3.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // pmtHighCursor
            // 
            this.pmtHighCursor.Color = System.Drawing.Color.Lime;
            this.pmtHighCursor.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.pmtHighCursor.LabelVisible = true;
            this.pmtHighCursor.Plot = this.pmtOnAvgPlot;
            this.pmtHighCursor.SnapMode = NationalInstruments.UI.CursorSnapMode.NearestPoint;
            this.pmtHighCursor.AfterMove += new NationalInstruments.UI.AfterMoveXYCursorEventHandler(this.PMTCursorMoved);
            // 
            // pmtOnPlot
            // 
            this.pmtOnPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.pmtOnPlot.PointStyle = NationalInstruments.UI.PointStyle.Cross;
            this.pmtOnPlot.XAxis = this.xAxis3;
            this.pmtOnPlot.YAxis = this.pmtYAxis;
            // 
            // pmtOffPlot
            // 
            this.pmtOffPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.pmtOffPlot.PointColor = System.Drawing.Color.Magenta;
            this.pmtOffPlot.PointStyle = NationalInstruments.UI.PointStyle.Cross;
            this.pmtOffPlot.XAxis = this.xAxis3;
            this.pmtOffPlot.YAxis = this.pmtYAxis;
            // 
            // pmtOffAvgPlot
            // 
            this.pmtOffAvgPlot.LineColor = System.Drawing.Color.PowderBlue;
            this.pmtOffAvgPlot.XAxis = this.xAxis3;
            this.pmtOffAvgPlot.YAxis = this.pmtYAxis;
            // 
            // pmtFitPlot
            // 
            this.pmtFitPlot.LineColor = System.Drawing.Color.Silver;
            this.pmtFitPlot.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.pmtFitPlot.LineWidth = 2F;
            this.pmtFitPlot.XAxis = this.xAxis3;
            this.pmtFitPlot.YAxis = this.pmtYAxis;
            // 
            // pmtXAxis
            // 
            this.pmtXAxis.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 634);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(970, 20);
            this.statusBar1.SizingGrip = false;
            this.statusBar1.TabIndex = 13;
            this.statusBar1.Text = "Ready";
            // 
            // xAxis5
            // 
            this.xAxis5.CaptionBackColor = System.Drawing.SystemColors.ControlLight;
            this.xAxis5.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // differenceYAxis
            // 
            this.differenceYAxis.CaptionBackColor = System.Drawing.SystemColors.ControlLight;
            // 
            // differenceGraph
            // 
            this.differenceGraph.Location = new System.Drawing.Point(8, 304);
            this.differenceGraph.Name = "differenceGraph";
            this.differenceGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.differencePlot,
            this.differenceAvgPlot});
            this.differenceGraph.Size = new System.Drawing.Size(352, 280);
            this.differenceGraph.TabIndex = 15;
            this.differenceGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis5});
            this.differenceGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.differenceYAxis});
            // 
            // differencePlot
            // 
            this.differencePlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.differencePlot.PointColor = System.Drawing.Color.Lime;
            this.differencePlot.PointStyle = NationalInstruments.UI.PointStyle.Cross;
            this.differencePlot.XAxis = this.xAxis5;
            this.differencePlot.YAxis = this.differenceYAxis;
            // 
            // differenceAvgPlot
            // 
            this.differenceAvgPlot.LineColor = System.Drawing.Color.Red;
            this.differenceAvgPlot.XAxis = this.xAxis5;
            this.differenceAvgPlot.YAxis = this.differenceYAxis;
            // 
            // tofGraph
            // 
            this.tofGraph.Cursors.AddRange(new NationalInstruments.UI.XYCursor[] {
            this.tofLowCursor,
            this.tofHighCursor});
            this.tofGraph.Location = new System.Drawing.Point(8, 8);
            this.tofGraph.Name = "tofGraph";
            this.tofGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.tofOnPlot,
            this.tofOffPlot,
            this.tofOnAveragePlot,
            this.tofOffAveragePlot,
            this.tofFitPlot});
            this.tofGraph.Size = new System.Drawing.Size(352, 280);
            this.tofGraph.TabIndex = 16;
            this.tofGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis4});
            this.tofGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.tofYAxis,
            this.tofAvgYAxis});
            // 
            // tofLowCursor
            // 
            this.tofLowCursor.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.tofLowCursor.LabelVisible = true;
            this.tofLowCursor.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.tofLowCursor.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.tofLowCursor.Plot = this.tofOnAveragePlot;
            this.tofLowCursor.SnapMode = NationalInstruments.UI.CursorSnapMode.NearestPoint;
            this.tofLowCursor.AfterMove += new NationalInstruments.UI.AfterMoveXYCursorEventHandler(this.TOFCursorMoved);
            // 
            // tofOnAveragePlot
            // 
            this.tofOnAveragePlot.LineColor = System.Drawing.Color.Red;
            this.tofOnAveragePlot.XAxis = this.xAxis4;
            this.tofOnAveragePlot.YAxis = this.tofAvgYAxis;
            // 
            // tofAvgYAxis
            // 
            this.tofAvgYAxis.Position = NationalInstruments.UI.YAxisPosition.Right;
            // 
            // tofHighCursor
            // 
            this.tofHighCursor.Color = System.Drawing.Color.Lime;
            this.tofHighCursor.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.tofHighCursor.LabelVisible = true;
            this.tofHighCursor.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.tofHighCursor.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.tofHighCursor.Plot = this.tofOnAveragePlot;
            this.tofHighCursor.SnapMode = NationalInstruments.UI.CursorSnapMode.NearestPoint;
            this.tofHighCursor.AfterMove += new NationalInstruments.UI.AfterMoveXYCursorEventHandler(this.TOFCursorMoved);
            // 
            // tofOnPlot
            // 
            this.tofOnPlot.LineColor = System.Drawing.Color.Blue;
            this.tofOnPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.tofOnPlot.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.tofOnPlot.XAxis = this.xAxis4;
            this.tofOnPlot.YAxis = this.tofYAxis;
            // 
            // tofOffPlot
            // 
            this.tofOffPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.tofOffPlot.PointColor = System.Drawing.Color.LawnGreen;
            this.tofOffPlot.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.tofOffPlot.XAxis = this.xAxis4;
            this.tofOffPlot.YAxis = this.tofYAxis;
            // 
            // tofOffAveragePlot
            // 
            this.tofOffAveragePlot.LineColor = System.Drawing.Color.PowderBlue;
            this.tofOffAveragePlot.XAxis = this.xAxis4;
            this.tofOffAveragePlot.YAxis = this.tofAvgYAxis;
            // 
            // tofFitPlot
            // 
            this.tofFitPlot.LineColor = System.Drawing.Color.Silver;
            this.tofFitPlot.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.tofFitPlot.LineWidth = 2F;
            this.tofFitPlot.XAxis = this.xAxis4;
            this.tofFitPlot.YAxis = this.tofAvgYAxis;
            // 
            // tofFitModeCombo
            // 
            this.tofFitModeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tofFitModeCombo.Items.AddRange(new object[] {
            "off",
            "average"});
            this.tofFitModeCombo.Location = new System.Drawing.Point(64, 605);
            this.tofFitModeCombo.Name = "tofFitModeCombo";
            this.tofFitModeCombo.Size = new System.Drawing.Size(72, 21);
            this.tofFitModeCombo.TabIndex = 17;
            this.tofFitModeCombo.SelectedIndexChanged += new System.EventHandler(this.tofFitModeCombo_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(6, 584);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 23);
            this.label1.TabIndex = 18;
            this.label1.Text = "Fit TOF:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tofFitFunctionCombo
            // 
            this.tofFitFunctionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tofFitFunctionCombo.Location = new System.Drawing.Point(142, 605);
            this.tofFitFunctionCombo.Name = "tofFitFunctionCombo";
            this.tofFitFunctionCombo.Size = new System.Drawing.Size(88, 21);
            this.tofFitFunctionCombo.TabIndex = 19;
            this.tofFitFunctionCombo.SelectedIndexChanged += new System.EventHandler(this.tofFitFunctionCombo_SelectedIndexChanged);
            // 
            // spectrumFitFunctionCombo
            // 
            this.spectrumFitFunctionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.spectrumFitFunctionCombo.Location = new System.Drawing.Point(555, 605);
            this.spectrumFitFunctionCombo.Name = "spectrumFitFunctionCombo";
            this.spectrumFitFunctionCombo.Size = new System.Drawing.Size(88, 21);
            this.spectrumFitFunctionCombo.TabIndex = 22;
            this.spectrumFitFunctionCombo.SelectedIndexChanged += new System.EventHandler(this.spectrumFitFunctionCombo_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(474, 587);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 15);
            this.label2.TabIndex = 21;
            this.label2.Text = "Fit spectrum:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // spectrumFitModeCombo
            // 
            this.spectrumFitModeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.spectrumFitModeCombo.Items.AddRange(new object[] {
            "off",
            "average",
            "shot"});
            this.spectrumFitModeCombo.Location = new System.Drawing.Point(477, 605);
            this.spectrumFitModeCombo.Name = "spectrumFitModeCombo";
            this.spectrumFitModeCombo.Size = new System.Drawing.Size(72, 21);
            this.spectrumFitModeCombo.TabIndex = 20;
            this.spectrumFitModeCombo.SelectedIndexChanged += new System.EventHandler(this.spectrumFitModeCombo_SelectedIndexChanged);
            // 
            // tofFitResultsLabel
            // 
            this.tofFitResultsLabel.ForeColor = System.Drawing.Color.Blue;
            this.tofFitResultsLabel.Location = new System.Drawing.Point(260, 602);
            this.tofFitResultsLabel.Name = "tofFitResultsLabel";
            this.tofFitResultsLabel.Size = new System.Drawing.Size(206, 24);
            this.tofFitResultsLabel.TabIndex = 23;
            this.tofFitResultsLabel.Text = "...";
            this.tofFitResultsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // spectrumFitResultsLabel
            // 
            this.spectrumFitResultsLabel.ForeColor = System.Drawing.Color.Blue;
            this.spectrumFitResultsLabel.Location = new System.Drawing.Point(673, 602);
            this.spectrumFitResultsLabel.Name = "spectrumFitResultsLabel";
            this.spectrumFitResultsLabel.Size = new System.Drawing.Size(210, 24);
            this.spectrumFitResultsLabel.TabIndex = 24;
            this.spectrumFitResultsLabel.Text = "...";
            this.spectrumFitResultsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // updateTOFFitButton
            // 
            this.updateTOFFitButton.Location = new System.Drawing.Point(236, 603);
            this.updateTOFFitButton.Name = "updateTOFFitButton";
            this.updateTOFFitButton.Size = new System.Drawing.Size(18, 23);
            this.updateTOFFitButton.TabIndex = 25;
            this.updateTOFFitButton.Text = ">";
            this.updateTOFFitButton.UseVisualStyleBackColor = true;
            this.updateTOFFitButton.Click += new System.EventHandler(this.updateTOFFitButton_Click);
            // 
            // updateSpectrumFitButton
            // 
            this.updateSpectrumFitButton.Location = new System.Drawing.Point(649, 605);
            this.updateSpectrumFitButton.Name = "updateSpectrumFitButton";
            this.updateSpectrumFitButton.Size = new System.Drawing.Size(18, 23);
            this.updateSpectrumFitButton.TabIndex = 26;
            this.updateSpectrumFitButton.Text = ">";
            this.updateSpectrumFitButton.UseVisualStyleBackColor = true;
            this.updateSpectrumFitButton.Click += new System.EventHandler(this.updateSpectrumFitButton_Click);
            // 
            // tofFitDataSelectCombo
            // 
            this.tofFitDataSelectCombo.FormattingEnabled = true;
            this.tofFitDataSelectCombo.Items.AddRange(new object[] {
            "On",
            "Off"});
            this.tofFitDataSelectCombo.Location = new System.Drawing.Point(8, 605);
            this.tofFitDataSelectCombo.Name = "tofFitDataSelectCombo";
            this.tofFitDataSelectCombo.Size = new System.Drawing.Size(50, 21);
            this.tofFitDataSelectCombo.TabIndex = 27;
            // 
            // StandardViewerWindow
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(970, 654);
            this.Controls.Add(this.tofFitDataSelectCombo);
            this.Controls.Add(this.updateSpectrumFitButton);
            this.Controls.Add(this.updateTOFFitButton);
            this.Controls.Add(this.spectrumFitResultsLabel);
            this.Controls.Add(this.tofFitResultsLabel);
            this.Controls.Add(this.spectrumFitFunctionCombo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.spectrumFitModeCombo);
            this.Controls.Add(this.tofFitFunctionCombo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tofFitModeCombo);
            this.Controls.Add(this.tofGraph);
            this.Controls.Add(this.differenceGraph);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.pmtGraph);
            this.Controls.Add(this.analog2Graph);
            this.Controls.Add(this.analog1Graph);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "StandardViewerWindow";
            this.Text = "Standard View";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.WindowClosing);
            ((System.ComponentModel.ISupportInitialize)(this.analog1Graph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.analog2Graph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pmtGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pmtLowCursor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pmtHighCursor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.differenceGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofLowCursor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofHighCursor)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion


		private void TOFCursorMoved(object sender, NationalInstruments.UI.AfterMoveXYCursorEventArgs e)
		{
			viewer.TOFCursorMoved();
		}

		private void PMTCursorMoved(object sender, NationalInstruments.UI.AfterMoveXYCursorEventArgs e)
		{
			viewer.PMTCursorMoved();
		}
		private void tofFitModeCombo_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			viewer.TOFFitModeChanged(((ComboBox)sender).SelectedIndex);
		}

		private void tofFitFunctionCombo_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			viewer.TOFFitFunctionChanged(((ComboBox)sender).SelectedItem);
		}

		private void spectrumFitModeCombo_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			viewer.SpectrumFitModeChanged(((ComboBox)sender).SelectedIndex);
		}

		private void spectrumFitFunctionCombo_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			viewer.SpectrumFitFunctionChanged(((ComboBox)sender).SelectedItem);
		}

		private void updateTOFFitButton_Click(object sender, EventArgs e)
		{
			viewer.UpdateTOFFit();
		}

		private void updateSpectrumFitButton_Click(object sender, EventArgs e)
		{
			viewer.UpdateSpectrumFit();
		}


		// these functions and properties are all thread safe
		public void ClearAll()
		{
			ClearNIGraph(analog1Graph);
			ClearNIGraph(analog2Graph);
			ClearNIGraph(pmtGraph);
			ClearNIGraph(tofGraph);
			ClearNIGraph(differenceGraph);
		}
		public void ClearSpectra()
		{
			ClearNIGraph(analog1Graph);
			ClearNIGraph(analog2Graph);
			ClearNIGraph(pmtGraph);
			ClearNIGraph(differenceGraph);
		}

		public void ClearRealtimeSpectra()
		{
			ClearNIPlot(pmtGraph, pmtOnPlot);
			ClearNIPlot(pmtGraph, pmtOffPlot);
			ClearNIPlot(differenceGraph, differencePlot);
			ClearNIPlot(analog1Graph, analog1Plot);
			ClearNIPlot(analog2Graph, analog2Plot);
		}

		public void ClearRealtimeNotAnalog()
		{
			ClearNIPlot(tofGraph, tofOnPlot);
			ClearNIPlot(tofGraph, tofOffPlot);
			ClearNIPlot(pmtGraph, pmtOnPlot);
			ClearNIPlot(pmtGraph, pmtOffPlot);
			ClearNIPlot(differenceGraph, differencePlot);
		}

		public void ClearSpectrumFit()
		{
			ClearNIPlot(pmtGraph, pmtFitPlot);
		}

		public Range SpectrumAxes
		{
			set
			{
				SetGraphXAxisRange(pmtGraph, value.Minimum, value.Maximum);
				SetGraphXAxisRange(differenceGraph, value.Minimum, value.Maximum);
				SetGraphXAxisRange(analog1Graph, value.Minimum, value.Maximum);
				SetGraphXAxisRange(analog2Graph, value.Minimum, value.Maximum);
			}
		}

		public Range SpectrumGate
		{
			set
			{
				MoveCursor(pmtGraph, pmtLowCursor, value.Minimum);
				MoveCursor(pmtGraph, pmtHighCursor, value.Maximum);
			}
			get
			{
				double min = GetCursorPosition(pmtGraph, pmtLowCursor);
				double max = GetCursorPosition(pmtGraph, pmtHighCursor);
				if (max <= min) max = min + 1; //highly arbitrary
				return new Range(min, max);
			}
		}
		public Range TOFGate
		{
			set
			{
				MoveCursor(tofGraph, tofLowCursor, value.Minimum);
				MoveCursor(tofGraph, tofHighCursor, value.Maximum);
			}
			get
			{
				return new Range(GetCursorPosition(tofGraph, tofLowCursor),
					GetCursorPosition(tofGraph, tofHighCursor));
			}
		}

		public void PlotOnTOF(TOF t) { PlotY(tofGraph, tofOnPlot, t.GateStartTime, t.ClockPeriod, t.Data); }
		public void PlotOffTOF(TOF t) { PlotY(tofGraph, tofOffPlot, t.GateStartTime, t.ClockPeriod, t.Data); }
		public void PlotAverageOnTOF(TOF t)
		{
			PlotY(tofGraph, tofOnAveragePlot, t.GateStartTime, t.ClockPeriod, t.Data);
		}
		public void PlotAverageOffTOF(TOF t)
		{
			PlotY(tofGraph, tofOffAveragePlot, t.GateStartTime, t.ClockPeriod, t.Data);
		}
		public void AppendToAnalog1(double[] x, double[] y)
		{
			PlotXYAppend(analog1Graph, analog1Plot, x, y);
		}

		public void AppendToAnalog2(double[] x, double[] y)
		{
			PlotXYAppend(analog2Graph, analog2Plot, x, y);
		}
		public void AppendToPMTOn(double[] x, double[] y)
		{
			PlotXYAppend(pmtGraph, pmtOnPlot, x, y);
		}
		public void AppendToPMTOff(double[] x, double[] y)
		{
			PlotXYAppend(pmtGraph, pmtOffPlot, x, y);
		}
		public void AppendToDifference(double[] x, double[] y)
		{
			PlotXYAppend(differenceGraph, differencePlot, x, y);
		}

		public void PlotAveragePMTOn(double[] x, double[] y)
		{
			PlotXY(pmtGraph, pmtOnAvgPlot, x, y);
		}
		public void PlotAveragePMTOff(double[] x, double[] y)
		{
			PlotXY(pmtGraph, pmtOffAvgPlot, x, y);
		}
		public void PlotAverageDifference(double[] x, double[] y)
		{
			PlotXY(differenceGraph, differenceAvgPlot, x, y);
		}
		public void PlotSpectrumFit(double[] x, double[] y)
		{
			PlotXY(pmtGraph, pmtFitPlot, x, y);
		}

		public void ClearTOFFit()
		{
			ClearNIPlot(tofGraph, tofFitPlot);
		}

		public void PlotTOFFit(int start, int period, double[] data)
		{
			PlotY(tofGraph, tofFitPlot, start, period, data);
		}

		// UI delegates and thread-safe helpers
		private delegate void ClearDataDelegate();
		private void ClearNIGraph(Graph graph)
		{
			graph.Invoke(new ClearDataDelegate(graph.ClearData));
		}
		private void ClearNIPlot(Graph graph, Plot plot)
		{
			graph.Invoke(new ClearDataDelegate(plot.ClearData));
		}
		private void SetGraphXAxisRangeHelper(XYGraph graph, double start, double end)
		{
			graph.XAxes[0].Range = new Range(start, end);
		}
		private delegate void SetGraphXAxisRangeDelegate(XYGraph graph, double start, double end);
		private void SetGraphXAxisRange(XYGraph graph, double start, double end)
		{
			graph.Invoke(new SetGraphXAxisRangeDelegate(SetGraphXAxisRangeHelper),
				new Object[] { graph, start, end });
		}
		private delegate void PlotXYDelegate(double[] x, double[] y);
		private void PlotXYAppend(Graph graph, ScatterPlot plot, double[] x, double[] y)
		{
			graph.Invoke(new PlotXYDelegate(plot.PlotXYAppend), new Object[] { x, y });
		}
		private void PlotXY(Graph graph, ScatterPlot plot, double[] x, double[] y)
		{
			graph.Invoke(new PlotXYDelegate(plot.PlotXY), new Object[] { x, y });
		}
		private delegate void PlotYDelegate(double[] yData, double start, double inc);
		private void PlotY(Graph graph, WaveformPlot p, double start, double inc, double[] ydata)
		{
			graph.Invoke(new PlotYDelegate(p.PlotY), new Object[] { ydata, start, inc });
		}

		private void MoveCursorHelper(XYCursor cursor, double x)
		{
			cursor.XPosition = x;
		}
		private delegate void MoveCursorDelegate(XYCursor cursor, double x);
		private void MoveCursor(Graph graph, XYCursor cursor, double x)
		{
			graph.Invoke(new MoveCursorDelegate(MoveCursorHelper), new Object[] { cursor, x });
		}

		private delegate double GetCursorPositionDelegate(XYCursor cursor);
		private double GetCursorPositionHelper(XYCursor cursor)
		{
			return cursor.XPosition;
		}
		private double GetCursorPosition(Graph graph, XYCursor cursor)
		{
			double x = (double)graph.Invoke(new GetCursorPositionDelegate(GetCursorPositionHelper),
				new Object[] { cursor });
			return x;
		}

		public void SetLabel(Label label, string text)
		{
			label.Invoke(new SetLabelDelegate(SetLabelHelper), new object[] { label, text });
		}
		private delegate void SetLabelDelegate(Label label, string text);
		private void SetLabelHelper(Label label, string text)
		{
			label.Text = text;
		}

        private void TofFitComboHelper(bool state)
        {
            tofFitFunctionCombo.Enabled = state;
        }
        private delegate void ComboStateDelegate(bool state);
        public void SetTofFitFunctionComboState(bool state)
        {
            tofFitFunctionCombo.Invoke(new ComboStateDelegate(TofFitComboHelper), state);
        }

        private int TofFitDataSelectHelper()
        {
            return tofFitDataSelectCombo.SelectedIndex;
        }
        private delegate int TofFitDataDelegate();
        public int GetTofFitDataSelection()
        {
            return (int) tofFitDataSelectCombo.Invoke(new TofFitDataDelegate(TofFitDataSelectHelper));
        }

        private void SpectrumFitComboHelper(bool state)
        {
            spectrumFitFunctionCombo.Enabled = state;
        }
        public void SetSpectrumFitFunctionComboState(bool state)
        {
            spectrumFitFunctionCombo.Invoke(new ComboStateDelegate(SpectrumFitComboHelper), state);
        }

		private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			viewer.ToggleVisible();
			e.Cancel = true;
		}
	}
}
