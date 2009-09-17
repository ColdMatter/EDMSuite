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
        private const double UPPER_CC_VOLTAGE_LIMIT = 3.5; //volts CC: Cavity control
        private const double LOWER_CC_VOLTAGE_LIMIT = 2.0; //volts CC: Cavity control
        private const int DELAY_BETWEEN_STEPS = 0; //milliseconds?
        private const int STEPS = 1000;


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
        public object rampStopLock = new object();
    //    public object dataReadLock = new object();

        
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
            Thread rampThread = new Thread(new ThreadStart(rampLoop));
            rampThread.Start();
            //rampLoop();
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
            string[] laserFitResults = new string[] {"0","0","0", "0","0"};
            string[] refLaserFitResults = new string[] { "0", "0", "0","0","0" };

            for (i = 0; i < STEPS; i++)
            {
                dx[i] = data[0, i];
                dy1[i] = data[1, i];
                dy2[i] = data[2, i];
            }
           laserFitResults = convertFitResultsToString(fitCavityScan(dx, dy1));
           refLaserFitResults = convertFitResultsToString(fitCavityScan(dx, dy2));

           ui.WriteToAmplitude1Box(laserFitResults[3]);
           ui.WriteToAmplitude2Box(refLaserFitResults[3]);
           ui.WriteToBackground1Box(laserFitResults[4]);
           ui.WriteToBackground2Box(refLaserFitResults[4]);
           ui.WriteToGlobalPhase1Box(laserFitResults[0]);
           ui.WriteToGlobalPhase2Box(refLaserFitResults[0]);
           ui.WriteToFreq1Box(laserFitResults[1]);
           ui.WriteToFreq2Box(refLaserFitResults[1]);
           ui.WriteToFinesse1Box(laserFitResults[2]);
           ui.WriteToFinesse2Box(refLaserFitResults[2]);
        }
    
        public class cavityFitResults
        {
        public double GlobalPhase;      //The offset which I will want to keep constant in the lock
        public double Freqency;             //Something (in ang. freq. units) which maps voltage to wavelength(?)
        public double Finesse;
        public double Amplitude;
        public double Background;
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
                //lock (dataReadLock)
                //{
                    data[0, i] = voltage;
                //     inputLaserTask.Start();
                    data[1, i] = p1Reader.ReadSingleSample();
                //     inputLaserTask.Stop();
                //     inputRefLaserTask.Start();
                    data[2, i] = p2Reader.ReadSingleSample();
                //     inputRefLaserTask.Stop();
                //}
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
                FitAndDisplay(data);
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
            double[] coefficients = new double[] {0,0,0,0,0};
            int i=0;
            double bgEstimate = 0;
            for (i=0; i<STEPS; i++)
            {
                bgEstimate += dataY[i];
            }
            bgEstimate = bgEstimate / STEPS;
            //Some initial guesses for the parameters
            coefficients[0] = 0;
            coefficients[1] = 12;
            coefficients[2] = 60;
        //    coefficients[3] = ArrayOperation.GetMax(dataY)-bgEstimate;
        //    coefficients[4] = bgEstimate;
            double mse = 0;

            CurveFit.NonLinearFit(dataX, dataY, new ModelFunctionCallback(airy),
                coefficients, out mse, 100000);
            results.GlobalPhase = coefficients[0];
            results.Freqency = coefficients[1];
            results.Finesse = coefficients[2];
           // results.Amplitude = coefficients[3];
           // results.Background = coefficients[4];

            return results;
        }

        private double airy(double x, double[] parameters)
        {
            double gp = parameters[0];
            double freq = parameters[1];
            double F = parameters[2];
        //    double a = parameters[3];
        //    double b = parameters[4];

            return  6 / (1 + F * Math.Pow(Math.Cos(freq * x +gp), 2))+0.5;
        }
        
        private string[] convertFitResultsToString(cavityFitResults results)
        {
            int i = 0;
            string[] textList = new string[] { "0", "0", "0" ,"0","0"};
            double[] resultList = new double[] { results.GlobalPhase,
                results.Freqency, results.Finesse, results.Amplitude,results.Background};
            for (i = 0; i < 3; i++)
            {
                textList[i] = Convert.ToString(resultList[i]);
            }
            return textList;
        }

        #endregion
    }
    
   


}
