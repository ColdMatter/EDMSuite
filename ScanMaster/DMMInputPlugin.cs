using System;
using System.Collections;
using System.Xml.Serialization;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.HAL;

using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
    /// A plugin to capture GPIB data using an E-series board.
    /// </summary>
    [Serializable]
    public class DMMInputPlugin : GPIBInputPlugin
    {
        [NonSerialized]
        private HP34401A dmm;

        [NonSerialized]
        private double latestData;
        
        protected override void InitialiseSettings()
        {
            settings["meter"] = "bCurrentMeter";
        }

        public override void AcquisitionStarting()
        {
            dmm = (HP34401A)Environs.Hardware.Instruments[(string)settings["meter"]];
            dmm.Connect();
        }

        public override void ScanStarting()
        {
        }

        public override void ScanFinished()
        {
        }

        public override void AcquisitionFinished()
        {
            dmm.Disconnect();
        }

        public override void ArmAndWait()
        {
            lock (this)
            {
                if (!Environs.Debug)
                {
                    
                    latestData= dmm.ReadCurrent();
                    
                }
            }
        }

        [XmlIgnore]
        public override double GPIBval
        {
            get
            {
                return latestData;
            }
        }
    }
}
