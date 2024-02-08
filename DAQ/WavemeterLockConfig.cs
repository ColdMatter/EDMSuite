using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.WavemeterLock
{
    /// <summary>
    /// Configuration for wavemeterlock
    /// </summary>
    public class WavemeterLockConfig
    {
        public string name;

        public WavemeterLockConfig(string Name)
        {
            name = Name;
        }

        public WavemeterLockConfig()
        {
            name = "Default";
        }

        public Dictionary<string, string> slaveLasers = new Dictionary<string, string>();//Name, Analog channel Name
        public Dictionary<string, int> channelNumbers = new Dictionary<string, int>();//Name, Wavemeter channel number
        public Dictionary<string, string> lockBlockFlag = new Dictionary<string, string>();//Name, Digital channel name
        public Dictionary<string, double> setPoints = new Dictionary<string, double>();//Laser setpoint
        public Dictionary<string, double> pGains = new Dictionary<string, double>();//P gains
        public Dictionary<string, double> IGains = new Dictionary<string, double>();//I gains


        public void AddSlaveLaser(string name, string channel, int num)
        {
            slaveLasers.Add(name, channel);
            channelNumbers.Add(name, num);
        }

        public void AddLockBlock(string name, string channel)
        {
            lockBlockFlag.Add(name, channel);
        }

        public void AddLaserConfiguration(string name, double setPoint, double PGain, double IGain)
        {
            setPoints.Add(name, setPoint);
            pGains.Add(name, PGain);
            IGains.Add(name, IGain);
        }

    }
}
