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

namespace TransferCavityLock
{
    /// <summary>
    /// Front panel of the laser controller
    /// </summary>
    public partial class MainForm : Form
    {
        public Controller controller;


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
        
        /// <summary>
        /// This controls which parts of the UI are enabled for a given machine state.
        /// </summary>
        public void updateUIState(Controller.ControllerState state)
        {
            switch (state)
            {
                case Controller.ControllerState.STOPPED:
                    rampStartButton.Enabled = true;
                    rampStopButton.Enabled = false;

                    lockEnableCheck.Enabled = false;

                    fitAndStabilizeEnableCheck.Enabled = false;

                    VoltageToLaserTextBox.Enabled = false;
                    LaserSetPointTextBox.Enabled = false;
                    GainTextbox.Enabled = false;

                    NumberOfScanpointsTextBox.Enabled = true;
                    CavityScanWidthTextBox.Enabled = true;
                    CavityScanOffsetTextBox.Enabled = true;

                    rampLED.Value = false;
                    break;

                case Controller.ControllerState.FREERUNNING:
                    rampStartButton.Enabled = false;
                    rampStopButton.Enabled = true;

                    fitAndStabilizeEnableCheck.Enabled = true;
                    fitAndStabilizeEnableCheck.Checked = false;

                    lockEnableCheck.Enabled = false;
                    VoltageToLaserTextBox.Enabled = false;
                    LaserSetPointTextBox.Enabled = false;
                    GainTextbox.Enabled = false;

                    NumberOfScanpointsTextBox.Enabled = false;
                    CavityScanWidthTextBox.Enabled = false;
                    CavityScanOffsetTextBox.Enabled = false;

                    rampLED.Value = true;
                    break;

                case Controller.ControllerState.LASERLOCKING:
                    VoltageToLaserTextBox.Enabled = false;
                    GainTextbox.Enabled = false;
                    break;

                case Controller.ControllerState.LASERLOCKED:

                    break;

                case Controller.ControllerState.CAVITYSTABILIZED:
                    VoltageToLaserTextBox.Enabled = true;
                    LaserSetPointTextBox.Enabled = true;
                    lockEnableCheck.Enabled = true;
                    GainTextbox.Enabled = true;
                    break;

            }
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

        public void SetRadioButton(RadioButton button, bool state)
        {
            button.Invoke(new SetRadioButtonDelegate(SetRadioButtonHelper), new object[] { button, state });
        }
        private delegate void SetRadioButtonDelegate(RadioButton button, bool state);
        private void SetRadioButtonHelper(RadioButton button, bool state)
        {
            button.Checked = state;
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
        public void ScatterGraphPlot(ScatterGraph graph, ScatterPlot plot, double[] x, double[] y)
        {
            graph.Invoke(new plotScatterGraphDelegate(plotScatterGraphHelper), new object[] {plot, x, y });
        }





        private void rampStartButton_Click(object sender, EventArgs e)
        {
            controller.StartRamp();
        }

        private void rampStopButton_Click(object sender, EventArgs e)
        {
            lockEnableCheck.Checked = false;
            fitAndStabilizeEnableCheck.Checked = false;
            
            lock (controller.rampStopLock)
            {
                controller.StopRamp();
            }

        }

        private void setPointAdjustPlusButton_Click(object sender, EventArgs e)
        {
            lock (controller.tweakLock)
            {
                controller.Increments++;
            }
        }

        private void setPointAdjustMinusButton_Click(object sender, EventArgs e)
        {
            lock (controller.tweakLock)
            {
                controller.Decrements++;
            }
        }


        private void fitAndStabilizeEnableCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (fitAndStabilizeEnableCheck.CheckState == CheckState.Checked)
            {
                controller.StabilizeCavity();
            }
            if (fitAndStabilizeEnableCheck.CheckState == CheckState.Unchecked)
            {
                controller.UnlockCavity();
            }
        }

        private void lockEnableCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (lockEnableCheck.CheckState == CheckState.Checked)
            {
                controller.EngageLock();
            }
            if (lockEnableCheck.CheckState == CheckState.Unchecked)
            {
                controller.DisengageLock();
            }

        }


        private void VoltageToLaserChanged(object sender, EventArgs e)
        {
            try
            {
                controller.WindowVoltageToLaserChanged(Double.Parse(VoltageToLaserTextBox.Text));
            }
            catch (Exception)
            {
            }
        }

        private void GainChanged(object sender, EventArgs e)
        {
            try
            {
                controller.WindowGainChanged(Double.Parse(GainTextbox.Text));
            }
            catch (Exception)
            {
                
            }
        }

        
        #endregion

        #region Setting and getting parameter values from textboxes
        
        public void SetLaserVoltage(double value)
        {
            SetTextBox(VoltageToLaserTextBox, Convert.ToString(value));
        }

        public void SetNumberOfPoints(int value)
        {
            SetTextBox(NumberOfScanpointsTextBox, Convert.ToString(value));
        }
        public int GetNumberOfPoints()
        {
            return Int32.Parse(NumberOfScanpointsTextBox.Text);
        }

        public void SetGain(double value)
        {
            SetTextBox(GainTextbox, Convert.ToString(value)); 
        }
        public double GetGain()
        {
            return Double.Parse(GainTextbox.Text);
        }

        public double GetScanWidth()
        {
            return Double.Parse(CavityScanWidthTextBox.Text);
        }
        
        public void SetScanWidth(double value)
        {           
            SetTextBox(CavityScanWidthTextBox, Convert.ToString(value)); 
        }

        public void SetSetPointIncrementSize(double value)
        {
            SetTextBox(setPointIncrementBox, Convert.ToString(value)); 
        }

        public double GetScanOffset()
        {
            return Double.Parse(CavityScanOffsetTextBox.Text);
        }
        public void SetScanOffset(double value)
        {
            SetTextBox(CavityScanOffsetTextBox, Convert.ToString(value)); 
        }
        public double GetLaserSetPoint()
        {
            return Double.Parse(LaserSetPointTextBox.Text);
        }
        public void SetLaserSetPoint(double value)
        {
            SetTextBox(LaserSetPointTextBox, Convert.ToString(value));
        }
        #endregion

        private void setPointIncrementBox_TextChanged(object sender, EventArgs e)
        {
            controller.setPointIncrementSize = Double.Parse(setPointIncrementBox.Text);
        }


        
    }
}

