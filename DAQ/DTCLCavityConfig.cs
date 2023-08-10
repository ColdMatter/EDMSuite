using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Environment;
using DAQ.HAL;

namespace DAQ.DigitalTransferCavityLock
{
    public class DTCLCavityConfig
    {

        public class LaserConfig
        {
            private DTCLCavityConfig cavityConfig;
            public LaserConfig(DTCLCavityConfig cConfig)
            {
                cavityConfig = cConfig;
            }
            public string Name;
            public string feedbackChannel;
            public string inputChannel;
            public string counterChannel;
            public string timebaseChannel;
            public int timebaseFrequency;
            public string syncChannel = "";

            public AnalogOutputChannel FeedbackChannel
            {
                get
                {
                    return (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[feedbackChannel];
                }
            }

            public CounterChannel InputChannel
            {
                get
                {
                    return (CounterChannel)Environs.Hardware.CounterChannels[inputChannel];
                }
            }

            public CounterChannel Counter
            {
                get
                {
                    return (CounterChannel)Environs.Hardware.CounterChannels[counterChannel];
                }
            }

            public CounterChannel TimebaseChannel
            {
                get
                {
                    return (CounterChannel)Environs.Hardware.CounterChannels[timebaseChannel];
                }
            }

            public CounterChannel SyncChannel
            {
                get
                {
                    if (syncChannel == "")
                        return new CounterChannel(cavityConfig.config.SynchronsiationCounter.Name + "InternalOutput", cavityConfig.config.SynchronsiationCounter.PhysicalChannel + "InternalOutput");
                    return (CounterChannel)Environs.Hardware.CounterChannels[syncChannel];
                }
            }


        }

        public DTCLConfig config;

        public LaserConfig MasterLaser;
        public Dictionary<string, LaserConfig> SlaveLasers = new Dictionary<string, LaserConfig> { };
        public string Name;

        public DTCLCavityConfig(string name, DTCLConfig _config)
        {
            config = _config;
            Name = name;
            MasterLaser = new LaserConfig(this);
        }

        public void AddSlaveLaser(string name, string inputChannel, string feedbackChannel, string counterChannel, string timebaseChannel, int timebaseFreq)
        {
            SlaveLasers.Add(name, new LaserConfig(this));
            SlaveLasers[name].Name = name;
            SlaveLasers[name].feedbackChannel = feedbackChannel;
            SlaveLasers[name].inputChannel = inputChannel;
            SlaveLasers[name].counterChannel = counterChannel;
            SlaveLasers[name].timebaseChannel = timebaseChannel;
            SlaveLasers[name].timebaseFrequency = timebaseFreq;
        }

        public void ConfigureMasterLaser(string inputChannel, string feedbackChannel, string counterChannel, string timebaseChannel, int timebaseFreq)
        {
            MasterLaser.feedbackChannel = feedbackChannel;
            MasterLaser.inputChannel = inputChannel;
            MasterLaser.counterChannel = counterChannel;
            MasterLaser.timebaseChannel = timebaseChannel;
            MasterLaser.timebaseFrequency = timebaseFreq;
        }

        public void AddSlaveLaser(string name, string inputChannel, string feedbackChannel, string counterChannel, string timebaseChannel, int timebaseFreq, string sync)
        {
            AddSlaveLaser(name, inputChannel, feedbackChannel, counterChannel, timebaseChannel, timebaseFreq);
            SlaveLasers[name].syncChannel = sync;
        }

        public void ConfigureMasterLaser(string inputChannel, string feedbackChannel, string counterChannel, string timebaseChannel, int timebaseFreq, string sync)
        {
            ConfigureMasterLaser(inputChannel, feedbackChannel, counterChannel, timebaseChannel, timebaseFreq);
            MasterLaser.syncChannel = sync;
        }

    }
}
