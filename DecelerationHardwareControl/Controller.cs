using System;
using System.Globalization;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using DAQ.Environment;
using DAQ.HAL;
using DAQ.TransferCavityLock;
using NationalInstruments.DAQmx;
using NationalInstruments;
using NationalInstruments.DAQmx;

namespace DecelerationHardwareControl
{
    public class Controller : MarshalByRefObject, TransferCavityLockable
    {
        #region Constants
        private const double synthOffAmplitude = -130.0;
        #endregion

        ControlWindow window;

        HP8673BSynth synth = (HP8673BSynth)Environs.Hardware.Instruments["synth"];
        FlowMeter flowMeter = (FlowMeter)Environs.Hardware.Instruments["flowmeter"];

        private TransferCavityLockable TCLHelper = new DAQMxTCLHelperSWTimed
            ("cavity", "analogTrigger3", "laser", "p2", "p1", "analogTrigger2", "cavityTriggerOut");

        private bool analogsAvailable;
        private double lastCavityData;
        private double lastrefCavityData;
        private DateTime cavityTimestamp;
        private DateTime refcavityTimestamp;
        private double laserFrequencyControlVoltage;

        private double aomControlVoltage;
        private Task outputTask = new Task("AomControllerOutput");
        private AnalogOutputChannel aomChannel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["highvoltage"];
        public AnalogSingleChannelWriter aomWriter;


        private AnalogInputChannel pressureSourceChamber = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["PressureSourceChamber"];
        private Task pressureMonitorTask = new Task();
        private AnalogInputChannel pressureRough = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["RoughVacuum"];
        private Task roughVacuumTask = new Task();
        private AnalogInputChannel voltageReference = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["VoltageReference"];
        private Task voltageReferenceTask = new Task();
        private AnalogInputChannel therm30KTemp = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["10KThermistor30KPlate"];
        private Task thermistor30KPlateTask = new Task();
        private AnalogInputChannel shieldTemp = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["30KShield"];
        private Task shieldTask = new Task();
        private AnalogInputChannel cellTemp = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["4KCell"];
        private Task cellTask = new Task();

        public string command;
	


        public double normsigGain;

        public double SynthOnFrequency
        {
            get
            {
                return Double.Parse(window.synthOnFreqBox.Text);
            }
            set
            {
                window.SetTextBox(window.synthOnFreqBox, value.ToString());
            }
        }

        public double SynthOnAmplitude
        {
            get
            {
                return Double.Parse(window.synthOnAmpBox.Text);
            }
            set
            {
                window.SetTextBox(window.synthOnAmpBox, value.ToString());
            }
        }

        // The keys of the hashtable are the names of the analog output channels
        // The values are all booleans - true means the channel is blocked
        private Hashtable analogOutputsBlocked;

        
        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        public void Start()
        {
            analogsAvailable = true;
            // all the analog outputs are unblocked at the outset
            analogOutputsBlocked = new Hashtable();
            foreach (DictionaryEntry de in Environs.Hardware.AnalogOutputChannels) 
                analogOutputsBlocked.Add(de.Key, false);

            aomChannel.AddToTask(outputTask, -10, 10);
            outputTask.Control(TaskAction.Verify);
            aomWriter = new AnalogSingleChannelWriter(outputTask.Stream);

            pressureSourceChamber.AddToTask(pressureMonitorTask, -10, 10);
            pressureRough.AddToTask(roughVacuumTask, -10, 10);
            voltageReference.AddToTask(voltageReferenceTask, -10, 10);
            therm30KTemp.AddToTask(thermistor30KPlateTask, -10, 10);
            shieldTemp.AddToTask(shieldTask, -10, 10);
            cellTemp.AddToTask(cellTask, -10, 10);

            window = new ControlWindow();
            window.controller = this;
            Application.Run(window);
        }

        // Applications may set this control voltage themselves, but when they do
        // they should set this property too
        public double LaserFrequencyControlVoltage
        {
            get { return laserFrequencyControlVoltage; }
            set { laserFrequencyControlVoltage = value; }
        }

        public double AOMControlVoltage
        {
            get { return aomControlVoltage; }
            set { aomControlVoltage = value; }
        }

        // returns true if the channel is blocked
        public bool GetAnalogOutputBlockedStatus(string channel)
        {
            return (bool)analogOutputsBlocked[channel];
        }

        // set to true to block the output channel
        public void SetAnalogOutputBlockedStatus(string channel, bool state)
        {
            analogOutputsBlocked[channel] = state;
        }
            

        public bool LaserLocked
        {
            get
            {
                return window.LaserLockCheckBox.Checked;
            }
            set
            {
                window.SetCheckBox(window.LaserLockCheckBox, value);
            }
        }

        public bool AnalogInputsAvailable
        {
            get { return analogsAvailable; }
            set { analogsAvailable = value; }
        }

        public void UpdateLockCavityData(double cavityValue)
        {
            lastCavityData = cavityValue;
            cavityTimestamp = DateTime.Now;
        }

        public void UpdateReferenceCavityData(double refcavityValue)
        {
            lastrefCavityData = refcavityValue;
            refcavityTimestamp = DateTime.Now;
        }

        public double LastCavityData
        {
            get { return lastCavityData; }
        }

        public DateTime LastCavityTimeStamp
        {
            get { return cavityTimestamp; }
        }

        public double TimeSinceLastCavityRead
        {
            get
            {
                TimeSpan delta = DateTime.Now - cavityTimestamp;
                return (delta.Milliseconds + 1000 * delta.Seconds + 60 * 1000 * delta.Minutes);
            }
        }

        public double AOMVoltage
        {
            get
            {
                return AOMControlVoltage;
            }
            set
            {
                if (!Environs.Debug)
                {
                    aomWriter.WriteSingleSample(true, value);
                    outputTask.Control(TaskAction.Unreserve);
                }
                else
                {
                    // Debug mode, do nothing
                }
                aomControlVoltage = value;
                
            }
        }

        public void diodeSaturationError()
        {
            window.SetDiodeWarning(window.diodeSaturation, true);
        }
        public void diodeSaturation()
        {
            window.SetDiodeWarning(window.diodeSaturation, false);
        }


        #region TransferCavityLockable Members

        public void ConfigureCavityScan(int numberOfSteps, bool autostart)
        {
            TCLHelper.ConfigureCavityScan(numberOfSteps, autostart);
        }

        public void ConfigureReadPhotodiodes(int numberOfMeasurements, bool autostart)
        {
            TCLHelper.ConfigureReadPhotodiodes(numberOfMeasurements, autostart);
        }

        public void ConfigureSetLaserVoltage(double voltage)
        {
            TCLHelper.ConfigureSetLaserVoltage(voltage);
        }

        public void ConfigureScanTrigger()
        {
            TCLHelper.ConfigureScanTrigger();
        }

        public void ScanCavity(double[] rampVoltages, bool autostart)
        {
            TCLHelper.ScanCavity(rampVoltages, autostart);
        }

        public void StartScan()
        {
            TCLHelper.StartScan();
        }

        public void StopScan()
        {
            TCLHelper.StopScan();
        }

        public double[,] ReadPhotodiodes(int numberOfMeasurements)
        {
            return TCLHelper.ReadPhotodiodes(numberOfMeasurements);
        }

        public void SetLaserVoltage(double voltage)
        {
            TCLHelper.SetLaserVoltage(voltage);
        }

        public void ReleaseCavityHardware()
        {
            TCLHelper.ReleaseCavityHardware();
        }

        public void SendScanTriggerAndWaitUntilDone()
        {
            TCLHelper.SendScanTriggerAndWaitUntilDone();
        }
        public void ReleaseLaser()
        {
            TCLHelper.ReleaseLaser();
        }

        public void EnableSynth(bool enable)
        {
            synth.Connect();
            if (enable)
            {
                synth.Frequency = SynthOnFrequency;
                synth.Amplitude = SynthOnAmplitude;
                synth.Enabled = true;
            }
            else
            {
                synth.Enabled = false;
            }
            synth.Disconnect();
        }

        public void UpdateSynthSettings()
        {
            synth.Connect();
            synth.Frequency = SynthOnFrequency;
            synth.Amplitude = SynthOnAmplitude;
            synth.Disconnect();
        }

        private double VoltageResistanceConversion(double voltage, double Vref)
        {
            return 47120 * (voltage / (Vref - voltage));
        }

        private double VoltageRoughVacuumConversion(double voltage)
        {
            return Math.Pow(10, ((voltage - 6.143)/1.286));
        }

        private double VoltagePressureConversion(double voltage)
        {
            return Math.Pow(10, (1.667 * voltage - 11.33));
        }

        public void UpdateMonitoring()
        {
            AnalogSingleChannelReader reader3 = new AnalogSingleChannelReader(voltageReferenceTask.Stream);
            double Vref = reader3.ReadSingleSample();
            
            AnalogSingleChannelReader reader1 = new AnalogSingleChannelReader(pressureMonitorTask.Stream);
            double analogDataIn1 = reader1.ReadSingleSample();
            window.monitorPressureSourceChamber.Text = VoltagePressureConversion(analogDataIn1).ToString("E02",CultureInfo.InvariantCulture);

            AnalogSingleChannelReader reader2 = new AnalogSingleChannelReader(roughVacuumTask.Stream);
            double analogDataIn2 = reader2.ReadSingleSample();
            window.monitorRoughVacuum.Text = VoltageRoughVacuumConversion(analogDataIn2).ToString("E02", CultureInfo.InvariantCulture);


            AnalogSingleChannelReader reader4 = new AnalogSingleChannelReader(thermistor30KPlateTask.Stream);
            double analogDataIn4 = reader4.ReadSingleSample();
            window.monitor10KTherm30KPlate.Text = VoltageResistanceConversion(analogDataIn4, Vref).ToString("E04", CultureInfo.InvariantCulture);

            AnalogSingleChannelReader reader5 = new AnalogSingleChannelReader(shieldTask.Stream);
            double analogDataIn5 = reader5.ReadSingleSample();
            window.monitorShield.Text = VoltageResistanceConversion(analogDataIn5, Vref).ToString("E04", CultureInfo.InvariantCulture);

            AnalogSingleChannelReader reader6 = new AnalogSingleChannelReader(cellTask.Stream);
            double analogDataIn6 = reader6.ReadSingleSample();
            window.monitorColdPlate.Text = VoltageResistanceConversion(analogDataIn6, Vref).ToString("E04", CultureInfo.InvariantCulture);
        }


        public string GetCommand()
        {
            return window.CommandBox.Text;
        }

        //public void ReadFlowMeter()
        //{
            //string 
            //window.SetTextBox(window.CommandBox, value.ToString());

            //return window.CommandBox.Text;        
        //}



        public void SetFlowMeter()
        {
            flowMeter.SetFlow(GetCommand());
        }

        #endregion
    }
}