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
        public Controller controller;

        public object plotLock = new object();
        private double lv;
        public int NewStepNumber = 100;
        public double ScanWidth = 0.3;
        public double ScanOffset = 3;

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
            this.SetLaserVoltage(0.0);
            numberOfPointsTextBox.Text = Convert.ToString(NewStepNumber);
            numberOfPointsTextBox.Enabled = true;
            cavityScanWidthTextBox.Text = Convert.ToString(ScanWidth);
            cavityScanWidthTextBox.Enabled = true;
            cavityScanOffsetTextBox.Text = Convert.ToString(ScanOffset);
            cavityScanOffsetTextBox.Enabled = true;
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
        private delegate void AppendToMeasuredPeakDistanceTextBoxDelegate(string text);
        private delegate void ClearMeasuredPeakDistanceTextBoxDelegate();
        public void AddToMeasuredPeakDistanceTextBox(String text)
        {
            measuredPeakDistanceTextBox.Invoke(new ClearMeasuredPeakDistanceTextBoxDelegate(measuredPeakDistanceTextBox.Clear));
            measuredPeakDistanceTextBox.Invoke(new AppendToMeasuredPeakDistanceTextBoxDelegate(measuredPeakDistanceTextBox.AppendText), text);
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
        private delegate void clearP1Delegate();
        public void clearP1()
        {
            p1Intensity.Invoke(new clearP1Delegate(p1Intensity.ClearData));
        }
        private delegate void PlotXYOnP1Delegate(double[] x, double[] y);
        public void plotXYOnP1(double[] x, double[] y)
        {
            p1Intensity.Invoke(new PlotXYOnP1Delegate(p1Intensity.PlotXY), x,y);
        }
        public void PlotOnP1(double[,] data)
        {
            int i = 0;
            double[] dx = new double[controller.CavityScanParameters.Steps];
            double[] dy = new double[controller.CavityScanParameters.Steps];
            for (i = controller.CavityScanParameters.Steps ; i <  controller.CavityScanParameters.Steps ; i++)
            {
                dx[i] = data[0, i];
                dy[i] = data[1, i];
            }
            clearP1();
            plotXYOnP1(dx, dy);
        }

        /// <summary>
        /// Plots the cavity peaks from the He Ne
        /// </summary>
        private delegate void clearP2Delegate();
        public void clearP2()
        {
            p2Intensity.Invoke(new clearP2Delegate(p2Intensity.ClearData));
        }
        private delegate void PlotXYOnP2Delegate(double[] x, double[] y);
        public void plotXYOnP2(double[] x, double[] y)
        {
            p2Intensity.Invoke(new PlotXYOnP2Delegate(p2Intensity.PlotXY), x,y);
        }
        public void PlotOnP2(double[,] data)
        {
            int i = 0;
            double[] dx = new double[controller.CavityScanParameters.Steps];
            double[] dy = new double[controller.CavityScanParameters.Steps];
            for (i = controller.CavityScanParameters.Steps; i < controller.CavityScanParameters.Steps; i++)
            {
                dx[i] = data[0, i];
                dy[i] = data[1, i];
            }
            clearP2();
            plotXYOnP2(dx, dy);
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
            get { return /*Convert.ToDouble(initLaserVoltageUpDownBox.Value)*/ lv; }
            set { /*initLaserVoltageUpDownBox.Value = Convert.ToDecimal(value)*/ lv = value; }
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
            return Convert.ToInt32(Invoke(new getGainDelegate(getGain)));
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

        private void rampStartButton_Click(object sender, EventArgs e)
        {
            this.AddToTextBox("Start button pressed.");
            controller.Ramping = true;
            this.rampLED.Value = true;
            controller.startRamp();
            this.rampStartButton.Enabled = false;
            this.rampStopButton.Enabled = true;
            this.numberOfPointsTextBox.Enabled = false;
            this.cavityScanWidthTextBox.Enabled = false;
            this.cavityScanOffsetTextBox.Enabled = false;
            this.initLaserVoltageUpDownBox.Value = Convert.ToDecimal(0.0);
        }

        private void rampStopButton_Click(object sender, EventArgs e)
        {
            lock (controller.rampStopLock)
            {
                this.AddToTextBox("Stop button pressed.");
                controller.Ramping = false;
            }
            this.numberOfPointsTextBox.Enabled = true;
            rampStartButton.Enabled = true;
            rampStopButton.Enabled = false;
            this.rampLED.Value = false;
            this.cavityScanWidthTextBox.Enabled = true;
            this.cavityScanOffsetTextBox.Enabled = true;
            this.initLaserVoltageUpDownBox.Value = Convert.ToDecimal(0.0);
        }

        private void setPointUpDownBox_ValueChanged(object sender, EventArgs e)
        {
            this.setPoint = Convert.ToDouble(setPointUpDownBox.Value);
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
                this.setPointUpDownBox.Value = Convert.ToDecimal(0.0);
                this.measuredPeakDistanceTextBox.Text = "0.0";
                this.initLaserVoltageUpDownBox.Enabled = false;
                this.setPointUpDownBox.Enabled = true;
                this.GainTrackBar.Enabled = false;
            }
            if (lockEnableCheck.CheckState == CheckState.Unchecked)
            {
                this.setPointUpDownBox.Value = Convert.ToDecimal(0.0);
                this.measuredPeakDistanceTextBox.Text = "0.0";
                this.initLaserVoltageUpDownBox.Enabled = true;
                this.setPointUpDownBox.Enabled = false;
                controller.FirstLock = true;
               // controller.StepToNewSetPoint(controller.LaserScanParameters, 0);
                WriteToVoltageToLaserBox(Convert.ToString(GetLaserVoltage()));
                this.GainTrackBar.Enabled = true;
                
            }
        }

        private void initLaserVoltageUpDownBox_ValueChanged(object sender, EventArgs e)
        {
            lv = Convert.ToDouble(initLaserVoltageUpDownBox.Value);
        }

        private void numberOfPointsTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                NewStepNumber = Convert.ToInt32(numberOfPointsTextBox.Text);
            }
            catch (FormatException f)
            { Console.Error.WriteLine(f.ToString()); }
        }

        private void cavityScanWidthTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ScanWidth = Convert.ToDouble(cavityScanWidthTextBox.Text);
            }
            catch (FormatException f)
            { Console.Error.WriteLine(f.ToString()); }

        }

        private void cavityScanOffsetTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ScanOffset = Convert.ToDouble(cavityScanOffsetTextBox.Text);
            }
            catch (FormatException f)
            { Console.Error.WriteLine(f.ToString()); }
        }

        #endregion

     

        


        

    }
    
}
