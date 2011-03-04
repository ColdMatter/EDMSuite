using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Runtime.Remoting.Lifetime;
using System.Windows.Forms;
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

namespace SympatheticHardwareControl
{
    /// <summary>
    /// This is the interface to the sympathetic specific hardware.
    /// 
    /// Everything is just bundled into a single
    /// class. The methods/properties are grouped: the first set change the state of the hardware, these
    /// usually act immediately, but sometimes you need to call an update method. Read the code to find out
    /// which are which. The second set of methods read out the state of the hardware. These invariably need
    /// to be brought up to date with an update method before use.
    /// 
    /// Things to go when adding a hardware component to the controller:
    /// 1/ Declare any new pieces of hardware in Setup (and any constants if necessary)
    /// 2/ List the Tasks involving the piece of hardware
    /// 3/ Create the tasks
    /// 
    /// </summary>
    public class Controller : MarshalByRefObject
    {
        #region Constants
       //Put any constants and stuff here
        #endregion

        #region Setup

        // table of all digital tasks
        Hashtable digitalTasks = new Hashtable();
  
        //Cameras
        public const string motCamera = "cam0";
        private string cameraAttributesFile = "cameraAttributes.txt";
        private bool cameraBusy = false;
        
        // list Hardware (boards on computer are already known!?)
        //e.g.  HP8657ASynth greenSynth = (HP8657ASynth)Environs.Hardware.GPIBInstruments["green"];
        //      Synth redSynth = (Synth)Environs.Hardware.GPIBInstruments["red"];
        //      ICS4861A voltageController = (ICS4861A)Environs.Hardware.GPIBInstruments["4861"];

        Task aom0rfAmplitudeTask;
        Task aom0rfFrequencyTask;
        Task aom1rfAmplitudeTask;
        Task aom1rfFrequencyTask;
        Task aom2rfAmplitudeTask;
        Task aom2rfFrequencyTask;
        Task aom3rfAmplitudeTask;
        Task aom3rfFrequencyTask;
        Task coil0CurrentTask;
        Task coil1CurrentTask;

        // list ALL Tasks
        //e.g.  Task bBoxAnalogOutputTask;
        //      Task steppingBBiasAnalogOutputTask;
        


        // Declare that there will be a window
        ControlWindow window;
        private bool sHCUIControl;

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }


        public void Start()
        {
            // make the digital tasks. The function "CreateDigitalTask" is defined later
            //e.g   CreateDigitalTask("notEOnOff");
            //      CreateDigitalTask("eOnOff");
           
            CreateDigitalTask("aom0Enable");
            CreateDigitalTask("aom1Enable");
            CreateDigitalTask("aom2Enable");
            CreateDigitalTask("aom3Enable");

            // make the analog output tasks. The function "CreateAnalogOutputTask" is defined later
            //e.g.  bBoxAnalogOutputTask = CreateAnalogOutputTask("b");
            //      steppingBBiasAnalogOutputTask = CreateAnalogOutputTask("steppingBBias");

            aom0rfAmplitudeTask = CreateAnalogOutputTask("aom0amplitude");
            aom0rfFrequencyTask = CreateAnalogOutputTask("aom0frequency");
            aom1rfAmplitudeTask = CreateAnalogOutputTask("aom1amplitude");
            aom1rfFrequencyTask = CreateAnalogOutputTask("aom1frequency");
            aom2rfAmplitudeTask = CreateAnalogOutputTask("aom2amplitude");
            aom2rfFrequencyTask = CreateAnalogOutputTask("aom2frequency");
            aom3rfAmplitudeTask = CreateAnalogOutputTask("aom3amplitude");
            aom3rfFrequencyTask = CreateAnalogOutputTask("aom3frequency");
            coil0CurrentTask = CreateAnalogOutputTask("coil0Current");
            coil1CurrentTask = CreateAnalogOutputTask("coil1Current");

            // make analog input tasks. "CreateAnalogInputTask" is defined later
            //e.g   probeMonitorInputTask = CreateAnalogInputTask("probePD", 0, 5);
            //      pumpMonitorInputTask = CreateAnalogInputTask("pumpPD", 0, 5);

            //readAnalogVoltageTask = CreateAnalogInputTask("testAI");
            // make the control window
            window = new ControlWindow();
            window.controller = this;

            // run
            Application.Run(window);
        }

        // this method runs immediately after the GUI sets up
        internal void WindowLoaded()
        {
            // things like loading saved parameters, checking status of experiment etc. should go here.
            LoadParameters();
        }

        internal void WindowClosing()
        {
            // things like saving parameters, turning things off before quitting the program should go here
            StoreParameters();
        }

        // a list of functions for creating various tasks
        private Task CreateAnalogInputTask(string channel)
        {
            //SHCAI stands for "sympathetic hardware control analog input"
            Task task = new Task("SHCAI" + channel);
            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                task,
                0,
                10
            );
            task.Control(TaskAction.Verify);
            return task;
        }

        // an overload to specify input range
        private Task CreateAnalogInputTask(string channel, double lowRange, double highRange)
        {
            //SHCAI stands for "sympathetic hardware control analog input"
            Task task = new Task("SHCAI" + channel);
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
            //SHCAO stands for "sympathetic hardware control analog output"
            Task task = new Task("SHCAO" + channel);
            AnalogOutputChannel c = ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[channel]);
            c.AddToTask(
                task,
                c.RangeLow,
                c.RangeHigh
                );
            task.Control(TaskAction.Verify);
            return task;
        }

        // setting an analog voltage to an output
        private void SetAnalogOutput(Task task, double voltage)
        {
           
            AnalogSingleChannelWriter writer = new AnalogSingleChannelWriter(task.Stream);
            writer.WriteSingleSample(true, voltage);
            task.Control(TaskAction.Unreserve);
        }

        // reading an analog voltage from input
        private double ReadAnalogInput(Task task)
        {
            AnalogSingleChannelReader reader = new AnalogSingleChannelReader(task.Stream);
            double val = reader.ReadSingleSample();
            task.Control(TaskAction.Unreserve);
            return val;
        }

        // overload for reading multiple samples
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

        private void SetDigitalLine(string name, bool value)
        {
            Task digitalTask = ((Task)digitalTasks[name]);
            DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(digitalTask.Stream);
            writer.WriteSingleSampleSingleLine(true, value);
            digitalTask.Control(TaskAction.Unreserve);
        }
        #endregion

        #region Saving and loading experimental parameters
        // this isn't really very classy, but it works (says Jony)
        // declare all parameters which SHC controls here
        [Serializable]
        private struct DataStore
        {
          //e.g.    public double cPlus;
          //        public double cMinus
            public double aom0rfFrequency;
            public double aom0rfAmplitude;
            public bool aom0Enabled;
            public double aom1rfFrequency;
            public double aom1rfAmplitude;
            public bool aom1Enabled;
            public double aom2rfFrequency;
            public double aom2rfAmplitude;
            public bool aom2Enabled;
            public double aom3rfFrequency;
            public double aom3rfAmplitude;
            public bool aom3Enabled;
            public double coil0Current;
            public double coil1Current;
        }

        // Saving the parameters when closing the controller
        public void SaveParametersWithDialog()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "shc parameters|*.bin";
            saveFileDialog1.Title = "Save parameters";
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String dataStoreDir = settingsPath + "SympatheticHardwareController";
            saveFileDialog1.InitialDirectory = dataStoreDir;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    StoreParameters(saveFileDialog1.FileName);
                }
            }
        }

        // Quietly.
        public void StoreParameters()
        {
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String dataStoreFilePath = settingsPath + "\\SympatheticHardwareController\\parameters.bin";
            StoreParameters(dataStoreFilePath);
        }



        public void StoreParameters(String dataStoreFilePath)
        {
            DataStore dataStore = new DataStore();
            // fill the struct
            //e.g   dataStore.cPlus = CPlusVoltage;
            //      dataStore.cMinus = CMinusVoltage;
            dataStore.aom0rfAmplitude = Aom0rfAmplitude;
            dataStore.aom0rfFrequency = Aom0rfFrequency;
            dataStore.aom0Enabled = Aom0Enabled;
            dataStore.aom1rfAmplitude = Aom1rfAmplitude;
            dataStore.aom1rfFrequency = Aom1rfFrequency;
            dataStore.aom1Enabled = Aom1Enabled;
            dataStore.aom2rfAmplitude = Aom2rfAmplitude;
            dataStore.aom2rfFrequency = Aom2rfFrequency;
            dataStore.aom2Enabled = Aom2Enabled;
            dataStore.aom3rfAmplitude = Aom3rfAmplitude;
            dataStore.aom3rfFrequency = Aom3rfFrequency;
            dataStore.aom3Enabled = Aom3Enabled;
            dataStore.coil0Current = Coil0Current;
            dataStore.coil1Current = Coil1Current;
    

            // serialize it
            BinaryFormatter s = new BinaryFormatter();
            try
            {
                s.Serialize(new FileStream(dataStoreFilePath, FileMode.Create), dataStore);
            }
            catch (Exception)
            { Console.Out.WriteLine("Unable to store settings"); }
        }

        //Load parameters when opening the controller
        public void LoadParametersWithDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "shc parameters|*.bin";
            dialog.Title = "Load parameters";
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String dataStoreDir = settingsPath + "SympatheticHardwareController";
            dialog.InitialDirectory = dataStoreDir;
            dialog.ShowDialog();
            if (dialog.FileName != "") LoadParameters(dialog.FileName);
        }

        private void LoadParameters()
        {
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String dataStoreFilePath = settingsPath + "\\SympatheticHardwareController\\parameters.bin";
            LoadParameters(dataStoreFilePath);
        }

        private void LoadParameters(String dataStoreFilePath)
        {
            // deserialize
            BinaryFormatter s = new BinaryFormatter();
            // eat any errors in the following, as it's just a convenience function
            FileStream fs = new FileStream(dataStoreFilePath, FileMode.Open);
            try
            {
                DataStore dataStore = (DataStore)s.Deserialize(fs);

                // copy parameters out of the struct
                //e.g   CPlusVoltage = dataStore.cPlus;
                //e.g   CMinusVoltage = dataStore.cMinus;
                Aom0Enabled = dataStore.aom0Enabled;
                Aom0rfAmplitude = dataStore.aom0rfAmplitude;
                Aom0rfFrequency = dataStore.aom0rfFrequency;
                Aom1Enabled = dataStore.aom1Enabled;
                Aom1rfAmplitude = dataStore.aom1rfAmplitude;
                Aom1rfFrequency = dataStore.aom1rfFrequency;
                Aom2Enabled = dataStore.aom2Enabled;
                Aom2rfAmplitude = dataStore.aom2rfAmplitude;
                Aom2rfFrequency = dataStore.aom2rfFrequency;
                Aom3Enabled = dataStore.aom3Enabled;
                Aom3rfAmplitude = dataStore.aom3rfAmplitude;
                Aom3rfFrequency = dataStore.aom3rfFrequency;
                Coil0Current = dataStore.coil0Current;
                Coil1Current = dataStore.coil1Current;
            }
            catch (Exception)
            { Console.Out.WriteLine("Unable to load settings"); }
            finally
            {
                fs.Close();
            }
        }
        #endregion

        #region Saving and loading images
        // Saving the image
        public void SaveImageWithDialog(VisionImage image)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "shc images|*.jpg";
            saveFileDialog1.Title = "Save Image";
            String dataPath = (string)Environs.FileSystem.Paths["dataPath"];
            String dataStoreDir = dataPath + "SHC Single Images";
            saveFileDialog1.InitialDirectory = dataStoreDir;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    StoreImage(saveFileDialog1.FileName, image);
                }
            }
        }

        // Quietly.
        public void StoreImage(VisionImage image)
        {
            String dataPath = (string)Environs.FileSystem.Paths["dataPath"];
            String dataStoreFilePath = dataPath + "\\SHC Single Images\\tempImage.jpg";
            StoreImage(dataStoreFilePath, image);
        }



        public void StoreImage(String dataStoreFilePath, VisionImage image)
        {
            image.WriteJpegFile(dataStoreFilePath);
        }

        //Load image when opening the controller
        public void LoadImagesWithDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "shc images|*.jpg";
            dialog.Title = "Load Image";
            String dataPath = (string)Environs.FileSystem.Paths["dataPath"];
            String dataStoreDir = dataPath + "SHC Single Images";
            dialog.InitialDirectory = dataStoreDir;
            dialog.ShowDialog();
            if (dialog.FileName != "") LoadImage(dialog.FileName);
        }

        private void LoadImage()
        {
            String dataPath = (string)Environs.FileSystem.Paths["dataPath"];
            String dataStoreFilePath = dataPath + "\\SHC Single Images\\tempImage.jpg";
            LoadImage(dataStoreFilePath);
        }

        private void LoadImage(String dataStoreFilePath)
        {
            VisionImage image = new VisionImage();
            image.ReadFile(dataStoreFilePath);
            window.AttachToViewer(window.motViewer, image);
        }



        #endregion

        #region Public properties for controlling the panel. 
        //This gets/sets the values on the GUI panel
        
       
        //This is a set of properties for controlling an aom

        public bool SHCUIControl
        {
            get
            {
                return sHCUIControl;
            }
            set
            {
                sHCUIControl = value;
            }
        }

        public bool CameraBusy
        {
            get
            {
                return cameraBusy;
            }
            set
            {
                cameraBusy = value;
            }
        }


        public bool Aom0Enabled
        {
            get
            {
                return window.aom0CheckBox.Checked;
            }
            set
            {
                window.SetCheckBox(window.aom0CheckBox, value);
            }
        }
        public double Aom0rfAmplitude
        {
            get
            {
                return Double.Parse(window.aom0rfAmplitudeTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.aom0rfAmplitudeTextBox, Convert.ToString(value));
            }
        }
        public double Aom0rfFrequency
        {
            get
            {
                return Double.Parse(window.aom0rfFrequencyTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.aom0rfFrequencyTextBox, Convert.ToString(value));
            }
        }



        public bool Aom1Enabled
        {
            get
            {
                return window.aom1CheckBox.Checked;
            }
            set
            {
                window.SetCheckBox(window.aom1CheckBox, value);
            }
        }
        public double Aom1rfAmplitude
        {
            get
            {
                return Double.Parse(window.aom1rfAmplitudeTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.aom1rfAmplitudeTextBox, Convert.ToString(value));
            }
        }
        public double Aom1rfFrequency
        {
            get
            {
                return Double.Parse(window.aom1rfFrequencyTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.aom1rfFrequencyTextBox, Convert.ToString(value));
            }
        }



        public bool Aom2Enabled
        {
            get
            {
                return window.aom2CheckBox.Checked;
            }
            set
            {
                window.SetCheckBox(window.aom2CheckBox, value);
            }
        }
        public double Aom2rfAmplitude
        {
            get
            {
                return Double.Parse(window.aom2rfAmplitudeTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.aom2rfAmplitudeTextBox, Convert.ToString(value));
            }
        }
        
        public double Aom2rfFrequency
        {
            get
            {
                return Double.Parse(window.aom2rfFrequencyTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.aom2rfFrequencyTextBox, Convert.ToString(value));
            }
        }

        public bool Aom3Enabled
        {
            get
            {
                return window.aom3CheckBox.Checked;
            }
            set
            {
                window.SetCheckBox(window.aom3CheckBox, value);
            }
        }
        public double Aom3rfAmplitude
        {
            get
            {
                return Double.Parse(window.aom3rfAmplitudeTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.aom3rfAmplitudeTextBox, Convert.ToString(value));
            }
        }
        
        public double Aom3rfFrequency
        {
            get
            {
                return Double.Parse(window.aom3rfFrequencyTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.aom3rfFrequencyTextBox, Convert.ToString(value));
            }
        }

        public double Coil0Current
        {
            get
            {
                return Double.Parse(window.coil0CurrentTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.coil0CurrentTextBox, Convert.ToString(value));
            }
        }
        public double Coil1Current
        {
            get
            {
                return Double.Parse(window.coil1CurrentTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.coil1CurrentTextBox, Convert.ToString(value));
            }
        }
        //streaming video bool
        private bool streaming;
        public bool Streaming
        {
            get
            {
                return this.streaming;
            }
            set
            {
                this.streaming = value;
            }
        }
        
        //camera session
        private ImaqdxSession cameraSession;
        public ImaqdxSession CameraSession
        {
            get
            {
                return cameraSession;
            }
            set
            {
                cameraSession = value;
            }
        }
        
        #endregion

        #region Public properties for monitoring the hardware
        //This gets the values on the GUI panel
        /*
        public double BCurrent00
        {
            get
            {
                return Double.Parse(window.bCurrent00TextBox.Text);
            }
        }
         */

        #endregion

        #region Hardware control methods - safe for remote
        //This turns the aom on and off.

        public void UpdateAOM0(bool ttl, double amp, double freq)
        {
            SetDigitalLine("aom0Enable", ttl);
            SetAnalogOutput(aom0rfAmplitudeTask, amp);
            SetAnalogOutput(aom0rfFrequencyTask, freq);
            window.SetLED(window.aom0LED, ttl);
        }

        public void UpdateAOM1(bool ttl, double amp, double freq)
        {
            SetDigitalLine("aom1Enable", ttl);
            SetAnalogOutput(aom1rfAmplitudeTask, amp);
            SetAnalogOutput(aom1rfFrequencyTask, freq);
            window.SetLED(window.aom1LED, ttl);
        }

        public void UpdateAOM2(bool ttl, double amp, double freq)
        {
            SetDigitalLine("aom2Enable", ttl);
            SetAnalogOutput(aom2rfAmplitudeTask, amp);
            SetAnalogOutput(aom2rfFrequencyTask, freq);
            window.SetLED(window.aom2LED, ttl);
        }

        public void UpdateAOM3(bool ttl, double amp, double freq)
        {
            SetDigitalLine("aom3Enable", ttl);
            SetAnalogOutput(aom3rfAmplitudeTask, amp);
            SetAnalogOutput(aom3rfFrequencyTask, freq);
            window.SetLED(window.aom3LED, ttl);
        }
        public void UpdateCoil0(double current)
        {
            SetAnalogOutput(coil0CurrentTask, current);
        }
        public void UpdateCoil1(double current)
        {
            SetAnalogOutput(coil1CurrentTask, current);
        }

        public void StopManualControl()
        {
            UpdateAOM0(false, 0.0, 0.0);
            UpdateAOM1(false, 0.0, 0.0);
            UpdateAOM2(false, 0.0, 0.0);
            UpdateAOM3(false, 0.0, 0.0);
            UpdateCoil0(0.0);
            UpdateCoil1(0.0);
            window.SetLED(window.manualControlLED, false);
            this.SHCUIControl = false;
        }
        public void StartManualControl()
        {
            UpdateAOM0(Aom0Enabled, Aom0rfAmplitude, Aom0rfFrequency);
            UpdateAOM1(Aom1Enabled, Aom1rfAmplitude, Aom1rfFrequency);
            UpdateAOM2(Aom2Enabled, Aom2rfAmplitude, Aom2rfFrequency);
            UpdateAOM3(Aom3Enabled, Aom3rfAmplitude, Aom3rfFrequency);
            UpdateCoil0(Coil0Current);
            UpdateCoil1(Coil1Current);
            window.SetLED(window.manualControlLED, true);
            this.SHCUIControl = true;


        }
        #endregion

        #region Camera Control
        //camera stuff

        //untriggered single shot commands

        public void CameraSnapshot()
        {
            VisionImage image = new VisionImage();
            ImaqdxSession session = new ImaqdxSession(motCamera);
            session.Snap(image);
            session.Close();
            
            if (window.saveImageCheckBox.Checked == true)
            {
                StoreImage(image);
            }
            window.AttachToViewer(window.motViewer, image);

        }

        public void CameraSnapshot(string dataStoreFilePath)
        {
            VisionImage image = new VisionImage();
            ImaqdxSession session = new ImaqdxSession(motCamera);
            session.Snap(image);
            session.Close();
            
            if (window.saveImageCheckBox.Checked == true)
            {
                StoreImage(dataStoreFilePath, image);
            }
            
            window.AttachToViewer(window.motViewer, image);
        }
        
        //triggered snapshot
       
        public void ManualCameraSnapshot()
        {
            VisionImage image = new VisionImage();
            SetupCameraSession(cameraAttributesFile);
            CameraSession.Acquisition.Configure(ImaqdxAcquisitionType.SingleShot, 1);
            CameraSession.Acquisition.Start();
            CameraSession.Acquisition.GetLastImage(image);
            CameraSession.Acquisition.Stop();
            CloseCameraSession(CameraSession);
            if (window.saveImageCheckBox.Checked == true)
            {
                StoreImage(image);
            }
            window.AttachToViewer(window.motViewer, image);
        }
        public void ManualCameraSnapshot(string dataStoreFilePath)
        {
            VisionImage image = new VisionImage();
            SetupCameraSession(cameraAttributesFile);
            CameraSession.Acquisition.Configure(ImaqdxAcquisitionType.SingleShot, 1);
            CameraSession.Acquisition.Start();
            CameraSession.Acquisition.GetLastImage(image);
            CameraSession.Acquisition.Stop();
            CloseCameraSession(CameraSession);
            if (window.saveImageCheckBox.Checked == true)
            {
                StoreImage(dataStoreFilePath,image);
            }
            window.AttachToViewer(window.motViewer, image);
        }

        //If there is no camera session, it sets one up using the attributes file.
        public void SetupCameraSession(string attributesFile)
        {
            if (CameraBusy == false)
            {
                CameraBusy = true;
                ImaqdxSession session = new ImaqdxSession(motCamera);
                String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
                session.Attributes.ReadAttributesFromFile(settingsPath + "SympatheticHardwareController\\" + cameraAttributesFile);
                CameraSession = session;
            }
        }
        //Shutdown
        public void CloseCameraSession(ImaqdxSession session)
        {
            session.Acquisition.Unconfigure();
            session.Close();
           // CameraSession = session;
            CameraBusy = false;

        }

        
        //streaming video
        public object streamStopLock = new object();
        public void CameraStream()
        {
            Thread streamThread = new Thread(new ThreadStart(cameraStream));
            streamThread.Start();
        }
        private void cameraStream()
        {
            this.Streaming = true;
            VisionImage image = new VisionImage();
            /*ImaqdxSession session = new ImaqdxSession(motCamera);
            session.ConfigureGrab();
            window.AttachToViewer(window.motViewer, image);
            for(;;)
            {
                session.Grab(image, true);
                window.UpdateViewer(window.motViewer);

                lock(streamStopLock)
                {
                if(Streaming == false)
                    {
                        session.Close();
                        return;
                    }
                }
                
            }*/
            if (CameraBusy == false)
            {
                SetupCameraSession(cameraAttributesFile);
                CameraSession.ConfigureGrab();
                window.AttachToViewer(window.motViewer, image);
                for (; ; )
                {
                    CameraSession.Grab(image, true);
                    window.UpdateViewer(window.motViewer);

                    lock (streamStopLock)
                    {
                        if (Streaming == false)
                        {
                            CloseCameraSession(CameraSession);
                            return;
                        }
                    }

                }
            }
            
        }
        
        
        #endregion

        



    }
}
