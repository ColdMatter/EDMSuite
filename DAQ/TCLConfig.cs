using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace DAQ.TransferCavityLock2012
{
    public class TCLConfig
    {
          
        public TCLConfig(string name)
        {
            configurationName = name;
            slaveVoltageUpperLimit = 5.0;
            slaveVoltageLowerLimit = 0.0;
            defaultLaserVoltage = 0.0;
            maxInputVoltage = 10.0;
            defaultScanPoints = 1000;
            analogSampleRate = 50000;
            maximumNLMFSteps = 100;
            pointsToConsiderEitherSideOfPeakInFWHMs = 4;
            triggerOnRisingEdge = true;
        }

        private string configurationName;
        public string Name
        {
            get { return configurationName; }
            set { configurationName = value; }
        }

        private int tcpChannel;
        public int TCPChannel
        {
            get { return tcpChannel; }
            set { tcpChannel = value; }
        }

        private string baseRamp;
        public string BaseRamp // This is what used to be called Cavity in previous version
        {
            get { return baseRamp; }
            set { baseRamp = value; }
        }

        private double slaveVoltageUpperLimit;
        public double SlaveVoltageUpperLimit
        {
            get { return slaveVoltageUpperLimit; }
            set { slaveVoltageUpperLimit = value; }
        }
    
        private double slaveVoltageLowerLimit;
        public double SlaveVoltageLowerLimit
        {
            get { return slaveVoltageLowerLimit; }
            set { slaveVoltageLowerLimit = value; }
        }

        private double defaultLaserVoltage;
        public double DefaultLaserVoltage
        {
            get { return defaultLaserVoltage; }
            set { defaultLaserVoltage = value; }
        }

        private double maxInputVoltage;
        public double MaxInputVoltage
        {
            get { return maxInputVoltage; }
            set { maxInputVoltage = value; }
        }

        private int defaultScanPoints;
        public int DefaultScanPoints
        {
            get { return defaultScanPoints; }
            set { defaultScanPoints = value; }
        }

        private double analogSampleRate;
        public double AnalogSampleRate
        {
            get { return analogSampleRate; }
            set { analogSampleRate = value; }
        }

        private int maximumNLMFSteps; 
        public int MaximumNLMFSteps
        {
            get { return maximumNLMFSteps; }
            set { maximumNLMFSteps = value; }
        }

        private double pointsToConsiderEitherSideOfPeakInFWHMs;
        public double PointsToConsiderEitherSideOfPeakInFWHMs
        {
            get { return pointsToConsiderEitherSideOfPeakInFWHMs; }
            set { pointsToConsiderEitherSideOfPeakInFWHMs = value; }
        }

        private bool triggerOnRisingEdge;
        public bool TriggerOnRisingEdge
        {
            get { return triggerOnRisingEdge; }
            set { triggerOnRisingEdge = value; }
        }

        private string trigger;
        public string Trigger
        {
            get { return trigger; }
            set { trigger = value; }
        }

        private string extTTLClock;
        public string ExtTTLClock
        {
            get { return extTTLClock; }
            set { extTTLClock = value; }
        }

        private Dictionary<string, TCLSingleCavityConfig> cavities = new Dictionary<string, TCLSingleCavityConfig>();
        public Dictionary<string, TCLSingleCavityConfig> Cavities
        {
            get { return cavities; }
            set { cavities = value; }
        }

        public void AddCavity(string cavityName)
        {
            Cavities.Add(cavityName, new TCLSingleCavityConfig(cavityName));
        }


        #region Legacy Methods
        // Below are legacy methods to maintain backwards compatability. 

        public string Cavity
        {
            get { return baseRamp; }
            set { baseRamp = value; }
        }

        // Helper method to allow generating a default cavity if none have been added
        public TCLSingleCavityConfig GetDefaultCavity()
        {
            if (cavities.Count == 0)
            {
                AddCavity(configurationName);
                return Cavities[configurationName];
            }
            else if (cavities.Count == 1)
            {
                return Cavities.First().Value;
            }
            else
            {
                throw new System.InvalidOperationException(
                    "If you have more than a single cavity on this instance of TCL then you must change configuration for individuals cavities separately"
                    );
            }
        }

        // These will only work if you have only a single cavity
        public string MasterLaser
        {
            get { return GetDefaultCavity().MasterLaser; }
            set { GetDefaultCavity().MasterLaser = value; }
        }

        public string Ramp
        {
            get { return GetDefaultCavity().RampOffset; }
            set { GetDefaultCavity().RampOffset = value; }
        }

        public void AddLaser(string name, string photodiode)
        {
            GetDefaultCavity().AddSlaveLaser(name, photodiode);
        }

        public void AddFSRCalibration(string name, double spacingbetweenPeaksInVolts)
        {
            GetDefaultCavity().AddFSRCalibration(name, spacingbetweenPeaksInVolts);
        }

        public Dictionary<string, double> DefaultGains
        {
            get { return GetDefaultCavity().DefaultGains; }
            set { GetDefaultCavity().DefaultGains = value; }
        }

        public void AddDefaultGain(string name, double gain)
        {
            GetDefaultCavity().AddDefaultGain(name, gain);
        }       

        
        #endregion 
    }
}
