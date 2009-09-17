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
            controller.Ramping = false;
            controller.RampTriggerMethod = "int";
            rampStartButton.Enabled = true;
            rampStopButton.Enabled = false;
        }
        #endregion

        #region passing values set by UI into program

        private delegate void AppendToTextBoxDelegate(string text);
        private delegate void ClearTextBoxDelegate();
        public void AddToTextBox(String text)
        {
            textBox.Invoke(new ClearTextBoxDelegate(textBox.Clear));
            textBox.Invoke(new AppendToTextBoxDelegate(textBox.AppendText), text);
        }
        private delegate void WriteToGlobalPhase1BoxDelegate(string text);
        private delegate void ClearGlobalPhase1BoxDelegate();
        public void WriteToGlobalPhase1Box(String text)
        {
            gp1.Invoke(new ClearGlobalPhase1BoxDelegate(gp1.Clear));
            gp1.Invoke(new WriteToGlobalPhase1BoxDelegate(gp1.AppendText), text);
        }
        private delegate void WriteToGlobalPhase2BoxDelegate(string text);
        private delegate void ClearGlobalPhase2BoxDelegate();
        public void WriteToGlobalPhase2Box(String text)
        {
            gp2.Invoke(new ClearGlobalPhase2BoxDelegate(gp2.Clear));
            gp2.Invoke(new WriteToGlobalPhase2BoxDelegate(gp2.AppendText), text);
        }

        private delegate void WriteToFreq1BoxDelegate(string text);
        private delegate void ClearFreq1BoxDelegate();
        public void WriteToFreq1Box(String text)
        {
            freq1.Invoke(new ClearFreq1BoxDelegate(freq1.Clear));
            freq1.Invoke(new WriteToFreq1BoxDelegate(freq1.AppendText), text);
        }
        private delegate void WriteToFreq2BoxDelegate(string text);
        private delegate void ClearFreq2BoxDelegate();
        public void WriteToFreq2Box(String text)
        {
            freq2.Invoke(new ClearFreq2BoxDelegate(freq2.Clear));
            freq2.Invoke(new WriteToFreq2BoxDelegate(freq2.AppendText), text);
        }

        private delegate void WriteToFinesse1BoxDelegate(string text);
        private delegate void ClearFinesse1BoxDelegate();
        public void WriteToFinesse1Box(String text)
        {
            finesse1.Invoke(new ClearFinesse1BoxDelegate(finesse1.Clear));
            finesse1.Invoke(new WriteToFinesse1BoxDelegate(finesse1.AppendText), text);
        }
        private delegate void WriteToFinesse2BoxDelegate(string text);
        private delegate void ClearFinesse2BoxDelegate();
        public void WriteToFinesse2Box(String text)
        {
            finesse2.Invoke(new ClearFinesse2BoxDelegate(finesse2.Clear));
            finesse2.Invoke(new WriteToFinesse2BoxDelegate(finesse2.AppendText), text);
        }

        private delegate void WriteToAmplitude1BoxDelegate(string text);
        private delegate void ClearAmplitude1BoxDelegate();
        public void WriteToAmplitude1Box(String text)
        {
            amplitude1.Invoke(new ClearAmplitude1BoxDelegate(amplitude1.Clear));
            amplitude1.Invoke(new WriteToAmplitude1BoxDelegate(amplitude1.AppendText), text);
        }
        private delegate void WriteToAmplitude2BoxDelegate(string text);
        private delegate void ClearAmplitude2BoxDelegate();
        public void WriteToAmplitude2Box(String text)
        {
            amplitude2.Invoke(new ClearAmplitude2BoxDelegate(amplitude2.Clear));
            amplitude2.Invoke(new WriteToAmplitude2BoxDelegate(amplitude2.AppendText), text);
        }

        private delegate void WriteToBackground1BoxDelegate(string text);
        private delegate void ClearBackground1BoxDelegate();
        public void WriteToBackground1Box(String text)
        {
            background1.Invoke(new ClearBackground1BoxDelegate(background1.Clear));
            background1.Invoke(new WriteToBackground1BoxDelegate(background1.AppendText), text);
        }
        private delegate void WriteToBackground2BoxDelegate(string text);
        private delegate void ClearBackground2BoxDelegate();
        public void WriteToBackground2Box(String text)
        {
            background2.Invoke(new ClearBackground2BoxDelegate(background2.Clear));
            background2.Invoke(new WriteToBackground2BoxDelegate(background2.AppendText), text);
        }

        private delegate void PlotOnP1Delegate(double[,] data);
        public void PlotOnP1(double[,] data)
        {
            int i = 0;
            double[] dx = new double[controller.RampSteps];
            double[] dy = new double[controller.RampSteps];
            for (i = 0; i < controller.RampSteps; i++)
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
            double[] dx = new double[controller.RampSteps];
            double[] dy = new double[controller.RampSteps];
            for (i = 0; i < controller.RampSteps; i++)
            {
                dx[i] = data[0, i];
                dy[i] = data[2, i];
            }
            p2Intensity.PlotXY(dx, dy);
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
            }
            else
            {
                controller.Ramping = true;
                this.rampLED.Value = true;
                this.AddToTextBox("Trigger is set to external.");
                rampStartButton.Enabled = false;
                rampStopButton.Enabled = true;
                triggerMenu.Enabled = false;
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
            this.rampLED.Value = false;
        }

        private void triggerMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            string menuSelection = triggerMenu.Text;
            controller.RampTriggerMethod = menuSelection;
            this.AddToTextBox("Trigger method selected: " + menuSelection + ". ");
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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void gp1_TextChanged(object sender, EventArgs e)
        {

        }

        private void freq1_TextChanged(object sender, EventArgs e)
        {

        }

        private void finesse1_TextChanged(object sender, EventArgs e)
        {

        }

        private void amplitude1_TextChanged(object sender, EventArgs e)
        {

        }

        private void background1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void gp2_TextChanged(object sender, EventArgs e)
        {

        }

        private void freq2_TextChanged(object sender, EventArgs e)
        {

        }

        private void finesse2_TextChanged(object sender, EventArgs e)
        {

        }

        private void amplitude2_TextChanged(object sender, EventArgs e)
        {

        }

        private void background2_TextChanged(object sender, EventArgs e)
        {

        }

        private void fitResultsP1_Enter(object sender, EventArgs e)
        {

        }

        private void fitResultsP2_Enter(object sender, EventArgs e)
        {

        }
        #endregion

        

    }
    
}
