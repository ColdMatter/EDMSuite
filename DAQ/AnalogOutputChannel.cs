using System;

using NationalInstruments.DAQmx;

namespace DAQ.HAL
{
	/// <summary>
	/// 
	/// </summary>
	public class AnalogOutputChannel : DAQMxChannel
	{
		public AnalogOutputChannel(String name, String physicalChannel)
		{
			this.name = name;
			this.physicalChannel = physicalChannel;
		}

		public void AddToTask(Task task, double outputRangeLow, double outputRangeHigh)
		{
			task.AOChannels.CreateVoltageChannel(
				physicalChannel,
				name,
				outputRangeLow,
				outputRangeHigh,
				AOVoltageUnits.Volts
				);
		}
	}
}
