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
        public Controller controller;

        public LockControlPanel(string name)
        {
            this.name = name;
            InitializeComponent();
        }

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


        private delegate void plotScatterGraphDelegate(ScatterPlot plot,
            double[] x, double[] y);
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
        #endregion

        #region Events
        private void setPointAdjustPlusButton_Click(object sender, EventArgs e)
        {
            lock (controller.tweakLock)
            {
                controller.AddSetPointIncrement(name);
            }
        }

        private void setPointAdjustMinusButton_Click(object sender, EventArgs e)
        {
            lock (controller.tweakLock)
            {
                controller.AddSetPointDecrement(name);
            }
        }


        private void lockEnableCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (lockEnableCheck.CheckState == CheckState.Checked)
            {
                controller.EngageLock(name);
            }
            if (lockEnableCheck.CheckState == CheckState.Unchecked)
            {
                controller.DisengageLock(name);
            }

        }


        private void VoltageToLaserChanged(object sender, EventArgs e)
        {
            try
            {
                controller.VoltageToLaserChanged(name, Double.Parse(VoltageToLaserTextBox.Text));
            }
            catch (Exception)
            {
            }
        }

        private void GainChanged(object sender, EventArgs e)
        {
            try
            {
                controller.GainChanged(name, Double.Parse(GainTextbox.Text));
            }
            catch (Exception)
            {

            }
        }
        private void setPointIncrementBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                controller.SetPointIncrementSize(name, Double.Parse(setPointIncrementBox.Text));
            }
            catch { }
        }
        #endregion

        #region Setting and getting parameter values from textboxes

        public void SetLaserVoltage(double value)
        {
            SetTextBox(VoltageToLaserTextBox, Convert.ToString(value));
        }

        public void SetGain(double value)
        {
            SetTextBox(GainTextbox, Convert.ToString(value));
        }
        public double GetGain()
        {
            return Double.Parse(GainTextbox.Text);
        }

        public void SetSetPointIncrementSize(double value)
        {
            SetTextBox(setPointIncrementBox, Convert.ToString(value));
        }

        public double GetLaserSetPoint()
        {
            return Double.Parse(LaserSetPointTextBox.Text);
        }
        public void SetLaserSetPoint(double value)
        {
            SetTextBox(LaserSetPointTextBox, Convert.ToString(value));
        }

        public void DisplayData(double[] cavityData, double[] slaveData)
        {
            scatterGraphPlot(SlaveLaserIntensityScatterGraph,
                SlaveDataPlot, cavityData, slaveData);
        }
        public void DisplayFit(double[] cavityData, double[] slaveData)
        {
            scatterGraphPlot(SlaveLaserIntensityScatterGraph, SlaveFitPlot, cavityData, slaveData);
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
                    break;

                case SlaveLaser.LaserState.LOCKING:
                    VoltageToLaserTextBox.Enabled = false;
                    GainTextbox.Enabled = false;
                    lockedLED.Value = false;
                    break;

                case SlaveLaser.LaserState.LOCKED:
                    lockedLED.Value = true;
                    break;

            }
        }
        #endregion
    }
}
