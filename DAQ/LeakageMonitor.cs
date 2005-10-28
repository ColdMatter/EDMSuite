using System;
using System.Threading;
using NationalInstruments.DAQmx;
using NationalInstruments.VisaNS;
using DAQ.Environment;
using DAQ.HAL;

namespace DAQ.HAL
{
	public class LeakageMonitor
	{
		private Random rn;
		private CounterChannel currentLeakageCounterChannel;
		private Task counterTask;
		private CounterReader leakageReader;
	
		// calibration constants
		private double slope;
		private double offset;

		public LeakageMonitor(CounterChannel clChannel, double slope, double offset)
		{
			currentLeakageCounterChannel = clChannel;
			this.slope = slope;
			this.offset = offset;
			rn = new Random();
		}

		public void Initialize()
		{
			counterTask = new Task("");	
			if (!Environs.Debug)
			{
				counterTask.CIChannels.CreateFrequencyChannel(
					currentLeakageCounterChannel.PhysicalChannel,
					"",
					4000,
					6000,
					CIFrequencyStartingEdge.Rising,
					CIFrequencyMeasurementMethod.HighFrequencyTwoCounter,
					.1,
					10,			
					CIFrequencyUnits.Hertz
					);	
				counterTask.Stream.Timeout = 100;
			}
			leakageReader = new CounterReader(counterTask.Stream);	
		}

		public double GetCurrent()
		{
			double raw;
			if (!Environs.Debug)
			{
				raw = leakageReader.ReadSingleSampleDouble();
				counterTask.Control(TaskAction.Unreserve);
			}
			else
			{
				raw = rn.NextDouble() * 5000;
			}
			return (raw - offset)/(slope * 10000000);
		}
	}
}
