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
        public double LockLevel;
        public double gain;
        public DTCLLockable(Func<double> _resource, string _feedback)
        {
            resource = _resource;
            feedback = _feedback;

            output = new Task("Feedback to " + ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[feedback]).Name);
            FeedbackChannel.AddToTask(output, MinVoltage, MaxVoltage);
            output.Control(TaskAction.Verify);
            outputWriter = new AnalogSingleChannelWriter(output.Stream);
            CurrentVoltage = 0;

        }

        public bool Locked
        {
            get
            {
                return locked;
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

        public virtual double LockError
        {
            get
            {
                return LockLevel - resource();
            }
        }

        public void ArmLock(double _LockLevel, double Gain)
        {

            locked = true;
            LockLevel = _LockLevel;
            gain = Gain;
        }

        public void DisarmLock()
        {
            locked = false;
        }
        public virtual void UpdateLock()
        {
            if (!locked)
                return;
            CurrentVoltage = CurrentVoltage + LockError * gain;
        }


    }
}
