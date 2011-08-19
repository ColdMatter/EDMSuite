using System;
using System.Threading;
using NationalInstruments.DAQmx;
using DAQ.TransferCavityLock;
using DAQ.Environment;
using DAQ.HAL;
using System.Windows.Forms;

namespace TransferCavityLock
{
    /// <summary>
    /// A class for locking the laser using a transfer cavity.
    /// </summary>
    public class Controller : MarshalByRefObject
    {

        #region Declarations

        private const double UPPER_LC_VOLTAGE_LIMIT = 10.0; //volts LC: Laser control
        private const double LOWER_LC_VOLTAGE_LIMIT = -10.0; //volts LC: Laser control
        private const double UPPER_CC_VOLTAGE_LIMIT = 10.0; //volts CC: Cavity control
        private const double LOWER_CC_VOLTAGE_LIMIT = 0; //volts CC: Cavity control

        private const double TWEAK_GAIN = 0.01;
        public int Increments = 0;          // for tweaking the laser set point
        public int Decrements = 0;
        public double setPointIncrementSize = TWEAK_GAIN;

        public const int default_ScanPoints = 200;
        public const double default_ScanOffset = 6;
        public const double default_ScanWidth = 2.7;
        public const double default_Gain = 0.5;
        public const double default_VoltageToLaser = 0.0;



        private MainForm ui;

        //private TransferCavityLockable tcl = 
        //   (TransferCavityLockable)Activator.GetObject(typeof(TransferCavityLockable), 
        //    "tcp://localhost:1172/controller.rem");
        TransferCavityLockable tcl = new DAQMxTCLHelperSWTimed("cavity", "analogTrigger3",
            "laser", "p2", "p1", "analogTrigger2", "cavityTriggerOut");


        public enum ControllerState
        {
            STOPPED, FREERUNNING, CAVITYSTABILIZED, LASERLOCKING, LASERLOCKED
        };
        public ControllerState State = ControllerState.STOPPED;

        public enum LaserState
        {
            FREE, BUSY
        };
        public LaserState lState = LaserState.FREE;

        public object rampStopLock = new object();
        public object tweakLock = new object();

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

            State = ControllerState.STOPPED;
            initializeControllerValues();
            Application.Run(ui);
        }

        #endregion

        #region Public methods

        //This gets called from the form_load atm. ask Jony to see if this can be changed.
        //putting it in start doesn't seem to do anything.
        public void InitializeUI()
        {
            setUIInitialValues();
            ui.updateUIState(State);
        }

        public void StartRamp()
        {
            State = Controller.ControllerState.FREERUNNING;
            Thread.Sleep(2000);
            Thread rampThread = new Thread(new ThreadStart(rampLoop));
            ui.updateUIState(State);
            
            rampThread.Start();
        }

        public void StopRamp()
        {
            State = ControllerState.STOPPED;
            ui.updateUIState(State); 
        }

        public void EngageLock()
        {
            State = ControllerState.LASERLOCKING;
            ui.updateUIState(State);
        }
        public void DisengageLock()
        {
            State = ControllerState.FREERUNNING;
            ui.updateUIState(State);
        }
        public void StabilizeCavity()
        {
            State = ControllerState.CAVITYSTABILIZED;
            ui.updateUIState(State);
        }
        public void UnlockCavity()
        {
            State = ControllerState.FREERUNNING;
            ui.updateUIState(State);
        }

        private int numberOfPoints;
        private double scanWidth;

        private double voltageToLaser;
        public double VoltageToLaser
        {
            get
            {
                return voltageToLaser;
            }
            set
            {
                voltageToLaser = value;
                ui.SetLaserVoltage(voltageToLaser);
            }
        }
        internal void WindowVoltageToLaserChanged(double voltage)
        {
            voltageToLaser = voltage;
        }
        
        private double gain;
        public double Gain
        {
            get
            {
                return gain;
            }
            set
            {
                gain = value;
                ui.SetGain(gain);
            }
        }
        internal void WindowGainChanged(double g)
        {
            gain = g;
        }

        private double scanOffset;
        public double ScanOffset
        {
            get
            {
                return scanOffset;
            }
            set
            {
                scanOffset = value;
                ui.SetScanOffset(scanOffset);
            }
        }
        private double laserSetPoint;
        public double LaserSetPoint
        {
            get 
            { 
                return laserSetPoint; 
            }
            set
            {
                laserSetPoint = value;
                ui.SetLaserSetPoint(laserSetPoint);
            }
        }


        #endregion

        #region Private methods

        // This sets the initial values into the various boxes on the UI.
        private void setUIInitialValues()
        {
            ui.SetScanOffset(default_ScanOffset);
            ui.SetScanWidth(default_ScanWidth);
            ui.SetLaserVoltage(default_VoltageToLaser);
            ui.SetGain(default_Gain);
            ui.SetNumberOfPoints(default_ScanPoints);
            ui.SetSetPointIncrementSize(setPointIncrementSize);
        }
        private void initializeControllerValues()
        {
            scanOffset = default_ScanOffset;
            voltageToLaser = default_VoltageToLaser;
            gain = default_Gain;
        }
        /// <summary>
        /// A function to scan across the voltage range set by the limits high and low. 
        /// Reads from the two photodiodes and spits out an array.
        /// The basic unit of the program.
        /// </summary>
        private CavityScanData scan(ScanParameters sp)
        {
            CavityScanData scanData = new CavityScanData(sp.Steps);
            scanData.parameters = sp;

            double[] voltages = sp.CalculateRampVoltages();

            tcl.ScanCavity(voltages, false);
            tcl.StartScan();

            Thread.Sleep(100);
            tcl.SendScanTriggerAndWaitUntilDone();

            scanData.PhotodiodeData = tcl.ReadPhotodiodes(sp.Steps);

            tcl.StopScan();

            return scanData;
        }


        /// <summary>
        /// The main loop. Scans the cavity, looks at photodiodes, corrects the cavity scan range for the next
        /// scan and locks the laser.
        /// It does a first scan of the data before starting.
        /// It then enters a loop where the next scan is prepared. The preparation varies depending on 
        /// the ControllerState. Once all the preparation is done, the next scan is started. And so on.
        /// </summary>
        private void rampLoop()
        {
            readParametersFromUI();
            ScanParameters sp = createInitialScanParameters();
            initializeHardware();
            CavityScanData scanData = scan(sp);

            double[] masterDataFit;
            double[] slaveDataFit;

            while (State != ControllerState.STOPPED)
            {
                displayData(sp, scanData);

                masterDataFit = CavityScanFitHelper.FitLorenzianToMasterData(scanData, sp.Low, sp.High);
                displayMasterFit(sp, masterDataFit);
                switch (State)
                {
                    case ControllerState.FREERUNNING:
                        releaseLaser();
                        break;

                    case ControllerState.CAVITYSTABILIZED:
                        ScanOffset = calculateNewScanCentre(sp, masterDataFit);
                        sp.High = ScanOffset + scanWidth;
                        sp.Low = ScanOffset - scanWidth;
                       
                        engageLaser();
                        break;

                    case ControllerState.LASERLOCKING:
                        ScanOffset = calculateNewScanCentre(sp, masterDataFit);
                        sp.High = ScanOffset + scanWidth;
                        sp.Low = ScanOffset - scanWidth;

                       
                        
                        slaveDataFit = CavityScanFitHelper.FitLorenzianToSlaveData(scanData, sp.Low, sp.High);
                        displaySlaveFit(sp, slaveDataFit);
                        LaserSetPoint = CalculateLaserSetPoint(masterDataFit, slaveDataFit);

                        State = ControllerState.LASERLOCKED;
                        ui.updateUIState(State);
                        break;

                    case ControllerState.LASERLOCKED:
                        ScanOffset = calculateNewScanCentre(sp, masterDataFit);
                        sp.High = ScanOffset + scanWidth;
                        sp.Low = ScanOffset - scanWidth;

                        LaserSetPoint = tweakSetPoint(LaserSetPoint); //does nothing if not tweaked

                        slaveDataFit = CavityScanFitHelper.FitLorenzianToSlaveData(scanData, sp.Low, sp.High);
                        displaySlaveFit(sp, slaveDataFit);
                        Console.WriteLine("width=" + slaveDataFit[0].ToString() + ", centre =" + slaveDataFit[1].ToString()
                            + ", amp=" + slaveDataFit[2].ToString() + ", offset=" + slaveDataFit[3].ToString());
                       
                        double shift = calculateDeviationFromSetPoint(LaserSetPoint, masterDataFit, slaveDataFit);
                        VoltageToLaser = calculateNewVoltageToLaser(VoltageToLaser, shift);

                        break;

                }
                if (lState == LaserState.BUSY)
                {
                    tcl.SetLaserVoltage(VoltageToLaser);
                }

                scanData = scan(sp);
            }

            finalizeRamping();
        }


        private void displayData(ScanParameters sp, CavityScanData data)
        {
            ui.ScatterGraphPlot(ui.SlaveLaserIntensityScatterGraph,
                ui.SlaveDataPlot, sp.CalculateRampVoltages(),  data.SlavePhotodiodeData);
            ui.ScatterGraphPlot(ui.MasterLaserIntensityScatterGraph, ui.MasterDataPlot,
                sp.CalculateRampVoltages(), data.MasterPhotodiodeData);
        }
        private void displayMasterFit(ScanParameters sp, double[] fitCoefficients)
        {
            double[] fitPoints = new double[sp.Steps];
            double[] ramp = sp.CalculateRampVoltages();
            double n = fitCoefficients[3];
            double q = fitCoefficients[2];
            double c = fitCoefficients[1];
            double w = fitCoefficients[0];
            for (int i = 0; i < sp.Steps; i++)
            {
                if (w == 0) w = 0.001; // watch out for divide by zero
                fitPoints[i] = n + q * (1 / (1 + (((ramp[i] - c) * (ramp[i] - c)) / ((w / 2) * (w / 2)))));
            }
            ui.ScatterGraphPlot(ui.MasterLaserIntensityScatterGraph,
                ui.MasterFitPlot, ramp, fitPoints);

        }
        
        private void displaySlaveFit(ScanParameters sp, double[] fitCoefficients)
        {
            double[] fitPoints = new double[sp.Steps];
            double[] ramp = sp.CalculateRampVoltages();
            double n = fitCoefficients[3];
            double q = fitCoefficients[2];
            double c = fitCoefficients[1];
            double w = fitCoefficients[0];
            for (int i = 0; i < sp.Steps; i++)
            {
                if (w == 0) w = 0.001; // watch out for divide by zero
                fitPoints[i] = n + q * (1 / (1 + (((ramp[i] - c) * (ramp[i] - c)) / ((w / 2) * (w / 2)))));
            }
            ui.ScatterGraphPlot(ui.SlaveLaserIntensityScatterGraph,
                ui.SlaveFitPlot, ramp, fitPoints);

        }
        /// <summary>
        /// Gets some parameters from the UI and stores them on the controller.
        /// </summary>
        private void readParametersFromUI()
        {
            // read out UI params
            numberOfPoints = ui.GetNumberOfPoints();
            scanWidth = ui.GetScanWidth();
            scanOffset = ui.GetScanOffset();
        }
        

        private ScanParameters createInitialScanParameters()
        {
            ScanParameters sp = new ScanParameters();
            sp.Steps = numberOfPoints;
            sp.Low = ScanOffset - scanWidth;
            sp.High = ScanOffset + scanWidth;
            sp.SleepTime = 0;

            return sp;
        }

        private void finalizeRamping()
        {
            tcl.ReleaseCavityHardware();
            if (lState == LaserState.BUSY)
            {
                VoltageToLaser = 0.0;
                tcl.SetLaserVoltage(0.0);
                tcl.ReleaseLaser();
                lState = LaserState.FREE;
            }
        }
        private void releaseLaser()
        {
            if (lState == LaserState.BUSY)
            {
                tcl.ReleaseLaser();
                lState = LaserState.FREE;
            }
        }
        private void engageLaser()
        {
            if (lState == LaserState.FREE)
            {
                lState = LaserState.BUSY;
                tcl.ConfigureSetLaserVoltage(VoltageToLaser);
            }
        }
        private void initializeHardware()
        {
            tcl.ConfigureCavityScan(numberOfPoints, false);
            tcl.ConfigureReadPhotodiodes(numberOfPoints, false);
            tcl.ConfigureScanTrigger();
            //tcl.ConfigureSetLaserVoltage(VoltageToLaser);
        }

        
        /// <summary>
        /// This adjusts the scan range of the next scan, so that the HeNe peak stays in the middle of the scan.
        /// It modifies the scan parameters that are passed to it.
        /// </summary>
        private double calculateNewScanCentre(ScanParameters scanParameters, double[] fitCoefficients)
        {
            double newCentroid = new double();
            if (fitCoefficients[1] - scanWidth > LOWER_CC_VOLTAGE_LIMIT
               && fitCoefficients[1] + scanWidth < UPPER_CC_VOLTAGE_LIMIT
               && fitCoefficients[1] < scanParameters.High
               && fitCoefficients[1] > scanParameters.Low) //Only change limits if fits are reasonable.
            {
                newCentroid = fitCoefficients[1];
            }
            else
            {
                newCentroid = scanParameters.High - scanWidth;
            }
            Console.WriteLine(newCentroid); 
            return newCentroid;
        }

        /// <summary>
        /// Measures the laser set point (the distance between the he-ne and TiS peaks in cavity voltage units)
        /// The lock (see calculateDeviationFromSetPoint) will adjust the voltage fed to the TiS to keep this number constant.
        /// </summary>     

        private double CalculateLaserSetPoint(double[] masterFitCoefficients, double[] slaveFitCoefficients)
        {
            double setPoint = new double();
            if (slaveFitCoefficients[1] > LOWER_CC_VOLTAGE_LIMIT
               && slaveFitCoefficients[1] < UPPER_CC_VOLTAGE_LIMIT) //Only change limits if fits are reasonable.
            {
                setPoint = Math.Round(slaveFitCoefficients[1] - masterFitCoefficients[1], 4);
            }
            else
            {
                setPoint = 0.0;
            }
            return setPoint;

        }

        private double tweakSetPoint(double oldSetPoint)
        {
            double newSetPoint = oldSetPoint + setPointIncrementSize * (Increments - Decrements); 
            Increments = 0;
            Decrements = 0;
            return newSetPoint;
        }

        private double calculateDeviationFromSetPoint(double laserSetPoint,
            double[] masterFitCoefficients, double[] slaveFitCoefficients)
        {
            double currentPeakSeparation = new double();

            if (slaveFitCoefficients[1] > LOWER_CC_VOLTAGE_LIMIT
                && slaveFitCoefficients[1] < UPPER_CC_VOLTAGE_LIMIT) //Only change limits if fits are reasonable.
            {
                currentPeakSeparation = slaveFitCoefficients[1] - masterFitCoefficients[1];
            }
            else
            {
                currentPeakSeparation = LaserSetPoint;
            }

            return currentPeakSeparation - LaserSetPoint;

            

        }

        private double calculateNewVoltageToLaser(double vtolaser, double measuredVoltageChange)
        {
            double newVoltage;
            if (vtolaser
                + Gain * measuredVoltageChange > UPPER_LC_VOLTAGE_LIMIT
                || vtolaser
                + Gain * measuredVoltageChange < LOWER_LC_VOLTAGE_LIMIT)
            {
                newVoltage = vtolaser;
            }
            else
            {
                newVoltage = vtolaser + Gain * measuredVoltageChange; //Feedback 
            }
            return Math.Round(newVoltage, 4);
        }

        #endregion

        

    }




}
