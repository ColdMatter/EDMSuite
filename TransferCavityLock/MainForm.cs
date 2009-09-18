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

        private delegate void WriteToInterval1BoxDelegate(string text);
        private delegate void ClearInterval1BoxDelegate();
        public void WriteToInterval1Box(String text)
        {
            interval1.Invoke(new ClearInterval1BoxDelegate(interval1.Clear));
            interval1.Invoke(new WriteToInterval1BoxDelegate(interval1.AppendText), text);
        }
        private delegate void WriteToInterval2BoxDelegate(string text);
        private delegate void ClearInterval2BoxDelegate();
        public void WriteToInterval2Box(String text)
        {
            interval2.Invoke(new ClearInterval2BoxDelegate(interval2.Clear));
            interval2.Invoke(new WriteToInterval2BoxDelegate(interval2.AppendText), text);
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

        private delegate void fitsPlotDelegate(double[,] data);
        public void fitsPlot(double[,] data)
        {
            int i = 0;
            double[] dx = new double[controller.RampSteps];
            double[] dy = new double[controller.RampSteps];
            for (i = 0; i < controller.RampSteps; i++)
            {
                dx[i] = data[0, i];
                dy[i] = data[1, i];
            }
            plotFitsWindow.PlotXY(dx, dy);
        }

        private delegate void fitsPlot2Delegate(double[,] data);
        public void fitsPlot2(double[,] data)
        {
            int i = 0;
            double[] dx = new double[controller.RampSteps];
            double[] dy = new double[controller.RampSteps];
            for (i = 0; i < controller.RampSteps; i++)
            {
                dx[i] = data[0, i];
                dy[i] = data[1, i];
            }
            plotFitsWindow2.PlotXY(dx, dy);
        }

        public double getIntervalGuess()
        {
            double num = Convert.ToDouble(interGuessBox.Text);
            return num;
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
                fitEnableCheck.Enabled = false;
            }
            else
            {
                controller.Ramping = true;
                this.rampLED.Value = true;
                this.AddToTextBox("Trigger is set to external.");
                rampStartButton.Enabled = false;
                rampStopButton.Enabled = true;
                triggerMenu.Enabled = false;
                fitEnableCheck.Enabled = false;
            }
        }

        private void rampStopButton_Click(object sender, EventArgs e)
        {
            lock (controller.rampStopLock)
            {
                this.AddToTextBox("Stop button pressed.");
                controller.Ramping = false;
            }
            fitEnableCheck.Enabled = true;
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

        

        private void fitResultsP1_Enter(object sender, EventArgs e)
        {

        }

        private void fitResultsP2_Enter(object sender, EventArgs e)
        {
            
        }
        private void fitEnableCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (controller.Fitting == true)
            {
                controller.Fitting = false;
            }
            if (controller.Fitting == false)
            {
                controller.Fitting = true;
            }
            else
            {
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void gp1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void interval1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void gp2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void interval2_TextChanged(object sender, EventArgs e)
        {

        }
        private void plotFitsWindow_PlotDataChanged(object sender, NationalInstruments.UI.XYPlotDataChangedEventArgs e)
        {

        }

        private void interGuessBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void plotFitsWindow2_PlotDataChanged(object sender, NationalInstruments.UI.XYPlotDataChangedEventArgs e)
        {

        }
        #endregion



       


  

     
     


        

    }
    
}
