using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.TransferCavityLock2012
{
    public class TCLSingleCavityConfig
    {
        public string Name;

        public TCLSingleCavityConfig(string name)
        {
            Name = name;
        }

        private string masterLaser;
        public string MasterLaser
        {
            get { return masterLaser; }
            set { masterLaser = value; }
        }

        private string rampOffset;
        public string RampOffset // This is what used to be called ramp in previous version
        {
            get { return rampOffset; }
            set { rampOffset = value; }
        }

        private Dictionary<string, string> slaveLasers = new Dictionary<string, string>();
        public Dictionary<string, string> SlaveLasers
        {
            get { return slaveLasers; }
            set { slaveLasers = value; }
        }

        private Dictionary<string, double> fsrCalibrations = new Dictionary<string, double>();
        public Dictionary<string, double> FSRCalibrations
        {
            get { return fsrCalibrations; }
            set { fsrCalibrations = value; }
        }

        private Dictionary<string, double> defaultGains = new Dictionary<string, double>();
        public Dictionary<string, double> DefaultGains
        {
            get { return defaultGains; }
            set { defaultGains = value; }
        }

        public void AddSlaveLaser(string name, string photodiode)
        {
            slaveLasers.Add(name, photodiode);
        }

        public Dictionary<string, string> BlockChannels = new Dictionary<string, string>();
        public void AddLockBlocker(string laserName, string blockFlagChannel)
        {
            BlockChannels.Add(laserName, blockFlagChannel);
        }

        public void AddFSRCalibration(string name, double spacingbetweenPeaksInVolts)
        {
            fsrCalibrations.Add(name, spacingbetweenPeaksInVolts);
        }

        public void AddDefaultGain(string name, double gain)
        {
            defaultGains.Add(name, gain);
        }       
    }
}
