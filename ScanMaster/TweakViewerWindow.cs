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
	public class TweakViewerWindow : System.Windows.Forms.Form
	{

		// this windows associated Viewer
		private TweakViewer viewer;
        private System.ComponentModel.Container components = null;
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
        public ComboBox tofFitModeCombo;
        public System.Windows.Forms.Label tofFitResultsLabel;
		private WaveformPlot tofFitPlot;
        private Button updateTOFFitButton;
        public ComboBox tofFitDataSelectCombo;
        private SplitContainer splitContainer1;
        private StatusBar statusBar1;
        private SplitContainer splitContainer2;
        private StatusBar statusBar2;
        private Button defaultGateButton;
        private TextBox OnAvTextBox;
        private TextBox OnErrTextBox;
        private TextBox OffAvTextBox;
        private TextBox OffErrTextBox;
        private TextBox asymTextBox;
        private Label label2;
        private Label label3;
        private ScatterPlot pmtFitPlot;
        private XAxis xAxis3;
        private YAxis pmtYAxis;
        private ScatterPlot pmtOffAvgPlot;
        private ScatterPlot pmtOnAvgPlot;
        private ScatterPlot pmtOffPlot;
        private ScatterPlot pmtOnPlot;
        private XYCursor pmtHighCursor;
        private XYCursor pmtLowCursor;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Button resetButton;
        private ScatterGraph pmtGraph;
        private XYCursor xyCursor1;
        private ScatterPlot scatterPlot1;
        private XAxis xAxis1;
        private YAxis yAxis1;
        private XYCursor xyCursor2;
        private ScatterPlot scatterPlot2;
        private ScatterPlot scatterPlot3;
        private ScatterPlot scatterPlot4;
        private ScatterPlot scatterPlot5;
        private TextBox asymErrTextBox;

        public TweakViewerWindow(TweakViewer viewer)
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TweakViewerWindow));
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
            this.tofFitResultsLabel = new System.Windows.Forms.Label();
            this.updateTOFFitButton = new System.Windows.Forms.Button();
            this.tofFitDataSelectCombo = new System.Windows.Forms.ComboBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.statusBar2 = new System.Windows.Forms.StatusBar();
            this.defaultGateButton = new System.Windows.Forms.Button();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.OnAvTextBox = new System.Windows.Forms.TextBox();
            this.OnErrTextBox = new System.Windows.Forms.TextBox();
            this.OffAvTextBox = new System.Windows.Forms.TextBox();
            this.OffErrTextBox = new System.Windows.Forms.TextBox();
            this.asymTextBox = new System.Windows.Forms.TextBox();
            this.asymErrTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.pmtFitPlot = new NationalInstruments.UI.ScatterPlot();
            this.xAxis3 = new NationalInstruments.UI.XAxis();
            this.pmtYAxis = new NationalInstruments.UI.YAxis();
            this.pmtOffAvgPlot = new NationalInstruments.UI.ScatterPlot();
            this.pmtOnAvgPlot = new NationalInstruments.UI.ScatterPlot();
            this.pmtOffPlot = new NationalInstruments.UI.ScatterPlot();
            this.pmtOnPlot = new NationalInstruments.UI.ScatterPlot();
            this.pmtHighCursor = new NationalInstruments.UI.XYCursor();
            this.pmtLowCursor = new NationalInstruments.UI.XYCursor();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.resetButton = new System.Windows.Forms.Button();
            this.pmtGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.xyCursor1 = new NationalInstruments.UI.XYCursor();
            this.scatterPlot1 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.xyCursor2 = new NationalInstruments.UI.XYCursor();
            this.scatterPlot2 = new NationalInstruments.UI.ScatterPlot();
            this.scatterPlot3 = new NationalInstruments.UI.ScatterPlot();
            this.scatterPlot4 = new NationalInstruments.UI.ScatterPlot();
            this.scatterPlot5 = new NationalInstruments.UI.ScatterPlot();
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
            ((System.ComponentModel.ISupportInitialize)(this.pmtHighCursor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pmtLowCursor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pmtGraph)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor2)).BeginInit();
            this.SuspendLayout();
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
            this.differenceAvgPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
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
            // tofFitResultsLabel
            // 
            this.tofFitResultsLabel.ForeColor = System.Drawing.Color.Blue;
            this.tofFitResultsLabel.Location = new System.Drawing.Point(260, 602);
            this.tofFitResultsLabel.Name = "tofFitResultsLabel";
            this.tofFitResultsLabel.Size = new System.Drawing.Size(100, 24);
            this.tofFitResultsLabel.TabIndex = 23;
            this.tofFitResultsLabel.Text = "...";
            this.tofFitResultsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitContainer1.Location = new System.Drawing.Point(0, 636);
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
            // OnAvTextBox
            // 
            this.OnAvTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.OnAvTextBox.Location = new System.Drawing.Point(392, 91);
            this.OnAvTextBox.Name = "OnAvTextBox";
            this.OnAvTextBox.ReadOnly = true;
            this.OnAvTextBox.Size = new System.Drawing.Size(100, 20);
            this.OnAvTextBox.TabIndex = 31;
            // 
            // OnErrTextBox
            // 
            this.OnErrTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.OnErrTextBox.Location = new System.Drawing.Point(516, 91);
            this.OnErrTextBox.Name = "OnErrTextBox";
            this.OnErrTextBox.ReadOnly = true;
            this.OnErrTextBox.Size = new System.Drawing.Size(100, 20);
            this.OnErrTextBox.TabIndex = 32;
            // 
            // OffAvTextBox
            // 
            this.OffAvTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.OffAvTextBox.Location = new System.Drawing.Point(392, 133);
            this.OffAvTextBox.Name = "OffAvTextBox";
            this.OffAvTextBox.ReadOnly = true;
            this.OffAvTextBox.Size = new System.Drawing.Size(100, 20);
            this.OffAvTextBox.TabIndex = 33;
            // 
            // OffErrTextBox
            // 
            this.OffErrTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.OffErrTextBox.Location = new System.Drawing.Point(516, 133);
            this.OffErrTextBox.Name = "OffErrTextBox";
            this.OffErrTextBox.ReadOnly = true;
            this.OffErrTextBox.Size = new System.Drawing.Size(100, 20);
            this.OffErrTextBox.TabIndex = 34;
            // 
            // asymTextBox
            // 
            this.asymTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.asymTextBox.Location = new System.Drawing.Point(392, 182);
            this.asymTextBox.Name = "asymTextBox";
            this.asymTextBox.ReadOnly = true;
            this.asymTextBox.Size = new System.Drawing.Size(100, 20);
            this.asymTextBox.TabIndex = 35;
            // 
            // asymErrTextBox
            // 
            this.asymErrTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.asymErrTextBox.Location = new System.Drawing.Point(516, 182);
            this.asymErrTextBox.Name = "asymErrTextBox";
            this.asymErrTextBox.ReadOnly = true;
            this.asymErrTextBox.Size = new System.Drawing.Size(100, 20);
            this.asymErrTextBox.TabIndex = 36;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(399, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 37;
            this.label2.Text = "Average on shot";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(400, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 13);
            this.label3.TabIndex = 38;
            this.label3.Text = "Average off shot";
            // 
            // pmtFitPlot
            // 
            this.pmtFitPlot.LineColor = System.Drawing.Color.Silver;
            this.pmtFitPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.pmtFitPlot.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.pmtFitPlot.LineWidth = 2F;
            this.pmtFitPlot.XAxis = this.xAxis3;
            this.pmtFitPlot.YAxis = this.pmtYAxis;
            // 
            // xAxis3
            // 
            this.xAxis3.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // pmtOffAvgPlot
            // 
            this.pmtOffAvgPlot.LineColor = System.Drawing.Color.PowderBlue;
            this.pmtOffAvgPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.pmtOffAvgPlot.XAxis = this.xAxis3;
            this.pmtOffAvgPlot.YAxis = this.pmtYAxis;
            // 
            // pmtOnAvgPlot
            // 
            this.pmtOnAvgPlot.LineColor = System.Drawing.Color.Red;
            this.pmtOnAvgPlot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.pmtOnAvgPlot.XAxis = this.xAxis3;
            this.pmtOnAvgPlot.YAxis = this.pmtYAxis;
            // 
            // pmtOffPlot
            // 
            this.pmtOffPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.pmtOffPlot.PointColor = System.Drawing.Color.Magenta;
            this.pmtOffPlot.PointStyle = NationalInstruments.UI.PointStyle.Cross;
            this.pmtOffPlot.XAxis = this.xAxis3;
            this.pmtOffPlot.YAxis = this.pmtYAxis;
            // 
            // pmtOnPlot
            // 
            this.pmtOnPlot.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.pmtOnPlot.PointStyle = NationalInstruments.UI.PointStyle.Cross;
            this.pmtOnPlot.XAxis = this.xAxis3;
            this.pmtOnPlot.YAxis = this.pmtYAxis;
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
            // pmtLowCursor
            // 
            this.pmtLowCursor.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.pmtLowCursor.LabelVisible = true;
            this.pmtLowCursor.Plot = this.pmtOnAvgPlot;
            this.pmtLowCursor.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            this.pmtLowCursor.AfterMove += new NationalInstruments.UI.AfterMoveXYCursorEventHandler(this.PMTCursorMoved);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(389, 166);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(112, 13);
            this.label4.TabIndex = 39;
            this.label4.Text = "Average on-off/on+off";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(538, 75);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 40;
            this.label5.Text = "Shot error";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(538, 117);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 41;
            this.label6.Text = "Shot error";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(538, 166);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 42;
            this.label7.Text = "Shot error";
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(392, 220);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(224, 23);
            this.resetButton.TabIndex = 43;
            this.resetButton.Text = "Reset!";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // pmtGraph
            // 
            this.pmtGraph.Cursors.AddRange(new NationalInstruments.UI.XYCursor[] {
            this.xyCursor1,
            this.xyCursor2});
            this.pmtGraph.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.pmtGraph.Location = new System.Drawing.Point(373, 304);
            this.pmtGraph.Name = "pmtGraph";
            this.pmtGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot2,
            this.scatterPlot3,
            this.scatterPlot1,
            this.scatterPlot4,
            this.scatterPlot5});
            this.pmtGraph.Size = new System.Drawing.Size(584, 280);
            this.pmtGraph.TabIndex = 44;
            this.pmtGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.pmtGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // xyCursor1
            // 
            this.xyCursor1.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor1.LabelVisible = true;
            this.xyCursor1.Plot = this.scatterPlot1;
            this.xyCursor1.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // scatterPlot1
            // 
            this.scatterPlot1.LineColor = System.Drawing.Color.Red;
            this.scatterPlot1.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.scatterPlot1.XAxis = this.xAxis1;
            this.scatterPlot1.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.Mode = NationalInstruments.UI.AxisMode.Fixed;
            // 
            // xyCursor2
            // 
            this.xyCursor2.Color = System.Drawing.Color.Lime;
            this.xyCursor2.HorizontalCrosshairMode = NationalInstruments.UI.CursorCrosshairMode.None;
            this.xyCursor2.LabelVisible = true;
            this.xyCursor2.Plot = this.scatterPlot1;
            this.xyCursor2.SnapMode = NationalInstruments.UI.CursorSnapMode.Floating;
            // 
            // scatterPlot2
            // 
            this.scatterPlot2.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.scatterPlot2.PointStyle = NationalInstruments.UI.PointStyle.Cross;
            this.scatterPlot2.XAxis = this.xAxis1;
            this.scatterPlot2.YAxis = this.yAxis1;
            // 
            // scatterPlot3
            // 
            this.scatterPlot3.LineStyle = NationalInstruments.UI.LineStyle.None;
            this.scatterPlot3.PointColor = System.Drawing.Color.Magenta;
            this.scatterPlot3.PointStyle = NationalInstruments.UI.PointStyle.Cross;
            this.scatterPlot3.XAxis = this.xAxis1;
            this.scatterPlot3.YAxis = this.yAxis1;
            // 
            // scatterPlot4
            // 
            this.scatterPlot4.LineColor = System.Drawing.Color.PowderBlue;
            this.scatterPlot4.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.scatterPlot4.XAxis = this.xAxis1;
            this.scatterPlot4.YAxis = this.yAxis1;
            // 
            // scatterPlot5
            // 
            this.scatterPlot5.LineColor = System.Drawing.Color.Silver;
            this.scatterPlot5.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.scatterPlot5.LineStyle = NationalInstruments.UI.LineStyle.DashDot;
            this.scatterPlot5.LineWidth = 2F;
            this.scatterPlot5.XAxis = this.xAxis1;
            this.scatterPlot5.YAxis = this.yAxis1;
            // 
            // TweakViewerWindow
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(970, 659);
            this.Controls.Add(this.pmtGraph);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.asymErrTextBox);
            this.Controls.Add(this.asymTextBox);
            this.Controls.Add(this.OffErrTextBox);
            this.Controls.Add(this.OffAvTextBox);
            this.Controls.Add(this.OnErrTextBox);
            this.Controls.Add(this.OnAvTextBox);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.tofFitDataSelectCombo);
            this.Controls.Add(this.updateTOFFitButton);
            this.Controls.Add(this.tofFitResultsLabel);
            this.Controls.Add(this.tofFitFunctionCombo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tofFitModeCombo);
            this.Controls.Add(this.tofGraph);
            this.Controls.Add(this.differenceGraph);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "TweakViewerWindow";
            this.Text = "Tweak View";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.WindowClosing);
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
            ((System.ComponentModel.ISupportInitialize)(this.pmtHighCursor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pmtLowCursor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pmtGraph)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xyCursor2)).EndInit();
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

		private void updateSpectrumFitButton_Click(object sender, EventArgs e)
		{
			viewer.UpdateSpectrumFit();
		}


		// these functions and properties are all thread safe
		public void ClearAll()
		{

			ClearNIGraph(pmtGraph);
			ClearNIGraph(tofGraph);
			ClearNIGraph(differenceGraph);
		}
		public void ClearSpectra()
		{

			ClearNIGraph(pmtGraph);
			ClearNIGraph(differenceGraph);
		}

		public void ClearRealtimeSpectra()
		{
			ClearNIPlot(pmtGraph, pmtOnPlot);
			ClearNIPlot(pmtGraph, pmtOffPlot);
			ClearNIPlot(differenceGraph, differencePlot);

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

        public void UpdateTextBoxes(double onAv, double offAv, double asymAv, double onErr, double offErr, double asymErr )
        {
            
            SetTextBox(OnAvTextBox, onAv.ToString());
            SetTextBox(OffAvTextBox, offAv.ToString());
            SetTextBox(asymTextBox, asymAv.ToString());
            SetTextBox(OnErrTextBox, onErr.ToString());
            SetTextBox(OffErrTextBox, offErr.ToString());
            SetTextBox(asymErrTextBox, asymErr.ToString());


            if (asymAv < 0)
            {
                SetBoxColour(asymTextBox, System.Drawing.Color.Red);
                SetBoxColour(asymErrTextBox, System.Drawing.Color.Red);

            }
            else
            {
                SetBoxColour(asymTextBox, System.Drawing.Color.Green);
                SetBoxColour(asymErrTextBox, System.Drawing.Color.Green);
            }
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

        public void SetTextBox(TextBox box, string text)
        {
            box.Invoke(new SetTextBoxDelegate(SetTextBoxHelper), new object[] { box, text });
        }
        private delegate void SetTextBoxDelegate(TextBox box, string text);
        private void SetTextBoxHelper(TextBox box, string text)
        {
            box.Text = text;
        }

        public void SetBoxColour(TextBox box, System.Drawing.Color col)
        {
            box.Invoke(new SetBoxColourDelegate(SetBoxColourHelper), new object[] { box, col });
        }
        private delegate void SetBoxColourDelegate(TextBox box, System.Drawing.Color col);
        private void SetBoxColourHelper(TextBox box, System.Drawing.Color col)
        {
            box.BackColor = col;
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



		private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			viewer.ToggleVisible();
			e.Cancel = true;
		}



        private void defaultGateButton_Click(object sender, EventArgs e)
        {
            viewer.SetGatesToDefault();
        }



        private void resetButton_Click(object sender, EventArgs e)
        {
            viewer.flipRestFlat(); 
        }


	}
}
