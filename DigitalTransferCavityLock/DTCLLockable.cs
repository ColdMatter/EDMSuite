using System;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;

namespace DigitalTransferCavityLock
{
    public abstract class DTCLLockable
    {
        protected Func<double> resource;
        protected Task output;
        protected AnalogSingleChannelWriter outputWriter;
        private string feedback;
        protected bool locked = false;
        protected double lockLevel;
        public DTCLLockable(Func<double> _resource, string _feedback)
        {
            resource = _resource;
            feedback = _feedback;

            output = new Task("Feedback to " + _feedback);
            FeedbackChannel.AddToTask(output, MinVoltage, MaxVoltage);
            output.Control(TaskAction.Verify);
            outputWriter = new AnalogSingleChannelWriter(output.Stream);

        }

        public double LockLevel
        {
            get
            {
                return lockLevel;
            }
        }

        private AnalogOutputChannel FeedbackChannel
        {
            get
            {
                return (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[feedback];
            }
        }

        private double MaxVoltage
        {
            get
            {
                return FeedbackChannel.RangeHigh;
            }
        }
        private double MinVoltage
        {
            get
            {
                return FeedbackChannel.RangeLow;
            }
        }

        private double currentVoltage;
        public double CurrentVoltage
        {
            get
            {
                return currentVoltage;
            }
            set
            {
                if (value > MaxVoltage)
                    currentVoltage = MaxVoltage;
                else if (value < MinVoltage)
                    currentVoltage = MinVoltage;
                else
                    currentVoltage = value;
                output.Start();
                outputWriter.WriteSingleSample(true, currentVoltage);
                output.Stop();
            }
        }

        public virtual double VoltageError
        {
            get
            {
                return lockLevel - resource();
            }
        }

        public void ArmLock(double LockLevel)
        {
            locked = true;
            lockLevel = LockLevel;
        }
        public void DisarmLock()
        {
            locked = false;
        }
        public abstract void UpdateLock();


    }
}
