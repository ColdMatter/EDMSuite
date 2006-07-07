using System;

using NationalInstruments.DAQmx;

namespace DAQ.HAL
{
	/// <summary>
	/// 
	/// </summary>
	public class DAQMxChannel
	{
		protected String name;
		protected String physicalChannel;

		public String Name
		{
			get { return name; }
		}

		public String PhysicalChannel
		{
			get { return physicalChannel; }
		}
	}
}
