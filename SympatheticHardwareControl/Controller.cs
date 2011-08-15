using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Runtime.Remoting.Lifetime;
using System.Windows.Forms;
using System.Drawing;

using NationalInstruments;
using NationalInstruments.DAQmx;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using NationalInstruments.VisaNS;

using DAQ.HAL;
using DAQ.Environment;

using NationalInstruments.Vision;
using NationalInstruments.Vision.Acquisition.Imaqdx;
using NationalInstruments.Vision.Internal;
using NationalInstruments.Vision.WindowsForms.Internal;

using SympatheticHardwareControl.CameraControl;

namespace SympatheticHardwareControl
{
    /// <summary>
    /// This is the interface to the sympathetic specific hardware.
    /// 
    /// Flow chart (under normal conditions): UI -> controller -> hardware. Basically, that's it.
    /// Exceptions: 
    /// -controller can set values displayed on UI (only meant for special cases, like startup or re-loading a saved parameter set)
    /// -Some public functions allow the HC to be controlled remotely (see below).
    /// 
    /// HardwareState:
    /// There are 3 states which the controller can be in: OFF, LOCAL and REMOTE.
    /// OFF just means the HC is idle.
    /// The state is set to LOCAL when this program is actively upating the state of the hardware. It does this by
    /// reading off what's on the UI, finding any discrepancies between the current state of the hardware and the values on the UI
    /// and by updating the hardware accordingly.
    /// After finishing with the update, it resets the state to OFF.
    /// When the state is set to REMOTE, the UI is disactivated. The hardware controller saves all the parameter values upon switching from
    /// LOCAL to REMOTE, then does nothing. When switching back, it reinstates the hardware state to what it was before it switched to REMOTE.
    /// Use this when you want to control the hardware from somewhere else (e.g. MOTMaster).
    ///
    /// Remoting functions (SetValue):
    /// Having said that, you'll notice that there are also public functions for modifying parameter values without putting the HC in REMOTE.
    /// I wrote it like this because you want to be able to do two different things:
    /// -Have the hardware controller take a back seat and let something else control the hardware for a while (e.g. MOTMaster)
    /// This is when you should use the REMOTE state.
    /// -You still want the HC to keep track of the hardware (hence remaining in LOCAL), but you want to send commands to it remotely 
    /// (say from a console) instead of from the UI. This is when you would use the SetValue functions.
    /// 
    /// The Hardware Report:
    /// The Hardware report is a way of passing a dictionary (gauges, temperature measurements, error signals) to another program
    /// (MotMaster, say). MOTMaster can then save the dictionary along with the data. I hope this will help towards answering questions
    /// like: "what was the source chamber pressure when we took this data?". At the moment, the hardware state is also included in the report.
    /// 
    /// </summary>
    public class Controller : MarshalByRefObject, CameraControllable, HardwareReportable
    {
        #region Constants
        //Put any constants and stuff here

        private static string cameraAttributesPath = (string)Environs.FileSystem.Paths["UntriggeredCameraAttributesPath"];
        private static string profilesPath = (string)Environs.FileSystem.Paths["settingsPath"]
            + "SympatheticHardwareController\\";

        private static Hashtable calibrations = Environs.Hardware.Calibrations;
        #endregion

        #region Setup



        // table of all digital analogTasks
        Hashtable digitalTasks = new Hashtable();

        //Cameras
        public ImageMaster ImageController;
        

        // Declare that there will be a controlWindow
        ControlWindow controlWindow;
        
        HardwareMonitorWindow monitorWindow;

        //private bool sHCUIControl;
        public enum SHCUIControlState { OFF, LOCAL, REMOTE };
        public SHCUIControlState HCState = new SHCUIControlState();



        private class cameraNotFoundException : ArgumentException { };

        
        hardwareState stateRecord;
        private Dictionary<string, Task> analogTasks;

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }


        public void Start()
        {

            // make the digital analogTasks. The function "CreateDigitalTask" is defined later
            //e.g   CreateDigitalTask("notEOnOff");
            //      CreateDigitalTask("eOnOff");

            //This is to keep track of the various things which the HC controls.
            analogTasks = new Dictionary<string, Task>();
            stateRecord = new hardwareState();
            stateRecord.analogs = new Dictionary<string, double>();
            stateRecord.digitals = new Dictionary<string, bool>();
            

            CreateDigitalTask("aom0enable");
            CreateDigitalTask("aom1enable");
            CreateDigitalTask("aom2enable");
            CreateDigitalTask("aom3enable");

            // make the analog output analogTasks. The function "CreateAnalogOutputTask" is defined later
            //e.g.  bBoxAnalogOutputTask = CreateAnalogOutputTask("b");
            //      steppingBBiasAnalogOutputTask = CreateAnalogOutputTask("steppingBBias");

            CreateAnalogOutputTask("aom0amplitude");
            CreateAnalogOutputTask("aom0frequency");
            CreateAnalogOutputTask("aom1amplitude");
            CreateAnalogOutputTask("aom1frequency");
            CreateAnalogOutputTask("aom2amplitude");
            CreateAnalogOutputTask("aom2frequency");
            CreateAnalogOutputTask("aom3amplitude");
            CreateAnalogOutputTask("aom3frequency");
            CreateAnalogOutputTask("coil0current");
            CreateAnalogOutputTask("coil1current");
            
            CreateAnalogInputTask("laserLockErrorSignal", -10, 10);
            CreateAnalogInputTask("chamber1Pressure");

            // make the control controlWindow
            controlWindow = new ControlWindow();
            controlWindow.controller = this;
            

            HCState = SHCUIControlState.OFF;



             Application.Run(controlWindow);

        }

        // this method runs immediately after the GUI sets up
        internal void ControllerLoaded()
        {

            StartCameraControl();
            stateRecord = loadParameters(profilesPath + "StoppedParameters.bin");
            setValuesDisplayedOnUI(stateRecord);
            ApplyRecordedStateToHardware();


        }

        public void ControllerStopping()
        {
            // things like saving parameters, turning things off before quitting the program should go here
            
            StoreParameters(profilesPath + "StoppedParameters.bin");
        }
        public void OpenNewHardwareMonitorWindow()
        {
            monitorWindow = new HardwareMonitorWindow();
            monitorWindow.controller = this;
            monitorWindow.Show();
        }
        #endregion

        #region private methods for creating un-timed Tasks/channels
        // a list of functions for creating various analogTasks
        private void CreateAnalogInputTask(string channel)
        {
            analogTasks[channel] = new Task(channel);
            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                analogTasks[channel],
                0,
                10
            );
            analogTasks[channel].Control(TaskAction.Verify);
        }

        // an overload to specify input range
        private void CreateAnalogInputTask(string channel, double lowRange, double highRange)
        {
            analogTasks[channel] = new Task(channel);
            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                analogTasks[channel],
                lowRange,
                highRange
            );
            analogTasks[channel].Control(TaskAction.Verify);
        }


        private void CreateAnalogOutputTask(string channel)
        {
            stateRecord.analogs[channel] = (double)0.0;
            analogTasks[channel] = new Task(channel);
            AnalogOutputChannel c = ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[channel]);
            c.AddToTask(
                analogTasks[channel],
                c.RangeLow,
                c.RangeHigh
                );
            analogTasks[channel].Control(TaskAction.Verify);
        }

        // setting an analog voltage to an output
        public void SetAnalogOutput(string channel, double voltage)
        {
            SetAnalogOutput(channel, voltage, false);
        }
        //Overload for using a calibration before outputting to hardware
        public void SetAnalogOutput(string channelName, double voltage, bool useCalibration)
        {
            
            AnalogSingleChannelWriter writer = new AnalogSingleChannelWriter(analogTasks[channelName].Stream);
            double output;
            if (useCalibration)
            {
                try
                {
                    output = ((Calibration)calibrations[channelName]).Convert(voltage);
                }
                catch (DAQ.HAL.Calibration.CalibrationRangeException)
                {
                    MessageBox.Show("The number you have typed is out of the calibrated range! \n Try typing something more sensible.");
                    throw new CalibrationException();
                }
                catch
                {
                    MessageBox.Show("Calibration error");
                    throw new CalibrationException();
                }
            }
            else
            {
                output = voltage;
            }
            try
            {
                writer.WriteSingleSample(true, output);
                analogTasks[channelName].Control(TaskAction.Unreserve);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public class CalibrationException : ArgumentOutOfRangeException { };
        // reading an analog voltage from input
        public double ReadAnalogInput(string channel)
        {
            return ReadAnalogInput(channel, false);
        }
        public double ReadAnalogInput(string channelName, bool useCalibration)
        {
            AnalogSingleChannelReader reader = new AnalogSingleChannelReader(analogTasks[channelName].Stream);
            double val = reader.ReadSingleSample();
            analogTasks[channelName].Control(TaskAction.Unreserve);
            if (useCalibration)
            {
                try
                {
                    return ((Calibration)calibrations[channelName]).Convert(val);
                }
                catch
                {
                    MessageBox.Show("Calibration error");
                    return val;
                }
            }
            else
            {
                return val;
            }
        }
        
        // overload for reading multiple samples
        public double ReadAnalogInput(string channel, double sampleRate, int numOfSamples, bool useCalibration)
        {
            //Configure the timing parameters of the task
            analogTasks[channel].Timing.ConfigureSampleClock("", sampleRate,
                SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, numOfSamples);

            //Read in multiple samples
            AnalogSingleChannelReader reader = new AnalogSingleChannelReader(analogTasks[channel].Stream);
            double[] valArray = reader.ReadMultiSample(numOfSamples);
            analogTasks[channel].Control(TaskAction.Unreserve);

            //Calculate the average of the samples
            double sum = 0;
            for (int j = 0; j < numOfSamples; j++)
            {
                sum = sum + valArray[j];
            }
            double val = sum / numOfSamples;
            if (useCalibration)
            {
                try
                {
                    return ((Calibration)calibrations[channel]).Convert(val);
                }
                catch
                {
                    MessageBox.Show("Calibration error");
                    return val;
                }
            }
            else
            {
                return val;
            }
        }


        private void CreateDigitalTask(String name)
        {
            stateRecord.digitals[name] = false;
            Task digitalTask = new Task(name);
            ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[name]).AddToTask(digitalTask);
            digitalTask.Control(TaskAction.Verify);
            digitalTasks.Add(name, digitalTask);
        }

        public void SetDigitalLine(string name, bool value)
        {
            Task digitalTask = ((Task)digitalTasks[name]);
            DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(digitalTask.Stream);
            writer.WriteSingleSampleSingleLine(true, value);
            digitalTask.Control(TaskAction.Unreserve);
        }

        #endregion

        #region keeping track of the state of the hardware!
        /// <summary>
        /// There's this thing I've called a hardware state. It's something which keeps track of digital and analog values.
        /// I then have something called stateRecord (defines above as an instance of hardwareState) which keeps track of 
        /// what the hardware is doing.
        /// Anytime the hardware gets modified by this program, the stateRecord get updated. Don't hack this. 
        /// It's useful to know what the hardware is doing at all times.
        /// When switching to REMOTE, the updates no longer happen. That's why we store the state before switching to REMOTE and apply the state
        /// back again when returning to LOCAL.
        /// </summary>
        [Serializable]
        private struct hardwareState
        {
            public Dictionary<string, double> analogs;
            public Dictionary<string, bool> digitals;
        }
        

        #endregion

        #region Saving and loading experimental parameters
        // Saving the parameters when closing the controller
        public void SaveParametersWithDialog()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "shc parameters|*.bin";
            saveFileDialog1.Title = "Save parameters";
            saveFileDialog1.InitialDirectory = profilesPath;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    StoreParameters(saveFileDialog1.FileName);
                }
            }
        }

        private void StoreParameters(String dataStoreFilePath)
        {
            stateRecord = readValuesOnUI();
            BinaryFormatter s = new BinaryFormatter();
            FileStream fs = new FileStream(dataStoreFilePath, FileMode.Create);
            try
            {
                //s.Serialize(fs, dataStore);
                s.Serialize(fs, stateRecord);
            }
            catch (Exception)
            {
                Console.Out.WriteLine("Saving failed");
            }
            finally
            {
                fs.Close();
                controlWindow.WriteToConsole("Saved parameters to " + dataStoreFilePath);
            }

        }

        //Load parameters when opening the controller
        public void LoadParametersWithDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "shc parameters|*.bin";
            dialog.Title = "Load parameters";
            dialog.InitialDirectory = profilesPath;
            dialog.ShowDialog();
            if (dialog.FileName != "") stateRecord = loadParameters(dialog.FileName);
            setValuesDisplayedOnUI(stateRecord);
        }

        private hardwareState loadParameters(String dataStoreFilePath)
        {
            // deserialize
            BinaryFormatter s = new BinaryFormatter();
            FileStream fs;
            hardwareState state = new hardwareState();
            fs = new FileStream(dataStoreFilePath, FileMode.Open);
            try
            {
                state = (hardwareState)s.Deserialize(fs);
            }
            catch (Exception e)
            { MessageBox.Show(e.Message); }
            finally
            {
                fs.Close();
                controlWindow.WriteToConsole("Loaded parameters from " + dataStoreFilePath);
            }
            return state;
        }
        #endregion

        #region Controlling hardware and UI.
        //This gets/sets the values on the GUI panel

        

        #region Updating the hardware

        public void ApplyRecordedStateToHardware()
        {
            applyToHardware(stateRecord);          
        }


        public void UpdateHardware()
        {
            hardwareState uiState = readValuesOnUI();

            hardwareState changes = getDiscrepancies(stateRecord, uiState);

            applyToHardware(changes);

            updateStateRecord(changes);


        }

        private void applyToHardware(hardwareState state)
        {
            if (state.analogs.Count != 0 || state.digitals.Count != 0)
            {
                if (HCState == SHCUIControlState.OFF)
                {

                    HCState = SHCUIControlState.LOCAL;
                    controlWindow.UpdateUIState(HCState);

                    applyAnalogs(state);
                    applyDigitals(state);

                    HCState = SHCUIControlState.OFF;
                    controlWindow.UpdateUIState(HCState);

                    controlWindow.WriteToConsole("Update finished!");
                }
            }
            else
            {
                controlWindow.WriteToConsole("The values on the UI are identical to those on the controller's records. Hardware must be up to date.");
            }
        }

        private hardwareState getDiscrepancies(hardwareState oldState, hardwareState newState)
        {
            hardwareState state = new hardwareState();
            state.analogs = new Dictionary<string, double>();
            state.digitals = new Dictionary<string, bool>();
            foreach(KeyValuePair<string, double> pairs in oldState.analogs)
            {
                if (oldState.analogs[pairs.Key] != newState.analogs[pairs.Key])
                {
                    state.analogs[pairs.Key] = newState.analogs[pairs.Key];
                }
            }
            foreach (KeyValuePair<string, bool> pairs in oldState.digitals)
            {
                if (oldState.digitals[pairs.Key] != newState.digitals[pairs.Key])
                {
                    state.digitals[pairs.Key] = newState.digitals[pairs.Key];
                }
            }
            return state;
        }

        private void updateStateRecord(hardwareState changes)
        {
            foreach (KeyValuePair<string, double> pairs in changes.analogs)
            {
                stateRecord.analogs[pairs.Key] = changes.analogs[pairs.Key];
            }
            foreach (KeyValuePair<string, bool> pairs in changes.digitals)
            {
                stateRecord.digitals[pairs.Key] = changes.digitals[pairs.Key];
            }
        }

        
        private void applyAnalogs(hardwareState state)
        {
            List<string> toRemove = new List<string>();  //In case of errors, keep track of things to delete from the list of changes.
            foreach (KeyValuePair<string, double> pairs in state.analogs)
            {
                try
                {
                    if (calibrations.ContainsKey(pairs.Key))
                    {
                        SetAnalogOutput(pairs.Key, pairs.Value, true);

                    }
                    else
                    {
                        SetAnalogOutput(pairs.Key, pairs.Value);
                    }
                    controlWindow.WriteToConsole("Set channel '" + pairs.Key.ToString() + "' to " + pairs.Value.ToString());
                }
                catch (CalibrationException)
                {
                    controlWindow.WriteToConsole("Failed to set channel '"+ pairs.Key.ToString() + "' to new value");                    
                    toRemove.Add(pairs.Key);
                }
            }
            foreach (string s in toRemove)  //Remove those from the list of changes, as nothing was done to the Hardware.
            {
                state.analogs.Remove(s);
            }
        }

        private void applyDigitals(hardwareState state)
        {
            foreach (KeyValuePair<string, bool> pairs in state.digitals)
            {
                SetDigitalLine(pairs.Key, pairs.Value);
                controlWindow.WriteToConsole("Set channel '" + pairs.Key.ToString() + "' to " + pairs.Value.ToString());
            }
        }
        #endregion 

        #region Reading and Writing to UI

        private hardwareState readValuesOnUI()
        {
            hardwareState state = new hardwareState();
            state.analogs = readUIAnalogs(stateRecord.analogs.Keys);
            state.digitals = readUIDigitals(stateRecord.digitals.Keys);
            return state;
        }
        private Dictionary<string, double> readUIAnalogs(Dictionary<string, double>.KeyCollection keys)
        {
            Dictionary<string, double> analogs = new Dictionary<string, double>();
            string[] keyArray = new string[keys.Count];
            keys.CopyTo(keyArray, 0);
            for (int i = 0; i < keys.Count; i++)
            {
                analogs[keyArray[i]] = controlWindow.ReadAnalog(keyArray[i]);
            }
            return analogs;
        }
        private Dictionary<string, bool> readUIDigitals(Dictionary<string, bool>.KeyCollection keys)
        {
            Dictionary<string, bool> digitals = new Dictionary<string,bool>();
            string[] keyArray = new string[keys.Count];
            keys.CopyTo(keyArray, 0);
            for (int i = 0; i < keys.Count; i++)
            {
                digitals[keyArray[i]] = controlWindow.ReadDigital(keyArray[i]);
            }
            return digitals;
        }
       

        private void setValuesDisplayedOnUI(hardwareState state)
        {
            setUIAnalogs(state);
            setUIDigitals(state);
        }
        private void setUIAnalogs(hardwareState state)
        {
            foreach (KeyValuePair<string, double> pairs in state.analogs)
            {
                controlWindow.SetAnalog(pairs.Key, (double)pairs.Value);
            }
        }
        private void setUIDigitals(hardwareState state)
        {
            foreach (KeyValuePair<string, bool> pairs in state.digitals)
            {
                controlWindow.SetDigital(pairs.Key, (bool)pairs.Value);
            }
        }

        #endregion

        #region Remoting stuff

        /// <summary>
        /// This is used when you want another program to take control of some/all of the hardware. The hc then just saves the
        /// last hardware state, then prevents you from making any changes to the UI.
        /// </summary>
        public void StartRemoteControl()
        {
            if (HCState == SHCUIControlState.OFF)
            {
                if (!ImageController.IsCameraFree())
                {
                    StopCameraStream();
                }             
                StoreParameters(profilesPath + "tempParameters.bin");
                HCState = SHCUIControlState.REMOTE;
                controlWindow.UpdateUIState(HCState);
                controlWindow.WriteToConsole("Remoting Started!");
            }
            else
            {
                MessageBox.Show("Controller is busy");
            }

        }
        public void StopRemoteControl()
        {
            try
            {
                controlWindow.WriteToConsole("Remoting Stopped!");
                setValuesDisplayedOnUI(loadParameters(profilesPath + "tempParameters.bin"));
                
                if (System.IO.File.Exists(profilesPath + "tempParameters.bin"))
                {
                    System.IO.File.Delete(profilesPath + "tempParameters.bin");
                }
            }
            catch (Exception)
            {
                controlWindow.WriteToConsole("Unable to load Parameters.");
            }
            HCState = SHCUIControlState.OFF;
            controlWindow.UpdateUIState(HCState);
            ApplyRecordedStateToHardware();
        }

        /// <summary>
        /// These SetValue functions are for giving commands to the hc from another program, while keeping the hc in control of hardware.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="value"></param>
        public void SetValue(string channel, double value)
        {
            HCState = SHCUIControlState.LOCAL;
            stateRecord.analogs[channel] = value;
            SetAnalogOutput(channel, value, false);
            setValuesDisplayedOnUI(stateRecord);
            HCState = SHCUIControlState.OFF;

        }
        public void SetValue(string channel, double value, bool useCalibration)
        {

            stateRecord.analogs[channel] = value;
            HCState = SHCUIControlState.LOCAL;
            SetAnalogOutput(channel, value, useCalibration);
            setValuesDisplayedOnUI(stateRecord);
            HCState = SHCUIControlState.OFF;

        }
        public void SetValue(string channel, bool value)
        {
            HCState = SHCUIControlState.LOCAL;
            stateRecord.digitals[channel] = value;
            SetDigitalLine(channel, value);
            setValuesDisplayedOnUI(stateRecord);
            HCState = SHCUIControlState.OFF;

        }
        #endregion

        #endregion

        //camera stuff

        #region Local camera control

        public void StartCameraControl()
        {
            try
            {
                ImageController = new ImageMaster("cam0", cameraAttributesPath);
                ImageController.controller = this;
                ImageController.Initialize();
                PrintCameraAttributesToConsole();
            }
            catch (ImaqdxException e)
            {
                MessageBox.Show(e.Message, "Camera Initialization Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();

            }
        }
        public void CameraStream()
        {
            
            ImageController.SetCameraAttributes(cameraAttributesPath);
            controlWindow.WriteToConsole("Applied camera attributes from " + cameraAttributesPath);
            PrintCameraAttributesToConsole();
            controlWindow.WriteToConsole("Streaming from camera");
            ImageController.Stream();
                   
        }

        public void StopCameraStream()
        {
            ImageController.StopStream();
            
            controlWindow.WriteToConsole("Streaming stopped");
        }

        public void CameraSnapshot()
        {
            controlWindow.WriteToConsole("Taking snapshot");
            ImageController.SetCameraAttributes(cameraAttributesPath);
            controlWindow.WriteToConsole("Applied camera attributes from " + cameraAttributesPath);
            PrintCameraAttributesToConsole();
            try
            {
                ImageController.Snapshot();
            }
            catch (TimeoutException)
            {
              
            }

        }


        /// <summary>
        /// This is cheezy. I'm channelling information from the camera to the console. The controller is the only bit that sees
        /// both.
        /// </summary>
        public void PrintCameraAttributesToConsole()
        {
            controlWindow.WriteToConsole("Attributes loaded in camera:");
            controlWindow.WriteToConsole(ImageController.ImaqdxSession.Attributes.WriteAttributesToString());
            
        }
        #endregion

        #region Saving and Loading Images

        public void SaveImageWithDialog()
        {
            ImageController.SaveImageWithDialog();
            controlWindow.WriteToConsole("Image saved");
        }
        public void LoadImagesWithDialog()
        {
            ImageController.LoadImagesWithDialog();
            controlWindow.WriteToConsole("Image loaded");
        }

        #endregion

        #region Remote Image Processing
        //Written for taking images triggered by TTL. This "Arm" sets the camera so it's expecting a TTL.

        public byte[,] GrabSingleImage(string cameraAttributesPath)
        {
            ImageController.SetCameraAttributes(cameraAttributesPath);
            controlWindow.WriteToConsole("Applied camera attributes from " + cameraAttributesPath);
            PrintCameraAttributesToConsole();
            
            return ImageController.Snapshot();
            
        }
        public byte[][,] GrabMultipleImages(string cameraAttributesPath, int numberOfShots)
        {
            
            ImageController.SetCameraAttributes(cameraAttributesPath);
            controlWindow.WriteToConsole("Applied camera attributes from " + cameraAttributesPath);
            PrintCameraAttributesToConsole();
            
            try
            {
                byte[][,] images = ImageController.TriggeredSequence(numberOfShots);

                return images;
            }

            catch (TimeoutException)
            {
                FinishRemoteCameraControl();
                return null;
            }
            
        }
        public void PrintIntervalInConsole(long interval)
        {
            controlWindow.WriteToConsole("Duration of acquisition: " + interval.ToString());
        }

        public bool IsReadyForAcquisition()
        {
            return ImageController.IsReadyForAcqisition();
        }

        public void PrepareRemoteCameraControl()
        {
            StartRemoteControl();
        }
        public void FinishRemoteCameraControl()
        {
            StopRemoteControl();
        }

        #endregion

        #region Hardware Monitor

        #region Laser Lock Error Monitor

        public object leStopLock = new object();
        private bool monitorLE = false;
        public double LaserLockErrorThreshold = new double();

        public void StartMonitoringLaserErrorSignal()
        {
            monitorLE = true;
            Thread LLEThread = new Thread(new ThreadStart(leMonitorLoop));
            LLEThread.Start();
        }
        

        private double getLaserThresholdFromUI()
        {
            return monitorWindow.GetLaserErrorSignalThreshold();
        }

        private void leMonitorLoop()
        {
            Color ledColor = new Color();
            while (monitorLE)
            {
                Thread.Sleep(1000);

                LaserLockErrorThreshold = getLaserThresholdFromUI();

                double error = ReadLaserErrorSignal();
                
                bool isLocked = isLaserLocked(LaserLockErrorThreshold, error);
                
                if (isLocked)
                {
                    ledColor = Color.LightGreen;
                }
                else
                {
                    ledColor = Color.Red;
                    MessageBox.Show("Careful! Laser appears to be unlocked!");
                }
                lock (leStopLock)
                {

                    monitorWindow.SetLaserErrorSignal(error, ledColor);
                    if (!monitorLE)
                    {
                        return;
                    }
                }

            }
        }
        private bool isLaserLocked(double threshold, double error)
        {
            if (-threshold <= error && error <= threshold)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public void StopMonitoringLaserErrorSignal()
        {
            monitorLE = false;
        }

        public double ReadLaserErrorSignal()
        {
            double es = 10;
            try
            {
                es = ReadAnalogInput("laserLockErrorSignal");
            }
            catch
            {
            }
            return es;
        }
        #endregion

        #region Pressure Gauges

        #region Chamber 1
        private bool monitorC1P = false;
        public object c1pStopLock = new object();


        public void StartChamber1PressureMonitor()
        {
            monitorC1P = true;
            Thread C1PThread = new Thread(new ThreadStart(chamber1PressureMonitorLoop));
            C1PThread.Start();
        }


        private void chamber1PressureMonitorLoop()
        {
            while (monitorC1P)
            {
                Thread.Sleep(1000);
                double pressure = ReadChannel1Pressure();                
                lock (c1pStopLock)
                {
                    monitorWindow.SetChamber1Pressure(pressure);
                    if (!monitorC1P)
                    {
                        return;
                    }
                }

            }
        }

        public double ReadChannel1Pressure()
        {
            double value = 10;
            try
            {
                value = ReadAnalogInput("chamber1Pressure", true);
            }
            catch
            {
            }
            return value;
        }

        public void StopChamber1PressureMonitor()
        {
            monitorC1P = false;
        }

        #endregion
        #endregion

        #region Remote Access for Hardware Monitor

        public Dictionary<String, Object> GetHardwareReport()
        {
            Dictionary<String, Object> report = new Dictionary<String, Object>();
            report["laserLockErrorSignal"] = ReadLaserErrorSignal();
            report["chamber1Pressure"] = ReadChannel1Pressure();
            foreach (KeyValuePair<string, double> pair in stateRecord.analogs)
            {
                report[pair.Key] = pair.Value;
            }
            foreach (KeyValuePair<string, bool> pair in stateRecord.digitals)
            {
                report[pair.Key] = pair.Value;
            }
            return report;
        }
        #endregion

        #endregion


    }
}
