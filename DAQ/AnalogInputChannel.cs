using System;

using NationalInstruments.DAQmx;

namespace DAQ.HAL
{

	/// <summary>
	/// 
	/// </summary>
	public class AnalogInputChannel : DAQMxChannel
	{
		
		private AITerminalConfiguration terminalConfig;

		public AnalogInputChannel(String name, String physicalChannel, AITerminalConfiguration terminalConfig) 
		{
			this.name = name;
			this.physicalChannel = physicalChannel;
			this.terminalConfig = terminalConfig;
		}

		public AITerminalConfiguration TerminalConfig
		{
			get { return terminalConfig; }
		}

		public void AddToTask( Task task, double inputRangeLow, double inputRangeHigh )
		{
			task.AIChannels.CreateVoltageChannel(
				physicalChannel,
				name,
				terminalConfig,
				inputRangeLow,
				inputRangeHigh,
				AIVoltageUnits.Volts
				);
		}
	}
}
