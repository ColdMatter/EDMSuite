using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WavemeterLock
{
    public partial class LockControlPanel : UserControl
    {

        public string name;
        public string analogChannel;
        private int channelNumber = 0;
        double setFrequency = 0;
        public Controller controller;
        

        public LockControlPanel(string name, string AnalogChannel, Controller controller)
        {
            this.name = name;
            this.controller = controller;
            analogChannel = AnalogChannel;
            InitializeComponent();
            
        }


        private void LockControlPanel_Load(object sender, EventArgs e)
        {
            
        }

        public void updatePanel()
        {

            displayWL.Text = controller.displayWL(channelNumber);
            displayFreq.Text = controller.displayFreq(channelNumber);
            lockMsg.Text = controller.getLaserState(name);
            frequencyError.Text = Convert.ToString(1000000 * controller.gerFrequencyError(name));


            if (!controller.returnLaserState(name))
            {
                lockButton.Text = "Lock";

            }
            else
            {
                lockButton.Text = "Unlock";
            }

            VOut.Text = Convert.ToString(controller.getOutputvoltage(name));
        }

        #region Events

        private void lockButton_Click(object sender, EventArgs e)
        {
            setFrequency = Convert.ToDouble(SetPoint.Text);
            controller.setFrequency(name, setFrequency);

            if (!controller.returnLaserState(name))
            {
                if (SetPoint != null && !string.IsNullOrWhiteSpace(SetPoint.Text))
                {
                    lockMsg.Text = "Lock On";
                    lockLED.Value = true;
                    setFrequency = Convert.ToDouble(SetPoint.Text);
                    controller.EngageLock(name);
                    SetPoint.Enabled = false;
                }
                else
                {
                    lockMsg.Text = "Null Input!";
                }
            }

            else
            {
                lockMsg.Text = "Lock Off";
                lockLED.Value = false;
                controller.DisengageLock(name);
                SetPoint.Enabled = true;
            }


        }

        private void showButton_Click(object sender, EventArgs e)
        {
            try
            {
                channelNumber = Convert.ToInt32(LockChannelNumber.Text);
                controller.setChannel(name, channelNumber);
                errorMsg.Text = "";
            }
            catch (FormatException)
            {
                errorMsg.Text = "Format Error!";
            }

            
        }

        private void PGainSet_Click(object sender, EventArgs e)
        {
            controller.setPGain(name, Convert.ToDouble(PGain.Text));
        }

        private void IGainSet_Click(object sender, EventArgs e)
        {
            controller.setIGain(name, Convert.ToDouble(IGain.Text));
        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            controller.resetOutput(name);
        }

        private void stepUpBtn_Click(object sender, EventArgs e)
        {
            setFrequency += Convert.ToDouble(stepSize.Text) / 1000000;
            controller.setFrequency(name, setFrequency);
            SetPoint.Text = Convert.ToString(setFrequency);
        }

        private void stepDownBtn_Click(object sender, EventArgs e)
        {
            setFrequency -= Convert.ToDouble(stepSize.Text) / 1000000;
            controller.setFrequency(name, setFrequency);
            SetPoint.Text = Convert.ToString(setFrequency);
        }

        #endregion
    }
}
