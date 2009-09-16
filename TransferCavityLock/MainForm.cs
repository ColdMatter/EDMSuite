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
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            controller.RampChannel = "laser";
            controller.Ramping = false;
            controller.RampTriggerMethod = "int";
            rampStartButton.Enabled = true;
            rampStopButton.Enabled = false;
        }
        #endregion

        #region passing values set by UI into program

        private delegate void AppendToTextBoxDelegate(string text);
        public void AddToTextBox(String text)
        {
            textBox.Invoke(new AppendToTextBoxDelegate(textBox.AppendText), text);
        }

        private delegate void PlotOnP1Delegate(double[,] data);
        public void PlotOnP1(double[,] data)
        {
            int i = 0;
            double[] dx = new double[100];
            double[] dy = new double[100];
            for(i=0; i<100 ; i++)
            {
                dx[i]=data[0,i];
                dy[i]=data[1,i];
            }
            p1Intensity.PlotXY(dx,dy);
        }

        private delegate void PlotOnP2Delegate(double[,] data);
        public void PlotOnP2(double[,] data)
        {
            int i = 0;
            double[] dx = new double[100];
            double[] dy = new double[100];
            for (i = 0; i < 100; i++)
            {
                dx[i] = data[0, i];
                dy[i] = data[2, i];
            }
            p1Intensity.PlotXY(dx, dy);
        }

        #endregion

        #region controls

        private void voltageRampControl_Enter(object sender, EventArgs e)
        {

        }
        
        private void rampStartButton_Click(object sender, EventArgs e)
        {
            this.AddToTextBox("Start button pressed.");
            if (controller.RampTriggerMethod == "int")
            {
                controller.Ramping = true;
                this.rampLED.Value = true;
                controller.startRamp();
                rampStartButton.Enabled = false;
                rampStopButton.Enabled = true;
                triggerMenu.Enabled = false;
                rampChannelMenu.Enabled = false;
            }
            else
            {
                controller.Ramping = true;
                this.rampLED.Value = true;
                this.AddToTextBox("Trigger is set to external.");
                rampStartButton.Enabled = false;
                rampStopButton.Enabled = true;
                triggerMenu.Enabled = false;
                rampChannelMenu.Enabled = false;
            }
        }

        private void rampStopButton_Click(object sender, EventArgs e)
        {
            lock (controller.rampStopLock)
            {
                this.AddToTextBox("Stop button pressed.");
                controller.Ramping = false;
            }
            rampStartButton.Enabled = true;
            rampStopButton.Enabled = false;
            triggerMenu.Enabled = true;
            rampChannelMenu.Enabled = true;
            this.rampLED.Value = false;
        }

        private void triggerMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            string menuSelection = triggerMenu.Text;
            controller.RampTriggerMethod = menuSelection;
            this.AddToTextBox("Trigger method selected: " + menuSelection + ". ");
        }

        private void rampChannelMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            string menuSelection = rampChannelMenu.Text;
            controller.RampChannel = menuSelection;
            this.AddToTextBox("Ramp channel selected: " + menuSelection + ". ");
        }

        private void rampLED_StateChanged(object sender, NationalInstruments.UI.ActionEventArgs e)
        {

        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void p1Intensity_PlotDataChanged(object sender, NationalInstruments.UI.XYPlotDataChangedEventArgs e)
        {

        }

        private void p2Intensity_PlotDataChanged(object sender, NationalInstruments.UI.XYPlotDataChangedEventArgs e)
        {

        }

        #endregion

        

    }
    
}
