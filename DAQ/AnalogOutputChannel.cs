using System;

using NationalInstruments.DAQmx;

namespace DAQ.HAL
{
	/// <summary>
	/// 
	/// </summary>
	public class AnalogOutputChannel : DAQMxChannel
	{
        private double rangeLow, rangeHigh;
        public double RangeLow
        {
            get { return rangeLow; }
        }
        public double RangeHigh
        {
            get { return rangeHigh; }
        }


		public AnalogOutputChannel(String name, String physicalChannel)
		{
			this.name = name;
			this.physicalChannel = physicalChannel;
            this.rangeLow = -10;
            this.rangeHigh = 10;
        }

        public AnalogOutputChannel(String name, String physicalChannel, double rangeLow, double rangeHigh)
        {
            this.name = name;
            this.physicalChannel = physicalChannel;
            this.rangeLow = rangeLow;
            this.rangeHigh = rangeHigh;
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
