using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RFMOTHardwareControl
{
    public partial class HardwareMonitorWindow : Form
    {
        public Controller controller;

        private double currentFrequency;

        public HardwareMonitorWindow()
        {
            InitializeComponent();

        }

        private void startMonitoringLaserFrequencies()
        {
            controller.startMonitoringLaserFrequencies();
            monitorFrequenciesCheckBox.Checked = true;
        }

        private void stopMonitoringLaserFrequencies()
        {
            controller.stopMonitoringLaserFrequencies();
            setTextBox(laser1FrequencyTextBox, "");
            monitorFrequenciesCheckBox.Checked = false;
        }

        public void updateFrequency(double newFreq)
        {
            setTextBox(laser1FrequencyTextBox, newFreq.ToString("F4"));
            if(newFreq>10 & newFreq<500){currentFrequency = newFreq;}
        }

        private void setTextBox(TextBox box, string text)
        {
            box.Invoke(new setTextDelegate(setTextHelper), new object[] { box, text });
        }
        private delegate void setTextDelegate(TextBox box, string text);

        private void setTextHelper(TextBox box, string text)
        {
            box.Text = text;
        }

        public double getCurrentFrequency()
        {
            return currentFrequency;
        }

        private void monitorFrequenciesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (monitorFrequenciesCheckBox.Checked == true)
            {
                startMonitoringLaserFrequencies();
            }
            else
            {
                stopMonitoringLaserFrequencies();
            }
        }

        private void HardwareMonitorWindow_Close(object sender, EventArgs e)
        {
            stopMonitoringLaserFrequencies();
            stopMonitoringFieldGradient();
        }

        #region Monitoring Field Gradient

        public void updateFieldGradient(double newVal)
        {
            setTextBox(fieldGradientTB, newVal.ToString("F3"));
        }

        private void stopMonitoringFieldGradient()
        {
            controller.stopMonitoringFieldGradient();
            setTextBox(fieldGradientTB, "");
            fieldGradientCB.Checked = false;
        }

        private void startMonitoringFieldGradient()
        {
            controller.startMonitoringFieldGradient();
            fieldGradientCB.Checked = true;
        }



        #endregion

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void laser1FrequencyTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void fieldGradientCB_CheckedChanged(object sender, EventArgs e)
        {
            if (fieldGradientCB.Checked)
            {
                startMonitoringFieldGradient();

            }
            else
            {
                stopMonitoringFieldGradient();
            }
        }

        private void startAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startMonitoringFieldGradient();
            startMonitoringLaserFrequencies();
            startAllToolStripMenuItem.Checked = true;
            startAllToolStripMenuItem.Enabled = false;
        }

        private void stopAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopMonitoringFieldGradient();
            stopMonitoringLaserFrequencies();
            startAllToolStripMenuItem.Checked = false;
            startAllToolStripMenuItem.Enabled = true;
        }

        private void HardwareMonitorWindow_Load(object sender, EventArgs e)
        {

        }

    }
}
