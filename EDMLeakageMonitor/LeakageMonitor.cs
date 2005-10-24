using System;
using System.Threading;
using NationalInstruments.DAQmx;
using NationalInstruments.VisaNS;
using DAQ.Environment;
using DAQ.HAL;



namespace EDMLeakageMonitor
{
	public class LeakageMonitor
	{
		private Random rn;
		private CounterChannel currentLeakageCounterChannel;
		private Task counterTask;
		private CounterReader leakageReader;
		private bool breakAcquisition = false;	
		public bool BreakAcquisition
		{
			set
			{ 
				breakAcquisition = value;
			}
			get
			{
				return breakAcquisition;
			}
		}


		// calibration constants
		private double slope;
		private double offset;

		public LeakageMonitor(CounterChannel clChannel, double slope, double offset)
		{
			currentLeakageCounterChannel = clChannel;
			this.slope = slope;
			this.offset = offset;
			//Thread.Sleep(10);
			rn = new Random(DateTime.Now.Millisecond);
		}

		public void Initialize()
		{
			counterTask = new Task("");	
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

				leakageReader = new CounterReader(counterTask.Stream);
				
		
		}

		public double GetCurrent()
		{
			try
			{
				double data = this.leakageReader.ReadSingleSampleDouble();
				return ConvertRawToCurrent(data);
			}
			catch(DaqException)
			{
				return 0;
			}
		}

		public void Kill()
		{
			counterTask.Stop();
			counterTask.Dispose();
		}

		private double meanArray(double[] data)
		{
			double runningSum = new double();
			runningSum=0;
			for(int i=0;i<data.Length;i++)
			{
				runningSum=runningSum+data[i];
			}

			return runningSum/data.Length;
		}

		private double ConvertRawToCurrent( double raw )
		{
			return ((raw-offset)/(slope*10000000));
		}
	}
}
