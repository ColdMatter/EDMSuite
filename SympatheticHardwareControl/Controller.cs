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
    public class Controller : MarshalByRefObject, CameraControlable, HardwareReportable
    {
        #region Constants
        //Put any constants and stuff here

        private static string internalProfilesPath = (string)Environs.FileSystem.Paths["settingsPath"]
            + "\\SympatheticHardwareController\\internalProfiles\\";
        private static string cameraAttributesPath = (string)Environs.FileSystem.Paths["UntriggeredCameraAttributesPath"];
        private static string profilesPath = (string)Environs.FileSystem.Paths["settingsPath"]
            + "\\SympatheticHardwareController\\";

        private static Hashtable calibrations = Environs.Hardware.Calibrations;
        #endregion

        #region Setup



        // table of all digital tasks
        Hashtable digitalTasks = new Hashtable();
        public string p = cameraAttributesPath;
        //Cameras
        IMAQdxCameraControl cam0Control;

        // list Hardware (boards on computer are already known!?)
        //e.g.  HP8657ASynth greenSynth = (HP8657ASynth)Environs.Hardware.GPIBInstruments["green"];
        //      Synth redSynth = (Synth)Environs.Hardware.GPIBInstruments["red"];
        //      ICS4861A voltageController = (ICS4861A)Environs.Hardware.GPIBInstruments["4861"];

        //Hardware Control Tasks:
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

        //Hardware Monitor Tasks;
        Task laserLockErrorSignalMonitorTask;
        Task chamber1PressureMonitorTask;


        // Declare that there will be a controlWindow
        ControlWindow controlWindow;
        ImageViewerWindow imageWindow;
        HardwareMonitorWindow monitorWindow;

        //private bool sHCUIControl;
        public enum SHCUIControlState { OFF, LOCAL, REMOTE };
        public SHCUIControlState HCState = new SHCUIControlState();

        private DataStore dataStore = new DataStore();
        private class cameraNotFoundException : ArgumentException { };

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

            laserLockErrorSignalMonitorTask = CreateAnalogInputTask("laserLockErrorSignal", -10, 10);
            chamber1PressureMonitorTask = CreateAnalogInputTask("chamber1Pressure");

            // make analog input tasks. "CreateAnalogInputTask" is defined later
            //e.g   probeMonitorInputTask = CreateAnalogInputTask("probePD", 0, 5);
            //      pumpMonitorInputTask = CreateAnalogInputTask("pumpPD", 0, 5);

            //readAnalogVoltageTask = CreateAnalogInputTask("testAI");
            // make the control controlWindow
            controlWindow = new ControlWindow();
            controlWindow.controller = this;

            imageWindow = new ImageViewerWindow();
            imageWindow.controller = this;




            HCState = SHCUIControlState.OFF;

            // run
            //Application.Run(imageWindow);
            //Application.Run(controlWindow);

            Application.Run(controlWindow);

        }

        // this method runs immediately after the GUI sets up
        internal void WindowLoaded()
        {
            try
            {
                cam0Control = new IMAQdxCameraControl("cam0", cameraAttributesPath);
                cam0Control.InitializeCamera();
                imageWindow.Show();
            }
            catch (ImaqdxException e)
            {
                MessageBox.Show(e.Message, "Camera Initialization Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();

            }
            // things like loading saved parameters, checking status of experiment etc. should go here.
            LoadParameters(internalProfilesPath + "OffState.bin");

        }

        public void Stop()
        {
            // things like saving parameters, turning things off before quitting the program should go here
            StoreParameters();
            StopCameraStream();
            try
            {
                cam0Control.CloseCamera();
            }
            catch { }
            Application.Exit();
        }
        public void OpenNewHardwareMonitorWindow()
        {
            monitorWindow = new HardwareMonitorWindow();
            monitorWindow.controller = this;
            monitorWindow.Show();
        }
        #endregion

        #region private methods for creating un-timed Tasks/channels
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

        #region keeping track of the things on this controller!
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

        // Quietly.
        public void StoreParameters()
        {
            StoreParameters(profilesPath + "parameters.bin");
        }



        public void StoreParameters(String dataStoreFilePath)
        {
            BinaryFormatter s = new BinaryFormatter();
            FileStream fs = new FileStream(dataStoreFilePath, FileMode.Create);
            try
            {
                s.Serialize(fs, dataStore);
            }
            catch (Exception)
            {
                Console.Out.WriteLine("Saving failed");
            }
            finally
            {
                fs.Close();
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
            if (dialog.FileName != "") LoadParameters(dialog.FileName);
        }

        private void LoadParameters(String dataStoreFilePath)
        {
            // deserialize
            BinaryFormatter s = new BinaryFormatter();
            FileStream fs = new FileStream(dataStoreFilePath, FileMode.Open);
            // eat any errors in the following, as it's just a convenience function
            try
            {
                dataStore = (DataStore)s.Deserialize(fs);

                // copy parameters out of the struct
                //e.g   CPlusVoltage = dataStore.cPlus;
                //e.g   CMinusVoltage = dataStore.cMinus;
                setUIAom0EnabledState(dataStore.aom0Enabled);
                setUIAom0rfAmplitude(dataStore.aom0rfAmplitude);
                setUIAom0rfFrequency(dataStore.aom0rfFrequency);
                setUIAom1EnabledState(dataStore.aom1Enabled);
                setUIAom1rfAmplitude(dataStore.aom1rfAmplitude);
                setUIAom1rfFrequency(dataStore.aom1rfFrequency);
                setUIAom2EnabledState(dataStore.aom2Enabled);
                setUIAom2rfAmplitude(dataStore.aom2rfAmplitude);
                setUIAom2rfFrequency(dataStore.aom2rfFrequency);
                setUIAom3EnabledState(dataStore.aom3Enabled);
                setUIAom3rfAmplitude(dataStore.aom3rfAmplitude);
                setUIAom3rfFrequency(dataStore.aom3rfFrequency);
                setUICoil0Current(dataStore.coil0Current);
                setUICoil1Current(dataStore.coil1Current);

            }
            catch (Exception)
            { Console.Out.WriteLine("Unable to load settings"); }
            finally
            {
                fs.Close();
            }
        }
        #endregion

        #region Controlling hardware and UI.
        //This gets/sets the values on the GUI panel

        public bool ReadAndApplyUIAom0EnabledState()
        {
            bool value = controlWindow.ReadAom0EnabledState();
            dataStore.aom0Enabled = value;
            SetDigitalLine("aom0Enable", value);
            return value;
        }
        private void setUIAom0EnabledState(bool value)
        {
            controlWindow.SetAom0EnabledState(value);
        }

        public double ReadAndApplyUIAom0rfAmplitude()
        {
            double value = controlWindow.ReadAom0rfAmplitude();
            dataStore.aom0rfAmplitude = value;
            SetAnalogOutput(aom0rfAmplitudeTask, value);
            return value;
        }
        private void setUIAom0rfAmplitude(double value)
        {
            controlWindow.SetAom0rfAmplitude(value);
        }

        public double ReadAndApplyUIAom0rfFrequency()
        {
            double value = controlWindow.ReadAom0rfFrequency();
            dataStore.aom0rfFrequency = value;
            SetAnalogOutput(aom0rfFrequencyTask, value);
            return value;
        }
        private void setUIAom0rfFrequency(double value)
        {
            controlWindow.SetAom0rfFrequency(value);
        }

        ///

        public bool ReadAndApplyUIAom1EnabledState()
        {
            bool value = controlWindow.ReadAom1EnabledState();
            dataStore.aom1Enabled = value;
            SetDigitalLine("aom1Enable", value);
            return value;
        }
        private void setUIAom1EnabledState(bool value)
        {
            controlWindow.SetAom1EnabledState(value);
        }

        public double ReadAndApplyUIAom1rfAmplitude()
        {
            double value = controlWindow.ReadAom1rfAmplitude();
            dataStore.aom1rfAmplitude = value;
            SetAnalogOutput(aom1rfAmplitudeTask, value);
            return value;
        }
        private void setUIAom1rfAmplitude(double value)
        {
            controlWindow.SetAom1rfAmplitude(value);
        }

        public double ReadAndApplyUIAom1rfFrequency()
        {
            double value = controlWindow.ReadAom1rfFrequency();
            dataStore.aom1rfFrequency = value;
            SetAnalogOutput(aom1rfFrequencyTask, value);
            return value;
        }
        private void setUIAom1rfFrequency(double value)
        {
            controlWindow.SetAom1rfFrequency(value);
        }

        ///

        public bool ReadAndApplyUIAom2EnabledState()
        {
            bool value = controlWindow.ReadAom2EnabledState();
            SetDigitalLine("aom2Enable", value);
            dataStore.aom2Enabled = value;
            return value;
        }
        private void setUIAom2EnabledState(bool value)
        {
            controlWindow.SetAom2EnabledState(value);
        }

        public double ReadAndApplyUIAom2rfAmplitude()
        {
            double value = controlWindow.ReadAom2rfAmplitude();
            dataStore.aom2rfAmplitude = value;
            SetAnalogOutput(aom2rfAmplitudeTask, value);
            return value;
        }
        private void setUIAom2rfAmplitude(double value)
        {
            controlWindow.SetAom2rfAmplitude(value);
        }

        public double ReadAndApplyUIAom2rfFrequency()
        {
            double value = controlWindow.ReadAom2rfFrequency();
            dataStore.aom2rfFrequency = value;
            SetAnalogOutput(aom2rfFrequencyTask, value);
            return value;
        }
        private void setUIAom2rfFrequency(double value)
        {
            controlWindow.SetAom2rfFrequency(value);
        }

        ///

        public bool ReadAndApplyUIAom3EnabledState()
        {
            bool value = controlWindow.ReadAom3EnabledState();
            dataStore.aom3Enabled = value;
            SetDigitalLine("aom3Enable", value);
            return value;
        }
        private void setUIAom3EnabledState(bool value)
        {
            controlWindow.SetAom3EnabledState(value);
        }

        public double ReadAndApplyUIAom3rfAmplitude()
        {
            double value = controlWindow.ReadAom3rfAmplitude();
            dataStore.aom3rfAmplitude = value;
            SetAnalogOutput(aom3rfAmplitudeTask, value);
            return value;
        }
        private void setUIAom3rfAmplitude(double value)
        {
            controlWindow.SetAom3rfAmplitude(value);
        }

        public double ReadAndApplyUIAom3rfFrequency()
        {
            double value = controlWindow.ReadAom3rfFrequency();
            dataStore.aom3rfFrequency = value;
            SetAnalogOutput(aom3rfFrequencyTask, value);
            return value;
        }
        private void setUIAom3rfFrequency(double value)
        {
            controlWindow.SetAom3rfFrequency(value);
        }

        public double ReadAndApplyUICoil0Current()
        {
            double value = controlWindow.ReadCoil0Current();
            dataStore.coil0Current = value;
            SetAnalogOutput(coil0CurrentTask, value);
            return value;
        }
        private void setUICoil0Current(double value)
        {
            controlWindow.SetCoil0Current(value);
        }

        public double ReadAndApplyUICoil1Current()
        {
            double value = controlWindow.ReadCoil1Current();
            dataStore.coil1Current = value;
            SetAnalogOutput(coil1CurrentTask, value);
            return value;
        }
        private void setUICoil1Current(double value)
        {
            controlWindow.SetCoil1Current(value);
        }

        #endregion

        #region Manging the controller's state

        public void StopManualControl()
        {
            stopAll();
            HCState = SHCUIControlState.OFF;
            controlWindow.UpdateUIState(HCState);
        }

        public void StartManualControl()
        {
            if (HCState == SHCUIControlState.OFF)
            {
                HCState = SHCUIControlState.LOCAL;
                controlWindow.UpdateUIState(HCState);
                refreshHardwareState();
            }
            else
            {
                Console.Out.WriteLine("Controller is currently busy.");
            }
        }
        public void StartManualControlUsingLastSavedValues()
        {
            if (HCState == SHCUIControlState.OFF)
            {
                HCState = SHCUIControlState.LOCAL;
                controlWindow.UpdateUIState(HCState);
                LoadLastSavedParameterValues();
            }
            else
            {
                Console.Out.WriteLine("Controller is currently busy.");
            }
        }
        public void LoadLastSavedParameterValues()
        {
            applyState(internalProfilesPath + "LastHardwareState.bin");
        }

        private void refreshHardwareState()
        {
            ReadAndApplyUIAom0EnabledState();
            ReadAndApplyUIAom0rfAmplitude();
            ReadAndApplyUIAom0rfFrequency();
            ReadAndApplyUIAom1EnabledState();
            ReadAndApplyUIAom1rfAmplitude();
            ReadAndApplyUIAom1rfFrequency();
            ReadAndApplyUIAom2EnabledState();
            ReadAndApplyUIAom2rfAmplitude();
            ReadAndApplyUIAom2rfFrequency();
            ReadAndApplyUIAom3EnabledState();
            ReadAndApplyUIAom3rfAmplitude();
            ReadAndApplyUIAom3rfFrequency();
            ReadAndApplyUICoil0Current();
            ReadAndApplyUICoil1Current();
        }


        private void stopAll()
        {
            StoreParameters(internalProfilesPath + "LastHardwareState.bin");
            LoadParameters(internalProfilesPath + "OffState.bin");
            refreshHardwareState();

        }
        private void applyState(String dataStoreFilePath)
        {
            LoadParameters(dataStoreFilePath);
            refreshHardwareState();
        }

        /// <summary>
        /// Note: It saves the parameter values and the HCstate before moving to remote mode. when returning to manual control, these values are
        /// brought back.
        /// </summary>
        private SHCUIControlState preRemotingState;
        public void StartRemoteControl()
        {
            preRemotingState = HCState;
            if (streaming)
            {
                StopCameraStream();
            }
            StoreParameters(internalProfilesPath + "tempParameters.bin");
            HCState = SHCUIControlState.REMOTE;
            controlWindow.UpdateUIState(HCState);

        }
        public void StopRemoteControl()
        {
            try
            {
                applyState(internalProfilesPath + "tempParameters.bin");
                if (System.IO.File.Exists(internalProfilesPath + "tempParameters.bin"))
                {
                    System.IO.File.Delete(internalProfilesPath + "tempParameters.bin");
                }
            }
            catch (Exception)
            {
                Console.Out.WriteLine("Unable to load Parameters.");
            }
            HCState = preRemotingState;
            controlWindow.UpdateUIState(HCState);
        }
        #endregion

        //camera stuff

        #region Testing the camera
        //untriggered single shot commands. This just starts a new Session, takes one image then closes the Session.
        //Avoid using these. I think, there should only be a single Session per camera for the entire time the program is running.
        //I wrote these to test the camera.
        //Cameras
        /* public const string motCamera = "cam0";
        
         public void CameraSnapshot()
         {
             VisionImage image = new VisionImage();
             ImaqdxSession Session = new ImaqdxSession(motCamera);
             Session.Snap(image);
             Session.Close();
            
             if (controlWindow.saveImageCheckBox.Checked == true)
             {
                 StoreImage(image);
             }
             controlWindow.AttachToViewer(controlWindow.motViewer, image);

         }

         public void CameraSnapshot(string dataStoreFilePath)
         {
             VisionImage image = new VisionImage();
             ImaqdxSession Session = new ImaqdxSession(motCamera);
             Session.Snap(image);
             Session.Close();
            
             if (controlWindow.saveImageCheckBox.Checked == true)
             {
                 StoreImage(dataStoreFilePath, image);
             }
            
             controlWindow.AttachToViewer(controlWindow.motViewer, image);
         }
         //streaming video
         public object streamStopLock = new object();
         public void CameraStream()
         {
             Thread streamThread = new Thread(new ThreadStart(streamAndDisplay));
             streamThread.Start();
         }
         private void streamAndDisplay()
         {
             this.Streaming = true;
             VisionImage image = new VisionImage();
             ImaqdxSession Session = new ImaqdxSession(motCamera);
             Session.ConfigureGrab();
             controlWindow.AttachToViewer(controlWindow.motViewer, image);
             for (; ; )
             {
                 Session.Grab(image, true);
                 controlWindow.UpdateViewer(controlWindow.motViewer);

                 lock (streamStopLock)
                 {
                     if (Streaming == false)
                     {
                         Session.Close();
                         return;
                     }
                 }

             }
         }*/
        #endregion

        #region Camera control

        public object streamStopLock = new object();
        public void CameraStream()
        {
            streaming = true;
            Thread streamThread = new Thread(new ThreadStart(streamAndDisplay));
            streamThread.Start();
        }
        public void CameraSnapshot()
        {
            Thread cameraThread = new Thread(new ThreadStart(takeSnapshotAndDisplay));
            cameraThread.Start();
        }

        private void takeSnapshotAndDisplay()
        {
            VisionImage image = new VisionImage();
            cam0Control.Session.Snap(image);
            imageWindow.Image = image;
        }

        private void streamAndDisplay()
        {
            VisionImage image = new VisionImage();
            cam0Control.Session.ConfigureGrab();
            for (; ; )
            {
                try
                {
                    cam0Control.Session.Grab(image, true);
                }
                catch (ImaqdxException e)
                { MessageBox.Show("You're probably already streaming...\n" + e.Message); }
                catch (InvalidOperationException e)
                {
                    MessageBox.Show("Something bad happened. Stopping the image stream.\n" + e.Message); 
                    streaming = false;
                }
                lock (streamStopLock)
                {
                    imageWindow.Image = image;
                    if (!streaming)
                    {
                        return;
                    }
                }

            }
        }
        private bool streaming = false;
        public void StopCameraStream()
        {
            streaming = false;
        }

        public void SetCameraAttributes()
        {
            cam0Control.SetCameraAttributes();
        }

        #endregion

        #region Saving and Loading Images

        private ImageFileIOManager imageFileIO = new ImageFileIOManager();
        public void SaveImageWithDialog()
        {
            imageFileIO.SaveImageWithDialog(imageWindow.Image);
        }
        public void LoadImagesWithDialog()
        {
            imageWindow.Image = imageFileIO.LoadImagesWithDialog();
        }

        #endregion

        #region Remote Image Processing
        //Written for taking images triggered by TTL. This "Arm" sets the camera so it's expecting a TTL.
        private void armCameraAndWait(VisionImage image, string cameraAttributesPath)
        {
            cam0Control.SetCameraAttributes(cameraAttributesPath);
            cam0Control.Session.Snap(image);
        }
        public byte[,] GrabImage(string cameraAttributesPath)
        {

            isDone = false;
            VisionImage image = new VisionImage();
            armCameraAndWait(image, cameraAttributesPath);
            imageWindow.Image = image;
            PixelValue2D pval = image.ImageToArray();
            isDone = true;
            return pval.U8;
        }

        private bool isDone;
        public bool IsDone()
        {
            return isDone;
        }
        public bool PrepareRemoteCameraControl()
        {
            StartRemoteControl();
            return true;
        }
        public bool FinishRemoteCameraControl()
        {
            StopRemoteControl();
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
                es = ReadAnalogInput(laserLockErrorSignalMonitorTask);
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
                double voltage = ReadChannel1Pressure();                
                lock (c1pStopLock)
                {
                    double pressure = 
                        ((Calibration)calibrations["chamber1Pressure"]).Convert(voltage);
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
                value = ReadAnalogInput(chamber1PressureMonitorTask);
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
            return report;
        }
        #endregion

        #endregion
    }
}
