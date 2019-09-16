//using System;
//using System.Drawing;
//using System.Collections;
//using System.ComponentModel;
//using System.Windows.Forms;

//using NationalInstruments.UI;
//using NationalInstruments.UI.WindowsForms;

//namespace ScanMaster.GUI
//{
//    /// <summary>
//    /// Summary description for StatisticsViewerWindow.
//    /// </summary>
//    public class RollViewerWindow : System.Windows.Forms.Form
//    {
//        private RollViewer viewer;
//        private NationalInstruments.UI.XAxis xAxis1;
//        private NationalInstruments.UI.WaveformPlot signalPlot;
//        private NationalInstruments.UI.YAxis signalAxis;
//        private NationalInstruments.UI.XAxis xAxis2;
//        private NationalInstruments.UI.YAxis yAxis1;
//        private NationalInstruments.UI.WindowsForms.WaveformGraph signalGraph;
//        private NationalInstruments.UI.WindowsForms.WaveformGraph iodineGraph;
//        private NationalInstruments.UI.WaveformPlot iodinePlot;
//        public ComboBox dataSelectorCombo;
//        private Label dataSelectionLabel;

//        private System.ComponentModel.Container components = null;

//        public RollViewerWindow(RollViewer viewer)
//        {
//            this.viewer = viewer;
//            InitializeComponent();
//        }

//        /// <summary>
//        /// Clean up any resources being used.
//        /// </summary>
//        protected override void Dispose( bool disposing )
//        {
//            if( disposing )
//            {
//                if(components != null)
//                {
//                    components.Dispose();
//                }
//            }
//            base.Dispose( disposing );
//        }

//        #region Windows Form Designer generated code
//        /// <summary>
//        /// Required method for Designer support - do not modify
//        /// the contents of this method with the code editor.
//        /// </summary>
//        private void InitializeComponent()
//        {
//            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RollViewerWindow));
//            this.signalGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
//            this.signalPlot = new NationalInstruments.UI.WaveformPlot();
//            this.xAxis1 = new NationalInstruments.UI.XAxis();
//            this.signalAxis = new NationalInstruments.UI.YAxis();
//            this.iodineGraph = new NationalInstruments.UI.WindowsForms.WaveformGraph();
//            this.iodinePlot = new NationalInstruments.UI.WaveformPlot();
//            this.xAxis2 = new NationalInstruments.UI.XAxis();
//            this.yAxis1 = new NationalInstruments.UI.YAxis();
//            this.dataSelectorCombo = new System.Windows.Forms.ComboBox();
//            this.dataSelectionLabel = new System.Windows.Forms.Label();
//            ((System.ComponentModel.ISupportInitialize)(this.signalGraph)).BeginInit();
//            ((System.ComponentModel.ISupportInitialize)(this.iodineGraph)).BeginInit();
//            this.SuspendLayout();
//            // 
//            // signalGraph
//            // 
//            this.signalGraph.Location = new System.Drawing.Point(16, 166);
//            this.signalGraph.Name = "signalGraph";
//            this.signalGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
//            this.signalPlot});
//            this.signalGraph.Size = new System.Drawing.Size(696, 400);
//            this.signalGraph.TabIndex = 0;
//            this.signalGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
//            this.xAxis1});
//            this.signalGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
//            this.signalAxis});
//            // 
//            // signalPlot
//            // 
//            this.signalPlot.LineWidth = 2F;
//            this.signalPlot.XAxis = this.xAxis1;
//            this.signalPlot.YAxis = this.signalAxis;
//            // 
//            // xAxis1
//            // 
//            this.xAxis1.Mode = NationalInstruments.UI.AxisMode.StripChart;
//            this.xAxis1.Range = new NationalInstruments.UI.Range(0, 200);
//            // 
//            // signalAxis
//            // 
//            this.signalAxis.Caption = "Signal";
//            // 
//            // iodineGraph
//            // 
//            this.iodineGraph.Location = new System.Drawing.Point(16, 8);
//            this.iodineGraph.Name = "iodineGraph";
//            this.iodineGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {
//            this.iodinePlot});
//            this.iodineGraph.Size = new System.Drawing.Size(696, 152);
//            this.iodineGraph.TabIndex = 1;
//            this.iodineGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
//            this.xAxis2});
//            this.iodineGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
//            this.yAxis1});
//            // 
//            // iodinePlot
//            // 
//            this.iodinePlot.FillMode = NationalInstruments.UI.PlotFillMode.Fill;
//            this.iodinePlot.FillToBaseColor = System.Drawing.Color.ForestGreen;
//            this.iodinePlot.LineWidth = 2F;
//            this.iodinePlot.XAxis = this.xAxis2;
//            this.iodinePlot.YAxis = this.yAxis1;
//            // 
//            // xAxis2
//            // 
//            this.xAxis2.Mode = NationalInstruments.UI.AxisMode.StripChart;
//            this.xAxis2.Range = new NationalInstruments.UI.Range(0, 200);
//            // 
//            // yAxis1
//            // 
//            this.yAxis1.Caption = "Signal";
//            // 
//            // dataSelectorCombo
//            // 
//            this.dataSelectorCombo.FormattingEnabled = true;
//            this.dataSelectorCombo.Items.AddRange(new object[] {
//            "On",
//            "Off"});
//            this.dataSelectorCombo.Location = new System.Drawing.Point(49, 566);
//            this.dataSelectorCombo.Name = "dataSelectorCombo";
//            this.dataSelectorCombo.Size = new System.Drawing.Size(55, 21);
//            this.dataSelectorCombo.TabIndex = 2;
//            this.dataSelectorCombo.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
//            // 
//            // dataSelectionLabel
//            // 
//            this.dataSelectionLabel.AutoSize = true;
//            this.dataSelectionLabel.Location = new System.Drawing.Point(13, 569);
//            this.dataSelectionLabel.Name = "dataSelectionLabel";
//            this.dataSelectionLabel.Size = new System.Drawing.Size(30, 13);
//            this.dataSelectionLabel.TabIndex = 3;
//            this.dataSelectionLabel.Text = "Data";
//            // 
//            // RollViewerWindow
//            // 
//            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
//            this.ClientSize = new System.Drawing.Size(730, 599);
//            this.Controls.Add(this.dataSelectionLabel);
//            this.Controls.Add(this.dataSelectorCombo);
//            this.Controls.Add(this.iodineGraph);
//            this.Controls.Add(this.signalGraph);
//            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
//            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
//            this.MaximizeBox = false;
//            this.Name = "RollViewerWindow";
//            this.Text = "Rolling View";
//            this.Closing += new System.ComponentModel.CancelEventHandler(this.WindowClosing);
//            ((System.ComponentModel.ISupportInitialize)(this.signalGraph)).EndInit();
//            ((System.ComponentModel.ISupportInitialize)(this.iodineGraph)).EndInit();
//            this.ResumeLayout(false);
//            this.PerformLayout();

//        }
//        #endregion

//        // graph functions
//        public void ClearAll()
//        {
//            ClearNIGraph(signalGraph);
//            ClearNIGraph(iodineGraph);
//        }

//        public void AppendToSignalGraph(double[] y)
//        {
//            PlotYAppend(signalGraph, signalPlot, y);
//        }

//        public void AppendToIodineGraph(double[] y)
//        {
//            PlotYAppend(iodineGraph, iodinePlot, y);
//        }


//        // UI delegates and thread-safe helpers
//        private delegate void ClearDataDelegate();
//        private void ClearNIGraph(Graph graph) 
//        {
//            graph.Invoke(new ClearDataDelegate(graph.ClearData));
//        }

//        private delegate void PlotYDelegate(double[] y);
//        private void PlotYAppend(Graph graph, WaveformPlot plot, double[] y)
//        {
//            graph.Invoke(new PlotYDelegate(plot.PlotYAppend), new Object[] {y});
//        }

//        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
//        {
//            e.Cancel = true;
//            viewer.ToggleVisible();
//        }

//        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
//        {
//            viewer.updateDataSelection((string)((ComboBox)sender).SelectedItem);
//        }


//    }
//}
