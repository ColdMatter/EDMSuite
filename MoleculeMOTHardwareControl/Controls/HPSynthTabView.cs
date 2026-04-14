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
    public partial class HPSynthTabView : MoleculeMOTHardwareControl.Controls.GenericView
    {
        protected HPSynthTabController castController;

        public HPSynthTabView(HPSynthTabController controllerInstance)
            : base(controllerInstance)
        {
            InitializeComponent();
            castController = (HPSynthTabController)controller; // saves casting in every method
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

        public bool GetOutput()
        {
            return outputSwitch.Value;
        }
        #endregion


        // UI Event Handlers

        private void UpdateSettings(object sender, EventArgs e)
        {
            double freq = GetFrequency();
            double amp = GetAmplitude();
            
            castController.SetFrequency(freq);
            castController.SetAmplitude(amp);
        }

        private void SetOutput(object sender, EventArgs e)
        {
            if (outputSwitch.Focused) // Only do it if its a UI event
            {
                bool state = GetOutput();
                castController.SetOutput(state);
                outputIndicator.Value = state;
            }
        }
    }
}
