using NationalInstruments;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MoleculeMOTHardwareControl
{
    public partial class ControlWindow : Form
    {
        
        public Controller controller;
        
        public ControlWindow()
        {
            InitializeComponent();
        }

        #region Windfreak Tab

        public void SetWindfreakTriggerModes(Array values)
        {
            windfreakTriggerModeComboBox.DataSource = values;
        }

        public double GetWindfreakFrequency()
        {
            return (double)windfreakFreqInput.Value;
        }

        public double GetWindfreakAmplitude()
        {
            return (double)windfreakAmpInput.Value;
        }

        private void UpdateWindfreak(object sender, EventArgs e)
        {
            controller.UpdateWindfreak();
        }

        private void ToggleWindfreakOutput(object sender, ActionEventArgs e)
        {
            controller.SetWindfreakOutput(windfreakOutputSwitch.Value);
            windfreakOutputIndicator.Value = windfreakOutputSwitch.Value;
        }

        private void windfreakTriggerModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string value = windfreakTriggerModeComboBox.SelectedValue.ToString();
            controller.ChangeWindfreakTriggerMode(value);
        }

        private void ToggleWindfreakChannel(object sender, ActionCancelEventArgs e)
        {
            controller.SetWindfreakChannel(!windfreakChannelSwitch.Value);
        }

        #endregion

        public void SetCheckBox(CheckBox box, bool state)
        {
            box.Invoke(new SetCheckDelegate(SetCheckHelper), new object[] { box, state });
        }
        private delegate void SetCheckDelegate(CheckBox box, bool state);
        private void SetCheckHelper(CheckBox box, bool state)
        {
            box.Checked = state;
        }

        public void SetTextBox(TextBox box, string text)
        {
            box.Invoke(new SetTextDelegate(SetTextHelper), new object[] { box, text });
        }
        private delegate void SetTextDelegate(TextBox box, string text);
        private void SetTextHelper(TextBox box, string text)
        {
            box.Text = text;
        }

        public void SetDiodeWarning(Led led, bool state)
        {
            led.Invoke(new SetWarningDelegate(SetWarningHelper), new object[] { led, state });
        }
        private delegate void SetWarningDelegate(Led led, bool state);
        private void SetWarningHelper(Led led, bool state)
        {
            led.Value = state;
        }

        private void GetData_Click(object sender, EventArgs e)
        {
            controller.UpdateMonitoring();
        }

        private void ReadFlow_Click(object sender, EventArgs e)
        {
            //controller.ReadFlowMeter();
        }

        private void SetFlow_Click(object sender, EventArgs e)
        {
            controller.SetFlowMeter();
        }

        private void monitorPressureSourceChamber_TextChanged(object sender, EventArgs e)
        {

        }

        private void PressureSourceChamber_Click(object sender, EventArgs e)
        {

        }
              

        private void button2_Click(object sender, EventArgs e)
        {
            controller.StartSidebandRead();
        }

        private void scatterGraph1_PlotDataChanged(object sender, XYPlotDataChangedEventArgs e)
        {

        }

        private void scatterGraph2_PlotDataChanged(object sender, XYPlotDataChangedEventArgs e)
        {

        }
        
        private void scatterGraph5_PlotDataChanged(object sender, XYPlotDataChangedEventArgs e)
        {

        }        
        
        private void scatterGraph6_PlotDataChanged(object sender, XYPlotDataChangedEventArgs e)
        {

        }

        private delegate void plotScatterGraphDelegate(ScatterPlot plot, double[] x, double[] y);
        private void plotScatterGraphHelper(ScatterPlot plot,
            double[] x, double[] y)
        {
            lock (this)
            {
                plot.ClearData();
                
                plot.PlotXY(x, y);
            }
        }


        private void scatterGraphPlot(ScatterGraph graph, ScatterPlot plot, double[] x, double[] y)
        {
            graph.Invoke(new plotScatterGraphDelegate(plotScatterGraphHelper), new object[] { plot, x, y });
        }

        public void displaySidebandData(ScatterGraph graph, double[] xvals, double[] data)
        {
            scatterGraphPlot(graph, scatterPlot1, xvals, data);
            
        }
         

        public void displaySidebandData628V1(ScatterGraph graph, double[] xvals, double[] data)
        {
           scatterGraphPlot(graph, scatterPlot2, xvals, data);
        }
        
        public void displaySidebandData531(ScatterGraph graph, double[] xvals, double[] data)
        {
            scatterGraphPlot(graph, scatterPlot5, xvals, data);
        }
        
        public void displaySidebandData628Slowing(ScatterGraph graph, double[] xvals, double[] data)
        {
            scatterGraphPlot(graph, scatterPlot6, xvals, data);
        }

        private void scatterGraph4_PlotDataChanged(object sender, XYPlotDataChangedEventArgs e)
        {

        }

        private void scatterGraph1_PlotDataChanged_1(object sender, XYPlotDataChangedEventArgs e)
        {

        }

        private void stopReadingButton_Click(object sender, EventArgs e)
        {
            controller.sidebandMonitorRunning = false;
        }

        private void sideBandTab_Click(object sender, EventArgs e)
        {

        }

        private void aom1UpdateButton_Click(object sender, EventArgs e)
        {
            double freq = double.Parse(aom1FreqBox.Text);
            double amp = double.Parse(aom1AmplitudeBox.Text);
            controller.SetMOTAOMFreq(freq);
            controller.SetMOTAOMAmp(amp);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void triggeredCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void FrequencyTab_Click(object sender, EventArgs e)
        {

        }

        

        private void ChangeWindfreakTriggerMode(object sender, EventArgs e)
        {
            
        }

        private void changeWindfreakTriggerMode(object sender, EventArgs e)
        {

        }

        

        private void label1_Click(object sender, EventArgs e)
        {

        }

        
       

        

        
        
       
    }
}