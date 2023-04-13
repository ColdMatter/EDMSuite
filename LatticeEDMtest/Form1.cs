using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace LatticeHardwareControl
{
    public partial class Form1 : Form
    {
        public Program controller;
        public Form1()
        {
            InitializeComponent();
            controller = new Program();
        }

        #region Controls needed for commands in form

        public void SetTextBox(TextBox box, string text)
        {
            box.Invoke(new SetTextDelegate(SetTextHelper), new object[] { box, text });
        }
        public delegate void SetTextDelegate(TextBox box, string text);

        public void EnableControl(Control control, bool enabled)
        {
            control.Invoke(new EnableControlDelegate(EnableControlHelper), new object[] { control, enabled });
        }
        private delegate void EnableControlDelegate(Control control, bool enabled);
        private void EnableControlHelper(Control control, bool enabled)
        {
            control.Enabled = enabled;
        }

        public void SetTextHelper(TextBox box, string text)
        {
            box.Text = text;
        }

        public void AddPointToChart(Chart chart, string series, DateTime xpoint, double ypoint)
        {
            chart.Invoke(new AddPointToChartDelegate(AddPointToChartHelper), new object[] { chart, series, xpoint, ypoint });
        }
        private delegate void AddPointToChartDelegate(Chart chart, string series, DateTime xpoint, double ypoint);
        private void AddPointToChartHelper(Chart chart, string series, DateTime xpoint, double ypoint)
        {
            // All charts are for temperature or pressure - these should not be zero or negative.
            // This also has the benefit of allowing logarithmic Y scales to be used on plots, without an exception being thrown.
            chart.Series[series].Points.AddXY(xpoint, ypoint);
        }

        #endregion

        private void label1_Click(object sender, EventArgs e)
        {

        }


        private void btSetNewHeliumFlowSetpoint_Click(object sender, EventArgs e)
        {
            controller.SetHeliumFlowSetpoint();
        }

        private void btStartFlowActMonitor_Click_1(object sender, EventArgs e)
        {
            controller.StartFlowMonitorPoll();
        }

        private void btStopFlowActMonitor_Click_1(object sender, EventArgs e)
        {
            controller.StopFlowMonitorPoll();
        }
    }
}
