using System;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using DAQ.Environment;
using DAQ.HAL;
using DAQ.TransferCavityLock;
using NationalInstruments.DAQmx;

namespace DecelerationHardwareControl
{
    public class Controller : MarshalByRefObject, TransferCavityLockable
    {

        ControlWindow window;

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
        #endregion

    }
}