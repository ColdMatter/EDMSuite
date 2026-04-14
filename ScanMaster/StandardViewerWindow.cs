using System;
using System.Collections;
using System.Windows.Forms;

using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using ScanMaster.Acquire.Plugin;

using Data;

namespace ScanMaster.GUI
{

	/// <summary>
	/// </summary>
	public class StandardViewerWindow : System.Windows.Forms.Form
	{

		// this windows associated Viewer
		private StandardViewer viewer;
        private System.ComponentModel.IContainer components;
        private NationalInstruments.UI.WindowsForms.ScatterGraph analog1Graph;
		private NationalInstruments.UI.XAxis xAxis1;
		private NationalInstruments.UI.YAxis yAxis1;
		private NationalInstruments.UI.WindowsForms.ScatterGraph analog2Graph;
		private NationalInstruments.UI.XAxis xAxis2;
        private NationalInstruments.UI.YAxis yAxis2;
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
		private NationalInstruments.UI.WindowsForms.ScatterGraph ratioGraph;
		private NationalInstruments.UI.WindowsForms.WaveformGraph tofGraph;
		private NationalInstruments.UI.XAxis xAxis4;
		private NationalInstruments.UI.WaveformPlot tofOnPlot;
		private NationalInstruments.UI.WaveformPlot tofOffPlot;
		private NationalInstruments.UI.XYCursor tofLowCursor;
		private NationalInstruments.UI.XYCursor tofHighCursor;
		private NationalInstruments.UI.WaveformPlot tofOnAveragePlot;
		private NationalInstruments.UI.WaveformPlot tofOffAveragePlot;
		private NationalInstruments.UI.ScatterPlot pmtOnAvgPlot;
		private NationalInstruments.UI.ScatterPlot pmtOffAvgPlot;
		private NationalInstruments.UI.YAxis ratioYAxis;
		private NationalInstruments.UI.YAxis tofYAxis;
		private NationalInstruments.UI.YAxis tofAvgYAxis;
		private NationalInstruments.UI.ScatterPlot pmtFitOffAvgPlot;
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.ComboBox tofFitFunctionCombo;
		public ComboBox tofFitModeCombo;
		public System.Windows.Forms.ComboBox spectrumFitFunctionCombo;
		public ComboBox spectrumFitModeCombo;
		public System.Windows.Forms.Label tofFitResultsLabel;
		public System.Windows.Forms.Label spectrumFitResultsLabelOffShots;
		private WaveformPlot tofFitPlot;
		private Button updateTOFFitButton;
        private Button updateSpectrumFitOffShotsButton;
        public ComboBox tofFitDataSelectCombo;
        private SplitContainer splitContainer1;
        private StatusBar statusBar1;
        private Label label3;
        private Button updateNoiseResultsbutton;
        public Label noiseResultsLabel;
        private SplitContainer splitContainer2;
        private StatusBar statusBar2;
        private Button defaultGateButton;
        private WaveformGraph tofGraph2;
        private XYCursor normSigGateLow;
        private WaveformPlot tofOnAveragePlot2;
        private XAxis xAxis6;
        private YAxis yAxis3;
        private XYCursor normSigGateHigh;
        private WaveformPlot tofOnPlot2;
        private YAxis yAxis4;
        private WaveformPlot tofOffPlot2;
        private WaveformPlot tofOffAveragePlot2;
        private WaveformPlot tofFitPlot2;
        private WaveformGraph tofGraphNormed;
        private XYCursor xyCursor3;
        private WaveformPlot tofOnAverageNormedPlot;
        private XAxis xAxis7;
        private YAxis yAxis5;
        private XYCursor xyCursor4;
        private WaveformPlot tofOnNormedPlot;
        private YAxis yAxis6;
        private WaveformPlot tofOffNormedPlot;
        private WaveformPlot tofOffAverageNormedPlot;
        private WaveformPlot tofFitNormedPlot;
        private ToolStripLabel toolStripLabel1;
        private ToolStripPropertyEditor toolStripPropertyEditor1;
        private ToolStripLabel toolStripLabel2;
        private ToolStripPropertyEditor toolStripPropertyEditor2;
        private ToolStripLabel toolStripLabel3;
        private ToolStripPropertyEditor toolStripPropertyEditor3;
        private XYCursor normBgGateLow;
        private XYCursor normBgGateHigh;
        private ScatterPlot ratioPlot;
        private ScatterPlot ratioAvgPlot;
        private ScatterGraph differenceGraph;
        private XAxis xAxis8;
        private YAxis differenceYAxis;
        private ScatterPlot differencePlot;
        private ScatterPlot differenceAvgPlot;
        private InstrumentControlStrip instrumentControlStrip1;
        private ToolStripLabel toolStripLabel4;
        private ToolStripPropertyEditor toolStripPropertyEditor4;
        private ToolStripLabel toolStripLabel5;
        private ToolStripPropertyEditor toolStripPropertyEditor5;
        private ToolStripLabel toolStripLabel6;
        private ToolStripPropertyEditor toolStripPropertyEditor6;
        private ToolStripLabel toolStripLabel7;
        private ToolStripPropertyEditor toolStripPropertyEditor7;
        private Label label5;
        private Label label4;
        private Label label6;
        private XYCursor ratioCursor1;
        private XYCursor differenceCursor;
        private ContextMenuStrip contextMenuStrip1;
        public Label label7;
        private ScatterPlot pmtFitOnAvgPlot;
        private Label label8;
        private Label label9;
        private Label label10;
        private Button updateSpectrumFitOnShotsButton;
        public Label spectrumFitResultsLabelOnShots;
        private Label label12;
        private ScatterPlot ratioFitPlot2;
        private Button button1;
        public Label ratioFitResults;
        private Button button2;
        private Button buttonDisplaySettingsOfInterest;
        public Label labelPGsetingsOfInterest;
        private Label label2;
        private Label label11;
        private Label label13;
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
            this.components = new System.ComponentModel.Container();
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
            this.pmtFitOffAvgPlot = new NationalInstruments.UI.ScatterPlot();
            this.pmtFitOnAvgPlot = new NationalInstruments.UI.ScatterPlot();
            this.pmtXAxis = new NationalInstruments.UI.XAxis();
            this.xAxis5 = new NationalInstruments.UI.XAxis();
            this.ratioYAxis = new NationalInstruments.UI.YAxis();
            this.ratioGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.ratioCursor1 = new NationalInstruments.UI.XYCursor();
            this.ratioPlot = new NationalInstruments.UI.ScatterPlot();
            this.ratioAvgPlot = new NationalInstruments.UI.ScatterPlot();
            this.ratioFitPlot2 = new NationalInstruments.UI.ScatterPlot();
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
            this.spectrumFitModeCombo = new System.Windows.Forms.ComboBox();
            this.tofFitResultsLabel = new System.Windows.Forms.Label();
            this.spectrumFitResultsLabelOffShots = new System.Windows.Forms.Label();
            this.updateTOFFitButton = new System.Windows.Forms.Button();
            this.updateSpectrumFitOffShotsButton = new System.Windows.Forms.Button();
            this.tofFitDataSelectCombo = new System.Windows.Forms.ComboBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.statusBar2 = new System.Windows.Forms.StatusBar();
            this.defaultGateButton = new System.Windows.Forms.Button();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.label3 = new System.Windows.Forms.Label();
            this.updateNoiseResultsbutton = new System.Windows.Forms.Button();
            this.noiseResultsLabel = new System.Windows.Forms.Label();
            this.tofGraph2 = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.normSigGateLow = new NationalInstruments.UI.XYCursor();
            this.tofOnAveragePlot2 = new NationalInstruments.UI.WaveformPlot();
            this.xAxis6 = new NationalInstruments.UI.XAxis();
            this.yAxis3 = new NationalInstruments.UI.YAxis();
            this.normSigGateHigh = new NationalInstruments.UI.XYCursor();
            this.normBgGateLow = new NationalInstruments.UI.XYCursor();
            this.tofOnPlot2 = new NationalInstruments.UI.WaveformPlot();
            this.yAxis4 = new NationalInstruments.UI.YAxis();
            this.normBgGateHigh = new NationalInstruments.UI.XYCursor();
            this.tofOffPlot2 = new NationalInstruments.UI.WaveformPlot();
            this.tofOffAveragePlot2 = new NationalInstruments.UI.WaveformPlot();
            this.tofFitPlot2 = new NationalInstruments.UI.WaveformPlot();
            this.tofGraphNormed = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.xyCursor3 = new NationalInstruments.UI.XYCursor();
            this.tofOnAverageNormedPlot = new NationalInstruments.UI.WaveformPlot();
            this.xAxis7 = new NationalInstruments.UI.XAxis();
            this.yAxis5 = new NationalInstruments.UI.YAxis();
            this.xyCursor4 = new NationalInstruments.UI.XYCursor();
            this.tofOnNormedPlot = new NationalInstruments.UI.WaveformPlot();
            this.yAxis6 = new NationalInstruments.UI.YAxis();
            this.tofOffNormedPlot = new NationalInstruments.UI.WaveformPlot();
            this.tofOffAverageNormedPlot = new NationalInstruments.UI.WaveformPlot();
            this.tofFitNormedPlot = new NationalInstruments.UI.WaveformPlot();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripPropertyEditor1 = new NationalInstruments.UI.WindowsForms.ToolStripPropertyEditor();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripPropertyEditor2 = new NationalInstruments.UI.WindowsForms.ToolStripPropertyEditor();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripPropertyEditor3 = new NationalInstruments.UI.WindowsForms.ToolStripPropertyEditor();
            this.differenceGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.differenceCursor = new NationalInstruments.UI.XYCursor();
            this.differencePlot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis8 = new NationalInstruments.UI.XAxis();
            this.differenceYAxis = new NationalInstruments.UI.YAxis();
            this.differenceAvgPlot = new NationalInstruments.UI.ScatterPlot();
            this.instrumentControlStrip1 = new NationalInstruments.UI.WindowsForms.InstrumentControlStrip();
            this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel7 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripPropertyEditor7 = new NationalInstruments.UI.WindowsForms.ToolStripPropertyEditor();
            this.toolStripPropertyEditor4 = new NationalInstruments.UI.WindowsForms.ToolStripPropertyEditor();
            this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripPropertyEditor5 = new NationalInstruments.UI.WindowsForms.ToolStripPropertyEditor();
            this.toolStripLabel6 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripPropertyEditor6 = new NationalInstruments.UI.WindowsForms.ToolStripPropertyEditor();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.updateSpectrumFitOnShotsButton = new System.Windows.Forms.Button();
            this.spectrumFitResultsLabelOnShots = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.ratioFitResults = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.buttonDisplaySettingsOfInterest = new System.Windows.Forms.Button();
            this.labelPGsetingsOfInterest = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.analog1Graph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.analog2Graph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pmtGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pmtLowCursor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pmtHighCursor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ratioGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ratioCursor1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofLowCursor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofHighCursor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.normSigGateLow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.normSigGateHigh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.normBgGateLow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.normBgGateHigh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraphNormed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.differenceGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.differenceCursor)).BeginInit();
            this.instrumentControlStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // analog1Graph
            // 
            this.analog1Graph.Location = new System.Drawing.Point(376, 21);
            this.analog1Graph.Name = "analog1Graph";
            this.analog1Graph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.analog1Plot});
            this.analog1Graph.Size = new System.Drawing.Size(626, 126);
            this.analog1Graph.TabIndex = 0;
            this.analog1Graph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.analog1Graph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            this.analog1Graph.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.analog1Graph_PlotDataChanged);
            // 
            // analog1Plot
            // 
            this.analog1Plot.AntiAliased = true;
            this.analog1Plot.LineColor = System.Drawing.Color.Red;
            this.analog1Plot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.analog1Plot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.analog1Plot.PointColor = System.Drawing.Color.Red;
            this.analog1Plot.PointStyle = NationalInstruments.UI.PointStyle.SolidCircle;
            this.analog1Plot.ToolTipXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.analog1Plot.ToolTipYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.analog1Plot.XAxis = this.xAxis1;
            this.analog1Plot.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // yAxis1
            // 
            this.yAxis1.EditRangeNumericFormatMode = NationalInstruments.UI.NumericFormatMode.CreateGenericMode("G3");
            this.yAxis1.MajorDivisions.LabelFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G9");
            // 
            // analog2Graph
            // 
            this.analog2Graph.Location = new System.Drawing.Point(376, 149);
            this.analog2Graph.Name = "analog2Graph";
            this.analog2Graph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.analog2Plot});
            this.analog2Graph.Size = new System.Drawing.Size(626, 125);
            this.analog2Graph.TabIndex = 1;
            this.analog2Graph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis2});
            this.analog2Graph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis2});
            // 
            // analog2Plot
            // 
            this.analog2Plot.AntiAliased = true;
            this.analog2Plot.LineColor = System.Drawing.Color.Blue;
            this.analog2Plot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.analog2Plot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.analog2Plot.PointColor = System.Drawing.Color.Blue;
            this.analog2Plot.PointStyle = NationalInstruments.UI.PointStyle.SolidDiamond;
            this.analog2Plot.XAxis = this.xAxis2;
            this.analog2Plot.YAxis = this.yAxis2;
            // 
            // xAxis2
            // 
            this.xAxis2.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // yAxis2
            // 
            this.yAxis2.EditRangeNumericFormatMode = NationalInstruments.UI.NumericFormatMode.CreateGenericMode("G9");
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
            this.pmtGraph.Location = new System.Drawing.Point(375, 599);
            this.pmtGraph.Name = "pmtGraph";
            this.pmtGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.pmtOnPlot,
            this.pmtOffPlot,
            this.pmtOnAvgPlot,
            this.pmtOffAvgPlot,
            this.pmtFitOffAvgPlot,
            this.pmtFitOnAvgPlot});
            this.pmtGraph.Size = new System.Drawing.Size(627, 232);
            this.pmtGraph.TabIndex = 9;
            this.pmtGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis3});
            this.pmtGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.pmtYAxis});
            this.pmtGraph.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.pmtGraph_PlotDataChanged);
            // 
            // pmtLowCursor
            // 
            this.pmtLowCursor.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.pmtLowCursor.LabelVisible = true;
            this.pmtLowCursor.Plot = this.pmtOnAvgPlot;
            this.pmtLowCursor.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            this.pmtLowCursor.AfterMove += new NationalInstruments.UI.AfterMoveXYCursorEventHandler(this.PMTCursorMoved);
            // 
            // pmtOnAvgPlot
            // 
            this.pmtOnAvgPlot.LineColor = System.Drawing.Color.Red;
            this.pmtOnAvgPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
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
            this.pmtHighCursor.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
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
            this.pmtOffPlot.PointColor = System.Drawing.Color.LawnGreen;
            this.pmtOffPlot.PointStyle = NationalInstruments.UI.PointStyle.Cross;
            this.pmtOffPlot.XAxis = this.xAxis3;
            this.pmtOffPlot.YAxis = this.pmtYAxis;
            // 
            // pmtOffAvgPlot
            // 
            this.pmtOffAvgPlot.LineColor = System.Drawing.Color.PowderBlue;
            this.pmtOffAvgPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.pmtOffAvgPlot.XAxis = this.xAxis3;
            this.pmtOffAvgPlot.YAxis = this.pmtYAxis;
            // 
            // pmtFitOffAvgPlot
            // 
            this.pmtFitOffAvgPlot.LineColor = System.Drawing.Color.Silver;
            this.pmtFitOffAvgPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.pmtFitOffAvgPlot.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.pmtFitOffAvgPlot.LineWidth = 2F;
            this.pmtFitOffAvgPlot.XAxis = this.xAxis3;
            this.pmtFitOffAvgPlot.YAxis = this.pmtYAxis;
            // 
            // pmtFitOnAvgPlot
            // 
            this.pmtFitOnAvgPlot.LineColor = System.Drawing.Color.Red;
            this.pmtFitOnAvgPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.pmtFitOnAvgPlot.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.pmtFitOnAvgPlot.LineWidth = 2F;
            this.pmtFitOnAvgPlot.XAxis = this.xAxis3;
            this.pmtFitOnAvgPlot.YAxis = this.pmtYAxis;
            // 
            // pmtXAxis
            // 
            this.pmtXAxis.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // xAxis5
            // 
            this.xAxis5.CaptionBackColor = System.Drawing.SystemColors.ControlLight;
            this.xAxis5.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // ratioYAxis
            // 
            this.ratioYAxis.CaptionBackColor = System.Drawing.SystemColors.ControlLight;
            // 
            // ratioGraph
            // 
            this.ratioGraph.Cursors.AddRange(new NationalInstruments.UI.XYCursor[] {
            this.ratioCursor1});
            this.ratioGraph.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.ratioGraph.Location = new System.Drawing.Point(711, 321);
            this.ratioGraph.Name = "ratioGraph";
            this.ratioGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.ratioPlot,
            this.ratioAvgPlot,
            this.ratioFitPlot2});
            this.ratioGraph.Size = new System.Drawing.Size(288, 231);
            this.ratioGraph.TabIndex = 15;
            this.ratioGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis5});
            this.ratioGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.ratioYAxis});
            this.ratioGraph.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.differenceGraph_PlotDataChanged);
            // 
            // ratioCursor1
            // 
            this.ratioCursor1.Color = System.Drawing.Color.Yellow;
            this.ratioCursor1.LabelVisible = true;
            this.ratioCursor1.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.ratioCursor1.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.ratioCursor1.Plot = this.ratioPlot;
            this.ratioCursor1.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            this.ratioCursor1.XPosition = 0D;
            this.ratioCursor1.YPosition = 0.5D;
            // 
            // ratioPlot
            // 
            this.ratioPlot.LineColor = System.Drawing.Color.Orange;
            this.ratioPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.ratioPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.ratioPlot.PointColor = System.Drawing.Color.Orange;
            this.ratioPlot.PointStyle = NationalInstruments.UI.PointStyle.Cross;
            this.ratioPlot.XAxis = this.xAxis5;
            this.ratioPlot.YAxis = this.ratioYAxis;
            // 
            // ratioAvgPlot
            // 
            this.ratioAvgPlot.LineColor = System.Drawing.Color.Orange;
            this.ratioAvgPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.ratioAvgPlot.XAxis = this.xAxis5;
            this.ratioAvgPlot.YAxis = this.ratioYAxis;
            // 
            // ratioFitPlot2
            // 
            this.ratioFitPlot2.LineColor = System.Drawing.Color.Orange;
            this.ratioFitPlot2.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.ratioFitPlot2.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.ratioFitPlot2.LineWidth = 2F;
            this.ratioFitPlot2.XAxis = this.xAxis5;
            this.ratioFitPlot2.YAxis = this.ratioYAxis;
            // 
            // tofGraph
            // 
            this.tofGraph.Cursors.AddRange(new NationalInstruments.UI.XYCursor[] {
            this.tofLowCursor,
            this.tofHighCursor});
            this.tofGraph.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.tofGraph.Location = new System.Drawing.Point(8, 22);
            this.tofGraph.Name = "tofGraph";
            this.tofGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.tofOnPlot,
            this.tofOffPlot,
            this.tofOnAveragePlot,
            this.tofOffAveragePlot,
            this.tofFitPlot});
            this.tofGraph.Size = new System.Drawing.Size(362, 255);
            this.tofGraph.TabIndex = 16;
            this.tofGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis4});
            this.tofGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.tofYAxis,
            this.tofAvgYAxis});
            this.tofGraph.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.tofGraph_PlotDataChanged);
            // 
            // tofLowCursor
            // 
            this.tofLowCursor.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.tofLowCursor.LabelVisible = true;
            this.tofLowCursor.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.tofLowCursor.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.tofLowCursor.Plot = this.tofOnAveragePlot;
            this.tofLowCursor.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            this.tofLowCursor.AfterMove += new NationalInstruments.UI.AfterMoveXYCursorEventHandler(this.TOFCursorMoved);
            // 
            // tofOnAveragePlot
            // 
            this.tofOnAveragePlot.LineColor = System.Drawing.Color.Red;
            this.tofOnAveragePlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOnAveragePlot.XAxis = this.xAxis4;
            this.tofOnAveragePlot.YAxis = this.tofAvgYAxis;
            // 
            // xAxis4
            // 
            this.xAxis4.Mode = NationalInstruments.UI.AxisMode.Fixed;
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
            this.tofHighCursor.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            this.tofHighCursor.AfterMove += new NationalInstruments.UI.AfterMoveXYCursorEventHandler(this.TOFCursorMoved);
            // 
            // tofOnPlot
            // 
            this.tofOnPlot.LineColor = System.Drawing.Color.Blue;
            this.tofOnPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
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
            this.tofOffAveragePlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOffAveragePlot.XAxis = this.xAxis4;
            this.tofOffAveragePlot.YAxis = this.tofAvgYAxis;
            // 
            // tofFitPlot
            // 
            this.tofFitPlot.LineColor = System.Drawing.Color.Silver;
            this.tofFitPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
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
            this.tofFitModeCombo.Location = new System.Drawing.Point(64, 857);
            this.tofFitModeCombo.Name = "tofFitModeCombo";
            this.tofFitModeCombo.Size = new System.Drawing.Size(72, 21);
            this.tofFitModeCombo.TabIndex = 17;
            this.tofFitModeCombo.SelectedIndexChanged += new System.EventHandler(this.tofFitModeCombo_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(6, 789);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 23);
            this.label1.TabIndex = 18;
            this.label1.Text = "Fit TOF:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tofFitFunctionCombo
            // 
            this.tofFitFunctionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tofFitFunctionCombo.Location = new System.Drawing.Point(142, 857);
            this.tofFitFunctionCombo.Name = "tofFitFunctionCombo";
            this.tofFitFunctionCombo.Size = new System.Drawing.Size(88, 21);
            this.tofFitFunctionCombo.TabIndex = 19;
            this.tofFitFunctionCombo.SelectedIndexChanged += new System.EventHandler(this.tofFitFunctionCombo_SelectedIndexChanged);
            // 
            // spectrumFitFunctionCombo
            // 
            this.spectrumFitFunctionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.spectrumFitFunctionCombo.Location = new System.Drawing.Point(606, 835);
            this.spectrumFitFunctionCombo.Name = "spectrumFitFunctionCombo";
            this.spectrumFitFunctionCombo.Size = new System.Drawing.Size(100, 21);
            this.spectrumFitFunctionCombo.TabIndex = 22;
            this.spectrumFitFunctionCombo.SelectedIndexChanged += new System.EventHandler(this.spectrumFitFunctionCombo_SelectedIndexChanged);
            // 
            // spectrumFitModeCombo
            // 
            this.spectrumFitModeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.spectrumFitModeCombo.Items.AddRange(new object[] {
            "none",
            "average",
            "shot"});
            this.spectrumFitModeCombo.Location = new System.Drawing.Point(884, 836);
            this.spectrumFitModeCombo.Name = "spectrumFitModeCombo";
            this.spectrumFitModeCombo.Size = new System.Drawing.Size(112, 21);
            this.spectrumFitModeCombo.TabIndex = 20;
            this.spectrumFitModeCombo.SelectedIndexChanged += new System.EventHandler(this.spectrumFitModeCombo_SelectedIndexChanged);
            // 
            // tofFitResultsLabel
            // 
            this.tofFitResultsLabel.ForeColor = System.Drawing.Color.Blue;
            this.tofFitResultsLabel.Location = new System.Drawing.Point(260, 854);
            this.tofFitResultsLabel.Name = "tofFitResultsLabel";
            this.tofFitResultsLabel.Size = new System.Drawing.Size(100, 24);
            this.tofFitResultsLabel.TabIndex = 23;
            this.tofFitResultsLabel.Text = "...";
            this.tofFitResultsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // spectrumFitResultsLabelOffShots
            // 
            this.spectrumFitResultsLabelOffShots.ForeColor = System.Drawing.Color.Blue;
            this.spectrumFitResultsLabelOffShots.Location = new System.Drawing.Point(603, 862);
            this.spectrumFitResultsLabelOffShots.Name = "spectrumFitResultsLabelOffShots";
            this.spectrumFitResultsLabelOffShots.Size = new System.Drawing.Size(303, 24);
            this.spectrumFitResultsLabelOffShots.TabIndex = 24;
            this.spectrumFitResultsLabelOffShots.Text = "...";
            this.spectrumFitResultsLabelOffShots.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // updateTOFFitButton
            // 
            this.updateTOFFitButton.Location = new System.Drawing.Point(236, 855);
            this.updateTOFFitButton.Name = "updateTOFFitButton";
            this.updateTOFFitButton.Size = new System.Drawing.Size(18, 23);
            this.updateTOFFitButton.TabIndex = 25;
            this.updateTOFFitButton.Text = ">";
            this.updateTOFFitButton.UseVisualStyleBackColor = true;
            this.updateTOFFitButton.Click += new System.EventHandler(this.updateTOFFitButton_Click);
            // 
            // updateSpectrumFitOffShotsButton
            // 
            this.updateSpectrumFitOffShotsButton.Location = new System.Drawing.Point(481, 862);
            this.updateSpectrumFitOffShotsButton.Name = "updateSpectrumFitOffShotsButton";
            this.updateSpectrumFitOffShotsButton.Size = new System.Drawing.Size(116, 23);
            this.updateSpectrumFitOffShotsButton.TabIndex = 26;
            this.updateSpectrumFitOffShotsButton.Text = "Fit off-shot spectrum:";
            this.updateSpectrumFitOffShotsButton.UseVisualStyleBackColor = true;
            this.updateSpectrumFitOffShotsButton.Click += new System.EventHandler(this.updateSpectrumFitOffShotsButton_Click);
            // 
            // tofFitDataSelectCombo
            // 
            this.tofFitDataSelectCombo.FormattingEnabled = true;
            this.tofFitDataSelectCombo.Items.AddRange(new object[] {
            "On",
            "Off"});
            this.tofFitDataSelectCombo.Location = new System.Drawing.Point(8, 810);
            this.tofFitDataSelectCombo.Name = "tofFitDataSelectCombo";
            this.tofFitDataSelectCombo.Size = new System.Drawing.Size(50, 21);
            this.tofFitDataSelectCombo.TabIndex = 27;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitContainer1.Location = new System.Drawing.Point(0, 957);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.statusBar1);
            this.splitContainer1.Size = new System.Drawing.Size(1167, 23);
            this.splitContainer1.SplitterDistance = 444;
            this.splitContainer1.TabIndex = 30;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.statusBar2);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.defaultGateButton);
            this.splitContainer2.Size = new System.Drawing.Size(444, 23);
            this.splitContainer2.SplitterDistance = 280;
            this.splitContainer2.TabIndex = 0;
            // 
            // statusBar2
            // 
            this.statusBar2.Location = new System.Drawing.Point(0, 0);
            this.statusBar2.Name = "statusBar2";
            this.statusBar2.Size = new System.Drawing.Size(280, 23);
            this.statusBar2.SizingGrip = false;
            this.statusBar2.TabIndex = 32;
            this.statusBar2.Text = "Ready";
            // 
            // defaultGateButton
            // 
            this.defaultGateButton.Location = new System.Drawing.Point(3, 0);
            this.defaultGateButton.Name = "defaultGateButton";
            this.defaultGateButton.Size = new System.Drawing.Size(120, 23);
            this.defaultGateButton.TabIndex = 26;
            this.defaultGateButton.Text = "Default Gate";
            this.defaultGateButton.UseVisualStyleBackColor = true;
            this.defaultGateButton.Click += new System.EventHandler(this.defaultGateButton_Click);
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 0);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(719, 23);
            this.statusBar1.SizingGrip = false;
            this.statusBar1.TabIndex = 14;
            this.statusBar1.Text = "Ready";
            this.statusBar1.PanelClick += new System.Windows.Forms.StatusBarPanelClickEventHandler(this.statusBar1_PanelClick);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(2, 935);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 13);
            this.label3.TabIndex = 31;
            this.label3.Text = "Factor over shot noise:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // updateNoiseResultsbutton
            // 
            this.updateNoiseResultsbutton.Location = new System.Drawing.Point(118, 931);
            this.updateNoiseResultsbutton.Name = "updateNoiseResultsbutton";
            this.updateNoiseResultsbutton.Size = new System.Drawing.Size(18, 24);
            this.updateNoiseResultsbutton.TabIndex = 32;
            this.updateNoiseResultsbutton.Text = ">";
            this.updateNoiseResultsbutton.UseVisualStyleBackColor = true;
            this.updateNoiseResultsbutton.Click += new System.EventHandler(this.updateNoiseResultsbutton_Click);
            // 
            // noiseResultsLabel
            // 
            this.noiseResultsLabel.ForeColor = System.Drawing.Color.Blue;
            this.noiseResultsLabel.Location = new System.Drawing.Point(139, 926);
            this.noiseResultsLabel.Name = "noiseResultsLabel";
            this.noiseResultsLabel.Size = new System.Drawing.Size(230, 25);
            this.noiseResultsLabel.TabIndex = 33;
            this.noiseResultsLabel.Text = "...";
            this.noiseResultsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tofGraph2
            // 
            this.tofGraph2.Cursors.AddRange(new NationalInstruments.UI.XYCursor[] {
            this.normSigGateLow,
            this.normSigGateHigh,
            this.normBgGateLow,
            this.normBgGateHigh});
            this.tofGraph2.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.tofGraph2.Location = new System.Drawing.Point(8, 296);
            this.tofGraph2.Name = "tofGraph2";
            this.tofGraph2.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.tofOnPlot2,
            this.tofOffPlot2,
            this.tofOnAveragePlot2,
            this.tofOffAveragePlot2,
            this.tofFitPlot2});
            this.tofGraph2.Size = new System.Drawing.Size(361, 255);
            this.tofGraph2.TabIndex = 34;
            this.tofGraph2.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis6});
            this.tofGraph2.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis4,
            this.yAxis3});
            this.tofGraph2.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.tofGraph2_PlotDataChanged);
            // 
            // normSigGateLow
            // 
            this.normSigGateLow.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.normSigGateLow.LabelVisible = true;
            this.normSigGateLow.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.normSigGateLow.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.normSigGateLow.Plot = this.tofOnAveragePlot2;
            this.normSigGateLow.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // tofOnAveragePlot2
            // 
            this.tofOnAveragePlot2.LineColor = System.Drawing.Color.Red;
            this.tofOnAveragePlot2.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOnAveragePlot2.XAxis = this.xAxis6;
            this.tofOnAveragePlot2.YAxis = this.yAxis3;
            // 
            // xAxis6
            // 
            this.xAxis6.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // yAxis3
            // 
            this.yAxis3.Position = NationalInstruments.UI.YAxisPosition.Right;
            // 
            // normSigGateHigh
            // 
            this.normSigGateHigh.Color = System.Drawing.Color.Lime;
            this.normSigGateHigh.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.normSigGateHigh.LabelVisible = true;
            this.normSigGateHigh.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.normSigGateHigh.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.normSigGateHigh.Plot = this.tofOnAveragePlot2;
            this.normSigGateHigh.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // normBgGateLow
            // 
            this.normBgGateLow.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.normBgGateLow.LabelVisible = true;
            this.normBgGateLow.LineStyle = NationalInstruments.UI.LineStyle.Dot;
            this.normBgGateLow.Plot = this.tofOnPlot2;
            this.normBgGateLow.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // tofOnPlot2
            // 
            this.tofOnPlot2.LineColor = System.Drawing.Color.Blue;
            this.tofOnPlot2.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOnPlot2.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.tofOnPlot2.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.tofOnPlot2.XAxis = this.xAxis6;
            this.tofOnPlot2.YAxis = this.yAxis4;
            // 
            // normBgGateHigh
            // 
            this.normBgGateHigh.Color = System.Drawing.Color.Lime;
            this.normBgGateHigh.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.normBgGateHigh.LabelVisible = true;
            this.normBgGateHigh.LineStyle = NationalInstruments.UI.LineStyle.Dot;
            this.normBgGateHigh.Plot = this.tofOnPlot2;
            this.normBgGateHigh.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // tofOffPlot2
            // 
            this.tofOffPlot2.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.tofOffPlot2.PointColor = System.Drawing.Color.LawnGreen;
            this.tofOffPlot2.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.tofOffPlot2.XAxis = this.xAxis6;
            this.tofOffPlot2.YAxis = this.yAxis4;
            // 
            // tofOffAveragePlot2
            // 
            this.tofOffAveragePlot2.LineColor = System.Drawing.Color.PowderBlue;
            this.tofOffAveragePlot2.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOffAveragePlot2.XAxis = this.xAxis6;
            this.tofOffAveragePlot2.YAxis = this.yAxis3;
            // 
            // tofFitPlot2
            // 
            this.tofFitPlot2.LineColor = System.Drawing.Color.Silver;
            this.tofFitPlot2.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofFitPlot2.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.tofFitPlot2.LineWidth = 2F;
            this.tofFitPlot2.XAxis = this.xAxis6;
            this.tofFitPlot2.YAxis = this.yAxis3;
            // 
            // tofGraphNormed
            // 
            this.tofGraphNormed.Cursors.AddRange(new NationalInstruments.UI.XYCursor[] {
            this.xyCursor3,
            this.xyCursor4});
            this.tofGraphNormed.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.tofGraphNormed.Location = new System.Drawing.Point(8, 577);
            this.tofGraphNormed.Name = "tofGraphNormed";
            this.tofGraphNormed.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.tofOnNormedPlot,
            this.tofOffNormedPlot,
            this.tofOnAverageNormedPlot,
            this.tofOffAverageNormedPlot,
            this.tofFitNormedPlot});
            this.tofGraphNormed.Size = new System.Drawing.Size(361, 255);
            this.tofGraphNormed.TabIndex = 35;
            this.tofGraphNormed.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis7});
            this.tofGraphNormed.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis6,
            this.yAxis5});
            // 
            // xyCursor3
            // 
            this.xyCursor3.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor3.LabelVisible = true;
            this.xyCursor3.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor3.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor3.Plot = this.tofOnAverageNormedPlot;
            this.xyCursor3.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // tofOnAverageNormedPlot
            // 
            this.tofOnAverageNormedPlot.LineColor = System.Drawing.Color.Red;
            this.tofOnAverageNormedPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOnAverageNormedPlot.XAxis = this.xAxis7;
            this.tofOnAverageNormedPlot.YAxis = this.yAxis5;
            // 
            // xAxis7
            // 
            this.xAxis7.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // yAxis5
            // 
            this.yAxis5.Position = NationalInstruments.UI.YAxisPosition.Right;
            // 
            // xyCursor4
            // 
            this.xyCursor4.Color = System.Drawing.Color.Lime;
            this.xyCursor4.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor4.LabelVisible = true;
            this.xyCursor4.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor4.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor4.Plot = this.tofOnAverageNormedPlot;
            this.xyCursor4.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // tofOnNormedPlot
            // 
            this.tofOnNormedPlot.LineColor = System.Drawing.Color.Blue;
            this.tofOnNormedPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOnNormedPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.tofOnNormedPlot.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.tofOnNormedPlot.XAxis = this.xAxis7;
            this.tofOnNormedPlot.YAxis = this.yAxis6;
            // 
            // tofOffNormedPlot
            // 
            this.tofOffNormedPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.tofOffNormedPlot.PointColor = System.Drawing.Color.LawnGreen;
            this.tofOffNormedPlot.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.tofOffNormedPlot.XAxis = this.xAxis7;
            this.tofOffNormedPlot.YAxis = this.yAxis6;
            // 
            // tofOffAverageNormedPlot
            // 
            this.tofOffAverageNormedPlot.LineColor = System.Drawing.Color.PowderBlue;
            this.tofOffAverageNormedPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOffAverageNormedPlot.XAxis = this.xAxis7;
            this.tofOffAverageNormedPlot.YAxis = this.yAxis5;
            // 
            // tofFitNormedPlot
            // 
            this.tofFitNormedPlot.LineColor = System.Drawing.Color.Silver;
            this.tofFitNormedPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofFitNormedPlot.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.tofFitNormedPlot.LineWidth = 2F;
            this.tofFitNormedPlot.XAxis = this.xAxis7;
            this.tofFitNormedPlot.YAxis = this.yAxis5;
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(98, 22);
            this.toolStripLabel1.Text = "InteractionMode:";
            // 
            // toolStripPropertyEditor1
            // 
            this.toolStripPropertyEditor1.AutoSize = false;
            this.toolStripPropertyEditor1.Name = "toolStripPropertyEditor1";
            this.toolStripPropertyEditor1.RenderMode = NationalInstruments.UI.PropertyEditorRenderMode.Inherit;
            this.toolStripPropertyEditor1.Size = new System.Drawing.Size(120, 23);
            this.toolStripPropertyEditor1.Source = new NationalInstruments.UI.PropertyEditorSource(this.tofGraph, "InteractionMode");
            this.toolStripPropertyEditor1.Text = "ZoomX, ZoomY, ZoomAroundPoint, PanX, PanY, DragCursor, DragAnnotationCaption, Edi" +
    "tRange";
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(23, 23);
            // 
            // toolStripPropertyEditor2
            // 
            this.toolStripPropertyEditor2.AutoSize = false;
            this.toolStripPropertyEditor2.Name = "toolStripPropertyEditor2";
            this.toolStripPropertyEditor2.RenderMode = NationalInstruments.UI.PropertyEditorRenderMode.Inherit;
            this.toolStripPropertyEditor2.Size = new System.Drawing.Size(120, 20);
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(75, 15);
            this.toolStripLabel3.Text = "Annotations:";
            // 
            // toolStripPropertyEditor3
            // 
            this.toolStripPropertyEditor3.AutoSize = false;
            this.toolStripPropertyEditor3.Name = "toolStripPropertyEditor3";
            this.toolStripPropertyEditor3.RenderMode = NationalInstruments.UI.PropertyEditorRenderMode.Inherit;
            this.toolStripPropertyEditor3.Size = new System.Drawing.Size(120, 23);
            this.toolStripPropertyEditor3.Source = new NationalInstruments.UI.PropertyEditorSource(this.tofGraph, "Annotations");
            this.toolStripPropertyEditor3.Text = "(Collection)";
            // 
            // differenceGraph
            // 
            this.differenceGraph.Cursors.AddRange(new NationalInstruments.UI.XYCursor[] {
            this.differenceCursor});
            this.differenceGraph.ImmediateUpdates = true;
            this.differenceGraph.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.differenceGraph.Location = new System.Drawing.Point(375, 320);
            this.differenceGraph.Name = "differenceGraph";
            this.differenceGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.differencePlot,
            this.differenceAvgPlot});
            this.differenceGraph.Size = new System.Drawing.Size(291, 231);
            this.differenceGraph.TabIndex = 36;
            this.differenceGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis8});
            this.differenceGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.differenceYAxis});
            this.differenceGraph.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.differenceGraph_PlotDataChanged_1);
            // 
            // differenceCursor
            // 
            this.differenceCursor.Color = System.Drawing.Color.Thistle;
            this.differenceCursor.LabelVisible = true;
            this.differenceCursor.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.differenceCursor.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.differenceCursor.Plot = this.differencePlot;
            this.differenceCursor.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            this.differenceCursor.XPosition = 0D;
            this.differenceCursor.YPosition = -20D;
            // 
            // differencePlot
            // 
            this.differencePlot.LineColor = System.Drawing.Color.Orange;
            this.differencePlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.differencePlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.differencePlot.PointColor = System.Drawing.Color.Magenta;
            this.differencePlot.PointStyle = NationalInstruments.UI.PointStyle.Cross;
            this.differencePlot.XAxis = this.xAxis8;
            this.differencePlot.YAxis = this.differenceYAxis;
            // 
            // xAxis8
            // 
            this.xAxis8.CaptionBackColor = System.Drawing.SystemColors.ControlLight;
            this.xAxis8.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // differenceYAxis
            // 
            this.differenceYAxis.CaptionBackColor = System.Drawing.SystemColors.ControlLight;
            this.differenceYAxis.Range = new NationalInstruments.UI.Range(-40D, 10D);
            // 
            // differenceAvgPlot
            // 
            this.differenceAvgPlot.LineColor = System.Drawing.Color.Magenta;
            this.differenceAvgPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.differenceAvgPlot.XAxis = this.xAxis8;
            this.differenceAvgPlot.YAxis = this.differenceYAxis;
            // 
            // instrumentControlStrip1
            // 
            this.instrumentControlStrip1.AutoSize = false;
            this.instrumentControlStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.instrumentControlStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel4,
            this.toolStripLabel7,
            this.toolStripPropertyEditor7,
            this.toolStripPropertyEditor4,
            this.toolStripLabel5,
            this.toolStripPropertyEditor5,
            this.toolStripLabel6,
            this.toolStripPropertyEditor6});
            this.instrumentControlStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.instrumentControlStrip1.Location = new System.Drawing.Point(389, 927);
            this.instrumentControlStrip1.Name = "instrumentControlStrip1";
            this.instrumentControlStrip1.Size = new System.Drawing.Size(291, 26);
            this.instrumentControlStrip1.TabIndex = 37;
            // 
            // toolStripLabel4
            // 
            this.toolStripLabel4.Name = "toolStripLabel4";
            this.toolStripLabel4.Size = new System.Drawing.Size(98, 23);
            this.toolStripLabel4.Text = "InteractionMode:";
            // 
            // toolStripLabel7
            // 
            this.toolStripLabel7.Name = "toolStripLabel7";
            this.toolStripLabel7.Size = new System.Drawing.Size(36, 23);
            this.toolStripLabel7.Text = "Plots:";
            // 
            // toolStripPropertyEditor7
            // 
            this.toolStripPropertyEditor7.AutoSize = false;
            this.toolStripPropertyEditor7.Name = "toolStripPropertyEditor7";
            this.toolStripPropertyEditor7.RenderMode = NationalInstruments.UI.PropertyEditorRenderMode.Inherit;
            this.toolStripPropertyEditor7.Size = new System.Drawing.Size(120, 23);
            this.toolStripPropertyEditor7.Source = new NationalInstruments.UI.PropertyEditorSource(this.differenceGraph, "Plots");
            this.toolStripPropertyEditor7.Text = "(Collection)";
            this.toolStripPropertyEditor7.SourceValueChanged += new System.EventHandler(this.toolStripPropertyEditor7_SourceValueChanged);
            // 
            // toolStripPropertyEditor4
            // 
            this.toolStripPropertyEditor4.AutoSize = false;
            this.toolStripPropertyEditor4.Name = "toolStripPropertyEditor4";
            this.toolStripPropertyEditor4.RenderMode = NationalInstruments.UI.PropertyEditorRenderMode.Inherit;
            this.toolStripPropertyEditor4.Size = new System.Drawing.Size(120, 23);
            this.toolStripPropertyEditor4.Source = new NationalInstruments.UI.PropertyEditorSource(this.differenceGraph, "InteractionMode");
            this.toolStripPropertyEditor4.Text = "ZoomX, ZoomY, ZoomAroundPoint, PanX, PanY, DragCursor, DragAnnotationCaption, Edi" +
    "tRange";
            // 
            // toolStripLabel5
            // 
            this.toolStripLabel5.Name = "toolStripLabel5";
            this.toolStripLabel5.Size = new System.Drawing.Size(50, 15);
            this.toolStripLabel5.Text = "Cursors:";
            // 
            // toolStripPropertyEditor5
            // 
            this.toolStripPropertyEditor5.AutoSize = false;
            this.toolStripPropertyEditor5.Name = "toolStripPropertyEditor5";
            this.toolStripPropertyEditor5.RenderMode = NationalInstruments.UI.PropertyEditorRenderMode.Inherit;
            this.toolStripPropertyEditor5.Size = new System.Drawing.Size(120, 23);
            this.toolStripPropertyEditor5.Source = new NationalInstruments.UI.PropertyEditorSource(this.differenceGraph, "Cursors");
            this.toolStripPropertyEditor5.Text = "(Collection)";
            // 
            // toolStripLabel6
            // 
            this.toolStripLabel6.Name = "toolStripLabel6";
            this.toolStripLabel6.Size = new System.Drawing.Size(75, 15);
            this.toolStripLabel6.Text = "Annotations:";
            // 
            // toolStripPropertyEditor6
            // 
            this.toolStripPropertyEditor6.AutoSize = false;
            this.toolStripPropertyEditor6.Name = "toolStripPropertyEditor6";
            this.toolStripPropertyEditor6.RenderMode = NationalInstruments.UI.PropertyEditorRenderMode.Inherit;
            this.toolStripPropertyEditor6.Size = new System.Drawing.Size(120, 23);
            this.toolStripPropertyEditor6.Source = new NationalInstruments.UI.PropertyEditorSource(this.differenceGraph, "Annotations");
            this.toolStripPropertyEditor6.Text = "(Collection)";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(708, 294);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(203, 23);
            this.label5.TabIndex = 39;
            this.label5.Text = "(signalOn - bgOn) / (signalOff - bgOff)";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(414, 294);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(202, 23);
            this.label4.TabIndex = 40;
            this.label4.Text = "(signalOn - bgOn) - (singalOff - bgOff)";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label4.Click += new System.EventHandler(this.label4_Click_1);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(401, 573);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(440, 23);
            this.label6.TabIndex = 41;
            this.label6.Text = "Spectrum (gated TOF): signalOn-bgOn (red), signalOff-bgOff (blue)";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(868, 578);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(84, 13);
            this.label7.TabIndex = 44;
            this.label7.Text = "Final scan num: ";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(800, 837);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(78, 20);
            this.label8.TabIndex = 45;
            this.label8.Text = "update mode:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label8.Click += new System.EventHandler(this.label8_Click);
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(387, 835);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(85, 23);
            this.label9.TabIndex = 46;
            this.label9.Text = "Fit spectrum:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(2, 904);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(994, 24);
            this.label10.TabIndex = 47;
            this.label10.Text = resources.GetString("label10.Text");
            this.label10.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // updateSpectrumFitOnShotsButton
            // 
            this.updateSpectrumFitOnShotsButton.Location = new System.Drawing.Point(481, 885);
            this.updateSpectrumFitOnShotsButton.Name = "updateSpectrumFitOnShotsButton";
            this.updateSpectrumFitOnShotsButton.Size = new System.Drawing.Size(116, 23);
            this.updateSpectrumFitOnShotsButton.TabIndex = 50;
            this.updateSpectrumFitOnShotsButton.Text = "Fit on-shot spectrum:";
            this.updateSpectrumFitOnShotsButton.UseVisualStyleBackColor = true;
            this.updateSpectrumFitOnShotsButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // spectrumFitResultsLabelOnShots
            // 
            this.spectrumFitResultsLabelOnShots.ForeColor = System.Drawing.Color.Blue;
            this.spectrumFitResultsLabelOnShots.Location = new System.Drawing.Point(603, 884);
            this.spectrumFitResultsLabelOnShots.Name = "spectrumFitResultsLabelOnShots";
            this.spectrumFitResultsLabelOnShots.Size = new System.Drawing.Size(303, 24);
            this.spectrumFitResultsLabelOnShots.TabIndex = 51;
            this.spectrumFitResultsLabelOnShots.Text = "...";
            this.spectrumFitResultsLabelOnShots.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(481, 834);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(116, 20);
            this.label12.TabIndex = 52;
            this.label12.Text = "fitting functions:";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1005, 292);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(150, 34);
            this.button1.TabIndex = 53;
            this.button1.Text = "Fit exponential to the ratio:";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // ratioFitResults
            // 
            this.ratioFitResults.ForeColor = System.Drawing.Color.Blue;
            this.ratioFitResults.Location = new System.Drawing.Point(1005, 367);
            this.ratioFitResults.Name = "ratioFitResults";
            this.ratioFitResults.Size = new System.Drawing.Size(92, 65);
            this.ratioFitResults.TabIndex = 54;
            this.ratioFitResults.Text = "...";
            this.ratioFitResults.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ratioFitResults.Click += new System.EventHandler(this.ratioFitResults_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(1005, 332);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(150, 23);
            this.button2.TabIndex = 55;
            this.button2.Text = "Clear the fit:";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // buttonDisplaySettingsOfInterest
            // 
            this.buttonDisplaySettingsOfInterest.Location = new System.Drawing.Point(1005, 599);
            this.buttonDisplaySettingsOfInterest.Name = "buttonDisplaySettingsOfInterest";
            this.buttonDisplaySettingsOfInterest.Size = new System.Drawing.Size(157, 49);
            this.buttonDisplaySettingsOfInterest.TabIndex = 56;
            this.buttonDisplaySettingsOfInterest.Text = "Display pg settings of interest";
            this.buttonDisplaySettingsOfInterest.UseVisualStyleBackColor = true;
            this.buttonDisplaySettingsOfInterest.Click += new System.EventHandler(this.buttonDisplaySettingsOfInterest_Click);
            // 
            // labelPGsetingsOfInterest
            // 
            this.labelPGsetingsOfInterest.ForeColor = System.Drawing.Color.Blue;
            this.labelPGsetingsOfInterest.Location = new System.Drawing.Point(1005, 651);
            this.labelPGsetingsOfInterest.Name = "labelPGsetingsOfInterest";
            this.labelPGsetingsOfInterest.Size = new System.Drawing.Size(150, 181);
            this.labelPGsetingsOfInterest.TabIndex = 57;
            this.labelPGsetingsOfInterest.Text = "...";
            this.labelPGsetingsOfInterest.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(7, 554);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(388, 23);
            this.label2.TabIndex = 58;
            this.label2.Text = "TOF combined: Ch1 YgOn-YgOff (movingAveraged over 100 samples)";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label2.Click += new System.EventHandler(this.label2_Click_1);
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(8, 271);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(85, 23);
            this.label11.TabIndex = 59;
            this.label11.Text = "TOF, CH2:";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label13
            // 
            this.label13.Location = new System.Drawing.Point(9, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(85, 23);
            this.label13.TabIndex = 60;
            this.label13.Text = "TOF, CH1:";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label13.Click += new System.EventHandler(this.label13_Click);
            // 
            // StandardViewerWindow
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1167, 980);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelPGsetingsOfInterest);
            this.Controls.Add(this.buttonDisplaySettingsOfInterest);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.ratioFitResults);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.spectrumFitResultsLabelOnShots);
            this.Controls.Add(this.updateSpectrumFitOnShotsButton);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.differenceGraph);
            this.Controls.Add(this.tofGraphNormed);
            this.Controls.Add(this.tofGraph2);
            this.Controls.Add(this.noiseResultsLabel);
            this.Controls.Add(this.updateNoiseResultsbutton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.tofFitDataSelectCombo);
            this.Controls.Add(this.updateSpectrumFitOffShotsButton);
            this.Controls.Add(this.updateTOFFitButton);
            this.Controls.Add(this.spectrumFitResultsLabelOffShots);
            this.Controls.Add(this.tofFitResultsLabel);
            this.Controls.Add(this.spectrumFitFunctionCombo);
            this.Controls.Add(this.spectrumFitModeCombo);
            this.Controls.Add(this.tofFitFunctionCombo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tofFitModeCombo);
            this.Controls.Add(this.tofGraph);
            this.Controls.Add(this.ratioGraph);
            this.Controls.Add(this.pmtGraph);
            this.Controls.Add(this.analog2Graph);
            this.Controls.Add(this.analog1Graph);
            this.Controls.Add(this.instrumentControlStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "StandardViewerWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Standard View";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.WindowClosing);
            this.Load += new System.EventHandler(this.StandardViewerWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.analog1Graph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.analog2Graph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pmtGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pmtLowCursor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pmtHighCursor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ratioGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ratioCursor1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofLowCursor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofHighCursor)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.normSigGateLow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.normSigGateHigh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.normBgGateLow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.normBgGateHigh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraphNormed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.differenceGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.differenceCursor)).EndInit();
            this.instrumentControlStrip1.ResumeLayout(false);
            this.instrumentControlStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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

		private void updateSpectrumFitOffShotsButton_Click(object sender, EventArgs e)
		{
			viewer.UpdateSpectrumFitOffShots();
		}


		// these functions and properties are all thread safe
		public void ClearAll()
		{
			ClearNIGraph(analog1Graph);
			ClearNIGraph(analog2Graph);
			ClearNIGraph(pmtGraph);
            ClearNIGraph(differenceGraph);
            ClearNIGraph(ratioGraph);
            ClearNIGraph(tofGraph);
            ClearNIGraph(tofGraph2);
            ClearNIGraph(tofGraphNormed);
			ClearNIGraph(ratioGraph);
		}
		public void ClearSpectra()
		{
			ClearNIGraph(analog1Graph);
			ClearNIGraph(analog2Graph);
			ClearNIGraph(pmtGraph);
			ClearNIGraph(ratioGraph);
            ClearNIGraph(differenceGraph);
        }

		public void ClearRealtimeSpectra()
		{
			ClearNIPlot(pmtGraph, pmtOnPlot);
			ClearNIPlot(pmtGraph, pmtOffPlot);
			ClearNIPlot(ratioGraph, ratioPlot);
            ClearNIPlot(differenceGraph, differencePlot);
            ClearNIPlot(analog1Graph, analog1Plot);
			ClearNIPlot(analog2Graph, analog2Plot);
		}

		public void ClearRealtimeNotAnalog()
		{
			ClearNIPlot(tofGraph, tofOnPlot);
			ClearNIPlot(tofGraph, tofOffPlot);
            ClearNIPlot(tofGraph2, tofOnPlot2);
            ClearNIPlot(tofGraph2, tofOffPlot2);
			ClearNIPlot(pmtGraph, pmtOnPlot);
			ClearNIPlot(pmtGraph, pmtOffPlot);
			ClearNIPlot(ratioGraph, ratioPlot);
            ClearNIPlot(differenceGraph, differencePlot);
        }

		public void ClearSpectrumFitOffShots()
		{
			ClearNIPlot(pmtGraph, pmtFitOffAvgPlot);
		}

        public void ClearSpectrumFitOnShots()
        {
            ClearNIPlot(pmtGraph, pmtFitOnAvgPlot);
        }

        public void ClearSpectrumFitRatio()
        {
            ClearNIPlot(ratioGraph, ratioFitPlot2);
        }

        public Range SpectrumAxes
		{
			set
			{
				SetGraphXAxisRange(pmtGraph, value.Minimum, value.Maximum);
				SetGraphXAxisRange(ratioGraph, value.Minimum, value.Maximum);
                SetGraphXAxisRange(differenceGraph, value.Minimum, value.Maximum);
                SetGraphXAxisRange(analog1Graph, value.Minimum, value.Maximum);
				SetGraphXAxisRange(analog2Graph, value.Minimum, value.Maximum);
			}
		}

        public Range TOFAxes
        {
            set
            {
                if (value != null)
                {
                    SetGraphXAxisRange(tofGraph, value.Minimum, value.Maximum);
                    SetGraphXAxisRange(tofGraphNormed, value.Minimum, value.Maximum);
                }
                else
                {
                    tofGraph.XAxes[0].Mode = AxisMode.AutoScaleLoose;
                    tofGraphNormed.XAxes[0].Mode = AxisMode.AutoScaleLoose;
                }
            }
        }

        public Range TOF2Axes
        {
            set
            {
                if (value != null)
                {
                    SetGraphXAxisRange(tofGraph2, value.Minimum, value.Maximum);
                }
                else
                {
                    tofGraph2.XAxes[0].Mode = AxisMode.AutoScaleLoose;
                }
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
				if (max <= min) max = min + 0.1; //highly arbitrary
				return new Range(min, max);
			}
		}

        public Range ratioPlotCursor
        {// this sets the cursor to be a reasonable value based on the scan range
            set
            {
                MoveCursor(ratioGraph, ratioCursor1, value.Minimum);//move the cursor to be at the desired x-position
            }
            //get // This is to make it complate, I don't think we need this at all
            //{
            //    double min = GetCursorPosition(ratioGraph, pmtLowCursor);
            //    double max = GetCursorPosition(pmtGraph, pmtHighCursor);
             //   if (max <= min) max = min + 0.1; //highly arbitrary
            //    return new Range(min, max);
            //}
        }

        public Range differencePlotCursor
        {// this sets the cursor to be a reasonable value based on the scan range
            set
            {
                MoveCursor(differenceGraph, differenceCursor, value.Minimum);//move the cursor to be at the desired x-position
            }
            //get // This is to make it complate, I don't think we need this at all
            //{
            //    double min = GetCursorPosition(ratioGraph, pmtLowCursor);
            //    double max = GetCursorPosition(pmtGraph, pmtHighCursor);
            //   if (max <= min) max = min + 0.1; //highly arbitrary
            //    return new Range(min, max);
            //}
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
                double min = GetCursorPosition(tofGraph, tofLowCursor);
                double max = GetCursorPosition(tofGraph, tofHighCursor);
                if (max <= min) max = min + 1; //also somewhat arbitrary
                return new Range(min, max);
			}
		}

        public Range NormSigGate
        {
            set
            {
                MoveCursor(tofGraph2, normSigGateLow, value.Minimum);
                MoveCursor(tofGraph2, normSigGateHigh, value.Maximum);
            }
            get
            {
                double min = GetCursorPosition(tofGraph2, normSigGateLow);
                double max = GetCursorPosition(tofGraph2, normSigGateHigh);
                if (max <= min) max = min + 1; //also somewhat arbitrary
                return new Range(min, max);
            }
        }

        public Range NormBgGate
        {
            set
            {
                MoveCursor(tofGraph2, normBgGateLow, value.Minimum);
                MoveCursor(tofGraph2, normBgGateHigh, value.Maximum);
            }
            get
            {
                double min = GetCursorPosition(tofGraph2, normBgGateLow);
                double max = GetCursorPosition(tofGraph2, normBgGateHigh);
                if (max <= min) max = min + 1; //still as arbitrary as ever
                return new Range(min, max);
            }
        }

        public void PlotOnTOF(TOF t) { PlotY(tofGraph, tofOnPlot, t.GateStartTime, t.ClockPeriod, t.Data); }
        public void PlotOnTOF(ArrayList t) 
        {
           
            PlotY(tofGraph, tofOnPlot, ((TOF)t[0]).GateStartTime, ((TOF)t[0]).ClockPeriod, ((TOF)t[0]).Data);
            if (t.Count == 4)  //t is the TOFs for the on shots, it is sth like detectionCh1_YgOn, detectionCh1_Ygoff, detectionCh2_YgOn, detectionCh2_Ygoff
            {
                PlotY(tofGraph2, tofOnPlot2, ((TOF)t[2]).GateStartTime, ((TOF)t[2]).ClockPeriod, ((TOF)t[2]).Data);

            }
            else if (t.Count == 2)  //t is the TOFs for the on shots, it is sth like detectionCh1_YgOn, detectionCh1_Ygoff
            {
                PlotY(tofGraph2, tofOnPlot2, ((TOF)t[1]).GateStartTime, ((TOF)t[1]).ClockPeriod, ((TOF)t[1]).Data);
                
            }

        }

        public void PlotOffTOF(TOF t) { PlotY(tofGraph, tofOffPlot, t.GateStartTime, t.ClockPeriod, t.Data); }
        public void PlotOffTOF(ArrayList t) 
        { 
            PlotY(tofGraph, tofOffPlot, ((TOF)t[0]).GateStartTime, ((TOF)t[0]).ClockPeriod, ((TOF)t[0]).Data);
            if (t.Count > 2)// in this case, we are likely taking pmt1 YgOn, pmt1 YgOff, pmt2 YgOn, pmt2 YgOff
            {
                PlotY(tofGraph2, tofOffPlot2, ((TOF)t[2]).GateStartTime, ((TOF)t[2]).ClockPeriod, ((TOF)t[2]).Data);
            }
            else if (t.Count > 1)// in this case, we are likely taking pmt1 YgOn, pmt2 YgOn
            {
                PlotY(tofGraph2, tofOffPlot2, ((TOF)t[1]).GateStartTime, ((TOF)t[1]).ClockPeriod, ((TOF)t[1]).Data);
            }
        }
		
		public void PlotAverageOnTOF(TOF t)
		{
			PlotY(tofGraph, tofOnAveragePlot, t.GateStartTime, t.ClockPeriod, t.Data);
		}
        public void PlotAverageOnTOF(ArrayList t)
        {
            PlotY(tofGraph, tofOnAveragePlot, ((TOF)t[0]).GateStartTime, ((TOF)t[0]).ClockPeriod, ((TOF)t[0]).Data);
            if (t.Count > 2) // in this case, we are likely taking pmt1 YgOn, pmt1 YgOff, pmt2 YgOn, pmt2 YgOff
            {
                PlotY(tofGraph2, tofOnAveragePlot2, ((TOF)t[2]).GateStartTime, ((TOF)t[2]).ClockPeriod, ((TOF)t[2]).Data);
                PlotY(tofGraphNormed, tofOnAverageNormedPlot, ((TOF)t[0]).GateStartTime, ((TOF)t[0]).ClockPeriod, MovingAverage(((TOF)t[0]-(TOF)t[1]).Data,100));
            }
            else if (t.Count > 1)// in this case, we are likely taking pmt1 YgOn, pmt2 YgOn
            {
                PlotY(tofGraph2, tofOnAveragePlot2, ((TOF)t[1]).GateStartTime, ((TOF)t[1]).ClockPeriod, ((TOF)t[1]).Data);
                PlotY(tofGraphNormed, tofOnAverageNormedPlot, ((TOF)t[0]).GateStartTime, ((TOF)t[0]).ClockPeriod, MovingAverage(((TOF)t[0] - (TOF)t[1]).Data, 100));
            }
        }
        

        public void PlotAverageOffTOF(TOF t)
		{
			PlotY(tofGraph, tofOffAveragePlot, t.GateStartTime, t.ClockPeriod, t.Data);
		}
        public void PlotAverageOffTOF(ArrayList t)
        {
            PlotY(tofGraph, tofOffAveragePlot, ((TOF)t[0]).GateStartTime, ((TOF)t[0]).ClockPeriod, ((TOF)t[0]).Data);
            if (t.Count > 2)
            {
                PlotY(tofGraph2, tofOffAveragePlot2, ((TOF)t[2]).GateStartTime, ((TOF)t[2]).ClockPeriod, ((TOF)t[2]).Data);
                PlotY(tofGraphNormed, tofOffAverageNormedPlot, ((TOF)t[0]).GateStartTime, ((TOF)t[0]).ClockPeriod, MovingAverage(((TOF)t[0] - (TOF)t[1]).Data,100));
            }
            else if (t.Count > 1)
            {
                PlotY(tofGraph2, tofOffAveragePlot2, ((TOF)t[1]).GateStartTime, ((TOF)t[1]).ClockPeriod, ((TOF)t[1]).Data);
                PlotY(tofGraphNormed, tofOffAverageNormedPlot, ((TOF)t[0]).GateStartTime, ((TOF)t[0]).ClockPeriod, MovingAverage(((TOF)t[0] - (TOF)t[1]).Data, 100));
            }
        }

        public void PlotNormedOnTOF(TOF t) { PlotY(tofGraphNormed, tofOnNormedPlot, t.GateStartTime, t.ClockPeriod, t.Data); }
        public void PlotNormedOffTOF(TOF t) { PlotY(tofGraphNormed, tofOffNormedPlot, t.GateStartTime, t.ClockPeriod, t.Data); }
        public void PlotAverageNormedOnTOF(TOF t) { PlotY(tofGraphNormed, tofOnAverageNormedPlot, t.GateStartTime, t.ClockPeriod, t.Data); }
        public void PlotAverageNormedOffTOF(TOF t) { PlotY(tofGraphNormed, tofOffAverageNormedPlot, t.GateStartTime, t.ClockPeriod, t.Data); }
        

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
        public void AppendToRatio(double[] x, double[] y)
        {
            PlotXYAppend(ratioGraph, ratioPlot, x, y);
        }

        public void PlotAveragePMTOn(double[] x, double[] y)
		{
			PlotXY(pmtGraph, pmtOnAvgPlot, x, y);
		}
		public void PlotAveragePMTOff(double[] x, double[] y)
		{
			PlotXY(pmtGraph, pmtOffAvgPlot, x, y);
		}

        public void updateCurrentScanNumber(int currentScanNumber)
        {
            this.label7.Text = "Current scan num: "+ currentScanNumber.ToString();
        }

		public void PlotAverageDifference(double[] x, double[] y)
		{
			PlotXY(differenceGraph, differenceAvgPlot, x, y);
		}
        public void PlotAverageRatio(double[] x, double[] y)
        {
            PlotXY(ratioGraph, ratioAvgPlot, x, y);
        }
        public void PlotSpectrumFitOffAvg(double[] x, double[] y)
		{
			PlotXY(pmtGraph, pmtFitOffAvgPlot, x, y);
		}
        public void PlotSpectrumFitOnAvg(double[] x, double[] y)
        {
            PlotXY(pmtGraph, pmtFitOnAvgPlot, x, y);
        }
        public void PlotSpectrumFitRatio(double[] x, double[] y)
        {
            PlotXY(ratioGraph, ratioFitPlot2, x, y);
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
        public static double[] MovingAverage(double[] data, int windowSize)
            // Guanchen copied from ChatGPT 2025Dec03, which implements a partial window moving average
        {
            if (windowSize <= 0)
                throw new ArgumentException("windowSize must be positive.");

            double[] result = new double[data.Length];
            double sum = 0.0;
            int count = 0;

            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i];
                count++;

                // When window exceeds the desired size, subtract the element that moves out
                if (count > windowSize)
                {
                    sum -= data[i - windowSize];
                    count = windowSize;
                }

                result[i] = sum / count;  // average of current window
            }

            return result;
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

        public void SetStatus(string text)
        {
            this.Invoke(new SetStatusDelegate(SetStatusHelper), new object[] { text });
        }
        public void SetTOFStatus(string text)
        {
            this.Invoke(new SetStatusDelegate(SetTOFStatusHelper), new object[] { text });
        }
        private delegate void SetStatusDelegate(string text);
        private void SetStatusHelper(string text)
        {
            statusBar1.Text = text;
        }
        private void SetTOFStatusHelper(string text)
        {
            statusBar2.Text = text;
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

        private void updateNoiseResultsbutton_Click(object sender, EventArgs e)
        {
            viewer.UpdateNoiseResults();
        }


        private void defaultGateButton_Click(object sender, EventArgs e)
        {
            viewer.SetGatesToDefault();
        }

        private void StandardViewerWindow_Load(object sender, EventArgs e)
        {

        }

        private void tofGraph_PlotDataChanged(object sender, XYPlotDataChangedEventArgs e)
        {

        }

        private void pmtGraph_PlotDataChanged(object sender, XYPlotDataChangedEventArgs e)
        {

        }

        private void analog1Graph_PlotDataChanged(object sender, XYPlotDataChangedEventArgs e)
        {

        }

        private void differenceGraph_PlotDataChanged(object sender, XYPlotDataChangedEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click_1(object sender, EventArgs e)
        {

        }

        private void statusBar1_PanelClick(object sender, StatusBarPanelClickEventArgs e)
        {

        }

        private void differenceGraph_PlotDataChanged_1(object sender, XYPlotDataChangedEventArgs e)
        {

        }

        private void toolStripPropertyEditor7_SourceValueChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            viewer.UpdateSpectrumFitOnShots();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            viewer.UpdateRatioFit();
        }

        private void ratioFitResults_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            ClearSpectrumFitRatio();
        }
        private void DisplaySettingsOfInterest()//try to display the setting of interest for easy-saving screen shot
        {
            //get the whole informaiton from the current pg settings
            PluginSettings pgSettings = Controller.GetController().ProfileManager.CurrentProfile.AcquisitorConfig.pgPlugin.Settings;
            //get the data of interest
            string shutterslowdelayText;
            string DurationV0Text;
            string repumpDelayText;
            string repumpDurationText;
            string v0chirpTriggerDelayText;
            string offShotSlowingDurationText;

            try { shutterslowdelayText = pgSettings["shutterslowdelay"].ToString(); }
            catch { shutterslowdelayText = ""; }

            try { DurationV0Text = pgSettings["DurationV0"].ToString(); }
            catch { DurationV0Text = ""; }

            try { repumpDelayText = pgSettings["repumpDelay"].ToString(); }
            catch { repumpDelayText = ""; }


            try { repumpDurationText = pgSettings["repumpDuration"].ToString(); }
            catch { repumpDurationText = ""; }


            try { v0chirpTriggerDelayText = pgSettings["v0chirpTriggerDelay"].ToString(); }
            catch { v0chirpTriggerDelayText = ""; }


            try { offShotSlowingDurationText = pgSettings["offShotSlowingDuration"].ToString(); }
            catch { offShotSlowingDurationText = ""; }



            //display the data of interest
            this.labelPGsetingsOfInterest.Text = "v0chirpTriggerDelay:\t" + v0chirpTriggerDelayText;
            this.labelPGsetingsOfInterest.Text = this.labelPGsetingsOfInterest.Text + "\n"+"shutterslowdelaly:\t" + shutterslowdelayText;
            this.labelPGsetingsOfInterest.Text = this.labelPGsetingsOfInterest.Text+ "\n" + "DurationV0:\t" + DurationV0Text;
            this.labelPGsetingsOfInterest.Text = this.labelPGsetingsOfInterest.Text + "\n"+" "+ "\n" + "repumpDelay:\t" + repumpDelayText;
            this.labelPGsetingsOfInterest.Text = this.labelPGsetingsOfInterest.Text + "\n" + "repumpDuration:\t" + repumpDurationText;
            this.labelPGsetingsOfInterest.Text = this.labelPGsetingsOfInterest.Text + "\n" + " " + "\n" + "offShotSlowingDuration:\t" + offShotSlowingDurationText;
            

        }

        private void buttonDisplaySettingsOfInterest_Click(object sender, EventArgs e)
        {
            DisplaySettingsOfInterest();
        }

        private void tofGraph2_PlotDataChanged(object sender, XYPlotDataChangedEventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }
    }
}
