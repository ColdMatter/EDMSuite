using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using Data;
using Data.Scans;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;
using DAQ.Remoting;
using System.Windows.Forms;
using NationalInstruments.Analysis.Math;

namespace TransferCavityLock
{
    public class RampParameters
    {
        private double low;
        private double high;
        private int steps;
        private int sleepTime;
        private double stepSize;

        public RampParameters()
        {
            this.low = new double();
            this.high = new double();
            this.steps = new int();
            this.sleepTime = new int();
            this.stepSize = new double();
        }

        public double Low
        {
            set { low = value; }
            get { return low; }
        }
        public double High
        {
            set { high = value; }
            get { return high; }
        }
        public int Steps
        {
            set { steps = value; }
            get { return steps; }
        }
        public int SleepTime
        {
            set { sleepTime = value; }
            get { return sleepTime; }
        }
        public double StepSize
        {
            set { stepSize = value; }
            get { return stepSize; }
        }
    }
}
