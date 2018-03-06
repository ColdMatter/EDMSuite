using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NationalInstruments.DAQmx;
using DAQ.HAL;
using DAQ.Environment;

namespace MoleculeMOTHardwareControl.Controls
{
    public abstract class GenericController : MarshalByRefObject
    {
        public GenericView view;

        public GenericController()
        {
            view = CreateControl();
        }

        abstract protected GenericView CreateControl(); // Derived classes must implement this method to create the controls

        public virtual Dictionary<string, object> Report()
        {
            return null;
        }

        protected AnalogSingleChannelReader CreateAnalogInputReader(string channelName)
        {
            Task task = new Task();
            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channelName]).AddToTask(task, -10.0, 10.0);
            task.Control(TaskAction.Verify);
            return new AnalogSingleChannelReader(task.Stream);
        }

        protected DigitalSingleChannelWriter CreateDigitalOutputWriter(string channelName)
        {
            Task task = new Task();
            ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[channelName]).AddToTask(task);
            task.Control(TaskAction.Verify);
            return new DigitalSingleChannelWriter(task.Stream);
        }
    }
}
