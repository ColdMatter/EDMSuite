using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DecelerationHardwareControl
{
    public class Controller : MarshalByRefObject
    {

        ControlWindow window;
        private bool analogsAvailable;
        private double lastCavityData;
        private DateTime cavityTimestamp;
        
        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        public void Start()
        {
            window = new ControlWindow();
            window.controller = this;
            Application.Run(window);
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