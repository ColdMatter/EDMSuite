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

        /// <summary>
        /// Get everything warmed up
        /// </summary>
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
            lockEnableCheck.Enabled = false;
            setPointUpDownBox.Enabled = false;
            setPointUpDownBox.Value = Convert.ToDecimal(0.0);
            setPointUpDownBox.Maximum = Convert.ToDecimal(9.5);
            setPointUpDownBox.Minimum = Convert.ToDecimal(-9.5);
            setPointUpDownBox.Increment = Convert.ToDecimal(0.01);
            setPointUpDownBox.DecimalPlaces = 2;
            initLaserVoltageUpDownBox.Enabled = false;
            initLaserVoltageUpDownBox.Value = Convert.ToDecimal(0.0);
            initLaserVoltageUpDownBox.Maximum = Convert.ToDecimal(9.5);
            initLaserVoltageUpDownBox.Minimum = Convert.ToDecimal(-9.5);
            initLaserVoltageUpDownBox.Increment = Convert.ToDecimal(0.1);
            initLaserVoltageUpDownBox.DecimalPlaces = 1;
        }
        #endregion

        #region passing values set by UI into program
        /// <summary>
        /// Various things you need to communicate from the front panel to the program.
        /// </summary>

        /// <summary>
        /// Write to a text box (for messages)
        /// </summary>
        private delegate void AppendToTextBoxDelegate(string text);
        private delegate void ClearTextBoxDelegate();
        public void AddToTextBox(String text)
        {
            textBox.Invoke(new ClearTextBoxDelegate(textBox.Clear));
            textBox.Invoke(new AppendToTextBoxDelegate(textBox.AppendText), text);
        }

        /// <summary>
        /// Displays the voltage applied to the laser
        /// </summary>
        private delegate void WriteToVoltageToLaserBoxDelegate(string text);
        private delegate void ClearVoltageToLaserBoxDelegate();
        public void WriteToVoltageToLaserBox(String text)
        {
            voltageToLaserBox.Invoke(new ClearVoltageToLaserBoxDelegate(voltageToLaserBox.Clear));
            voltageToLaserBox.Invoke(new WriteToVoltageToLaserBoxDelegate(voltageToLaserBox.AppendText), text);
        }

        /// <summary>
        /// Plots the cavity peaks from the laser 
        /// </summary>
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
            p1Intensity.ClearData();
            p1Intensity.PlotXY(dx,dy);
        }

        /// <summary>
        /// Plots the cavity peaks from the He Ne
        /// </summary>
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
            p2Intensity.ClearData();
            p2Intensity.PlotXY(dx, dy);
        }

        /// <summary>
        /// Plots the fit for the He Ne peak
        /// </summary>
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
            plotFitsWindow.ClearData();
            plotFitsWindow.PlotXY(dx, dy);
        }

        /// <summary>
        /// Plots the fit for the peak from the laser 
        /// </summary>
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
            plotFitsWindow2.ClearData();
            plotFitsWindow2.PlotXY(dx, dy);
        }

        /// <summary>
        /// reads the value in the box and returns it
        /// </summary>
        public double getLaserVoltage()
        {
            decimal dec = initLaserVoltageUpDownBox.Value;
            return Convert.ToDouble(dec);
        }

        /// <summary>
        /// reads the value in the box and returns it
        /// </summary>
        public double getSetPoint()
        {
            decimal dec = setPointUpDownBox.Value;
            return Convert.ToDouble(dec);
        }

        /// <summary>
        /// set the setPoint (I think, only used during the first run of the lock)
        /// </summary>
        public void setSetPoint(double point)
        {
            setPointUpDownBox.Value = Convert.ToDecimal(point);
        }

        /// <summary>
        /// checks to see if the lock tick is engaged.
        /// </summary>
        public bool checkLockEnableCheck()
        {
            bool lockEnabled = false;
            if (lockEnableCheck.CheckState == CheckState.Checked)
            {
                lockEnabled = true;
                initLaserVoltageUpDownBox.Enabled = false;
                setPointUpDownBox.Enabled = true;
            }
            if (lockEnableCheck.CheckState == CheckState.Unchecked)
            {
                lockEnabled = false;
                initLaserVoltageUpDownBox.Enabled = true;
                setPointUpDownBox.Enabled = false;
            }
            else { }
            return lockEnabled;
        }

        /// <summary>
        /// checks to see if the fit tick is engaged.
        /// </summary>
        public bool checkFitEnableCheck()
        {
            bool fitEnabled = false;
            if (fitEnableCheck.CheckState == CheckState.Checked)
            {
                fitEnabled = true;
                initLaserVoltageUpDownBox.Enabled = true;
                lockEnableCheck.Enabled = true;
            }
            if (fitEnableCheck.CheckState == CheckState.Unchecked)
            {
                fitEnabled = false;
            }
            else { }
            return fitEnabled;
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


        private void plotFitsWindow_PlotDataChanged(object sender, NationalInstruments.UI.XYPlotDataChangedEventArgs e)
        {

        }
        private void plotFitsWindow2_PlotDataChanged(object sender, NationalInstruments.UI.XYPlotDataChangedEventArgs e)
        {

        }


        private void lockParams_Enter(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void voltageToLaserBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void setPointUpDownBox_ValueChanged(object sender, EventArgs e)
        {

        }

        private void fitEnableCheck_CheckedChanged(object sender, EventArgs e)
        {
            
        }
        private void lockEnableCheck_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
        private void initLaserVoltageUpDownBox_ValueChanged(object sender, EventArgs e)
        {

        }

        #endregion

 
     



  
        


     

       

     

    



       


  

     
     


        

    }
    
}
