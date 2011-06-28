using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SympatheticHardwareControl
{
    public partial class HardwareMonitorWindow : Form
    {
        public Controller controller;

        public HardwareMonitorWindow()
        {
            InitializeComponent();
        }

        #region ThreadSafe wrappers

        private void setCheckBox(CheckBox box, bool state)
        {
            box.Invoke(new setCheckDelegate(setCheckHelper), new object[] { box, state });
        }
        private delegate void setCheckDelegate(CheckBox box, bool state);
        private void setCheckHelper(CheckBox box, bool state)
        {
            box.Checked = state;
        }

        private void setTextBox(TextBox box, string text)
        {
            box.Invoke(new setTextDelegate(setTextHelper), new object[] { box, text });
        }
        private delegate void setTextDelegate(TextBox box, string text);
        private void setTextHelper(TextBox box, string text)
        {
            box.Text = text;
        }

        private void setLED(NationalInstruments.UI.WindowsForms.Led led, bool val)
        {
            led.Invoke(new SetLedDelegate(SetLedHelper), new object[] { led, val });
        }
        private delegate void SetLedDelegate(NationalInstruments.UI.WindowsForms.Led led, bool val);
        private void SetLedHelper(NationalInstruments.UI.WindowsForms.Led led, bool val)
        {
            led.Value = val;
        }

        private void setLEDColour(NationalInstruments.UI.WindowsForms.Led led, Color val)
        {
            led.Invoke(new SetLedColourDelegate(SetLedColourHelper), new object[] { led, val });
        }
        private delegate void SetLedColourDelegate(NationalInstruments.UI.WindowsForms.Led led, Color val);
        private void SetLedColourHelper(NationalInstruments.UI.WindowsForms.Led led, Color val)
        {
            led.OnColor = val;
        }

        #endregion

        #region Public Methods

        public void SetLaserErrorSignal(double value, Color ledColour)
        {
            setLEDColour(laserErrorLED, ledColour);
            setTextBox(laserErrorMonitorTextbox, Convert.ToString(value));
        }

       
        private void laserErrorMonitorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (laserErrorMonitorCheckBox.Checked)
            {
                setLED(laserErrorLED, true);
                controller.StartMonitoringLaserErrorSignal();
            }
            if (!laserErrorMonitorCheckBox.Checked)
            {
                setLED(laserErrorLED, false);
                controller.StopMonitoringLaserErrorSignal();
            }
        }
        public double GetLaserErrorSignalThreshold()
        {
            return Double.Parse(laserLockErrorThresholdTextBox.Text);
        }


        public void SetChamber1Pressure(double value)
        {
            setTextBox(chamber1PressureTextBox, Convert.ToString(value));
        }
        private void chamber1PressureCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if(chamber1PressureCheckBox.Checked)
            {
                controller.StartChamber1PressureMonitor();
            }
            if (!chamber1PressureCheckBox.Checked)
            {
                controller.StopChamber1PressureMonitor();
            }
        }

        #endregion
        #region Menu
        private void startAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            laserErrorMonitorCheckBox.Checked = true;
            chamber1PressureCheckBox.Checked = true;
        }

        private void stopAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopAll();
        }
        private void stopAll()
        {
            laserErrorMonitorCheckBox.Checked = false;
            chamber1PressureCheckBox.Checked = false;
        }
        #endregion

        private void HardwareMonitorWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopAll();
        }

        

    }
}
