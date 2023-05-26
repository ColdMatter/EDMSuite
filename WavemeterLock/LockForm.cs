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
    public partial class LockForm : Form
    {
        public Controller controller;
        Dictionary<string, LockControlPanel> panelList = new Dictionary<string, LockControlPanel>();

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
            
            foreach(string slavePanel in panelList.Keys)
            {
                panelList[slavePanel].updatePanel();
            }

            if(controller.WMLState == Controller.ControllerState.RUNNING)
            {
                wmlLED.Value = true;
                masterBttn.Text = "Stop WML";
                
            }

            else
            {
                wmlLED.Value = false;
                masterBttn.Text = "Start WML";
            }

                  
        }

        private void masterBttn_Click(object sender, EventArgs e)
        {
            if (controller.WMLState == Controller.ControllerState.RUNNING)
            {
                controller.WMLState = Controller.ControllerState.STOPPED;
                foreach(LockControlPanel panel in panelList.Values)
                {
                    panel.Enabled = false;
                }
            }

            else
            {
                controller.WMLState = Controller.ControllerState.RUNNING;
                controller.startWML();
                foreach (LockControlPanel panel in panelList.Values)
                {
                    panel.Enabled = true;
                }
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
    }
}

       
