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
        private const double UPPER_CC_VOLTAGE_LIMIT = 3.1; //volts CC: Cavity control
        private const double LOWER_CC_VOLTAGE_LIMIT = 2.9; //volts CC: Cavity control
        private const double FINESSE_COEFFICIENT_LASER = 200;
        private const double FINESSE_COEFFICIENT_REF_LASER = 100;
        private const int DELAY_BETWEEN_STEPS = 0; //milliseconds?
        private const int STEPS = 100;

        private double upper_cc_voltage_limit_fast = 3.1; //volts CC: Cavity control
        private double lower_cc_voltage_limit_fast = 2.9; //volts CC: Cavity control


        //private ScanMaster.Controller scanMaster;
        //private ScanMaster.Analyze.GaussianFitter fitter;
        //private SympatheticHardwareControl.Controller hardwareControl;

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
        private bool fitting = false;
        public object rampStopLock = new object();
        private double finesse = 1;

        
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

        #region Public properties

        // the getter asks the hardware controller for the laser frequency control voltage
        // the setter sets the value and tells the hardware controller about it
       // public double LaserVoltage
       // {
       //     get
       //     {
       //         return hardwareControl.LaserFrequencyControlVoltage;
       //     }
       //     set
       //     {
       //         if (value >= LOWER_LC_VOLTAGE_LIMIT && value <= UPPER_LC_VOLTAGE_LIMIT)
       //         {
       //             if (!Environs.Debug)
       //             {
       //                 laserWriter.WriteSingleSample(true, value);
       //                 outputLaserTask.Control(TaskAction.Unreserve);
       //             }
       //             else
       //             {
       //                // Debug mode, do nothing
       //             }
                    //hardwareControl.LaserFrequencyControlVoltage = value;
       //         }
       //         else
       //         {
       //            // Out of range, do nothing
       //         }
       //    }
       // }
        // the getter asks the hardware controller for the cavity control voltage
        // the setter sets the value and tells the hardware controller about it
       // public double CavityVoltage
       // {
       //     get
       //     {
       //         return hardwareControl.CavityControlVoltage;
       //     }
       //     set
       //     {
       //         if (value >= LOWER_CC_VOLTAGE_LIMIT && value <= UPPER_CC_VOLTAGE_LIMIT)
       //         {
       //             if (!Environs.Debug)
       //             {
       //                 cavityWriter.WriteSingleSample(true, value);
       //                 outputCavityTask.Control(TaskAction.Unreserve);
       //             }
       //             else
       //             {
       //                 // Debug mode, do nothing
       //             }
       //             //hardwareControl.CavityControlVoltage = value;
       //         }
       //         else
       //         {
       //             // Out of range, do nothing
       //         }
       //     }
       // }

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
            Thread rampThread = new Thread(new ThreadStart(rampLoopFast));
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
        public bool Fitting
        {
            set
            {
                fitting = value;
            }
            get { return fitting; }
        }
        public int RampSteps
        {
            get { return STEPS; }
        }


        public void FitAndDisplay(double[,] data)
        {
            int i = 0;
            double[] dx = new double[STEPS];
            double[] dy1 = new double[STEPS];
            double[] dy2 = new double[STEPS];
            string[] laserFitResults = new string[] {"0","0"};
            string[] refLaserFitResults = new string[] { "0", "0"};
            cavityFitResults p1Res;
            cavityFitResults p2Res;

            for (i = 0; i < STEPS; i++)
            {
                dx[i] = data[0, i];
                dy1[i] = data[1, i];
                dy2[i] = data[2, i];
            }
            finesse = FINESSE_COEFFICIENT_LASER;
            p1Res = fitCavityScan(dx, dy1);
            laserFitResults = convertFitResultsToString(p1Res);
            ui.fitsPlot(makeFitPlot(data, p1Res));

            finesse = FINESSE_COEFFICIENT_REF_LASER;
            p2Res = fitCavityScan(dx, dy2);
            refLaserFitResults = convertFitResultsToString(p2Res);
            ui.fitsPlot2(makeFitPlot(data, p2Res));

           ui.WriteToGlobalPhase1Box(laserFitResults[0]);
           ui.WriteToGlobalPhase2Box(refLaserFitResults[0]);
           ui.WriteToInterval1Box(laserFitResults[1]);
           ui.WriteToInterval2Box(refLaserFitResults[1]);
          
        }

        public void FitAndDisplayFast(double[,] data)
        {
            int i = 0;
            double[] dx = new double[STEPS];
            double[] dy1 = new double[STEPS];
            double[] dy2 = new double[STEPS];
            double[] results1 = new double[4];
            double[] results2 = new double[4];


            for (i = 0; i < STEPS; i++)
            {
                dx[i] = data[0, i];
                dy1[i] = data[1, i];
                dy2[i] = data[2, i];
            }

            //results1 = CurveFit.GaussianFit(dx, dy1);
            CurveFit.GaussianFitInPlace(dx, dy1, FitMethod.LeastSquare, 6, 3, 0.2,
                out results1[0], out results1[1], out results1[2], out results1[3]);
            CurveFit.GaussianFitInPlace(dx, dy2, FitMethod.LeastSquare, 6, 3, 0.5, 
                out results2[0], out results2[1], out results2[2], out results2[3]);
           // results2 = CurveFit.GaussianFit(dx, dy2);
            ui.AddToTextBox(Convert.ToString(results2[1]));
            //Console.Write(Convert.ToString(results2));
            if (results2[1] < 5)
            {
                if (results2[1] > 2)
                {
                    upper_cc_voltage_limit_fast = results2[1] + 0.2;
                    lower_cc_voltage_limit_fast = results2[1] - 0.2;
                }
            }
        }

        public class cavityFitResults
        {
        public double GlobalPhase;      //The offset which I will want to keep constant in the lock
        public double PeakInterval;             //Peak interval
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
            AnalogSingleChannelWriter writeChannel;
            Task taskChannel;

            double[,] data;
            data = new double[3, STEPS];

            writeChannel = cavityWriter;
            taskChannel = outputCavityTask;

            for(; ; )
            {
                data = scanVoltageRange(LOWER_CC_VOLTAGE_LIMIT, UPPER_CC_VOLTAGE_LIMIT, cavityWriter, 
                    outputCavityTask, DELAY_BETWEEN_STEPS, STEPS);
                ui.PlotOnP1(data);
                ui.PlotOnP2(data);
                if (fitting)
                {
                    FitAndDisplay(data);
                }
                lock (rampStopLock)
                {
                    if (ramping == false)
                    {
                       cavityWriter.WriteSingleSample(true, 0);
                       return;
                    }
                }
            }
        }

        private void rampLoopFast()
        {
            AnalogSingleChannelWriter writeChannel;
            Task taskChannel;

            double[,] data;
            data = new double[3, STEPS];

            writeChannel = cavityWriter;
            taskChannel = outputCavityTask;

            for (; ; )
            {
                data = scanVoltageRange(lower_cc_voltage_limit_fast, upper_cc_voltage_limit_fast, cavityWriter,
                    outputCavityTask, DELAY_BETWEEN_STEPS, STEPS);
                ui.PlotOnP1(data);
                ui.PlotOnP2(data);
                if (fitting)
                {
                    FitAndDisplayFast(data);
                }
                lock (rampStopLock)
                {
                    if (ramping == false)
                    {
                        cavityWriter.WriteSingleSample(true, 0);
                        return;
                    }
                }
            }
        }

        private cavityFitResults fitCavityScan(double[] dataX, double[] dataY)
        {
            cavityFitResults results = new cavityFitResults();
            double[] normDataY = new double[STEPS];
            double[] coefficients = new double[] {0,0};
            int i=0;
            double maxY = ArrayOperation.GetMax(dataY);
            
            for (i = 0; i < STEPS; i++)
            {
                normDataY[i] = dataX[i] + dataY[i] / maxY
                    - (UPPER_CC_VOLTAGE_LIMIT - LOWER_CC_VOLTAGE_LIMIT) / 2;
            }
            //Some initial guesses for the parameters
            coefficients[0] = 0;
            coefficients[1] = ui.getIntervalGuess();
         
            double mse = 0;
            CurveFit.NonLinearFit(dataX, normDataY, new ModelFunctionCallback(tiltedAiry),
                coefficients, out mse, 1000);
            results.GlobalPhase = coefficients[0];
            results.PeakInterval = coefficients[1];

            return results;
        }

        private double tiltedAiry(double x, double[] parameters)
        {
            double gp = parameters[0];
            double inter = parameters[1];
            
            return x + 1 / (1 + finesse * Math.Pow(Math.Cos(inter * x + gp), 2))
                -(UPPER_CC_VOLTAGE_LIMIT-LOWER_CC_VOLTAGE_LIMIT)/2;
        }

        private string[] convertFitResultsToString(cavityFitResults results)
        {
            int i = 0;
            string[] textList = new string[2] {"didnt work","crap"};
            double[] resultList = new double[] { results.GlobalPhase,
                results.PeakInterval};
            
            for (i = 0; i < 2; i++)
            {
                textList[i] = Convert.ToString(resultList[i]);
            }
            return textList;
        }

        private double[,] makeFitPlot(double[,] data, cavityFitResults results)
        {
            int i = 0;
            double[,] fitData = new double[2,STEPS];
            for(i=0; i<STEPS; i++)
            {
                fitData[0, i] = data[0, i];
                fitData[1, i] = 1 / (1 + finesse * Math.Pow(Math.Cos(results.PeakInterval * data[0, i] + results.GlobalPhase), 2));
            }
            return fitData;
        }
        #endregion
    }
    
   


}
