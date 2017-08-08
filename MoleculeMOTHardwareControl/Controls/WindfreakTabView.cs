using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAQ.HAL;

namespace MoleculeMOTHardwareControl.Controls
{
    public partial class WindfreakTabView : MoleculeMOTHardwareControl.Controls.GenericView
    {
        protected WindfreakTabController castController;

        public WindfreakTabView() : base()
        {
            castController = (WindfreakTabController)controller; // saves casting in every method
        }

        #region UI Update Handlers

        public void UpdateFrequency(double freq)
        {
            freqInput.Value = (decimal)freq;
        }

        public void UpdateAmplitude(double amp)
        {
            ampInput.Value = (decimal)amp;
        }

        public void UpdateOutput(bool state)
        {
            outputSwitch.Value = state;
            outputIndicator.Value = state;
        } 

        public void UpdateTriggerMode(WindfreakSynth.TriggerTypes trigger)
        {
            triggerModeComboBox.SelectedItem = trigger;
        }

        public void InitializeTriggerModes(Array triggers)
        {
            triggerModeComboBox.DataSource = triggers;
        }

        #endregion


        #region UI Query handlers

        public double GetFrequency()
        {
            return (double)freqInput.Value;
        }

        public double GetAmplitude()
        {
            return (double)ampInput.Value;
        }

        public bool GetChannel()
        {
            return !channelSwitch.Value;
        }

        public bool GetOutput()
        {
            return outputSwitch.Value;
        }

        #endregion


        // UI Event Handlers

        private void ToggleChannel(object sender, NationalInstruments.UI.ActionEventArgs e)
        {
            castController.SyncChannel();
        }

        private void SetFrequencyAmplitude(object sender, EventArgs e)
        {
            double freq = GetFrequency();
            double amp = GetAmplitude();
            bool channel = GetChannel();
            castController.SetFrequency(freq, channel);
            castController.SetAmplitude(amp, channel);
        }

        private void ReadSettings(object sender, EventArgs e)
        {
            castController.ReadSettings();
        }

        private void SetOutput(object sender, EventArgs e)
        {
            if (outputSwitch.Focused) // Only do it if its a UI event
            {
                bool state = GetOutput();
                bool channel = GetChannel();
                castController.SetOutput(state, channel);
                outputIndicator.Value = state;
            }
        }

        private void SetTriggerMode(object sender, EventArgs e)
        {
            if (triggerModeComboBox.Focused) // Only do it if its a UI event
            {
                string value = triggerModeComboBox.SelectedItem.ToString();
                castController.SetTriggerMode(value);
            }
        }
    }
}
