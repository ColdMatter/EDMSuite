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
            controller.RAMPING = false;
            rampStartButton.Enabled = true;
            rampStopButton.Enabled = false;
            lockEnableCheck.Enabled = false;
            setPointUpDownBox.Enabled = false;
            setPointUpDownBox.Value = Convert.ToDecimal(0.0);
            setPointUpDownBox.Maximum = Convert.ToDecimal(9.5);
            setPointUpDownBox.Minimum = Convert.ToDecimal(-9.5);
            setPointUpDownBox.Increment = Convert.ToDecimal(0.001);
            setPointUpDownBox.DecimalPlaces = 3;
            initLaserVoltageUpDownBox.Enabled = false;
            initLaserVoltageUpDownBox.Value = Convert.ToDecimal(0.0);
            initLaserVoltageUpDownBox.Maximum = Convert.ToDecimal(9.5);
            initLaserVoltageUpDownBox.Minimum = Convert.ToDecimal(-9.5);
            initLaserVoltageUpDownBox.Increment = Convert.ToDecimal(0.001);
            initLaserVoltageUpDownBox.DecimalPlaces = 3;
            GainTrackBar.Minimum = 0;
            GainTrackBar.Maximum = 30;
            GainTrackBar.Value = 0;
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
            double[] dx = new double[controller.CavityScanParameters.Steps/2];
            double[] dy = new double[controller.CavityScanParameters.Steps/2];
            for (i = controller.CavityScanParameters.Steps / 4; i < 3 * controller.CavityScanParameters.Steps / 4; i++)
            {
                dx[i - controller.CavityScanParameters.Steps / 4] = data[0, i];
                dy[i - controller.CavityScanParameters.Steps / 4] = data[1, i];
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
            double[] dx = new double[controller.CavityScanParameters.Steps/2];
            double[] dy = new double[controller.CavityScanParameters.Steps/2];
            for (i = controller.CavityScanParameters.Steps / 4; i < 3 * controller.CavityScanParameters.Steps / 4; i++)
            {
                dx[i - controller.CavityScanParameters.Steps / 4] = data[0, i];
                dy[i - controller.CavityScanParameters.Steps / 4] = data[2, i];
            }
            p2Intensity.ClearData();
            p2Intensity.PlotXY(dx, dy);
        }

        /// <summary>
        /// threading for the laser voltage. GetLaserVoltage returns the value stored in the updown box, while SetLaserVoltage sets it.
        /// </summary>
        private delegate double getLaserVoltageDelegate();
        private double getLaserVoltage()
        {
            return laserVoltage;
        }
        public double GetLaserVoltage()
        {
            return Convert.ToDouble(Invoke(new getLaserVoltageDelegate(getLaserVoltage)));
        }

        private delegate void setLaserVoltageDelegate(double laserVoltage);
        private void setLaserVoltage(double lV)
        {
            laserVoltage = lV;
        }
        public void SetLaserVoltage(double lV)
        {
            Invoke(new setLaserVoltageDelegate(setLaserVoltage), lV);
        }
        /// <summary>
        /// reads the value in the box and returns it
        /// </summary>
        private delegate double getSetPointDelegate();
        private double getSetPoint()
        {
            return setPoint;
        }
        public double GetSetPoint()
        {
            return Convert.ToDouble(Invoke(new getSetPointDelegate(getSetPoint)));
        }
        /// <summary>
        /// set the setPoint (I think, only used during the first run of the lock)
        /// </summary>
        private delegate void setSetPointDelegate(double point);
        private void setSetPoint(double point)
        {
            setPoint = point;    
        }
        public void SetSetPoint(double point)
        {
            Invoke(new setSetPointDelegate(setSetPoint), point);
        }
        
        private double setPoint
        {
            get { return Convert.ToDouble(setPointUpDownBox.Value); }
            set { setPointUpDownBox.Value = Convert.ToDecimal(value); }
        }
        private int gain
        {
            get { return GainTrackBar.Value; }
            set { GainTrackBar.Value = value; }
        }
        private double laserVoltage
        {
            get { return Convert.ToDouble(initLaserVoltageUpDownBox.Value); }
            set { initLaserVoltageUpDownBox.Value = Convert.ToDecimal(value); }
        }
        /// <summary>
        /// Get and set the Gain on the laser lock
        /// </summary>
        private delegate void setGainDelegate(int point);
        private void setGain(int point)
        {
            gain = point;
        }
        public void SetGain(int point)
        {
            Invoke(new setGainDelegate(setGain), point);
        }
        private delegate int getGainDelegate();
        private int getGain()
        {
            return gain;
        }
        public int GetGain()
        {
            return Convert.ToInt16(Invoke(new getGainDelegate(getGain)));
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
            }
            if (lockEnableCheck.CheckState == CheckState.Unchecked)
            {
                lockEnabled = false;
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
            }
            if (fitEnableCheck.CheckState == CheckState.Unchecked)
            {

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
            controller.RAMPING = true;
            this.rampLED.Value = true;
            controller.startRamp();
            this.rampStartButton.Enabled = false;
            this.rampStopButton.Enabled = true;
        }

        private void rampStopButton_Click(object sender, EventArgs e)
        {
            lock (controller.rampStopLock)
            {
                this.AddToTextBox("Stop button pressed.");
                controller.RAMPING = false;
            }
            rampStartButton.Enabled = true;
            rampStopButton.Enabled = false;
            this.rampLED.Value = false;
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

        private void lockParams_Enter(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
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
            if (fitEnableCheck.CheckState == CheckState.Checked)
            {
                this.initLaserVoltageUpDownBox.Enabled = true;
                this.lockEnableCheck.Enabled = true;
            }
            if (fitEnableCheck.CheckState == CheckState.Unchecked)
            {
                this.initLaserVoltageUpDownBox.Enabled = false;
                this.lockEnableCheck.Enabled = false;
            }
        }
        private void lockEnableCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (lockEnableCheck.CheckState == CheckState.Checked)
            {
                this.initLaserVoltageUpDownBox.Enabled = false;
                this.setPointUpDownBox.Enabled = true;
            }
            if (lockEnableCheck.CheckState == CheckState.Unchecked)
            {
                this.initLaserVoltageUpDownBox.Enabled = true;
                this.setPointUpDownBox.Enabled = false;
                controller.FirstLock = true;
                controller.StepToNewSetPoint(controller.LaserScanParameters, GetLaserVoltage());
                WriteToVoltageToLaserBox(Convert.ToString(GetLaserVoltage()));
                
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
        private void initLaserVoltageUpDownBox_ValueChanged(object sender, EventArgs e)
        {
            
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void GainTrackBar_Scroll(object sender, EventArgs e)
        {

        }

        #endregion

        

    

 
     



  
        


     

       

     

    



       


  

     
     


        

    }
    
}
