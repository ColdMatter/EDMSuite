using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.HAL;
using DAQ.Environment;

namespace DAQ.DigitalTransferCavityLock
{
    public class DTCLConfig
    {
        public Dictionary<string, DTCLCavityConfig> cavities = new Dictionary<string, DTCLCavityConfig>();
        public string rampOut;
        public string synchronisationCounter;
        public string timebaseChannel;
        public int timebaseFrequency;

        public double defaultCavityGain = -0.1;
        public double defaultGain = 0.1;

        public double defaultRampAmplitude = 4;
        public double defaultRampFrequency = 250;
        public double defaultRampOffset = 0;

        public double MHzConv = 500;

        public string resetOut = "";

        public CounterChannel ResetOut
        {
            get
            {
                return (CounterChannel)Environs.Hardware.CounterChannels[resetOut];
            }
        }

        public AnalogOutputChannel RampOut
        {
            get
            {
                return (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[rampOut];
            }
        }

        public CounterChannel SynchronsiationCounter
        {
            get
            {
                return (CounterChannel)Environs.Hardware.CounterChannels[synchronisationCounter];
            }
        }

        public CounterChannel TimebaseChannel
        {
            get
            {
                return (CounterChannel)Environs.Hardware.CounterChannels[timebaseChannel];
            }
        }

        public DTCLConfig(string syncCounter)
        {
            synchronisationCounter = syncCounter;
        }

        public void AddCavity(string name)
        {
            DTCLCavityConfig newCavity = new DTCLCavityConfig(name, this);
            cavities.Add(name, newCavity);
        }

    }
}
