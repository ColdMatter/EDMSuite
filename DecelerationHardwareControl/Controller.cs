using System;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using DAQ.Environment;

namespace DecelerationHardwareControl
{
    public class Controller : MarshalByRefObject
    {

        ControlWindow window;
        private bool analogsAvailable;
        private double lastCavityData;
        private DateTime cavityTimestamp;
        private double laserFrequencyControlVoltage;

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
            window = new ControlWindow();
            window.controller = this;
            Application.Run(window);
        }

        // Applications may set this control voltage themselves, but when they do
        // they should this property too
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

        public double LastCavityData
        {
            get { return lastCavityData; }
        }

        public double TimeSinceLastCavityRead
        {
            get
            {
                TimeSpan delta = DateTime.Now - cavityTimestamp;
                return (delta.Milliseconds + 1000 * delta.Seconds + 60 * 1000 * delta.Minutes);
            }
        }
    }
}