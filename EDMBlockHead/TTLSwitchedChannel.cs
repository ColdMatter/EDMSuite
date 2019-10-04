using System;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.HAL;

namespace EDMBlockHead.Acquire.Channels
{
	/// <summary>
	/// A channel that maps a modulation to a TTL line. The channels output can be inverted
	/// relative to the modulation by setting the invert flag.
	/// </summary>
	public class TTLSwitchedChannel : SwitchedChannel
	{
		public string Channel;
		public bool Invert;

		private bool currentState = false;
		private Task digitalTask;
		DigitalSingleChannelWriter writer;

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
					if (!Invert) writer.WriteSingleSampleSingleLine(true, value);
					else writer.WriteSingleSampleSingleLine(true, !value);
				}	
			}
		}

		public override void AcquisitionStarting()
		{
			if (!Environs.Debug)
			{
				digitalTask = new Task(Channel);
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[Channel]).AddToTask(digitalTask);
				digitalTask.Control(TaskAction.Verify);
				writer = new DigitalSingleChannelWriter(digitalTask.Stream);
			}
		}

		public override void AcquisitionFinishing()
		{
			if (!Environs.Debug) digitalTask.Dispose();
		}
	}
}
