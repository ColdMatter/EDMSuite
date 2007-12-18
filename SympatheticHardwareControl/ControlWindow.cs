using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SympatheticHardwareControl
{
    public partial class ControlWindow : Form
    {
        public Controller controller;
        
        public ControlWindow()
        {
            InitializeComponent();
        }

        private void pressureLabel1_Click(object sender, EventArgs e)
        {

        }

        private void ControlWindow_Load(object sender, EventArgs e)
        {
            controller.WindowLoaded();
        }

        public void outputAIdata(TextBox box, string text)
        {
        	box.Invoke(new SetTextDelegate(SetTextHelper), new object[] {box,text});
        }
        private delegate void SetTextDelegate(TextBox box, String text);
        public void SetTextHelper(TextBox box, String text)
        {
            box.Text = text;
        }

        private void hpButtonOff_CheckedChanged(object sender, EventArgs e)
        {
            if (hpButtonOff.Checked)
            {
                controller.EnableHVBurst("hplusBurstEnable", false);
                controller.SetHVSwitchState("hplusdc", false);
            }

        }

        private void hpButtonOff_Clicked(object sender, EventArgs e)
        {
            
        }

        private void hpButtonOn_Clicked(object sender, EventArgs e)
        {
        
        }

        private void hpButtonBurst_Clicked(object sender, EventArgs e)
        {
            controller.EnableHVBurst("hplusBurstEnable", true);
            controller.SetHVSwitchState("hplusdc", false);
        }

        private void hnButtonOff_Clicked(object sender, EventArgs e)
        {
            controller.EnableHVBurst("hminusBurstEnable", false);
            controller.SetHVSwitchState("hminusdc", false);
        }

        private void hnButtonOn_Clicked(object sender, EventArgs e)
        {
            controller.EnableHVBurst("hminusBurstEnable", false);
            controller.SetHVSwitchState("hminusdc", true);
        }

        private void hnButtonBurst_Clicked(object sender, EventArgs e)
        {
            controller.EnableHVBurst("hminusBurstEnable", true);
            controller.SetHVSwitchState("hminusdc", false);
        }

        private void vpButtonOff_Clicked(object sender, EventArgs e)
        {
            controller.EnableHVBurst("vplusBurstEnable", false);
            controller.SetHVSwitchState("vplusdc", false);
        }

        private void vpButtonOn_Clicked(object sender, EventArgs e)
        {
            controller.EnableHVBurst("vplusBurstEnable", false);
            controller.SetHVSwitchState("vplusdc", true);
        }

        private void vpButtonBurst_Clicked(object sender, EventArgs e)
        {
            controller.EnableHVBurst("vplusBurstEnable", true);
            controller.SetHVSwitchState("vplusdc", false);
        }

        private void vnButtonOff_Clicked(object sender, EventArgs e)
        {
            controller.EnableHVBurst("vminusBurstEnable", false);
            controller.SetHVSwitchState("vminusdc", false);
        }

        private void vnButtonOn_Clicked(object sender, EventArgs e)
        {
            controller.EnableHVBurst("vminusBurstEnable", false);
            controller.SetHVSwitchState("vminusdc", true);
        }

        private void vnButtonBurst_Clicked(object sender, EventArgs e)
        {
            controller.EnableHVBurst("vminusBurstEnable", true);
            controller.SetHVSwitchState("vminusdc", false);
        }

        private void allOffButton_Click(object sender, EventArgs e)
        {
            hpButtonOff.Checked = true;
        }

        private void hpButtonOn_CheckedChanged(object sender, EventArgs e)
        {
            if (hpButtonOn.Checked)
            {
                controller.EnableHVBurst("hplusBurstEnable", false);
                controller.SetHVSwitchState("hplusdc", true);
            }
        }
              
    }
}
  