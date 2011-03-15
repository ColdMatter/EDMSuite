using System;
using System.Threading;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;
using System.Windows.Forms;
using NationalInstruments.Analysis.Math;

namespace TransferCavityLock
{
    /// <summary>
    /// A class for locking the laser using a transfer cavity.
    /// </summary>
    public class Controller : MarshalByRefObject
    {

        #region Definitions

        private const double UPPER_LC_VOLTAGE_LIMIT = 10.0; //volts LC: Laser control
        private const double LOWER_LC_VOLTAGE_LIMIT = -10.0; //volts LC: Laser control
        private const double UPPER_CC_VOLTAGE_LIMIT = 10.0; //volts CC: Cavity control
        private const double LOWER_CC_VOLTAGE_LIMIT = -10.0; //volts CC: Cavity control
        private double voltageDifference = 0;
        private const int DELAY_BETWEEN_STEPS = 0; //milliseconds?
        /// <summary>
        /// A flag for when the cavity is ramping
        /// </summary>
        public bool Ramping = false;
        /// <summary>
        /// A flag for whether this is the first run with the lock engaged. (To read back the set point)
        /// </summary>
        public bool FirstLock = true;
        /// <summary>
        /// The voltage sent to the laser.
        /// </summary>
        public double LaserVoltage = 0.0;
        public ScanParameters LaserScanParameters;
        public ScanParameters CavityScanParameters;
        private int STEPS = 400;

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
                cavityChannel.AddToTask(outputCavityTask, -10, 10);

                outputCavityTask.Timing.ConfigureSampleClock("", 1000,
                    SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, 2 * STEPS);
                outputCavityTask.AOChannels[0].DataTransferMechanism = AODataTransferMechanism.Dma;

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
                    SampleQuantityMode.FiniteSamples, 2 * STEPS);
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

                CavityScanParameters = new ScanParameters(sampleReader, cavityWriter);
                LaserScanParameters = new ScanParameters(sampleReader, laserWriter);
            }

            Application.Run(ui);
        }

        #endregion

        #region Public methods

        public void startRamp()
        {
            Thread.Sleep(2000);
            Thread rampThread = new Thread(new ThreadStart(rampLoop));
            rampThread.Start();
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
            scanParameters.SetPoint = newSetPoint;
        }


        #endregion

        #region Private methods

        /// <summary>
        /// A function to scan across the voltage range set by the limits high and low. Reads from the two photodiodes and spits out an array.
        /// </summary>
        private void scan(ScanParameters scanParameters, CavityScanData data)
        {
            double[,] tempData = new double[2, scanParameters.Steps];
            double[] ramp = new double[2 * scanParameters.Steps];
            for (int i = 0; i < scanParameters.Steps; i++)
            {
                if (data.Voltages[i] < UPPER_CC_VOLTAGE_LIMIT)
                {
                    ramp[i] = data.Voltages[i];
                }
                else
                {
                    ui.AddToTextBox("Cavity is out of range!");
                    ramp[i] = UPPER_CC_VOLTAGE_LIMIT - 0.01;
                }
            }
            for (int i = scanParameters.Steps; i < 2 * scanParameters.Steps; i++)
            {
                if (data.Voltages[2 * scanParameters.Steps - i - 1] < UPPER_CC_VOLTAGE_LIMIT)
                {
                    ramp[i] = data.Voltages[2 * scanParameters.Steps - i - 1];
                }
                else
                {
                    ui.AddToTextBox("Cavity is out of range!");
                    ramp[i] = UPPER_CC_VOLTAGE_LIMIT - 0.01;
                }
            }

            triggerWriter.WriteSingleSampleSingleLine(true, false);
            scanParameters.Writer.WriteMultiSample(false, ramp);
            sampleTask.Start();
            outputCavityTask.Start();


            Thread.Sleep(10);
            triggerWriter.WriteSingleSampleSingleLine(true, true);
            outputCavityTask.WaitUntilDone();

            tempData = scanParameters.Reader.ReadMultiSample(scanParameters.Steps);
            outputCavityTask.Stop();
            sampleTask.Stop();

            triggerWriter.WriteSingleSampleSingleLine(true, false);
            if (scanParameters.Record.Equals(true))
            {
                for (int i = 0; i < scanParameters.Steps; i++)
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
            bool fit;
            bool locking;

            if (ui.NewStepNumber != CavityScanParameters.Steps)
            {
                CavityScanParameters.Steps = ui.NewStepNumber;
                CavityScanParameters.AdjustStepSize();
            }
            CavityScanParameters.ArmScan(ui.ScanOffset - ui.ScanWidth, ui.ScanOffset + ui.ScanWidth, 0, /*2*CavityScanParameters.Steps*/ STEPS, true, ui.ScanOffset);
            LaserScanParameters.ArmScan(LOWER_LC_VOLTAGE_LIMIT, UPPER_LC_VOLTAGE_LIMIT, 20, 50, false, 0);

            Thread.Sleep(2000);
            for (; ; )
            {
                CavityScanData data = new CavityScanData(CavityScanParameters.Steps);
                data.PrepareData(CavityScanParameters);
                scan(CavityScanParameters, data);

                ui.clearP1();
                ui.clearP2();
                ui.plotXYOnP1(data.Voltages, data.P1Data);
                ui.plotXYOnP2(data.Voltages, data.P2Data);

                fit = ui.checkFitEnableCheck(); //Check to see if fitting is enabled
                locking = ui.checkLockEnableCheck(); //Check to see if locking is enabled
                if (locking == false) //if not locking
                {
                    if (this.LaserVoltage != ui.GetLaserVoltage())
                    {
                        ui.AddToTextBox("Ramping laser!");
                        StepToNewSetPoint(LaserScanParameters, ui.GetLaserVoltage());

                        this.LaserVoltage = ui.GetLaserVoltage(); //set the laser voltage to the voltage indicated in the "updown" box
                        ui.AddToTextBox("Ramping finished!");
                        ui.WriteToVoltageToLaserBox(Convert.ToString(Math.Round(LaserVoltage, 3)));
                    }

                }
                if (fit == true)
                {
                    stabilizeCavity(data, CavityScanParameters); //Fit to cavity peaks

                    if (locking == true)
                    {
                        lockLaser(data); //lock the laser!
                    }
                }

                lock (rampStopLock) //This is to break out of the ramping to stop the program.
                {
                    if (Ramping == false)
                    {
                        this.LaserVoltage = 0;
                        StepToNewSetPoint(LaserScanParameters, LaserVoltage);
                        ui.WriteToVoltageToLaserBox(Convert.ToString(Math.Round(LaserVoltage, 3)));
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
                voltages[i] = data.Voltages[i];
                reducedData2[i] = data.P2Data[i];
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
                parameters.High = coefficients[1] + ui.ScanWidth;//Adjust scan range!
                parameters.Low = coefficients[1] - ui.ScanWidth;
            }
        }

        /// <summary>
        /// Laser lock! Fits to the laser peaks and operates the lock. It needs the data from the scan,
        /// the parameters from the cavity fit and some info on where to write to.
        /// </summary>
        private void lockLaser(CavityScanData data)
        {
            double oldLaserVoltage = LaserVoltage;
            double mse = 0;
            double[] voltages = new double[CavityScanParameters.Steps];
            double[] reducedData1 = new double[CavityScanParameters.Steps];
            for (int i = 0; i < CavityScanParameters.Steps; i++)
            {
                voltages[i] = data.Voltages[i];
                reducedData1[i] = data.P1Data[i];
            }
            double[] coefficients = new double[] {0.05, voltages[ArrayOperation.GetIndexOfMax(reducedData1)],
                ArrayOperation.GetMax(reducedData1) - ArrayOperation.GetMin(reducedData1)};
            CurveFit.NonLinearFit(voltages, reducedData1, new ModelFunctionCallback(lorentzianNarrow),
                 coefficients, out mse, 1000);          //Fitting a lorentzian


            if (coefficients[1] < CavityScanParameters.High && coefficients[1] > CavityScanParameters.Low && oldLaserVoltage < UPPER_LC_VOLTAGE_LIMIT && oldLaserVoltage > LOWER_LC_VOLTAGE_LIMIT)//make sure we're not sending stupid voltages to the laser
            {
                {
                    if (FirstLock == true)              //if this is the first time we're trying to lock
                    {
                        voltageDifference = Math.Round(coefficients[1] - CavityScanParameters.SetPoint, 3);
                        ui.SetSetPoint(voltageDifference);
                        ui.AddToMeasuredPeakDistanceTextBox(Convert.ToString(voltageDifference));
                        LaserVoltage = ui.GetLaserVoltage();
                        FirstLock = false;                                  //Lock for real next time
                    }
                    else                                 //We've been here before
                    {
                        voltageDifference = ui.GetSetPoint();                   //get the set point
                        ui.AddToMeasuredPeakDistanceTextBox(Convert.ToString(Math.Round(coefficients[1] - CavityScanParameters.SetPoint, 3)));
                        LaserVoltage = oldLaserVoltage - ui.GetGain() * (voltageDifference - Math.Round(coefficients[1] - CavityScanParameters.SetPoint, 3)); //Feedback

                        if (LaserVoltage > UPPER_LC_VOLTAGE_LIMIT || LaserVoltage < LOWER_LC_VOLTAGE_LIMIT)
                        {
                            ui.AddToTextBox("Cannot lock: set point exceeds range which can be sent to laser");
                        }
                        else
                        {
                            ui.AddToTextBox(Convert.ToString(ui.GetGain()));
                            LaserScanParameters.Writer.WriteSingleSample(true, LaserVoltage);     //Write the new value of the laser control voltage
                        }
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
