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

        public Dictionary<string, string> slaveLasers = new Dictionary<string, string>();//Name, Channel Name
       

        
        public void AddSlaveLaser(string name, string channel)
        {
            slaveLasers.Add(name, channel);
        }

       


    }
}
