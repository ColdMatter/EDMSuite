using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using Data;
using Data.Scans;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;
using DAQ.Remoting;
using System.Windows.Forms;
using NationalInstruments.Analysis.Math;

namespace TransferCavityLock
{
    /// <summary>
    /// A class for locking the laser using a transfer cavity.
    /// </summary>
    public class DeadBolt : MarshalByRefObject
    {

        #region Definitions

        private const double UPPER_LC_VOLTAGE_LIMIT = 10.0; //volts LC: Laser control
        private const double LOWER_LC_VOLTAGE_LIMIT = -10.0; //volts LC: Laser control
        private const double UPPER_CC_VOLTAGE_LIMIT = 0.0; //volts CC: Cavity control
        private const double LOWER_CC_VOLTAGE_LIMIT = 10.0; //volts CC: Cavity control
        private const double FINESSE_COEFFICIENT_LASER = 200;
        private const double FINESSE_COEFFICIENT_REF_LASER = 100;
        private const int DELAY_BETWEEN_STEPS = 0; //milliseconds?
        private const int STEPS = 100;

        private double upper_cc_voltage_limit = 3.05; //volts CC: Cavity control
        private double lower_cc_voltage_limit = 2.95; //volts CC: Cavity control
        private double laser_voltage = 0.0;
        private double gain = 1;
        private double laser_Offset_Voltage = 0;

        private MainForm ui;

        private Task outputLaserTask; //Some stuff to let you write to laser
        private AnalogOutputChannel laserChannel;
        private AnalogSingleChannelWriter laserWriter;

        private Task outputCavityTask; //Some stuff to let you write to piezo driver
        private AnalogOutputChannel cavityChannel;
        private AnalogSingleChannelWriter cavityWriter;

        private Task inputLaserTask;
        private AnalogInputChannel p1Channel; //p1 is the signal from the laser we are trying to lock
        private AnalogSingleChannelReader p1Reader;
        //private AnalogMultiChannelReader cavityReader;

        private Task inputRefLaserTask;
        private AnalogInputChannel p2Channel; //p2 is the signal from the reference He-Ne lock
        private AnalogSingleChannelReader p2Reader;

        private string rampTriggerMethod = "int";
        private volatile bool ramping = false;
        private bool first_Lock = true;
        public object rampStopLock = new object();

        
        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely. Inherited from Mike. No idea.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        #endregion

        #region Setup

        public void Start()
        {
            ui = new MainForm();
            ui.controller = this;


            /// <summary>
            /// This is the part where I define all the input and output channels.
            /// </summary>
            if (!Environs.Debug)
            {
                outputLaserTask = new Task("FeedbackToLaser");
                laserChannel =
                        (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["laser"];
                laserChannel.AddToTask(outputLaserTask, -10, 10);
                outputLaserTask.Control(TaskAction.Verify);
                laserWriter = new AnalogSingleChannelWriter(outputLaserTask.Stream);

                outputCavityTask = new Task("CavityPiezoVoltage");
                cavityChannel =
                        (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["cavity"];
                cavityChannel.AddToTask(outputCavityTask, 0, 10);
                outputCavityTask.Control(TaskAction.Verify);
                cavityWriter = new AnalogSingleChannelWriter(outputCavityTask.Stream);

                inputLaserTask = new Task("CavityPeaksFromLaser");
                p1Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["p1"];
                p1Channel.AddToTask(inputLaserTask, 0, 10);
                //inputLaserTask.Timing.
                p1Reader = new AnalogSingleChannelReader(inputLaserTask.Stream);

                inputRefLaserTask = new Task("CavityPeaksFromRefLaser");
                p2Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["p2"];
                p2Channel.AddToTask(inputRefLaserTask, 0, 10);
                p2Reader = new AnalogSingleChannelReader(inputRefLaserTask.Stream);
            }

            Application.Run(ui);
        }

        #endregion

       #region Public methods

        /// <summary>
        /// Let's get this party started. This starts ramping the cavity. If "fit" and "lock" are enabled,
        /// it does them too.
        /// </summary>

        public void startRamp()
        {
            cavityWriter.WriteSingleSample(true, 3);
            Thread.Sleep(2000);
            Thread rampThread = new Thread(new ThreadStart(rampLoop));
            rampThread.Start();
        }
        

        

        /// <summary>
        /// A few functions for setting and getting the values rampTriggerMethod.  If we ever want to trigger the
        /// ramp from an external trigger, write into this part of the code.
        /// Set to "int" (internal) by default.
        /// </summary>
        public string RampTriggerMethod
        {
            set
            {
                rampTriggerMethod = value;
            }
            get { return rampTriggerMethod; }
        }

        /// <summary>
        /// A flag for when the cavity is ramping
        /// </summary>

        public bool Ramping
        {
            set
            {
                ramping = value;
            }
            get { return ramping; }
        }

        /// <summary>
        /// A flag for whether this is the first run with the lock engaged. (To read back the set point)
        /// </summary>
        public bool FirstLock
        {
            set
            {
                first_Lock = value;
            }
            get { return first_Lock; }
        }
       
        /// <summary>
        /// Some way of accessing the number of steps if you're outside DeadBolt
        /// </summary>

        public int RampSteps
        {
            get { return STEPS; }
        }

        /// <summary>
        /// A way of accessing the lower voltage limit of the scan. This is used to adjust the scan range
        /// to compensate for any temperature drifts in the cavity
        /// </summary>
        public double LowRampLimit
        {
            set { lower_cc_voltage_limit = value; }
            get { return lower_cc_voltage_limit; }
        }

        /// <summary>
        /// A way of accessing the upper voltage limit of the scan. This is used to adjust the scan range
        /// to compensate for any temperature drifts in the cavity
        /// </summary>
        public double UpperRampLimit
        {
            set { upper_cc_voltage_limit = value;}
            get { return upper_cc_voltage_limit; }
        }

        /// <summary>
        /// The voltage fed to the laser.
        /// </summary>
        public double LaserVoltage
        {
            set { laser_voltage = value; }
            get { return laser_voltage; }
        }

        /// <summary>
        /// The set point of the laser lock. This corresponds to the difference in voltage applied to the cavity
        /// in order to get from the He-Ne peak to the laser peak.
        /// </summary>
        public double LaserSetPoint
        {
            set { laser_Offset_Voltage = value; }
            get { return laser_Offset_Voltage; }
        }


        #endregion

        #region Private methods

        /// <summary>
        /// A function to scan across the voltage range set by the limits high and low. Reads from the two photodiodes and spits out an array.
        /// </summary>

        private double[,] scanVoltageRange(double low, double high, AnalogSingleChannelWriter writeChannel, 
            Task taskChannel, int delayAtEachStep, int steps, bool takeData)
        {
            double stepsize = (high - low) / steps;
            double voltage = low;
            double[,] data;
            data = new double[3,steps];

            writeChannel.WriteSingleSample(true, low);  //Set initial voltage to low

            for (int i = 0; i < steps; i++)
            {
                if (takeData == true)
                {
                    data[0, i] = voltage;
                    data[1, i] = p1Reader.ReadSingleSample();
                    data[2, i] = p2Reader.ReadSingleSample();

                }
                voltage += stepsize;
                writeChannel.WriteSingleSample(true, voltage);  //Write new value to channel
                Thread.Sleep(delayAtEachStep);
            }
            return(data);
        }

        /// <summary>
        /// The main loop of the program. Scans the cavity, looks at photodiodes, corrects the range for the next
        /// scan and locks the laser.
        /// </summary>
        private void rampLoop()
        {
            
            double[,] data;
            data = new double[3, STEPS];
            double[] refCavParams = new double[3];
            bool fitBool;
            bool lockBool;
            double rampVoltageStart = 0; //For use when not locked. In order to change the voltage smoothly, need to keep track of starting point of voltage. This is for that purpose.

            for (; ; )
            {
                data = scanVoltageRange(LowRampLimit, UpperRampLimit, cavityWriter,
                outputCavityTask, DELAY_BETWEEN_STEPS, STEPS, true);
                ui.PlotOnP1(data); //Plot laser peaks
                ui.PlotOnP2(data); //Plot He-Ne peaks
                fitBool = ui.checkFitEnableCheck(); //Check to see if fitting is enabled
                lockBool = ui.checkLockEnableCheck(); //Check to see if locking is enabled
                if (lockBool == false) //if not locking
                {
                    rampVoltageStart = LaserVoltage;
                    LaserVoltage = ui.getLaserVoltage(); //set the laser voltage to the voltage indicated in the "updown" box
                    if (rampVoltageStart != LaserVoltage)
                    {
                        ui.AddToTextBox("Ramping laser!");
                        scanVoltageRange(rampVoltageStart,LaserVoltage, laserWriter, outputLaserTask, 50, 50, false);
                        ui.AddToTextBox("Ramping finished!");
                    }
                    //laserWriter.WriteSingleSample(true, LaserVoltage); //write that voltage to the laser
                }
                if (fitBool == true) //if fitting
                {
                    refCavParams = fitDisplayAndModifyScanLimits(data); //Fit to cavity peaks and stabilize laser
                    LaserSetPoint = ui.GetSetPoint(); //reads in the setPoint from "updown" box. (only useful when locking)
                    fitLaserAndLockToReference(data, refCavParams, laserWriter); //lock the laser!
                }
                lock (rampStopLock) //This is to break out of the ramping to stop the program.
                {
                    if (Ramping == false)
                    {
                        cavityWriter.WriteSingleSample(true, 0);
                        return;
                    }
                }
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// A program which fits to the reference cavity peaks and adjusts the scan range to keep a peak
        /// in the middle of the scan. Need to feed it the data from the photodiodes.
        /// Data should be d[0,i]=voltage, d[1,i]= p1voltage, d[2, i] = p2voltage
        /// </summary>
        private double[] fitDisplayAndModifyScanLimits(double[,] data) 
        {
            int i = 0;      //Some rearranging of data
            double[] dx = new double[STEPS];
            double[] dy2 = new double[STEPS];
            for (i = 0; i < STEPS; i++)
            {
                dx[i] = data[0, i];
                dy2[i] = data[2, i];
            }
            double mse = 0; //Mean standard error (I think). Something needed for fit function
            double[] coefficients = new double[] { 0.01, dx[ArrayOperation.GetIndexOfMax(dy2)],
                ArrayOperation.GetMax(dy2) - ArrayOperation.GetMin(dy2)}; //parameters to fit. {width, centroid, amplitude}. Actually not fitting to width at all.
            CurveFit.NonLinearFit(dx, dy2, new ModelFunctionCallback(lorentzian),
                 coefficients, out mse, 1000); //Fit a lorentzian.
            ui.fitsPlot(makeFitPlotData(data, coefficients)); //Plot out the fit (should figure out how to do that on same graph when I get a chance)
            if (coefficients[1] > 0.0 && coefficients[1] < 10.0 
                && coefficients[1] < UpperRampLimit && coefficients[1] > LowRampLimit) //Only change limits if fits are reasonnable.
            {
                UpperRampLimit = coefficients[1] + 0.17;//Adjust scan range!
                LowRampLimit = coefficients[1] - 0.17;
            }
            return coefficients; //return the fit parameters for later use.

        }

        /// <summary>
        /// Laser lock! Fits to the laser peaks and operates the lock. It needs the data from the scan,
        /// the parameters from the cavity fit and some info on where to write to.
        /// </summary>
        private void fitLaserAndLockToReference(double[,] data, double[] parameters, 
            AnalogSingleChannelWriter writeChannel)
        {
            bool lockBool;                          //whether to actually lock the laser
            int i = 0;                              //some data manipulation
            double oldLaserVoltage = LaserVoltage;
            double[] dx = new double[STEPS];
            double[] dy1 = new double[STEPS];
            for (i = 0; i < STEPS; i++)
            {
                dx[i] = data[0, i];
                dy1[i] = data[1, i];
            }
            double mse = 0;
            double[] coefficients = new double[] { 0.002, dx[ArrayOperation.GetIndexOfMax(dy1)],
                ArrayOperation.GetMax(dy1) - ArrayOperation.GetMin(dy1)};
            CurveFit.NonLinearFit(dx, dy1, new ModelFunctionCallback(lorentzianNarrow),
                 coefficients, out mse, 1000);          //Fitting a lorentzian
            ui.fitsPlot2(makeFitPlotData(data, coefficients));
            
            if (coefficients[1] > 0.0 && coefficients[1] < 10.0 
                && coefficients[1] < UpperRampLimit && coefficients[1] > LowRampLimit
                && LaserVoltage < 10.0 && LaserVoltage > -10.0)//make sure we're not sending stupid voltages to the laser
            {
                lockBool = ui.checkLockEnableCheck();   //check whether lock is engaged
                if (lockBool == true)                   //if locking
                {
                    if (FirstLock == true)              //if this is the first time we're trying to lock
                    {
                        LaserSetPoint = coefficients[1] - parameters[1];    //SetPoint is difference between peaks
                        ui.SetSetPoint(LaserSetPoint);                      //Set this value to the box
                        FirstLock = false;                                  //Lock for real next time
                    }
                    if (FirstLock == false)                                 //We've been here before
                    {
                        LaserSetPoint = ui.GetSetPoint();                   //get the set point
                    }
                    LaserVoltage = oldLaserVoltage + gain * (parameters[1] - coefficients[1] + LaserSetPoint); //Feedback
                    writeChannel.WriteSingleSample(true, LaserVoltage);     //Write the new value of the laser control voltage
                }
                if (lockBool == false)                  //If we're not really locking, make sure the firstlock bool is true (for when we activate the lock)
                { 
                    FirstLock = true;  
                }
                ui.WriteToVoltageToLaserBox(Convert.ToString(Math.Round(LaserVoltage, 3))); //Write out the voltage actually being sent to the laser
            }


                  
        }


        private double lorentzian(double x, double[] parameters) //A Lorentzian
        {
            double width = parameters[0];
            double centroid = parameters[1];
            double amplitude = parameters[2];
            if (width < 0) width = Math.Abs(width); // watch out for divide by zero
            return amplitude / (1 + Math.Pow((1 / 0.01), 2) * Math.Pow(x - centroid, 2));
        }
        private double lorentzianNarrow(double x, double[] parameters) //A Narrow Lorentzian (Kind of silly to have to have this...)
        {
            double width = parameters[0];
            double centroid = parameters[1];
            double amplitude = parameters[2];
            if (width < 0) width = Math.Abs(width); // watch out for divide by zero
            return amplitude / (1 + Math.Pow((1 / 0.002), 2) * Math.Pow(x - centroid, 2));
        }

        private double[,] makeFitPlotData(double[,] data, double[] parameters) //from a fit, make the data to plot
        {
            int i = 0;
            double[,] fitData = new double[2,STEPS];
            double width = parameters[0];
            double centroid = parameters[1];
            double amplitude = parameters[2];
            for(i=0; i<STEPS; i++)
            {
                fitData[0, i] = data[0, i];
                fitData[1, i] = amplitude / (1 + Math.Pow((1/width),2) * Math.Pow(data[0, i] - centroid, 2));
            }
            return fitData;

        }
        #endregion
    }
    
   


}
