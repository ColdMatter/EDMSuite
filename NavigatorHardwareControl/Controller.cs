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
        #region setup 
        public ControlWindow controlWindow;

        //table of digital tasks
        Hashtable digitalTasks = new Hashtable();

        //dictionary of analog output tasks
        private Dictionary<string, Task> analogoutTasks;

        //enumerate the state of the hardware controller for remoting access
        public enum NavHardwareState { OFF, LOCAL, REMOTE };
        public NavHardwareState hcState = new NavHardwareState();

        public DataStore dataStore = new DataStore();
        hardwareState stateRecord;
        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        //Create the Muquans Communicator
        public MuquansCommunicator muquans;
        //these are used to handle errors from the slave and aom dds processes
        public StreamReader slaveErr;
        public StreamReader aomErr;

        public void Start()
        {
            analogoutTasks = new Dictionary<string, Task>();
            //initialise the hardware state
            stateRecord = new hardwareState();
            stateRecord.analogs = new Dictionary<string, double>();
            stateRecord.digitals = new Dictionary<string, bool>();

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

                //these analogin tasks currently use the whole voltage range. perhaps this should be changed as necessary
                CreateAnalogInputTask("photodiode");
                CreateAnalogInputTask("accelpos");
                CreateAnalogInputTask("accelmin");
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

        #region Parameter Serialisation
        // this is basically just a collection of dictionaries to make it a bit easier to add values as necessary. The keys used are the names of the object that represents them in the hardwarecontroller
        [Serializable]
        public class DataStore
        {
            public Dictionary<string, double> ddsFreqs = new Dictionary<string,double>();
            public Dictionary<string, double> ddsPhases = new Dictionary<string,double>();
            public Dictionary<string, double> laserVals = new Dictionary<string,double>();
            public Dictionary<string, bool> laserStates = new Dictionary<string,bool>();      

        }

        public void SaveParametersWithDialog()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "nav parameters|*.bin";
            saveFileDialog1.Title = "Save parameters";
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String dataStoreDir = settingsPath + "NavHardwareController";
            saveFileDialog1.InitialDirectory = dataStoreDir;
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
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String dataStoreFilePath = settingsPath + "\\NavHardwareController\\parameters.bin";
            StoreParameters(dataStoreFilePath);
        }

        public void StoreParameters(String dataStoreFilePath)
        {
            // serialize it
            BinaryFormatter s = new BinaryFormatter();
            try
            {
                s.Serialize(new FileStream(dataStoreFilePath, FileMode.Create), dataStore);
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
            String dataStoreDir = settingsPath + "NavHardwareController";
            dialog.InitialDirectory = dataStoreDir;
            dialog.ShowDialog();
            if (dialog.FileName != "") LoadParameters(dialog.FileName);
        }

        private void LoadParameters()
        {
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String dataStoreFilePath = settingsPath + "\\NavHardwareController\\parameters.bin";
            LoadParameters(dataStoreFilePath);
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
              

            }
            catch (Exception e)
            { Console.WriteLine("Unable to load settings: "+e.Message); }
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

        #endregion

        #region Hardware task creation methods
        private Task CreateAnalogInputTask(string channel)
        {
            Task task = new Task("EDMHCIn" + channel);
            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                task,
                0,
                10
            );
            task.Control(TaskAction.Verify);
            return task;
        }

        private Task CreateAnalogInputTask(string channel, double lowRange, double highRange)
        {
            Task task = new Task("NavHCIn" + channel);
            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                task,
                lowRange,
                highRange
            );
            task.Control(TaskAction.Verify);
            return task;
        }

        private Task CreateAnalogOutputTask(string channel)
        {
            Task task = new Task("NavHCOut" + channel);
            AnalogOutputChannel c = ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[channel]);
            c.AddToTask(
                task,
                c.RangeLow,
                c.RangeHigh
                );
            task.Control(TaskAction.Verify);
            return task;
        }

        private void SetAnalogOutput(Task task, double voltage)
        {
            AnalogSingleChannelWriter writer = new AnalogSingleChannelWriter(task.Stream);
            writer.WriteSingleSample(true, voltage);
            task.Control(TaskAction.Unreserve);
        }

        private double ReadAnalogInput(Task task)
        {
            AnalogSingleChannelReader reader = new AnalogSingleChannelReader(task.Stream);
            double val = reader.ReadSingleSample();
            task.Control(TaskAction.Unreserve);
            return val;
        }

        private double ReadAnalogInput(Task task, double sampleRate, int numOfSamples)
        {
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
