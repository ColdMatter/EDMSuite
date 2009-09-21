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
        // It just overrides the lifetime lease system completely.
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
        /// A function to repeatedly scan the voltage. It's meant to be triggered by pressing the
        /// "start ramping" button, and only stop when "stop ramping" is pressed.
        /// </summary>

        public void startRamp()
        {
            //slow rampLoop()
            //Thread rampThread = new Thread(new ThreadStart(rampLoop));
            //rampThread.Start();
            //faster rampLoop();
            cavityWriter.WriteSingleSample(true, 3);
            Thread.Sleep(2000);
            Thread rampThread = new Thread(new ThreadStart(rampLoop));
            rampThread.Start();
        }
        

        

        /// <summary>
        /// A few functions for setting and getting the values rampTriggerMethod, rampChannel and ramping.
        /// </summary>
        public string RampTriggerMethod
        {
            set
            {
                rampTriggerMethod = value;
            }
            get { return rampTriggerMethod; }
        }     
        public bool Ramping
        {
            set
            {
                ramping = value;
            }
            get { return ramping; }
        }
        public bool FirstLock
        {
            set
            {
                first_Lock = value;
            }
            get { return first_Lock; }
        }
        public int RampSteps
        {
            get { return STEPS; }
        }
        public double LowRampLimit
        {
            set { lower_cc_voltage_limit = value; }
            get { return lower_cc_voltage_limit; }
        }
        public double UpperRampLimit
        {
            set { upper_cc_voltage_limit = value;}
            get { return upper_cc_voltage_limit; }
        }
        public double LaserVoltage
        {
            set { laser_voltage = value; }
            get { return laser_voltage; }
        }
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
            Task taskChannel, int delayAtEachStep, int steps)
        {
            double stepsize = (high - low) / steps;
            double voltage = low;
            double[,] data;
            data = new double[3,steps];

            writeChannel.WriteSingleSample(true, low);  //Set initial voltage to low

            for (int i = 0; i < steps; i++)
            {
              
                data[0, i] = voltage;
                data[1, i] = p1Reader.ReadSingleSample();
                data[2, i] = p2Reader.ReadSingleSample();
                voltage += stepsize;
                writeChannel.WriteSingleSample(true, voltage);  //Write new value to channel
                Thread.Sleep(delayAtEachStep);
            }
            return(data);
        }

        private void rampLoop()
        {
            
            double[,] data;
            data = new double[3, STEPS];
            double[] refCavParams = new double[3];
            bool fitBool;
            bool lockBool;

            for (; ; )
            {
                data = scanVoltageRange(LowRampLimit, UpperRampLimit, cavityWriter,
                outputCavityTask, DELAY_BETWEEN_STEPS, STEPS);
                ui.PlotOnP1(data);
                ui.PlotOnP2(data);
                fitBool = ui.checkFitEnableCheck();
                lockBool = ui.checkLockEnableCheck();
                if (lockBool == false)
                {
                    LaserVoltage = ui.getLaserVoltage();
                    laserWriter.WriteSingleSample(true, LaserVoltage);
                }
                if (fitBool == true)
                {
                    refCavParams = fitDisplayAndModifyScanLimits(data);
                    LaserSetPoint = ui.getSetPoint();
                    fitLaserAndLockToReference(data, refCavParams, laserWriter);
                }
                lock (rampStopLock)
                {
                    if (Ramping == false)
                    {
                        cavityWriter.WriteSingleSample(true, 0);
                        return;
                    }
                }
            }
        }

        private double[] fitDisplayAndModifyScanLimits(double[,] data)
        {
            int i = 0;
            double[] dx = new double[STEPS];
            double[] dy2 = new double[STEPS];
            for (i = 0; i < STEPS; i++)
            {
                dx[i] = data[0, i];
                dy2[i] = data[2, i];
            }
            double mse = 0;
            double[] coefficients = new double[] { 0.01, dx[ArrayOperation.GetIndexOfMax(dy2)],
                ArrayOperation.GetMax(dy2) - ArrayOperation.GetMin(dy2)};
            CurveFit.NonLinearFit(dx, dy2, new ModelFunctionCallback(lorentzian),
                 coefficients, out mse, 1000);
            ui.fitsPlot(makeFitPlotData(data, coefficients));
            if (coefficients[1] > 0.0 && coefficients[1] < 10.0 
                && coefficients[1] < UpperRampLimit && coefficients[1] > LowRampLimit) //Only change limits if fits are reasonnable.
            {
                UpperRampLimit = coefficients[1] + 0.15;
                LowRampLimit = coefficients[1] - 0.15;
            }
            return coefficients;

        }


        private void fitLaserAndLockToReference(double[,] data, double[] parameters, 
            AnalogSingleChannelWriter writeChannel)
        {
            bool lockBool;
            int i = 0;
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
                 coefficients, out mse, 1000);
            ui.fitsPlot2(makeFitPlotData(data, coefficients));
            
            if (coefficients[1] > 0.0 && coefficients[1] < 10.0 
                && coefficients[1] < UpperRampLimit && coefficients[1] > LowRampLimit
                && LaserVoltage < 10.0 && LaserVoltage > -10.0)//make sure we're not sending stupid voltages to the laser
            {
                lockBool = ui.checkLockEnableCheck();
                if (lockBool == true)
                {
                    if (FirstLock == true)
                    {
                        LaserSetPoint = coefficients[1] - parameters[1];
                        ui.setSetPoint(LaserSetPoint);
                        FirstLock = false;
                    }
                    if (FirstLock == false)
                    {
                        LaserSetPoint = ui.getSetPoint();
                    }
                    LaserVoltage = oldLaserVoltage + gain * (parameters[1] - coefficients[1] + LaserSetPoint);
                    laserWriter.WriteSingleSample(true, LaserVoltage);
                }
                if (lockBool == false) 
                { 
                    FirstLock = true;  
                }
                ui.WriteToVoltageToLaserBox(Convert.ToString(Math.Round(LaserVoltage, 3)));
            }


                  
        }


        private double lorentzian(double x, double[] parameters)
        {
            double width = parameters[0];
            double centroid = parameters[1];
            double amplitude = parameters[2];
            if (width < 0) width = Math.Abs(width); // watch out for divide by zero
            return amplitude / (1 + Math.Pow((1 / 0.01), 2) * Math.Pow(x - centroid, 2));
        }
        private double lorentzianNarrow(double x, double[] parameters)
        {
            double width = parameters[0];
            double centroid = parameters[1];
            double amplitude = parameters[2];
            if (width < 0) width = Math.Abs(width); // watch out for divide by zero
            return amplitude / (1 + Math.Pow((1 / 0.002), 2) * Math.Pow(x - centroid, 2));
        }

        private double[,] makeFitPlotData(double[,] data, double[] parameters)
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
