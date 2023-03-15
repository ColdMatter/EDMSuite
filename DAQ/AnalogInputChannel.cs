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
        private Double calibration;
		public bool invert { get; }

		public AnalogInputChannel(String name, String physicalChannel, AITerminalConfiguration terminalConfig) 
		{
			this.name = name;
			this.physicalChannel = physicalChannel;
			this.terminalConfig = terminalConfig;
            this.calibration = 1;
			this.invert = false;
		}

		public AnalogInputChannel(String name, String physicalChannel, AITerminalConfiguration terminalConfig, bool invert)
		{
			this.name = name;
			this.physicalChannel = physicalChannel;
			this.terminalConfig = terminalConfig;
			this.calibration = 1;
			this.invert = invert;
		}


		public AnalogInputChannel(String name, String physicalChannel, AITerminalConfiguration terminalConfig, Double calibration)
        {
            this.name = name;
            this.physicalChannel = physicalChannel;
            this.terminalConfig = terminalConfig;
            this.calibration = calibration;
			this.invert = false;
		}

		public string Device
        {
            get { return '/' + physicalChannel.Split('/')[1]; }
        }


		public AITerminalConfiguration TerminalConfig
		{
			get { return terminalConfig; }
		}

        public Double Calibration
        {
            get { return calibration; }
        }


		public void AddToTask( Task task, double inputRangeLow, double inputRangeHigh )
		{
			AIChannel aichannel = task.AIChannels.CreateVoltageChannel(
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
