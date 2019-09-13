using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.TransferCavityLock2012;

namespace TransferCavityLock2012
{
    public class Cavity
    {
        public string Name;
        public Controller Controller;
        public MasterLaser Master;
        public Dictionary<string, SlaveLaser> SlaveLasers;

        public Cavity(TCLSingleCavityConfig config)
        {
            Name = config.Name;
            Master = new MasterLaser(config.RampOffset, config.MasterLaser, this);
            if (config.DefaultGains.ContainsKey("Master"))
            {
                Master.Gain = config.DefaultGains["Master"];
            }
            else
            {
                Master.Gain = 1.0;
            }
            SlaveLasers = new Dictionary<string, SlaveLaser>();
            foreach (KeyValuePair<string, string> entry in config.SlaveLasers)
            {
                string laser = entry.Key;
                SlaveLaser slave = new SlaveLaser(laser, entry.Value, this);
                slave.FSRCalibration = config.FSRCalibrations[entry.Key];
                if (config.DefaultGains.ContainsKey(laser))
                {
                    slave.Gain = config.DefaultGains[laser];
                }
                else
                {
                    slave.Gain = 0.0;
                }
                if (config.BlockChannels.ContainsKey(laser))
                {
                    slave.BlockChannel = config.BlockChannels[laser];
                }
                SlaveLasers.Add(laser, slave);
            }
        }

        public Laser[] GetAllLasers()
        {
            Laser[] lasers = new Laser[SlaveLasers.Count + 1];
            lasers[0] = Master;
            Array.Copy(SlaveLasers.Values.ToArray(), 0, lasers, 1, SlaveLasers.Count);
            return lasers;
        }

    }
}
