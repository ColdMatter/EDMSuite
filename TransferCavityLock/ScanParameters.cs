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
    public class ScanParameters : RampParameters
    {
        private AnalogSingleChannelWriter writer;
        private AnalogMultiChannelReader reader;
        private bool record;
        private double setPoint;
        private RampParameters rampParams;
        

        
        public AnalogMultiChannelReader Reader
        {
            set { reader = value; }
            get { return reader; }
        }
        public AnalogSingleChannelWriter Writer
        {
            set { writer = value; }
            get { return writer; }
        }
        public bool Record
        {
            set { record = value; }
            get { return record; }
        }
        public double SetPoint
        {
            set { setPoint = value; }
            get { return setPoint; }
        }
        

        public ScanParameters(AnalogMultiChannelReader r, AnalogSingleChannelWriter w)
        {
            this.reader = r;
            this.writer = w;
            this.record = new bool();
            this.setPoint = 0.0;
            this.rampParams = new RampParameters();
        }

        public void ArmScan(double low, double high, int sleepTime, int steps, bool record, double setPoint)
        {
            this.Low = low;
            this.High = high;
            this.SleepTime = sleepTime;
            this.Steps = steps;
            this.record = record;
            this.StepSize = (high - low) / (steps - 1);
            this.SetPoint = setPoint;
        }
        public void AdjustStepSize()
        {
            this.StepSize = (this.High - this.Low) / (this.Steps - 1);
        }
    }
}
