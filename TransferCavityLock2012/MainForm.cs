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

        public MainForm(string name)
        {
            InitializeComponent();
            this.Text = "TransferCavityLock - Controlling " + name;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            controller.InitializeUI();
        }
        
        public void AddSlaveLaser(SlaveLaser sl)
        {
            string title = sl.Name;
            TabPage newTab = new TabPage(title);
            LockControlPanel panel = new LockControlPanel(title,sl.LowerVoltageLimit,sl.UpperVoltageLimit, sl.Gain);
            panel.controller = this.controller;
            slaveLasersTab.TabPages.Add(newTab);
            newTab.Controls.Add(panel);
            slaveLasersTab.Enabled = true;
            slaveLasers.Add(title, panel);
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

        public void SetMasterGain(double value)
        {
            SetTextBox(MasterGainTextBox, Convert.ToString(value));
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
        public void DisplayMasterData(double[] cavityData, double[] masterData)
        {
            scatterGraphPlot(MasterLaserIntensityScatterGraph, MasterDataPlot, cavityData, masterData);
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

        public void UpdateElapsedTime(double time)
        {
            SetTextBox(updateRateTextBox, Convert.ToString(time));
        }

        public double GetElapsedTime()
        {
           return Double.Parse(updateRateTextBox.Text);
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

        public void SetMasterSetPointTextBox(double value)
        {
            SetTextBox(MasterSetPointTextBox, Convert.ToString(value));
        }

        public double GetVtoOffsetVoltage()
        {
            return Double.Parse(VToOffsetTextBox.Text);
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

        public void SetLaserSD(string name, double value)
        {
            slaveLasers[name].SetLaserSD(value);
        }
        public double GetLaserSD(string name)
        {
            return slaveLasers[name].GetLaserSD();
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

        private void logCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (logCheckBox.Checked)
            {
                controller.StartLogger();
            }
            else
            {
                controller.StopLogger();
            }
        }

        private void CavLockVoltageTrackBar_Scroll(object sender, EventArgs e)
        {
            double val =((double)CavLockVoltageTrackBar.Value)/100;
            SetVtoOffsetVoltage(val);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            lock (controller.rampStopLock)
            {
                controller.StopTCL();
            }
        }

        //private void VToOffsetTextBox_TextChanged(object sender, EventArgs e)
        //{
          //CavLockVoltageTrackBar.Value = (int)(100 * GetVtoOffsetVoltage());
        //}

        private void axisCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool boxState = axisCheckBox.Checked;

            ScatterPlot[] plots = { MasterDataPlot, MasterFitPlot, cavityDataPlot };

            foreach(ScatterPlot plot in plots)
            {
                plot.CanScaleYAxis =!boxState;
                plot.CanScaleXAxis =!boxState;
            }

            foreach (LockControlPanel pannel in slaveLasers.Values)
            {
                pannel.AdjustAxesAutoScale(!boxState);
            }

        }

        private void masterLockEnableCheck_CheckedChanged(object sender, EventArgs e)
        {

            if (masterLockEnableCheck.CheckState == CheckState.Checked)
            {
                controller.MasterLaser.ArmLock();
            }
            if (masterLockEnableCheck.CheckState == CheckState.Unchecked)
            {
                controller.MasterLaser.DisengageLock();
            }

        }


        private void voltageRampControl_Enter(object sender, EventArgs e)
        {

        }

        private void loadProfileSetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.LoadParametersWithDialog();
        } 
       
    }
}

