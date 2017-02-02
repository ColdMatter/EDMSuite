using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;
using Data;
using Data.Scans;
using DAQ.Analog;

namespace DAQ.Analog
{
    public class MMAIConfiguration
    {
        public Dictionary<string, MMAIChannelConfiguration> AIChannels = new Dictionary<string, MMAIChannelConfiguration>();
        private int samplerate;
        private int samples;
        private double[,] aiData;

        //This class will contain all the required information to configure the timed analog-input tasks when using MOTMaster.
        public MMAIConfiguration()
        {

        }

        public void AddChannel(string channelName, double aiLow, double aiHigh)
        {
            MMAIChannelConfiguration AIChanConfig = new MMAIChannelConfiguration(aiLow, aiHigh);

            AIChannels[channelName] = AIChanConfig;
        }

        public int SampleRate
        {
            get
            {
                return samplerate;
            }
            set
            {
                samplerate = value;
            }
        }

        public int Samples
        {
            get
            {
                return samples;
            }
            set
            {
                samples = value;
            }
        }

        public int nChannels
        {
            get
            {
                return AIChannels.Count();
            }
        }

        public double[,] AIData
        {
            get
            { return aiData; }

            set
            {
                aiData= value;
            }
        }

    }
}
