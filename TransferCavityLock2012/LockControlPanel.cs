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

        public LockControlPanel(string name, double lowerVoltageLimit, double upperVoltageLimit, double igain, double pgain)
        {
            this.name = name;
            this.upperVoltageLimit = upperVoltageLimit;
            this.lowerVoltageLimit = lowerVoltageLimit;
            InitializeComponent();
            this.IGainTextbox.Text = igain.ToString();
            this.PGainTextbox.Text = pgain.ToString();
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
            if (!VoltageToLaserTextBox.Focused && !VoltageTrackBar.Focused) return;
            double number;
            System.Globalization.NumberStyles numberStyle = System.Globalization.NumberStyles.Number; // Determines what formats are allowed for numbers
            if (Double.TryParse(VoltageToLaserTextBox.Text, numberStyle, System.Globalization.CultureInfo.InvariantCulture, out number))
            {
                Controller.VoltageToSlaveLaserChanged(CavityPanel.CavityName, name, number);
            }
            else
            {
                Controller.ShowDialog(
                    "Invalid entry",
                    "Couldn't convert '" + VoltageToLaserTextBox.Text + "' to a double."
                );
            }
        }

        private void GainChanged(object sender, EventArgs e)
        {
            try
            {
                Controller.SlaveIGainChanged(CavityPanel.CavityName, name, Double.Parse(IGainTextbox.Text));
            }
            catch (Exception)
            {

            }
        }

        private void PGainChanged(object sender, EventArgs e)
        {
            try
            {
                Controller.SlavePGainChanged(CavityPanel.CavityName, name, Double.Parse(PGainTextbox.Text));
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

        public void SetOperatingLED(bool locked, bool normalOperatingRange)
        {
            UIHelper.SetLEDState(lockedLED, locked);
            Color color = normalOperatingRange ? Color.Lime : Color.DarkOrange;
            UIHelper.SetLEDColor(lockedLED, color);
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
            UIHelper.ScatterGraphPlot(SlaveLaserIntensityScatterGraph,
                SlaveDataPlot, cavityData, slaveData);
        }
        public void DisplayFit(double[] cavityData, double[] slaveData)
        {
            UIHelper.ScatterGraphPlot(SlaveLaserIntensityScatterGraph, SlaveFitPlot, cavityData, slaveData);
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
                    UIHelper.EnableControl(VoltageToLaserTextBox, true);
                    UIHelper.EnableControl(LaserSetPointTextBox, false);
                    UIHelper.EnableControl(IGainTextbox, true);
                    UIHelper.EnableControl(PGainTextbox, true);
                    UIHelper.SetLEDState(lockedLED, false);
                    UIHelper.EnableControl(VoltageTrackBar, true);
                    break;

                case SlaveLaser.LaserState.LOCKING:
                    UIHelper.EnableControl(VoltageToLaserTextBox, false);
                    UIHelper.EnableControl(IGainTextbox, false);
                    UIHelper.EnableControl(PGainTextbox, false);
                    UIHelper.SetLEDState(lockedLED, false);
                    UIHelper.EnableControl(VoltageTrackBar, false);
                    break;

                case SlaveLaser.LaserState.LOCKED:
                    UIHelper.SetLEDState(lockedLED, true);
                    break;
            }
        }

        public void EnableLocking()
        {
            UIHelper.EnableControl(lockEnableCheck, true);
        }

        #endregion

    }
}
