using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Runtime.Remoting.Lifetime;
using System.Windows.Forms;
using System.Drawing;
using timerAlias = System.Timers.Timer;

using NationalInstruments;
using NationalInstruments.DAQmx;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using NationalInstruments.VisaNS;

using DAQ;
using DAQ.HAL;
using DAQ.Environment;
using DAQ.TransferCavityLock;

using IMAQ;


namespace RFMOTHardwareControl
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
    public class Controller : MarshalByRefObject, CameraControllable, ExperimentReportable
    {
        #region Constants
        //Put any constants and stuff here

        private static string cameraAttributesPath = (string)Environs.FileSystem.Paths["UntriggeredCameraAttributesPath"];
        private static string profilesPath = (string)Environs.FileSystem.Paths["settingsPath"]
            + "RFMOTHardwareController\\";

        private static Hashtable calibrations = Environs.Hardware.Calibrations;
        #endregion

        #region Setup

        NovatechSerialPort novatechdds = (NovatechSerialPort)Environs.Hardware.Instruments["dds"];

        mcFreqCtr freqCtr = (mcFreqCtr)Environs.Hardware.Instruments["mcFrqCtr"];
        private bool freqCtrConnected = false;

        private timerAlias frqCtrTimer = new timerAlias(1000);
        

        public string getNewFreq()
        {
            return freqCtr.f.ToString("F4");
        }

        // table of all digital analogTasks
        Hashtable digitalTasks = new Hashtable();

        //Cameras
        public CameraController ImageController;
        

        // Declare that there will be a controlWindow
        ControlWindow controlWindow;
        HardwareMonitorWindow monitorWindow;
        ImageAnalysisWindow imAnalWindow;
        voltageLogger voltageLoggerWindow;
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

            CreateDigitalTask("coolaom");

            CreateDigitalTask("coolaom2");

            CreateDigitalTask("camtrig");

            //CreateDigitalTask("AOSampleClockExternal");

            CreateDigitalTask("refaom");

            CreateDigitalTask("repumpShutter");

            CreateDigitalTask("ctrInputSelect");

            // make the analog output analogTasks. The function "CreateAnalogOutputTask" is defined later
            //e.g.  bBoxAnalogOutputTask = CreateAnalogOutputTask("b");
            //      steppingBBiasAnalogOutputTask = CreateAnalogOutputTask("steppingBBias");

            CreateAnalogOutputTask("motfet");

            CreateAnalogOutputTask("motlightatn");

            CreateAnalogOutputTask("motlightatn2");

            CreateAnalogOutputTask("coolsetpt");

            CreateAnalogOutputTask("zbias");

            CreateAnalogOutputTask("biasA");

            CreateAnalogOutputTask("biasB");

            CreateAnalogOutputTask("coolingfeedfwd");

            CreateAnalogInputTask("MOTFieldGradient");
            
            // make the control controlWindow
            controlWindow = new ControlWindow();
            controlWindow.controller = this;
            

            HCState = SHCUIControlState.OFF;



             Application.Run(controlWindow);

        }

        // this method runs immediately after the GUI sets up
        internal void ControllerLoaded()
        {

            hardwareState loadedState = new hardwareState();
            loadedState = loadParameters(profilesPath + "StoppedParameters.bin");
            foreach (KeyValuePair<string, double> pair in loadedState.analogs)
            {
                stateRecord.analogs[pair.Key] = pair.Value;
            }
            foreach (KeyValuePair<string, bool> pair in loadedState.digitals)
            {
                stateRecord.digitals[pair.Key] = pair.Value;
            }
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

        #region Voltage Logger

       public void openNewVoltageLoggerWindow()
        {
            voltageLoggerWindow = new voltageLogger();
            voltageLoggerWindow.Show();

        }

        #endregion

        #region ImageAnalysisWindow

        public void OpenNewImageAnalysisWindow()
        {
            if (ImageController == null)
            {
                StartCameraControl();
            }
            imAnalWindow = new ImageAnalysisWindow();
            imAnalWindow.controller = ImageController;
            imAnalWindow.Show();
            startImageAnalysis();
        }

        private bool analyseImage = false;

        private void startImageAnalysis()
        {
            analyseImage = true;
            Thread imageAnalThread = new Thread(doImageAnalysis);
            imageAnalThread.Start();
        }

        public void stopImageAnalysis()
        {
            analyseImage = false;
        }

        private void doImageAnalysis()
        {
            while (analyseImage)
            {
                if(ImageController.rectangleROI.Count != 0)
                {
                    imAnalWindow.updateImageAndAnalyse();
                }
                Thread.Sleep(200);
                
            }
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

                    applyAnalogs(state);
                    applyDigitals(state);

                    HCState = SHCUIControlState.OFF;

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

        #endregion

        #region MiniCircuitsFrequencyCounter stuff

        private void connectFreqCounter()
        {
            freqCtr.connectFrqCtr();

            freqCtrConnected = freqCtr.isConnected();

            if (freqCtrConnected)
            {
                controlWindow.WriteToConsole("Frequency Counter Connected");
            }
            else
            {
                controlWindow.WriteToConsole("Frequency Counter Failed To Connect");
            }
        }

        public void updateFreqCtrSettings()
        {
            freqCtr.GT = controlWindow.getGateTime();
            frqCtrTimer.Interval = 1000 * controlWindow.getGateTime();
            if (controlWindow.getAcquisitionState())
            {
                frqCtrTimer.Start();
            }
            else
            {
                frqCtrTimer.Stop();
            }
            /*Call update hardware to process any change in counter rf switch input*/
            UpdateHardware();
        }

        #endregion

        #region Serial port stuff

        public void COMPortsLookupAndDisplay()
        {
            string[] s = COMPortsLookup();
            displayCOMPorts(s);
        }
        private string[] COMPortsLookup()
        {
            string[] COMPortsList = SerialPort.GetPortNames();//method to get list of comports
            return COMPortsList;
        }
        private void displayCOMPorts(string[] s)
        {
            controlWindow.FillCOMPortsComboBox(s);
        }
        public void startNovatech(string portName)
        {
            novatechdds = new NovatechSerialPort();
            novatechdds.novatechSerialPort.DataReceived += new SerialDataReceivedEventHandler(ddsDataReceivedHandler);
            novatechdds.openNovatechPort(portName, 19200);
            controlWindow.WriteToConsole("Port name is " + novatechdds.novatechSerialPort.PortName.ToString());
            controlWindow.WriteToConsole("Specified baud rate is " + novatechdds.novatechSerialPort.BaudRate.ToString());
            controlWindow.WriteToConsole("Port is open : " + novatechdds.novatechSerialPort.IsOpen.ToString());
            //add event handler to serial port in controller
        }
        public void endNovatech()
        {
            novatechdds.novatechSerialPort.Close();
            controlWindow.WriteToConsole("Port is open : " + novatechdds.novatechSerialPort.IsOpen);
        }
        private void ddsDataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string dataReceived = sp.ReadExisting();
            if (novatechdds.errorMessages.ContainsKey(dataReceived))
            {
                controlWindow.WriteToConsole(dataReceived + novatechdds.errorMessages[dataReceived]);
            }
            else
            {
                controlWindow.WriteToConsole("Data at com port is " + dataReceived);
            }
        }
        public void passDDSNewFrequencies(Dictionary<string, double> newFrequencies)
        {
            novatechdds.updateChannelFrequenciesDictionary(newFrequencies);
           // writeDDSFrequenciesToConsole();
        }
        public void writeDDSFrequenciesToConsole()
        {
            Dictionary<string, NovatechSerialPort.ddsChannel> currentFrequencies = new Dictionary<string, NovatechSerialPort.ddsChannel>();
            currentFrequencies = novatechdds.getFrequenciesDictionary();
            foreach (var key in currentFrequencies.Keys)
            {
                var value = currentFrequencies[key].Freq;
                controlWindow.WriteToConsole(key + "is set to " + value.ToString() + "MHz");
            }
        }
        public void writeSerialCommand(string command)
        {
            try
            {
                controlWindow.WriteToConsole("Writing command \"" + command + "\" to COM Port...");
                novatechdds.writeSerialCommandToBuffer(command);
            }
            catch (Exception e)
            {
                controlWindow.WriteToConsole(e.Message + "  COM Port error..");
            }
        }
        #endregion

        #region Local camera control

        public void StartCameraControl()
        {
            try
            {
                ImageController = new CameraController("cam2");
                ImageController.Initialize();
                ImageController.PrintCameraAttributesToConsole();
                controlWindow.WriteToConsole(ImageController.IsCameraFree().ToString());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Camera Initialization Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();

            }
        }
        public void CameraStream()
        {
            try
            {
                ImageController.Stream(cameraAttributesPath);
            }
            catch { }
        }

        public void StopCameraStream()
        {
            try
            {
                ImageController.StopStream();
            }
            catch { }
        }

        public void CameraSnapshot()
        {
            try
            {
                ImageController.SingleSnapshot(cameraAttributesPath);
            }
            catch { }
        }

        #endregion

        #region Saving Images

        public void SaveImageWithDialog()
        {
            ImageController.SaveImageWithDialog();
        }
        public void SaveImage(string path)
        {
            try
            {
                ImageController.SaveImage(path);
            }
            catch { }
        }
        #endregion

        #region Remote Camera Control
        //Written for taking images triggered by TTL. This "Arm" sets the camera so it's expecting a TTL.

        public byte[,] GrabSingleImage(string cameraAttributesPath)
        {
            return ImageController.SingleSnapshot(cameraAttributesPath);
        }
        public byte[][,] GrabMultipleImages(string cameraAttributesPath, int numberOfShots)
        {
            try
            {
                byte[][,] images = ImageController.MultipleSnapshot(cameraAttributesPath, numberOfShots);
                return images;
            }

            catch (TimeoutException)
            {
                FinishRemoteCameraControl();
                return null;
            }

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

        #region Remoting stuff

        /// <summary>
        /// This is used when you want another program to take control of some/all of the hardware. The hc then just saves the
        /// last hardware state, then prevents you from making any changes to the UI. Use this if your other program wants direct control of hardware.
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
        /// Use this if you want the HC to keep control, but you want to control the HC from some other program
        /// </summary>
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

        public double GetFrequency()
        {
            return freqCtr.f;
        }
        #endregion

        #region Remote Access for Hardware Monitor

        #region monitorLaserFrequencies

        private bool monitorFrequencies = false;

        public void startMonitoringLaserFrequencies()
        {
            if (!freqCtrConnected)
            {
                connectFreqCounter();
            }

            monitorFrequencies = true;
            Thread laserFreqThread = new Thread(monitorLaserFrequencies);
            laserFreqThread.Start();
        }

        public void stopMonitoringLaserFrequencies()
        {
            monitorFrequencies = false;
        }

        private void monitorLaserFrequencies()
        {
            while (monitorFrequencies)
            {
                monitorWindow.updateFrequency(freqCtr.f);
                Thread.Sleep(1000);
            }

        }

        public void closeHardwareMonitorWindow()
        {
            stopMonitoringLaserFrequencies();
            monitorWindow.Close();
        }

        #endregion

        #region Monitor MOT coil current

        private bool monitoringFieldGradient = false;

        public void startMonitoringFieldGradient()
        {
            monitoringFieldGradient = true;
            Thread fieldGradientThread = new Thread(monitorFieldGradient);
            fieldGradientThread.Start();
        }

        private void monitorFieldGradient()
        {
            while(monitoringFieldGradient){
                double newVal = 10.0 * ReadAnalogInput("MOTFieldGradient",true);
                monitorWindow.updateFieldGradient(newVal);
                Thread.Sleep(500);
            }
        }

        public void stopMonitoringFieldGradient()
        {
            monitoringFieldGradient = false;
        }

        #endregion

        public Dictionary<String, Object> GetExperimentReport()
        {
            Dictionary<String, Object> report = new Dictionary<String, Object>();
            //Here we add any extra keys and values read from the hardware controller UI, which it might be desirable to know later on.
            //E.g. report["LaserLockErrorSignal"] = readErrorSignal();
            report["CoolingLaserFrequency"] = monitorWindow.getCurrentFrequency();
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
    }
}
