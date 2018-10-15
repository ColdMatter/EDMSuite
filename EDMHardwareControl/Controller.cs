using System;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
using System.Linq;

using DAQ.HAL;
using DAQ.Environment;
using Data;

using System.Diagnostics;

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
        private const double rfAWGOffAmplitude = -130.0;
        private const double rfAWGOffFrequency = 36.0;
        private const int eDischargeTime = 5000;
        private const int eBleedTime = 1000;
        private const int eWaitTime = 500;
        private const int eChargeTime = 5000;
        // E field monitor scale factors - what you need to multiply the monitor voltage by
        // to get the plate voltage
        public double CPlusMonitorScale { get { return 1000; } }
        public double CMinusMonitorScale { get { return 1000; } }
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
        //ICS4861A voltageController = (ICS4861A)Environs.Hardware.Instruments["4861"];
        HP34401A bCurrentMeter = (HP34401A)Environs.Hardware.Instruments["bCurrentMeter"];
        Agilent53131A rfCounter = (Agilent53131A)Environs.Hardware.Instruments["rfCounter"];
        SerialAgilent53131A rfCounter2 = (SerialAgilent53131A)Environs.Hardware.Instruments["rfCounter2"];
        HP438A rfPower = (HP438A)Environs.Hardware.Instruments["rfPower"];
        SerialDAQ bfieldCntrl = (SerialDAQ)Environs.Hardware.Instruments["BfieldController"];
        SerialMotorControllerBCD probePolCont = (SerialMotorControllerBCD)Environs.Hardware.Instruments["probePolControl"];
        SerialMotorControllerBCD pumpPolCont = (SerialMotorControllerBCD)Environs.Hardware.Instruments["pumpPolControl"];
        AnapicoSynth anapico = (AnapicoSynth)Environs.Hardware.Instruments["anapico"];
        NIPXI5670 rfAWG = (NIPXI5670)Environs.Hardware.Instruments["rfAWG"];
        

        Hashtable digitalTasks = new Hashtable();
        Hashtable digitalInputTasks = new Hashtable();
        //LeakageMonitor northLeakageMonitor =
        //   new LeakageMonitor((CounterChannel)Environs.Hardware.CounterChannels["northLeakage"], northSlope, northOffset, currentMonitorMeasurementTime);
        //LeakageMonitor southLeakageMonitor =
        //    new LeakageMonitor((CounterChannel)Environs.Hardware.CounterChannels["southLeakage"], southSlope, southOffset, currentMonitorMeasurementTime);
        LeakageMonitor northLeakageMonitor = new LeakageMonitor("northLeakage", northVolt2FreqSlope, northFreq2AmpSlope, northOffset);
        LeakageMonitor southLeakageMonitor = new LeakageMonitor("southLeakage", southVolt2FreqSlope, southFreq2AmpSlope, southOffset);
        LeyboldPTR225Gauge pressureMonitor = new LeyboldPTR225Gauge("Middle Penning gauge", "middlePenningGauge");
        BrilliantLaser yag = (BrilliantLaser)Environs.Hardware.YAG;
        Task bBoxAnalogOutputTask;
        //Task steppingBBiasAnalogOutputTask;
        Task pumpAOMAnalogOutputTask;
        Task probeAOMAnalogOutputTask;
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
        Task groundedInputTask;
        Task piMonitorTask;
        Task i2ErrorSignalInputTask;
        Task i2BiasOutputTask;
        Task uWaveDCFMAnalogOutputTask;
        //Task uWaveMixerAnalogOutputTask;
        Task VCO161AmpAnalogOutputTask;
        Task VCO161FreqAnalogOutputTask;
        Task VCO30AmpAnalogOutputTask;
        Task VCO30FreqAnalogOutputTask;
        Task VCO155AmpAnalogOutputTask;
        Task VCO155FreqAnalogOutputTask;
        Task pumpMicrowaveMixerVoltageAnalogOutputTask;
        Task bottomProbeMicrowaveMixerVoltageAnalogOutputTask;
        Task topProbeMicrowaveMixerVoltageAnalogOutputTask;

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
            CreateDigitalTask("pumprfSwitch");
            CreateDigitalTask("rfPowerAndFreqSelectSwitch");
            CreateDigitalTask("mwEnable");
            CreateDigitalTask("mwSelectPumpChannel");
            CreateDigitalTask("mwSelectTopProbeChannel");
            CreateDigitalTask("mwSelectBottomProbeChannel");
            CreateDigitalTask("fmSelect");
            CreateDigitalTask("attenuatorSelect");
            CreateDigitalTask("scramblerEnable");
            CreateDigitalTask("b");
            CreateDigitalTask("notB");
            CreateDigitalTask("db");
            CreateDigitalTask("notDB");
            CreateDigitalTask("piFlip");
            CreateDigitalTask("piFlipEnable");
            CreateDigitalTask("notPIFlipEnable");
            CreateDigitalTask("argonShutter");
            CreateDigitalTask("targetStepper");
            //CreateDigitalTask("rfCountSwBit1");
            //CreateDigitalTask("rfCountSwBit2");
            
            CreateDigitalTask("ttlSwitch");
            //CreateDigitalTask("I2PropSwitch");
            //CreateDigitalTask("I2IntSwitch");
            CreateDigitalTask("eSwitching");
            CreateDigitalTask("patternTTL");
            CreateDigitalTask("mwSwitching");

            // digitial input tasks


            // initialise the current leakage monitors
            northLeakageMonitor.Initialize();
            southLeakageMonitor.Initialize();

            // initialise the polarisers
            probePolCont.InitPolariserControl();
            pumpPolCont.InitPolariserControl();

            // analog outputs
            bBoxAnalogOutputTask = CreateAnalogOutputTask("bScan");
            //steppingBBiasAnalogOutputTask = CreateAnalogOutputTask("steppingBBias");
            //flPZTVAnalogOutputTask = CreateAnalogOutputTask("899ExternalScan");
            pumpAOMAnalogOutputTask = CreateAnalogOutputTask("pumpAOM");
            probeAOMAnalogOutputTask = CreateAnalogOutputTask("probeAOM");
            rf1AttenuatorOutputTask = CreateAnalogOutputTask("rf1Attenuator");
            rf2AttenuatorOutputTask = CreateAnalogOutputTask("rf2Attenuator");
            rf1FMOutputTask = CreateAnalogOutputTask("rf1FM");
            rf2FMOutputTask = CreateAnalogOutputTask("rf2FM");
            cPlusOutputTask = CreateAnalogOutputTask("cPlus");
            cMinusOutputTask = CreateAnalogOutputTask("cMinus");
            phaseScramblerVoltageOutputTask = CreateAnalogOutputTask("phaseScramblerVoltage");
            //flPZT2TempOutputTask = CreateAnalogOutputTask("flPZT2Temp");
            //flPZT2CurOutputTask = CreateAnalogOutputTask("flPZT2Cur");
            //flAOMAnalogOutputTask = CreateAnalogOutputTask("fibreAOM");
            i2BiasOutputTask = CreateAnalogOutputTask("I2LockBias");
            uWaveDCFMAnalogOutputTask = CreateAnalogOutputTask("uWaveDCFM");
            //uWaveMixerAnalogOutputTask = CreateAnalogOutputTask("uWaveMixerV");
            VCO161AmpAnalogOutputTask = CreateAnalogOutputTask("VCO161Amp");
            VCO161FreqAnalogOutputTask = CreateAnalogOutputTask("VCO161Freq");
            VCO30AmpAnalogOutputTask = CreateAnalogOutputTask("VCO30Amp");
            VCO30FreqAnalogOutputTask = CreateAnalogOutputTask("VCO30Freq");
            VCO155AmpAnalogOutputTask = CreateAnalogOutputTask("VCO155Amp");
            VCO155FreqAnalogOutputTask = CreateAnalogOutputTask("VCO155Freq");
            pumpMicrowaveMixerVoltageAnalogOutputTask = CreateAnalogOutputTask("pumpMixerV");
            bottomProbeMicrowaveMixerVoltageAnalogOutputTask = CreateAnalogOutputTask("bottomProbeMixerV");
            topProbeMicrowaveMixerVoltageAnalogOutputTask = CreateAnalogOutputTask("topProbeMixerV");


            

            // analog inputs
            probeMonitorInputTask = CreateAnalogInputTask("probePD", 0, 5);
            pumpMonitorInputTask = CreateAnalogInputTask("pumpPD", 0, 5);
            cPlusMonitorInputTask = CreateAnalogInputTask("cPlusMonitor");
            cMinusMonitorInputTask = CreateAnalogInputTask("cMinusMonitor");
            //rfPowerMonitorInputTask = CreateAnalogInputTask("rfPower");
            miniFlux1MonitorInputTask = CreateAnalogInputTask("miniFlux1");
            miniFlux2MonitorInputTask = CreateAnalogInputTask("miniFlux2");
            miniFlux3MonitorInputTask = CreateAnalogInputTask("miniFlux3");
            groundedInputTask = CreateAnalogInputTask("ground");
            piMonitorTask = CreateAnalogInputTask("piMonitor");
            //northLeakageInputTask = CreateAnalogInputTask("northLeakage");
            //southLeakageInputTask = CreateAnalogInputTask("southLeakage");
            //diodeRefCavInputTask = CreateAnalogInputTask("diodeLaserRefCavity");
            //diodeCurrentMonInputTask = CreateAnalogInputTask("diodeLaserCurrent");
            i2ErrorSignalInputTask = CreateAnalogInputTask("iodine");
            

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
            /*ReturnPolarizersToZero();*/
            rfAWG.Disconnect(); //End the ni-rfsg session
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
            public double pumpAOM;
            public double pumpAOMStep;
            public double pumpMicrowaveMixerVoltage;
            public double topProbeMicrowaveMixerVoltage;
            public double bottomProbeMicrowaveMixerVoltage;
            public double vco161Amp;
            public double vco30Amp;
            public double vco155Amp;
            public double vco161Freq;
            public double vco30Freq;
            public double vco155Freq;
            public double anapicoCWFreq;
            public double pumpmwDwellOnTime;
            public double pumpmwDwellOffTime;
            public double bottomProbemwDwellOnTime;
            public double bottomProbemwDwellOffTime;
            public double topProbemwDwellOnTime;
            public double topProbemwDwellOffTime;
            public double anapicof0Freq;
            public double anapicof1Freq;

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
            dataStore.flPZT = probeAOMVoltage;
            dataStore.flPZTStep = probeAOMStep;
            dataStore.overshootFactor = EOvershootFactor;
            dataStore.overshootHold = EOvershootHold;
            dataStore.pumpAOM = PumpAOMVoltage;
            dataStore.pumpAOMStep = PumpAOMStep;
            dataStore.pumpMicrowaveMixerVoltage = pumpMicrowaveMixerVoltage;
            dataStore.topProbeMicrowaveMixerVoltage = topProbeMicrowaveMixerVoltage;
            dataStore.bottomProbeMicrowaveMixerVoltage = bottomProbeMicrowaveMixerVoltage;
            dataStore.vco161Amp = VCO161AmpVoltage;
            dataStore.vco155Amp = VCO155AmpVoltage;
            dataStore.vco30Amp = VCO30AmpVoltage;
            dataStore.vco161Freq = VCO161FreqVoltage;
            dataStore.vco155Freq = VCO155FreqVoltage;
            dataStore.vco30Freq = VCO30FreqVoltage;
            dataStore.anapicoCWFreq = AnapicoCWFrequency;
            dataStore.anapicof0Freq = AnapicoFrequency0;
            dataStore.anapicof1Freq = AnapicoFrequency1;
            dataStore.pumpmwDwellOnTime = AnapicoPumpMWDwellOnTime;
            dataStore.pumpmwDwellOffTime = AnapicoPumpMWDwellOffTime;
            dataStore.bottomProbemwDwellOnTime = AnapicoBottomProbeMWDwellOnTime;
            dataStore.bottomProbemwDwellOffTime = AnapicoBottomProbeMWDwellOffTime;
            dataStore.topProbemwDwellOnTime = AnapicoTopProbeMWDwellOnTime;
            dataStore.topProbemwDwellOffTime = AnapicoTopProbeMWDwellOffTime;


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
                probeAOMVoltage = dataStore.flPZT;
                probeAOMStep = dataStore.flPZTStep;
                EOvershootFactor = dataStore.overshootFactor;
                EOvershootHold = dataStore.overshootHold;
                PumpAOMVoltage = dataStore.pumpAOM;
                PumpAOMStep = dataStore.pumpAOMStep;
                pumpMicrowaveMixerVoltage = dataStore.pumpMicrowaveMixerVoltage;
                topProbeMicrowaveMixerVoltage = dataStore.topProbeMicrowaveMixerVoltage;
                bottomProbeMicrowaveMixerVoltage = dataStore.bottomProbeMicrowaveMixerVoltage;
                VCO161AmpVoltage = dataStore.vco161Amp;
                VCO155AmpVoltage = dataStore.vco155Amp;
                VCO30AmpVoltage = dataStore.vco30Amp;
                VCO161FreqVoltage = dataStore.vco161Freq;
                VCO155FreqVoltage = dataStore.vco155Freq;
                VCO30FreqVoltage = dataStore.vco30Freq;
                AnapicoCWFrequency = dataStore.anapicoCWFreq;
                AnapicoFrequency0 = dataStore.anapicof0Freq;
                AnapicoFrequency1 = dataStore.anapicof1Freq;
                AnapicoPumpMWDwellOnTime = dataStore.pumpmwDwellOnTime;
                AnapicoPumpMWDwellOffTime = dataStore.pumpmwDwellOffTime;
                AnapicoBottomProbeMWDwellOnTime = dataStore.bottomProbemwDwellOnTime;
                AnapicoBottomProbeMWDwellOffTime = dataStore.bottomProbemwDwellOffTime;
                AnapicoTopProbeMWDwellOnTime = dataStore.topProbemwDwellOnTime;
                AnapicoTopProbeMWDwellOffTime = dataStore.topProbemwDwellOffTime;

            }
            catch (Exception)
            { Console.Out.WriteLine("Unable to load settings"); }
        }

        #endregion

        #region Public properties for controlling the hardware

        public double WindowValue(double value, double minValue, double maxValue)
        {
            if ((value < maxValue) && (value > minValue))
            {
                return value;
            }
            else
            {
                if (value < minValue)
                {
                    return minValue;
                }
                else
                {
                    return maxValue;
                }
            }

        }

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

        public void ChangePolarity(bool polarity)
        {
            window.SetCheckBox(window.ePolarityCheck, polarity);
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


        public double ProbeAOMFrequencyCentre
        {
            get
            {
                return Double.Parse(window.probeAOMFreqCentreTextBox.Text);
            }
        }

        public double ProbeAOMFrequencyStep
        {
            get
            {
                return Double.Parse(window.probeAOMFreqStepTextBox.Text);
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

        public double probeAOMVoltage
        {
            get
            {
                return Double.Parse(window.probeAOMVTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.probeAOMVTextBox, value.ToString());
            }
        }

       

        public double probeAOMStep
        {
            get
            {
                return Double.Parse(window.probeAOMStepTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.probeAOMStepTextBox, value.ToString());
            }
        }

        public double PumpAOMVoltage
        {
            get
            {
                return Double.Parse(window.pumpAOMVoltageTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.pumpAOMVoltageTextBox, value.ToString());
            }
        }

        public double PumpAOMStep
        {
            get
            {
                return Double.Parse(window.pumpAOMStepTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.pumpAOMStepTextBox, value.ToString());
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

        public double probePolAngle
        {
            set
            {
                window.SetTextBox(window.probePolMesAngle, value.ToString());
            }
            get
            {
                return Double.Parse(window.probePolMesAngle.Text);
            }
        }

        public double probePolBacklash
        {
            set
            {
                window.SetTextBox(window.probeBacklashTextBox, value.ToString());
            }
            get
            {
                return Double.Parse(window.probeBacklashTextBox.Text);
            }
        }

        public double pumpPolBacklash
        {
            set
            {
                window.SetTextBox(window.pumpBacklashTextBox, value.ToString());
            }
            get
            {
                return Double.Parse(window.pumpBacklashTextBox.Text);
            }
        }

        public double pumpPolAngle
        {
            set
            {
                window.SetTextBox(window.pumpPolMesAngle, value.ToString());
            }
            get
            {
                return Double.Parse(window.pumpPolMesAngle.Text);
            }
        }

        public double probePolVoltageFrac
        {
            set
            {
                window.probePolVoltTrackBar.Value = (int)Math.Round(100*value);
            }
            get
            {
                return ((double)window.probePolVoltTrackBar.Value)/100;
            }
        }

        public double pumpPolVoltageFrac
        {
            set
            {
                window.pumpPolVoltTrackBar.Value = (int)Math.Round(100 * value);
            }
            get
            {
                return ((double)window.pumpPolVoltTrackBar.Value) / 100;
            }
        }

        public bool probePolPosModeEnabled
        {
            set
            {
                window.probePolModeSelectSwitch.Value = value;
            }
            get
            {
                return window.probePolModeSelectSwitch.Value;
            }

        }

        public bool pumpPolPosModeEnabled
        {
            set
            {
                window.pumpPolModeSelectSwitch.Value = value;
            }
            get
            {
                return window.pumpPolModeSelectSwitch.Value;
            }

        }

        public double VCO161AmpVoltage
        {
            get
            {
                return Double.Parse(window.VCO161AmpVoltageTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.VCO161AmpVoltageTextBox, value.ToString());
            }
        }

        public double VCO161FreqVoltage
        {
            get
            {
                return Double.Parse(window.VCO161FreqTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.VCO161FreqTextBox, value.ToString());
            }
        }

        public double VCO30AmpVoltage
        {
            get
            {
                return Double.Parse(window.VCO30AmpVoltageTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.VCO30AmpVoltageTextBox, value.ToString());
            }
        }

        public double VCO30FreqVoltage
        {
            get
            {
                return Double.Parse(window.VCO30FreqVoltageTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.VCO30FreqVoltageTextBox, value.ToString());
            }
        }

        public double VCO155AmpVoltage
        {
            get
            {
                return Double.Parse(window.VCO155AmpVoltageTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.VCO155AmpVoltageTextBox, value.ToString());
            }
        }

        public double VCO155FreqVoltage
        {
            get
            {
                return Double.Parse(window.VCO155FreqVoltageTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.VCO155FreqVoltageTextBox, value.ToString());
            }
        }

        public double VCO161AmpIncrement
        {
            get
            {
                return Double.Parse(window.VCO161AmpStepTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.VCO161AmpStepTextBox, value.ToString());
            }
        }

        public double VCO161FreqIncrement
        {
            get
            {
                return Double.Parse(window.VCO161FreqStepTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.VCO161FreqStepTextBox, value.ToString());
            }
        }

        public double VCO30AmpIncrement
        {
            get
            {
                return Double.Parse(window.VCO30AmpStepTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.VCO30AmpStepTextBox, value.ToString());
            }
        }

        public double VCO30FreqIncrement
        {
            get
            {
                return Double.Parse(window.VCO30FreqStepTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.VCO30FreqStepTextBox, value.ToString());
            }
        }
        public double VCO155AmpIncrement
        {
            get
            {
                return Double.Parse(window.VCO155AmpStepTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.VCO155AmpStepTextBox, value.ToString());
            }
        }

        public double VCO155FreqIncrement
        {
            get
            {
                return Double.Parse(window.VCO155FreqStepTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.VCO155FreqStepTextBox, value.ToString());
            }
        }

        public double uWaveDCFMVoltage
        {
            get
            {
                return Double.Parse(window.uWaveDCFMTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.uWaveDCFMTextBox, value.ToString());
            }
        }

        public double pumpMicrowaveMixerVoltage
        {
            get
            {
                return Double.Parse(window.pumpMixerVoltageTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.pumpMixerVoltageTextBox, value.ToString());
            }
        }

        public double bottomProbeMicrowaveMixerVoltage
        {
            get
            {
                return Double.Parse(window.bottomProbeMixerVoltageTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.bottomProbeMixerVoltageTextBox, value.ToString());
            }
        }

        public double topProbeMicrowaveMixerVoltage
        {
            get
            {
                return Double.Parse(window.topProbeMixerVoltageTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.topProbeMixerVoltageTextBox, value.ToString());
            }
        }

        public double uWaveDCFMIncrement
        {
            get
            {
                return Double.Parse(window.uWaveDCFMStepTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.uWaveDCFMStepTextBox, value.ToString());
            }
        }

        public bool MwListSweepEnabled
        {
            get
            {
                return window.listSweepEnabledCheckBox.Checked;
            }
            set
            {
                window.SetCheckBox(window.listSweepEnabledCheckBox, value);
            }
        }

        public bool MwSwitchState
        {
            get
            {
                return window.microwaveStateCheckBox.Checked;
            }
            set
            {
                window.SetCheckBox(window.microwaveStateCheckBox, value);
            }
        }

        public double AnapicoCWFrequency
        {
            get
            {
                return Double.Parse(window.anapicoCwFreqBox.Text);
            }
            set
            {
                window.SetTextBox(window.anapicoCwFreqBox, value.ToString());
            }
        }

        public double AnapicoFrequency0
        {
            get
            {
                return Double.Parse(window.anapicof0FreqTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.anapicof0FreqTextBox, value.ToString());
            }
        }

        public double AnapicoFrequency1
        {
            get
            {
                return Double.Parse(window.anapicof1FreqTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.anapicof1FreqTextBox, value.ToString());
            }
        }

        public double AnapicoPumpMWDwellOnTime
        {
            get
            {
                return Double.Parse(window.pumpMWDwellOnTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.pumpMWDwellOnTextBox, value.ToString("E2"));
            }
        }

        public double AnapicoPumpMWDwellOffTime
        {
            get
            {
                return Double.Parse(window.pumpMWDwellOffTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.pumpMWDwellOffTextBox, value.ToString("E2"));
            }
        }

        public double AnapicoBottomProbeMWDwellOnTime
        {
            get
            {
                return Double.Parse(window.bottomProbeMWDwellOnTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.bottomProbeMWDwellOnTextBox, value.ToString("E2"));
            }
        }

        public double AnapicoBottomProbeMWDwellOffTime
        {
            get
            {
                return Double.Parse(window.bottomProbeMWDwellOffTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.bottomProbeMWDwellOffTextBox, value.ToString("E2"));
            }
        }

        public double AnapicoTopProbeMWDwellOnTime
        {
            get
            {
                return Double.Parse(window.topProbeMWDwellOnTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.topProbeMWDwellOnTextBox, value.ToString("E2"));
            }
        }

        public double AnapicoTopProbeMWDwellOffTime
        {
            get
            {
                return Double.Parse(window.topProbeMWDwellOffTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.topProbeMWDwellOffTextBox, value.ToString("E2"));
            }
        }

        public bool AnapicoPumpMWf0Indicator
        {
            set
            {
                window.SetLED(window.pumpMWf0Indicator, value);
            }
        }

        public bool AnapicoPumpMWf1Indicator
        {
            set
            {
                window.SetLED(window.pumpMWf1Indicator, value);
            }
        }

        public bool AnapicoBottomProbeMWf0Indicator
        {
            set
            {
                window.SetLED(window.bottomProbeMWf0Indicator, value);
            }
        }

        public bool AnapicoBottomProbeMWf1Indicator
        {
            set
            {
                window.SetLED(window.bottomProbeMWf1Indicator, value);
            }
        }

        public bool AnapicoTopProbeMWf0Indicator
        {
            set
            {
                window.SetLED(window.topProbeMWf0Indicator, value);
            }
        }

        public bool AnapicoTopProbeMWf1Indicator
        {
            set
            {
                window.SetLED(window.topProbeMWf1Indicator, value);
            }
        }

        public string AnapicoCurrentList
        {
            set
            {
                window.SetTextBox(window.displayCurrentListBox, value);
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

        public double RF1aGuess
        {
            get
            {
                return Double.Parse(window.rf1aCentreGuessTextBox.Text);
            }
        }

        public double RF2aGuess
        {
            get
            {
                return Double.Parse(window.rf2aCentreGuessTextBox.Text);
            }
        }

        public double RF1fGuess
        {
            get
            {
                return Double.Parse(window.rf1fCentreGuessTextBox.Text);
            }
        }

        public double RF2fGuess
        {
            get
            {
                return Double.Parse(window.rf2fCentreGuessTextBox.Text);
            }
        }
        public double SteppingBiasVoltage
        {
            set
            {
                window.SetTextBox(window.steppingBBoxBiasTextBox, value.ToString());
            }

            get
            {
                return Double.Parse(window.steppingBBoxBiasTextBox.Text);
            }
        }
        private double miniFlux1Voltage;

        public double MiniFlux1Voltage
        {
            get
            {
                return miniFlux1Voltage;
            }
        }

        private double piFlipMonVoltage;
        public double PiFlipMonVoltage
        {
            get
            {
                return piFlipMonVoltage;
            }
        }

        private double cPlusMonitorVoltage;
        private double cMinusMonitorVoltage;

        private double hpVoltage;
        public double HPVoltage
        {
         get
         {
             return hpVoltage;
         }
        }


        public double CPlusMonitorVoltage
        {
            get
            {
                return cPlusMonitorVoltage;
            }
        }

        public double CMinusMonitorVoltage
        {
            get
            {
                return cMinusMonitorVoltage;
            }
        }


        private double miniFlux2Voltage;
        public double MiniFlux2Voltage
        {
 
            get
            {
                return miniFlux2Voltage;
            }
        }

        private double miniFlux3Voltage;
        public double MiniFlux3Voltage
        {
            get
            {
                return miniFlux3Voltage;
            }
        }

        private double pumpPDVoltage;
        public double PumpPDVoltage
        {
            get
            {
                return pumpPDVoltage;
            }
        }

        private double probePDVoltage;
        public double ProbePDVoltage
        {

            get
            {
                return probePDVoltage;
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


        public double PumpAOMFrequencyCentre
        {
            get
            {
                return Double.Parse(window.pumpAOMFreqCentreTextBox.Text);
            }
        }

        public double PumpAOMFrequencyStep
        {
            get
            {
                return Double.Parse(window.pumpAOMFreqStepTextBox.Text);
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

        public void Switch(string channel, bool state)
        {
            switch (channel)
            {
                case "eChan":
                    SwitchEAndWait(state);
                    break;
                case "probeAOM": //probe laser
                    SwitchLF1(state);
                    break;
                case "pumpAOM": //probe laser
                    SwitchLF2(state);
                    break;
                case "mwChan":
                    SwitchMwAndWait(state);
                    break;
            }
        }

        public void ReSwitch(string channel, bool state)
        {
            switch (channel)
            {
                case "eChan":
                    break;
                case "probeAOM": //probe laser
                    break;
                case "pumpAOM": //probe laser
                    break;
                case "mwChan":
                    ReSwitchMwAndWait(state);
                    break;
            }
        }


        private bool lf1State;
        private double calculateProbeAOMFrequency(bool lf1State)
        {

            return probeAOMVoltage + (lf1State ? probeAOMStep : -probeAOMStep);
        }
        public void SwitchLF1(bool lf1State)
        {
            this.lf1State = lf1State;

            SetAnalogOutput(probeAOMAnalogOutputTask, calculateProbeAOMFrequency(lf1State));
        }

        private bool lf2State;
        private double calculatePumpAOMFrequency(bool lf2State)
        {

            return PumpAOMVoltage + (lf2State ? PumpAOMStep : -PumpAOMStep);
        }
        public void SwitchLF2(bool lf2State)
        {
            this.lf2State = lf2State;

            SetAnalogOutput(pumpAOMAnalogOutputTask, calculatePumpAOMFrequency(lf2State));
        }


        public void FieldsOff()
        {
            CPlusVoltage = 0;
            CMinusVoltage = 0;
            CPlusOffVoltage = 0;
            CMinusOffVoltage = 0;
            UpdateVoltages();
            EFieldEnabled = false;
        }

        private bool switchingEfield; 
        public bool SwitchingEfields
        {
            get
            {
                return switchingEfield;
            }
            set
            {
                switchingEfield = value;
                SetDigitalLine("eSwitching", value);
            }

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
                window.EnableControl(window.ePolarityCheck, false);
                window.EnableControl(window.eBleedCheck, false);
                switchThread.Start();
            }
        }

        double kPositiveChargeMin = 2;
        double kPositiveChargeMax = 20;
        double kNegativeChargeMin = -2;
        double kNegativeChargeMax = -20;

        // this function switches the E field polarity with ramped turn on and off. 
        // It also switches off the Synth to prevent rf discharges while the fields are off
        public void SwitchEWorker()
        {
            
            bool startingSynthState = GreenSynthEnabled;
            lock (switchingLock)
            {
                // raise flag for switching E-fields
                SwitchingEfields = true;
                //switch off the synth
                GreenSynthEnabled = false;
                // we always switch, even if it's into the same state.
                window.SetLED(window.switchingLED, true);
                // Add any asymmetry
                // ramp the field down if on
                if (EFieldEnabled)
                {
                    RampVoltages(CPlusVoltage, CPlusOffVoltage, CMinusVoltage, CMinusOffVoltage, 20, ERampDownTime); 
                }
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
                    { }
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
            GreenSynthEnabled = startingSynthState;
            ESwitchDone();
        }

        public void SwitchEWorkerForIRamp()
        {

            bool startingSynthState = GreenSynthEnabled;
            lock (switchingLock)
            {
                // raise flag for switching E-fields
                SwitchingEfields = true;
                //switch off the synth
                GreenSynthEnabled = false;
                // we always switch, even if it's into the same state.
                window.SetLED(window.switchingLED, true);
                // Add any asymmetry
                // ramp the field down if on
                int eSign;
                if(EFieldPolarity){eSign=1;}else{eSign=-1;}
                if (EFieldEnabled)
                {
                    RampVoltages(eSign * CPlusVoltage, eSign * CPlusOffVoltage, -eSign * CMinusVoltage, -eSign * CMinusOffVoltage, 5, ERampDownTime);
                }
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
                RampVoltages(-eSign * CPlusOffVoltage, -eSign * EOvershootFactor * cPlusToWrite,
                               eSign * CMinusOffVoltage, eSign * EOvershootFactor * cMinusToWrite, 5, ERampUpTime);
                // impose the overshoot delay
                Thread.Sleep((int)(1000 * EOvershootHold));
                // ramp back to the control point
                RampVoltages(-eSign * EOvershootFactor * cPlusToWrite, -eSign * cPlusToWrite,
                                eSign * EOvershootFactor * cMinusToWrite, eSign * cMinusToWrite, 5, 0);
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
                    { }
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
            GreenSynthEnabled = startingSynthState;
            ESwitchDone();
        }
        private void activateEAlarm(bool newEPolarity)
        {
            window.AddAlert("E-switch - switching to state: " + newEPolarity + "; manual state: " + EManualState + 
                "; north current: " + lastNorthCurrent + "; south current: " + lastSouthCurrent + " .");
        }

        private void ESwitchDone()
        {
            SwitchingEfields = false; 
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
                window.EnableControl(window.ePolarityCheck, false);
                window.EnableControl(window.eBleedCheck, false);
                //SetAnalogOutput(cPlusOutputTask, CPlusVoltage);
                //SetAnalogOutput(cMinusOutputTask, CMinusVoltage);
            }
            else
            {
                SetAnalogOutput(cPlusOutputTask, cPlusOff);
                SetAnalogOutput(cMinusOutputTask, cMinusOff);
                window.EnableControl(window.ePolarityCheck, true);
                window.EnableControl(window.eBleedCheck, true);
            }
        }

        public void SetCPlusVoltage(double voltage)
        {
            CPlusVoltage = voltage;
            UpdateVoltages();
        }

        public void SetCMinusVoltage(double voltage)
        {
            CMinusVoltage = voltage;
            UpdateVoltages();
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

        //This method is supposed to automate the process of setting rf attenuator settings before EDM data taking. 
        //It assumes that the correct measured rfxa centre power is in the rfxaCentreGuessTextBoxes, and parses this 
        //to use as its target.

        public void AutomaticRFxACalculation()
        {

            double newRF1a = 0;
            double newRF2a = 0;
            int a = 0;
            double gain = 0.2;

            SetAttenutatorVoltages();
            while (newRF1a != RF1aGuess || newRF2a != RF2aGuess && a < 5)
            {
                UpdateRFPowerMonitor();
                newRF1a = RF1PowerCentre;
                newRF2a = RF2PowerCentre;
                RF1AttCentre = windowVoltage(RF1AttCentre + gain * (RF1aGuess - newRF1a), 0, 5);
                RF2AttCentre = windowVoltage(RF2AttCentre + gain * (RF2aGuess - newRF2a), 0, 5);
                SetAttenutatorVoltages();
                a++;
            }

        }

        //This method is supposed to automate the process of setting rf DCFM settings before EDM data taking. 
        //It assumes that the correct measured rfxf centre freq is in the rfxfCentreGuessTextBoxes, and parses this 
        //to use as its target.

        public void AutomaticRFxFCalculation()
        {

            double newRF1f = 0;
            double newRF2f = 0;
            int a = 0;
            double gain = 0.000025;

            SetFMVoltages();
            while (newRF1f != RF1fGuess || newRF2f != RF2fGuess && a < 5)
            {
                UpdateRFFrequencyMonitor();
                newRF1f = RF1FrequencyCentre;
                newRF2f = RF2FrequencyCentre;
                RF1FMCentre = windowVoltage(RF1FMCentre + gain * (RF1fGuess - newRF1f), 0, 1);
                RF2FMCentre = windowVoltage(RF2FMCentre + gain * (RF2fGuess - newRF2f), 0, 1);

                SetFMVoltages();
                a++;
            }

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

        public void UpdateBVoltage()
        {
            hpVoltage=bCurrentMeter.ReadVoltage();
        }

        public void FlipDB()
        {
            CalFlipEnabled = false;
            System.Threading.Thread.Sleep(1000);
            CalFlipEnabled = true;
            System.Threading.Thread.Sleep(1000);
            CalFlipEnabled = false;
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
            cPlusMonitorVoltage = ReadAnalogInput(cPlusMonitorInputTask);
            cMinusMonitorVoltage = -ReadAnalogInput(cMinusMonitorInputTask);
        }
        public void UpdateVMonitorUI()
        {
            UpdateVMonitor();
            window.SetTextBox(window.cPlusVMonitorTextBox, CPlusMonitorVoltage.ToString());
            window.SetTextBox(window.cMinusVMonitorTextBox, CMinusMonitorVoltage.ToString());
        }

        public double LastNorthCurrent
        {
            get { return northLeakageMonitor.GetCurrent(); }
        }

        public double LastSouthCurrent
        {
            get { return southLeakageMonitor.GetCurrent(); }
        }

        private double lastNorthCurrent;
        private double lastSouthCurrent;
        private Queue<double> nCurrentSamples = new Queue<double>();
        private Queue<double> sCurrentSamples = new Queue<double>();
        public void UpdateIMonitor()
        {
            ReconfigureIMonitors();

            //sample the leakage current
            lastNorthCurrent = northLeakageMonitor.GetCurrent();
            lastSouthCurrent = southLeakageMonitor.GetCurrent();

            //plot the most recent samples
            window.PlotYAppend(window.leakageGraph, window.northLeakagePlot,
                        new double[] { lastNorthCurrent });
            window.PlotYAppend(window.leakageGraph, window.southLeakagePlot,
                                    new double[] { lastSouthCurrent });

            //add samples to Queues for averaging
            nCurrentSamples.Enqueue(lastNorthCurrent);
            sCurrentSamples.Enqueue(lastSouthCurrent);

            //drop samples when array is larger than the moving average sample length
            while (nCurrentSamples.Count > movingAverageSampleLength)
            {
                nCurrentSamples.Dequeue();
                sCurrentSamples.Dequeue();
            }

            //average samples
            double nAvCurr = nCurrentSamples.Average();
            double sAvCurr = sCurrentSamples.Average();
            double nAvCurrErr = Math.Sqrt((nCurrentSamples.Sum(d => Math.Pow(d - nAvCurr, 2))) / (nCurrentSamples.Count() - 1)) / (Math.Sqrt(nCurrentSamples.Count()));
            double sAvCurrErr = Math.Sqrt((sCurrentSamples.Sum(d => Math.Pow(d - sAvCurr, 2))) / (sCurrentSamples.Count() - 1)) / (Math.Sqrt(sCurrentSamples.Count()));

            //update text boxes
            window.SetTextBox(window.northIMonitorTextBox, (nAvCurr).ToString());
            window.SetTextBox(window.northIMonitorErrorTextBox, (nAvCurrErr).ToString());
            window.SetTextBox(window.southIMonitorTextBox, (sAvCurr).ToString());
            window.SetTextBox(window.southIMonitorErrorTextBox, (sAvCurrErr).ToString());
        }

        public void ClearIMonitorAv()
        {
            nCurrentSamples.Clear();
            sCurrentSamples.Clear();
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
            double ground = ReadAnalogInput(groundedInputTask);
            lastNorthCurrent = northLeakageMonitor.GetCurrent();
            ground = ReadAnalogInput(groundedInputTask);
            lastSouthCurrent = southLeakageMonitor.GetCurrent();
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


        private JSONSerializer serializer;
        public void StartLoggingCurrent()
        {
            serializer = new JSONSerializer();
            string initialDataDir = Environs.FileSystem.GetDataDirectory(
                                                (String)Environs.FileSystem.Paths["scanMasterDataPath"]);
            serializer.StartLogFile(initialDataDir +
                Environs.FileSystem.GenerateNextDataFileName() + ".json");
            serializer.StartProcessingData();
        }
        public void StopLoggingCurrent()
        {
            serializer.EndLogFile();
        }

        private Thread iMonitorPollThread;
        private int iMonitorPollPeriod = 200;
        private Object iMonitorLock;
        private bool iMonitorFlag;
        private int movingAverageSampleLength = 10;
        internal void StartIMonitorPoll()
        {
            iMonitorPollThread = new Thread(new ThreadStart(IMonitorPollWorker));
            window.EnableControl(window.startIMonitorPollButton, false);
            window.EnableControl(window.stopIMonitorPollButton, true);
            iMonitorPollPeriod = Int32.Parse(window.iMonitorPollPeriod.Text);
            movingAverageSampleLength = Int32.Parse(window.currentMonitorSampleLengthTextBox.Text);
            nCurrentSamples.Clear();
            sCurrentSamples.Clear();
            iMonitorLock = new Object();
            iMonitorFlag = false;
            iMonitorPollThread.Start();
        }

        internal void StopIMonitorPoll()
        {
            iMonitorFlag = true;
        }
        private void IMonitorPollWorker()
        {
            for ( ; ; )
            {
                Thread.Sleep(iMonitorPollPeriod);
                lock (iMonitorLock)
                {
                    UpdateIMonitor();
                    if (window.logCurrentDataCheckBox.Checked)
                    {
                        serializer.AddData(new CurrentMonitorDataLog(DateTime.Now,
                            iMonitorPollPeriod,
                            northVolt2FreqSlope,
                            southVolt2FreqSlope,
                            northFreq2AmpSlope,
                            northOffset,
                            southOffset,
                            lastNorthCurrent,
                            lastSouthCurrent,
                            switchingEfield,
                            window.ePolarityCheck.Checked));
                    }
                    if(iMonitorFlag)
                    {
                        iMonitorFlag = false;
                        break;
                    }
                }
            }
            window.EnableControl(window.startIMonitorPollButton, true);
            window.EnableControl(window.stopIMonitorPollButton, false);
        }

        public void UpdateLaserPhotodiodes()
        {
            double groundValue = ReadAnalogInput(groundedInputTask);
            probePDVoltage = ReadAnalogInput(probeMonitorInputTask);
            groundValue = ReadAnalogInput(groundedInputTask);
            pumpPDVoltage = ReadAnalogInput(pumpMonitorInputTask);
        }

        public void UpdateLaserPhotodiodesUI()
        {
            UpdateLaserPhotodiodes();
            window.SetTextBox(window.probeMonitorTextBox, probePDVoltage.ToString());
            window.SetTextBox(window.pumpMonitorTextBox, pumpPDVoltage.ToString());

        }

        public void UpdateMiniFluxgates()
        {
            double groundValue = ReadAnalogInput(groundedInputTask);
            miniFlux1Voltage = ReadAnalogInput(miniFlux1MonitorInputTask);
            groundValue = ReadAnalogInput(groundedInputTask);
            miniFlux2Voltage = ReadAnalogInput(miniFlux2MonitorInputTask);
            groundValue = ReadAnalogInput(groundedInputTask);
            miniFlux3Voltage = ReadAnalogInput(miniFlux3MonitorInputTask);
        }

        public void UpdateMiniFluxgatesUI()
        {
            UpdateMiniFluxgates();
            window.SetTextBox(window.miniFlux1TextBox , miniFlux1Voltage.ToString());
            window.SetTextBox(window.miniFlux2TextBox, miniFlux2Voltage.ToString());
            window.SetTextBox(window.miniFlux3TextBox, miniFlux3Voltage.ToString());
        }

        public void UpdatePiMonitor()
        {
            piFlipMonVoltage = ReadAnalogInput(piMonitorTask);
        }
        public void UpdatePiMonitorUI()
        {
            UpdatePiMonitor();
            window.SetTextBox(window.piFlipMonTextBox, PiFlipMonVoltage.ToString());
        }

        public void CheckPiMonitor()
        {
            SetPhaseFlip1(true);
            SetPhaseFlip2(false);
            UpdatePiMonitor();
            double piMonitorV1 = PiFlipMonVoltage;
            window.SetTextBox(window.piMonitor1TextBox, piMonitorV1.ToString());
            SetPhaseFlip2(true);
            UpdatePiMonitor();
            double piMonitorV2 = PiFlipMonVoltage;
            window.SetTextBox(window.piMonitor2TextBox, piMonitorV2.ToString());
            SetPhaseFlip1(false);
            SetPhaseFlip2(false);
            if (Math.Round(piMonitorV1)==Math.Round(piMonitorV2)) window.AddAlert("Pi-flip - V1: " + piMonitorV1 + "; V2: " + piMonitorV1 + " .");
        }

        public void UpdateDiodeCurrentMonitor()
        {
            //double diodeCurrentMonValue = ReadAnalogInput(diodeCurrentMonInputTask);
            //window.SetTextBox(window.diodeCurrentTextBox, diodeCurrentMonValue.ToString());
        }

        public void UpdateDiodeCurrentGraphAndMonitor()
        {
            //double diodeCurrentMonValue = ReadAnalogInput(diodeCurrentMonInputTask);
            //window.SetTextBox(window.diodeCurrentTextBox, diodeCurrentMonValue.ToString());
            //window.PlotYAppend(window.diodeCurrentGraph, window.diodeCurrentPlot,
                                    //new double[] { diodeCurrentMonValue });
        }

        public void UpdateI2ErrorSigMonitor()
        {
            double i2ErrorSigVal = ReadAnalogInput(i2ErrorSignalInputTask);
            window.SetTextBox(window.I2ErrorSigTextBox, i2ErrorSigVal.ToString());
        }

        public void UpdateI2ErrorSigGraphAndMonitor()
        {
            double i2ErrorSigVal = ReadAnalogInput(i2ErrorSignalInputTask);
            window.SetTextBox(window.I2ErrorSigTextBox, i2ErrorSigVal.ToString());
            window.PlotYAppend(window.I2ErrorSigGraph, window.I2ErrorSigPlot,
                new double[] { i2ErrorSigVal });
        }

        private Thread i2ErrorSigMonitorPollThread;
        private object i2ErrorSigMonitorLock = new object();
        private bool i2ErrorSigMonitorStopFlag = false;
        private int i2ErrorSigMonitorPollPeriod = 250;
        internal void StartI2ErrorSigPoll()
        {
            lock (i2ErrorSigMonitorLock)
            {
                i2ErrorSigMonitorPollThread = new Thread(new ThreadStart(i2ErrorSigMonitorPollWorker));
                window.EnableControl(window.startI2ErrorSigPollButton, false);
                window.EnableControl(window.stopI2ErrorSigPollButton, true);
                i2ErrorSigMonitorPollPeriod = Int32.Parse(window.I2ErrorPollPeriodTextBox.Text);
                i2ErrorSigMonitorPollThread.Start();
            }

        }

        internal void StopI2ErrorSigPoll()
        {
            lock (i2ErrorSigMonitorLock) i2ErrorSigMonitorStopFlag = true;
        }

        private void i2ErrorSigMonitorPollWorker()
        {
            for (; ; )
            {
                Thread.Sleep(i2ErrorSigMonitorPollPeriod);
                UpdateI2ErrorSigGraphAndMonitor();
                lock (i2ErrorSigMonitorLock)
                {
                    if (i2ErrorSigMonitorStopFlag)
                    {
                        i2ErrorSigMonitorStopFlag = false;
                        break;
                    }
                }
            }
            window.EnableControl(window.startI2ErrorSigPollButton, true);
            window.EnableControl(window.stopI2ErrorSigPollButton, false);
        }

        public void SetI2Bias(double value)
        {
                SetAnalogOutput(i2BiasOutputTask, value);
                window.I2BiasVoltageTextBox.Text = value.ToString();
            return;
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

        public void SetTargetStepperHigh()
        {
                SetDigitalLine("targetStepper", true);
                Thread.Sleep(5);
        }

        public void SetTargetStepperLow()
        {
            SetDigitalLine("targetStepper", false);
            Thread.Sleep(5);
        }


        public void UpdateProbeAOMFreqMonitor()
        {
            double diff = 0;
            double I2PlusFreq = 0;
            double I2MinusFreq = 0;
            int averages = 1;       //The averaging is to remove glitches in the measured step caused by the AOM modulation, which would play havoc with the Pump AOM locking scheme 

                window.SetRadioButton(window.probeAOMStepPlusButton, true);
                UpdateProbeAOMV();
                Thread.Sleep(10);
                rfCounter.Channel = 2;
                for (int i = 1; i <= averages; i++)
                {
                    I2PlusFreq = I2PlusFreq+rfCounter.Frequency;
                    Thread.Sleep(10);
                }

                I2PlusFreq = I2PlusFreq / averages;
                window.SetTextBox(window.probeAOMFreqPlusTextBox, String.Format("{0:F0}", I2PlusFreq));

                window.SetRadioButton(window.probeAOMMinusButton, true);
                UpdateProbeAOMV();
                Thread.Sleep(10);
                for (int i = 1; i <= averages; i++)
                {
                    I2MinusFreq = I2MinusFreq + rfCounter.Frequency;
                    Thread.Sleep(10);
                }
                I2MinusFreq = I2MinusFreq / averages;

                diff = (I2PlusFreq - I2MinusFreq) / 2;


            window.SetTextBox(window.probeAOMFreqMinusTextBox, String.Format("{0:F0}", I2MinusFreq));
            window.SetTextBox(window.probeAOMFreqCentreTextBox, String.Format("{0:F0}", ((I2PlusFreq + I2MinusFreq) / 2)));
            window.SetTextBox(window.probeAOMFreqStepTextBox, String.Format("{0:F0}", ((I2PlusFreq - I2MinusFreq) / 2)));
        }

       
        

        public void UpdatePumpAOMFreqMonitor()
        {
            window.SetRadioButton(window.pumpAOMStepPlusButton, true);
            UpdatePumpAOM();
            Thread.Sleep(10);
            double pumpAOMPlusFreq = rfCounter2.Frequency();
            window.SetTextBox(window.pumpAOMFreqPlusTextBox, String.Format("{0:F0}", pumpAOMPlusFreq));

            window.SetRadioButton(window.pumpAOMStepMinusButton, true);
            UpdatePumpAOM();
            Thread.Sleep(10);
            double pumpAOMMinusFreq = rfCounter2.Frequency();
            window.SetTextBox(window.pumpAOMFreqMinusTextBox, String.Format("{0:F0}", pumpAOMMinusFreq));
            window.SetTextBox(window.pumpAOMFreqCentreTextBox, String.Format("{0:F0}", ((pumpAOMPlusFreq + pumpAOMMinusFreq) / 2)));
            window.SetTextBox(window.pumpAOMFreqStepTextBox, String.Format("{0:F0}", ((pumpAOMPlusFreq - pumpAOMMinusFreq) / 2)));

        }



        //internal void UpdateProbePolarizerAngle()
        //{
        //    motorController1.MoveAbsoluteEx(0,
        //        (int)Double.Parse(window.probePolarizerAngleTextBox.Text), 0, true);
        //}

        //internal void UpdatePumpPolarizerAngle()
        //{
        //    motorController2.MoveAbsoluteEx(0,
        //        (int)Double.Parse(window.pumpPolarizerAngleTextBox.Text), 0, true);
        //}

        //public void SetProbePolarizerAngle(double theta)
        //{
        //    window.SetTextBox(window.probePolarizerAngleTextBox, theta.ToString());
        //    UpdateProbePolarizerAngle();
        //}

        //public void SetPumpPolarizerAngle(double theta)
        //{
        //    window.SetTextBox(window.pumpPolarizerAngleTextBox, theta.ToString());
        //    UpdatePumpPolarizerAngle();
        //}

        public void EnableGreenSynth(bool enable)
        {
            try
            {
                greenSynth.Connect();
            }
            catch (Exception e)
            {
                MessageBox.Show("Connect error: " + e.Message);
            }

            try
            {
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
            }

            catch (Exception e)
            {
                MessageBox.Show("Error while setting parameters: " + e.Message);
            }

            try
            {
                greenSynth.Disconnect();
            }
            catch (Exception e)
            {
                MessageBox.Show("Disconnect error: " + e.Message);
            }
            
        }




        public void EnableRFSwitch(bool enable)
        {
            SetDigitalLine("rfSwitch", enable);
        }

        public void EnablePumpRFSwitch(bool enable)
        {
            SetDigitalLine("pumprfSwitch", enable);
        }

        public void ConnectRFToSensorSwitch(bool fromSyntToSensor)
        {
            SetDigitalLine("rfPowerAndFreqSelectSwitch", fromSyntToSensor);
        }


        public void EnableMicrowaves(bool enable)
        {
            SetDigitalLine("mwEnable", enable);
        }

        public void SendMicrowavesToPump(bool enable)
        {
            SetDigitalLine("mwSelectPumpChannel", enable);
        }

        public void SendMicrowavesToBottomProbe(bool enable)
        {
            SetDigitalLine("mwSelectBottomProbeChannel", enable);
        }

        public void SendMicrowavesToTopProbe(bool enable)
        {
            SetDigitalLine("mwSelectTopProbeChannel", enable);
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
            SetDigitalLine("notB", !enable);
        }

        public void SetCalFlip(bool enable)
        {
            SetDigitalLine("db", enable);
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



        public void SetArgonShutter(bool enable)
        {
            SetDigitalLine("argonShutter", enable);
        }

        

        public void SetSwitchTTL(bool enable)
        {
            SetDigitalLine("ttlSwitch", enable);
            window.switchScanTTLSwitch.Value = enable;
        }

        public void SetPatternTTL(bool enable)
        {
            SetDigitalLine("patternTTL", enable);
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

        //public void SetSteppingBBiasVoltage()
        //{
        //    double bBoxVoltage = Double.Parse(window.steppingBBoxBiasTextBox.Text);
        //    SetAnalogOutput(steppingBBiasAnalogOutputTask, bBoxVoltage);
        //}

        public void SetSteppingBBiasVoltage()
        {
            bfieldCntrl.SetOut1(SteppingBiasVoltage);
        }

        public void SetRandomProbePosition()
        {
            probePolCont.SetRandomPosition(probePolBacklash);
        }

        public void SetRandomPumpPosition()
        {
            pumpPolCont.SetRandomPosition(pumpPolBacklash);
        }


        public void SetProbePolAngle()
        {
            double probePolariserAngle = Double.Parse(window.probePolSetAngle.Text);
            probePolCont.SetPositionWithBacklash(probePolariserAngle,probePolBacklash);
        }
        
        public void SetPumpPolAngle()
        {
            double pumpPolariserAngle = Double.Parse(window.pumpPolSetAngle.Text);
            pumpPolCont.SetPositionWithBacklash(pumpPolariserAngle,pumpPolBacklash);
        }
        
        public void UpdateProbePolAngleMonitor()
        {
            probePolAngle = probePolCont.MeasurePosition();
        }

        public void UpdatePumpPolAngleMonitor()
        {
            pumpPolAngle = pumpPolCont.MeasurePosition();
        }

        public void UpdateProbePolMode()
        {
            if (probePolPosModeEnabled == true)
            {
                window.probePolSetAngle.Enabled = true;
                window.probePolSetAngle.Enabled = true;
                window.probePolVoltTrackBar.Enabled = false;
                window.probePolVoltStopButton.Enabled = false;
                window.probePolVoltTrackBar.Value = 0;

                UpdateProbePolAngleMonitor(); //Makes sure that the monitors are reading the current position value

                probePolCont.PositionModeEnable(probePolAngle); //enables position mode with the correct polariser position

            }
            else
            {
                window.probePolSetAngle.Enabled = false;
                window.probePolSetAngle.Enabled = false;
                window.probePolVoltTrackBar.Enabled = true;
                window.probePolVoltStopButton.Enabled = true;
                window.probePolVoltTrackBar.Value = 0;

                probePolCont.VoltageModeEnable();
            }

        }

        public void UpdatePumpPolMode()
        {
            if (pumpPolPosModeEnabled == true)
            {
                window.pumpPolSetAngle.Enabled = true;
                window.pumpPolSetAngle.Enabled = true;
                window.pumpPolVoltTrackBar.Enabled = false;
                window.pumpPolVoltStopButton.Enabled = false;
                window.pumpPolVoltTrackBar.Value = 0;

                UpdatePumpPolAngleMonitor(); //Makes sure that the monitors are reading the current position value

                pumpPolCont.PositionModeEnable(pumpPolAngle); //enables position mode with the correct polariser position

            }
            else
            {
                window.pumpPolSetAngle.Enabled = false;
                window.pumpPolSetAngle.Enabled = false;
                window.pumpPolVoltTrackBar.Enabled = true;
                window.pumpPolVoltStopButton.Enabled = true;
                window.pumpPolVoltTrackBar.Value = 0;

                pumpPolCont.VoltageModeEnable();
            }

        }

        public void SetProbePolVoltage()
        {
            double probeVoltageToSend = probePolVoltageFrac;
            probePolCont.SetMotorVoltage(probeVoltageToSend);
        }

        public void SetPumpPolVoltage()
        {
            double pumpVoltageToSend = pumpPolVoltageFrac;
            pumpPolCont.SetMotorVoltage(pumpVoltageToSend);
        }

        public void SetProbePolVoltageZero()
        {
            probePolVoltageFrac = 0;
            SetProbePolVoltage();
        }

        public void SetPumpPolVoltageZero()
        {
            pumpPolVoltageFrac = 0;
            SetPumpPolVoltage();
        }

        public void SetProbePolAngleZero()
        {
            probePolCont.PositionModeEnable(0);
            UpdateProbePolAngleMonitor();
        }
        public void SetPumpPolAngleZero()
        {
            pumpPolCont.PositionModeEnable(0);
            UpdatePumpPolAngleMonitor();
        }

        //This method runs when HC closes, and puts the polarisers physically back to zero

 
        public void ShowMessageBox()
        {
            MessageBox.Show("Spinning the Polarizers back to zero");
        }


        public void ReturnPolarizersToZero()
        {
            Thread t = new Thread(new ThreadStart(ShowMessageBox));
            t.Start();
       
            try
            {
                
                probePolPosModeEnabled = true;
                UpdateProbePolMode();
                probePolCont.SetPosition(0);
                UpdatePumpPolMode();
                pumpPolCont.SetPosition(0);

                UpdateProbePolAngleMonitor();
                UpdatePumpPolAngleMonitor();

                while (probePolAngle * probePolAngle >= 1 || pumpPolAngle * pumpPolAngle >= 1) //To make sure that they've made it back to zero (more or less)
                {
                    UpdateProbePolAngleMonitor();
                    UpdatePumpPolAngleMonitor();
                }
                probePolCont.Disconnect();
                pumpPolCont.Disconnect();
            }
            catch (Exception)
            {
                
                throw;
            }
            t.Abort();
        }

        public void SetSteppingBBiasVoltage(double v)
        {
            window.SetTextBox(window.steppingBBoxBiasTextBox, v.ToString());
            bfieldCntrl.SetOut1(v);
        }

        //This method is supposed to automate the process of setting a bias current before EDM data taking. 
        //It assumes that the correct bias current is in the Bias current monitor text box, and parses this to use as its target.

        public void AutomaticBiasCalculation()
        {
            double targetBias = Math.Round(BiasCurrent);

            //double guessBiasVoltage = SteppingBiasVoltage;
            double newBias = 0;
            int a = 0;
            int bSign = BManualState ? 1 : -1;
            double gain = 0.0005;

            SetSteppingBBiasVoltage();

            while (newBias != targetBias & a < 5)
            {
                UpdateBCurrentMonitor();
                newBias = Math.Round(BiasCurrent);
                SteppingBiasVoltage = SteppingBiasVoltage + bSign * gain * (targetBias - newBias);
                SetSteppingBBiasVoltage();
                //SetSteppingBBiasVoltage(guessBiasVoltage);
                a++;
            }

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

        public void UpdateProbeAOMV()
        {
            double pztVoltage = probeAOMVoltage;
            if (window.probeAOMMinusButton.Checked) pztVoltage -= probeAOMStep;
            if (window.probeAOMStepPlusButton.Checked) pztVoltage += probeAOMStep;
            pztVoltage = windowVoltage(pztVoltage, 0, 10);
            SetAnalogOutput(probeAOMAnalogOutputTask, pztVoltage);
            window.probeAOMtrackBar.Value = 100*(int)pztVoltage;
        }

        public void UpdateProbeAOMV(double pztVoltage)
        {
            SetAnalogOutput(probeAOMAnalogOutputTask, pztVoltage);
            window.probeAOMVTextBox.Text = pztVoltage.ToString();
        }

        public void SetprobeAOMVoltage(double v)
        {
            window.SetTextBox(window.probeAOMVTextBox, v.ToString());
            SetAnalogOutput(probeAOMAnalogOutputTask, v);
        }

        
        public void UpdatePumpAOM()
        {
            double pumpAOMVoltage = PumpAOMVoltage;
            if (window.pumpAOMStepMinusButton.Checked) pumpAOMVoltage -= PumpAOMStep;
            if (window.pumpAOMStepPlusButton.Checked) pumpAOMVoltage += PumpAOMStep;
            pumpAOMVoltage = windowVoltage(pumpAOMVoltage, 0, 10);
            SetAnalogOutput(pumpAOMAnalogOutputTask, pumpAOMVoltage);
            window.pumpAOMTrackBar.Value = 100 * (int)pumpAOMVoltage;
        }

        public void UpdatePumpAOM(double pumpAOMVoltage)
        {
            SetAnalogOutput(pumpAOMAnalogOutputTask, pumpAOMVoltage);
            window.pumpAOMVoltageTextBox.Text = pumpAOMVoltage.ToString();
        }

        public void SetPumpAOMVoltage(double v)
        {
            window.SetTextBox(window.pumpAOMVoltageTextBox, v.ToString());
            SetAnalogOutput(pumpAOMAnalogOutputTask, v);
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

        public void SetGreenSynthAmp(double amp)
        {
            GreenSynthOnAmplitude = windowVoltage(amp, -30, 16);
        }
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
            if (rf1FMVoltage > 1.2) rf1FMVoltage = 1.2;
            double rf2FMVoltage = Double.Parse(window.rf2FMVoltage.Text);
            if (window.rf2FMMinusRB.Checked) rf2FMVoltage -= Double.Parse(window.rf2FMIncTextBox.Text);
            if (window.rf2FMPlusRB.Checked) rf2FMVoltage += Double.Parse(window.rf2FMIncTextBox.Text);
            if (rf2FMVoltage > 1.2) rf2FMVoltage = 1.2;
            SetAnalogOutput(rf1FMOutputTask, rf1FMVoltage);
            SetAnalogOutput(rf2FMOutputTask, rf2FMVoltage);
        }

        internal void SetRFHWTrigger0(bool enable)
        {
            SetDigitalLine("rfHWTrigger0", enable);
        }

        internal void SetRFBlanking(bool enable)
        {
            SetDigitalLine("rfAmpBlanking", enable);
        }

        
        public void UpdateVCO161AmpVoltage(double pztVoltage)
        {
            SetAnalogOutput(VCO161AmpAnalogOutputTask, pztVoltage);
            window.VCO161AmpVoltageTextBox.Text = pztVoltage.ToString();
        }

        public void UpdateVCO161FreqVoltage(double pztVoltage)
        {
            SetAnalogOutput(VCO161FreqAnalogOutputTask, pztVoltage);
            window.VCO161FreqTextBox.Text = pztVoltage.ToString();
        }

        public void UpdateVCO30AmpVoltage(double pztVoltage)
        {
            SetAnalogOutput(VCO30AmpAnalogOutputTask, pztVoltage);
            window.VCO30AmpVoltageTextBox.Text = pztVoltage.ToString();
        }

        public void UpdateVCO30FreqVoltage(double pztVoltage)
        {
            SetAnalogOutput(VCO30FreqAnalogOutputTask, pztVoltage);
            window.VCO30FreqVoltageTextBox.Text = pztVoltage.ToString();
        }

        public void UpdateVCO155AmpVoltage(double pztVoltage)
        {
            SetAnalogOutput(VCO155AmpAnalogOutputTask, pztVoltage);
            window.VCO155AmpVoltageTextBox.Text = pztVoltage.ToString();
        }

        public void UpdateVCO155FreqVoltage(double pztVoltage)
        {
            SetAnalogOutput(VCO155FreqAnalogOutputTask, pztVoltage);
            window.VCO155FreqVoltageTextBox.Text = pztVoltage.ToString();
        }

        public void UpdateVCO161AmpV()
        {
            double pztVoltage = VCO161AmpVoltage;
            pztVoltage = windowVoltage(pztVoltage, 0, 10);
            SetAnalogOutput(VCO161AmpAnalogOutputTask, pztVoltage);
            window.VCO161AmpTrackBar.Value = 100 * (int)pztVoltage;
        }

        public void UpdateVCO161FreqV()
        {
            double pztVoltage = VCO161FreqVoltage;
            pztVoltage = windowVoltage(pztVoltage, 0, 10);
            SetAnalogOutput(VCO161FreqAnalogOutputTask, pztVoltage);
            window.VCO161FreqTrackBar.Value = 100 * (int)pztVoltage;
        }

        public void UpdateVCO30AmpV()
        {
            double pztVoltage = VCO30AmpVoltage;
            pztVoltage = windowVoltage(pztVoltage, 0, 10);
            SetAnalogOutput(VCO30AmpAnalogOutputTask, pztVoltage);
            window.VCO30AmpTrackBar.Value = 100 * (int)pztVoltage;
        }

        public void UpdateVCO30FreqV()
        {
            double pztVoltage = VCO30FreqVoltage;
            pztVoltage = windowVoltage(pztVoltage, 0, 10);
            SetAnalogOutput(VCO30FreqAnalogOutputTask, pztVoltage);
            window.VCO30FreqTrackBar.Value = 100 * (int)pztVoltage;
        }

        public void UpdateVCO155AmpV()
        {
            double pztVoltage = VCO155AmpVoltage;
            pztVoltage = windowVoltage(pztVoltage, 0, 10);
            SetAnalogOutput(VCO155AmpAnalogOutputTask, pztVoltage);
            window.VCO155AmpTrackBar.Value = 100 * (int)pztVoltage;
        }

        public void UpdateVCO155FreqV()
        {
            double pztVoltage = VCO155FreqVoltage;
            pztVoltage = windowVoltage(pztVoltage, 0, 10);
            SetAnalogOutput(VCO155FreqAnalogOutputTask, pztVoltage);
            window.VCO155FreqTrackBar.Value = 100 * (int)pztVoltage;
        }

        private int plusVoltage = 0;//For tweaking voltages in hardware controller
        private int minusVoltage = 0;

        public void IncreaseVCOVoltage()
        {
            plusVoltage++;
        }

        public void DecreaseVCOVoltage()
        {
            minusVoltage++;
        }

        public void TweakVCO161AmpV()
        {
            double pztVoltage = VCO161AmpVoltage;
            pztVoltage = windowVoltage(pztVoltage, 0, 10);
            double newPZTVoltage = pztVoltage + VCO161AmpIncrement * (plusVoltage - minusVoltage);
            plusVoltage = 0;
            minusVoltage = 0;
            SetAnalogOutput(VCO161AmpAnalogOutputTask, newPZTVoltage);
            window.VCO161AmpVoltageTextBox.Text = newPZTVoltage.ToString();
            window.VCO161AmpTrackBar.Value = 100 * (int)newPZTVoltage;
        }

        public void TweakVCO161FreqV()
        {
            double pztVoltage = VCO161FreqVoltage;
            pztVoltage = windowVoltage(pztVoltage, 0, 10);
            double newPZTVoltage = pztVoltage + VCO161FreqIncrement * (plusVoltage - minusVoltage);
            plusVoltage = 0;
            minusVoltage = 0;
            SetAnalogOutput(VCO161FreqAnalogOutputTask, newPZTVoltage);
            window.VCO161FreqTextBox.Text = newPZTVoltage.ToString();
            window.VCO161FreqTrackBar.Value = 100 * (int)newPZTVoltage;
        }

        public void TweakVCO30AmpV()
        {
            double pztVoltage = VCO30AmpVoltage;
            pztVoltage = windowVoltage(pztVoltage, 0, 10);
            double newPZTVoltage = pztVoltage + VCO30AmpIncrement * (plusVoltage - minusVoltage);
            plusVoltage = 0;
            minusVoltage = 0;
            SetAnalogOutput(VCO30AmpAnalogOutputTask, newPZTVoltage);
            window.VCO30AmpVoltageTextBox.Text = newPZTVoltage.ToString();
            window.VCO30AmpTrackBar.Value = 100 * (int)newPZTVoltage;
        }

        public void TweakVCO30FreqV()
        {
            double pztVoltage = VCO30FreqVoltage;
            pztVoltage = windowVoltage(pztVoltage, 0, 10);
            double newPZTVoltage = pztVoltage + VCO30FreqIncrement * (plusVoltage - minusVoltage);
            plusVoltage = 0;
            minusVoltage = 0;
            SetAnalogOutput(VCO30FreqAnalogOutputTask, newPZTVoltage);
            window.VCO30FreqVoltageTextBox.Text = newPZTVoltage.ToString();
            window.VCO30FreqTrackBar.Value = 100 * (int)newPZTVoltage;
        }
        public void TweakVCO155AmpV()
        {
            double pztVoltage = VCO155AmpVoltage;
            pztVoltage = windowVoltage(pztVoltage, 0, 10);
            double newPZTVoltage = pztVoltage + VCO155AmpIncrement * (plusVoltage - minusVoltage);
            plusVoltage = 0;
            minusVoltage = 0;
            SetAnalogOutput(VCO155AmpAnalogOutputTask, newPZTVoltage);
            window.VCO155AmpVoltageTextBox.Text = newPZTVoltage.ToString();
            window.VCO155AmpTrackBar.Value = 100 * (int)newPZTVoltage;
        }

        public void TweakVCO155FreqV()
        {
            double pztVoltage = VCO155FreqVoltage;
            pztVoltage = windowVoltage(pztVoltage, 0, 10);
            double newPZTVoltage = pztVoltage + VCO155FreqIncrement * (plusVoltage - minusVoltage);
            plusVoltage = 0;
            minusVoltage = 0;
            SetAnalogOutput(VCO155FreqAnalogOutputTask, newPZTVoltage);
            window.VCO155FreqVoltageTextBox.Text = newPZTVoltage.ToString();
            window.VCO155FreqTrackBar.Value = 100 * (int)newPZTVoltage;
        }

        public void UpdateuWaveDCFMV()
        {
            double voltage = uWaveDCFMVoltage;
            voltage = windowVoltage(voltage, -2.5, 2.5);
            SetAnalogOutput(uWaveDCFMAnalogOutputTask, voltage);
            window.uWaveDCFMTrackBar.Value = 100 * (int)voltage;
        }

        public void UpdateuWaveMixerV()
        {
    
            double pumpVoltage = windowVoltage(pumpMicrowaveMixerVoltage, 0, 5);
            double bottomProbeVoltage = windowVoltage(bottomProbeMicrowaveMixerVoltage, 0, 5);
            double topProbeVoltage = windowVoltage(topProbeMicrowaveMixerVoltage, 0, 5);

            SetAnalogOutput(pumpMicrowaveMixerVoltageAnalogOutputTask, pumpVoltage);
            SetAnalogOutput(bottomProbeMicrowaveMixerVoltageAnalogOutputTask, bottomProbeVoltage);
            SetAnalogOutput(topProbeMicrowaveMixerVoltageAnalogOutputTask, topProbeVoltage);

            window.pumpMixerVoltageTrackBar.Value = 100 * (int)pumpVoltage;
            window.bottomProbeMixerVoltageTrackBar.Value = 100 * (int)bottomProbeVoltage;
            window.topProbeMixerVoltageTrackBar.Value = 100 * (int)topProbeVoltage;

            pumpMicrowaveMixerVoltage = pumpVoltage;
            bottomProbeMicrowaveMixerVoltage = bottomProbeVoltage;
            topProbeMicrowaveMixerVoltage = topProbeVoltage;

        }

        public void UpdateuWaveDCFMVoltage(double pztVoltage)
        {
            SetAnalogOutput(uWaveDCFMAnalogOutputTask, pztVoltage);
            window.uWaveDCFMTextBox.Text = pztVoltage.ToString();
        }

        public void UpdatePumpMicrowaveMixerV(double pztVoltage)
        {
           SetAnalogOutput(pumpMicrowaveMixerVoltageAnalogOutputTask, pztVoltage);
           window.pumpMixerVoltageTextBox.Text = pztVoltage.ToString();
        }

        public void UpdateBottomProbeMicrowaveMixerV(double pztVoltage)
        {
            SetAnalogOutput(bottomProbeMicrowaveMixerVoltageAnalogOutputTask, pztVoltage);
            window.bottomProbeMixerVoltageTextBox.Text = pztVoltage.ToString();
        }

        public void UpdateTopProbeMicrowaveMixerV(double pztVoltage)
        {
            SetAnalogOutput(topProbeMicrowaveMixerVoltageAnalogOutputTask, pztVoltage);
            window.topProbeMixerVoltageTextBox.Text = pztVoltage.ToString();
        }



        public void TweakuWaveDCFMVoltage()
        {
            double pztVoltage = uWaveDCFMVoltage;
            pztVoltage = windowVoltage(pztVoltage, -2.5, 2.5);
            double newPZTVoltage = pztVoltage + uWaveDCFMIncrement * (plusVoltage - minusVoltage);
            plusVoltage = 0;
            minusVoltage = 0;
            SetAnalogOutput(uWaveDCFMAnalogOutputTask, newPZTVoltage);
            window.uWaveDCFMTextBox.Text = newPZTVoltage.ToString();
            window.uWaveDCFMTrackBar.Value = 100 * (int)newPZTVoltage;
        }

        public void EnableAnapico(bool enable)
        {
            UpdateAnapicoRAMList(MwSwitchState);
            anapico.Connect();
            if (enable)
            {
                anapico.CWFrequency = AnapicoCWFrequency;
                anapico.Enabled = true;
            }
            else
            {
                anapico.Enabled = false;
            }
            anapico.Disconnect();
        }

        public void UpdateAnapicoCW()
        {
            anapico.Connect();
            anapico.CWFrequency = AnapicoCWFrequency;
            anapico.Disconnect();
        }

        // When writing a list to RAM, the data has to be transferred according to the IEEE 488.2 Definite Length Block Response Data format.
        // This is #<number of digits that follows this><number of data bytes><data>
        // <data> has to be the form <frequency in Hz>;<power in dBm>;<dwell on time>;<dwell off time>\r\n<next frequency in Hz>...
        public void UpdateAnapicoRAMList(bool trueState)
        {
            try
            {
                bool currentMwListSweepStatus = MwListSweepEnabled;
                MwListSweepEnabled = false;
                anapico.Connect();
                if (trueState)
                {
                    string list = AnapicoFrequency1.ToString() + ";15;" + AnapicoPumpMWDwellOnTime.ToString("E") + ";" + AnapicoPumpMWDwellOffTime.ToString("E") + "\r\n"
                        + AnapicoFrequency0.ToString() + ";15;" + AnapicoBottomProbeMWDwellOnTime.ToString("E") + ";" + AnapicoBottomProbeMWDwellOffTime.ToString("E") + "\r\n"
                        + AnapicoFrequency1.ToString() + ";15;" + AnapicoTopProbeMWDwellOnTime.ToString("E") + ";" + AnapicoTopProbeMWDwellOffTime.ToString("E") + "\r\n";

                    int numBytes = list.Length;

                    int numDigits = numBytes.ToString().Length;

                    string sendList = "#" + numDigits.ToString() + numBytes.ToString() + list;

                    AnapicoBottomProbeMWf0Indicator = true;
                    AnapicoBottomProbeMWf1Indicator = false;
                    AnapicoTopProbeMWf0Indicator = false;
                    AnapicoTopProbeMWf1Indicator = true;

                    anapico.WriteList(sendList);
                }
                else
                {
                    string list = AnapicoFrequency1.ToString() + ";15;" + AnapicoPumpMWDwellOnTime.ToString() + ";" + AnapicoPumpMWDwellOffTime.ToString() + "\r\n"
                        + AnapicoFrequency1.ToString() + ";15;" + AnapicoBottomProbeMWDwellOnTime.ToString() + ";" + AnapicoBottomProbeMWDwellOffTime.ToString() + "\r\n"
                        + AnapicoFrequency0.ToString() + ";15;" + AnapicoTopProbeMWDwellOnTime.ToString() + ";" + AnapicoTopProbeMWDwellOffTime.ToString() + "\r\n";

                    int numBytes = list.Length;

                    int numDigits = numBytes.ToString().Length;

                    string sendList = "#" + numDigits.ToString() + numBytes.ToString() + list;

                    AnapicoBottomProbeMWf0Indicator = false;
                    AnapicoBottomProbeMWf1Indicator = true;
                    AnapicoTopProbeMWf0Indicator = true;
                    AnapicoTopProbeMWf1Indicator = false;

                    anapico.WriteList(sendList);
                }
                anapico.Disconnect();
                MwListSweepEnabled = currentMwListSweepStatus;
            }
            catch
            {
                //If the command fails, try waiting a second and turnning the synth on and off. This also re-loads the list into the memmory
                Thread.Sleep((int)(1000));
                EnableAnapico(false);
                EnableAnapico(true);
            }
        }

        public void EnableAnapicoListSweep(bool enable)
        {
            anapico.Connect();
            if (enable)
            {
                anapico.ListSweepEnabled = true;
            }
            else
            {
                anapico.ListSweepEnabled = false;
            }
            anapico.Disconnect();
        }

        // When reading a list to RAM, the data is transferred according to the IEEE 488.2 Definite Length Block Response Data format.
        // This is #<number of digits that follows this><number of data bytes><data>
        // <data> is in the form <frequency in Hz>;<power in dBm>;<dwell on time>;<dwell off time>\r\n<next frequency in Hz>...
        public void GetAnapicoCurrentList()
        {
            anapico.Connect();

            string list = anapico.ReadList();
            int numDigits = Convert.ToInt32(list[1].ToString());
            string subList = list.Substring(numDigits + 2);

            char[] delimiters = {';','\r','\n'};
            string[] splitList = subList.Split(delimiters);

            string displayList = string.Empty;

            for (int i = 0; i < splitList.Length/4; ++i)
            {
                int j = 4 * i;
                string num = Convert.ToString(i + 1);
                displayList += "Frequency " + num + " (Hz): " + splitList[j] + "\r\n" 
                    + "Dwell on time " + num + " (s): " + splitList[j + 2] + "\r\n" 
                    + "Dwell off time " + num + " (s): " + splitList[j + 3] + "\r\n";
            }

            AnapicoCurrentList = displayList;

            anapico.Disconnect();
        }

        private bool switchingMw;
        public bool SwitchingMw
        {
            get
            {
                return switchingMw;
            }
            set
            {
                    switchingMw = value;
                try
                {
                    SetDigitalLine("mwSwitching", value);
                }
                catch (Exception e)
                {
                    MessageBox.Show("cannot set digital line" + Environment.NewLine + e, "Connect error ...");
                }
            }

        }

        public void SwitchMw()
        {
            SwitchMw(!MwSwitchState);
        }

        public void SwitchMwAndWait(bool state)
        {
            SwitchMw(state);
            switchMwThread.Join();
        }

        public void ReSwitchMwAndWait(bool state)
        {
                //If the command fails, try waiting a second and turnning the synth on and off. This also re-loads the list into the memmory
                Thread.Sleep((int)(1000));
                EnableAnapico(false);
                EnableAnapico(true);
                Thread.Sleep((int)(1000));
                SwitchMwAndWait(state);

        }

        public void SwitchMwAndWait()
        {
            SwitchMwAndWait(!MwSwitchState);
        }


        private bool newMwSwitchState;
        private object switchingMwLock = new object();
        private Thread switchMwThread;
        public void SwitchMw(bool state)
        {
            lock (switchingMwLock)
            {
                newMwSwitchState = state;
                switchMwThread = new Thread(new ThreadStart(SwitchMwWorker));
                window.EnableControl(window.microwaveStateCheckBox, false);
                window.EnableControl(window.listSweepEnabledCheckBox, false);
                window.EnableControl(window.anapicoEnabledCheckBox, false);
                switchMwThread.Start();
            }
        }

        public void SwitchMwWorker()
        {

            lock (switchingMwLock)
            {
                // raise flag for switching microwaves

                SwitchingMw = true;

                MwSwitchState = newMwSwitchState;
                Thread.Sleep((int)(100)); //Here I put in the 100ms delay time (of the Anapico switching) by hand. 
            }

            MwSwitchDone();
        }

        private void MwSwitchDone()
        {
            SwitchingMw = false;
            window.EnableControl(window.microwaveStateCheckBox, true);
            window.EnableControl(window.listSweepEnabledCheckBox, true);
            window.EnableControl(window.anapicoEnabledCheckBox, true);
        }

        #region Pressure monitor

        private double lastPressure;
        private Queue<double> pressureSamples = new Queue<double>();
        public void UpdatePressureMonitor()
        {
            //sample the pressure
            lastPressure = pressureMonitor.Pressure;

            //add samples to Queues for averaging
            pressureSamples.Enqueue(lastPressure);

            //drop samples when array is larger than the moving average sample length
            while (pressureSamples.Count > pressureMovingAverageSampleLength)
            {
                pressureSamples.Dequeue();
            }

            //average samples
            double avgPressure = pressureSamples.Average();
            string avgPressureExpForm = avgPressure.ToString("E");

            //update text boxes
            window.SetTextBox(window.pressureMonitorTextBox, (avgPressureExpForm).ToString());
        }

        public void PlotLastPressure()
        {
            //sample the pressure
            lastPressure = pressureMonitor.Pressure;

            //plot the most recent samples
            window.PlotYAppend(window.pressureGraph, window.pressurePlot,
                        new double[] { lastPressure });
        }

        public void ClearPressureMonitorAv()
        {
            pressureSamples.Clear();
        }


        private JSONSerializer pressureDataSerializer;
        public void StartLoggingPressure()
        {
            pressureDataSerializer = new JSONSerializer();
            string initialDataDir = Environs.FileSystem.GetDataDirectory(
                                                (String)Environs.FileSystem.Paths["scanMasterDataPath"]);
            pressureDataSerializer.StartLogFile(initialDataDir +
                Environs.FileSystem.GenerateNextDataFileName() + ".json");
            pressureDataSerializer.StartProcessingData();
        }
        public void StopLoggingPressure()
        {
            pressureDataSerializer.EndLogFile();
        }

        private Thread pressureMonitorPollThread;
        private int pressureMonitorPollPeriod = 100;
        private Object pressureMonitorLock;
        private bool pressureMonitorFlag;
        private int pressureMovingAverageSampleLength = 10;
        private int pressureMonitorLogPeriod = 1000;
        private int pressureLoggingRate;
        internal void StartPressureMonitorPoll()
        {
            pressureMonitorPollThread = new Thread(new ThreadStart(pressureMonitorPollWorker));
            window.EnableControl(window.startPressureMonitorPollButton, false);
            window.EnableControl(window.stopPressureMonitorPollButton, true);
            pressureMonitorPollPeriod = Int32.Parse(window.pressureMonitorPollPeriodTextBox.Text);
            pressureMovingAverageSampleLength = Int32.Parse(window.pressureMonitorSampleLengthTextBox.Text);
            pressureMonitorLogPeriod = Int32.Parse(window.pressureMonitorLogPeriodTextBox.Text) * 1000; // Convert from seconds to milliseconds
            pressureLoggingRate = pressureMonitorLogPeriod / pressureMonitorPollPeriod;
            pressureSamples.Clear();
            pressureMonitorLock = new Object();
            pressureMonitorFlag = false;
            pressureMonitorPollThread.Start();
        }

        internal void StopPressureMonitorPoll()
        {
            pressureMonitorFlag = true;
        }
        private void pressureMonitorPollWorker()
        {
            int count = 0;

            for (; ; )
            {
                Thread.Sleep(pressureMonitorPollPeriod);
                ++count;
                lock (pressureMonitorLock)
                {
                    UpdatePressureMonitor();
                    if (count == pressureLoggingRate)
                    {
                        PlotLastPressure();

                        if (window.logPressureDataCheckBox.Checked)
                        {
                            pressureDataSerializer.AddData(new PressureMonitorDataLog(DateTime.Now,
                                pressureMonitorPollPeriod,
                                lastPressure));
                        }

                        count = 0;
                    }
                    if (pressureMonitorFlag)
                    {
                        pressureMonitorFlag = false;
                        break;
                    }
                }
            }
            window.EnableControl(window.startPressureMonitorPollButton, true);
            window.EnableControl(window.stopPressureMonitorPollButton, false);
        }

        #endregion

        #region Rf AWG Continuous Generation Controls

        public double RfAWGOnFrequency
        {
            get
            {
                return Double.Parse(window.rfAWGOnFreqTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rfAWGOnFreqTextBox, value.ToString());
            }
        }

        public double RfAWGOnAmplitude
        {
            get
            {
                return Double.Parse(window.rfAWGOnAmpTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rfAWGOnAmpTextBox, value.ToString());
            }
        }

        public bool RfAWGContinuousGenerationEnabled
        {
            get
            {
                return window.rfAWGContinuousGenerationCheckBox.Checked;
            }
            set
            {
                window.SetCheckBox(window.rfAWGContinuousGenerationCheckBox, value);
            }
        }

        public void EnableRfAWGContinuousGeneration(bool enable)
        {
            if (enable)
            {
                try
                {
                    rfAWG.Connect();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error while connecting: " + e.Message);
                }

                try
                {
                    rfAWG.Frequency = RfAWGOnFrequency;
                    rfAWG.Amplitude = RfAWGOnAmplitude;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error while setting parameters: " + e.Message);
                }

                try
                {
                    rfAWG.StartGeneration();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error while starting generation: " + e.Message);
                }
            }

            else
            {
                try
                {
                    rfAWG.Disconnect();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Stop error: " + e.Message);
                }
            }

        }

        #endregion

        #region Rf AWG Pulsed Generation Controls

        public double RfAWGRf1Frequency
        {
            get
            {
                return Double.Parse(window.rfAWGRf1FreqTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rfAWGRf1FreqTextBox, value.ToString());
            }
        }

        public double RfAWGRf2Frequency
        {
            get
            {
                return Double.Parse(window.rfAWGRf2FreqTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rfAWGRf2FreqTextBox, value.ToString());
            }
        }

        public double RfAWGRf1Amplitude
        {
            get
            {
                return Double.Parse(window.rfAWGRf1AmpTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rfAWGRf1AmpTextBox, value.ToString());
            }
        }

        public double RfAWGRf2Amplitude
        {
            get
            {
                return Double.Parse(window.rfAWGRf2AmpTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rfAWGRf2AmpTextBox, value.ToString());
            }
        }

        public int RfAWGRf1PulseLength
        {
            get
            {
                return Int32.Parse(window.rfAWGRf1PulseLengthTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rfAWGRf1PulseLengthTextBox, value.ToString());
            }
        }

        public int RfAWGRf2PulseLength
        {
            get
            {
                return Int32.Parse(window.rfAWGRf2PulseLengthTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rfAWGRf2PulseLengthTextBox, value.ToString());
            }
        }

        public int RfAWGRf1CentreTime
        {
            get
            {
                return Int32.Parse(window.rfAWGRf1CentreTimeTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rfAWGRf1CentreTimeTextBox, value.ToString());
            }
        }

        public int RfAWGRf2CentreTime
        {
            get
            {
                return Int32.Parse(window.rfAWGRf2CentreTimeTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rfAWGRf2CentreTimeTextBox, value.ToString());
            }
        }

        public double RfAWGCarrierFrequency
        {
            get
            {
                return Double.Parse(window.rfAWGCarrierFreqTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rfAWGCarrierFreqTextBox, value.ToString());
            }
        }

        public int RfAWGWaveformLength
        {
            get
            {
                return Int32.Parse(window.rfAWGWaveformLengthTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rfAWGWaveformLengthTextBox, value.ToString());
            }
        }

        public double RfAWGPhaseOffset
        {
            get
            {
                return Double.Parse(window.rfAWGPhaseOffsetTextBox.Text);
            }
            set
            {
                window.SetTextBox(window.rfAWGPhaseOffsetTextBox, value.ToString());
            }
        }

        public bool RfAWGPulsedGenerationEnabled
        {
            get
            {
                return window.rfAWGPulsedGenerationCheckBox.Checked;
            }
            set
            {
                window.SetCheckBox(window.rfAWGPulsedGenerationCheckBox, value);
            }
        }

        public void ConnectRfAWG()
        {
            try
            {
                rfAWG.Connect();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while connecting: " + e.Message);
            }
        }

        public void EnableRfAWGPulsedGeneration()
        {
            IQData iqData = GenerateIQData();

            try
            {
                rfAWG.Frequency = RfAWGCarrierFrequency;
                rfAWG.Amplitude = RfAWGRf1Amplitude > RfAWGRf2Amplitude ? RfAWGRf1Amplitude : RfAWGRf2Amplitude;
                rfAWG.IData = iqData.IData;
                rfAWG.QData = iqData.QData;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while setting parameters: " + e.Message);
            }

            try
            {
                rfAWG.StartPulsedGeneration();
                window.rfsgStatusTimer.Enabled = true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while starting generation: " + e.Message);
            }
        }

        public void UpdateRfAWGPulsedGeneration()
        {
            rfAWG.StopGeneration();
            window.rfsgStatusTimer.Enabled = false;
            EnableRfAWGPulsedGeneration();
        }

        public void DisableRfAWGPulsedGeneration()
        {
            try
            {
                window.rfsgStatusTimer.Enabled = false;
                rfAWG.Disconnect();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while disconnecting from rfsg: " + e.Message);
            }
        }

        private IQData GenerateIQData()
        {
            IQData iqData;
            object locker = new object();
            lock (locker)
            {
                double rf1FreqDiff = 1e6 * (RfAWGRf1Frequency - RfAWGCarrierFrequency);
                double rf2FreqDiff = 1e6 * (RfAWGRf2Frequency - RfAWGCarrierFrequency);
                double phaseOffset = RfAWGPhaseOffset;
                bool rf1MorePower = RfAWGRf1Amplitude > RfAWGRf2Amplitude;
                double scaledAmp = ScaledAmp(RfAWGRf1Amplitude, RfAWGRf2Amplitude);
                double rf1Amp = rf1MorePower ? 1 : scaledAmp;
                double rf2Amp = rf1MorePower ? scaledAmp : 1;

                // Always use IQ rate of 100MS/s, so a single time unit is 10ns
                // Input units are in us
                const int MICROSECOND_TO_TIME_UNIT = 100;
                int length = RfAWGWaveformLength * MICROSECOND_TO_TIME_UNIT;
                int rf1StartTime = (RfAWGRf1CentreTime - (RfAWGRf1PulseLength / 2)) * MICROSECOND_TO_TIME_UNIT;
                int rf1EndTime = rf1StartTime + (RfAWGRf1PulseLength * MICROSECOND_TO_TIME_UNIT);
                int rf2StartTime = (RfAWGRf2CentreTime - (RfAWGRf2PulseLength / 2)) * MICROSECOND_TO_TIME_UNIT;
                int rf2EndTime = rf2StartTime + (RfAWGRf2PulseLength * MICROSECOND_TO_TIME_UNIT);

                iqData = new IQData(length);
                double iDataPoint = 0.0;
                double qDataPoint = 0.0;

                for (int i = rf1StartTime; i < rf1EndTime; i++)
                {
                    iDataPoint = rf1Amp * Math.Cos(2 * Math.PI * rf1FreqDiff * i / 1e8);
                    qDataPoint = rf1Amp * Math.Sin(2 * Math.PI * rf1FreqDiff * i / 1e8);
                    iqData.WriteIQData(i, iDataPoint, qDataPoint);
                }

                for (int i = rf2StartTime; i < rf2EndTime; i++)
                {
                    iDataPoint = rf2Amp * Math.Cos(2 * Math.PI * rf2FreqDiff * i / 1e8 + phaseOffset);
                    qDataPoint = rf2Amp * Math.Sin(2 * Math.PI * rf2FreqDiff * i / 1e8 + phaseOffset);
                    iqData.WriteIQData(i, iDataPoint, qDataPoint);
                }
            }
            
            return iqData;

        }
        private double ScaledAmp(double rf1Amp, double rf2Amp)
        {
            double ampDiff = rf1Amp > rf2Amp ? rf2Amp - rf1Amp : rf1Amp - rf2Amp;
            return Math.Pow(10, ampDiff / 20);
        }

        public void CheckRfAWGPulsedGeneration()
        {
            rfAWG.CheckGeneration();
            if (rfAWG.GenerationComplete)
            {
                var sw = new Stopwatch();
                sw.Start();
                rfAWG.StopGeneration();
                window.rfsgStatusTimer.Enabled = false;
                EnableRfAWGPulsedGeneration();
                sw.Stop();
                window.SetTextBox(window.rfAWGTestTextBox, sw.ElapsedMilliseconds.ToString());
            }
        }

        #endregion

        #endregion

    }
}
