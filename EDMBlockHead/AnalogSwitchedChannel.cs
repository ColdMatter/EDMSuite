using System;
using System.Collections.Generic;
using System.Text;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.HAL;
using EDMConfig;

namespace EDMBlockHead.Acquire.Channels
{
	public class AnalogSwitchedChannel : SwitchedChannel
	{
		public string Channel;

		private bool currentState = false;
		private Task analogTask;
		AnalogSingleChannelWriter writer;

		public override bool State
		{
			get
			{
				return currentState;
			}
			set
			{
				currentState = value;
				if (!Environs.Debug)
				{
					double step = ((AnalogModulation)Modulation).Step;
					double valToWrite = ((AnalogModulation)Modulation).Centre + (value ? step : -step);
					writer.WriteSingleSample(true, valToWrite);
				}	
			}
		}

		public override void AcquisitionStarting()
		{
			if (!Environs.Debug)
			{
				analogTask = new Task(Channel);
				((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[Channel]).AddToTask(analogTask, 0, 5);
				analogTask.Control(TaskAction.Verify);
				writer = new AnalogSingleChannelWriter(analogTask.Stream);
			}
		}

		public override void AcquisitionFinishing()
		{
			if (!Environs.Debug) analogTask.Dispose();
		}
	}
}
