using System;
using System.Threading;
using NationalInstruments.DAQmx;
using DAQ.TransferCavityLock2012;
using DAQ.Environment;
using DAQ.HAL;
using DAQ;
using Data;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

namespace TransferCavityLock2023
{
    /// <summary>
    /// A class for locking the laser using a transfer cavity.
    /// The controller reads the voltage fed to the piezo of the cavity, the voltage from a photodiode measuring the light from a master laser (He-Ne)
    /// and a further n photodiode signals from n lasers you're trying to lock.
    /// The controller then fits a lorenzian to each of these datasets, and works out what to do with each slave laser to keep the peak distances the same.
    /// 
    ///In the hardware class, you need to make a TCLConfig and populate all the fields of that TCLConfig.
    /// </summary>
    public class Controller : MarshalByRefObject
    {
        public int defaultScanPoints;
        public int numScanAverages = 50;

        private List<double> scanTimes;

        private MainForm ui;

        private JSONSerializer serializer;

        [Serializable]
        private struct DataStore
        {
            public double scanPoints;
            public double gain;
        }

        public TCLConfig config;

        private Mutex dataMutex;
        private object acquiredData;

        private Mutex controlMutex;
        private bool cleanup;

        public Dictionary<string, Cavity> Cavities; // Stores all the laser classes.
        private Dictionary<string, int> aiChannelsLookup; // All ai channels, including the cavity and the master.
        private Dictionary<string, int> diChannelsLookup; // All di channels for blocking lock for particular laser
        private ScanParameters scanParameters; // All parameters used by a scan
        TransferCavity2012Lockable tcl;

        public Controller(string configName)
        {
            config = (TCLConfig)Environs.Hardware.GetInfo(configName);
        }

        public enum ControllerState
        {
            STOPPED, RUNNING
        };

        public ControllerState TCLState = ControllerState.STOPPED;
        public object rampStopLock = new object();
        public object tweakLock = new object();

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        public void Start()
        {
            initializeCavityControl();

            initializeAIs();

            defaultScanPoints = config.DefaultScanPoints;

            initializeScanParameters();

            TCLState = ControllerState.STOPPED;

            ui = new MainForm(config.Name);
            ui.controller = this;
            Application.Run(ui);
        }

        private void initializeAIs()
        {
            Dictionary<string, string> analogs = new Dictionary<string, string>();
            Dictionary<string, string> digitals = new Dictionary<string, string>();

            analogs.Add("baseRamp", config.BaseRamp);
            foreach (Cavity cavity in Cavities.Values)
            {
                analogs.Add(getUniqueKey(cavity.Name, cavity.Master.FeedbackChannel), cavity.Master.PhotoDiodeChannel);
                foreach (SlaveLaser slaveLaser in cavity.SlaveLasers.Values)
                {
                    analogs.Add(getUniqueKey(cavity.Name, slaveLaser.FeedbackChannel), slaveLaser.PhotoDiodeChannel);
                    if (slaveLaser.BlockChannel != null)
                    {
                        digitals.Add(getUniqueKey(cavity.Name, slaveLaser.FeedbackChannel), slaveLaser.BlockChannel);
                    }
                }
            }

            List<string> analogChannelsToRead = new List<string>();
            List<string> digitalChannelsToRead = new List<string>();
            aiChannelsLookup = new Dictionary<string, int>();
            diChannelsLookup = new Dictionary<string, int>();
            foreach (string s in analogs.Keys)
            {
                aiChannelsLookup.Add(s, aiChannelsLookup.Count);
                analogChannelsToRead.Add(analogs[s]);
            }
            foreach (string s in digitals.Keys)
            {
                diChannelsLookup.Add(s, diChannelsLookup.Count);
                digitalChannelsToRead.Add(digitals[s]);
            }
            tcl = new DAQMxTCL2012ExtTriggeredMultiReadHelper(analogChannelsToRead.ToArray(), digitalChannelsToRead.ToArray(), config.Trigger);
        }

        private void initializeCavityControl()
        {
            Cavities = new Dictionary<string, Cavity>();

            foreach (KeyValuePair<string, TCLSingleCavityConfig> entry in config.Cavities)
            {
                Cavity cavity = new Cavity(entry.Value);
                cavity.Controller = this;
                Cavities.Add(entry.Key, cavity);
            }
        }

        private void initializeScanParameters()
        {
            scanParameters = new ScanParameters();
            scanParameters.Steps = defaultScanPoints;
            scanParameters.Channels = aiChannelsLookup;
            scanParameters.AnalogSampleRate = config.AnalogSampleRate;
            scanParameters.TriggerOnRisingEdge = config.TriggerOnRisingEdge;
        }

        internal void initializeUI()
        {
            ui.SetNumberOfPoints(defaultScanPoints);
            foreach (Cavity cavity in Cavities.Values)
            {
                ui.AddCavity(cavity);
                ui.SetSummedVoltageTextBox(cavity.Name, 0);
            }
            
            ui.UpdateUIState(TCLState);
        }

        public void StartLogger()
        {
            serializer = new JSONSerializer();
            serializer.StartLogFile(Environs.FileSystem.Paths["transferCavityData"] + "log.json");
            serializer.StartProcessingData();
        }

        public void StopLogger()
        {
            serializer.CompleteAdding();
        }

        public void StartTCL()
        {
            TCLState = Controller.ControllerState.RUNNING;
            Thread.Sleep(2000);
            Thread mainThread = new Thread(new ThreadStart(mainLoop));
            ui.UpdateUIState(TCLState);

            mainThread.Start();
        }

        public void NumberScanPointsChanged(int number)
        {
            scanParameters.Steps = number;
        }

        public void StopTCL()
        {
            TCLState = ControllerState.STOPPED;
            ui.UpdateUIState(TCLState);
        }

        public void ShowDialog(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        #region Passing Events from UIs to the correct slave laser class.

        private void changeLaserVoltage(Laser laser, double voltage)
        {
            try
            {
                laser.CurrentVoltage = voltage;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                double value = (double)ex.ActualValue;
                // If its below lower voltage limit, set to lower limit other wise must be above so set to upper limit
                laser.CurrentVoltage = value < laser.LowerVoltageLimit ? laser.LowerVoltageLimit : laser.UpperVoltageLimit;
            }
        }

        internal void VoltageToMasterLaserChanged(string cavityName, double voltage)
        {
            changeLaserVoltage(Cavities[cavityName].Master, voltage);
        }

        internal void VoltageToSlaveLaserChanged(string cavityName, string slaveName, double voltage)
        {
            changeLaserVoltage(Cavities[cavityName].SlaveLasers[slaveName], voltage);
        }

        internal void MasterGainChanged(string cavityName, double g)
        {
            Cavities[cavityName].Master.Gain = g;
        }

        internal void SlaveGainChanged(string cavityName, string slaveName, double g)
        {
            Cavities[cavityName].SlaveLasers[slaveName].Gain = g;
        }

        internal void MasterSetPointChanged(string cavityName, double setPoint)
        {
            Cavities[cavityName].Master.LaserSetPoint = setPoint;
        }

        internal void UpdateSetPointInGUI(string cavityName, string slaveName, double setpoint)
        {
            ui.SetLaserSetPoint(cavityName, slaveName, setpoint);
        }

        internal void AdjustSetPoint(string cavityName, string slaveName, double adjustment)
        {
            double previousSetPoint = Cavities[cavityName].SlaveLasers[slaveName].LaserSetPoint;
            Cavities[cavityName].SlaveLasers[slaveName].LaserSetPoint = previousSetPoint + adjustment;
        }

        public void EngageMasterLock(string cavityName)
        {
            MasterLaser laser = Cavities[cavityName].Master;
            laser.ArmLock();
            ui.UpdateMasterUIState(cavityName, laser.lState);
        }

        public void DisengageMasterLock(string cavityName)
        {
            MasterLaser laser = Cavities[cavityName].Master;
            laser.DisengageLock();
            ui.UpdateMasterUIState(cavityName, laser.lState);
        }

        public void EngageLock(string cavityName, string slaveName)
        {
            SlaveLaser laser = Cavities[cavityName].SlaveLasers[slaveName];
            laser.ArmLock();
            ui.UpdateSlaveUIState(cavityName, slaveName, laser.lState);
            ui.ClearErrorGraph(cavityName, slaveName);
        }

        public void DisengageLock(string cavityName, string slaveName)
        {
            SlaveLaser laser = Cavities[cavityName].SlaveLasers[slaveName];
            laser.DisengageLock();
            ui.UpdateSlaveUIState(cavityName, slaveName, laser.lState);
        }

        #endregion

        #region Remote methods

        public void SetLaserSetpoint(string cavityName, string laserName, double newSetpoint)
        {
            Cavities[cavityName].SlaveLasers[laserName].LaserSetPoint = newSetpoint;
        }

        public void SetLaserOutputVoltage(string cavityName, string laserName, double newVoltage)
        {
            Cavities[cavityName].SlaveLasers[laserName].CurrentVoltage = newVoltage;
        }

        public void UnlockLaser(string cavityName, string laserName)
        {
            DisengageLock(cavityName, laserName);
        }

        public void LockLaser(string cavityName, string laserName)
        {
            EngageLock(cavityName, laserName);
        }

        public double GetLaserSetpoint(string cavityName, string laserName)
        {
            return Cavities[cavityName].SlaveLasers[laserName].LaserSetPoint;
        }

        public double GetLaserVoltage(string cavityName, string laserName)
        {
            return Cavities[cavityName].SlaveLasers[laserName].CurrentVoltage;
        }

        public void RefreshVoltageOnUI(string cavityName, string laserName)
        {
            ui.SetLaserVoltageTextBox(cavityName, laserName, Cavities[cavityName].SlaveLasers[laserName].CurrentVoltage);
        }

        #region Legacy
        // These methods are to maintain backwards compatability with peoples code

        private string getDefaultCavityName()
        {
            if (Cavities.Count == 1)
            {
                return Cavities.First().Key;
            }
            else
            {
                throw new InvalidOperationException("Must specify cavity if more than one cavity!");
            }
        }

        public void SetLaserSetpoint(string laserName, double newSetpoint)
        {
            SetLaserSetpoint(getDefaultCavityName(), laserName, newSetpoint);
        }

        public void SetLaserOutputVoltage(string laserName, double newVoltage)
        {
            SetLaserOutputVoltage(getDefaultCavityName(), laserName, newVoltage);
        }

        public void UnlockLaser(string laserName)
        {
            UnlockLaser(getDefaultCavityName(), laserName);
        }

        public void LockLaser(string laserName)
        {
            LockLaser(getDefaultCavityName(), laserName);
        }

        public double GetLaserSetpoint(string laserName)
        {
            return GetLaserSetpoint(getDefaultCavityName(), laserName);
        }

        
        public double GetLaserVoltage(string laserName)
        {
            return GetLaserVoltage(getDefaultCavityName(), laserName);
        }

        public void RefreshVoltageOnUI(string laserName)
        {
            RefreshVoltageOnUI(getDefaultCavityName(), laserName);
        }
        #endregion
        #endregion

        #region Main Program
        private string getUniqueKey(string cavityName, string laserName)
        {
            // Create a unique name for use in dictionaries
            // Its not bullet proof but I think it will work unless you are trying to break it... 
            // Point of abstracting method is that this can be changed without breaking everything should the need arise
            return cavityName + "_" + laserName;
        }

        private void initialiseAIHardware(ScanParameters sp)
        {
            tcl.ConfigureHardware(sp.Steps, sp.AnalogSampleRate, sp.TriggerOnRisingEdge, false);
        }

        private void acquireData(ScanParameters sp)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            while (true)
            {
                dataMutex.WaitOne();
                acquiredData = tcl.Read(sp.Steps); // this step cost most of the time
                dataMutex.ReleaseMutex();
                controlMutex.WaitOne();
                if (cleanup) break;
                controlMutex.ReleaseMutex();
            }
            controlMutex.ReleaseMutex();
        }

        private List<Laser> AllLasers
        {
            // List all lasers controlled by this instance
            get
            {
                List<Laser> lasers = new List<Laser>();
                foreach (Cavity cavity in Cavities.Values)
                {
                    lasers.AddRange(cavity.GetAllLasers());
                }
                return lasers;
            }
        }

        private void updateRampPlot(double[] data)
        {
            double[] indices = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                indices[i] = i;
            }
            ui.DisplayRampData(indices, data);
        }

        private void updateGUIForLaser(Laser laser, double[] rampData)
        {
            if (laser is MasterLaser)
            {
                updateGUIForMasterLaser((MasterLaser)laser, rampData);
            }
            else
            {
                updateGUIForSlaveLaser((SlaveLaser)laser, rampData);
            }
        }

        private void updateGUIForMasterLaser(MasterLaser laser, double[] rampData)
        {
            double[] laserScanData = laser.LatestScanData;
            if (laser.peakLocation != null)
            {
                double peakLocation = (double) laser.peakLocation;
                ui.DisplayMasterData(laser.ParentCavity.Name, rampData, laserScanData, peakLocation);
                if (laser.IsLocked)
                {
                    double summedVoltage = laser.CurrentVoltage;
                    ui.SetSummedVoltageTextBox(laser.ParentCavity.Name, summedVoltage);
                    ui.SetVoltageIntoCavityTextBox(laser.ParentCavity.Name, summedVoltage + peakLocation);
                }
            }
            else
            {
                ui.DisplayMasterData(laser.ParentCavity.Name, rampData, laserScanData);
            }
        }

        private void updateGUIForSlaveLaser(SlaveLaser laser, double[] rampData)
        {
            double[] laserScanData = laser.LatestScanData;
            if (laser.peakLocation != null)
            {
                double peakLocation = (double)laser.peakLocation;
                ui.DisplaySlaveData(laser.ParentCavity.Name, laser.Name, rampData, laserScanData, peakLocation);
                if (laser.LockCount > 0)
                {
                    ui.SetLaserVoltageTextBox(laser.ParentCavity.Name, laser.Name, laser.CurrentVoltage);
                    bool laserInNormalOperatingRange = laser.CurrentVoltage < laser.UpperVoltageLimit && laser.CurrentVoltage > laser.LowerVoltageLimit;
                    bool laserIsLocked = laser.lState == Laser.LaserState.LOCKED;
                    ui.SetLaserOperatingLED(laser.ParentCavity.Name, laser.Name, laserIsLocked, laserInNormalOperatingRange);
                    List<double> errorList = laser.OldFrequencyErrors;
                    double standardDev = Math.Sqrt(errorList.Average(x => x * x));
                    ui.SetLaserSDTextBox(laser.ParentCavity.Name, laser.Name, standardDev);
                    ui.AppendToErrorGraph(laser.ParentCavity.Name, laser.Name, laser.LockCount, laser.FrequencyError);
                }
            }
            else
            {
                ui.DisplaySlaveData(laser.ParentCavity.Name, laser.Name, rampData, laserScanData);
            }
        }

        private void logLaserParams(Laser laser) 
        {
            // Trying to be consistent with old format here
            if (laser is SlaveLaser)
            {
                double slaveCentre = laser.peakLocation != null ? (double) laser.peakLocation : 0;
                double masterCentre = laser.ParentCavity.Master.peakLocation != null ? (double) laser.ParentCavity.Master.peakLocation : 0;

                if (ui.logCheckBox.Checked && serializer != null)
                {
                    serializer.AddData(new TCLDataLog(DateTime.Now,
                    laser.Name,
                    masterCentre,
                    slaveCentre,
                    laser.LaserSetPoint,
                    laser.VoltageError,
                    laser.Gain,
                    laser.CurrentVoltage));
                }
            }
        }

        private double updateLockRate(Stopwatch stopWatch)
        {
            double elapsedTime = stopWatch.Elapsed.TotalSeconds;
            scanTimes.Add(elapsedTime);
            if (scanTimes.Count > numScanAverages)
            {
                scanTimes.RemoveAt(0);
            }
            double averageScanTime = scanTimes.Sum() / scanTimes.Count;
            double averageUpdateRate = 1 / averageScanTime;
            return averageUpdateRate;
        }

        private void endLoop()
        {
            tcl.DisposeReadTask();
            foreach (Laser laser in AllLasers)
            {
                if (laser.lState != Laser.LaserState.FREE)
                {
                    laser.DisengageLock();
                    laser.DisposeLaserControl();
                }
            }
        }

        private void mainLoop()
        {
            dataMutex = new Mutex();
            acquiredData = false;

            controlMutex = new Mutex();
            cleanup = false;

            initialiseAIHardware(scanParameters);
            ScanData scanData = new ScanData(aiChannelsLookup, diChannelsLookup);
            scanTimes = new List<double>();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            int loopCount = 0;

            controlMutex.WaitOne();
            Thread DAQThread = new Thread(new ThreadStart(() => acquireData(scanParameters)));
            DAQThread.Start();
            while (TCLState != ControllerState.STOPPED)
            {
                // Read data
                controlMutex.ReleaseMutex();
                dataMutex.WaitOne(); // 0.01s are costed by waiting here
                
                if (!(acquiredData is TCLReadData))
                {
                    dataMutex.ReleaseMutex();
                    controlMutex.WaitOne();
                    continue;
                }

                controlMutex.WaitOne();
                TCLReadData rawData = (TCLReadData)acquiredData;
                dataMutex.ReleaseMutex();

                bool updateGUI = !ui.dissableGUIupdateCheckBox.Checked;
                scanData.AddNewScan(rawData, ui.scanAvCheckBox.Checked, numScanAverages);
                
                // Fitting
                double[] rampData = scanData.GetRampData();
                foreach (Laser laser in AllLasers)
                {
                    double[] laserScanData = scanData.GetLaserData(getUniqueKey(laser.ParentCavity.Name, laser.FeedbackChannel));
                    bool lockBlocked = scanData.LaserLockBlocked(getUniqueKey(laser.ParentCavity.Name, laser.FeedbackChannel));
                    laser.UpdateScan(rampData, laserScanData, lockBlocked);
                }

                // Locking
                foreach (Laser laser in AllLasers)
                {
                    laser.UpdateLock();
                }

                // GUI updates and logging
                if (updateGUI)
                {
                    updateRampPlot(rampData);
                }
                foreach (Laser laser in AllLasers)
                {
                    if (updateGUI)
                    {
                        updateGUIForLaser(laser, rampData);
                    }
                    if (updateGUI)
                    {
                        logLaserParams(laser);
                    }
                }

                double averageUpdateRate = updateLockRate(stopWatch);
                if (updateGUI || loopCount % 100 == 0)
                {
                    ui.UpdateLockRate(averageUpdateRate);
                }

                stopWatch.Reset();
                stopWatch.Start();
                loopCount++;
            }
            cleanup = true;
            controlMutex.ReleaseMutex();
            DAQThread.Join();
            endLoop();
        }
        #endregion

        public void LoadParametersWithDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "tlc parameters|*.bin";
            dialog.Title = "Load parameters";
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String dataStoreDir = settingsPath + "TransferCavityLock\\" + config.Name;
            dialog.InitialDirectory = dataStoreDir;
            dialog.ShowDialog();
            DialogResult result;
            if (dialog.FileName != "")
            {
                result = MessageBox.Show("This will unlock all lasers. Do you want to continue?", "Careful now ...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == System.Windows.Forms.DialogResult.Yes)
                    loadParameters(dialog.FileName);
            }
        }

        private void loadParameters(String dataStoreFilePath)
        {
            // deserialize
            BinaryFormatter s = new BinaryFormatter();
            // eat any errors in the following, as it's just a convenience function
            try
            {
                DataStore dataStore = (DataStore)s.Deserialize(new FileStream(dataStoreFilePath, FileMode.Open));

                // copy parameters out of the struct
                //CPlusVoltage = dataStore.cPlus;
            }
            catch (Exception)
            { 
                Console.Out.WriteLine("Unable to load settings"); 
            }
        }
    }

}
