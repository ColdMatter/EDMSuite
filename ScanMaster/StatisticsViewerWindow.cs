using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;

namespace ScanMaster.GUI
{
	/// <summary>
	/// Summary description for StatisticsViewerWindow.
	/// </summary>
	public class StatisticsViewerWindow : System.Windows.Forms.Form
	{
		private StatisticsViewer viewer;
		private NationalInstruments.UI.WindowsForms.WaveformGraph graph;
		private NationalInstruments.UI.XAxis xAxis1;
		private NationalInstruments.UI.WaveformPlot signalPlot;
		private NationalInstruments.UI.YAxis signalAxis;
		private NationalInstruments.UI.WaveformPlot signalNoisePlot;
		private System.Windows.Forms.Label meanLabel;
		private NationalInstruments.UI.YAxis signalNoiseAxis;
		private System.Windows.Forms.Label sdLabel;
		private Label noiseLabel;

		private System.ComponentModel.Container components = null;

		public StatisticsViewerWindow(StatisticsViewer viewer)
		{
			this.viewer = viewer;
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StatisticsViewerWindow));
			this.graph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
			this.signalPlot = new NationalInstruments.UI.WaveformPlot();
			this.xAxis1 = new NationalInstruments.UI.XAxis();
			this.signalAxis = new NationalInstruments.UI.YAxis();
			this.signalNoisePlot = new NationalInstruments.UI.WaveformPlot();
			this.signalNoiseAxis = new NationalInstruments.UI.YAxis();
			this.meanLabel = new System.Windows.Forms.Label();
			this.sdLabel = new System.Windows.Forms.Label();
			this.noiseLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.graph)).BeginInit();
			this.SuspendLayout();
			// 
			// graph
			// 
			this.graph.Location = new System.Drawing.Point(16, 16);
			this.graph.Name = "graph";
			this.graph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
            this.signalPlot,
            this.signalNoisePlot});
			this.graph.Size = new System.Drawing.Size(892, 400);
			this.graph.TabIndex = 0;
			this.graph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
			this.graph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.signalAxis,
            this.signalNoiseAxis});
			// 
			// signalPlot
			// 
			this.signalPlot.XAxis = this.xAxis1;
			this.signalPlot.YAxis = this.signalAxis;
			// 
			// xAxis1
			// 
			this.xAxis1.Mode = NationalInstruments.UI.AxisMode.StripChart;
			this.xAxis1.Range = new NationalInstruments.UI.Range(0, 50);
			// 
			// signalAxis
			// 
			this.signalAxis.Caption = "Signal";
			// 
			// signalNoisePlot
			// 
			this.signalNoisePlot.LineColor = System.Drawing.Color.Red;
			this.signalNoisePlot.XAxis = this.xAxis1;
			this.signalNoisePlot.YAxis = this.signalNoiseAxis;
			// 
			// signalNoiseAxis
			// 
			this.signalNoiseAxis.Caption = "Factor over shot noise";
			this.signalNoiseAxis.CaptionPosition = NationalInstruments.UI.YAxisPosition.Right;
			this.signalNoiseAxis.Position = NationalInstruments.UI.YAxisPosition.Right;
			// 
			// meanLabel
			// 
			this.meanLabel.Font = new System.Drawing.Font("Arial Black", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.meanLabel.Location = new System.Drawing.Point(21, 424);
			this.meanLabel.Name = "meanLabel";
			this.meanLabel.Size = new System.Drawing.Size(301, 104);
			this.meanLabel.TabIndex = 1;
			this.meanLabel.Text = "---";
			// 
			// sdLabel
			// 
			this.sdLabel.Font = new System.Drawing.Font("Arial Black", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.sdLabel.Location = new System.Drawing.Point(328, 424);
			this.sdLabel.Name = "sdLabel";
			this.sdLabel.Size = new System.Drawing.Size(321, 104);
			this.sdLabel.TabIndex = 2;
			this.sdLabel.Text = "---";
			// 
			// noiseLabel
			// 
			this.noiseLabel.Font = new System.Drawing.Font("Arial Black", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.noiseLabel.Location = new System.Drawing.Point(655, 424);
			this.noiseLabel.Name = "noiseLabel";
			this.noiseLabel.Size = new System.Drawing.Size(253, 104);
			this.noiseLabel.TabIndex = 3;
			this.noiseLabel.Text = "---";
			// 
			// StatisticsViewerWindow
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(920, 536);
			this.Controls.Add(this.noiseLabel);
			this.Controls.Add(this.sdLabel);
			this.Controls.Add(this.meanLabel);
			this.Controls.Add(this.graph);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "StatisticsViewerWindow";
			this.Text = "Statistics View";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.WindowClosing);
			((System.ComponentModel.ISupportInitialize)(this.graph)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		// graph functions
		public void ClearAll()
		{
			ClearNIGraph(graph);
//			meanLabel.Text = "---";
//			sdLabel.Text = "---";
		}

		public void AppendToSignalGraph(double y)
		{
			PlotYAppend(graph, signalPlot, y);
		}

		public void AppendToSignalNoiseGraph(double y)
		{
			PlotYAppend(graph, signalNoisePlot, y);
		}

		public void SetMeanText(double mean)
		{
			SetLabelText(meanLabel, String.Format("{0:F2}",mean));
		}

		public void SetSDText(double sd)
		{
			SetLabelText(sdLabel, String.Format("{0:F2}",sd));
		}

		internal void SetNoiseText(double noise)
		{
			SetLabelText(noiseLabel, String.Format("{0:F2}", noise));
		}
		
		
		// UI delegates and thread-safe helpers
		private delegate void ClearDataDelegate();
		private void ClearNIGraph(Graph graph) 
		{
			graph.Invoke(new ClearDataDelegate(graph.ClearData));
		}
		private void SetLabelTextHelper(Label label, String text)
		{
			label.Text = text;
		}
		private delegate void SetLabelTextDelegate(Label label, String text);
		private void SetLabelText(Label label, String text)
		{
			label.Invoke(new SetLabelTextDelegate(SetLabelTextHelper),
				new Object[] {label, text});
		}

		private delegate void PlotYDelegate(double y);
		private void PlotYAppend(Graph graph, WaveformPlot plot, double y)
		{
			graph.Invoke(new PlotYDelegate(plot.PlotYAppend), new Object[] {y});
		}

		private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			viewer.ToggleVisible();
		}

	}
}
