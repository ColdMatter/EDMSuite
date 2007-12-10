using System;
using System.Collections;
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
        private static double northSlope = 200;
        private static double southSlope = 200;
        private static double northOffset = 0;
        private static double southOffset = 0;
        private static double currentMonitorMeasurementTime = 0.01;



        #endregion

        #region Setup

        // hardware
        HP8657ASynth greenSynth = (HP8657ASynth)Environs.Hardware.GPIBInstruments["green"];
        Synth redSynth = (Synth)Environs.Hardware.GPIBInstruments["red"];
        ICS4861A voltageController = (ICS4861A)Environs.Hardware.GPIBInstruments["4861"];
        HP34401A bCurrentMeter = (HP34401A)Environs.Hardware.GPIBInstruments["bCurrentMeter"];
        Agilent53131A rfCounter = (Agilent53131A)Environs.Hardware.GPIBInstruments["rfCounter"];
        Hashtable digitalTasks = new Hashtable();
        LeakageMonitor northLeakageMonitor =
            new LeakageMonitor((CounterChannel)Environs.Hardware.CounterChannels["northLeakage"], northSlope, northOffset, currentMonitorMeasurementTime);
        LeakageMonitor southLeakageMonitor =
            new LeakageMonitor((CounterChannel)Environs.Hardware.CounterChannels["southLeakage"], southSlope, southOffset, currentMonitorMeasurementTime);
        BrilliantLaser yag = (BrilliantLaser)Environs.Hardware.YAG;
        Task bBoxAnalogOutputTask;
        Task steppingBBiasAnalogOutputTask;
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
        Task rfPowerMonitorInputTask;

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
            CreateDigitalTask("notEOnOff");
            CreateDigitalTask("eOnOff");
            CreateDigitalTask("ePol");
            CreateDigitalTask("notEPol");
            CreateDigitalTask("eBleed");
            CreateDigitalTask("rfSwitch");
            CreateDigitalTask("fmSelect");
            CreateDigitalTask("attenuatorSelect");
            CreateDigitalTask("b");
            CreateDigitalTask("notDB");
            CreateDigitalTask("piFlip");
            CreateDigitalTask("piFlipEnable");
            CreateDigitalTask("pumpShutter");
            CreateDigitalTask("pump2Shutter");

            // initialise the current leakage monitors
            northLeakageMonitor.Initialize();
            southLeakageMonitor.Initialize();


            // analog outputs
            bBoxAnalogOutputTask = CreateAnalogOutputTask("b");
            steppingBBiasAnalogOutputTask = CreateAnalogOutputTask("steppingBBias");
            rf1AttenuatorOutputTask = CreateAnalogOutputTask("rf1Attenuator");
            rf2AttenuatorOutputTask = CreateAnalogOutputTask("rf2Attenuator");
            rf1FMOutputTask = CreateAnalogOutputTask("rf1FM");
            rf2FMOutputTask = CreateAnalogOutputTask("rf2FM");
            cPlusOutputTask = CreateAnalogOutputTask("cPlus");
            cMinusOutputTask = CreateAnalogOutputTask("cMinus");

            // analog inputs
            probeMonitorInputTask = CreateAnalogInputTask("probePD");
            pumpMonitorInputTask = CreateAnalogInputTask("pumpPD");
            cPlusMonitorInputTask = CreateAnalogInputTask("cPlusMonitor");
            cMinusMonitorInputTask = CreateAnalogInputTask("cMinusMonitor");
            rfPowerMonitorInputTask = CreateAnalogInputTask("rfPower");

            // make the control window
            window = new ControlWindow();
            window.controller = this;
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

        private void SetDigitalLine(string name, bool value)
        {
            Task digitalTask = ((Task)digitalTasks[name]);
            DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(digitalTask.Stream);
            writer.WriteSingleSampleSingleLine(true, value);
            digitalTask.Control(TaskAction.Unreserve);
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
        }

        private void StoreParameters()
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
 
            // serialize it
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String dataStoreFilePath = settingsPath + "\\EDMHardwareController\\parameters.bin";
            BinaryFormatter s = new BinaryFormatter();
            try
            {
                s.Serialize(new FileStream(dataStoreFilePath, FileMode.Create), dataStore);
            }
            catch (Exception)
            { Console.Out.WriteLine("Unable to store settings"); }
        }

        private void LoadParameters()
        {
            // deserialize
            String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
            String dataStoreFilePath = settingsPath + "\\EDMHardwareController\\parameters.bin";
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
                return window.pump2ShutterCheck.Checked;
            }
            set
            {
                window.SetCheckBox(window.pump2ShutterCheck, value);
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

        public double SteppingBiasCurrent
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

        // this function switches the E field polarity with ramped turn on and off
        public void SwitchEWorker()
        {
            lock (switchingLock)
            {
                // don't waste time if the field isn't really switching
                if (newEPolarity != EFieldPolarity)
                {
                    window.SetLED(window.switchingLED, true);
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
                    // ramp the field up
                    RampVoltages(CPlusOffVoltage, CPlusVoltage, CMinusOffVoltage, CMinusVoltage, 20, ERampDownTime);
                    // set as enabled
                    EFieldEnabled = true;
                    Thread.Sleep((int)(1000 * ERampUpDelay));
                    window.SetLED(window.switchingLED, false);

                }
            }
            ESwitchDone();
        }

        private void ESwitchDone()
        {
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
                Thread.Sleep((int)rampDelay);
                // flash the ramp LED
                window.SetLED(window.rampLED, (i % 2) == 0);
            }
            window.SetLED(window.rampLED, false);

        }

        // calculate the asymmetric field values
        private void CalculateVoltages()
        {
            cPlusToWrite = CPlusVoltage;
            cMinusToWrite = CMinusVoltage;
            if (EFieldEnabled && window.eFieldAsymmetryCheckBox.Checked)
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
            window.SetCheckBox(window.rfSwitchEnableCheck, false);
            // rf1 - switch box off and then on to make sure it fires the checkChanged event
            window.SetCheckBox(window.fmSelectCheck, false);
            window.SetCheckBox(window.fmSelectCheck, true);
            window.SetRadioButton(window.rf1FMPlusRB, true);
            SetFMVoltages();
            Thread.Sleep(100);
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
            window.SetCheckBox(window.rfSwitchEnableCheck, false);
            // rf1 - switch box off and then on to make sure it fires the checkChanged event
            window.SetCheckBox(window.attenuatorSelectCheck, false);
            window.SetCheckBox(window.attenuatorSelectCheck, true);
            window.SetRadioButton(window.rf1AttPlusRB, true);
            SetAttenutatorVoltages();
            Thread.Sleep(100);
            double rf1PlusPower = ReadPowerMonitor();
            window.SetTextBox(window.rf1PlusPowerMon, String.Format("{0:F2}", rf1PlusPower));
            window.SetRadioButton(window.rf1AttMinusRB, true);
            SetAttenutatorVoltages();
            Thread.Sleep(100);
            double rf1MinusPower = ReadPowerMonitor();
            window.SetTextBox(window.rf1MinusPowerMon, String.Format("{0:F2}", rf1MinusPower));
            window.SetTextBox(window.rf1CentrePowerMon, String.Format("{0:F2}", ((rf1MinusPower + rf1PlusPower) / 2)));
            window.SetTextBox(window.rf1StepPowerMon, String.Format("{0:F2}", ((rf1PlusPower - rf1MinusPower) / 2)));

            // rf2
            window.SetCheckBox(window.attenuatorSelectCheck, false);
            window.SetRadioButton(window.rf2AttPlusRB, true);
            SetAttenutatorVoltages();
            Thread.Sleep(100);
            double rf2PlusPower = ReadPowerMonitor();
            window.SetTextBox(window.rf2PlusPowerMon, String.Format("{0:F2}", rf2PlusPower));
            window.SetRadioButton(window.rf2AttMinusRB, true);
            SetAttenutatorVoltages();
            Thread.Sleep(100);
            double rf2MinusPower = ReadPowerMonitor();
            window.SetTextBox(window.rf2MinusPowerMon, String.Format("{0:F2}", rf2MinusPower));
            window.SetTextBox(window.rf2CentrePowerMon, String.Format("{0:F2}", ((rf2MinusPower + rf2PlusPower) / 2)));
            window.SetTextBox(window.rf2StepPowerMon, String.Format("{0:F2}", ((rf2PlusPower - rf2MinusPower) / 2)));
        }

        // This is a little cheezy - it probably should be in its own class.
        // This method reads the power meter input and converts the result to dBm.
        private double ReadPowerMonitor()
        {
            double rawReading = ReadAnalogInput(rfPowerMonitorInputTask, 10000, 5000);
            return rawReading;
        }

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

        public void UpdateIMonitor()
        {
            ReconfigureIMonitors();
            window.SetTextBox(window.northIMonitorTextBox, (northLeakageMonitor.GetCurrent()).ToString());
            window.SetTextBox(window.southIMonitorTextBox, (southLeakageMonitor.GetCurrent()).ToString());
            window.PlotYAppend(window.leakageGraph, window.northLeakagePlot,
                                    new double[] { northLeakageMonitor.GetCurrent() });
            window.PlotYAppend(window.leakageGraph, window.southLeakagePlot,
                                    new double[] { southLeakageMonitor.GetCurrent() });

        }

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

        private void ReconfigureIMonitors()
        {
            currentMonitorMeasurementTime = Double.Parse(window.IMonitorMeasurementLengthTextBox.Text);
            northSlope = Double.Parse(window.leakageMonitorSlopeTextBox.Text);
            southSlope = Double.Parse(window.leakageMonitorSlopeTextBox.Text);

            southLeakageMonitor.MeasurementTime = currentMonitorMeasurementTime;
            northLeakageMonitor.MeasurementTime = currentMonitorMeasurementTime;
            northLeakageMonitor.Slope = northSlope;
            southLeakageMonitor.Slope = southSlope;
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

        public void UpdateLaserPhotodiodes()
        {
            double probePDValue = ReadAnalogInput(probeMonitorInputTask);
            window.SetTextBox(window.probeMonitorTextBox, probePDValue.ToString());
            double pumpPDValue = ReadAnalogInput(pumpMonitorInputTask);
            window.SetTextBox(window.pumpMonitorTextBox, pumpPDValue.ToString());
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
            // TODO: implement once we buy stepper controller.
        }

        #endregion

        #region Hardware control methods - local use only


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
        }

        internal void SetPumpShutter(bool enable)
        {
            SetDigitalLine("pumpShutter", enable);
        }

        internal void SetPump2Shutter(bool enable)
        {
            SetDigitalLine("pump2Shutter", enable);
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

        public void SetSteppingBBiasBVoltage()
        {
            double bBoxVoltage = Double.Parse(window.steppingBBoxBiasTextBox.Text);
            SetAnalogOutput(steppingBBiasAnalogOutputTask, bBoxVoltage);
        }


        public void SetSteppingBBiasBVoltage(double v)
        {
            window.SetTextBox(window.steppingBBoxBiasTextBox, v.ToString());
            SetAnalogOutput(steppingBBiasAnalogOutputTask, v);
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
