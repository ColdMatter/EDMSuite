using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using DAQ.Environment;
using DAQ.HAL;
using DAQ.TransferCavityLock;
using NationalInstruments.DAQmx;
using NationalInstruments;

namespace MoleculeMOTHardwareControl
{
    public class Controller : MarshalByRefObject, TransferCavityLockable
    {
        #region Constants
        private const double synthOffAmplitude = -130.0;
        #endregion

        ControlWindow window;

        // Instruments
        HP8673BSynth synth = (HP8673BSynth)Environs.Hardware.Instruments["synth"];
        //FlowMeter flowMeter = (FlowMeter)Environs.Hardware.Instruments["flowmeter"];
        WindfreakSynthesizer windfreak = (WindfreakSynthesizer)Environs.Hardware.Instruments["windfreak"];

        private TransferCavityLockable TCLHelper = new DAQMxTCLHelperSWTimed(
            "cavity", "analogTrigger3", "laser", "p2", "p1", "analogTrigger2", "cavityTriggerOut"
        );

        private static Hashtable calibrations = Environs.Hardware.Calibrations;

        public bool sidebandMonitorRunning = false;
        private string[] sidebandChannelList = {"cavityVoltage","mot606", "mot628V1","slowing531","slowing628V1"};
        private Task sidebandMonitorTask = new Task("sidebandMonitor");
        private AnalogMultiChannelReader sidebandReader;
        private int sidebandMonitorSampleRate = 2500;
        private int sidebandMonitorSamplesPerChannel = 4000;
        private int waitBetweenReads = 1000;

        private bool analogsAvailable;
        private double lastCavityData;
        private double lastrefCavityData;
        private DateTime cavityTimestamp;
        private DateTime refcavityTimestamp;
        private double laserFrequencyControlVoltage;

        private Dictionary<string, Task> analogTasks = new Dictionary<string, Task>();

        private Task motAOMFreqOutputTask = new Task("MOTAOMFrequencyOutput");
        private AnalogOutputChannel motAOMFreqChannel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["v00Frequency"];
        private Task motAOMAmpOutputTask = new Task("MOTAOMAmplitudeOutput");
        private AnalogOutputChannel motAOMAmpChannel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["v00Intensity"];

        public AnalogSingleChannelWriter analogWriter;


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

        public void AddAnalogOutput(Task task, AnalogOutputChannel channel, string channelName, double lowLimit, double highLimit)
        {
            channel.AddToTask(task, lowLimit, highLimit);
            task.Control(TaskAction.Verify);
            analogTasks.Add(channelName, task);
        }

        public void Start()
        {
            analogsAvailable = true;
            // all the analog outputs are unblocked at the outset
            analogOutputsBlocked = new Hashtable();
            foreach (DictionaryEntry de in Environs.Hardware.AnalogOutputChannels) 
                analogOutputsBlocked.Add(de.Key, false);

            AddAnalogOutput(motAOMFreqOutputTask, motAOMFreqChannel, "motAOMFreq", -10, 10);
            AddAnalogOutput(motAOMAmpOutputTask, motAOMAmpChannel, "motAOMAmp", -10, 10);
            

            //pressureSourceChamber.AddToTask(pressureMonitorTask, -10, 10);
            //pressureRough.AddToTask(roughVacuumTask, -10, 10);
            //voltageReference.AddToTask(voltageReferenceTask, -10, 10);
            //therm30KTemp.AddToTask(thermistor30KPlateTask, -10, 10);
            //shieldTemp.AddToTask(shieldTask, -10, 10);
            //cellTemp.AddToTask(cellTask, -10, 10);
            //InitializeSidebandRead();

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

        public void SetMOTAOMFreq(double frequency)
        {
            SetAnalogOutput("motAOMFreq", frequency, true);
        }

        public void SetMOTAOMAmp(double amplitude)
        {
            SetAnalogOutput("motAOMAmp", amplitude, true);
        }

        public class CalibrationException : ArgumentOutOfRangeException { };
        public void SetAnalogOutput(string channel, double value, bool useCalibration)
        {

            analogWriter = new AnalogSingleChannelWriter(analogTasks[channel].Stream);
            bool changeIt = true;
            double output = 0.0;
            if (useCalibration)
            {
                try
                {
                    output = ((Calibration)calibrations[channel]).Convert(value);
                }
                catch (DAQ.HAL.Calibration.CalibrationRangeException)
                {
                    MessageBox.Show("The number is outside the calibrated range. The value will not be updated.");
                    changeIt = false;
                }
                catch
                {
                    MessageBox.Show("Calibration error");
                    changeIt = false;
                }
            }
            else
            {
                output = value;
            }
            if (changeIt)
            {
                try
                {
                    analogWriter.WriteSingleSample(true, output);
                    analogTasks[channel].Control(TaskAction.Unreserve);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        #region Windfreak Members

        public void UpdateWindfreak(double freq, double amp)
        {
            windfreak.UpdateContinuousSettings(freq, amp);
        }

        public void SetWindfreakOutput(bool outputIsOn)
        {
            windfreak.SetOutput(outputIsOn);
        }

        #endregion

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

     

        public void InitializeSidebandRead()
        {
            foreach (string channel in sidebandChannelList)
                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                    sidebandMonitorTask,
                    0, 10);

            // internal clock, finite acquisition
            sidebandMonitorTask.Timing.ConfigureSampleClock(
                "",
                sidebandMonitorSampleRate,
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.FiniteSamples,
                sidebandMonitorSamplesPerChannel);

            sidebandMonitorTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo("usbAnalogTrigger"),
                DigitalEdgeStartTriggerEdge.Rising);

            sidebandMonitorTask.Control(TaskAction.Verify);
            
            sidebandReader = new AnalogMultiChannelReader(sidebandMonitorTask.Stream);
        }

        public void StartSidebandRead()
        {
            Thread readSidebandsThread = new Thread(new ThreadStart(ReadSidebands));
            readSidebandsThread.Start();
        }

      
        private double[,] sidebandData;
        public void ReadSidebands()
        {
            sidebandMonitorRunning = true;
            while (sidebandMonitorRunning)
            {

                sidebandMonitorTask.Start();
                sidebandData = sidebandReader.ReadMultiSample(sidebandMonitorSamplesPerChannel);
                sidebandMonitorTask.Stop();

                double[] xvals = new double[sidebandMonitorSamplesPerChannel];
                double[] yvals606 = new double[sidebandMonitorSamplesPerChannel];
                double[] yvals628V1 = new double[sidebandMonitorSamplesPerChannel];
                double[] yvals531 = new double[sidebandMonitorSamplesPerChannel];
                double[] yvals628Slowing = new double[sidebandMonitorSamplesPerChannel];
                for (int j = 0; j < sidebandMonitorSamplesPerChannel; j++)
                    xvals[j] = sidebandData[0, j];
                for (int j = 0; j < sidebandMonitorSamplesPerChannel; j++)
                    yvals606[j] = sidebandData[1, j];
                for (int j = 0; j < sidebandMonitorSamplesPerChannel; j++)
                    yvals628V1[j] = sidebandData[2, j];
                for (int j = 0; j < sidebandMonitorSamplesPerChannel; j++)
                    yvals531[j] = sidebandData[3, j];
                for (int j = 0; j < sidebandMonitorSamplesPerChannel; j++)
                    yvals628Slowing[j] = sidebandData[4, j];

                window.displaySidebandData(window.scatterGraph1, xvals, yvals606);
                window.displaySidebandData628V1(window.scatterGraph2, xvals, yvals628V1);
                window.displaySidebandData531(window.scatterGraph6, xvals, yvals531);
                window.displaySidebandData628Slowing(window.scatterGraph5, xvals, yvals628Slowing);

                Thread.Sleep(waitBetweenReads);
            }
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
      //      flowMeter.SetFlow(GetCommand());
        }

        #endregion
    }
}