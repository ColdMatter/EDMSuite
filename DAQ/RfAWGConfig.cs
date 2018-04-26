using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace DAQ.RfArbitraryWaveformGenerator
{
    public class RfAWGConfig
    {
          
        public RfAWGConfig()
        {
            defaultScanPoints = 1000;
        }

        private int tcpChannel;
        public int TCPChannel
        {
            get { return tcpChannel; }
            set { tcpChannel = value; }
        }

        private int defaultScanPoints;
        public int DefaultScanPoints
        {
            get { return defaultScanPoints; }
            set { defaultScanPoints = value; }
        }

        private double sampleRate;
        public double SampleRate
        {
            get { return sampleRate; }
            set { sampleRate = value; }
        }

        private double pulseLengthInMicroseconds;
        public double PulseLengthInMicroseconds
        {
            get { return pulseLengthInMicroseconds; }
            set { pulseLengthInMicroseconds = value; }
        }     

    }
}
