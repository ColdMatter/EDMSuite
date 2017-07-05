using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MoleculeMOTHardwareControl.Controls
{
    public partial class WindfreakTabView : MoleculeMOTHardwareControl.Controls.GenericView
    {
        public WindfreakTabView()
        {
            InitializeComponent();
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
            WindfreakTabController castController = (WindfreakTabController)controller;
            castController.SyncChannel();
        }

        private void SetFrequencyAmplitude(object sender, EventArgs e)
        {
            WindfreakTabController castController = (WindfreakTabController)controller;
            castController.SetFreqAmp();
        }

        private void ReadSettings(object sender, EventArgs e)
        {
            WindfreakTabController castController = (WindfreakTabController)controller;
            castController.ReadSettings();
        }

        private void SetOutput(object sender, EventArgs e)
        {
            if (outputSwitch.Focused) // Only do it if its a UI event
            {
                WindfreakTabController castController = (WindfreakTabController)controller;
                bool state = GetOutput();
                castController.SetOutput(state);
                outputIndicator.Value = state;
            }
        }

        private void SetTriggerMode(object sender, EventArgs e)
        {
            if (triggerModeComboBox.Focused) // Only do it if its a UI event
            {
                WindfreakTabController castController = (WindfreakTabController)controller;
                string value = triggerModeComboBox.SelectedValue.ToString();
                castController.SetTriggerMode(value);
            }
        }
    }
}
