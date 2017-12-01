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
        private Dictionary<string, CavityControlPanel> CavityPanels = new Dictionary<string, CavityControlPanel>();

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
            controller.initializeUI();
        }

        public void AddCavity(Cavity cavity)
        {
            string title = cavity.Name;
            TabPage newTab = new TabPage(title);
            CavityControlPanel panel = new CavityControlPanel(cavity.Name);
            panel.controller = this.controller;
            foreach (KeyValuePair<string, SlaveLaser> entry in cavity.SlaveLasers)
            {
                panel.AddSlaveLaserPanel(entry.Value);
            }
            cavitiesTab.TabPages.Add(newTab);
            newTab.Controls.Add(panel);
            CavityPanels.Add(title, panel);
        }

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

        internal void UpdateMasterUIState(string cavityName, MasterLaser.LaserState state)
        {
            CavityPanels[cavityName].UpdateMasterUIState(state);
        }

        internal void UpdateSlaveUIState(string cavityName, string slaveName, SlaveLaser.LaserState state)
        {
            CavityPanels[cavityName].UpdateSlaveUIState(slaveName, state);
        }

        internal void AppendToErrorGraph(string cavityName, string laserName, int lockCount, double error)
        {
            CavityPanels[cavityName].AppendToErrorGraph(laserName, lockCount, error);
        }

        #region ThreadSafe wrappers

        

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

        public void SetNumberOfPoints(int value)
        {
            UIHelper.SetTextBox(NumberOfScanpointsTextBox, Convert.ToString(value));
        }

        #region Displaying Data
        public void DisplayRampData(double[] indeces, double[] cavityData)
        {
            UIHelper.scatterGraphPlot(CavityVoltageReadScatterGraph, cavityDataPlot, indeces, cavityData);
        }

        public void DisplayMasterData(string cavityName, double[] rampData, double[] masterData, double[] masterFitData)
        {
            CavityControlPanel control = CavityPanels[cavityName];
            control.DisplayMasterData(rampData, masterData);
            control.DisplayMasterFitData(rampData, masterFitData);
        }

        public void DisplayMasterData(string cavityName, double[] rampData, double[] masterData)
        {
            CavityControlPanel cavityTab = CavityPanels[cavityName];
            cavityTab.DisplayMasterData(rampData, masterData);
        }

        public void DisplaySlaveData(string cavityName, string laserName, double[] rampData, double[] data, double[] fitData)
        {
            LockControlPanel control = CavityPanels[cavityName].SlaveLaserPanels[laserName];
            control.DisplayData(rampData, data);
            control.DisplayFit(rampData, fitData);
        }

        public void DisplaySlaveData(string cavityName, string laserName, double[] rampData, double[] data)
        {
            LockControlPanel control = CavityPanels[cavityName].SlaveLaserPanels[laserName];
            control.DisplayData(rampData, data);
        }

        public void UpdateLockRate(double time)
        {
            UIHelper.SetTextBox(updateRateTextBox, Convert.ToString(time));
        }

        public void ClearErrorGraph(string cavityName, string laserName)
        {
            LockControlPanel control = CavityPanels[cavityName].SlaveLaserPanels[laserName];
            control.ClearErrorGraph();
        }

        public void SetSummedVoltageTextBox(string cavityName, double value)
        {
            CavityPanels[cavityName].SetSummedVoltageTextBox(value);
        }

        public void SetVoltageIntoCavityTextBox(string cavityName, double value)
        {
            CavityPanels[cavityName].SetVoltageIntoCavityTextBox(value);
        }


        #endregion

        #region Panel Control

        public void SetLaserVoltageTextBox(string cavityName, string slaveName, double value)
        {
            CavityPanels[cavityName].SlaveLaserPanels[slaveName].SetLaserVoltage(value);
        }

        public void SetLaserSetPoint(string cavityName, string slaveName, double value)
        {
            CavityPanels[cavityName].SlaveLaserPanels[slaveName].SetLaserSetPoint(value);
        }

        public void SetLaserSDTextBox(string cavityName, string slaveName, double value)
        {
            CavityPanels[cavityName].SlaveLaserPanels[slaveName].SetLaserSD(value);
        }

        public void DisplayData(string cavityName, string slaveName, double[] cavityData, double[] slaveData)
        {
            CavityPanels[cavityName].SlaveLaserPanels[slaveName].DisplayData(cavityData, slaveData);
        }
        public void DisplayFit(string cavityName, string slaveName, double[] cavityData, double[] slaveFitData)
        {
            CavityPanels[cavityName].SlaveLaserPanels[slaveName].DisplayFit(cavityData, slaveFitData);
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

        private void NumberOfScanpointsTextBox_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (Int32.TryParse(NumberOfScanpointsTextBox.Text, out value))
            {
                if (NumberOfScanpointsTextBox.Focused)
                {
                    controller.NumberScanPointsChanged(value);
                }
            }
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
            bool canScale = !axisCheckBox.Checked;
            List<ScatterPlot> plots = new List<ScatterPlot>();

            plots.Add(cavityDataPlot);
            foreach (CavityControlPanel cavityPanel in CavityPanels.Values)
            {
                plots.Add(cavityPanel.MasterDataPlot);
                plots.Add(cavityPanel.MasterFitPlot);
                foreach (LockControlPanel slavePanel in cavityPanel.SlaveLaserPanels.Values)
                {
                    plots.Add(slavePanel.SlaveDataPlot);
                    plots.Add(slavePanel.SlaveFitPlot);
                }
            }

            foreach(ScatterPlot plot in plots)
            {
                plot.CanScaleXAxis = canScale;
                plot.CanScaleYAxis = canScale;
            }
        }



        private void loadProfileSetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.LoadParametersWithDialog();
        }

       
    }
}

