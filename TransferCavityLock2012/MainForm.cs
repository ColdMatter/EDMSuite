using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using NationalInstruments.UI.WindowsForms;
using NationalInstruments.UI;

namespace TransferCavityLock2012
{
    /// <summary>
    /// Front panel of the laser controller
    /// </summary>
    public partial class MainForm : Form
    {
        public Controller controller;
        private Dictionary<string, LockControlPanel> slaveLasers = new Dictionary<string, LockControlPanel>();

        #region Setup

        /// <summary>
        /// The UI for TransferCavityLock
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            controller.InitializeUI();
        }
        
        public void AddSlaveLaser(string name)
        {
            string title = name;
            TabPage newTab = new TabPage(title);
            LockControlPanel panel = new LockControlPanel(name);
            panel.controller = this.controller;
            slaveLasersTab.TabPages.Add(newTab);
            newTab.Controls.Add(panel);
            slaveLasersTab.Enabled = true;
            slaveLasers.Add(name, panel);
        }

        public void ShowAllTabPanels()
        {
            for (int i = 0; i < slaveLasersTab.TabPages.Count; i++)
            {
                slaveLasersTab.TabPages[i].Show();
            }
        }
        /// <summary>
        /// This controls which parts of the UI are enabled for a given machine state.
        /// </summary>
        public void UpdateUIState(Controller.ControllerState state)
        {
            switch (state)
            {
                case Controller.ControllerState.STOPPED:
                    rampStartButton.Enabled = true;
                    rampStopButton.Enabled = false;
                    NumberOfScanpointsTextBox.Enabled = true;
                    rampLED.Value = false;
                    break;

                case Controller.ControllerState.RUNNING:
                    rampStartButton.Enabled = false;
                    rampStopButton.Enabled = true;
                    NumberOfScanpointsTextBox.Enabled = false;
                    rampLED.Value = true;
                    break;
            }
        }
        
        internal void UpdateUIState(string name, SlaveLaser.LaserState state)
        {
             slaveLasers[name].UpdateUIState(state);
        }
        #endregion

        #region ThreadSafe wrappers

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

        public void SetLED(Led led, bool val)
        {
            led.Invoke(new SetLedDelegate(SetLedHelper), new object[] { led, val });
        }
        private delegate void SetLedDelegate(Led led, bool val);
        private void SetLedHelper(Led led, bool val)
        {
            led.Value = val;
        }

        public void EnableControl(Control control, bool enabled)
        {
            control.Invoke(new EnableControlDelegate(EnableControlHelper), new object[] { control, enabled });
        }
        private delegate void EnableControlDelegate(Control control, bool enabled);
        private void EnableControlHelper(Control control, bool enabled)
        {
            control.Enabled = enabled;
        }


        private delegate void plotScatterGraphDelegate(ScatterPlot plot,double[] x, double[] y);
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
            graph.Invoke(new plotScatterGraphDelegate(plotScatterGraphHelper), new object[] {plot, x, y });
        }

      
        private void rampStartButton_Click(object sender, EventArgs e)
        {
            controller.StartTCL();
        }

        private void rampStopButton_Click(object sender, EventArgs e)
        {
            //lockEnableCheck.Checked = false;
            
            lock (controller.rampStopLock)
            {
                controller.StopTCL();
            }

        }

        private void stopAllLocking()
        {

        }


        
        #endregion

        #region Setting and getting parameter values from textboxes
       

        public void SetNumberOfPoints(int value)
        {
            SetTextBox(NumberOfScanpointsTextBox, Convert.ToString(value));
        }
        public int GetNumberOfPoints()
        {
            return Int32.Parse(NumberOfScanpointsTextBox.Text);
        }
        #endregion
    
        #region Displaying Data
        public void DisplayCavityData(double[] indeces, double[] cavityData)
        {
            scatterGraphPlot(CavityVoltageReadScatterGraph, cavityDataPlot, indeces, cavityData);
        }
        public void DisplayMasterData(double[] cavityData, double[] masterData, double[] masterFitData)
        {
            scatterGraphPlot(MasterLaserIntensityScatterGraph, MasterDataPlot, cavityData, masterData);
            scatterGraphPlot(MasterLaserIntensityScatterGraph, MasterFitPlot, cavityData, masterFitData);
        }
        public void DisplaySlaveData(string name, double[] cavityData, double[] slaveData, double[] slaveFitData)
        {
            slaveLasers[name].DisplayData(cavityData, slaveData);
            slaveLasers[name].DisplayFit(cavityData, slaveFitData);
        }
       
        public void DisplaySlaveDataNoFit(string name, double[] cavityData, double[] slaveData)
        {
            slaveLasers[name].DisplayData(cavityData, slaveData);
        }

        public void DisplayErrorData(string name, double[] time, double[] errordata)
        {
            slaveLasers[name].AppendToErrorGraph(time, errordata);
        }

        public void IncrementErrorCount(string name)
        {
            slaveLasers[name].Count++;
        }

        public int GetErrorCount(string name)
        {
            return slaveLasers[name].Count;
        }

        public void ClearErrorGraph(string name)
        {
            slaveLasers[name].ClearErrorGraph();
        }

        public void SetVtoOffsetVoltage(double value)
        {
            SetTextBox(VToOffsetTextBox, Convert.ToString(value));
        }

        public void SetMasterFitTextBox(double value)
        {
            SetTextBox(MasterFitTextBox, Convert.ToString(value));
        }


        #endregion

        #region Panel Control

        public void SetLaserVoltage(string name, double value)
        {
            slaveLasers[name].SetLaserVoltage(value);
        }

        public void SetGain(string name, double value)
        {
            slaveLasers[name].SetGain(value);
        }
        public double GetGain(string name)
        {
            return slaveLasers[name].GetGain();
        }

        public void SetSetPointIncrementSize(string name, double value)
        {
            slaveLasers[name].SetSetPointIncrementSize(value);
        }

        public double GetLaserSetPoint(string name)
        {
            return slaveLasers[name].GetLaserSetPoint();
        }
        public void SetLaserSetPoint(string name, double value)
        {
            slaveLasers[name].SetLaserSetPoint(value);
        }

        public void DisplayData(string name, double[] cavityData, double[] slaveData)
        {
            slaveLasers[name].DisplayData(cavityData, slaveData);
        }
        public void DisplayFit(string name, double[] cavityData, double[] slaveFitData)
        {
            slaveLasers[name].DisplayFit(cavityData, slaveFitData);
        }

        #endregion

        private void MasterLaserIntensityScatterGraph_PlotDataChanged(object sender, XYPlotDataChangedEventArgs e)
        {

        }

        private void masterLockEnableCheck_CheckedChanged(object sender, EventArgs e)
        {

        }

        public void MasterSetPointTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void MasterGainTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void logCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        public void VToOffsetTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }

        private void MasterFitTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void slaveLasersTab_SelectedIndexChanged(object sender, EventArgs e)
        {

        }



 



        
    }
}

