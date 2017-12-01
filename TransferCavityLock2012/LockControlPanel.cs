using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NationalInstruments.UI.WindowsForms;
using NationalInstruments.UI;

namespace TransferCavityLock2012
{
    public partial class LockControlPanel : UserControl
    {
        private string name;
        private double upperVoltageLimit = 10;
        private double lowerVoltageLimit = 0;

        public CavityControlPanel CavityPanel;
        public Controller Controller;

        public LockControlPanel()
        {
            this.name = "Test";
            InitializeComponent();
        }

        public LockControlPanel(string name)
        {
            this.name = name;
            InitializeComponent();
        }

        public LockControlPanel(string name, double lowerVoltageLimit, double upperVoltageLimit, double gain)
        {
            this.name = name;
            this.upperVoltageLimit = upperVoltageLimit;
            this.lowerVoltageLimit = lowerVoltageLimit;
            InitializeComponent();
            this.GainTextbox.Text = gain.ToString();
        }

        #region Events
        private void setPointAdjustPlusButton_Click(object sender, EventArgs e)
        {
            double setPointIncrement = Double.Parse(setPointIncrementBox.Text);
            lock (Controller.tweakLock)
            {
                Controller.AdjustSetPoint(CavityPanel.CavityName, name, setPointIncrement);
            }
        }

        private void setPointAdjustMinusButton_Click(object sender, EventArgs e)
        {
            double setPointIncrement = Double.Parse(setPointIncrementBox.Text);
            lock (Controller.tweakLock)
            {
                Controller.AdjustSetPoint(CavityPanel.CavityName, name, -setPointIncrement);
            }
        }

        private void lockEnableCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (lockEnableCheck.CheckState == CheckState.Checked)
            {
                Controller.EngageLock(CavityPanel.CavityName, name);
            }
            if (lockEnableCheck.CheckState == CheckState.Unchecked)
            {
                Controller.DisengageLock(CavityPanel.CavityName, name);
            }
        }


        private void VoltageToLaserChanged(object sender, EventArgs e)
        {
            try
            {
                Controller.VoltageToSlaveLaserChanged(CavityPanel.CavityName, name, Double.Parse(VoltageToLaserTextBox.Text));
            }
            catch (Exception)
            {
            }
        }

        private void GainChanged(object sender, EventArgs e)
        {
            try
            {
                Controller.SlaveGainChanged(CavityPanel.CavityName, name, Double.Parse(GainTextbox.Text));
            }
            catch (Exception)
            {

            }
        }

        private void slErrorResetButton_Click(object sender, EventArgs e)
        {
            ClearErrorGraph();
        }

        private void VoltageTrackBar_Scroll(object sender, EventArgs e)
        {

            SetLaserVoltage(((((double)VoltageTrackBar.Value) / 1000) * (upperVoltageLimit - lowerVoltageLimit)) + lowerVoltageLimit);
        }

        #endregion

        #region Setting and getting parameter values from textboxes

        public void SetLaserVoltage(double value)
        {
            UIHelper.SetTextBox(VoltageToLaserTextBox, Convert.ToString(value));
        }

        public void SetLaserSetPoint(double value)
        {
            UIHelper.SetTextBox(LaserSetPointTextBox, Convert.ToString(value));
        }

        public void SetLaserSD(double value)
        {
            UIHelper.SetTextBox(rmsVoltageDeviationTextBox, Convert.ToString(value));
        }

        public void DisplayData(double[] cavityData, double[] slaveData)
        {
            UIHelper.scatterGraphPlot(SlaveLaserIntensityScatterGraph,
                SlaveDataPlot, cavityData, slaveData);
        }
        public void DisplayFit(double[] cavityData, double[] slaveData)
        {
            UIHelper.scatterGraphPlot(SlaveLaserIntensityScatterGraph, SlaveFitPlot, cavityData, slaveData);
        }

        public void AppendToErrorGraph(int lockCount, double error)
        {
            UIHelper.appendPointToScatterGraph(ErrorScatterGraph, ErrorPlot, lockCount, error);
        }

        public void ClearErrorGraph()
        {
            UIHelper.ClearGraph(ErrorScatterGraph);
        }


        #endregion

        #region UI state control
        public void UpdateUIState(SlaveLaser.LaserState state)
        {
            switch (state)
            {
                case SlaveLaser.LaserState.FREE:
                    lockEnableCheck.Enabled = true;
                    VoltageToLaserTextBox.Enabled = true;
                    LaserSetPointTextBox.Enabled = false;
                    GainTextbox.Enabled = true;
                    lockedLED.Value = false;
                    VoltageTrackBar.Enabled = true;
                    break;

                case SlaveLaser.LaserState.LOCKING:
                    VoltageToLaserTextBox.Enabled = false;
                    GainTextbox.Enabled = false;
                    lockedLED.Value = false;
                    VoltageTrackBar.Enabled = false;
                    break;

                case SlaveLaser.LaserState.LOCKED:
                    lockedLED.Value = true;
                    break;
            }
        }

        #endregion
    }
}
