using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Runtime.Remoting.Lifetime;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Windows;

using NationalInstruments;
using NationalInstruments.DAQmx;
using NationalInstruments.UI;
using NationalInstruments.VisaNS;

using DAQ;
using DAQ.HAL;
using DAQ.Environment;

namespace NavigatorHardwareControl
{
    /// <summary>
    /// This is the interface to the Navigator hardware controller and is based largely on the sympathetic hardware controller.
    /// </summary>
    public class Controller : MarshalByRefObject
    {
        #region Constants
        private static Hashtable calibrations = Environs.Hardware.Calibrations;
        private static string profilesPath = (string)Environs.FileSystem.Paths["settingsPath"]
            + "NavigatorHardwareController\\";
        #endregion

        #region setup
        public ControlWindow controlWindow;

        //table of digital tasks
        Hashtable digitalTasks = new Hashtable();

        //dictionary of analog output tasks
        private Dictionary<string, Task> analogOutTasks;
        private Dictionary<string, Task> analogInTasks;

        //enumerate the state of the hardware controller for remoting access
        public enum NavHardwareState { OFF, LOCAL, REMOTE };
        public NavHardwareState hcState = new NavHardwareState();

        public HardwareState hardwareState;

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        //Create the Muquans Communicator
        public MuquansCommunicator muquans;

        //Create an instance of the Fibre Aligner
        public FibreAligner fibreAlign;
        //these are used to handle errors from the slave and aom dds processes
        public StreamReader slaveErr;
        public StreamReader aomErr;

        public void Start()
        {
            analogOutTasks = new Dictionary<string, Task>();
            analogInTasks = new Dictionary<string, Task>();
            digitalTasks = new Hashtable();

            //initialise the hardware state
            hardwareState = new HardwareState();
            hardwareState.analogs = new Dictionary<string, double>();
            hardwareState.digitals = new Dictionary<string, bool>();

            if (!Environs.Debug)
            {
         
                //Only creates these tasks when the Debug flag is set to false. Useful when developing on another computer
                CreateDigitalTask("motTTL");
                CreateDigitalTask("mphiTTL");
                CreateDigitalTask("ramanTTL");
                CreateDigitalTask("cameraTTL");

                CreateAnalogOutputTask("motCTRL");
                CreateAnalogOutputTask("mphiCTRL");
                CreateAnalogOutputTask("ramanCTRL");
                CreateAnalogOutputTask("horizPiezo");
                CreateAnalogOutputTask("vertPiezo");

                //these analogin tasks currently use the whole voltage range. perhaps this should be changed as necessary
                CreateAnalogInputTask("photodiode");
                CreateAnalogInputTask("accelpos");
                CreateAnalogInputTask("accelmin");
                CreateAnalogInputTask("fibrePD");
                try
                {
                    muquans = new MuquansCommunicator();
                }
                catch(Exception e)
                {
                    Console.WriteLine("Couldn't start Muquans communication: " + e.Message);
                }


                //Configures the error handling of the DDS programs
                //   muquans.ConfigureDDS("slave", 18);
                // muquans.ConfigureDDS("aom", 20);
                muquans.slaveDDS.EnableRaisingEvents = true;
                muquans.aomDDS.EnableRaisingEvents = true;

                //slaveErr = muquans.slaveDDS.StandardError;
                //aomErr = muquans.aomDDS.StandardError
                muquans.Start();
                muquans.slaveDDS.OutputDataReceived += new DataReceivedEventHandler(DDSErrorHandler);
                muquans.aomDDS.ErrorDataReceived += new DataReceivedEventHandler(DDSErrorHandler);

                //Starts the DDS programs - these port numbers depend on the virtual COM ports use
                ProcessStartInfo slaveInfo = muquans.ConfigureDDS("slave", 18);
                ProcessStartInfo aomInfo = muquans.ConfigureDDS("aom", 20);

                muquans.slaveDDS.StartInfo = slaveInfo;
                muquans.aomDDS.StartInfo = aomInfo;
                muquans.slaveDDS.Start();
                muquans.aomDDS.Start();
                
            }

            try
            {
                muquans = new MuquansCommunicator();
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't start Muquans communication: " + e.Message);
            }

            fibreAlign = new FibreAligner("horizPiezo", "vertPiezo", "fibrePD");
            fibreAlign.controller = this;
        }
        #endregion

        #region Parameter Serialisation and Hardware State Tracking
        ///<summary>
        // this is basically just a collection of dictionaries to make it a bit easier to add values as necessary. The keys used are the names of the object that represents them in the hardwarecontroller.
        ///Anytime the hardware gets modified by this program, the stateRecord get updated. Don't hack this. 
        /// It's useful to know what the hardware is doing at all times.
        /// When switching to REMOTE, the updates no longer happen. That's why we store the state before switching to REMOTE and apply the state
        /// back again when returning to LOCAL.
        /// </summary>
        [Serializable]
        public class HardwareState
        {
            //TODO make the objects that reference hardware not controlled via analogue/digital values behave properly
            public Dictionary<string, double[]> ddsValues = new Dictionary<string,double[]>();
            public Dictionary<string, double> laserVals = new Dictionary<string,double>();
            public Dictionary<string, bool> laserStates = new Dictionary<string,bool>();
            public Dictionary<string, double> analogs;
            public Dictionary<string, bool> digitals;

        }

        public void SaveParametersWithDialog()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "nav parameters|*.bin";
            saveFileDialog1.Title = "Save parameters";
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String hardwareStateDir = settingsPath + "NavHardwareController";
            saveFileDialog1.InitialDirectory = hardwareStateDir;
            if (saveFileDialog1.ShowDialog()==true)
            {
                if (saveFileDialog1.FileName != "")
                {
                    StoreParameters(saveFileDialog1.FileName);
                }
            }
        }

        public void StoreParameters()
        {
            hardwareState = readValuesOnUI();
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String hardwareStateFilePath = settingsPath + "\\NavHardwareController\\parameters.bin";
            StoreParameters(hardwareStateFilePath);
        }

        public void StoreParameters(String hardwareStateFilePath)
        {
            // serialize it
            BinaryFormatter s = new BinaryFormatter();
            try
            {
                s.Serialize(new FileStream(hardwareStateFilePath, FileMode.Create), hardwareState);
            }
            catch (Exception)
            { Console.Out.WriteLine("Unable to store settings"); }
        }

        public void LoadParametersWithDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "navhc parameters|*.bin";
            dialog.Title = "Load parameters";
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String hardwareStateDir = settingsPath + "NavHardwareController";
            dialog.InitialDirectory = hardwareStateDir;
            dialog.ShowDialog();
            if (dialog.FileName != "") hardwareState = LoadParameters(dialog.FileName);
            setValuesDisplayedOnUI(hardwareState);
        }

        private HardwareState LoadParameters()
        {
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String hardwareStateFilePath = settingsPath + "\\NavHardwareController\\parameters.bin";
            return LoadParameters(hardwareStateFilePath);
        }

        private HardwareState LoadParameters(String hardwareStateFilePath)
        {
            // deserialize
            BinaryFormatter s = new BinaryFormatter();
            HardwareState hardwareState = new HardwareState();
            // eat any errors in the following, as it's just a convenience function
            try
            {
                hardwareState = (HardwareState)s.Deserialize(new FileStream(hardwareStateFilePath, FileMode.Open));
            }
            catch (Exception e)
            { Console.WriteLine("Unable to load settings: "+e.Message); }
            return hardwareState;
        }


        #endregion

        #region Hardware task creation methods
        private void CreateAnalogInputTask(string channel)
        {

            Task task = new Task("NavHCIn" + channel);
            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                task,
                0,
                10
            );
            task.Control(TaskAction.Verify);
            analogInTasks.Add(channel, task);
        }

        private void CreateAnalogInputTask(string channel, double lowRange, double highRange)
        {
            Task task = new Task("NavHCIn" + channel);
            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                task,
                lowRange,
                highRange
            );
            task.Control(TaskAction.Verify);
            analogInTasks.Add(channel, task);
        }

        private void CreateAnalogOutputTask(string channel)
        {
            hardwareState.analogs[channel] = 0.0;
            Task task = new Task("NavHCOut" + channel);
            AnalogOutputChannel c = ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[channel]);
            c.AddToTask(
                task,
                c.RangeLow,
                c.RangeHigh
                );
            task.Control(TaskAction.Verify);
            analogOutTasks.Add(channel, task);
        }

        // setting an analog voltage to an output
        public void SetAnalogOutput(string channel, double voltage)
        {
            SetAnalogOutput(channel, voltage, false);
        }
        //Overload for using a calibration before outputting to hardware
        public void SetAnalogOutput(string channelName, double voltage, bool useCalibration)
        {

            AnalogSingleChannelWriter writer = new AnalogSingleChannelWriter(analogOutTasks[channelName].Stream);
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
                analogOutTasks[channelName].Control(TaskAction.Unreserve);
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
            AnalogSingleChannelReader reader = new AnalogSingleChannelReader(analogOutTasks[channelName].Stream);
            double val = reader.ReadSingleSample();
            analogOutTasks[channelName].Control(TaskAction.Unreserve);
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

        public double ReadAnalogInput(string channel, double sampleRate, int numOfSamples)
        {
            Task task = analogInTasks[channel];
            //Configure the timing parameters of the task
            task.Timing.ConfigureSampleClock("", sampleRate,
                SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, numOfSamples);

            //Read in multiple samples
            AnalogSingleChannelReader reader = new AnalogSingleChannelReader(task.Stream);
            double[] valArray = reader.ReadMultiSample(numOfSamples);
            task.Control(TaskAction.Unreserve);

            //Calculate the average of the samples
            double sum = 0;
            for (int j = 0; j < numOfSamples; j++)
            {
                sum = sum + valArray[j];
            }
            double val = sum / numOfSamples;
            return val;
        }

        private void CreateDigitalTask(String name)
        {
            hardwareState.digitals[name] = false;
            Task digitalTask = new Task(name);
            ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[name]).AddToTask(digitalTask);
            digitalTask.Control(TaskAction.Verify);
            digitalTasks.Add(name, digitalTask);
        }

        // We won't be using digital inputs but I'll leave this here in case
        //private void CreateDigitalInputTask(String name)
        //{
        //    Task digitalInputTask = new Task(name);
        //    ((DigitalInputChannel)Environs.Hardware.DigitalInputChannels[name]).AddToTask(digitalInputTask);
        //    digitalInputTask.Control(TaskAction.Verify);
        //    digitalInputTasks.Add(name, digitalInputTask);
        //}
        //
        //bool ReadDigitalLine(string name)
        //{
        //    Task digitalInputTask = ((Task)digitalInputTasks[name]);
        //    DigitalSingleChannelReader reader = new DigitalSingleChannelReader(digitalInputTask.Stream);
        //    bool digSample = reader.ReadSingleSampleSingleLine();
        //    digitalInputTask.Control(TaskAction.Unreserve);
        //    return digSample;
        //}

        private void SetDigitalLine(string name, bool value)
        {
            Task digitalTask = ((Task)digitalTasks[name]);
            DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(digitalTask.Stream);
            writer.WriteSingleSampleSingleLine(true, value);
            digitalTask.Control(TaskAction.Unreserve);
        }

        #endregion

        #region Controlling Hardware with UI

        #region Hardware Update
            public void ApplyRecordedStateToHardware()
        {
            applyToHardware(hardwareState);          
        }


        public void UpdateHardware()
        {
            HardwareState uiState = readValuesOnUI();

            HardwareState changes = getDiscrepancies(hardwareState, uiState);

            applyToHardware(changes);

            updateStateRecord(changes);


        }

        private void applyToHardware(HardwareState state)
        {
            if (state.analogs.Count != 0 || state.digitals.Count != 0)
            {
                if (hcState == NavHardwareState.OFF)
                {

                    hcState = NavHardwareState.LOCAL;
                    controlWindow.UpdateUIState(hcState);

                    applyAnalogs(state);
                    applyDigitals(state);

                    hcState = NavHardwareState.OFF;
                    controlWindow.UpdateUIState(hcState);

                    controlWindow.WriteToConsole("Update finished!");
                }
            }
            else
            {
                controlWindow.WriteToConsole("The values on the UI are identical to those on the controller's records. Hardware must be up to date.");
            }
        }

        private HardwareState getDiscrepancies(HardwareState oldState, HardwareState newState)
        {
            HardwareState state = new HardwareState();
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

        private void updateStateRecord(HardwareState changes)
        {
            foreach (KeyValuePair<string, double> pairs in changes.analogs)
            {
                hardwareState.analogs[pairs.Key] = changes.analogs[pairs.Key];
            }
            foreach (KeyValuePair<string, bool> pairs in changes.digitals)
            {
                hardwareState.digitals[pairs.Key] = changes.digitals[pairs.Key];
            }
        }

        
        private void applyAnalogs(HardwareState state)
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

        private void applyDigitals(HardwareState state)
        {
            foreach (KeyValuePair<string, bool> pairs in state.digitals)
            {
                SetDigitalLine(pairs.Key, pairs.Value);
                controlWindow.WriteToConsole("Set channel '" + pairs.Key.ToString() + "' to " + pairs.Value.ToString());
            }
        }
        #endregion 

        #region Reading and Writing to UI

        private HardwareState readValuesOnUI()
        {
            HardwareState state = new HardwareState();
            state.analogs = readUIAnalogs(hardwareState.analogs.Keys);
            state.digitals = readUIDigitals(hardwareState.digitals.Keys);
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
       

        private void setValuesDisplayedOnUI(HardwareState state)
        {
            setUIAnalogs(state);
            setUIDigitals(state);
        }
        private void setUIAnalogs(HardwareState state)
        {
            foreach (KeyValuePair<string, double> pairs in state.analogs)
            {
                controlWindow.SetAnalog(pairs.Key, (double)pairs.Value);
            }
        }
        private void setUIDigitals(HardwareState state)
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
        /// last hardware state, then prevents you from making any changes to the UI. Use this if your other program wants direct control of hardware.
        /// </summary>
        public void StartRemoteControl()
        {
            if (hcState == NavHardwareState.OFF)
            {
                StoreParameters(profilesPath + "tempParameters.bin");
                hcState = NavHardwareState.REMOTE;
                controlWindow.UpdateUIState(hcState);
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
                setValuesDisplayedOnUI(LoadParameters(profilesPath + "tempParameters.bin"));

                if (System.IO.File.Exists(profilesPath + "tempParameters.bin"))
                {
                    System.IO.File.Delete(profilesPath + "tempParameters.bin");
                }
            }
            catch (Exception)
            {
                controlWindow.WriteToConsole("Unable to load Parameters.");
            }
            hcState = NavHardwareState.OFF;
            controlWindow.UpdateUIState(hcState);
            ApplyRecordedStateToHardware();
        }

        /// <summary>
        /// These SetValue functions are for giving commands to the hc from another program, while keeping the hc in control of hardware.
        /// Use this if you want the HC to keep control, but you want to control the HC from some other program
        /// </summary>
        public void SetValue(string channel, double value)
        {
            hcState = NavHardwareState.LOCAL;
            hardwareState.analogs[channel] = value;
            SetAnalogOutput(channel, value, false);
            setValuesDisplayedOnUI(hardwareState);
            hcState = NavHardwareState.OFF;

        }
        public void SetValue(string channel, double value, bool useCalibration)
        {
            hardwareState.analogs[channel] = value;
            hcState = NavHardwareState.LOCAL;
            SetAnalogOutput(channel, value, useCalibration);
            setValuesDisplayedOnUI(hardwareState);
            hcState = NavHardwareState.OFF;

        }
        public void SetValue(string channel, bool value)
        {
            hcState = NavHardwareState.LOCAL;
            hardwareState.digitals[channel] = value;
            SetDigitalLine(channel, value);
            setValuesDisplayedOnUI(hardwareState);
            hcState = NavHardwareState.OFF;

        }
        #endregion

        #region Muquans Control
        public void UpdateDDS()
        {
            //Gets the DDS values to send to the Muquans communicator

        }

        public void EdfaLock(string edfaID, bool lockParam, double lockValue)
        {
            muquans.LockEDFA(edfaID, lockParam, lockValue);
        }

        public void StartEDFA(string id)
        {
            muquans.StartEDFA(id);
        }

        public void StopEDFA(string id)
        {
            muquans.StopEDFA(id);
        }

        public TextReader LockLaser(string laserID)
        {
            return muquans.LockLaser(laserID);
        }

        public void UnlockLaser(string laserID)
        {
            muquans.UnlockLaser(laserID);
        }
        #endregion

        #region Fibre Alignment
        /// <summary>
        /// Scans the piezos for the mirror mount and returns an array of the measured PD values.
        /// </summary>
        /// <param name="numSteps">number of voltage steps for piezos</param>
        /// <param name="numSamp">Number of samples per input</param>
        public double[,] ScanFibre(int numSteps, double sampleRate, int numSamples)
        {
            double horizVolt;
            double vertVolt;
            double[,] scanData = new double[numSteps, numSteps];
            //The voltage range for each piezo is from 0 to 10V
            for (int i = 0; i < numSteps; i++)
            {
                for (int j = 0; j < numSteps; j++)
                {
                    vertVolt = 10.0 * j / numSteps;
                    horizVolt = 10.0 * i / numSteps;
                    SetAnalogOutput("horizPiezo", horizVolt);
                    SetAnalogOutput("vertPiezo", vertVolt);
                    double value = ReadAnalogInput("fibrePD",sampleRate,numSamples);
                    scanData[i, j] = value;
                }
            }
            fibreAlign.ScanData = scanData;
            
            //TODO Implement code to write this scandata to a csv.
            return scanData;
        }

        public string LoadFibreScanData()
        {
            //Returns the path to a fibre scan
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = ".csv";
            openFile.Title = "Choose Fibre Scan Data to Test";
            openFile.InitialDirectory = (string)Environs.FileSystem.Paths["settingsPath"];
            openFile.ShowDialog();
            return openFile.FileName;
        }
        /// <summary>
        /// Aligns the fibre by trying to maximise the input power
        /// </summary>
        /// <param name="threshold">Threshold value for alignment. Ideally normalised to 1</param>
        /// <returns></returns>
        public int[] AlignFibre(double threshold, bool align)
        {
            int[] coords = new int[2];

            //Probably not a good idea to hardcode these here.
            fibreAlign.sampleRate = 10000.0;
            fibreAlign.numSamples = 100;
            coords = fibreAlign.AlignFibre(threshold, align);
            return coords;
        }

        #endregion

        #endregion

        #region Event Handlers

        private void DDSErrorHandler(object sendingProcess, DataReceivedEventArgs eventArgs)
        {
            //If one of the DDS programs exits, this prints the error output to the console and opens a message box
            controlWindow.WriteToConsole(eventArgs.Data);
            Console.WriteLine(eventArgs.Data);
            MessageBox.Show("One (or both) of the DDS programs has crashed. Check the console for more information.");
        }

        public void ControllerExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            //If there is an unhandled exception in the controller, it prints this to the console.
                   //this is designed to handled an exception in a thread and print it to the console. Should help prevent uneccessary terminations
            try
            {
                Exception ex = e.ExceptionObject as Exception;
                string errorMessage =
           "Unhandled Exception:\n\n" +
           ex.Message + "\n\n" +
           ex.GetType() +
           "\n\nStack Trace:\n" +
           ex.StackTrace;
                Console.WriteLine(errorMessage);
            }
            catch
            {
                MessageBox.Show("Fatal Error");
            }
        }
        #endregion


    }
}
