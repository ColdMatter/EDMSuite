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

        public double inputRangeLow;
        public double inputRangeHigh;

		public AnalogInputChannel(String name, String physicalChannel, AITerminalConfiguration terminalConfig) 
		{
            this.nameIt(name); 
			this.physicalChannel = physicalChannel;
			this.terminalConfig = terminalConfig;
            this.calibration = 1;
            this.inputRangeLow = -10;
            this.inputRangeHigh = 10;
		}

        public AnalogInputChannel(String name, String physicalChannel, AITerminalConfiguration terminalConfig, Double calibration)
        {
            this.nameIt(name); 
            this.physicalChannel = physicalChannel;
            this.terminalConfig = terminalConfig;
            this.calibration = calibration;
            this.inputRangeLow = -10;
            this.inputRangeHigh = 10;
        }
        public AnalogInputChannel(String name, String physicalChannel, AITerminalConfiguration terminalConfig, double inputRangeLow, double inputRangeHigh)
        {
            this.nameIt(name);
            this.physicalChannel = physicalChannel;
            this.terminalConfig = terminalConfig;
            this.calibration = 1;
            this.inputRangeLow = inputRangeLow;
            this.inputRangeHigh = inputRangeHigh;
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
