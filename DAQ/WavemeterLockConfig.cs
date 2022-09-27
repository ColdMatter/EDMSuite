using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.WavemeterLock
{
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


        public void AddSlaveLaser(string name, string channel, int num)
        {
            slaveLasers.Add(name, channel);
            channelNumbers.Add(name, num);
        }

       


    }
}
