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
using DAQ.Environment;

namespace CaFBECHardwareController.Controls
{
    public partial class WindfreakTabView : GenericView
    {
        protected WindfreakTabController castController;

        public WindfreakTabView(WindfreakTabController controllerInstance) : base(controllerInstance)
        {
            InitializeComponent();
            castController = (WindfreakTabController)controller; // saves casting in every method
        }

        #region UI Update Handlers

        public void UpdateConnectionErrorIndicator(bool state)
        {
            connectionLED.Value = state;
        }

        public void UpdateFrequency(double freq)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<double>(UpdateFrequency), freq);
                return;
            }
            freqInput.Value = (decimal)freq;
        }

        public void UpdateAmplitude(double amp)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<double>(UpdateAmplitude), amp);
                return;
            }
            ampInput.Value = (decimal)amp;
        }

        public void UpdateSweepUpper(double freq)
        {
            upperSweepFreq.Value = (decimal)freq;
        }

        public void UpdateSweepLower(double freq)
        {
            lowerSweepFreq.Value = (decimal)freq;
        }

        public void UpdateSweepStepSize(double freqStep)
        {
            sweepStepSize.Value = (decimal)freqStep;
        }

        public void UpdateSweepStepTime(double time)
        {
            sweepStepTime.Value = (decimal)time;
        }

        public void UpdateSweepDirection(bool state)
        {
            sweepDirectionSwitch.Value = state;
        }

        public void UpdateOutput(bool state)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(UpdateOutput), state);
                return;
            }
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

        public double GetSweepUpper()
        {
            return (double)upperSweepFreq.Value;
        }

        public double GetSweepLower()
        {
            return (double)lowerSweepFreq.Value;
        }

        public double GetSweepStepSize()
        {
            return (double)sweepStepSize.Value;
        }

        public double GetSweepStepTime()
        {
            return (double)sweepStepTime.Value;
        }

        public bool GetSweepDirection()
        {
            return sweepDirectionSwitch.Value;
        }

        #endregion


        // UI Event Handlers

        private void ToggleChannel(object sender, NationalInstruments.UI.ActionEventArgs e)
        {
            castController.SyncChannel();
        }

        private void UpdateSettings(object sender, EventArgs e)
        {

            double freq = GetFrequency();
            double amp = GetAmplitude();
            //double sweepUpper = GetSweepUpper();
            //double sweepLower = GetSweepLower();
            //double sweepStepSize = GetSweepStepSize();
            //double sweepStepTime = GetSweepStepTime();
            bool channel = GetChannel();
            castController.SetFrequency(freq, channel);
            castController.SetAmplitude(amp, channel);
            //castController.SetSweepUpper(sweepUpper, channel);
            //castController.SetSweepLower(sweepLower, channel);
            //castController.SetSweepStepSize(sweepStepSize, channel);
            //castController.SetSweepStepTime(sweepStepTime, channel);
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

        private void SetSweepDirection(object sender, NationalInstruments.UI.ActionEventArgs e)
        {
            if (sweepDirectionSwitch.Focused) // Only do it if its a UI event
            {
                WindfreakTabController castController = (WindfreakTabController)controller;
                bool state = GetSweepDirection();
                bool channel = GetChannel();
                castController.SetSweepDirection(state, channel);
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            castController.Connect();
        }
    }
}
