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

namespace EDMHardwareControl
{
    /// <summary>
    /// This is the interface to the edm specific hardware.
    /// 
    /// Everything is just bundled into a single
    /// class. The methods/properties are grouped: the first set change the state of the hardware, these
    /// usually act immediately, but sometimes you need to call an update method. Read the code to find out
    /// which are which. The second set of methods read out the state of the hardware. These invariably need
    /// to be brought up to date with an update method before use.
    /// </summary>
    public class Controller : MarshalByRefObject
    {
        #region Constants
        private const double greenSynthOffAmplitude = -130.0;
        private const double redSynthOffFrequency = 36.0;
        private const int eDischargeTime = 5000;
        private const int eBleedTime = 1000;
        private const int eWaitTime = 500;
        private const int eChargeTime = 5000;
        // E field monitor scale factors - what you need to multiply the monitor voltage by
        // to get the plate voltage
        public double CPlusMonitorScale { get { return 10000; } }
        public double CMinusMonitorScale { get { return 10000; } }
        // E field controller mode
        /*private enum EFieldMode { TTL, GPIB };
        private EFieldMode eFieldMode = EFieldMode.TTL;*/
        //Current Leakage Monitor calibration 
        //Convention for monitor to plate mapping:
        //north -> monitor1
        //south -> monitor2
        private static double northVolt2FreqSlope = 0.025425;
        private static double southVolt2FreqSlope = 0.0255023;
        private static double northFreq2AmpSlope = 0.2;
        private static double southFreq2AmpSlope = 0.2;
        private static double northOffset = 0;
        private static double southOffset = 0;
        private static double currentMonitorMeasurementTime = 0.01;

        
        #endregion

        #region Setup

        // hardware
        HP8657ASynth greenSynth = (HP8657ASynth)Environs.Hardware.Instruments["green"];
        Synth redSynth = (Synth)Environs.Hardware.Instruments["red"];
        ICS4861A voltageController = (ICS4861A)Environs.Hardware.Instruments["4861"];
        HP34401A bCurrentMeter = (HP34401A)Environs.Hardware.Instruments["bCurrentMeter"];
        Agilent53131A rfCounter = (Agilent53131A)Environs.Hardware.Instruments["rfCounter"];
        Agilent53131A rfCounter2 = (Agilent53131A)Environs.Hardware.Instruments["rfCounter2"];
        HP438A rfPower = (HP438A)Environs.Hardware.Instruments["rfPower"];
        Hashtable digitalTasks = new Hashtable();
        Hashtable digitalInputTasks = new Hashtable();
        //LeakageMonitor northLeakageMonitor =
        //   new LeakageMonitor((CounterChannel)Environs.Hardware.CounterChannels["northLeakage"], northSlope, northOffset, currentMonitorMeasurementTime);
        //LeakageMonitor southLeakageMonitor =
        //    new LeakageMonitor((CounterChannel)Environs.Hardware.CounterChannels["southLeakage"], southSlope, southOffset, currentMonitorMeasurementTime);
        LeakageMonitor northLeakageMonitor = new LeakageMonitor("northLeakage", northVolt2FreqSlope, northFreq2AmpSlope, northOffset);
        LeakageMonitor southLeakageMonitor = new LeakageMonitor("southLeakage", southVolt2FreqSlope, southFreq2AmpSlope, southOffset);
        BrilliantLaser yag = (BrilliantLaser)Environs.Hardware.YAG;
        Task bBoxAnalogOutputTask;
        Task steppingBBiasAnalogOutputTask;
        Task flPZTVAnalogOutputTask;
        Task rf1AttenuatorOutputTask;
        Task rf2AttenuatorOutputTask;
        Task rf1FMOutputTask;
        Task rf2FMOutputTask;
        Task probeMonitorInputTask;
        Task pumpMonitorInputTask;
        Task cPlusOutputTask;
        Task cMinusOutputTask;
        Task cPlusMonitorInputTask;
        Task cMinusMonitorInputTask;
        //Task rfPowerMonitorInputTask;
        Task phaseScramblerVoltageOutputTask;
        Task miniFlux1MonitorInputTask;
        Task miniFlux2MonitorInputTask;
        Task miniFlux3MonitorInputTask;
        Task piMonitorTask;
        Task diodeRefCavInputTask;
        Task diodeCurrentMonInputTask;
        Task diodeRefCavOutputTask;
        Task fibreAmpOutputTask;

        AxMG17MotorLib.AxMG17Motor motorController1;
        AxMG17MotorLib.AxMG17Motor motorController2;

        ControlWindow window;

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }


        public void Start()
        {
            // make the digital tasks
            //CreateDigitalTask("notEOnOff");
            //CreateDigitalTask("eOnOff");
            CreateDigitalTask("ePol");
            CreateDigitalTask("notEPol");
            CreateDigitalTask("eBleed");
            CreateDigitalTask("rfSwitch");
            CreateDigitalTask("fmSelect");
            CreateDigitalTask("attenuatorSelect");
            CreateDigitalTask("scramblerEnable");
            CreateDigitalTask("b");
            CreateDigitalTask("notDB");
            CreateDigitalTask("piFlip");
            CreateDigitalTask("piFlipEnable");
            CreateDigitalTask("notPIFlipEnable");
            CreateDigitalTask("pumpShutter");
            CreateDigitalTask("probeShutter");
            CreateDigitalTask("argonShutter");
            CreateDigitalTask("targetStepper");
            CreateDigitalTask("rfCountSwBit1");
            CreateDigitalTask("rfCountSwBit2");
            CreateDigitalTask("fibreAmpEnable");
            CreateDigitalTask("ttlSwitch");

            // digitial input tasks
            CreateDigitalInputTask("fibreAmpMasterErr");
            CreateDigitalInputTask("fibreAmpSeedErr");
            CreateDigitalInputTask("fibreAmpBackFeflectErr");
            CreateDigitalInputTask("fibreAmpTempErr");
            CreateDigitalInputTask("fibreAmpPowerSupplyErr");

            // initialise the current leakage monitors
            northLeakageMonitor.Initialize();
            southLeakageMonitor.Initialize();


            // analog outputs
            bBoxAnalogOutputTask = CreateAnalogOutputTask("b");
            steppingBBiasAnalogOutputTask = CreateAnalogOutputTask("steppingBBias");
            flPZTVAnalogOutputTask = CreateAnalogOutputTask("flPZT");
            rf1AttenuatorOutputTask = CreateAnalogOutputTask("rf1Attenuator");
            rf2AttenuatorOutputTask = CreateAnalogOutputTask("rf2Attenuator");
            rf1FMOutputTask = CreateAnalogOutputTask("rf1FM");
            rf2FMOutputTask = CreateAnalogOutputTask("rf2FM");
            cPlusOutputTask = CreateAnalogOutputTask("cPlus");
            cMinusOutputTask = CreateAnalogOutputTask("cMinus");
            phaseScramblerVoltageOutputTask = CreateAnalogOutputTask("phaseScramblerVoltage");
            diodeRefCavOutputTask = CreateAnalogOutputTask("diodeRefCavity");
            fibreAmpOutputTask = CreateAnalogOutputTask("fibreAmpPwr");

            // analog inputs
            probeMonitorInputTask = CreateAnalogInputTask("probePD", 0, 5);
            pumpMonitorInputTask = CreateAnalogInputTask("pumpPD", 0, 5);
            cPlusMonitorInputTask = CreateAnalogInputTask("cPlusMonitor");
            cMinusMonitorInputTask = CreateAnalogInputTask("cMinusMonitor");
            //rfPowerMonitorInputTask = CreateAnalogInputTask("rfPower");
            miniFlux1MonitorInputTask = CreateAnalogInputTask("miniFlux1");
            miniFlux2MonitorInputTask = CreateAnalogInputTask("miniFlux2");
            miniFlux3MonitorInputTask = CreateAnalogInputTask("miniFlux3");
            piMonitorTask = CreateAnalogInputTask("piMonitor");
            //northLeakageInputTask = CreateAnalogInputTask("northLeakage");
            //southLeakageInputTask = CreateAnalogInputTask("southLeakage");
            diodeRefCavInputTask = CreateAnalogInputTask("diodeLaserRefCavity");
            diodeCurrentMonInputTask = CreateAnalogInputTask("diodeLaserCurrent");

            // make the control window
            window = new ControlWindow();
            window.controller = this;

            // initialise the motor controllers - this needs to be done
            // after the window is made because the ActiveX object needs
            // to live in the window.
            motorController1 = window.motorController1;
            motorController1.StartCtrl();
            motorController2 = window.motorController2;
            motorController2.StartCtrl();
            // set the velocity and acceleration to maximum.
            motorController1.SetVelParams(0, 0, 10, 14);
            motorController2.SetVelParams(0, 0, 10, 14);

            Application.Run(window);
        }

        // this method runs immediately after the GUI sets up
        internal void WindowLoaded()
        {
            // update the GPIB switcher's cached voltages
            // works around a "first-time" bug with the E-field switch
            FieldsOff();
            EBleedEnabled = false;
            /*lastGPlus = GPlusVoltage;
			lastGMinus = GMinusVoltage;
			lastCPlus = CPlusVoltage;
			lastCMinus = CMinusVoltage;*/
            // Set the leakage current monitor textboxes to the default values.
            window.SetTextBox(window.southOffsetIMonitorTextBox, southOffset.ToString());
            window.SetTextBox(window.northOffsetIMonitorTextBox, northOffset.ToString());
            window.SetTextBox(window.IMonitorMeasurementLengthTextBox, currentMonitorMeasurementTime.ToString());
            LoadParameters();
        }

        internal void WindowClosing()
        {
            StoreParameters();
        }

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
            Task task = new Task("EDMHCIn" + channel);
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
            Task task = new Task("EDMHCOut" + channel);
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

        private void CreateDigitalInputTask(String name)
        {
            Task digitalInputTask = new Task(name);
            ((DigitalInputChannel)Environs.Hardware.DigitalInputChannels[name]).AddToTask(digitalInputTask);
            digitalInputTask.Control(TaskAction.Verify);
            digitalInputTasks.Add(name, digitalInputTask);
        }

        private void SetDigitalLine(string name, bool value)
        {
            Task digitalTask = ((Task)digitalTasks[name]);
            DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(digitalTask.Stream);
            writer.WriteSingleSampleSingleLine(true, value);
            digitalTask.Control(TaskAction.Unreserve);
        }

        bool ReadDigitalLine(string name)
        {
            Task digitalInputTask = ((Task)digitalInputTasks[name]);
            DigitalSingleChannelReader reader = new DigitalSingleChannelReader(digitalInputTask.Stream);
            bool digSample = reader.ReadSingleSampleSingleLine();
            digitalInputTask.Control(TaskAction.Unreserve);
            return digSample;
        }
            

        // this isn't really very classy, but it works
        [Serializable]
        private struct DataStore
        {
            public double cPlus;
            public double cMinus;
            public double rampDownTime;
            public double rampDownDelay;
            public double bleedTime;
            public double switchTime;
            public double rampUpTime;
            public double rampUpDelay;
            public double frequency;
            public double amplitude;
            public double dcfm;
            public double rf1AttC;
            public double rf1AttS;
            public double rf2AttC;
            public double rf2AttS;
            public double rf1FMC;
            public double rf1FMS;
            public double rf2FMC;
            public double rf2FMS;
            public double steppingBias;
            public double flPZT;
            public double flPZTStep;
            public double overshootFactor;
            public double overshootHold;
        }

        public void SaveParametersWithDialog()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "edmhc parameters|*.bin";
            saveFileDialog1.Title = "Save parameters";
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String dataStoreDir = settingsPath + "EDMHardwareController";
            saveFileDialog1.InitialDirectory = dataStoreDir;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
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
            String dataStoreFilePath = settingsPath + "\\EDMHardwareController\\parameters.bin";
            StoreParameters(dataStoreFilePath);
        }

        public void StoreParameters(String dataStoreFilePath)
        {
            DataStore dataStore = new DataStore();
            // fill the struct
            dataStore.cPlus = CPlusVoltage;
            dataStore.cMinus = CMinusVoltage;
            dataStore.rampDownTime = ERampDownTime;
            dataStore.rampDownDelay = ERampDownDelay;
            dataStore.bleedTime = EBleedTime;
            dataStore.switchTime = ESwitchTime;
            dataStore.rampUpTime = ERampUpTime;
            dataStore.rampUpDelay = ERampUpDelay;
            dataStore.frequency = GreenSynthOnFrequency;
            dataStore.amplitude = GreenSynthOnAmplitude;
            dataStore.dcfm = GreenSynthDCFM;
            dataStore.rf1AttC = RF1AttCentre;
            dataStore.rf1AttS = RF1AttStep;
            dataStore.rf2AttC = RF2AttCentre;
            dataStore.rf2AttS = RF2AttStep;
            dataStore.rf1FMC = RF1FMCentre;
            dataStore.rf1FMS = RF1FMStep;
            dataStore.rf2FMC = RF2FMCentre;
            dataStore.rf2FMS = RF2FMStep;
            dataStore.steppingBias = SteppingBiasVoltage;
            dataStore.flPZT = FLPZTVoltage;
            dataStore.flPZTStep = FLPZTStep;
            dataStore.overshootFactor = EOvershootFactor;
            dataStore.overshootHold = EOvershootHold;

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
            dialog.Filter = "edmhc parameters|*.bin";
            dialog.Title = "Load parameters";
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String dataStoreDir = settingsPath + "EDMHardwareController";
            dialog.InitialDirectory = dataStoreDir;
            dialog.ShowDialog();
            if (dialog.FileName != "") LoadParameters(dialog.FileName);
        }

        private void LoadParameters()
        {
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String dataStoreFilePath = settingsPath + "\\EDMHardwareController\\parameters.bin";
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
                CPlusVoltage = dataStore.cPlus;
                CMinusVoltage = dataStore.cMinus;
                ERampDownTime = dataStore.rampDownTime;
                ERampDownDelay = dataStore.rampDownDelay;
                EBleedTime = dataStore.bleedTime;
                ESwitchTime = dataStore.switchTime;
                ERampUpTime = dataStore.rampUpTime;
                ERampUpDelay = dataStore.rampUpDelay;
                GreenSynthOnFrequency = dataStore.frequency;
                GreenSynthOnAmplitude = dataStore.amplitude;
                GreenSynthDCFM = dataStore.dcfm;
                RF1AttCentre = dataStore.rf1AttC;
                RF1AttStep = dataStore.rf1AttS;
                RF2AttCentre = dataStore.rf2AttC;
                RF2AttStep = dataStore.rf2AttS;
                RF1FMCentre = dataStore.rf1FMC;
                RF1FMStep = dataStore.rf1FMS;
                RF2FMCentre = dataStore.rf2FMC;
                RF2FMStep = dataStore.rf2FMS;
                SetSteppingBBiasVoltage(dataStore.steppingBias);
                FLPZTVoltage = dataStore.flPZT;
                FLPZTStep = dataStore.flPZTStep;
                EOvershootFactor = dataStore.overshootFactor;
                EOvershootHold = dataStore.overshootHold;

            }
            catch (Exception)
            { Console.Out.WriteLine("Unable to load settings"); }
        }

        #endregion

        #region Public properties for controlling the hardware

        public double GreenSynthOnFrequency
        {
            get
            {
                return Double.Parse(window.greenOnFreqBox.Text);
            }
            set
            {
                window.SetTextBox(window.greenOnFreqBox, value.ToString());
            }
        }

        public double GreenSynthOnAmplitude
        {
            get
            {
                return Double.Parse(window.greenOnAmpBox.Text);
            }
            set
            {
                window.SetTextBox(window.greenOnAmpBox, value.ToString());
            }
        }

        public double GreenSynthDCFM
        {
            get
            {
                return Double.Parse(window.greenDCFMBox.Text);
            }
            set
            {
                window.SetTextBox(window.greenDCFMBox, value.ToString());
            }
        }


        public bool GreenSynthEnabled
        {
            get
            {
                return window.greenOnCheck.Checked;
            }
            set
            {
                window.SetCheckBox(window.greenOnCheck, value);
            }
        }

        public bool RFSwitchEnabled
        {
            get
            {
                return window.rfSwitchEnableCheck.Checked;
            }
            set
            {
                window.SetCheckBox(window.rfSwitchEnableCheck, value);
            }
        }


        public bool GreenDCFMSelected
        {
            get
            {
                return window.fmSelectCheck.Checked;
            }
            set
            {
                window.SetCheckBox(window.fmSelectCheck, value);
            }
        }

        public bool AttenuatorSelected
        {
            get
            {
                return window.attenuatorSelectCheck.Checked;
            }
            set
            {
                window.SetCheckBox(window.attenuatorSelectCheck, value);
            }
        }       

        public bool EFieldEnabled
        {
            get
            {
                return window.eOnCheck.Checked;
            }
            set
            {
                window.SetCheckBox(window.eOnCheck, value);
            }
        }

        public void EnableEField(bool enabled)
        {
            window.SetCheckBox(window.eOnCheck, enabled);
        }

        public bool EFieldPolarity
        {
            get
            {
                return window.ePolarityCheck.Checked;
            }
            set
            {
                window.SetCheckBox(window.ePolarityCheck, value);
            }
        }

        public bool EBleedEnabled
        {
            get
            {
                return window.eBleedCheck.Checked;
            }
            set
            {
                window.SetCheckBox(window.eBleedCheck, value);
            }
        }

        public void EnableBleed(bool enabled)
        {
            window.SetCheckBox(window.eBleedCheck, enabled);
        }

        public double CPlusVoltage
        {
            get
            {
                return Double.Parse(window.cPlusTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.cPlusTextBox, value.ToString());
            }
        }

        public double CMinusVoltage
        {
            get
            {
                return Double.Parse(window.cMinusTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.cMinusTextBox, value.ToString());
            }
        }

        public double CPlusOffVoltage
        {
            get
            {
                return Double.Parse(window.cPlusOffTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.cPlusOffTextBox, value.ToString());
            }
        }

        public double CMinusOffVoltage
        {
            get
            {
                return Double.Parse(window.cMinusOffTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.cMinusOffTextBox, value.ToString());
            }
        }

        public bool CalFlipEnabled
        {
            get
            {
                return window.calFlipCheck.Checked;
            }
            set
            {
                window.SetCheckBox(window.calFlipCheck, value);
            }
        }

        public bool BFlipEnabled
        {
            get
            {
                return window.bFlipCheck.Checked;
            }
            set
            {
                window.SetCheckBox(window.bFlipCheck, value);
            }
        }

        public bool PumpShutter
        {
            get
            {
                return window.pumpShutterCheck.Checked;
            }
            set
            {
                window.SetCheckBox(window.pumpShutterCheck, value);
            }
        }

        public bool Pump2Shutter
        {
            get
            {
                return window.probeShutterCheck.Checked;
            }
            set
            {
                window.SetCheckBox(window.probeShutterCheck, value);
            }
        }

        public double I2LockAOMFrequencyCentre
        {
            get
            {
                return Double.Parse(window.I2AOMFreqCentreTextBox.Text);
            }
        }

        public double I2LockAOMFrequencyStep
        {
            get
            {
                return Double.Parse(window.I2AOMFreqStepTextBox.Text);
            }
        }

        /* This is something of a cheesy hack. It lets the edm script check to see if the YAG
          * laser has failed.
          */
        public bool YAGInterlockFailed
        {
            get
            {
                return ((BrilliantLaser)Environs.Hardware.YAG).InterlockFailed;
            }
        }

        public double ERampDownTime
        {
            get
            {
                return Double.Parse(window.eRampDownTimeTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.eRampDownTimeTextBox, value.ToString());
            }
        }

        public double ERampDownDelay
        {
            get
            {
                return Double.Parse(window.eRampDownDelayTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.eRampDownDelayTextBox, value.ToString());
            }
        }

        public double EBleedTime
        {
            get
            {
                return Double.Parse(window.eBleedTimeTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.eBleedTimeTextBox, value.ToString());
            }
        }
        public double ESwitchTime
        {
            get
            {
                return Double.Parse(window.eSwitchTimeTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.eSwitchTimeTextBox, value.ToString());
            }
        }
        public double ERampUpTime
        {
            get
            {
                return Double.Parse(window.eRampUpTimeTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.eRampUpTimeTextBox, value.ToString());
            }
        }

        public double EOvershootFactor
        {
            get
            {
                return Double.Parse(window.eOvershootFactorTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.eOvershootFactorTextBox, value.ToString());
            }
        }

        public double EOvershootHold
        {
            get
            {
                return Double.Parse(window.eOvershootHoldTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.eOvershootHoldTextBox, value.ToString());
            }
        }

        public double ERampUpDelay
        {
            get
            {
                return Double.Parse(window.eRampUpDelayTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.eRampUpDelayTextBox, value.ToString());
            }
        }

        public double RF1AttCentre
        {
            get
            {
                return Double.Parse(window.rf1AttenuatorVoltageTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rf1AttenuatorVoltageTextBox, value.ToString());
            }
        }

        public double RF1AttStep
        {
            get
            {
                return Double.Parse(window.rf1AttIncTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rf1AttIncTextBox, value.ToString());
            }
        }

        public double RF2AttCentre
        {
            get
            {
                return Double.Parse(window.rf2AttenuatorVoltageTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rf2AttenuatorVoltageTextBox, value.ToString());
            }
        }

        public double RF2AttStep
        {
            get
            {
                return Double.Parse(window.rf2AttIncTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rf2AttIncTextBox, value.ToString());
            }
        }

        public double RF1FMCentre
        {
            get
            {
                return Double.Parse(window.rf1FMVoltage.Text);
            }
            set
            {
                window.SetTextBox(window.rf1FMVoltage, value.ToString());
            }
        }

        public double RF1FMStep
        {
            get
            {
                return Double.Parse(window.rf1FMIncTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rf1FMIncTextBox, value.ToString());
            }
        }

        public double RF2FMCentre
        {
            get
            {
                return Double.Parse(window.rf2FMVoltage.Text);
            }
            set
            {
                window.SetTextBox(window.rf2FMVoltage, value.ToString());
            }
        }

        public double RF2FMStep
        {
            get
            {
                return Double.Parse(window.rf2FMIncTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rf2FMIncTextBox, value.ToString());
            }
        }

        public double FLPZTVoltage
        {
            get
            {
                return Double.Parse(window.FLPZTVTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.FLPZTVTextBox, value.ToString());
            }
        }

        public double FLPZTStep
        {
            get
            {
                return Double.Parse(window.FLPZTStepTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.FLPZTStepTextBox, value.ToString());
            }
        }

        public double diodeRefCavVoltage
        {
            get
            {
                return Double.Parse(window.diodeRefCavTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.diodeRefCavTextBox, value.ToString());
            }
        }

        public double diodeRefCavStep
        {
            get
            {
                return Double.Parse(window.diodeRefCavStepTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.diodeRefCavStepTextBox, value.ToString());
            }
        }
        public double LeakageMonitorMeasurementTime
        {
            set
            {
                window.SetTextBox(window.IMonitorMeasurementLengthTextBox, value.ToString());
            }
            get
            {
                return Double.Parse(window.IMonitorMeasurementLengthTextBox.Text);
            }
        }

        // Another annoying method to work around stupid IronPython property setting bug
        public void SetLeakageMonitorMeasurementTime(double time)
        {
            window.SetTextBox(window.IMonitorMeasurementLengthTextBox, time.ToString());
        }


        #endregion

        #region Public properties for monitoring the hardware

        public double BCurrent00
        {
            get
            {
                return Double.Parse(window.bCurrent00TextBox.Text);
            }
        }

        public double BCurrent01
        {
            get
            {
                return Double.Parse(window.bCurrent01TextBox.Text);
            }
        }

        public double BCurrent10
        {
            get
            {
                return Double.Parse(window.bCurrent10TextBox.Text);
            }
        }

        public double BCurrent11
        {
            get
            {
                return Double.Parse(window.bCurrent11TextBox.Text);
            }
        }

        public double BiasCurrent
        {
            get
            {
                return Double.Parse(window.bCurrentBiasTextBox.Text);
            }
        }

        public double SteppingBiasVoltage
        {
            get
            {
                return Double.Parse(window.steppingBBoxBiasTextBox.Text);
            }
        }

        public double FlipStepCurrent
        {
            get
            {
                return Double.Parse(window.bCurrentFlipStepTextBox.Text);
            }
        }

        public double CalStepCurrent
        {
            get
            {
                return Double.Parse(window.bCurrentCalStepTextBox.Text);
            }
        }

        public double CPlusMonitorVoltage
        {
            get
            {
                return Double.Parse(window.cPlusVMonitorTextBox.Text);
            }
        }

        public double CMinusMonitorVoltage
        {
            get
            {
                return Double.Parse(window.cMinusVMonitorTextBox.Text);
            }
        }

        public double PumpAOMFreq
        {
            get
            {
                return Double.Parse(window.PumpAOMFreqTextBox.Text);
            }
        }



        public double RF1FrequencyCentre
        {
            get
            {
                return Double.Parse(window.rf1CentreFreqMon.Text);
            }
        }

        public double RF2FrequencyCentre
        {
            get
            {
                return Double.Parse(window.rf2CentreFreqMon.Text);
            }
        }

        public double RF1FrequencyStep
        {
            get
            {
                return Double.Parse(window.rf1StepFreqMon.Text);
            }
        }

        public double RF2FrequencyStep
        {
            get
            {
                return Double.Parse(window.rf2StepFreqMon.Text);
            }
        }

        public double RF1PowerCentre
        {
            get
            {
                return Double.Parse(window.rf1CentrePowerMon.Text);
            }
        }

        public double RF2PowerCentre
        {
            get
            {
                return Double.Parse(window.rf2CentrePowerMon.Text);
            }
        }

        public double RF1PowerStep
        {
            get
            {
                return Double.Parse(window.rf1StepPowerMon.Text);
            }
        }

        public double RF2PowerStep
        {
            get
            {
                return Double.Parse(window.rf2StepPowerMon.Text);
            }
        }

        public double PiMonitorVoltage
        {
            get
            {
                return Double.Parse(window.piMonitor1TextBox.Text);
            }
        }

        public double NorthCurrent
        {
            get
            {
                return lastNorthCurrent;
            }
        }

        public double SouthCurrent
        {
            get
            {
                return lastSouthCurrent;
            }
        }

        public bool EManualState
        {
            get
            {
                return window.eManualStateCheckBox.Checked;
            }
        }

        public bool BManualState
        {
            get
            {
                return window.bManualStateCheckBox.Checked;
            }
        }

        public bool RFManualState
        {
            get
            {
                return window.rfManualStateCheckBox.Checked;
            }
        }

        public double E0PlusBoost
        {
            get
            {
                return Double.Parse(window.zeroPlusBoostTextBox.Text);
            }
        }

        #endregion

        #region Hardware control methods - safe for remote

        public void FieldsOff()
        {
            CPlusVoltage = 0;
            CMinusVoltage = 0;
            CPlusOffVoltage = 0;
            CMinusOffVoltage = 0;
            UpdateVoltages();
            EFieldEnabled = false;

        }

        private bool switchingEfield = false;
        public bool SwitchingEfields
        {
            get { return switchingEfield; }
        }

        public void SwitchE()
        {
            SwitchE(!EFieldPolarity);
        }

        public void SwitchEAndWait(bool state)
        {
            SwitchE(state);
            switchThread.Join();
        }

        public void SwitchEAndWait()
        {
            SwitchEAndWait(!EFieldPolarity);
        }


        private bool newEPolarity;
        private object switchingLock = new object();
        private Thread switchThread;
        public void SwitchE(bool state)
        {
            lock (switchingLock)
            {
                newEPolarity = state;
                switchThread = new Thread(new ThreadStart(SwitchEWorker));
                window.EnableControl(window.switchEButton, false);
                switchThread.Start();
            }
        }

        double kPositiveChargeMin = 2;
        double kPositiveChargeMax = 20;
        double kNegativeChargeMin = -2;
        double kNegativeChargeMax = -20;

        // this function switches the E field polarity with ramped turn on and off
        public void SwitchEWorker()
        {
            lock (switchingLock)
            {
                // raise flag for switching E-fields
                switchingEfield = true;
                // we always switch, even if it's into the same state.
                window.SetLED(window.switchingLED, true);
                // Add any asymmetry
                // ramp the field down
                RampVoltages(CPlusVoltage, CPlusOffVoltage, CMinusVoltage, CMinusOffVoltage, 20, ERampDownTime);
                // set as disabled
                EFieldEnabled = false;
                Thread.Sleep((int)(1000 * ERampDownDelay));
                EBleedEnabled = true;
                Thread.Sleep((int)(1000 * EBleedTime));
                EBleedEnabled = false;
                EFieldPolarity = newEPolarity;
                Thread.Sleep((int)(1000 * ESwitchTime));
                CalculateVoltages();
                // ramp the field up to the overshoot voltage
                RampVoltages(CPlusOffVoltage, EOvershootFactor * cPlusToWrite,
                                CMinusOffVoltage, EOvershootFactor * cMinusToWrite, 20, ERampUpTime);
                // impose the overshoot delay
                Thread.Sleep((int)(1000 * EOvershootHold));
                 // ramp back to the control point
                RampVoltages(EOvershootFactor * cPlusToWrite, cPlusToWrite,
                                EOvershootFactor * cMinusToWrite, cMinusToWrite, 5, 0);
                // set as enabled
                EFieldEnabled = true;
                // monitor the tail of the charging current to make sure the switches are
                // working as they should (see spring2009 fiasco!)
                Thread.Sleep((int)(1000 * ERampUpDelay));
                window.SetLED(window.switchingLED, false);

                // check that the switch was ok (i.e. that the relays really switched)
                // If the manual state is true (0=>N+) then when switching into state 0
                // (false) the North plate should be at positive potential. So there should
                // be a positive current flowing.
                if (newEPolarity == EManualState) // if only C had a logical xor operator!
                {
                    // if the machine state is the same as the new switch state then the
                    // North plate should see -ve current and the South +ve
                    if ((lastNorthCurrent < kNegativeChargeMin) && (lastNorthCurrent > kNegativeChargeMax)
                        && (lastSouthCurrent > kPositiveChargeMin) && (lastSouthCurrent < kPositiveChargeMax))
                    {}
                    //else activateEAlarm(newEPolarity);
                }
                else
                {
                    // North should be +ve, South -ve
                    if ((lastSouthCurrent < kNegativeChargeMin) && (lastSouthCurrent > kNegativeChargeMax)
                        && (lastNorthCurrent > kPositiveChargeMin) && (lastNorthCurrent < kPositiveChargeMax))
                    { }
                    //else activateEAlarm(newEPolarity);
                }
            }
            ESwitchDone();
        }


        private void activateEAlarm(bool newEPolarity)
        {
            window.AddAlert("E-switch - switching to state: " + newEPolarity + "; manual state: " + EManualState + 
                "; north current: " + lastNorthCurrent + "; south current: " + lastSouthCurrent + " .");
        }

        private void ESwitchDone()
        {
            switchingEfield = false;
            window.EnableControl(window.switchEButton, true);
        }

        // this function is, like many in this class, a little cheezy.
        // it doesn't use update voltages, but rather writes direct to the analog outputs.
        private void RampVoltages(double startPlus, double targetPlus, double startMinus,
                                        double targetMinus, int numSteps, double rampTime)
        {
            double rampDelay = ((1000 * rampTime) / (double)numSteps);
            double diffPlus = targetPlus - startPlus;
            double diffMinus = targetMinus - startMinus;
            window.SetLED(window.rampLED, true);
            for (int i = 1; i <= numSteps; i++)
            {
                double newPlus = startPlus + (i * (diffPlus / numSteps));
                double newMinus = startMinus + (i * (diffMinus / numSteps));
                SetAnalogOutput(cPlusOutputTask, newPlus);
                SetAnalogOutput(cMinusOutputTask, newMinus);
                // don't sleep if no ramp delay (as sleep imposes a delay even when called with
                // sleep time = 0).
                if (rampTime != 0.0) Thread.Sleep((int)rampDelay);
                // flash the ramp LED
                window.SetLED(window.rampLED, (i % 2) == 0);
            }
            window.SetLED(window.rampLED, false);

        }

        // ** E-field asymmetry is currently disabled as not implemented consistently
        // calculate the asymmetric field values
        private void CalculateVoltages()
        {
            cPlusToWrite = CPlusVoltage;
            cMinusToWrite = CMinusVoltage;
            if (window.eFieldAsymmetryCheckBox.Checked)
            {
                if (EFieldPolarity == false)
                {
                    cPlusToWrite += Double.Parse(window.zeroPlusOneMinusBoostTextBox.Text);
                    cPlusToWrite += Double.Parse(window.zeroPlusBoostTextBox.Text);
                }
                else
                {
                    cMinusToWrite -= Double.Parse(window.zeroPlusOneMinusBoostTextBox.Text);
                }
            }
        }

        private double cPlusToWrite;
        private double cMinusToWrite;

        public void UpdateVoltages()
        {
            //Checks if E field enable box is checked or not before setting the fields
            double cPlusOff = CPlusOffVoltage;
            double cMinusOff = CMinusOffVoltage;
            if (EFieldEnabled)
            {
                CalculateVoltages();
                SetAnalogOutput(cPlusOutputTask, cPlusToWrite);
                SetAnalogOutput(cMinusOutputTask, cMinusToWrite);
                //SetAnalogOutput(cPlusOutputTask, CPlusVoltage);
                //SetAnalogOutput(cMinusOutputTask, CMinusVoltage);
            }
            else
            {
                SetAnalogOutput(cPlusOutputTask, cPlusOff);
                SetAnalogOutput(cMinusOutputTask, cMinusOff);
            }
        }

        public void UpdateRFFrequencyMonitor()
        {
            // make sure rf switch is off (this routes power to the measurement devices)
            window.SetCheckBox(window.rfSwitchEnableCheck, true);
            window.SetCheckBox(window.rfSwitchEnableCheck, false);
            // rf1 - switch box off and then on to make sure it fires the checkChanged event
            window.SetCheckBox(window.fmSelectCheck, false);
            window.SetCheckBox(window.fmSelectCheck, true);
            window.SetRadioButton(window.rf1FMPlusRB, true);
            SetFMVoltages();
            Thread.Sleep(100);
            // The synth is connected to channel three
            rfCounter.Channel = 3;
            double rf1PlusFreq = rfCounter.Frequency;
            window.SetTextBox(window.rf1PlusFreqMon, String.Format("{0:F0}", rf1PlusFreq));
            window.SetRadioButton(window.rf1FMMinusRB, true);
            SetFMVoltages();
            Thread.Sleep(100);
            double rf1MinusFreq = rfCounter.Frequency;
            window.SetTextBox(window.rf1MinusFreqMon, String.Format("{0:F0}", rf1MinusFreq));
            window.SetTextBox(window.rf1CentreFreqMon, String.Format("{0:F0}", ((rf1MinusFreq + rf1PlusFreq) / 2)));
            window.SetTextBox(window.rf1StepFreqMon, String.Format("{0:F0}", ((rf1PlusFreq - rf1MinusFreq) / 2)));

            // rf2
            window.SetCheckBox(window.fmSelectCheck, false);
            window.SetRadioButton(window.rf2FMPlusRB, true);
            SetFMVoltages();
            Thread.Sleep(100);
            double rf2PlusFreq = rfCounter.Frequency;
            window.SetTextBox(window.rf2PlusFreqMon, String.Format("{0:F0}", rf2PlusFreq));
            window.SetRadioButton(window.rf2FMMinusRB, true);
            SetFMVoltages();
            Thread.Sleep(100);
            double rf2MinusFreq = rfCounter.Frequency;
            window.SetTextBox(window.rf2MinusFreqMon, String.Format("{0:F0}", rf2MinusFreq));
            window.SetTextBox(window.rf2CentreFreqMon, String.Format("{0:F0}", ((rf2MinusFreq + rf2PlusFreq) / 2)));
            window.SetTextBox(window.rf2StepFreqMon, String.Format("{0:F0}", ((rf2PlusFreq - rf2MinusFreq) / 2)));            
        }

        public void UpdateRFPowerMonitor()
        {
            // make sure rf switch is off (this routes power to the measurement devices)
            window.SetCheckBox(window.rfSwitchEnableCheck, true);
            window.SetCheckBox(window.rfSwitchEnableCheck, false);
            // rf1 - switch box off and then on to make sure it fires the checkChanged event
            window.SetCheckBox(window.attenuatorSelectCheck, false);
            window.SetCheckBox(window.attenuatorSelectCheck, true);
            window.SetRadioButton(window.rf1AttPlusRB, true);
            SetAttenutatorVoltages();
            Thread.Sleep(100);
            double rf1PlusPower = rfPower.Power;
            window.SetTextBox(window.rf1PlusPowerMon, String.Format("{0:F3}", rf1PlusPower));
            window.SetRadioButton(window.rf1AttMinusRB, true);
            SetAttenutatorVoltages();
            Thread.Sleep(100);
            double rf1MinusPower = rfPower.Power;
            window.SetTextBox(window.rf1MinusPowerMon, String.Format("{0:F3}", rf1MinusPower));
            window.SetTextBox(window.rf1CentrePowerMon, String.Format("{0:F3}", ((rf1MinusPower + rf1PlusPower) / 2)));
            window.SetTextBox(window.rf1StepPowerMon, String.Format("{0:F3}", ((rf1PlusPower - rf1MinusPower) / 2)));

            // rf2
            window.SetCheckBox(window.attenuatorSelectCheck, false);
            window.SetRadioButton(window.rf2AttPlusRB, true);
            SetAttenutatorVoltages();
            Thread.Sleep(100);
            double rf2PlusPower = rfPower.Power;
            window.SetTextBox(window.rf2PlusPowerMon, String.Format("{0:F3}", rf2PlusPower));
            window.SetRadioButton(window.rf2AttMinusRB, true);
            SetAttenutatorVoltages();
            Thread.Sleep(100);
            double rf2MinusPower = rfPower.Power;
            window.SetTextBox(window.rf2MinusPowerMon, String.Format("{0:F3}", rf2MinusPower));
            window.SetTextBox(window.rf2CentrePowerMon, String.Format("{0:F3}", ((rf2MinusPower + rf2PlusPower) / 2)));
            window.SetTextBox(window.rf2StepPowerMon, String.Format("{0:F3}", ((rf2PlusPower - rf2MinusPower) / 2)));

        }

        // This is a little cheezy - it probably should be in its own class.
        // This method reads the power meter input and converts the result to dBm.
        // Have replaced the power monitor with the HP438A 24/06/08
        /*private double ReadPowerMonitor()
        {
            double rawReading = ReadAnalogInput(rfPowerMonitorInputTask, 10000, 5000);
            return rawReading;
        }
        */
        public void UpdateBCurrentMonitor()
        {
            // DB0 dB0
            BFlipEnabled = false;
            CalFlipEnabled = false;
            double i00 = 1000000 * bCurrentMeter.ReadCurrent();
            window.SetTextBox(window.bCurrent00TextBox, i00.ToString());
            Thread.Sleep(50);

            // DB0 dB1
            BFlipEnabled = false;
            CalFlipEnabled = true;
            double i01 = 1000000 * bCurrentMeter.ReadCurrent();
            window.SetTextBox(window.bCurrent01TextBox, i01.ToString());
            Thread.Sleep(50);

            // DB1 dB0
            BFlipEnabled = true;
            CalFlipEnabled = false;
            double i10 = 1000000 * bCurrentMeter.ReadCurrent();
            window.SetTextBox(window.bCurrent10TextBox, i10.ToString());
            Thread.Sleep(50);

            // DB1 dB1
            BFlipEnabled = true;
            CalFlipEnabled = true;
            double i11 = 1000000 * bCurrentMeter.ReadCurrent();
            window.SetTextBox(window.bCurrent11TextBox, i11.ToString());
            Thread.Sleep(50);

            // calculate the steps
            double bias = (i00 + i01 + i10 + i11) / 4;
            double calStep = (i01 - i00 - i11 + i10) / 4;
            double flipStep = (i10 - i00 + i11 - i01) / 4;
            window.SetTextBox(window.bCurrentBiasTextBox, bias.ToString());
            window.SetTextBox(window.bCurrentCalStepTextBox, calStep.ToString());
            window.SetTextBox(window.bCurrentFlipStepTextBox, flipStep.ToString());

            // check that the manual state is correct
            if (BManualState)
            {
                if (flipStep < 0) activateBAlarm(flipStep);
            }
            else
            {
                if (flipStep > 0) activateBAlarm(flipStep);
            }
        }

        private void activateBAlarm(double flipStep)
        {
            window.AddAlert("B-field - manual state: " + BManualState + "; DB: " + flipStep + " .");
        }

        public void UpdateVMonitor()
        {
            /*window.SetTextBox(window.cPlusVMonitorTextBox, 
                (cScale * voltageController.ReadInputVoltage(cPlusChan)).ToString());
            window.SetTextBox(window.cMinusVMonitorTextBox, 
                (cScale * voltageController.ReadInputVoltage(cMinusChan)).ToString());
            window.SetTextBox(window.gPlusVMonitorTextBox, 
                (gScale * voltageController.ReadInputVoltage(gPlusChan)).ToString());
            window.SetTextBox(window.gMinusVMonitorTextBox, 
                (gScale * voltageController.ReadInputVoltage(gMinusChan)).ToString());*/
            double cPlusMonitor = ReadAnalogInput(cPlusMonitorInputTask, 100000, 50000);
            window.SetTextBox(window.cPlusVMonitorTextBox, cPlusMonitor.ToString());
            double cMinusMonitor = ReadAnalogInput(cMinusMonitorInputTask, 100000, 50000);
            window.SetTextBox(window.cMinusVMonitorTextBox, cMinusMonitor.ToString());
        }

        private double lastNorthCurrent;
        private double lastSouthCurrent;
        public void UpdateIMonitor()
        {
            ReconfigureIMonitors();
            lastNorthCurrent = northLeakageMonitor.GetCurrent();
            lastSouthCurrent = southLeakageMonitor.GetCurrent();
            window.SetTextBox(window.northIMonitorTextBox, (lastNorthCurrent).ToString());
            window.SetTextBox(window.southIMonitorTextBox, (lastSouthCurrent).ToString());
            window.PlotYAppend(window.leakageGraph, window.northLeakagePlot,
                                    new double[] { lastNorthCurrent });
            window.PlotYAppend(window.leakageGraph, window.southLeakagePlot,
                                    new double[] { lastSouthCurrent });

        }

        List<double> northCList = new List<double>();
        List<double> southCList = new List<double>();
        public void UpdateIRecord()
        {
            ReconfigureIMonitors();
            lastNorthCurrent = northLeakageMonitor.GetCurrent();
            lastSouthCurrent = southLeakageMonitor.GetCurrent();
            window.SetTextBox(window.northIMonitorTextBox, (lastNorthCurrent).ToString());
            window.SetTextBox(window.southIMonitorTextBox, (lastSouthCurrent).ToString());
            window.PlotYAppend(window.leakageGraph, window.northLeakagePlot,
                                    new double[] { lastNorthCurrent });
            northCList.Add(lastNorthCurrent);
            window.PlotYAppend(window.leakageGraph, window.southLeakagePlot,
                                    new double[] { lastSouthCurrent });
            southCList.Add(lastSouthCurrent);

        }

        public void ReadIMonitor()
        {
            lastNorthCurrent = northLeakageMonitor.GetCurrent();
            lastSouthCurrent = southLeakageMonitor.GetCurrent();
        }

        //Thread updateIMonThread;
        //public void UpdateIMonitorAsync()
        //{
        //    updateIMonThread = new Thread(delegate()
        //        {
        //            lastNorthCurrent = northLeakageMonitor.GetCurrent();
        //           lastSouthCurrent = southLeakageMonitor.GetCurrent();
        //        });
        //    updateIMonThread.Start();
        //}

        //public void WaitForIMonitorAsync()
        //{
        //    updateIMonThread.Join();
        //}

        public void CalibrateIMonitors()
        {
            ReconfigureIMonitors();
            
            southLeakageMonitor.SetZero();
            northLeakageMonitor.SetZero();

            northOffset = northLeakageMonitor.Offset;
            southOffset = southLeakageMonitor.Offset;

            window.SetTextBox(window.southOffsetIMonitorTextBox, southOffset.ToString());
            window.SetTextBox(window.northOffsetIMonitorTextBox, northOffset.ToString());

        }

        public void ReconfigureIMonitors()
        {
            currentMonitorMeasurementTime = Double.Parse(window.IMonitorMeasurementLengthTextBox.Text);
            northFreq2AmpSlope = Double.Parse(window.leakageMonitorSlopeTextBox.Text);
            southFreq2AmpSlope = Double.Parse(window.leakageMonitorSlopeTextBox.Text);
            northVolt2FreqSlope = Double.Parse(window.northV2FSlopeTextBox.Text);
            southVolt2FreqSlope = Double.Parse(window.southV2FSlopeTextBox.Text);

            //southLeakageMonitor.MeasurementTime = currentMonitorMeasurementTime;
            //northLeakageMonitor.MeasurementTime = currentMonitorMeasurementTime;
            northLeakageMonitor.F2ISlope = northFreq2AmpSlope;
            southLeakageMonitor.F2ISlope = southFreq2AmpSlope;
            northLeakageMonitor.V2FSlope = northVolt2FreqSlope;
            southLeakageMonitor.V2FSlope = southVolt2FreqSlope;
        }

        private Thread iMonitorPollThread;
        private object iMonitorLock = new object();
        private bool iMonitorStopFlag = false;
        private int iMonitorPollPeriod = 200;
        internal void StartIMonitorPoll()
        {
            lock (iMonitorLock)
            {
                iMonitorPollThread = new Thread(new ThreadStart(IMonitorPollWorker));
                window.EnableControl(window.startIMonitorPollButton, false);
                window.EnableControl(window.stopIMonitorPollButton, true);
                iMonitorPollPeriod = Int32.Parse(window.iMonitorPollPeriod.Text);
                iMonitorPollThread.Start();
            }

        }

        internal void StopIMonitorPoll()
        {
            lock (iMonitorLock) iMonitorStopFlag = true;
        }

        private void IMonitorPollWorker()
        {
            for (; ; )
            {
                Thread.Sleep(iMonitorPollPeriod);
                UpdateIMonitor();
                lock (iMonitorLock)
                {
                    if (iMonitorStopFlag)
                    {
                        iMonitorStopFlag = false;
                        break;
                    }
                }
            }
            window.EnableControl(window.startIMonitorPollButton, true);
            window.EnableControl(window.stopIMonitorPollButton, false);
        }

        private Thread iRecordThread;
        private object iRecordLock = new object();
        private bool iRecordStopFlag = false;
        //private int iMonitorPollPeriod = 200;
        internal void StartIRecord()
        {
            lock (iRecordLock)
            {
                iRecordThread = new Thread(new ThreadStart(IRecordWorker));
                window.EnableControl(window.startIRecordButton, false);
                window.EnableControl(window.stopIRecordButton, true);
                window.EnableControl(window.saveToFile, true);
                iMonitorPollPeriod = Int32.Parse(window.iMonitorPollPeriod.Text);
                iRecordThread.Start();
            }

        }

        internal void StopIRecord()
        {
            lock (iRecordLock) iRecordStopFlag = true;
        }

        private void IRecordWorker()
        {
            for (; ; )
            {
                Thread.Sleep(iMonitorPollPeriod);
                UpdateIRecord();
                lock (iRecordLock)
                {
                    if (iRecordStopFlag)
                    {
                        iRecordStopFlag = false;
                        break;
                    }
                }
            }
            window.EnableControl(window.startIRecordButton, true);
            window.EnableControl(window.stopIRecordButton, false);
        }
        internal void SaveToFile()
        {
            using (StreamWriter sw = new StreamWriter("F://Data//general//LeakageCurrent.csv"))
            {
                sw.WriteLine(DateTime.Now);
                sw.WriteLine("Poll period =" + " " + iMonitorPollPeriod);
                //sw.WriteLine("Measurement time =" + " " + northLeakageMonitor.MeasurementTime);
                sw.WriteLine("North voltage to frequency slope =" + " " + northVolt2FreqSlope);
                sw.WriteLine("South voltage to frequency slope =" + " " + southVolt2FreqSlope);
                sw.WriteLine("Frequency to current slope =" + " " + northLeakageMonitor.F2ISlope);
                sw.WriteLine("North offset =" + " " + northLeakageMonitor.Offset);
                sw.WriteLine("South offset =" + " " + southLeakageMonitor.Offset);
                sw.WriteLine("northCurrent" + "," + "southCurrent");

                for (int i = 0; i < northCList.ToArray().Length; i++)
                {
                    sw.WriteLine(northCList[i] + "," + southCList[i]);
                }
                northCList.Clear();
                southCList.Clear();
                window.EnableControl(window.saveToFile, false);
            }
        }
        public void UpdateLaserPhotodiodes()
        {
            double probePDValue = ReadAnalogInput(probeMonitorInputTask);
            window.SetTextBox(window.probeMonitorTextBox, probePDValue.ToString());
            double pumpPDValue = ReadAnalogInput(pumpMonitorInputTask);
            window.SetTextBox(window.pumpMonitorTextBox, pumpPDValue.ToString());
        }

        public void UpdateMiniFluxgates()
        {
            double miniFlux1Value = ReadAnalogInput(miniFlux1MonitorInputTask);
            window.SetTextBox(window.miniFlux1TextBox, miniFlux1Value.ToString());
            double miniFlux2Value = ReadAnalogInput(miniFlux2MonitorInputTask);
            window.SetTextBox(window.miniFlux2TextBox, miniFlux2Value.ToString());
            double miniFlux3Value = ReadAnalogInput(miniFlux3MonitorInputTask);
            window.SetTextBox(window.miniFlux3TextBox, miniFlux3Value.ToString());
        }

        public void UpdatePiMonitor()
        {
            SetPhaseFlip1(true);
            SetPhaseFlip2(false);
            double piMonitorV1 = ReadAnalogInput(piMonitorTask);
            window.SetTextBox(window.piMonitor1TextBox, piMonitorV1.ToString());
            SetPhaseFlip2(true);
            double piMonitorV2 = ReadAnalogInput(piMonitorTask);
            window.SetTextBox(window.piMonitor2TextBox, piMonitorV2.ToString());
            SetPhaseFlip1(false);
            SetPhaseFlip2(false);
            if (true) window.AddAlert("Pi-flip - V1: " + piMonitorV1 + "; V2: " + piMonitorV1 + " .");
        }

        public void UpdateDiodeRefCavMonitor()
        {
            double diodeRefCavMonValue = ReadAnalogInput(diodeRefCavInputTask);
            window.SetTextBox(window.diodeRefCavMonTextBox, diodeRefCavMonValue.ToString());
        }

        public void UpdateDiodeCurrentMonitor()
        {
            double diodeCurrentMonValue = ReadAnalogInput(diodeCurrentMonInputTask);
            window.SetTextBox(window.diodeCurrentTextBox, diodeCurrentMonValue.ToString());
        }

        public void UpdateDiodeCurrentGraphAndMonitor()
        {
            double diodeCurrentMonValue = ReadAnalogInput(diodeCurrentMonInputTask);
            window.SetTextBox(window.diodeCurrentTextBox, diodeCurrentMonValue.ToString());
            window.PlotYAppend(window.diodeCurrentGraph, window.diodeCurrentPlot,
                                    new double[] { diodeCurrentMonValue });
        }

        public void UpdateFibreAmpFaults()
        {
            window.fibreAmpMasterFaultLED.Value =! ReadDigitalLine("fibreAmpMasterErr");
            window.fibreAmpSeedFaultLED.Value =! ReadDigitalLine("fibreAmpSeedErr");
            window.fibreAmpBackReflectFaultLED.Value =! ReadDigitalLine("fibreAmpBackFeflectErr");
            window.fibreAmpTempFaultLED.Value =! ReadDigitalLine("fibreAmpTempErr");
            window.fibreAmpPowerFaultLED.Value =! ReadDigitalLine("fibreAmpPowerSupplyErr");
        }

        private Thread diodeCurrentMonitorPollThread;
        private object diodeCurrentMonitorLock = new object();
        private bool diodeCurrentMonitorStopFlag = false;
        private int diodeCurrentMonitorPollPeriod = 200;
        internal void StartDiodeCurrentPoll()
        {
            lock (diodeCurrentMonitorLock)
            {
                diodeCurrentMonitorPollThread = new Thread(new ThreadStart(DiodeCurrentMonitorPollWorker));
                window.EnableControl(window.startDiodeCurrentPollButton, false);
                window.EnableControl(window.stopDiodeCurrentPollButton, true);
                diodeCurrentMonitorPollPeriod = Int32.Parse(window.diodeCurrentPollTextBox.Text);
                diodeCurrentMonitorPollThread.Start();
            }

        }

        internal void StopDiodeCurrentPoll()
        {
            lock (diodeCurrentMonitorLock) diodeCurrentMonitorStopFlag = true;
        }

        private void DiodeCurrentMonitorPollWorker()
        {
            for (; ; )
            {
                Thread.Sleep(diodeCurrentMonitorPollPeriod);
                UpdateDiodeCurrentGraphAndMonitor();
                lock (diodeCurrentMonitorLock)
                {
                    if (diodeCurrentMonitorStopFlag)
                    {
                        diodeCurrentMonitorStopFlag = false;
                        break;
                    }
                }
            }
            window.EnableControl(window.startDiodeCurrentPollButton, true);
            window.EnableControl(window.stopDiodeCurrentPollButton, false);
        }

        public void SetDiodeRefCav()
        {
            double refCavVoltage = Double.Parse(window.diodeRefCavTextBox.Text);
            if (window.diodeRefCavStepMinusButton.Checked) refCavVoltage -= Double.Parse(window.diodeRefCavStepTextBox.Text);
            if (window.diodeRefCavStepPlusButton.Checked) refCavVoltage += Double.Parse(window.diodeRefCavStepTextBox.Text);
            // HV supply must not go below 0V
            if (refCavVoltage < 0)
            {
                SetAnalogOutput(diodeRefCavOutputTask, 0.0);
                window.diodeRefCavTextBox.BackColor = System.Drawing.Color.Red;
                UpdateDiodeRefCavMonitor();
            }
            else if (refCavVoltage > 5)
            {
                SetAnalogOutput(diodeRefCavOutputTask, 5.0);
                window.diodeRefCavTextBox.BackColor = System.Drawing.Color.Red;
                UpdateDiodeRefCavMonitor();
            }
            else
            {
                SetAnalogOutput(diodeRefCavOutputTask, refCavVoltage);
                window.diodeRefCavTextBox.BackColor = System.Drawing.Color.LimeGreen;
                UpdateDiodeRefCavMonitor();
            }
            
        }

        public void SetFibreAmpPwr()
        {
            double fibreAmpVoltage = Double.Parse(window.fibreAmpPwrTextBox.Text);
            // supply must not go below 0V
            if (fibreAmpVoltage < 0)
            {
                SetAnalogOutput(fibreAmpOutputTask, 0.0);
                window.fibreAmpPwrTextBox.BackColor = System.Drawing.Color.Red;
            }
            else if (fibreAmpVoltage > 5)
            {
                SetAnalogOutput(fibreAmpOutputTask, 5.0);
                window.fibreAmpPwrTextBox.BackColor = System.Drawing.Color.Red;
            }
            else
            {
                SetAnalogOutput(fibreAmpOutputTask, fibreAmpVoltage);
                window.fibreAmpPwrTextBox.BackColor = System.Drawing.Color.LimeGreen;
            }

        }      

        // TODO: I'm not sure whether these button enabling properties are threadsafe.
        // Probably had better wrap them.
        public void StartYAGFlashlamps()
        {
            yag.StartFlashlamps(false);
            window.startYAGFlashlampsButton.Enabled = false;
            window.stopYagFlashlampsButton.Enabled = true;
        }

        public void StopYAGFlashlamps()
        {
            yag.StopFlashlamps();
            window.startYAGFlashlampsButton.Enabled = true;
            window.stopYagFlashlampsButton.Enabled = false;
        }

        public void EnableYAGQ()
        {
            yag.EnableQSwitch();
            window.yagQEnableButton.Enabled = false;
            window.yagQDisableButton.Enabled = true;
        }

        public void DisableYAGQ()
        {
            yag.DisableQSwitch();
            window.yagQEnableButton.Enabled = true;
            window.yagQDisableButton.Enabled = false;
        }

        public void CheckYAGInterlock()
        {
            window.SetTextBox(window.interlockStatusTextBox, yag.InterlockFailed.ToString());
        }

        public void UpdateYAGFlashlampVoltage()
        {
            yag.SetFlashlampVoltage((int)Double.Parse(window.yagFlashlampVTextBox.Text));
        }

        public void StepTarget()
        {
            int numSteps = (int)Double.Parse(window.TargetNumStepsTextBox.Text);
            StepTarget(numSteps);
        }

        public void StepTarget(int numSteps)
        {
            for (int i = 0; i < numSteps; i++)
            {
                SetDigitalLine("targetStepper", true);
                Thread.Sleep(5);
                SetDigitalLine("targetStepper", false);
                Thread.Sleep(5);
            }
        }

        public void UpdateI2AOMFreqMonitor()
        {
            window.SetRadioButton(window.FLPZTStepPlusButton, true);
            UpdateFLPZTV();
            Thread.Sleep(10);
            //bool[] cntrlSeq = (bool[])Environs.Hardware.GetInfo("IodineFreqMon");
            //SetDigitalLine("rfCountSwBit1", cntrlSeq[0]);
            //SetDigitalLine("rfCountSwBit2", cntrlSeq[1]);
            // The VCO is connected to channel two (should put this line in PXIEDMHardware.cs)
            rfCounter.Channel = 2; 
            double I2PlusFreq = rfCounter.Frequency;
            window.SetTextBox(window.I2AOMFreqPlusTextBox, String.Format("{0:F0}", I2PlusFreq));

            window.SetRadioButton(window.FLPZTStepMinusButton, true);
            UpdateFLPZTV();
            Thread.Sleep(10);
            double I2MinusFreq = rfCounter.Frequency;
            window.SetTextBox(window.I2AOMFreqMinusTextBox, String.Format("{0:F0}", I2MinusFreq));
            window.SetTextBox(window.I2AOMFreqCentreTextBox, String.Format("{0:F0}", ((I2PlusFreq + I2MinusFreq) / 2)));
            window.SetTextBox(window.I2AOMFreqStepTextBox, String.Format("{0:F0}", ((I2PlusFreq - I2MinusFreq) / 2)));
        }

        public void UpdatePumpAOMFreqMonitor()
        {

            //bool[] cntrlSeq = (bool[])Environs.Hardware.GetInfo("pumpAOMFreqMon");
            //SetDigitalLine("rfCountSwBit1", cntrlSeq[0]);
            //SetDigitalLine("rfCountSwBit2", cntrlSeq[1]);
            Thread.Sleep(10);
            // The VCO is connected to channel one (should put this line in PXIEDMHardware.cs)
            rfCounter2.Channel = 1;
            double PumpAOMFreq = rfCounter2.Frequency;
            window.SetTextBox(window.PumpAOMFreqTextBox, String.Format("{0:F0}", PumpAOMFreq));
        }

        internal void UpdateProbePolarizerAngle()
        {
            motorController1.MoveAbsoluteEx(0,
                (int)Double.Parse(window.probePolarizerAngleTextBox.Text), 0, true);
        }

        internal void UpdatePumpPolarizerAngle()
        {
            motorController2.MoveAbsoluteEx(0,
                (int)Double.Parse(window.pumpPolarizerAngleTextBox.Text), 0, true);
        }

        public void SetProbePolarizerAngle(double theta)
        {
            window.SetTextBox(window.probePolarizerAngleTextBox, theta.ToString());
            UpdateProbePolarizerAngle();
        }

        public void SetPumpPolarizerAngle(double theta)
        {
            window.SetTextBox(window.pumpPolarizerAngleTextBox, theta.ToString());
            UpdatePumpPolarizerAngle();
        }

        public void EnableGreenSynth(bool enable)
        {
            greenSynth.Connect();
            if (enable)
            {
                greenSynth.Frequency = GreenSynthOnFrequency;
                greenSynth.Amplitude = GreenSynthOnAmplitude;
                greenSynth.DCFM = GreenSynthDCFM;
            }
            else
            {
                greenSynth.Amplitude = greenSynthOffAmplitude;
            }
            greenSynth.Disconnect();
        }

        public void EnableRFSwitch(bool enable)
        {
            SetDigitalLine("rfSwitch", enable);
        }

        /*private double lastGPlus = 0;
        private double lastGMinus = 0;
        private double lastCPlus = 0;
        private double lastCMinus = 0;*/
        /*public void SetEFieldOnOff(bool enable)
        {
            /*if (eFieldMode == EFieldMode.TTL)
            {
                SetDigitalLine("eOnOff", enable);
                SetDigitalLine("notEOnOff", !enable);
            }
            if (eFieldMode == EFieldMode.GPIB)
            {
                if (!enable)
                {
                    // switching off, so save the voltages for when we switch back on
                    lastGPlus = GPlusVoltage;
                    lastGMinus = GMinusVoltage;
                    lastCPlus = CPlusVoltage;
                    lastCMinus = CMinusVoltage;
                    // set the voltages to zero and update
                    GPlusVoltage = 0;
                    GMinusVoltage = 0;
                    CPlusVoltage = 0;
                    CMinusVoltage = 0;
                    UpdateVoltages();
                }
                else
                {
                    // switching on, so restore the voltages at last switch off
                    GPlusVoltage = lastGPlus;
                    GMinusVoltage = lastGMinus;
                    CPlusVoltage = lastCPlus;
                    CMinusVoltage = lastCMinus;
                    UpdateVoltages();
                }
            }               
            
        }*/

        public void SetEPolarity(bool state)
        {
            SetDigitalLine("ePol", state);
            SetDigitalLine("notEPol", !state);
        }

        public void SetBleed(bool enable)
        {
            SetDigitalLine("eBleed", !enable);
        }

        public void SetBFlip(bool enable)
        {
            SetDigitalLine("b", enable);
        }

        public void SetCalFlip(bool enable)
        {
            SetDigitalLine("notDB", !enable);
        }

        public void SelectGreenDCFM(bool enable)
        {
            SetDigitalLine("fmSelect", enable);
        }

        internal void SelectAttenuator(bool enable)
        {
            SetDigitalLine("attenuatorSelect", enable);
        }

        public void SetPhaseFlip1(bool enable)
        {
            SetDigitalLine("piFlip", enable);
        }

        public void SetPhaseFlip2(bool enable)
        {
            SetDigitalLine("piFlipEnable", enable);
            SetDigitalLine("notPIFlipEnable", !enable);
        }

        internal void SetScramblerTTL(bool enable)
        {
            SetDigitalLine("scramblerEnable", enable);
        }

        internal void SetPumpShutter(bool enable)
        {
            SetDigitalLine("pumpShutter", enable);
        }

        internal void SetProbeShutter(bool enable)
        {
            SetDigitalLine("probeShutter", enable);
        }

        internal void SetArgonShutter(bool enable)
        {
            SetDigitalLine("argonShutter", enable);
        }

        internal void SetFibreAmpPowerSwitch(bool enable)
        {
            SetDigitalLine("fibreAmpEnable", enable);
            window.fibreAmpEnableLED.Value = enable;
        }

        public void SetSwitchTTL(bool enable)
        {
            SetDigitalLine("ttlSwitch", enable);
            window.switchScanTTLSwitch.Value = enable;
        }

        public void SetScanningBVoltage()
        {
            double bBoxVoltage = Double.Parse(window.scanningBVoltageBox.Text);
            SetAnalogOutput(bBoxAnalogOutputTask, bBoxVoltage);
        }

        //cheezy temporary hack
        public void SetScanningBVoltage(double v)
        {
            window.SetTextBox(window.scanningBVoltageBox, v.ToString());
            SetAnalogOutput(bBoxAnalogOutputTask, v);
        }

        public void SetSteppingBBiasVoltage()
        {
            double bBoxVoltage = Double.Parse(window.steppingBBoxBiasTextBox.Text);
            SetAnalogOutput(steppingBBiasAnalogOutputTask, bBoxVoltage);
        }


        public void SetSteppingBBiasVoltage(double v)
        {
            window.SetTextBox(window.steppingBBoxBiasTextBox, v.ToString());
            SetAnalogOutput(steppingBBiasAnalogOutputTask, v);
        }

        public void SetScramblerVoltage()
        {
            double scramblerVoltage = Double.Parse(window.scramblerVoltageTextBox.Text);
            SetAnalogOutput(phaseScramblerVoltageOutputTask, scramblerVoltage);
        }


        public void SetScramblerVoltage(double v)
        {
            window.SetTextBox(window.scramblerVoltageTextBox, v.ToString());
            SetAnalogOutput(phaseScramblerVoltageOutputTask, v);
        }

        public void UpdateFLPZTV()
        {
            double pztVoltage = Double.Parse(window.FLPZTVTextBox.Text);
            if (window.FLPZTStepMinusButton.Checked) pztVoltage -= Double.Parse(window.FLPZTStepTextBox.Text);
            if (window.FLPZTStepPlusButton.Checked) pztVoltage += Double.Parse(window.FLPZTStepTextBox.Text);
            pztVoltage = windowVoltage(pztVoltage, 0, 5);
            SetAnalogOutput(flPZTVAnalogOutputTask, pztVoltage);
            window.FLPZTVtrackBar.Value = 100*(int)pztVoltage;
        }

        public void UpdateFLPZTV(double pztVoltage)
        {
            SetAnalogOutput(flPZTVAnalogOutputTask, pztVoltage);
            window.FLPZTVTextBox.Text = pztVoltage.ToString();
        }

        public void SetFLPZTVoltage(double v)
        {
            window.SetTextBox(window.FLPZTVTextBox, v.ToString());
            SetAnalogOutput(flPZTVAnalogOutputTask, v);
        }

        public void SetScanningBZero()
        {
            window.SetTextBox(window.scanningBVoltageBox, "0.0");
            SetScanningBVoltage();
        }

        public void SetScanningBFS()
        {
            window.SetTextBox(window.scanningBVoltageBox, "5.0");
            SetScanningBVoltage();
        }

        // these are here as it seems IronPython has trouble setting attributes remotely
        public void SetRF1AttCentre(double v)
        {
            RF1AttCentre = windowVoltage(v, 0, 5);
        }
        public void SetRF2AttCentre(double v)
        {
            RF2AttCentre = windowVoltage(v, 0, 5);
        }
        public void SetRF1FMCentre(double v)
        {
            RF1FMCentre = windowVoltage(v, 0, 5);
        }
        public void SetRF2FMCentre(double v)
        {
            RF2FMCentre = windowVoltage(v, 0, 5);
        }

        private double windowVoltage(double vIn, double vMin, double vMax)
        {
            if (vIn < vMin) return vMin;
            if (vIn > vMax) return vMax;
            return vIn;
        }

        public void SetAttenutatorVoltages()
        {
            double rf1AttenuatorVoltage = Double.Parse(window.rf1AttenuatorVoltageTextBox.Text);
            if (window.rf1AttMinusRB.Checked) rf1AttenuatorVoltage -= Double.Parse(window.rf1AttIncTextBox.Text);
            if (window.rf1AttPlusRB.Checked) rf1AttenuatorVoltage += Double.Parse(window.rf1AttIncTextBox.Text);
            rf1AttenuatorVoltage = windowVoltage(rf1AttenuatorVoltage, 0, 5);
            double rf2AttenuatorVoltage = Double.Parse(window.rf2AttenuatorVoltageTextBox.Text);
            if (window.rf2AttMinusRB.Checked) rf2AttenuatorVoltage -= Double.Parse(window.rf2AttIncTextBox.Text);
            if (window.rf2AttPlusRB.Checked) rf2AttenuatorVoltage += Double.Parse(window.rf2AttIncTextBox.Text);
            rf2AttenuatorVoltage = windowVoltage(rf2AttenuatorVoltage, 0, 5);
            SetAnalogOutput(rf1AttenuatorOutputTask, rf1AttenuatorVoltage);
            SetAnalogOutput(rf2AttenuatorOutputTask, rf2AttenuatorVoltage);
        }

        internal void SetFMVoltages()
        {
            double rf1FMVoltage = Double.Parse(window.rf1FMVoltage.Text);
            if (window.rf1FMMinusRB.Checked) rf1FMVoltage -= Double.Parse(window.rf1FMIncTextBox.Text);
            if (window.rf1FMPlusRB.Checked) rf1FMVoltage += Double.Parse(window.rf1FMIncTextBox.Text);
            double rf2FMVoltage = Double.Parse(window.rf2FMVoltage.Text);
            if (window.rf2FMMinusRB.Checked) rf2FMVoltage -= Double.Parse(window.rf2FMIncTextBox.Text);
            if (window.rf2FMPlusRB.Checked) rf2FMVoltage += Double.Parse(window.rf2FMIncTextBox.Text);
            SetAnalogOutput(rf1FMOutputTask, rf1FMVoltage);
            SetAnalogOutput(rf2FMOutputTask, rf2FMVoltage);
        }
        #endregion

    }
}
