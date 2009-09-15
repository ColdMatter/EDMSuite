using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace TransferCavityLock
{
    /// <summary>
    /// Front panel of the laser controller
    /// </summary>
    public partial class MainForm : Form
    {
        public DeadBolt controller;
        
        #region load Mainform

        public MainForm()
        {
            InitializeComponent();
            this.vRampIntButton.Checked = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
        #endregion

        #region passing values set by UI into program

        public void SetRampChannel()
        {
            controller.RampChannel = rampChannelMenu.SelectedText;
            this.AddToTextBox("Ramp channel selected.");
        }
        public void SetTriggerMethod()
        {
            if (this.vRampIntButton.Checked == true)
            {
                controller.RampTriggerMethod = "int";
                this.AddToTextBox("RampTriggerMethod set to int.");
            }
            if (this.vRampExtButton.Checked == true)
            {
                controller.RampTriggerMethod = "ext";
                this.AddToTextBox("RampTriggerMethod set to ext.");

            }
            else
            {
                //Do nothing
            }
        }
        
        private delegate void AppendToTextBoxDelegate(string text);
        public void AddToTextBox(String text)
        {
            textBox.Invoke(new AppendToTextBoxDelegate(textBox.AppendText), text);
        }

        #endregion

        #region controls

        private void voltageRampControl_Enter(object sender, EventArgs e)
        {

        }

        private void rampStartButton_Click(object sender, EventArgs e)
        {
            this.AddToTextBox("Start button pressed.");
            if (controller.Status == DeadBolt.ControllerState.free & controller.RampTriggerMethod == "int")
            {
                controller.Ramping = true;
                this.rampLED.Value = true;
                controller.ScanVoltage(50, controller.RampChannel);
            }
            else
            {
                // do nothing
            }
        }

        private void rampStopButton_Click(object sender, EventArgs e)
        {
            this.AddToTextBox("Stop button pressed.");
            controller.Ramping = false;
            this.rampLED.Value = false;
            controller.Status = DeadBolt.ControllerState.free;
        }

        private void vRampIntButton_CheckedChanged(object sender, EventArgs e)
        {
            vRampExtButton.Checked = false;
        }

        private void vRampExtButton_CheckedChanged(object sender, EventArgs e)
        {   
            vRampIntButton.Checked = false;
        }

        private void rampChannelMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void rampLED_StateChanged(object sender, NationalInstruments.UI.ActionEventArgs e)
        {

        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {

        }

        #endregion

        

       

        
    }
}
