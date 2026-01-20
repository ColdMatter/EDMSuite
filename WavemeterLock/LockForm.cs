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
    public partial class LockForm : Form
    {
        public Controller controller;
        Dictionary<string, LockControlPanel> panelList = new Dictionary<string, LockControlPanel>();
        public bool isLockAll = false;

        public LockForm()
        {
            InitializeComponent();
        }

        public void AddLaserControlPanel(string laserName, string channel, int wavemeterChannle)
        {
            TabPage newTab = new TabPage(laserName);
            LockControlPanel panel = new LockControlPanel(laserName, channel, wavemeterChannle, controller);
            panelList.Add(laserName, panel);
            newTab.Controls.Add(panel);
            lockTab.TabPages.Add(newTab);
            panel.Enabled = false;

        }

        public void UpdateLockRate(double time)
        {
            UIHelper.SetTextBox(updateRateTextBox, Convert.ToString(Convert.ToInt32(time)));
        }


        private void timer1_Tick(object sender, EventArgs e)
        {

            foreach (string slavePanel in panelList.Keys)
            {
                panelList[slavePanel].updatePanel();
            }

            if (controller.WMLState == Controller.ControllerState.RUNNING)
            {
                wmlLED.Value = true;
                masterBttn.Text = "Stop WML";

            }

            else
            {
                wmlLED.Value = false;
                masterBttn.Text = "Start WML";
            }

            if (isLockAll)
            {
                button_lock_all.Text = "Unlock All";
            }

            else
            {
                button_lock_all.Text = "Lock All";
            }

            foreach(Laser laser in controller.lasers.Values)
            {
                int n = laser.WLMChannel;
                if(laser.lState == Laser.LaserState.LOCKED)
                {
                    toggle_led(n, true);
                }

                else
                {
                    toggle_led(n, false);
                }
            }

        }

        private void masterBttn_Click(object sender, EventArgs e)
        {
            if (controller.WMLState == Controller.ControllerState.RUNNING)
            {
                controller.WMLState = Controller.ControllerState.STOPPED;
                foreach (LockControlPanel panel in panelList.Values)
                {
                    panel.Enabled = false;
                }
                button_lock_all.Enabled = false;
            }

            else
            {
                controller.WMLState = Controller.ControllerState.RUNNING;
                controller.startWML();
                foreach (LockControlPanel panel in panelList.Values)
                {
                    panel.Enabled = true;
                }
                button_lock_all.Enabled = true;
            }


        }

        private void lock_all (object sender, EventArgs e)
        {
            if (isLockAll)
            {
                foreach(Laser laser in controller.lasers.Values)
                {
                    laser.DisengageLock();
                    controller.indicateRemoteConnection(laser.WLMChannel, false);
                }
                isLockAll = false;
            }

            else
            {
                foreach (Laser laser in controller.lasers.Values)
                {
                    laser.Lock();
                    controller.indicateRemoteConnection(laser.WLMChannel, true);
                }
                isLockAll = true;
            }
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

        private void TestLable_Click(object sender, EventArgs e)
        {

        }

        private void LockForm_Load(object sender, EventArgs e)
        {

        }

        private void lockTab_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBoxLockRate_Enter(object sender, EventArgs e)
        {

        }

        private void LockForm_Closing(object sender, FormClosingEventArgs e)
        {
            foreach (Laser laser in controller.lasers.Values)
            {
                controller.DisengageLock(laser.Name);
            }

            controller.removeWavemeterLock();
            Application.Exit();
        }

        //Very professional coding:
        public void enable_LED(int n)
        {
            switch (n)
            {
                case 1:
                    led1.Enabled = true;
                    led1.Visible = true;
                    break;
                case 2:
                    led2.Enabled = true;
                    led2.Visible = true;
                    break;
                case 3:
                    led3.Enabled = true;
                    led3.Visible = true;
                    break;
                case 4:
                    led4.Enabled = true;
                    led4.Visible = true;
                    break;
                case 5:
                    led5.Enabled = true;
                    led5.Visible = true;
                    break;
                case 6:
                    led6.Enabled = true;
                    led6.Visible = true;
                    break;
                case 7:
                    led7.Enabled = true;
                    led7.Visible = true;
                    break;
                case 8:
                    led8.Enabled = true;
                    led8.Visible = true;
                    break;
                default:
                    break;
            }
        }

        public void toggle_led(int n, bool val)
        {
            switch (n)
            {
                case 1:
                    led1.Value = val;
                    break;
                case 2:
                    led2.Value = val;
                    break;
                case 3:
                    led3.Value = val;
                    break;
                case 4:
                    led4.Value = val;
                    break;
                case 5:
                    led5.Value = val;
                    break;
                case 6:
                    led6.Value = val;
                    break;
                case 7:
                    led7.Value = val;
                    break;
                case 8:
                    led8.Value = val;
                    break;
                default:
                    break;
            }
        }

        private void saveSetPointsButton_Click(object sender, EventArgs e)
        {
            controller.logSetPoints(true);
        }

        private void loadSetPointsButton_Click(object sender, EventArgs e)
        {
            controller.loadSetPoints();
        }
    }
}

       
