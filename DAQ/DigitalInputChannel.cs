using System;

using NationalInstruments.DAQmx;

using DAQ.Pattern;

namespace DAQ.HAL
{
	/// <summary>
	/// 
	/// </summary>
	public class DigitalInputChannel : DAQMxChannel
	{
		private string device;
		private int port;
		private int line;

		public DigitalInputChannel( string name, string device, int port, int line )
		{
			this.name = name;
			this.device = device;
			this.port = port;
			this.line = line;
           
			physicalChannel = device + "/port" + port + "/line" + line;
		}

		public int BitNumber
		{
			get { return PatternBuilder32.ChannelFromNIPort(port, line); }
		}

		public void AddToTask(Task task)
		{
			task.DIChannels.CreateChannel(
				PhysicalChannel,
				name,
				ChannelLineGrouping.OneChannelForAllLines
				);
		}
	}
}
