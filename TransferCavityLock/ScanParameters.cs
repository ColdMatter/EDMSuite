using NationalInstruments.DAQmx;

namespace TransferCavityLock
{
    public class ScanParameters : RampParameters
    {
        public AnalogSingleChannelWriter Writer;
        public AnalogMultiChannelReader Reader;
        public bool Record;
        public double SetPoint = 0.0;
        public RampParameters rampParams = new RampParameters();

        public ScanParameters(AnalogMultiChannelReader r, AnalogSingleChannelWriter w)
        {
            this.Reader = r;
            this.Writer = w;
        }

        public void ArmScan(double low, double high, int sleepTime, int steps, bool record, double setPoint)
        {
            this.Low = low;
            this.High = high;
            this.SleepTime = sleepTime;
            this.Steps = steps;
            this.Record = record;
            this.StepSize = (high - low) / (steps - 1);
            this.SetPoint = setPoint;
        }
        public void AdjustStepSize()
        {
            this.StepSize = (this.High - this.Low) / (this.Steps - 1);
        }
    }
}
