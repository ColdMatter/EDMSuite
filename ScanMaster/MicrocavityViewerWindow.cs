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
	public class MicrocavityViewerWindow : System.Windows.Forms.Form
	{

		// this windows associated Viewer
		private MicrocavityViewer viewer;
		private System.ComponentModel.Container components = null;
		private NationalInstruments.UI.WindowsForms.ScatterGraph analog1Graph;
		private NationalInstruments.UI.XAxis xAxis1;
		private NationalInstruments.UI.YAxis yAxis1;
		private NationalInstruments.UI.WindowsForms.ScatterGraph analog2Graph;
		private NationalInstruments.UI.XAxis xAxis2;
        private NationalInstruments.UI.YAxis yAxis2;
		private NationalInstruments.UI.ScatterPlot analog1Plot;
        private NationalInstruments.UI.ScatterPlot analog2Plot;
        private NationalInstruments.UI.XAxis pmtXAxis;
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
		private NationalInstruments.UI.YAxis differenceYAxis;
		private NationalInstruments.UI.YAxis tofYAxis;
        private NationalInstruments.UI.YAxis tofAvgYAxis;
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
        private WaveformGraph tofGraph3;
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
        private ScatterGraph analog3Graph;
        private ScatterPlot scatterPlot1;
        private XAxis xAxis8;
        private YAxis yAxis7;
        private ScatterGraph analog4Graph;
        private ScatterPlot scatterPlot2;
        private XAxis xAxis9;
        private WaveformGraph tofGraph4;
        private XYCursor xyCursor1;
        private WaveformPlot waveformPlot1;
        private XAxis xAxis3;
        private YAxis yAxis9;
        private XYCursor xyCursor2;
        private WaveformPlot waveformPlot2;
        private YAxis yAxis10;
        private WaveformPlot waveformPlot3;
        private WaveformPlot waveformPlot4;
        private WaveformPlot waveformPlot5;
        private WaveformGraph tofGraph5;
        private XYCursor xyCursor5;
        private WaveformPlot waveformPlot6;
        private XAxis xAxis10;
        private YAxis yAxis11;
        private XYCursor xyCursor6;
        private WaveformPlot waveformPlot7;
        private YAxis yAxis12;
        private WaveformPlot waveformPlot8;
        private WaveformPlot waveformPlot9;
        private WaveformPlot waveformPlot10;
        private YAxis yAxis8;

		public MicrocavityViewerWindow(MicrocavityViewer viewer
            )
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MicrocavityViewerWindow));
            this.analog1Graph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.analog1Plot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.analog2Graph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.analog2Plot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis2 = new NationalInstruments.UI.XAxis();
            this.yAxis2 = new NationalInstruments.UI.YAxis();
            this.pmtXAxis = new NationalInstruments.UI.XAxis();
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
            this.tofGraph3 = new NationalInstruments.UI.WindowsForms.WaveformGraph();
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
            this.analog3Graph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot1 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis8 = new NationalInstruments.UI.XAxis();
            this.yAxis7 = new NationalInstruments.UI.YAxis();
            this.analog4Graph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot2 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis9 = new NationalInstruments.UI.XAxis();
            this.yAxis8 = new NationalInstruments.UI.YAxis();
            this.tofGraph4 = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.xyCursor1 = new NationalInstruments.UI.XYCursor();
            this.waveformPlot1 = new NationalInstruments.UI.WaveformPlot();
            this.xAxis3 = new NationalInstruments.UI.XAxis();
            this.yAxis9 = new NationalInstruments.UI.YAxis();
            this.xyCursor2 = new NationalInstruments.UI.XYCursor();
            this.waveformPlot2 = new NationalInstruments.UI.WaveformPlot();
            this.yAxis10 = new NationalInstruments.UI.YAxis();
            this.waveformPlot3 = new NationalInstruments.UI.WaveformPlot();
            this.waveformPlot4 = new NationalInstruments.UI.WaveformPlot();
            this.waveformPlot5 = new NationalInstruments.UI.WaveformPlot();
            this.tofGraph5 = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.xyCursor5 = new NationalInstruments.UI.XYCursor();
            this.waveformPlot6 = new NationalInstruments.UI.WaveformPlot();
            this.xAxis10 = new NationalInstruments.UI.XAxis();
            this.yAxis11 = new NationalInstruments.UI.YAxis();
            this.xyCursor6 = new NationalInstruments.UI.XYCursor();
            this.waveformPlot7 = new NationalInstruments.UI.WaveformPlot();
            this.yAxis12 = new NationalInstruments.UI.YAxis();
            this.waveformPlot8 = new NationalInstruments.UI.WaveformPlot();
            this.waveformPlot9 = new NationalInstruments.UI.WaveformPlot();
            this.waveformPlot10 = new NationalInstruments.UI.WaveformPlot();
            ((System.ComponentModel.ISupportInitialize)(this.analog1Graph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.analog2Graph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.differenceGraph)).BeginInit();
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
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.analog3Graph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.analog4Graph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor6)).BeginInit();
            this.SuspendLayout();
            // 
            // analog1Graph
            // 
            this.analog1Graph.Location = new System.Drawing.Point(376, 8);
            this.analog1Graph.Name = "analog1Graph";
            this.analog1Graph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.analog1Plot});
            this.analog1Graph.Size = new System.Drawing.Size(284, 126);
            this.analog1Graph.TabIndex = 0;
            this.analog1Graph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.analog1Graph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // analog1Plot
            // 
            this.analog1Plot.AntiAliased = true;
            this.analog1Plot.LineColor = System.Drawing.Color.Red;
            this.analog1Plot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.analog1Plot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.analog1Plot.PointColor = System.Drawing.Color.Red;
            this.analog1Plot.PointStyle = NationalInstruments.UI.PointStyle.SolidDiamond;
            this.analog1Plot.XAxis = this.xAxis1;
            this.analog1Plot.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // analog2Graph
            // 
            this.analog2Graph.Location = new System.Drawing.Point(376, 138);
            this.analog2Graph.Name = "analog2Graph";
            this.analog2Graph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.analog2Plot});
            this.analog2Graph.Size = new System.Drawing.Size(284, 125);
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
            // pmtXAxis
            // 
            this.pmtXAxis.Mode = NationalInstruments.UI.AxisMode.Fixed;
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
            this.differenceGraph.Location = new System.Drawing.Point(375, 530);
            this.differenceGraph.Name = "differenceGraph";
            this.differenceGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.differencePlot,
            this.differenceAvgPlot});
            this.differenceGraph.Size = new System.Drawing.Size(585, 255);
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
            this.differenceAvgPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.differenceAvgPlot.XAxis = this.xAxis5;
            this.differenceAvgPlot.YAxis = this.differenceYAxis;
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
            this.tofGraph.Location = new System.Drawing.Point(8, 8);
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
            this.tofFitModeCombo.Location = new System.Drawing.Point(64, 810);
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
            this.tofFitFunctionCombo.Location = new System.Drawing.Point(142, 810);
            this.tofFitFunctionCombo.Name = "tofFitFunctionCombo";
            this.tofFitFunctionCombo.Size = new System.Drawing.Size(88, 21);
            this.tofFitFunctionCombo.TabIndex = 19;
            this.tofFitFunctionCombo.SelectedIndexChanged += new System.EventHandler(this.tofFitFunctionCombo_SelectedIndexChanged);
            // 
            // spectrumFitFunctionCombo
            // 
            this.spectrumFitFunctionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.spectrumFitFunctionCombo.Location = new System.Drawing.Point(632, 810);
            this.spectrumFitFunctionCombo.Name = "spectrumFitFunctionCombo";
            this.spectrumFitFunctionCombo.Size = new System.Drawing.Size(88, 21);
            this.spectrumFitFunctionCombo.TabIndex = 22;
            this.spectrumFitFunctionCombo.SelectedIndexChanged += new System.EventHandler(this.spectrumFitFunctionCombo_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(551, 792);
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
            this.spectrumFitModeCombo.Location = new System.Drawing.Point(554, 810);
            this.spectrumFitModeCombo.Name = "spectrumFitModeCombo";
            this.spectrumFitModeCombo.Size = new System.Drawing.Size(72, 21);
            this.spectrumFitModeCombo.TabIndex = 20;
            this.spectrumFitModeCombo.SelectedIndexChanged += new System.EventHandler(this.spectrumFitModeCombo_SelectedIndexChanged);
            // 
            // tofFitResultsLabel
            // 
            this.tofFitResultsLabel.ForeColor = System.Drawing.Color.Blue;
            this.tofFitResultsLabel.Location = new System.Drawing.Point(260, 807);
            this.tofFitResultsLabel.Name = "tofFitResultsLabel";
            this.tofFitResultsLabel.Size = new System.Drawing.Size(100, 24);
            this.tofFitResultsLabel.TabIndex = 23;
            this.tofFitResultsLabel.Text = "...";
            this.tofFitResultsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // spectrumFitResultsLabel
            // 
            this.spectrumFitResultsLabel.ForeColor = System.Drawing.Color.Blue;
            this.spectrumFitResultsLabel.Location = new System.Drawing.Point(748, 807);
            this.spectrumFitResultsLabel.Name = "spectrumFitResultsLabel";
            this.spectrumFitResultsLabel.Size = new System.Drawing.Size(210, 24);
            this.spectrumFitResultsLabel.TabIndex = 24;
            this.spectrumFitResultsLabel.Text = "...";
            this.spectrumFitResultsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // updateTOFFitButton
            // 
            this.updateTOFFitButton.Location = new System.Drawing.Point(236, 808);
            this.updateTOFFitButton.Name = "updateTOFFitButton";
            this.updateTOFFitButton.Size = new System.Drawing.Size(18, 23);
            this.updateTOFFitButton.TabIndex = 25;
            this.updateTOFFitButton.Text = ">";
            this.updateTOFFitButton.UseVisualStyleBackColor = true;
            this.updateTOFFitButton.Click += new System.EventHandler(this.updateTOFFitButton_Click);
            // 
            // updateSpectrumFitButton
            // 
            this.updateSpectrumFitButton.Location = new System.Drawing.Point(724, 810);
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
            this.tofFitDataSelectCombo.Location = new System.Drawing.Point(8, 810);
            this.tofFitDataSelectCombo.Name = "tofFitDataSelectCombo";
            this.tofFitDataSelectCombo.Size = new System.Drawing.Size(50, 21);
            this.tofFitDataSelectCombo.TabIndex = 27;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitContainer1.Location = new System.Drawing.Point(0, 839);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.statusBar1);
            this.splitContainer1.Size = new System.Drawing.Size(970, 23);
            this.splitContainer1.SplitterDistance = 371;
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
            this.splitContainer2.Size = new System.Drawing.Size(371, 23);
            this.splitContainer2.SplitterDistance = 236;
            this.splitContainer2.TabIndex = 0;
            // 
            // statusBar2
            // 
            this.statusBar2.Location = new System.Drawing.Point(0, 0);
            this.statusBar2.Name = "statusBar2";
            this.statusBar2.Size = new System.Drawing.Size(236, 23);
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
            this.statusBar1.Size = new System.Drawing.Size(595, 23);
            this.statusBar1.SizingGrip = false;
            this.statusBar1.TabIndex = 14;
            this.statusBar1.Text = "Ready";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(378, 792);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 12);
            this.label3.TabIndex = 31;
            this.label3.Text = "Factor over shot noise:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // updateNoiseResultsbutton
            // 
            this.updateNoiseResultsbutton.Location = new System.Drawing.Point(381, 807);
            this.updateNoiseResultsbutton.Name = "updateNoiseResultsbutton";
            this.updateNoiseResultsbutton.Size = new System.Drawing.Size(18, 23);
            this.updateNoiseResultsbutton.TabIndex = 32;
            this.updateNoiseResultsbutton.Text = ">";
            this.updateNoiseResultsbutton.UseVisualStyleBackColor = true;
            this.updateNoiseResultsbutton.Click += new System.EventHandler(this.updateNoiseResultsbutton_Click);
            // 
            // noiseResultsLabel
            // 
            this.noiseResultsLabel.ForeColor = System.Drawing.Color.Blue;
            this.noiseResultsLabel.Location = new System.Drawing.Point(405, 808);
            this.noiseResultsLabel.Name = "noiseResultsLabel";
            this.noiseResultsLabel.Size = new System.Drawing.Size(143, 24);
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
            this.tofGraph2.Location = new System.Drawing.Point(8, 269);
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
            // tofGraph3
            // 
            this.tofGraph3.Cursors.AddRange(new NationalInstruments.UI.XYCursor[] {
            this.xyCursor3,
            this.xyCursor4});
            this.tofGraph3.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.tofGraph3.Location = new System.Drawing.Point(8, 530);
            this.tofGraph3.Name = "tofGraph3";
            this.tofGraph3.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.tofOnNormedPlot,
            this.tofOffNormedPlot,
            this.tofOnAverageNormedPlot,
            this.tofOffAverageNormedPlot,
            this.tofFitNormedPlot});
            this.tofGraph3.Size = new System.Drawing.Size(361, 255);
            this.tofGraph3.TabIndex = 35;
            this.tofGraph3.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis7});
            this.tofGraph3.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
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
            // analog3Graph
            // 
            this.analog3Graph.Location = new System.Drawing.Point(668, 9);
            this.analog3Graph.Name = "analog3Graph";
            this.analog3Graph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot1});
            this.analog3Graph.Size = new System.Drawing.Size(291, 125);
            this.analog3Graph.TabIndex = 2;
            this.analog3Graph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis8});
            this.analog3Graph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis7});
            // 
            // scatterPlot1
            // 
            this.scatterPlot1.AntiAliased = true;
            this.scatterPlot1.LineColor = System.Drawing.Color.Blue;
            this.scatterPlot1.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.scatterPlot1.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.scatterPlot1.PointColor = System.Drawing.Color.Blue;
            this.scatterPlot1.PointStyle = NationalInstruments.UI.PointStyle.SolidDiamond;
            this.scatterPlot1.XAxis = this.xAxis8;
            this.scatterPlot1.YAxis = this.yAxis7;
            // 
            // xAxis8
            // 
            this.xAxis8.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // analog4Graph
            // 
            this.analog4Graph.Location = new System.Drawing.Point(668, 138);
            this.analog4Graph.Name = "analog4Graph";
            this.analog4Graph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot2});
            this.analog4Graph.Size = new System.Drawing.Size(291, 125);
            this.analog4Graph.TabIndex = 4;
            this.analog4Graph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis9});
            this.analog4Graph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis8});
            this.analog4Graph.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.scatterGraph1_PlotDataChanged);
            // 
            // scatterPlot2
            // 
            this.scatterPlot2.AntiAliased = true;
            this.scatterPlot2.LineColor = System.Drawing.Color.Blue;
            this.scatterPlot2.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.scatterPlot2.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.scatterPlot2.PointColor = System.Drawing.Color.Blue;
            this.scatterPlot2.PointStyle = NationalInstruments.UI.PointStyle.SolidDiamond;
            this.scatterPlot2.XAxis = this.xAxis9;
            this.scatterPlot2.YAxis = this.yAxis8;
            // 
            // xAxis9
            // 
            this.xAxis9.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // tofGraph4
            // 
            this.tofGraph4.Cursors.AddRange(new NationalInstruments.UI.XYCursor[] {
            this.xyCursor1,
            this.xyCursor2});
            this.tofGraph4.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.tofGraph4.Location = new System.Drawing.Point(376, 269);
            this.tofGraph4.Name = "tofGraph4";
            this.tofGraph4.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.waveformPlot2,
            this.waveformPlot3,
            this.waveformPlot1,
            this.waveformPlot4,
            this.waveformPlot5});
            this.tofGraph4.Size = new System.Drawing.Size(284, 255);
            this.tofGraph4.TabIndex = 36;
            this.tofGraph4.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis3});
            this.tofGraph4.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis10,
            this.yAxis9});
            // 
            // xyCursor1
            // 
            this.xyCursor1.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor1.LabelVisible = true;
            this.xyCursor1.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor1.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor1.Plot = this.waveformPlot1;
            this.xyCursor1.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // waveformPlot1
            // 
            this.waveformPlot1.LineColor = System.Drawing.Color.Red;
            this.waveformPlot1.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.waveformPlot1.XAxis = this.xAxis3;
            this.waveformPlot1.YAxis = this.yAxis9;
            // 
            // xAxis3
            // 
            this.xAxis3.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // yAxis9
            // 
            this.yAxis9.Position = NationalInstruments.UI.YAxisPosition.Right;
            // 
            // xyCursor2
            // 
            this.xyCursor2.Color = System.Drawing.Color.Lime;
            this.xyCursor2.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor2.LabelVisible = true;
            this.xyCursor2.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor2.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor2.Plot = this.waveformPlot1;
            this.xyCursor2.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // waveformPlot2
            // 
            this.waveformPlot2.LineColor = System.Drawing.Color.Blue;
            this.waveformPlot2.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.waveformPlot2.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.waveformPlot2.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.waveformPlot2.XAxis = this.xAxis3;
            this.waveformPlot2.YAxis = this.yAxis10;
            // 
            // waveformPlot3
            // 
            this.waveformPlot3.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.waveformPlot3.PointColor = System.Drawing.Color.LawnGreen;
            this.waveformPlot3.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.waveformPlot3.XAxis = this.xAxis3;
            this.waveformPlot3.YAxis = this.yAxis10;
            // 
            // waveformPlot4
            // 
            this.waveformPlot4.LineColor = System.Drawing.Color.PowderBlue;
            this.waveformPlot4.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.waveformPlot4.XAxis = this.xAxis3;
            this.waveformPlot4.YAxis = this.yAxis9;
            // 
            // waveformPlot5
            // 
            this.waveformPlot5.LineColor = System.Drawing.Color.Silver;
            this.waveformPlot5.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.waveformPlot5.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.waveformPlot5.LineWidth = 2F;
            this.waveformPlot5.XAxis = this.xAxis3;
            this.waveformPlot5.YAxis = this.yAxis9;
            // 
            // tofGraph5
            // 
            this.tofGraph5.Cursors.AddRange(new NationalInstruments.UI.XYCursor[] {
            this.xyCursor5,
            this.xyCursor6});
            this.tofGraph5.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.tofGraph5.Location = new System.Drawing.Point(668, 269);
            this.tofGraph5.Name = "tofGraph5";
            this.tofGraph5.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.waveformPlot7,
            this.waveformPlot8,
            this.waveformPlot6,
            this.waveformPlot9,
            this.waveformPlot10});
            this.tofGraph5.Size = new System.Drawing.Size(284, 255);
            this.tofGraph5.TabIndex = 37;
            this.tofGraph5.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis10});
            this.tofGraph5.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis12,
            this.yAxis11});
            this.tofGraph5.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.waveformGraph2_PlotDataChanged);
            // 
            // xyCursor5
            // 
            this.xyCursor5.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor5.LabelVisible = true;
            this.xyCursor5.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor5.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor5.Plot = this.waveformPlot6;
            this.xyCursor5.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // waveformPlot6
            // 
            this.waveformPlot6.LineColor = System.Drawing.Color.Red;
            this.waveformPlot6.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.waveformPlot6.XAxis = this.xAxis10;
            this.waveformPlot6.YAxis = this.yAxis11;
            // 
            // xAxis10
            // 
            this.xAxis10.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // yAxis11
            // 
            this.yAxis11.Position = NationalInstruments.UI.YAxisPosition.Right;
            // 
            // xyCursor6
            // 
            this.xyCursor6.Color = System.Drawing.Color.Lime;
            this.xyCursor6.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor6.LabelVisible = true;
            this.xyCursor6.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor6.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor6.Plot = this.waveformPlot6;
            this.xyCursor6.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // waveformPlot7
            // 
            this.waveformPlot7.LineColor = System.Drawing.Color.Blue;
            this.waveformPlot7.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.waveformPlot7.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.waveformPlot7.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.waveformPlot7.XAxis = this.xAxis10;
            this.waveformPlot7.YAxis = this.yAxis12;
            // 
            // waveformPlot8
            // 
            this.waveformPlot8.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.waveformPlot8.PointColor = System.Drawing.Color.LawnGreen;
            this.waveformPlot8.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.waveformPlot8.XAxis = this.xAxis10;
            this.waveformPlot8.YAxis = this.yAxis12;
            // 
            // waveformPlot9
            // 
            this.waveformPlot9.LineColor = System.Drawing.Color.PowderBlue;
            this.waveformPlot9.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.waveformPlot9.XAxis = this.xAxis10;
            this.waveformPlot9.YAxis = this.yAxis11;
            // 
            // waveformPlot10
            // 
            this.waveformPlot10.LineColor = System.Drawing.Color.Silver;
            this.waveformPlot10.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.waveformPlot10.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.waveformPlot10.LineWidth = 2F;
            this.waveformPlot10.XAxis = this.xAxis10;
            this.waveformPlot10.YAxis = this.yAxis11;
            // 
            // MicrocavityViewerWindow
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(970, 862);
            this.Controls.Add(this.tofGraph5);
            this.Controls.Add(this.tofGraph4);
            this.Controls.Add(this.analog4Graph);
            this.Controls.Add(this.analog3Graph);
            this.Controls.Add(this.tofGraph3);
            this.Controls.Add(this.tofGraph2);
            this.Controls.Add(this.noiseResultsLabel);
            this.Controls.Add(this.updateNoiseResultsbutton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.splitContainer1);
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
            this.Controls.Add(this.analog2Graph);
            this.Controls.Add(this.analog1Graph);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MicrocavityViewerWindow";
            this.Text = "Standard View";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.WindowClosing);
            this.Load += new System.EventHandler(this.StandardViewerWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.analog1Graph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.analog2Graph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.differenceGraph)).EndInit();
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
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.analog3Graph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.analog4Graph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor6)).EndInit();
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
            ClearNIGraph(analog3Graph);
            ClearNIGraph(analog4Graph);
			ClearNIGraph(tofGraph);
            ClearNIGraph(tofGraph2);
            ClearNIGraph(tofGraph3);
            ClearNIGraph(tofGraph4);
            ClearNIGraph(tofGraph5);
            //2D graph
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
            ClearNIPlot(tofGraph2, tofOnPlot2);
            ClearNIPlot(tofGraph2, tofOffPlot2);
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

        public Range TOFAxes
        {
            set
            {
                if (value != null)
                {
                    SetGraphXAxisRange(tofGraph, value.Minimum, value.Maximum);
                    SetGraphXAxisRange(tofGraph3, value.Minimum, value.Maximum);
                }
                else
                {
                    tofGraph.XAxes[0].Mode = AxisMode.AutoScaleLoose;
                    tofGraph3.XAxes[0].Mode = AxisMode.AutoScaleLoose;
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
            if (t.Count > 1)
            {
                PlotY(tofGraph2, tofOnPlot2, ((TOF)t[1]).GateStartTime, ((TOF)t[1]).ClockPeriod, ((TOF)t[1]).Data);
                
            }

        }

        public void PlotOffTOF(TOF t) { PlotY(tofGraph, tofOffPlot, t.GateStartTime, t.ClockPeriod, t.Data); }
        public void PlotOffTOF(ArrayList t) 
        { 
            PlotY(tofGraph, tofOffPlot, ((TOF)t[0]).GateStartTime, ((TOF)t[0]).ClockPeriod, ((TOF)t[0]).Data);
            if (t.Count > 1)
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
            if (t.Count > 1)
            {
                PlotY(tofGraph2, tofOnAveragePlot2, ((TOF)t[1]).GateStartTime, ((TOF)t[1]).ClockPeriod, ((TOF)t[1]).Data);
            }
        }

		public void PlotAverageOffTOF(TOF t)
		{
			PlotY(tofGraph, tofOffAveragePlot, t.GateStartTime, t.ClockPeriod, t.Data);
		}
        public void PlotAverageOffTOF(ArrayList t)
        {
            PlotY(tofGraph, tofOffAveragePlot, ((TOF)t[0]).GateStartTime, ((TOF)t[0]).ClockPeriod, ((TOF)t[0]).Data);
            if (t.Count > 1)
            {
                PlotY(tofGraph2, tofOffAveragePlot2, ((TOF)t[1]).GateStartTime, ((TOF)t[1]).ClockPeriod, ((TOF)t[1]).Data);
            }
        }

        public void PlotNormedOnTOF(TOF t) { PlotY(tofGraph3, tofOnNormedPlot, t.GateStartTime, t.ClockPeriod, t.Data); }
        public void PlotNormedOffTOF(TOF t) { PlotY(tofGraph3, tofOffNormedPlot, t.GateStartTime, t.ClockPeriod, t.Data); }
        public void PlotAverageNormedOnTOF(TOF t) { PlotY(tofGraph3, tofOnAverageNormedPlot, t.GateStartTime, t.ClockPeriod, t.Data); }
        public void PlotAverageNormedOffTOF(TOF t) { PlotY(tofGraph3, tofOffAverageNormedPlot, t.GateStartTime, t.ClockPeriod, t.Data); }
        

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

        private void scatterGraph1_PlotDataChanged(object sender, XYPlotDataChangedEventArgs e)
        {

        }

        private void waveformGraph2_PlotDataChanged(object sender, XYPlotDataChangedEventArgs e)
        {

        }


	}
}
