using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;

namespace SympatheticHardwareControl
{
    public partial class ControlWindow : Form
    {
        public Controller controller;
        
        public ControlWindow()
        {
            InitializeComponent();
        }
        // This is the method that runs once the window has been created.
        private void ControlWindow_Load(object sender, EventArgs e)
        {
            controller.WindowLoaded();
        }

        // A wrapper for sending data to text boxes
        public void outputAIdata(TextBox box, string text)
        {
        	box.Invoke(new SetTextDelegate(SetTextHelper), new object[] {box,text});
        }
        private delegate void SetTextDelegate(TextBox box, String text);
        public void SetTextHelper(TextBox box, String text)
        {
            box.Text = text;
        }

        // A wrapper for setting the state of an led
        public void setLedState(Led led, bool state)
        {
            led.Invoke(new SetLedDelegate(SetLedHelper), new object[] { led, state });
        }
        private delegate void SetLedDelegate(Led led, bool state);
        public void SetLedHelper(Led led, bool state)
        {
            led.Value = state;
        }

        #region HV switching events
        private void hpButtonOff_CheckedChanged(object sender, EventArgs e)
        {
            if (hpButtonOff.Checked)
            {
                controller.EnableHVBurst("hplusBurstEnable", false);
                controller.SetHVSwitchState("hplusdc", false);
            }
        }

        private void hpButtonOn_CheckedChanged(object sender, EventArgs e)
        {
            if (hpButtonOn.Checked)
            {
                controller.EnableHVBurst("hplusBurstEnable", false);
                controller.SetHVSwitchState("hplusdc", true);
            }
        }

        private void hpButtonBurst_CheckedChanged(object sender, EventArgs e)
        {
            if (hpButtonBurst.Checked)
            {
                controller.EnableHVBurst("hplusBurstEnable", true);
                controller.SetHVSwitchState("hplusdc", false);
            }
        }

        private void hnButtonOff_CheckedChanged(object sender, EventArgs e)
        {
            if (hnButtonOff.Checked)
            {
                controller.EnableHVBurst("hminusBurstEnable", false);
                controller.SetHVSwitchState("hminusdc", false);
            }
        }

        private void hnButtonOn_CheckedChanged(object sender, EventArgs e)
        {
            if (hnButtonOn.Checked)
            {
                controller.EnableHVBurst("hminusBurstEnable", false);
                controller.SetHVSwitchState("hminusdc", true);
            }
        }

        private void hnButtonBurst_CheckedChanged(object sender, EventArgs e)
        {
            if (hnButtonBurst.Checked)
            {
                controller.EnableHVBurst("hminusBurstEnable", true);
                controller.SetHVSwitchState("hminusdc", false);
            }
        }

        private void vpButtonOff_CheckedChanged(object sender, EventArgs e)
        {
            if (vpButtonOff.Checked)
            {
                controller.EnableHVBurst("vplusBurstEnable", false);
                controller.SetHVSwitchState("vplusdc", false);
            }
        }

        private void vpButtonOn_CheckedChanged(object sender, EventArgs e)
        {
            if (vpButtonOn.Checked)
            {
                controller.EnableHVBurst("vplusBurstEnable", false);
                controller.SetHVSwitchState("vplusdc", true);
            }
        }

        private void vpButtonBurst_CheckedChanged(object sender, EventArgs e)
        {
            if (vpButtonBurst.Checked)
            {
                controller.EnableHVBurst("vplusBurstEnable", true);
                controller.SetHVSwitchState("vplusdc", false);
            }
        }

        private void vnButtonOff_CheckedChanged(object sender, EventArgs e)
        {
            if (vnButtonOff.Checked)
            {
                controller.EnableHVBurst("vminusBurstEnable", false);
                controller.SetHVSwitchState("vminusdc", false);
            }
        }

        private void vnButtonOn_CheckedChanged(object sender, EventArgs e)
        {
            if (vnButtonOn.Checked)
            {
                controller.EnableHVBurst("vminusBurstEnable", false);
                controller.SetHVSwitchState("vminusdc", true);
            }
        }

        private void vnButtonBurst_CheckedChanged(object sender, EventArgs e)
        {
            if (vnButtonBurst.Checked)
            {
                controller.EnableHVBurst("vminusBurstEnable", true);
                controller.SetHVSwitchState("vminusdc", false);
            }
        }

        private void allOffButton_Click(object sender, EventArgs e)
        {
            hpButtonOff.Checked = true;
            hnButtonOff.Checked = true;
            vpButtonOff.Checked = true;
            vnButtonOff.Checked = true;
        }

        private void allOnButton_Click(object sender, EventArgs e)
        {
            hpButtonOn.Checked = true;
            hnButtonOn.Checked = true;
            vpButtonOn.Checked = true;
            vnButtonOn.Checked = true;
        }

        private void allBurstButton_Click(object sender, EventArgs e)
        {
            hpButtonBurst.Checked = true;
            hnButtonBurst.Checked = true;
            vpButtonBurst.Checked = true;
            vnButtonBurst.Checked = true;
        } 
        #endregion
              
    }
}
  