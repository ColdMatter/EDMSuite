using System;
using System.Threading;
using NationalInstruments.DAQmx;
using DAQ.TransferCavityLock2012;
using DAQ.Environment;
using DAQ.HAL;
using DAQ;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TransferCavityLock2012
{
    /// <summary>
    /// A class for locking the laser using a transfer cavity.
    /// </summary>
    public class Controller : MarshalByRefObject
    {

        #region Declarations

        private const double UPPER_LC_VOLTAGE_LIMIT = 10.0; //volts LC: Laser control
        private const double LOWER_LC_VOLTAGE_LIMIT = -10.0; //volts LC: Laser control

        private const double TWEAK_GAIN = 0.01;
        public int Increments = 0;          // for tweaking the laser set point
        public int Decrements = 0;
        public double setPointIncrementSize = TWEAK_GAIN;

        public const int default_ScanPoints = 200;

        public const double default_Gain = 0.5;
        public const double default_VoltageToLaser = 0.0;



        private MainForm ui;

        //private TransferCavityLockable tcl = 
        //   (TransferCavityLockable)Activator.GetObject(typeof(TransferCavityLockable), 
        //    "tcp://localhost:1172/controller.rem");
        TransferCavity2012Lockable tcl = new DAQMxTCL2012HelperExtTriggeredMultiRead("laser","p2","p1","cavityVoltageRead","analogTrigger2");
        
        public enum ControllerState
        {
            STOPPED, FREERUNNING, LASERLOCKING, LASERLOCKED
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
        public void UnlockCavity()
        {
            State = ControllerState.FREERUNNING;
            ui.updateUIState(State);
        }

        private int numberOfPoints;

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
            ui.SetLaserVoltage(default_VoltageToLaser);
            ui.SetGain(default_Gain);
            ui.SetNumberOfPoints(default_ScanPoints);
            ui.SetSetPointIncrementSize(setPointIncrementSize);
        }
        private void initializeControllerValues()
        {
            voltageToLaser = default_VoltageToLaser;
            gain = default_Gain;
        }
        /// <summary>
        /// Reads some analogue inputs and spits out an array of doubles.
        /// The basic unit of the program.
        /// </summary>
        private CavityScanData scan(ScanParameters sp)
        {
            CavityScanData scanData = new CavityScanData(sp.Steps, 3);

            scanData.parameters = sp;

            scanData.AIData = tcl.ReadAI(sp.Steps);

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

            initializeHardware(sp);

            CavityScanData scanData = scan(sp);

            Dictionary<string, double[]> fits;
           
            
            while (State != ControllerState.STOPPED)
            {
                if (scanData != null)
                {
                    displayData(sp, scanData);

                    switch (State)
                    {
                        case ControllerState.FREERUNNING:
                            //releaseLaser();
                            setToLaserEngaged();
                            break;

                        case ControllerState.LASERLOCKING:

                            fits = fitAndDisplay(scanData);

                            LaserSetPoint = CalculateLaserSetPoint(fits["masterFits"], fits["slaveFits"]);

                            //System.Diagnostics.Debug.Write("width=" + slaveDataFit[0].ToString() + ", centre =" + slaveDataFit[1].ToString()
                            //    + ", amp=" + slaveDataFit[2].ToString() + ", offset=" + slaveDataFit[3].ToString());

                            State = ControllerState.LASERLOCKED;
                            ui.updateUIState(State);
                            break;

                        case ControllerState.LASERLOCKED:

                            fits = fitAndDisplay(scanData);

                            LaserSetPoint = tweakSetPoint(LaserSetPoint); //does nothing if not tweaked

                            double shift = calculateDeviationFromSetPoint(LaserSetPoint, fits["masterFits"], fits["slaveFits"]);
                            VoltageToLaser = calculateNewVoltageToLaser(VoltageToLaser, shift);
                            foreach (double d in fits["masterFits"])
                            {
                                System.Diagnostics.Debug.Write(d);
                            }
                            //System.Diagnostics.Debug.Write(fits["slaveFits"]);
                            break;

                    }
                    if (lState == LaserState.BUSY)
                    {
                        tcl.SetLaserVoltage(VoltageToLaser);
                    }
                }
                scanData = scan(sp);
            }
            finalizeRamping();
            releaseLaser();
        }

        private Dictionary<string, double[]> fitAndDisplay(CavityScanData scanData)
        {
            Dictionary<string, double[]> fits = new Dictionary<string, double[]>();

            double[] masterDataFit = CavityScanFitHelper.FitLorenzianToMasterData(scanData);
            double[] slaveDataFit = CavityScanFitHelper.FitLorenzianToSlaveData(scanData);

            double[] masterFitPoints = createFitData(scanData.CavityData, masterDataFit);
            double[] slaveFitPoints = createFitData(scanData.CavityData, slaveDataFit);

            ui.DisplayFitData(scanData.CavityData, masterFitPoints, slaveFitPoints);

            fits.Add("masterFits", masterDataFit);
            fits.Add("slaveFits", slaveDataFit);
            return fits;
        }

        private void displayData(ScanParameters sp, CavityScanData data)
        {
            double[] indeces = new double[data.CavityData.Length];
            for (int i = 0; i < indeces.Length; i++)
            {
                indeces[i] = i;
            }
            ui.DisplayData(indeces, data.MasterPhotodiodeData, data.SlavePhotodiodeData, data.CavityData);

        }

        private double[] createFitData(double[] cavityVoltages, double[] fitCoefficients)
        {
            double[] fitPoints = new double[cavityVoltages.Length];
            double n = fitCoefficients[3];
            double q = fitCoefficients[2];
            double c = fitCoefficients[1];
            double w = fitCoefficients[0];
            for (int i = 0; i < cavityVoltages.Length; i++)
            {
                if (w == 0) w = 0.001; // watch out for divide by zero
                fitPoints[i] = n + q * (1 / (1 + (((cavityVoltages[i] - c) * (cavityVoltages[i] - c)) / ((w / 2) * (w / 2)))));
            }
            return fitPoints;
        }

        /// <summary>
        /// Gets some parameters from the UI and stores them on the controller.
        /// </summary>
        private void readParametersFromUI()
        {
            // read out UI params
            numberOfPoints = ui.GetNumberOfPoints();
        }
        

        private ScanParameters createInitialScanParameters()
        {
            ScanParameters sp = new ScanParameters();
            sp.Steps = numberOfPoints;
            return sp;
        }

        private void finalizeRamping()
        {
            tcl.DisposeAITask();
            if (lState == LaserState.BUSY)
            {
                VoltageToLaser = 0.0;
                tcl.SetLaserVoltage(0.0);
                tcl.DisposeLaserTask();
                lState = LaserState.FREE;
            }
        }
        private void releaseLaser()
        {
            if (lState == LaserState.BUSY)
            {
                tcl.DisposeLaserTask();
                lState = LaserState.FREE;
            }
        }
        private void setToLaserEngaged()
        {
            if (lState == LaserState.FREE)
            {
                lState = LaserState.BUSY;
                tcl.ConfigureSetLaserVoltage(VoltageToLaser);
            }
        }
        private void initializeHardware(ScanParameters sp)
        {
            tcl.ConfigureReadAI(sp.Steps, false);

        }

        /// <summary>
        /// Measures the laser set point (the distance between the he-ne and TiS peaks in cavity voltage units)
        /// The lock (see calculateDeviationFromSetPoint) will adjust the voltage fed to the TiS to keep this number constant.
        /// </summary>     

        private double CalculateLaserSetPoint(double[] masterFitCoefficients, double[] slaveFitCoefficients)
        {
            return Math.Round(slaveFitCoefficients[1] - masterFitCoefficients[1], 4);
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
            currentPeakSeparation = slaveFitCoefficients[1] - masterFitCoefficients[1];
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
