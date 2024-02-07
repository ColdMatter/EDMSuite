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

using System.IO.Ports;



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
        
        }

        private void btStartFlowActMonitor_Click_1(object sender, EventArgs e)
        {
        
        }

        private void btStopFlowActMonitor_Click_1(object sender, EventArgs e)
        {
      
        }

        private void buttonStartPMonitor_Click(object sender, EventArgs e)
        {
            controller.StartPTMonitorPoll();
        }

        private void buttonStopPMonitor_Click(object sender, EventArgs e)
        {
            controller.StopPTMonitorPoll();
        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxSourcePressure_TextChanged(object sender, EventArgs e)
        {

        }

        private void button_Flow_controller_connect_Click(object sender, EventArgs e)
        {
            controller.ConnectFlowControl();
        }

        private void button_Get_Serial_Ports_click(object sender, EventArgs e)
        {
            string[] ArrayComPortsNames = null;
            int index = -1;
            string ComPortName = null;

            ArrayComPortsNames = SerialPort.GetPortNames();
            do
            {
                index += 1;
                richTextBox_output.Text += ArrayComPortsNames[index] + "\n";
            }
            while (!((ArrayComPortsNames[index] == ComPortName) ||
                                (index == ArrayComPortsNames.GetUpperBound(0))));
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void Flow_Controllers_Click(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void textbox_P_source_scroll_TextChanged(object sender, EventArgs e)
        {

        }

        private void button_set_flow_He_Click(object sender, EventArgs e)
        {

        }

        //private void button_Flow_controller_connect_Click_1(object sender, EventArgs e){}
    }
}
