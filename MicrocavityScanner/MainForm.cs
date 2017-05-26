using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;

using ScanMaster;

using Data;
using Data.Scans;

namespace MicrocavityScanner.GUI
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        // the application controller
        public Controller controller;

        private TransferCavityLock2012.Controller tclController;
        private ScanMaster.Controller smController;

        public MainForm(Controller controller)
        {
            this.controller = controller;
            
            //subscribe to form closing event
            this.FormClosing += this.MainForm_ClosingEvent;

            InitializeComponent();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter_1(object sender, EventArgs e)
        {

        }

        private void label1_Click_2(object sender, EventArgs e)
        {
            
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                FastAxisSelectCombo.SelectedIndex = Convert.ToInt32(Properties.Settings.Default["FastAxisLaserBind"]);
                SlowAxisSelectCombo.SelectedIndex = Convert.ToInt32(Properties.Settings.Default["SlowAxisLaserBind"]);

                //controller.laserSettings.Add("FastLaser", FastAxisSelectCombo.Text);
                controller.scanSettings.Add("FastAxisStart", Convert.ToDouble(FastAxisStart.Text));
                controller.scanSettings.Add("FastAxisEnd", Convert.ToDouble(FastAxisEnd.Text));
                controller.scanSettings.Add("FastAxisRes", Convert.ToDouble(FastAxisRes.Text));
                //controller.laserSettings.Add("SlowLaser", SlowAxisSelectCombo.Text);
                controller.scanSettings.Add("SlowAxisStart", Convert.ToDouble(SlowAxisStart.Text));
                controller.scanSettings.Add("SlowAxisEnd", Convert.ToDouble(SlowAxisEnd.Text));
                controller.scanSettings.Add("SlowAxisRes", Convert.ToDouble(SlowAxisRes.Text));
                controller.scanSettings.Add("Exposure", Convert.ToDouble(Exposure.Text));
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message, 
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        private void MainForm_ClosingEvent(object sender, FormClosingEventArgs e)
        {
            try
            {
                Properties.Settings.Default["FastAxisLaserBind"] = FastAxisSelectCombo.SelectedIndex;
                Properties.Settings.Default["SlowAxisLaserBind"] = SlowAxisSelectCombo.SelectedIndex;
                Properties.Settings.Default.Save();
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in saving the settings: " + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
           
        }

        private void FastAxisStart_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (FastAxisStart.Text != "")
                {
                    controller.scanSettings["FastAxisStart"] = Convert.ToDouble(FastAxisStart.Text);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FastAxisEnd_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (FastAxisEnd.Text != "")
                {
                    controller.scanSettings["FastAxisEnd"] = Convert.ToDouble(FastAxisEnd.Text);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FastAxisRes_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (FastAxisRes.Text != "")
                {
                    controller.scanSettings["FastAxisRes"] = Convert.ToDouble(FastAxisRes.Text);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SlowAxisStart_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (SlowAxisStart.Text != "")
                {
                    controller.scanSettings["SlowAxisStart"] = Convert.ToDouble(SlowAxisStart.Text);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SlowAxisEnd_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (SlowAxisEnd.Text != "")
                {
                    controller.scanSettings["SlowAxisEnd"] = Convert.ToDouble(SlowAxisEnd.Text);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SlowAxisRes_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (SlowAxisRes.Text != "")
                {
                    controller.scanSettings["SlowAxisRes"] = Convert.ToDouble(SlowAxisRes.Text);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Exposure_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (Exposure.Text != "")
                {
                    controller.scanSettings["Exposure"] = Convert.ToDouble(Exposure.Text);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void FastAxisSelectCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                controller.laserSettings["FastLaser"] = FastAxisSelectCombo.Text;
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SlowAxisSelectCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                controller.laserSettings["SlowLaser"] = SlowAxisSelectCombo.Text;
            }
            catch (Exception err)
            {
                MessageBox.Show("There has been an error in applying the settings:" + err.Message,
                    "Settings Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Controller.GetController().StartAcquire();
        }

        private void saveScanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Controller.GetController().SaveData();
        }

        public void UpdateGraphs(ScanPoint point)
        {
            //get the data points
            double y = point.ScanParameter;
            int shotcount = point.OnShots.Count;
            Shot shotOff = (Shot)point.OffShots[shotcount - 1];
            TOF shotOffTOF = (TOF)shotOff.TOFs[0];
            double x = shotOffTOF.Mean;
            Shot shotOn = (Shot)point.OnShots[shotcount - 1];
            TOF shotOnTOF = (TOF)shotOn.TOFs[0];
            double datapoint = shotOnTOF.Mean;
            double[] xplot = { x };
            double[] datapointplot = { datapoint };

            //append to scatter 
            if (shotcount == 1) { ClearNIGraph(scatterGraph1); }
            PlotXYAppend(scatterGraph1, scatterPlot1, xplot, datapointplot);
        }

        public void SlowFinished(ScanPoint point)
        {

        }

        public void ClearAll()
        {
            ClearNIGraph(SuperScanGraph);
            ClearNIGraph(scatterGraph1);
        }

        //This function only takes every n points until the length is 1000 points long
        //for long scans this prevents running out of memory and the plot falling
        //over.
        public double[,] DecimateData(double[,] data)
        {
            double divis = data.Length / (2 * 1000);
            int skip = Convert.ToInt32(Math.Ceiling(divis));
            double[,] decimated = new double[1000, 1];
            for (int i = 0; i <= 999; i++)
            {
                decimated[i, 0] = data[skip * i, 1];
                //decimated[i, 1] = data[skip*i, 1];
            }
            return decimated;
        }

        public double[,] OnTofFullData(Range x, double[] y)
        {
            double[,] array = new double[y.Length, 2];
            for (int i = 0; i <= y.Length - 1; i++)
            {
                array[i, 0] = x.Minimum + i;
                array[i, 1] = y[i];
            }
            return array;
        }

        //public void PlotOnTOF(TOF t) { PlotY(tofGraph, tofOnPlot, t.GateStartTime, t.ClockPeriod, t.Data); }
        //public void PlotOnTOF(ArrayList t)
        //{
        //    //Here need to use the delegat to fill graph.
        //    double[,] superscanplotappend = OnTofFullData(TOFGate, ((TOF)t[0]).Data);
        //    double historicalcounts = superscanPlot.HistoryCountX;
        //    PlotYAppend(superscanGraph, DecimateData(OnTofFullData(TOFGate, ((TOF)t[0]).Data)));
        //}

        //public void AppendToAnalog1(double[] x, double[] y)
        //{
        //    PlotXYAppend(analog1Graph, analog1Plot, x, y);
        //}

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
            graph.Invoke(new PlotYAppendDelegate(graph.PlotYAppend), new Object[] { zdata, false });
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Controller.GetController().StopAcquire();
        }
    }
}
