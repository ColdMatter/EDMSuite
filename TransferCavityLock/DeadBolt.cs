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
        private const double UPPER_CC_VOLTAGE_LIMIT = 5.0; //volts CC: Cavity control
        private const double LOWER_CC_VOLTAGE_LIMIT = 0.0; //volts CC: Cavity control

        private const int DELAY_BETWEEN_STEPS = 0; //milliseconds?
        private ScanParameters laserScanParameters;
        private ScanParameters cavityScanParameters;
        private int STEPS = 200;

        private double upper_cc_voltage_limit = 3.3; //volts CC: Cavity control
        private double lower_cc_voltage_limit = 2.7; //volts CC: Cavity control
        private double laser_voltage = 0.0;

        private MainForm ui;

        private Task outputLaserTask; //Some stuff to let you write to laser
        private AnalogOutputChannel laserChannel;
        private AnalogSingleChannelWriter laserWriter;

        private Task outputCavityTask; //Some stuff to let you write to piezo driver
        private AnalogOutputChannel cavityChannel;
        private AnalogSingleChannelWriter cavityWriter;

        private Task sampleTask;
        private AnalogInputChannel p1Channel; //p1 is the signal from the laser we are trying to lock
        private AnalogInputChannel p2Channel; //p2 is the signal from the reference He-Ne lock
        private AnalogMultiChannelReader sampleReader;

        private Task sendScanTrigger;
        private DigitalOutputChannel sendTriggerChannel;
        private DigitalSingleChannelWriter triggerWriter;
        
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
                cavityChannel.AddToTask(outputCavityTask, 0, 5);

                outputCavityTask.Timing.ConfigureSampleClock("", 1000 , 
                    SampleClockActiveEdge.Rising,SampleQuantityMode.FiniteSamples, STEPS);
                outputCavityTask.AOChannels[0].DataTransferMechanism = AODataTransferMechanism.Interrupts;
                
                outputCavityTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    (string)Environs.Hardware.GetInfo("analogTrigger3"), DigitalEdgeStartTriggerEdge.Rising);
                outputCavityTask.Control(TaskAction.Verify);
                cavityWriter = new AnalogSingleChannelWriter(outputCavityTask.Stream);

                sampleTask = new Task("ReadPhotodiodes");
                p1Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["p1"];
                p2Channel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["p2"];
                p1Channel.AddToTask(sampleTask, 0, 10);
                p2Channel.AddToTask(sampleTask, 0, 10);
                
                sampleTask.Timing.ConfigureSampleClock(
                    "",
                    1000,
                    SampleClockActiveEdge.Rising,
                    SampleQuantityMode.FiniteSamples, 2*STEPS);
                sampleTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                    (string)Environs.Hardware.GetInfo("analogTrigger2"),
                    DigitalEdgeStartTriggerEdge.Rising);
                sampleTask.Control(TaskAction.Verify);
                sampleReader = new AnalogMultiChannelReader(sampleTask.Stream);




                sendScanTrigger = new Task("Send Cavity Scan Trigger");
                sendTriggerChannel = (DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["cavityTriggerOut"];
                sendTriggerChannel.AddToTask(sendScanTrigger);
                sendScanTrigger.Control(TaskAction.Verify);
                triggerWriter = new DigitalSingleChannelWriter(sendScanTrigger.Stream);

                cavityScanParameters = new ScanParameters(sampleReader, cavityWriter);
                laserScanParameters = new ScanParameters(sampleReader, laserWriter);
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
            Thread.Sleep(2000);
            Thread rampThread = new Thread(new ThreadStart(rampLoop));
            rampThread.Start();
           // Thread triggerThread = new Thread(new ThreadStart(triggerLoop));
           // triggerThread.Start();
            
        }
       
        /// <summary>
        /// A flag for when the cavity is ramping
        /// </summary>

        public bool RAMPING
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
        /// The voltage sent to the laser.
        /// </summary>
        public double LaserVoltage
        {
            set { laser_voltage = value; }
            get { return laser_voltage; }
        }

        public ScanParameters CavityScanParameters
        {
            set { cavityScanParameters = value; }
            get { return cavityScanParameters; }
        }
        public ScanParameters LaserScanParameters
        {
            set { laserScanParameters = value; }
            get { return laserScanParameters; }
        }

        public void StepToNewSetPoint(ScanParameters scanParameters, double newSetPoint)
        {
            scanParameters.Low = scanParameters.SetPoint;
            scanParameters.High = newSetPoint;
            scanParameters.AdjustStepSize();
            double voltage;
            for (int i = 0; i < scanParameters.Steps; i++)
            {
                voltage = scanParameters.Low + i * scanParameters.StepSize;
                scanParameters.Writer.WriteSingleSample(true, voltage);
                Thread.Sleep(scanParameters.SleepTime);
            }
        }

        
        #endregion

        #region Private methods

        /// <summary>
        /// A function to scan across the voltage range set by the limits high and low. Reads from the two photodiodes and spits out an array.
        /// </summary>


        private void scan(ScanParameters scanParameters, CavityScanData data)
        {
   
            for (int i = 0; i < scanParameters.Steps; i++)
            {
                
                scanParameters.Writer.WriteSingleSample(true, data.Voltages[i]);
                if (scanParameters.Record.Equals(true))
                {
                    double[] tempData = new double[] { 0, 0 };
                    tempData = scanParameters.Reader.ReadSingleSample();
                    data.P1Data[i] = tempData[0];
                    data.P2Data[i] = tempData[1];
                }
            }
            
        }

        private void scanFast(ScanParameters scanParameters, CavityScanData data)
        {
            double[,] tempData = new double[2, cavityScanParameters.Steps];
            
            triggerWriter.WriteSingleSampleSingleLine(true, false);
        //    Thread.Sleep(50);
            scanParameters.Writer.WriteMultiSample(false, data.Voltages);
        //    Thread.Sleep(50);
            sampleTask.Start();
            outputCavityTask.Start();              
                
                 
            Thread.Sleep(10);
            triggerWriter.WriteSingleSampleSingleLine(true, true);
            outputCavityTask.WaitUntilDone();
                
            tempData = scanParameters.Reader.ReadMultiSample(cavityScanParameters.Steps);
            outputCavityTask.Stop();
            sampleTask.Stop();
               
            triggerWriter.WriteSingleSampleSingleLine(true, false);
            if (scanParameters.Record.Equals(true))
            {
                for (int i = 0; i < cavityScanParameters.Steps; i++)
                {
                    data.P1Data[i] = tempData[0, i];
                    data.P2Data[i] = tempData[1, i];
                }
            }   
        }

                
    

       
        /// <summary>
        /// The main loop of the program. Scans the cavity, looks at photodiodes, corrects the range for the next
        /// scan and locks the laser.
        /// </summary>
        private void rampLoop()
        {
            
            bool FIT_BOOL;
            bool LOCK_BOOL;
            
            
            cavityScanParameters.ArmScan(lower_cc_voltage_limit,upper_cc_voltage_limit,0, STEPS, true);
            laserScanParameters.ArmScan(LOWER_LC_VOLTAGE_LIMIT, UPPER_LC_VOLTAGE_LIMIT, 20, 50, false);
            CavityScanData data = new CavityScanData(cavityScanParameters.Steps);
            
        //    cavityScanParameters.Writer.WriteSingleSample(true, cavityScanParameters.SetPoint);
            Thread.Sleep(2000);
            for (; ; )
            {
                
                data.PrepareData(cavityScanParameters);
                scanFast(cavityScanParameters, data);

               // triggerWriter.WriteSingleSampleSingleLine(true, false);
                ui.PlotOnP1(data.ConvertCavityDataToDoublesArray()); //Plot laser peaks
                ui.PlotOnP2(data.ConvertCavityDataToDoublesArray()); //Plot He-Ne peaks

                FIT_BOOL = ui.checkFitEnableCheck(); //Check to see if fitting is enabled
                LOCK_BOOL = ui.checkLockEnableCheck(); //Check to see if locking is enabled
                if (LOCK_BOOL == false) //if not locking
                {
                    if (this.LaserVoltage != ui.GetLaserVoltage())
                    {
                        ui.AddToTextBox("Ramping laser!");
                        StepToNewSetPoint(laserScanParameters, ui.GetLaserVoltage());
                        this.LaserVoltage = ui.GetLaserVoltage(); //set the laser voltage to the voltage indicated in the "updown" box
                        ui.AddToTextBox("Ramping finished!");
                        ui.WriteToVoltageToLaserBox(Convert.ToString(Math.Round(LaserVoltage, 3))); 
                    }
                    
                }
                if (FIT_BOOL == true) //if fitting
                {
                    stabilizeCavity(data, cavityScanParameters); //Fit to cavity peaks

                    if (LOCK_BOOL == true)                   //if locking
                    {
                        //   cavityScanParameters.SetPoint = ui.GetSetPoint(); //reads in the setPoint from "updown" box. (only useful when locking)
                        lockLaser(data); //lock the laser!
                    }
                }
                 
                lock (rampStopLock) //This is to break out of the ramping to stop the program.
                {
                    if (RAMPING == false)
                    {
                 //       cavityScanParameters.Writer.WriteSingleSample(true, 0);
                        StepToNewSetPoint(laserScanParameters, 0);
                 //       laserScanParameters.Writer.WriteSingleSample(true, 0);
                 //       triggerWriter.WriteSingleSampleSingleLine(true, false);
                        return;
                    }
                }
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// A program which fits to the reference cavity peaks and adjusts the scan range to keep a peak
        /// in the middle of the scan. 
        /// </summary>
       

        private void stabilizeCavity(CavityScanData data, ScanParameters parameters)
        {
            double[] voltages = new double[parameters.Steps];
            double[] reducedData2 = new double[parameters.Steps];
            for (int i = 0; i < parameters.Steps; i++)
            {
                if (i > parameters.Steps / 4 && i < 3 * parameters.Steps / 4)
                {
                    voltages[i] = data.Voltages[i];
                    reducedData2[i] = data.P2Data[i];
                }
                else
                {
                    voltages[i] = 0;
                    reducedData2[i] = 0;
                }
            }
            double mse = 0; //Mean standard error (I think). Something needed for fit function
            double[] coefficients = new double[] {0.01, voltages[ArrayOperation.GetIndexOfMax(reducedData2)],
                ArrayOperation.GetMax(reducedData2) - ArrayOperation.GetMin(reducedData2)}; //parameters to fit. {width, centroid, amplitude}. Actually not fitting to width at all.
            CurveFit.NonLinearFit(voltages, reducedData2, new ModelFunctionCallback(lorentzian),
                 coefficients, out mse, 1000); //Fit a lorentzian
            if (coefficients[1] > LOWER_CC_VOLTAGE_LIMIT && coefficients[1] < UPPER_CC_VOLTAGE_LIMIT
                && coefficients[1] < parameters.High + 1.0 && coefficients[1] > parameters.Low - 1.0) //Only change limits if fits are reasonnable.
            {
                parameters.SetPoint = coefficients[1];  //The set point for the cavity.
                parameters.High = coefficients[1] + 0.3;//Adjust scan range!
                parameters.Low = coefficients[1] - 0.3;
              //  parameters.AdjustStepSize();
            }
            //return coefficients; //return the fit parameters for later use.

        }
     /*

        private void stabilizeCavity(CavityScanData data, ScanParameters parameters)
        {
            
            double[] coefficients = new double[] {0.01, data.Voltages[ArrayOperation.GetIndexOfMax(data.P2Data)],
               ArrayOperation.GetMax(data.P2Data) - ArrayOperation.GetMin(data.P2Data)}; //parameters to fit. {width, centroid, amplitude}. Actually not fitting to width at all.
            
            if (coefficients[1] > LOWER_CC_VOLTAGE_LIMIT && coefficients[1] < UPPER_CC_VOLTAGE_LIMIT
                && coefficients[1] < parameters.High  && coefficients[1] > parameters.Low) //Only change limits if fits are reasonnable.
            {
                parameters.SetPoint = coefficients[1];  //The set point for the cavity.
                parameters.High = coefficients[1] + 0.17;//Adjust scan range!
                parameters.Low = coefficients[1] - 0.17;
       //         parameters.AdjustStepSize();
            }
            //return coefficients; //return the fit parameters for later use.
            ui.AddToTextBox(Convert.ToString(coefficients[1]));
        }
        */
        /// <summary>
        /// Laser lock! Fits to the laser peaks and operates the lock. It needs the data from the scan,
        /// the parameters from the cavity fit and some info on where to write to.
        /// </summary>
        private void lockLaser(CavityScanData data)
        {

            
            double oldLaserVoltage = LaserVoltage;
            
            double mse = 0;
            double[] voltages = new double[cavityScanParameters.Steps];
            double[] reducedData1 = new double[cavityScanParameters.Steps];
            for (int i = 0; i < cavityScanParameters.Steps; i++)
            {
                if (i > cavityScanParameters.Steps / 4 && i < 3 * cavityScanParameters.Steps / 4)
                {
                    voltages[i] = data.Voltages[i];
                    reducedData1[i] = data.P1Data[i];
                }
                else
                {
                    voltages[i] = 0;
                    reducedData1[i] = 0;
                }
            }
            double[] coefficients = new double[] { 0.002, voltages[ArrayOperation.GetIndexOfMax(reducedData1)],
                ArrayOperation.GetMax(reducedData1) - ArrayOperation.GetMin(reducedData1)};
            CurveFit.NonLinearFit(voltages, reducedData1, new ModelFunctionCallback(lorentzianNarrow),
                 coefficients, out mse, 1000);          //Fitting a lorentzian
            
            
            if (coefficients[1] < cavityScanParameters.High && coefficients[1] > cavityScanParameters.Low && oldLaserVoltage < UPPER_LC_VOLTAGE_LIMIT && oldLaserVoltage > LOWER_LC_VOLTAGE_LIMIT)//make sure we're not sending stupid voltages to the laser
            {
                {
                    if (FirstLock == true)              //if this is the first time we're trying to lock
                    {
                        laserScanParameters.SetPoint = coefficients[1] - cavityScanParameters.SetPoint;    //SetPoint is difference between peaks
                        ui.SetSetPoint(laserScanParameters.SetPoint);                      //Set this value to the box
                        FirstLock = false;                                  //Lock for real next time
                    }
                    if (FirstLock == false)                                 //We've been here before
                    {
                        laserScanParameters.SetPoint = ui.GetSetPoint();                   //get the set point
                    }
                    LaserVoltage = oldLaserVoltage + ui.GetGain() * (cavityScanParameters.SetPoint - coefficients[1] + laserScanParameters.SetPoint); //Feedback
                    if (LaserVoltage > UPPER_LC_VOLTAGE_LIMIT || LaserVoltage < LOWER_LC_VOLTAGE_LIMIT)
                    {
                        ui.AddToTextBox("Cannot lock: set point exceeds range which can be sent to laser");
                    }
                    else
                    {
                        ui.AddToTextBox(Convert.ToString(ui.GetGain()));
                        laserScanParameters.Writer.WriteSingleSample(true, LaserVoltage);     //Write the new value of the laser control voltage
                    }
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



        #endregion
    }
    
   


}
