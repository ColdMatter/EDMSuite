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
		private NationalInstruments.UI.XAxis analog1xAxis;
		private NationalInstruments.UI.YAxis analog1yAxis;
		private NationalInstruments.UI.WindowsForms.ScatterGraph analog2Graph;
		private NationalInstruments.UI.XAxis analog2xAxis;
        private NationalInstruments.UI.YAxis analog2yAxis;
		private NationalInstruments.UI.ScatterPlot analog1Plot;
        private NationalInstruments.UI.ScatterPlot analog2Plot;
        private NationalInstruments.UI.XAxis pmtXAxis;
		private NationalInstruments.UI.WindowsForms.WaveformGraph tofGraph;
		private NationalInstruments.UI.XAxis tofXAxis;
		private NationalInstruments.UI.WaveformPlot tofOnPlot;
        private NationalInstruments.UI.WaveformPlot tofOffPlot;
		private NationalInstruments.UI.XYCursor tofLowCursor;
		private NationalInstruments.UI.XYCursor tofHighCursor;
		private NationalInstruments.UI.WaveformPlot tofOnAveragePlot;
        private NationalInstruments.UI.WaveformPlot tofOffAveragePlot;
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
        private XAxis tofxAxis2;
        private YAxis tofAvgyAxis2;
        private XYCursor normSigGateHigh;
        private WaveformPlot tofOnPlot2;
        private YAxis tofyAxis2;
        private WaveformPlot tofOffPlot2;
        private WaveformPlot tofOffAveragePlot2;
        private WaveformPlot tofFitPlot2;
        private WaveformGraph tofGraph3;
        private XYCursor xyCursor3;
        private WaveformPlot tofOnAveragePlot3;
        private XAxis tofxAxis3;
        private YAxis tofAvgyAxis3;
        private XYCursor xyCursor4;
        private WaveformPlot tofOnPlot3;
        private YAxis tofyAxis3;
        private WaveformPlot tofOffPlot3;
        private WaveformPlot tofOffAveragePlot3;
        private WaveformPlot tofFitPlot3;
        private ToolStripLabel toolStripLabel1;
        private ToolStripPropertyEditor toolStripPropertyEditor1;
        private ToolStripLabel toolStripLabel2;
        private ToolStripPropertyEditor toolStripPropertyEditor2;
        private ToolStripLabel toolStripLabel3;
        private ToolStripPropertyEditor toolStripPropertyEditor3;
        private XYCursor normBgGateLow;
        private XYCursor normBgGateHigh;
        private ScatterGraph analog3Graph;
        private ScatterPlot analog3Plot;
        private XAxis analog3xAxis;
        private YAxis analog3yAxis;
        private ScatterGraph analog4Graph;
        private ScatterPlot analog4Plot;
        private XAxis analog4xAxis;
        private WaveformGraph tofGraph4;
        private XYCursor xyCursor1;
        private WaveformPlot tofOnAveragePlot4;
        private XAxis tofxAxis4;
        private YAxis tofAvgyAxis4;
        private XYCursor xyCursor2;
        private WaveformPlot tofOnPlot4;
        private YAxis tofyAxis4;
        private WaveformPlot tofOffPlot4;
        private WaveformPlot tofOffAveragePlot4;
        private WaveformPlot tofFitPlot4;
        private WaveformGraph tofGraph5;
        private XYCursor xyCursor5;
        private WaveformPlot tofOnAveragePlot5;
        private XAxis tofxAxis5;
        private YAxis tofAvgyAxis5;
        private XYCursor xyCursor6;
        private WaveformPlot tofOnPlot5;
        private YAxis tofyAxis5;
        private WaveformPlot tofOffPlot5;
        private WaveformPlot tofOffAveragePlot5;
        private WaveformPlot tofFitPlot5;
        private IntensityGraph superscanGraph;
        private ColorScale colorScale1;
        private IntensityPlot superscanPlot;
        private IntensityXAxis superscanXAxis;
        private IntensityYAxis superscanYAxis;
        private WaveformGraph tofGraph6;
        private XYCursor xyCursor7;
        private WaveformPlot tofOnAveragePlot6;
        private XAxis tofxAxis6;
        private YAxis yAxis1;
        private XYCursor xyCursor8;
        private WaveformPlot tofOnPlot6;
        private YAxis tofyAxis6;
        private WaveformPlot tofOffPlot6;
        private WaveformPlot tofOffAveragePlot6;
        private WaveformPlot tofFitPlot6;
        private YAxis analog4yAxis;

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
            this.analog1xAxis = new NationalInstruments.UI.XAxis();
            this.analog1yAxis = new NationalInstruments.UI.YAxis();
            this.analog2Graph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.analog2Plot = new NationalInstruments.UI.ScatterPlot();
            this.analog2xAxis = new NationalInstruments.UI.XAxis();
            this.analog2yAxis = new NationalInstruments.UI.YAxis();
            this.pmtXAxis = new NationalInstruments.UI.XAxis();
            this.tofGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.tofLowCursor = new NationalInstruments.UI.XYCursor();
            this.tofOnAveragePlot = new NationalInstruments.UI.WaveformPlot();
            this.tofXAxis = new NationalInstruments.UI.XAxis();
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
            this.tofxAxis2 = new NationalInstruments.UI.XAxis();
            this.tofAvgyAxis2 = new NationalInstruments.UI.YAxis();
            this.normSigGateHigh = new NationalInstruments.UI.XYCursor();
            this.normBgGateLow = new NationalInstruments.UI.XYCursor();
            this.tofOnPlot2 = new NationalInstruments.UI.WaveformPlot();
            this.tofyAxis2 = new NationalInstruments.UI.YAxis();
            this.normBgGateHigh = new NationalInstruments.UI.XYCursor();
            this.tofOffPlot2 = new NationalInstruments.UI.WaveformPlot();
            this.tofOffAveragePlot2 = new NationalInstruments.UI.WaveformPlot();
            this.tofFitPlot2 = new NationalInstruments.UI.WaveformPlot();
            this.tofGraph3 = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.xyCursor3 = new NationalInstruments.UI.XYCursor();
            this.tofOnAveragePlot3 = new NationalInstruments.UI.WaveformPlot();
            this.tofxAxis3 = new NationalInstruments.UI.XAxis();
            this.tofAvgyAxis3 = new NationalInstruments.UI.YAxis();
            this.xyCursor4 = new NationalInstruments.UI.XYCursor();
            this.tofOnPlot3 = new NationalInstruments.UI.WaveformPlot();
            this.tofyAxis3 = new NationalInstruments.UI.YAxis();
            this.tofOffPlot3 = new NationalInstruments.UI.WaveformPlot();
            this.tofOffAveragePlot3 = new NationalInstruments.UI.WaveformPlot();
            this.tofFitPlot3 = new NationalInstruments.UI.WaveformPlot();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripPropertyEditor1 = new NationalInstruments.UI.WindowsForms.ToolStripPropertyEditor();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripPropertyEditor2 = new NationalInstruments.UI.WindowsForms.ToolStripPropertyEditor();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripPropertyEditor3 = new NationalInstruments.UI.WindowsForms.ToolStripPropertyEditor();
            this.analog3Graph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.analog3Plot = new NationalInstruments.UI.ScatterPlot();
            this.analog3xAxis = new NationalInstruments.UI.XAxis();
            this.analog3yAxis = new NationalInstruments.UI.YAxis();
            this.analog4Graph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.analog4Plot = new NationalInstruments.UI.ScatterPlot();
            this.analog4xAxis = new NationalInstruments.UI.XAxis();
            this.analog4yAxis = new NationalInstruments.UI.YAxis();
            this.tofGraph4 = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.xyCursor1 = new NationalInstruments.UI.XYCursor();
            this.tofOnAveragePlot4 = new NationalInstruments.UI.WaveformPlot();
            this.tofxAxis4 = new NationalInstruments.UI.XAxis();
            this.tofAvgyAxis4 = new NationalInstruments.UI.YAxis();
            this.xyCursor2 = new NationalInstruments.UI.XYCursor();
            this.tofOnPlot4 = new NationalInstruments.UI.WaveformPlot();
            this.tofyAxis4 = new NationalInstruments.UI.YAxis();
            this.tofOffPlot4 = new NationalInstruments.UI.WaveformPlot();
            this.tofOffAveragePlot4 = new NationalInstruments.UI.WaveformPlot();
            this.tofFitPlot4 = new NationalInstruments.UI.WaveformPlot();
            this.tofGraph5 = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.xyCursor5 = new NationalInstruments.UI.XYCursor();
            this.tofOnAveragePlot5 = new NationalInstruments.UI.WaveformPlot();
            this.tofxAxis5 = new NationalInstruments.UI.XAxis();
            this.tofAvgyAxis5 = new NationalInstruments.UI.YAxis();
            this.xyCursor6 = new NationalInstruments.UI.XYCursor();
            this.tofOnPlot5 = new NationalInstruments.UI.WaveformPlot();
            this.tofyAxis5 = new NationalInstruments.UI.YAxis();
            this.tofOffPlot5 = new NationalInstruments.UI.WaveformPlot();
            this.tofOffAveragePlot5 = new NationalInstruments.UI.WaveformPlot();
            this.tofFitPlot5 = new NationalInstruments.UI.WaveformPlot();
            this.superscanGraph = new NationalInstruments.UI.WindowsForms.IntensityGraph();
            this.colorScale1 = new NationalInstruments.UI.ColorScale();
            this.superscanPlot = new NationalInstruments.UI.IntensityPlot();
            this.superscanXAxis = new NationalInstruments.UI.IntensityXAxis();
            this.superscanYAxis = new NationalInstruments.UI.IntensityYAxis();
            this.tofGraph6 = new NationalInstruments.UI.WindowsForms.WaveformGraph();
            this.xyCursor7 = new NationalInstruments.UI.XYCursor();
            this.tofOnAveragePlot6 = new NationalInstruments.UI.WaveformPlot();
            this.tofxAxis6 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.xyCursor8 = new NationalInstruments.UI.XYCursor();
            this.tofOnPlot6 = new NationalInstruments.UI.WaveformPlot();
            this.tofyAxis6 = new NationalInstruments.UI.YAxis();
            this.tofOffPlot6 = new NationalInstruments.UI.WaveformPlot();
            this.tofOffAveragePlot6 = new NationalInstruments.UI.WaveformPlot();
            this.tofFitPlot6 = new NationalInstruments.UI.WaveformPlot();
            ((System.ComponentModel.ISupportInitialize)(this.analog1Graph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.analog2Graph)).BeginInit();
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
            ((System.ComponentModel.ISupportInitialize)(this.superscanGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor8)).BeginInit();
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
            this.analog1xAxis});
            this.analog1Graph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.analog1yAxis});
            // 
            // analog1Plot
            // 
            this.analog1Plot.AntiAliased = true;
            this.analog1Plot.LineColor = System.Drawing.Color.Red;
            this.analog1Plot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.analog1Plot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.analog1Plot.PointColor = System.Drawing.Color.Red;
            this.analog1Plot.PointStyle = NationalInstruments.UI.PointStyle.SolidDiamond;
            this.analog1Plot.XAxis = this.analog1xAxis;
            this.analog1Plot.YAxis = this.analog1yAxis;
            // 
            // analog1xAxis
            // 
            this.analog1xAxis.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // analog2Graph
            // 
            this.analog2Graph.Location = new System.Drawing.Point(666, 8);
            this.analog2Graph.Name = "analog2Graph";
            this.analog2Graph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.analog2Plot});
            this.analog2Graph.Size = new System.Drawing.Size(284, 126);
            this.analog2Graph.TabIndex = 1;
            this.analog2Graph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.analog2xAxis});
            this.analog2Graph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.analog2yAxis});
            // 
            // analog2Plot
            // 
            this.analog2Plot.AntiAliased = true;
            this.analog2Plot.LineColor = System.Drawing.Color.Blue;
            this.analog2Plot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.analog2Plot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.analog2Plot.PointColor = System.Drawing.Color.Blue;
            this.analog2Plot.PointStyle = NationalInstruments.UI.PointStyle.SolidDiamond;
            this.analog2Plot.XAxis = this.analog2xAxis;
            this.analog2Plot.YAxis = this.analog2yAxis;
            // 
            // analog2xAxis
            // 
            this.analog2xAxis.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // pmtXAxis
            // 
            this.pmtXAxis.Mode = NationalInstruments.UI.AxisMode.Fixed;
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
            this.tofXAxis});
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
            this.tofOnAveragePlot.XAxis = this.tofXAxis;
            this.tofOnAveragePlot.YAxis = this.tofAvgYAxis;
            // 
            // tofXAxis
            // 
            this.tofXAxis.Mode = NationalInstruments.UI.AxisMode.Fixed;
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
            this.tofOnPlot.XAxis = this.tofXAxis;
            this.tofOnPlot.YAxis = this.tofYAxis;
            // 
            // tofOffPlot
            // 
            this.tofOffPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.tofOffPlot.PointColor = System.Drawing.Color.LawnGreen;
            this.tofOffPlot.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.tofOffPlot.XAxis = this.tofXAxis;
            this.tofOffPlot.YAxis = this.tofYAxis;
            // 
            // tofOffAveragePlot
            // 
            this.tofOffAveragePlot.LineColor = System.Drawing.Color.PowderBlue;
            this.tofOffAveragePlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOffAveragePlot.XAxis = this.tofXAxis;
            this.tofOffAveragePlot.YAxis = this.tofAvgYAxis;
            // 
            // tofFitPlot
            // 
            this.tofFitPlot.LineColor = System.Drawing.Color.Silver;
            this.tofFitPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofFitPlot.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.tofFitPlot.LineWidth = 2F;
            this.tofFitPlot.XAxis = this.tofXAxis;
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
            this.tofxAxis2});
            this.tofGraph2.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.tofyAxis2,
            this.tofAvgyAxis2});
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
            this.tofOnAveragePlot2.XAxis = this.tofxAxis2;
            this.tofOnAveragePlot2.YAxis = this.tofAvgyAxis2;
            // 
            // tofxAxis2
            // 
            this.tofxAxis2.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // tofAvgyAxis2
            // 
            this.tofAvgyAxis2.Position = NationalInstruments.UI.YAxisPosition.Right;
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
            this.tofOnPlot2.XAxis = this.tofxAxis2;
            this.tofOnPlot2.YAxis = this.tofyAxis2;
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
            this.tofOffPlot2.XAxis = this.tofxAxis2;
            this.tofOffPlot2.YAxis = this.tofyAxis2;
            // 
            // tofOffAveragePlot2
            // 
            this.tofOffAveragePlot2.LineColor = System.Drawing.Color.PowderBlue;
            this.tofOffAveragePlot2.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOffAveragePlot2.XAxis = this.tofxAxis2;
            this.tofOffAveragePlot2.YAxis = this.tofAvgyAxis2;
            // 
            // tofFitPlot2
            // 
            this.tofFitPlot2.LineColor = System.Drawing.Color.Silver;
            this.tofFitPlot2.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofFitPlot2.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.tofFitPlot2.LineWidth = 2F;
            this.tofFitPlot2.XAxis = this.tofxAxis2;
            this.tofFitPlot2.YAxis = this.tofAvgyAxis2;
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
            this.tofOnPlot3,
            this.tofOffPlot3,
            this.tofOnAveragePlot3,
            this.tofOffAveragePlot3,
            this.tofFitPlot3});
            this.tofGraph3.Size = new System.Drawing.Size(361, 255);
            this.tofGraph3.TabIndex = 35;
            this.tofGraph3.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.tofxAxis3});
            this.tofGraph3.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.tofyAxis3,
            this.tofAvgyAxis3});
            // 
            // xyCursor3
            // 
            this.xyCursor3.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor3.LabelVisible = true;
            this.xyCursor3.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor3.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor3.Plot = this.tofOnAveragePlot3;
            this.xyCursor3.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // tofOnAveragePlot3
            // 
            this.tofOnAveragePlot3.LineColor = System.Drawing.Color.Red;
            this.tofOnAveragePlot3.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOnAveragePlot3.XAxis = this.tofxAxis3;
            this.tofOnAveragePlot3.YAxis = this.tofAvgyAxis3;
            // 
            // tofxAxis3
            // 
            this.tofxAxis3.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // tofAvgyAxis3
            // 
            this.tofAvgyAxis3.Position = NationalInstruments.UI.YAxisPosition.Right;
            // 
            // xyCursor4
            // 
            this.xyCursor4.Color = System.Drawing.Color.Lime;
            this.xyCursor4.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor4.LabelVisible = true;
            this.xyCursor4.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor4.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor4.Plot = this.tofOnAveragePlot3;
            this.xyCursor4.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // tofOnPlot3
            // 
            this.tofOnPlot3.LineColor = System.Drawing.Color.Blue;
            this.tofOnPlot3.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOnPlot3.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.tofOnPlot3.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.tofOnPlot3.XAxis = this.tofxAxis3;
            this.tofOnPlot3.YAxis = this.tofyAxis3;
            // 
            // tofOffPlot3
            // 
            this.tofOffPlot3.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.tofOffPlot3.PointColor = System.Drawing.Color.LawnGreen;
            this.tofOffPlot3.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.tofOffPlot3.XAxis = this.tofxAxis3;
            this.tofOffPlot3.YAxis = this.tofyAxis3;
            // 
            // tofOffAveragePlot3
            // 
            this.tofOffAveragePlot3.LineColor = System.Drawing.Color.PowderBlue;
            this.tofOffAveragePlot3.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOffAveragePlot3.XAxis = this.tofxAxis3;
            this.tofOffAveragePlot3.YAxis = this.tofAvgyAxis3;
            // 
            // tofFitPlot3
            // 
            this.tofFitPlot3.LineColor = System.Drawing.Color.Silver;
            this.tofFitPlot3.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofFitPlot3.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.tofFitPlot3.LineWidth = 2F;
            this.tofFitPlot3.XAxis = this.tofxAxis3;
            this.tofFitPlot3.YAxis = this.tofAvgyAxis3;
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
            this.analog3Graph.Location = new System.Drawing.Point(376, 138);
            this.analog3Graph.Name = "analog3Graph";
            this.analog3Graph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.analog3Plot});
            this.analog3Graph.Size = new System.Drawing.Size(284, 125);
            this.analog3Graph.TabIndex = 2;
            this.analog3Graph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.analog3xAxis});
            this.analog3Graph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.analog3yAxis});
            // 
            // analog3Plot
            // 
            this.analog3Plot.AntiAliased = true;
            this.analog3Plot.LineColor = System.Drawing.Color.Green;
            this.analog3Plot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.analog3Plot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.analog3Plot.PointColor = System.Drawing.Color.Green;
            this.analog3Plot.PointStyle = NationalInstruments.UI.PointStyle.SolidDiamond;
            this.analog3Plot.XAxis = this.analog3xAxis;
            this.analog3Plot.YAxis = this.analog3yAxis;
            // 
            // analog3xAxis
            // 
            this.analog3xAxis.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // analog4Graph
            // 
            this.analog4Graph.Location = new System.Drawing.Point(666, 138);
            this.analog4Graph.Name = "analog4Graph";
            this.analog4Graph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.analog4Plot});
            this.analog4Graph.Size = new System.Drawing.Size(284, 125);
            this.analog4Graph.TabIndex = 4;
            this.analog4Graph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.analog4xAxis});
            this.analog4Graph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.analog4yAxis});
            this.analog4Graph.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.scatterGraph1_PlotDataChanged);
            // 
            // analog4Plot
            // 
            this.analog4Plot.AntiAliased = true;
            this.analog4Plot.LineColor = System.Drawing.Color.Fuchsia;
            this.analog4Plot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.analog4Plot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.analog4Plot.PointColor = System.Drawing.Color.Fuchsia;
            this.analog4Plot.PointStyle = NationalInstruments.UI.PointStyle.SolidDiamond;
            this.analog4Plot.XAxis = this.analog4xAxis;
            this.analog4Plot.YAxis = this.analog4yAxis;
            // 
            // analog4xAxis
            // 
            this.analog4xAxis.Mode = NationalInstruments.UI.AxisMode.Fixed;
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
            this.tofOnPlot4,
            this.tofOffPlot4,
            this.tofOnAveragePlot4,
            this.tofOffAveragePlot4,
            this.tofFitPlot4});
            this.tofGraph4.Size = new System.Drawing.Size(284, 125);
            this.tofGraph4.TabIndex = 36;
            this.tofGraph4.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.tofxAxis4});
            this.tofGraph4.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.tofyAxis4,
            this.tofAvgyAxis4});
            // 
            // xyCursor1
            // 
            this.xyCursor1.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor1.LabelVisible = true;
            this.xyCursor1.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor1.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor1.Plot = this.tofOnAveragePlot4;
            this.xyCursor1.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // tofOnAveragePlot4
            // 
            this.tofOnAveragePlot4.LineColor = System.Drawing.Color.Red;
            this.tofOnAveragePlot4.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOnAveragePlot4.XAxis = this.tofxAxis4;
            this.tofOnAveragePlot4.YAxis = this.tofAvgyAxis4;
            // 
            // tofxAxis4
            // 
            this.tofxAxis4.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // tofAvgyAxis4
            // 
            this.tofAvgyAxis4.Position = NationalInstruments.UI.YAxisPosition.Right;
            // 
            // xyCursor2
            // 
            this.xyCursor2.Color = System.Drawing.Color.Lime;
            this.xyCursor2.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor2.LabelVisible = true;
            this.xyCursor2.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor2.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor2.Plot = this.tofOnAveragePlot4;
            this.xyCursor2.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // tofOnPlot4
            // 
            this.tofOnPlot4.LineColor = System.Drawing.Color.Blue;
            this.tofOnPlot4.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOnPlot4.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.tofOnPlot4.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.tofOnPlot4.XAxis = this.tofxAxis4;
            this.tofOnPlot4.YAxis = this.tofyAxis4;
            // 
            // tofOffPlot4
            // 
            this.tofOffPlot4.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.tofOffPlot4.PointColor = System.Drawing.Color.LawnGreen;
            this.tofOffPlot4.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.tofOffPlot4.XAxis = this.tofxAxis4;
            this.tofOffPlot4.YAxis = this.tofyAxis4;
            // 
            // tofOffAveragePlot4
            // 
            this.tofOffAveragePlot4.LineColor = System.Drawing.Color.PowderBlue;
            this.tofOffAveragePlot4.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOffAveragePlot4.XAxis = this.tofxAxis4;
            this.tofOffAveragePlot4.YAxis = this.tofAvgyAxis4;
            // 
            // tofFitPlot4
            // 
            this.tofFitPlot4.LineColor = System.Drawing.Color.Silver;
            this.tofFitPlot4.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofFitPlot4.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.tofFitPlot4.LineWidth = 2F;
            this.tofFitPlot4.XAxis = this.tofxAxis4;
            this.tofFitPlot4.YAxis = this.tofAvgyAxis4;
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
            this.tofGraph5.Location = new System.Drawing.Point(376, 400);
            this.tofGraph5.Name = "tofGraph5";
            this.tofGraph5.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.tofOnPlot5,
            this.tofOffPlot5,
            this.tofOnAveragePlot5,
            this.tofOffAveragePlot5,
            this.tofFitPlot5});
            this.tofGraph5.Size = new System.Drawing.Size(284, 124);
            this.tofGraph5.TabIndex = 37;
            this.tofGraph5.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.tofxAxis5});
            this.tofGraph5.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.tofyAxis5,
            this.tofAvgyAxis5});
            this.tofGraph5.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.waveformGraph2_PlotDataChanged);
            // 
            // xyCursor5
            // 
            this.xyCursor5.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor5.LabelVisible = true;
            this.xyCursor5.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor5.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor5.Plot = this.tofOnAveragePlot5;
            this.xyCursor5.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // tofOnAveragePlot5
            // 
            this.tofOnAveragePlot5.LineColor = System.Drawing.Color.Red;
            this.tofOnAveragePlot5.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOnAveragePlot5.XAxis = this.tofxAxis5;
            this.tofOnAveragePlot5.YAxis = this.tofAvgyAxis5;
            // 
            // tofxAxis5
            // 
            this.tofxAxis5.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // tofAvgyAxis5
            // 
            this.tofAvgyAxis5.Position = NationalInstruments.UI.YAxisPosition.Right;
            // 
            // xyCursor6
            // 
            this.xyCursor6.Color = System.Drawing.Color.Lime;
            this.xyCursor6.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor6.LabelVisible = true;
            this.xyCursor6.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor6.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor6.Plot = this.tofOnAveragePlot5;
            this.xyCursor6.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // tofOnPlot5
            // 
            this.tofOnPlot5.LineColor = System.Drawing.Color.Blue;
            this.tofOnPlot5.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOnPlot5.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.tofOnPlot5.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.tofOnPlot5.XAxis = this.tofxAxis5;
            this.tofOnPlot5.YAxis = this.tofyAxis5;
            // 
            // tofOffPlot5
            // 
            this.tofOffPlot5.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.tofOffPlot5.PointColor = System.Drawing.Color.LawnGreen;
            this.tofOffPlot5.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.tofOffPlot5.XAxis = this.tofxAxis5;
            this.tofOffPlot5.YAxis = this.tofyAxis5;
            // 
            // tofOffAveragePlot5
            // 
            this.tofOffAveragePlot5.LineColor = System.Drawing.Color.PowderBlue;
            this.tofOffAveragePlot5.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOffAveragePlot5.XAxis = this.tofxAxis5;
            this.tofOffAveragePlot5.YAxis = this.tofAvgyAxis5;
            // 
            // tofFitPlot5
            // 
            this.tofFitPlot5.LineColor = System.Drawing.Color.Silver;
            this.tofFitPlot5.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofFitPlot5.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.tofFitPlot5.LineWidth = 2F;
            this.tofFitPlot5.XAxis = this.tofxAxis5;
            this.tofFitPlot5.YAxis = this.tofAvgyAxis5;
            // 
            // superscanGraph
            // 
            this.superscanGraph.ColorScales.AddRange(new NationalInstruments.UI.ColorScale[] {
            this.colorScale1});
            this.superscanGraph.Location = new System.Drawing.Point(376, 530);
            this.superscanGraph.Name = "superscanGraph";
            this.superscanGraph.Plots.AddRange(new NationalInstruments.UI.IntensityPlot[] {
            this.superscanPlot});
            this.superscanGraph.Size = new System.Drawing.Size(573, 255);
            this.superscanGraph.TabIndex = 38;
            this.superscanGraph.XAxes.AddRange(new NationalInstruments.UI.IntensityXAxis[] {
            this.superscanXAxis});
            this.superscanGraph.YAxes.AddRange(new NationalInstruments.UI.IntensityYAxis[] {
            this.superscanYAxis});
            this.superscanGraph.PlotDataChanged += new NationalInstruments.UI.IntensityPlotDataChangedEventHandler(this.superscanGraph_PlotDataChanged);
            // 
            // colorScale1
            // 
            this.colorScale1.Mode = NationalInstruments.UI.ColorScaleMode.AutoScaleLoose;
            // 
            // superscanPlot
            // 
            this.superscanPlot.ColorScale = this.colorScale1;
            this.superscanPlot.HistoryCapacityX = 1000;
            this.superscanPlot.HistoryCapacityY = 1000;
            this.superscanPlot.XAxis = this.superscanXAxis;
            this.superscanPlot.YAxis = this.superscanYAxis;
            // 
            // tofGraph6
            // 
            this.tofGraph6.Cursors.AddRange(new NationalInstruments.UI.XYCursor[] {
            this.xyCursor7,
            this.xyCursor8});
            this.tofGraph6.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.tofGraph6.Location = new System.Drawing.Point(666, 270);
            this.tofGraph6.Name = "tofGraph6";
            this.tofGraph6.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.tofOnPlot6,
            this.tofOffPlot6,
            this.tofOnAveragePlot6,
            this.tofOffAveragePlot6,
            this.tofFitPlot6});
            this.tofGraph6.Size = new System.Drawing.Size(284, 254);
            this.tofGraph6.TabIndex = 39;
            this.tofGraph6.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.tofxAxis6});
            this.tofGraph6.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.tofyAxis6,
            this.yAxis1});
            // 
            // xyCursor7
            // 
            this.xyCursor7.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor7.LabelVisible = true;
            this.xyCursor7.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor7.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor7.Plot = this.tofOnAveragePlot6;
            this.xyCursor7.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // tofOnAveragePlot6
            // 
            this.tofOnAveragePlot6.LineColor = System.Drawing.Color.Red;
            this.tofOnAveragePlot6.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOnAveragePlot6.XAxis = this.tofxAxis6;
            this.tofOnAveragePlot6.YAxis = this.yAxis1;
            // 
            // tofxAxis6
            // 
            this.tofxAxis6.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // yAxis1
            // 
            this.yAxis1.Position = NationalInstruments.UI.YAxisPosition.Right;
            // 
            // xyCursor8
            // 
            this.xyCursor8.Color = System.Drawing.Color.Lime;
            this.xyCursor8.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor8.LabelVisible = true;
            this.xyCursor8.LabelXFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor8.LabelYFormat = new NationalInstruments.UI.FormatString(NationalInstruments.UI.FormatStringMode.Numeric, "G3");
            this.xyCursor8.Plot = this.tofOnAveragePlot6;
            this.xyCursor8.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // tofOnPlot6
            // 
            this.tofOnPlot6.LineColor = System.Drawing.Color.Blue;
            this.tofOnPlot6.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOnPlot6.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.tofOnPlot6.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.tofOnPlot6.XAxis = this.tofxAxis6;
            this.tofOnPlot6.YAxis = this.tofyAxis6;
            // 
            // tofOffPlot6
            // 
            this.tofOffPlot6.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.tofOffPlot6.PointColor = System.Drawing.Color.LawnGreen;
            this.tofOffPlot6.PointStyle = NationalInstruments.UI.PointStyle.Plus;
            this.tofOffPlot6.XAxis = this.tofxAxis6;
            this.tofOffPlot6.YAxis = this.tofyAxis6;
            // 
            // tofOffAveragePlot6
            // 
            this.tofOffAveragePlot6.LineColor = System.Drawing.Color.PowderBlue;
            this.tofOffAveragePlot6.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofOffAveragePlot6.XAxis = this.tofxAxis6;
            this.tofOffAveragePlot6.YAxis = this.yAxis1;
            // 
            // tofFitPlot6
            // 
            this.tofFitPlot6.LineColor = System.Drawing.Color.Silver;
            this.tofFitPlot6.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.tofFitPlot6.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.tofFitPlot6.LineWidth = 2F;
            this.tofFitPlot6.XAxis = this.tofxAxis6;
            this.tofFitPlot6.YAxis = this.yAxis1;
            // 
            // MicrocavityViewerWindow
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(970, 862);
            this.Controls.Add(this.tofGraph6);
            this.Controls.Add(this.superscanGraph);
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
            this.Controls.Add(this.analog2Graph);
            this.Controls.Add(this.analog1Graph);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MicrocavityViewerWindow";
            this.Text = "Microcavity View";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.WindowClosing);
            this.Load += new System.EventHandler(this.StandardViewerWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.analog1Graph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.analog2Graph)).EndInit();
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
            ((System.ComponentModel.ISupportInitialize)(this.superscanGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tofGraph6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor8)).EndInit();
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
            ClearNIGraph(tofGraph6);
            ClearNIGraph(superscanGraph);
			//ClearNIGraph(differenceGraph);
		}
		public void ClearSpectra()
		{
			ClearNIGraph(analog1Graph);
			ClearNIGraph(analog2Graph);
            ClearNIGraph(analog3Graph);
            ClearNIGraph(analog4Graph);
            ClearNIGraph(superscanGraph);
			//ClearNIGraph(pmtGraph);
			//ClearNIGraph(differenceGraph);
		}

        public void ClearSuperscan()
        {
            ClearNIGraph(superscanGraph);
        }

		public void ClearRealtimeSpectra()
		{
			//ClearNIPlot(pmtGraph, pmtOnPlot);
			//ClearNIPlot(pmtGraph, pmtOffPlot);
			//ClearNIPlot(differenceGraph, differencePlot);
			ClearNIPlot(analog1Graph, analog1Plot);
			ClearNIPlot(analog2Graph, analog2Plot);
		}

		public void ClearRealtimeNotAnalog()
		{
			ClearNIPlot(tofGraph, tofOnPlot);
			ClearNIPlot(tofGraph, tofOffPlot);
            ClearNIPlot(tofGraph2, tofOnPlot2);
            ClearNIPlot(tofGraph2, tofOffPlot2);
			//ClearNIPlot(pmtGraph, pmtOnPlot);
			//ClearNIPlot(pmtGraph, pmtOffPlot);
			//ClearNIPlot(differenceGraph, differencePlot);
		}

		public void ClearSpectrumFit()
		{
			//ClearNIPlot(pmtGraph, pmtFitPlot);
		}

		public Range SpectrumAxes
		{
			set
			{
				//SetGraphXAxisRange(pmtGraph, value.Minimum, value.Maximum);
				//SetGraphXAxisRange(differenceGraph, value.Minimum, value.Maximum);
				SetGraphXAxisRange(analog1Graph, value.Minimum, value.Maximum);
				SetGraphXAxisRange(analog2Graph, value.Minimum, value.Maximum);
                SetGraphXAxisRange(analog3Graph, value.Minimum, value.Maximum);
                SetGraphXAxisRange(analog4Graph, value.Minimum, value.Maximum);
			}
		}

        public Range TOFAxes
        {
            set
            {
                if (value != null)
                {
                    SetGraphXAxisRange(tofGraph, value.Minimum, value.Maximum);
                    SetGraphXAxisRange(tofGraph2, value.Minimum, value.Maximum);
                    SetGraphXAxisRange(tofGraph3, value.Minimum, value.Maximum);
                    SetGraphXAxisRange(tofGraph4, value.Minimum, value.Maximum);
                    SetGraphXAxisRange(tofGraph5, value.Minimum, value.Maximum);
                    SetGraphXAxisRange(tofGraph6, value.Minimum, value.Maximum);
                }
                else
                {
                    tofGraph.XAxes[0].Mode = AxisMode.AutoScaleLoose;
                    tofGraph2.XAxes[0].Mode = AxisMode.AutoScaleLoose;
                    tofGraph3.XAxes[0].Mode = AxisMode.AutoScaleLoose;
                    tofGraph4.XAxes[0].Mode = AxisMode.AutoScaleLoose;
                    tofGraph5.XAxes[0].Mode = AxisMode.AutoScaleLoose;
                    tofGraph6.XAxes[0].Mode = AxisMode.AutoScaleLoose;
                }
            }
        }

        //public Range TOF2Axes
        //{
        //    set
        //    {
        //        if (value != null)
        //        {
        //            SetGraphXAxisRange(tofGraph2, value.Minimum, value.Maximum);
        //        }
        //        else
        //        {
        //            tofGraph2.XAxes[0].Mode = AxisMode.AutoScaleLoose;
        //        }
        //    }
        //}

        public Range SpectrumGate
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
                if (max <= min) max = min + 1; //highly arbitrary
                return new Range(min, max);
            }
        }

        //public Range SpectrumGate
        //{
        //    set
        //    {
        //        MoveCursor(pmtGraph, pmtLowCursor, value.Minimum);
        //        MoveCursor(pmtGraph, pmtHighCursor, value.Maximum);
        //    }
        //    get
        //    {
        //        double min = GetCursorPosition(pmtGraph, pmtLowCursor);
        //        double max = GetCursorPosition(pmtGraph, pmtHighCursor);
        //        if (max <= min) max = min + 1; //highly arbitrary
        //        return new Range(min, max);
        //    }
        //}

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

        public void InitialiseSuperScan()
        {
            double[,] initarray = new double[1000,1];
            superscanGraph.Plot(initarray, TOFGate.Minimum, TOFGate.Interval, 0, 1);
        }

        public double[,] OnTofFullData(Range x, double[] y)
        {
            double[,] array = new double[y.Length,2];
            for ( int i = 0; i <= y.Length-1; i++ )
            {
                array[i,0] = x.Minimum + i;
                array[i,1] = y[i];
            }
            return array;
        }


        //This function only takes every n points until the length is 1000 points long
        //for long scans this prevents running out of memory and the plot falling
        //over.


        public double[,] DecimateData(double[,] data)
        {
            double divis = data.Length / (2*1000);
            int skip = Convert.ToInt32(Math.Ceiling(divis));
            double[,] decimated = new double[1000,1];
            for (int i = 0; i <= 999; i++)
            {
                decimated[i, 0] = data[skip*i,1];
                //decimated[i, 1] = data[skip*i, 1];
            }
            return decimated;
        }
        
        public void PlotOnTOF(TOF t) { PlotY(tofGraph, tofOnPlot, t.GateStartTime, t.ClockPeriod, t.Data); }
        public void PlotOnTOF(ArrayList t) 
        { 
            PlotY(tofGraph, tofOnPlot, ((TOF)t[0]).GateStartTime, ((TOF)t[0]).ClockPeriod, ((TOF)t[0]).Data);
            //Here need to use the delegat to fill graph.
            double[,] superscanplotappend = OnTofFullData(TOFGate, ((TOF)t[0]).Data);
            double historicalcounts = superscanPlot.HistoryCountX;
            PlotYAppend(superscanGraph, DecimateData(OnTofFullData(TOFGate,((TOF)t[0]).Data)));
            if (t.Count > 1)
            {
                PlotY(tofGraph2, tofOnPlot2, ((TOF)t[1]).GateStartTime, ((TOF)t[1]).ClockPeriod, ((TOF)t[1]).Data);
            }
            if (t.Count > 2)
            {
                PlotY(tofGraph3, tofOnPlot3, ((TOF)t[2]).GateStartTime, ((TOF)t[2]).ClockPeriod, ((TOF)t[2]).Data);
            } 
            if (t.Count > 3)
            {
                PlotY(tofGraph4, tofOnPlot4, ((TOF)t[3]).GateStartTime, ((TOF)t[3]).ClockPeriod, ((TOF)t[3]).Data);
            } 
            if (t.Count > 4)
            {
                PlotY(tofGraph5, tofOnPlot5, ((TOF)t[4]).GateStartTime, ((TOF)t[4]).ClockPeriod, ((TOF)t[4]).Data);
            }
            if (t.Count > 5)
            {
                PlotY(tofGraph6, tofOnPlot6, ((TOF)t[5]).GateStartTime, ((TOF)t[5]).ClockPeriod, ((TOF)t[5]).Data);
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
            if (t.Count > 2)
            {
                PlotY(tofGraph3, tofOffPlot3, ((TOF)t[2]).GateStartTime, ((TOF)t[2]).ClockPeriod, ((TOF)t[2]).Data);
            }
            if (t.Count > 3)
            {
                PlotY(tofGraph4, tofOffPlot4, ((TOF)t[3]).GateStartTime, ((TOF)t[3]).ClockPeriod, ((TOF)t[3]).Data);
            }
            if (t.Count > 4)
            {
                PlotY(tofGraph5, tofOffPlot5, ((TOF)t[4]).GateStartTime, ((TOF)t[4]).ClockPeriod, ((TOF)t[4]).Data);
            }
            if (t.Count > 5)
            {
                PlotY(tofGraph6, tofOffPlot6, ((TOF)t[5]).GateStartTime, ((TOF)t[5]).ClockPeriod, ((TOF)t[5]).Data);
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
            if (t.Count > 2)
            {
                PlotY(tofGraph3, tofOnAveragePlot3, ((TOF)t[2]).GateStartTime, ((TOF)t[2]).ClockPeriod, ((TOF)t[2]).Data);
            }
            if (t.Count > 3)
            {
                PlotY(tofGraph4, tofOnAveragePlot4, ((TOF)t[3]).GateStartTime, ((TOF)t[3]).ClockPeriod, ((TOF)t[3]).Data);
            }
            if (t.Count > 4)
            {
                PlotY(tofGraph5, tofOnAveragePlot5, ((TOF)t[4]).GateStartTime, ((TOF)t[4]).ClockPeriod, ((TOF)t[4]).Data);
            }
            if (t.Count > 5)
            {
                PlotY(tofGraph6, tofOnAveragePlot6, ((TOF)t[5]).GateStartTime, ((TOF)t[5]).ClockPeriod, ((TOF)t[5]).Data);
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
            if (t.Count > 2)
            {
                PlotY(tofGraph3, tofOffAveragePlot3, ((TOF)t[2]).GateStartTime, ((TOF)t[2]).ClockPeriod, ((TOF)t[2]).Data);
            }
            if (t.Count > 3)
            {
                PlotY(tofGraph4, tofOffAveragePlot4, ((TOF)t[3]).GateStartTime, ((TOF)t[3]).ClockPeriod, ((TOF)t[3]).Data);
            }
            if (t.Count > 4)
            {
                PlotY(tofGraph5, tofOffAveragePlot5, ((TOF)t[4]).GateStartTime, ((TOF)t[4]).ClockPeriod, ((TOF)t[4]).Data);
            }
            if (t.Count > 5)
            {
                PlotY(tofGraph6, tofOffAveragePlot6, ((TOF)t[5]).GateStartTime, ((TOF)t[5]).ClockPeriod, ((TOF)t[5]).Data);
            }
        }

        public void PlotNormedOnTOF(TOF t) { PlotY(tofGraph3, tofOnPlot3, t.GateStartTime, t.ClockPeriod, t.Data); }
        public void PlotNormedOffTOF(TOF t) { PlotY(tofGraph3, tofOffPlot3, t.GateStartTime, t.ClockPeriod, t.Data); }
        public void PlotAverageNormedOnTOF(TOF t) { PlotY(tofGraph3, tofOnAveragePlot3, t.GateStartTime, t.ClockPeriod, t.Data); }
        public void PlotAverageNormedOffTOF(TOF t) { PlotY(tofGraph3, tofOffAveragePlot3, t.GateStartTime, t.ClockPeriod, t.Data); }
        

		public void AppendToAnalog1(double[] x, double[] y)
		{
			PlotXYAppend(analog1Graph, analog1Plot, x, y);
		}

		public void AppendToAnalog2(double[] x, double[] y)
		{
			PlotXYAppend(analog2Graph, analog2Plot, x, y);
		}

        public void AppendToAnalog3(double[] x, double[] y)
        {
            PlotXYAppend(analog3Graph, analog3Plot, x, y);
        }

        public void AppendToAnalog4(double[] x, double[] y)
        {
            PlotXYAppend(analog4Graph, analog4Plot, x, y);
        }
        //public void AppendToPMTOn(double[] x, double[] y)
        //{
        //    PlotXYAppend(pmtGraph, pmtOnPlot, x, y);
        //}
        //public void AppendToPMTOff(double[] x, double[] y)
        //{
        //    PlotXYAppend(pmtGraph, pmtOffPlot, x, y);
        //}
        //public void AppendToDifference(double[] x, double[] y)
        //{
        //    PlotXYAppend(differenceGraph, differencePlot, x, y);
        //}

        //public void PlotAveragePMTOn(double[] x, double[] y)
        //{
        //    PlotXY(pmtGraph, pmtOnAvgPlot, x, y);
        //}
        //public void PlotAveragePMTOff(double[] x, double[] y)
        //{
        //    PlotXY(pmtGraph, pmtOffAvgPlot, x, y);
        //}
        //public void PlotAverageDifference(double[] x, double[] y)
        //{
        //    PlotXY(differenceGraph, differenceAvgPlot, x, y);
        //}
        public void PlotSpectrumFit(double[] x, double[] y)
        {
            //PlotXY(tofGraph, tofFitPlot, x, y);
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
        private delegate void PlotYAppendDelegate(double[,] zdata, bool invert);
        private void PlotYAppend(IntensityGraph graph, double[,] zdata)
        {
            graph.Invoke(new PlotYAppendDelegate(graph.PlotYAppend), new Object[] { zdata, false});
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

        private void superscanGraph_PlotDataChanged(object sender, IntensityPlotDataChangedEventArgs e)
        {

        }


	}
}
