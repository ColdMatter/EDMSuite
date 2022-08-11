using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using wlmData;

namespace WavemeterLock
{
    public partial class Form1 : Form
    {
        public Controller controller;
        public Form1()
        {
            InitializeComponent();
        }


        private int channelNumber = 0;
        bool lockOn = false;
        double setFrequency = 0;

        private void lockButton_Click(object sender, EventArgs e)
        {
            setFrequency = Convert.ToDouble(SetPoint.Text);
            controller.setFrequency(setFrequency);

            if (!lockOn)
            {
                if (SetPoint != null && !string.IsNullOrWhiteSpace(SetPoint.Text))
                {
                    lockOn = true;
                    lockMsg.Text = "Lock On";
                    setFrequency = Convert.ToDouble(SetPoint.Text);
                    controller.EngageLock();
                    SetPoint.Enabled = false;
                }
                else
                {
                    lockMsg.Text = "Null Input!";
                }
            }

            else
            {
                lockOn = false;
                lockMsg.Text = "Lock Off";
                controller.DisengageLock();
                SetPoint.Enabled = true;
            }


        }

        private void showButton_Click(object sender, EventArgs e)
        {
            channelNumber = Convert.ToInt32(LockChannelNumber.Text);
            controller.setChannel(channelNumber);
        }

        private void PGainSet_Click(object sender, EventArgs e)
        {
            controller.setPGain(Convert.ToDouble(PGain.Text));
        }

        private void IGainSet_Click(object sender, EventArgs e)
        {
            controller.setIGain(Convert.ToDouble(IGain.Text));
        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            controller.resetOutput();
        }

        private void stepUpBtn_Click(object sender, EventArgs e)
        {
            setFrequency += Convert.ToDouble(stepSize.Text)/1000000;
            controller.setFrequency(setFrequency);
            SetPoint.Text = Convert.ToString(setFrequency);
        }

        private void stepDownBtn_Click(object sender, EventArgs e)
        {
            setFrequency -= Convert.ToDouble(stepSize.Text)/1000000;
            controller.setFrequency(setFrequency);
            SetPoint.Text = Convert.ToString(setFrequency);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            displayWL.Text = controller.displayWL(channelNumber);
            displayFreq.Text = controller.displayFreq(channelNumber);
            loopCount.Text = Convert.ToString(controller.getLoopCount());
            laserState.Text = controller.getLaserState();
            frequencyError.Text = Convert.ToString(1000000 * controller.gerFrequencyError());
            channelNum.Text = controller.getChannelNum();
            

            if (!lockOn)
            {
                lockButton.Text = "Lock";

            }
            else
            {
                lockButton.Text = "Unlock";
            }
            VOut.Text = Convert.ToString(controller.getOutputvoltage());
        }



        private void LockChannelNumber_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }




        private void displayWL_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }
    }
}

       
