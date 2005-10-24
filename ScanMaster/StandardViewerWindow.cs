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
		private NationalInstruments.UI.XYCursor pmtHighCursor;

		public StandardViewerWindow(StandardViewer viewer)
		{
			this.viewer = viewer;
			InitializeComponent();
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(StandardViewerWindow));
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
			this.pmtGraph.Location = new System.Drawing.Point(376, 296);
			this.pmtGraph.Name = "pmtGraph";
			this.pmtGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
																					  this.pmtOnPlot,
																					  this.pmtOffPlot,
																					  this.pmtOnAvgPlot,
																					  this.pmtOffAvgPlot});
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
			// pmtXAxis
			// 
			this.pmtXAxis.Mode = NationalInstruments.UI.AxisMode.Fixed;
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 581);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Size = new System.Drawing.Size(970, 22);
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
			this.differenceGraph.Location = new System.Drawing.Point(8, 296);
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
																					   this.tofOffAveragePlot});
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
			this.tofLowCursor.Plot = this.tofOnAveragePlot;
			this.tofLowCursor.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
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
			this.tofHighCursor.Plot = this.tofOnAveragePlot;
			this.tofHighCursor.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
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
			// StandardViewerWindow
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(970, 603);
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
				return new Range(GetCursorPosition(pmtGraph, pmtLowCursor),
					GetCursorPosition(pmtGraph, pmtHighCursor));				
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
				new Object[] {graph, start, end});
		}
		private delegate void PlotXYDelegate(double[] x, double[] y);
		private void PlotXYAppend(Graph graph, ScatterPlot plot, double[] x, double[] y)
		{
			graph.Invoke(new PlotXYDelegate(plot.PlotXYAppend), new Object[] {x,y});
		}
		private void PlotXY(Graph graph, ScatterPlot plot, double[] x, double[] y)
		{
			graph.Invoke(new PlotXYDelegate(plot.PlotXY), new Object[] {x,y});
		}
		private delegate void PlotYDelegate(double[] yData, double start, double inc);
		private void PlotY(Graph graph, WaveformPlot p, double start, double inc, double[] ydata) 
		{
			graph.Invoke(new PlotYDelegate(p.PlotY), new Object[] {ydata, start, inc});
		}
		
		private void MoveCursorHelper(XYCursor cursor, double x)
		{
			cursor.XPosition = x;
		}
		private delegate void MoveCursorDelegate(XYCursor cursor, double x);
		private void MoveCursor(Graph graph, XYCursor cursor, double x)
		{
			graph.Invoke(new MoveCursorDelegate(MoveCursorHelper), new Object[] {cursor, x});
		}

		private delegate double GetCursorPositionDelegate(XYCursor cursor);
		private double GetCursorPositionHelper(XYCursor cursor)
		{
			return cursor.XPosition;
		}
		private double GetCursorPosition(Graph graph, XYCursor cursor)
		{
			double x = (double)graph.Invoke(new GetCursorPositionDelegate(GetCursorPositionHelper),
				new Object[] {cursor});
			return x;
		}

		private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			viewer.ToggleVisible();
			e.Cancel = true;
		}
		
	}
}


