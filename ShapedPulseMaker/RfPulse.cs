using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RfArbitraryWaveformGenerator
{
    [Serializable]
    public class RfPulse
    {
        public RfPulse()
        {
            pulseName = "untitled";
            pulseLength = 4;
            sampleRate = 100000000;
            totalSamples = 400;
        }

        public RfPulse(string name)
        {
            pulseName = name;
            pulseLength = 4;
            sampleRate = 100000000;
            totalSamples = 400;
        }

        private string pulseName;
        public string Name
        {
            get { return pulseName; }
            set { pulseName = value; }
        }

        private double pulseLength;
        public double PulseLength
        {
            get { return pulseLength; }
            set { pulseLength = value; }
        }

        private double sampleRate;
        public double SampleRate
        {
            get { return sampleRate; }
            set { sampleRate = value; }
        }

        private int totalSamples;
        public int TotalSamples
        {
            get { return totalSamples; }
            set 
            { 
                totalSamples = value;
                iData = new double[totalSamples];
                qData = new double[totalSamples];
            }
        }

        private double[] iData;
        public double[] IData
        {
            get { return iData; }
            set { iData = value; }
        }

        private double[] qData;
        public double[] QData
        {
            get { return qData; }
            set { qData = value; }
        }

    }
}
