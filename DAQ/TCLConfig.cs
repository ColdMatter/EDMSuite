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
            defaultGain = 0.5;
            defaultLaserVoltage = 0.0;
            maxInputVoltage = 10.0;
            defaultScanPoints = 1000;
            analogSampleRate = 50000;
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

        private string masterLaser;
        public string MasterLaser
        {
            get { return masterLaser; }
            set { masterLaser = value; }
        }

        private string cavity;
        public string Cavity
        {
            get { return cavity; }
            set { cavity = value; }
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

        private double defaultGain;
        public double DefaultGain
        {
            get { return defaultGain; }
            set { defaultGain = value; }
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

        private string trigger;
        public string Trigger
        {
            get { return trigger; }
            set { trigger = value; }
        }

        private string ramp;
        public string Ramp
        {
            get { return ramp; }
            set { ramp = value; }
        }

        private Dictionary<string, string> lasers = new Dictionary<string,string>();
        
        public Dictionary<string, string> Lasers
        {
            get { return lasers; }
            set { lasers = value; }
        }


        public void AddLaser(string name, string photodiode)
        {
            Lasers.Add(name, photodiode);
        }

















        public int AIConvertRate { get; set; }
    }
}
