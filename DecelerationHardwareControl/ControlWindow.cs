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

namespace DecelerationHardwareControl
{
    public partial class ControlWindow : Form
    {
        private double aom_voltage;
        public Controller controller;
        
        public ControlWindow()
        {
            InitializeComponent();
        }


        public void SetCheckBox(CheckBox box, bool state)
        {
            box.Invoke(new SetCheckDelegate(SetCheckHelper), new object[] { box, state });
        }
        private delegate void SetCheckDelegate(CheckBox box, bool state);
        private void SetCheckHelper(CheckBox box, bool state)
        {
            box.Checked = state;
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

        private void AomVoltageBox_ValueChanged(object sender, EventArgs e)
        {
            //the conversion factor below is liable to change over time. It will definitely change
            //if the gain on the voltage amplifier that aom_voltage goes through is changed.
            aom_voltage = ((double)AomVoltageBox.Value - 83.2) / 22.9143;
            controller.AOMVoltage = aom_voltage;
            //diodeSaturationError();
        }

        private void laserBlockCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            controller.SetAnalogOutputBlockedStatus("laser", laserBlockCheckBox.Checked);
        }

       

        
    }
}