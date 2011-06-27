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

        public void SetChamber1Pressure(double value)
        {
            setTextBox(chamber1PressureTextBox, Convert.ToString(value));
        }

        private void laserErrorMonitorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (laserErrorMonitorCheckBox.Checked)
            {
                controller.StartMonitoringLaserErrorSignal();
            }
            if (!laserErrorMonitorCheckBox.Checked)
            {
                controller.StopMonitoringLaserErrorSignal();
            }
        }
        #endregion

    }
}
