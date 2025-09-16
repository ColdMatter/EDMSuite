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

namespace CaFBECHardwareController.Controls
{
    public partial class GigatronicsTabView : GenericView
    {
        protected GigatronicsTabController castController;

        public GigatronicsTabView(GigatronicsTabController controllerInstance)
            : base(controllerInstance)
        {
            InitializeComponent();
            castController = (GigatronicsTabController)controller; // saves casting in every method
        }

        #region UI Update Handlers

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

        public void UpdateOutput(bool state)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(UpdateOutput), state);
                return;
            }
            toggleLED(outputIndicator, state);
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
            return readLED(outputIndicator);
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
            if (outputButton.Focused) // Only do it if its a UI event
            {
                bool state = GetOutput();
                castController.SetOutput(!state);
                toggleLED(outputIndicator, !state);
            }
        }

        private void toggleLED(Panel led, bool state)
        {
            if (state)
            {
                led.BackColor = Color.Green;
            }
            else
            {
                led.BackColor = Color.Red;
            }
        }
        private bool readLED(Panel led)
        {
            bool state;
            if (led.BackColor == Color.Green)
            {
                state = true;
            }
            else
            {
                state = false;
            }
            return state;
        }

        private void outputButton_Click(object sender, EventArgs e)
        {
            if (outputButton.Focused) // Only do it if its a UI event
            {
                bool state = GetOutput();
                castController.SetOutput(!state);
                toggleLED(outputIndicator, !state);
            }
        }
    }
}
