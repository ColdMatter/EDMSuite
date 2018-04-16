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

		public AnalogInputChannel(String name, String physicalChannel, AITerminalConfiguration terminalConfig) 
		{
            this.nameIt(name);
			this.physicalChannel = physicalChannel;
			this.terminalConfig = terminalConfig;
            this.calibration = 1;
		}

        public AnalogInputChannel(String name, String physicalChannel, AITerminalConfiguration terminalConfig, Double calibration)
        {
            this.nameIt(name);
            this.physicalChannel = physicalChannel;
            this.terminalConfig = terminalConfig;
            this.calibration = calibration;
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
