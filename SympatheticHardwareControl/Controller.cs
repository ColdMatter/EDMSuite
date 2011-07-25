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
    /// There are 2 modes of operation: LOCAL and REMOTE.
    /// When operating in LOCAL, the state displayed on the UI gets directly output to hardware. 
    /// The controller can only read from the UI panel and apply it to hardware. It has no capability of deciding
    /// what to output itself.
    /// In the unusual case when the controller needs to take over (only when stopping and starting control),
    /// it can load a set a parameters to the panel (which then gets applied to hardware in the usual way).
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
            try
            {
                 ImageController = new ImageMaster("cam0", cameraAttributesPath);
                 ImageController.controller = this;
                 ImageController.Initialize();
                //cameraControl = ConnectToCamera("cam0");
            }
            catch (ImaqdxException e)
            {
                MessageBox.Show(e.Message, "Camera Initialization Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();

            }
            finally
            {
                stateRecord = loadParameters(profilesPath + "StoppedParameters.bin");
                setUIValues(stateRecord);
                ApplyRecordedStateToHardware();
            }

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

        #region keeping track of the things on this controller!

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
            setUIValues(stateRecord);
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
       

        private void setUIValues(hardwareState state)
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

        public void StartRemoteControl()
        {
            if (HCState == SHCUIControlState.OFF)
            {
                if (ImageController.Streaming)
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
                setUIValues(loadParameters(profilesPath + "tempParameters.bin"));
                
                if (System.IO.File.Exists(profilesPath + "tempParameters.bin"))
                {
                    System.IO.File.Delete(profilesPath + "tempParameters.bin");
                }
            }
            catch (Exception)
            {
                Console.Out.WriteLine("Unable to load Parameters.");
            }
            HCState = SHCUIControlState.OFF;
            controlWindow.UpdateUIState(HCState);
            ApplyRecordedStateToHardware();
        }
        public void SetValue(string channel, double value)
        {

            stateRecord.analogs[channel] = value;
            SetAnalogOutput(channel, value, false);
            setUIValues(stateRecord);

        }
        public void SetValue(string channel, double value, bool useCalibration)
        {

            stateRecord.analogs[channel] = value;
            SetAnalogOutput(channel, value, useCalibration);
            setUIValues(stateRecord);

        }
        public void SetValue(string channel, bool value)
        {

            stateRecord.digitals[channel] = value;
            SetDigitalLine(channel, value);
            setUIValues(stateRecord);

        }
        #endregion

        #endregion

        //camera stuff

        #region Local camera control

        public void StartCameraControl()
        {
            ImageController.Initialize();
        }
        public void CameraStream()
        {
            controlWindow.WriteToConsole("Streaming from camera");

            ImageController.Stream();
                   
        }

        public void StopCameraStream()
        {
            bool finished = ImageController.StopStream();
            if (finished)
            {
                controlWindow.WriteToConsole("Streaming stopped");

            } 
        }

        public void CameraSnapshot()
        {
            controlWindow.WriteToConsole("Taking snapshot");
            ImageController.SetCameraAttributes();
            ImageController.Snapshot();
        }

        public void SetCameraAttributes()
        {
            ImageController.SetCameraAttributes();
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
            /*VisionImage image = ImageController.Snapshot();
            PixelValue2D pval = image.ImageToArray();
            return pval.U8;
             */
            return ImageController.Snapshot();
            
        }
        public byte[][,] GrabMultipleImages(string cameraAttributesPath, int numberOfShots)
        {
            
            ImageController.SetCameraAttributes(cameraAttributesPath);

            try
            {
                return ImageController.TriggeredSequence(numberOfShots);
            }

            catch (TimeoutException)
            {
                FinishRemoteCameraControl();
                return null;
            }
            
        }

       public bool IsReadyForAcquisition()
           {
               if (ImageController.State == ImageMaster.CameraState.READY_FOR_ACQUISITION)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool PrepareRemoteCameraControl()
        {
            StartRemoteControl();
            return true;
        }
        public bool FinishRemoteCameraControl()
        {
            StopRemoteControl();
            ImageController.SetCameraAttributes(cameraAttributesPath);
            return true;
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
