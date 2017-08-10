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

namespace TransferCavityLock2012
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

        #region Declarations
        public int defaultScanPoints = 900;
        public double defaultMasterGain = 0;

        private MainForm ui;

        private JSONSerializer serializer;

        public TCLConfig config;
        
        private Dictionary<string, double[]> fits;              //Somewhere to store all the fits
        public Dictionary<string, SlaveLaser> SlaveLasers;      //Stores all the slave laser classes.
        private Dictionary<string, int> aiChannels;             //All ai channels, including the cavity and the master.
        public MasterLaser MasterLaser;
        TransferCavity2012Lockable tcl;

        public Controller()
        {
        }

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

        #endregion

        #region Initialization

        public void Start()
        {
            ui = new MainForm(config.Name);
            ui.controller = this;
    
            Dictionary<string, string> analogs = new Dictionary<string, string>();
            foreach(string key in config.Lasers.Keys)
            {
                analogs.Add(key, config.Lasers[key]);
            }
            analogs.Add("master", config.MasterLaser);
            analogs.Add("cavity", config.Cavity);

            MasterLaser = new MasterLaser();
            initializeSlaveLaserControl(config.Lasers.Keys);
            initializeAIs(analogs.Values);

            defaultScanPoints = config.DefaultScanPoints;
            defaultMasterGain = config.DefaultGains["Master"];
                
            TCLState = ControllerState.STOPPED;
            Application.Run(ui);
        }

        private void initializeSlaveLaserControl(Dictionary<string, string>.KeyCollection lockableLasers)
        {
            SlaveLasers = new Dictionary<string, SlaveLaser>();
           
            foreach (string s in lockableLasers)
            {
                SlaveLasers.Add(s, new SlaveLaser(s));
                SlaveLasers[s].controller = this;
                SlaveLasers[s].Gain = config.DefaultGains[s];
            }
        }

        private void initializeAIs(Dictionary<string,string>.ValueCollection channels)
        {
            
            tcl = new DAQMxTCL2012ExtTriggeredMultiReadHelper(channels.ToArray(), config.Trigger);
            aiChannels = new Dictionary<string, int>();
            foreach (string s in channels)
            {
                aiChannels.Add(s, aiChannels.Count);
            }
        }


        public void InitializeUI()
        {
            ui.SetNumberOfPoints(defaultScanPoints);
            ui.SetMasterGain(defaultMasterGain);
            ui.SetVtoOffsetVoltage(0);
            foreach (KeyValuePair<string, SlaveLaser> laser in SlaveLasers)
            {
                ui.AddSlaveLaser(laser.Value);
            }
            ui.ShowAllTabPanels();
            ui.UpdateUIState(TCLState);
        }

        

        #endregion
        #region LOADING AND SAVING
        [Serializable]
        private struct DataStore
        {
            public double scanPoints;
            public double gain;
        }


        #endregion

        #region START AND STOP

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

        /// <summary>
        /// This will make the program exit the while loop in ramploop.
        /// Note: this will call 
        /// </summary>
        public void StopTCL()
        {
            TCLState = ControllerState.STOPPED;
            ui.UpdateUIState(TCLState); 
        }
        #endregion

        #region Controlling the state of the UI
        /// <summary>
        /// The controller needs to be able to change the UI. Enabling buttons, changing the values in text boxes...
        /// </summary>
        /// <param name="name"></param>
        /// <param name="state"></param>

        public void UpdateUIState(string name, SlaveLaser.LaserState state)
        {
            ui.UpdateUIState(name, state);
        }
        public void RefreshLockParametersOnUI(string name)
        {
            ui.SetLaserVoltage(name, SlaveLasers[name].VoltageToLaser);
            ui.SetGain(name, SlaveLasers[name].Gain);
            ui.SetLaserSetPoint(name, SlaveLasers[name].LaserSetPoint);
        }
        public void RefreshErrorParametersOnUI(string name)
        {
            double oldSD = ui.GetLaserSD(name);
            double newSD = Math.Pow(((Math.Pow(oldSD, 2) * 49 + Math.Pow(1500 * (fits[name + "Fits"][1] - fits["masterFits"][1] - SlaveLasers[name].LaserSetPoint) / config.FSRCalibrations[name], 2)) / 50), 0.5);
            ui.SetLaserSD(name, newSD);
        }


        #endregion

        #region Passing Events from UIs to the correct slave laser class.
        /// <summary>
        /// These are events triggered by one of the LockControlPanels. 
        /// Example: User changes the voltage to be sent to the slave laser.
        /// The UI will call VoltageToLaserChanged. Each LockControlPanel knows which laser it corresponds to. so the "name" argument tells
        /// the controller which SlaveLaser class it needs to send the command to!
        /// </summary>
        /// <param name="name"></param>
        /// <param name="voltage"></param>
        internal void VoltageToLaserChanged(string name, double voltage)
        {
            SlaveLasers[name].VoltageToLaser = voltage;
            SlaveLasers[name].SetLaserVoltage();
        }
        internal void GainChanged(string name, double g)
        {
            SlaveLasers[name].Gain = g;
        }
        internal void SetPointIncrementSize(string name, double value)
        {
            SlaveLasers[name].SetPointIncrementSize = value;
        }
        internal void AddSetPointIncrement(string name)
        {
            SlaveLasers[name].AddSetPointIncrement();
        }
        internal void AddSetPointDecrement(string name)
        {
            SlaveLasers[name].AddSetPointDecrement();
        }
        public void EngageLock(string name)
        {
            SlaveLasers[name].ArmLock();
            ui.UpdateUIState(TCLState);
        }
        public void DisengageLock(string name)
        {
            SlaveLasers[name].DisengageLock();
            ui.UpdateUIState(TCLState);
        }
        #endregion


        #region Remote methods

        public void SetLaserSetpoint(string laserName, double newSetpoint)
        {
            SlaveLasers[laserName].LaserSetPoint = newSetpoint;
        }

        public void SetLaserOutputVoltage(string laserName, double newVoltage)
        {
            SlaveLasers[laserName].VoltageToLaser = newVoltage;
            SlaveLasers[laserName].SetLaserVoltage(newVoltage); 
            SlaveLasers[laserName].SetLaserVoltage();
        }

        public void UnlockLaser(string laserName)
        {
            SlaveLasers[laserName].DisengageLock();
        }

        public void LockLaser(string laserName)
        {
            SlaveLasers[laserName].Lock();
        }

        public double GetLaserSetpoint(string laserName)
        {
            return SlaveLasers[laserName].LaserSetPoint;
        }

        public double GetLaserVoltage(string laserName)
        {
            return SlaveLasers[laserName].VoltageToLaser; 
        }

        public void RefreshVoltageOnUI(string name)
        {
            ui.SetLaserVoltage(name, SlaveLasers[name].VoltageToLaser);
        }

        #endregion

        #region SCAN! (MAIN PART OF PROGRAM. If reading through code, start here)

        /// <summary>
        /// Reads some analogue inputs.
        /// The basic unit of the program.
        /// </summary>
        private CavityScanData acquireAI(ScanParameters sp)
        {
            CavityScanData scanData = new CavityScanData(sp.Steps, aiChannels, config.Lasers, config.Cavity, config.MasterLaser); //How many channels we expect. one pd for each slave, the He-Ne and the cavity voltage.
            scanData.AIData = tcl.ReadAI(sp.Steps);
            return scanData;
        }


        /// <summary>
        /// The main loop. Reads the analog inputs, fits to the data and (when locked) adjusts the slave laser voltage.
        /// </summary>
        private void mainLoop()
        {
            fits = new Dictionary<string, double[]>(); //Somewhere to store the fits for an iteration.
            readParametersFromUI();                             //This isn't part of the loop. Do an initial setup of the parameters.
            ScanParameters sp = createInitialScanParameters();
            double masterVoltage = 0;
            setupMasterVoltageOut();
            writeMasterVoltageOut(0);
            disposeMasterVoltageOut();
            DateTime previousTime = DateTime.Now;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            initializeAIHardware(sp);

            CavityScanData scanData = new CavityScanData(sp.Steps, aiChannels, config.Lasers, config.Cavity, config.MasterLaser);
            int count = 0;
          
            while (TCLState != ControllerState.STOPPED)
            {
                updateTime(stopWatch);
                stopWatch.Reset();
                stopWatch.Start();
                double[] lastCav= scanData.GetCavityData();
                
                scanData = acquireAI(sp);
                if (ui.scanAvCheckBox.Checked == true)
                {
                    scanData.SetAverageCavityData(lastCav);
                }
                if (scanData != null)
                {
                    if (!ui.dissableGUIupdateCheckBox.Checked) { plotCavity(scanData); };
                    if ((scanData.GetCavityData())[sp.Steps - 1] < config.MaxInputVoltage) // if the cavity ramp voltage exceeds the input voltage - do nothing
                    {
                        if (checkRampChannel() == true)
                        {
                            switch (MasterLaser.lState)
                            {
                                case MasterLaser.LaserState.FREE:

                                    plotMaster(scanData);
                                    masterVoltage = ui.GetVtoOffsetVoltage();
                                    setupMasterVoltageOut();
                                    writeMasterVoltageOut(masterVoltage);
                                    disposeMasterVoltageOut();
                                    break;

                                case MasterLaser.LaserState.LOCKING:

                                    fits["masterFits"] = fitMaster(scanData);
                                    plotMaster(scanData, fits["masterFits"]);
                                    masterVoltage = calculateNewMasterVoltage(masterVoltage);
                                     //write difference to analog output
                                    setupMasterVoltageOut();
                                    writeMasterVoltageOut(masterVoltage);
                                    ui.SetVtoOffsetVoltage(masterVoltage);
                                    ui.SetMasterFitTextBox(fits["masterFits"][1]-masterVoltage);
                                    disposeMasterVoltageOut();
                                    MasterLaser.Lock();
                                    break;

                                case MasterLaser.LaserState.LOCKED:

                                    if (ui.fastFitCheckBox.Checked) 
                                    { 
                                        fits["masterFits"] = fastfitMaster(scanData, fits["masterFits"]); 
                                    } 
                                    else 
                                    { 
                                        fits["masterFits"] = fitMaster(scanData); 
                                    }
                                    if (!ui.dissableGUIupdateCheckBox.Checked) { plotMaster(scanData, fits["masterFits"]); };
                                    masterVoltage = calculateNewMasterVoltage(masterVoltage);
                                     //write difference to analog output
                                    setupMasterVoltageOut();
                                    writeMasterVoltageOut(masterVoltage);
                                    ui.SetVtoOffsetVoltage(masterVoltage);
                                    ui.SetMasterFitTextBox(fits["masterFits"][1]-masterVoltage);
                                    disposeMasterVoltageOut();
                                    break;
                            }
                        }
                            
                        foreach (KeyValuePair<string, SlaveLaser> pair in SlaveLasers)
                        {
                            string slName = pair.Key;
                            SlaveLaser sl = pair.Value;
                            
                            
                            //Some rearrangements to fit only when log fit slave lasers parameters on and/or lock slave lasers on.
                           
                            switch (sl.lState)
                            {
                                case SlaveLaser.LaserState.FREE:

                                    plotSlaveNoFit(slName, scanData);
                                    //RefreshVoltageOnUI(slName);
                                    break;

                                case SlaveLaser.LaserState.LOCKING:

                                    fits[slName + "Fits"] = fitSlave(slName, scanData);
                                    plotSlave(slName, scanData, fits[slName + "Fits"]);
                                    sl.CalculateLaserSetPoint(fits["masterFits"], fits[slName + "Fits"]);
                                    sl.Lock();
                                    //RefreshVoltageOnUI(slName);
                                    RefreshErrorGraph(slName);
                                    count = 0;
                                    break;


                                case SlaveLaser.LaserState.LOCKED:
                                    if (ui.fastFitCheckBox.Checked)
                                    {
                                        fits[slName + "Fits"] = fastfitSlave(slName, scanData, fits[slName + "Fits"]);
                                    }
                                    else
                                    {
                                        fits[slName + "Fits"] = fitSlave(slName, scanData);
                                    }
                                    if(!ui.dissableGUIupdateCheckBox.Checked){ plotSlave(slName, scanData, fits[slName + "Fits"]);};
                                    if (!ui.dissableGUIupdateCheckBox.Checked) { plotError(slName, new double[] { getErrorCount(slName) }, new double[] { fits[slName + "Fits"][1] - fits["masterFits"][1] - sl.LaserSetPoint }); };                                   
                                    sl.RefreshLock(fits["masterFits"], fits[slName + "Fits"]);
                                    RefreshLockParametersOnUI(sl.Name);
                                    RefreshErrorParametersOnUI(sl.Name);
                                    incrementCounter(slName);
                                    count++;
                                    break;
                            }
                            if (ui.logCheckBox.Checked)
                            {
                                double[] masterFitParams;
                                double[] slaveFitParams;
                                if (!fits.TryGetValue("masterFits", out masterFitParams))
                                {
                                    masterFitParams = new double[4] { 0, 0, 0, 0 };
                                }
                                if(!fits.TryGetValue(slName + "Fits", out slaveFitParams))
                                {
                                    slaveFitParams = new double[4] { 0, 0, 0, 0 };
                                };

                                serializer.AddData(new TCLDataLog(DateTime.Now, slName, masterFitParams[1], slaveFitParams[1], sl.VoltageToLaser));
                            }
                        }

                    }
                    else 
                    { 
                        Console.WriteLine("Cavity ramp voltage out of range");
                        Thread.Sleep(100);
                    }
                    
                }
            }
            endRamping();
        }

        #endregion

        #region Some plotting and fitting stuff

        private double[] fitMaster(CavityScanData data)
        {
            return CavityScanFitHelper.FitLorenzianToData(data.GetCavityData(), data.GetMasterData());
        }

        private double[] fastfitMaster(CavityScanData data, double[] masterFitParams)
        {
            return CavityScanFitHelper.FitLorenzianToData(data.GetCavityData(), data.GetMasterData(), masterFitParams, config.PointsToConsiderEitherSideOfPeakInFWHMs, config.MaximumNLMFSteps);
        }

        private void plotMaster(CavityScanData data, double[] MasterFit)
        {
            double[] cavity = data.GetCavityData();
            double[] master = data.GetMasterData();
            ui.DisplayMasterData(cavity, master, CavityScanFitHelper.CreatePointsFromFit(cavity, MasterFit));
        }
        private void plotMaster(CavityScanData data)
        {
            double[] cavity = data.GetCavityData();
            double[] master = data.GetMasterData();
            ui.DisplayMasterData(cavity, master);
        }

        private void plotCavity(CavityScanData data)
        {
            double[] indeces = new double[data.GetCavityData().Length];
            for (int i = 0; i < indeces.Length; i++)
            {
                indeces[i] = i;
            }
            ui.DisplayCavityData(indeces, data.GetCavityData());
        }

        private double[] fitSlave(string name, CavityScanData data)
        {
            return CavityScanFitHelper.FitLorenzianToData(data.GetCavityData(), data.GetSlaveData(name));
        }

        private double[] fastfitSlave(string name, CavityScanData data, double[] SlaveFitParams)
        {
            return CavityScanFitHelper.FitLorenzianToData(data.GetCavityData(), data.GetSlaveData(name), SlaveFitParams, config.PointsToConsiderEitherSideOfPeakInFWHMs,config.MaximumNLMFSteps);
        }

        private void plotSlave(string name, CavityScanData data, double[] slaveFit)
        {
            double[] cavity = data.GetCavityData();
            double[] slave = data.GetSlaveData(name);
            ui.DisplaySlaveData(name, cavity, slave, CavityScanFitHelper.CreatePointsFromFit(cavity, slaveFit));
        }

        private void plotSlaveNoFit(string name, CavityScanData data)
        {
            double[] cavity = data.GetCavityData();
            double[] slave = data.GetSlaveData(name);
            ui.DisplaySlaveDataNoFit(name, cavity, slave);
        }

        private void updateTime(Stopwatch stopwatch)
        {
            TimeSpan elapsed = stopwatch.Elapsed;
            double freq = 10000000 / elapsed.Ticks;
            double avfreq = ui.GetElapsedTime();
            double newAvFreq = (49 * avfreq + freq) / 50;
            ui.UpdateElapsedTime(newAvFreq);
        }

        private void plotError(string name, double[] time, double[] error )
        {
           ui.DisplayErrorData(name, time, error);
        }

        private void incrementCounter(string name)
        {
            ui.IncrementErrorCount(name);
        }

        private int getErrorCount(string name)
        {
           return ui.GetErrorCount(name);
        }


        private void RefreshErrorGraph(string name)
        {
            ui.ClearErrorGraph(name);
        }
        
        #endregion

        #region privates
        /// <summary>
        /// Gets some parameters from the UI and stores them on the controller. Mostly redundant now that the code has changed a lot.
        /// </summary>
        private int numberOfPoints;
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
        /// <summary>
        /// Functions for locking master laser to cavity length
        /// </summary>
        /// 
        private bool checkRampChannel()
        {   bool xMaster;
            if((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[config.Ramp]!=null)
            xMaster = true;
            else
            xMaster=false;
            return xMaster;
        }
        private double calculateNewMasterVoltage(double masterV)
        {
            double masterSetPoint=double.Parse(ui.MasterSetPointTextBox.Text);
            double masterFit = fits["masterFits"][1];
            double masterGain= double.Parse(ui.MasterGainTextBox.Text);;
            double masterVoltageShift = masterV - masterGain * (masterSetPoint - masterFit);
            return masterVoltageShift;
        }

        
        private Task masterOutputTask;
        private AnalogOutputChannel masterChannel;
        private AnalogSingleChannelWriter masterWriter;
        
        public void setupMasterVoltageOut()
        {
            masterOutputTask = new Task("rampfeedback");
            masterChannel=(AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[config.Ramp];
            masterChannel.AddToTask(masterOutputTask, masterChannel.RangeLow, masterChannel.RangeHigh);
            masterOutputTask.Control(TaskAction.Verify);
            masterWriter = new AnalogSingleChannelWriter(masterOutputTask.Stream);
        }
        public void writeMasterVoltageOut(double voltage)
        {
            masterOutputTask.Start();
            masterWriter.WriteSingleSample(true, voltage);
            masterOutputTask.Stop();
        }
        public void disposeMasterVoltageOut()
        {
            masterOutputTask.Dispose();
        }

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
                    LoadParameters(dialog.FileName);
            }
        }

        private void LoadParameters(String dataStoreFilePath)
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
            { Console.Out.WriteLine("Unable to load settings"); }
        }

        #endregion
        /// <summary>
        /// The function which gets called at the end, after breaking out of the while loop.
        /// </summary>
        private void endRamping()
        {
            tcl.DisposeAITask();
            foreach (KeyValuePair<string, SlaveLaser> pair in SlaveLasers)
            {
                string slName = pair.Key;
                SlaveLaser sl = pair.Value;

                if (sl.lState != SlaveLaser.LaserState.FREE)
                {
                    sl.VoltageToLaser = 0.0;
                    sl.DisengageLock();
                    sl.DisposeLaserControl();
                }
            }
        }
        /// <summary>
        /// Prepares the hardware for analog reads.
        /// </summary>
        /// <param name="sp"></param>
        private void initializeAIHardware(ScanParameters sp)
        {
            tcl.ConfigureReadAI(sp.Steps, config.AnalogSampleRate, config.TriggerOnRisingEdge, false);
        }

        
    }

}
