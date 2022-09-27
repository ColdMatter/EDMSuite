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
        private string device;

        public double RangeLow
        {
            get { return rangeLow; }
        }
        public double RangeHigh
        {
            get { return rangeHigh; }
        }

        public string Device
        {
            get { return device; }
        }

		public AnalogOutputChannel(String name, String physicalChannel)
		{
			this.name = name;
			this.physicalChannel = physicalChannel;
            this.rangeLow = -10;
            this.rangeHigh = 10;
            this.device = "/" + physicalChannel.Split('/')[1];
        }

        public AnalogOutputChannel(String name, String physicalChannel, double rangeLow, double rangeHigh)
        {
            this.name = name;
            this.physicalChannel = physicalChannel;
            this.rangeLow = rangeLow;
            this.rangeHigh = rangeHigh;
            this.device = "/" + physicalChannel.Split('/')[1];
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
