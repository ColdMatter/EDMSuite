using System;

using NationalInstruments.DAQmx;

namespace DAQ.HAL
{

	/// <summary>
	/// 
	/// </summary>
	public class CounterChannel : DAQMxChannel
	{ 

		public CounterChannel(String name, String physicalChannel)
		{
			this.name = name;
			this.physicalChannel = physicalChannel;
		}
	}
}
