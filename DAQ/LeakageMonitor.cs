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
        public double Slope
        {
            get
            {
                return slope;
            }
            set
            {
                slope = value;
            }
        }
        public double Offset
        {
            get
            {
                return offset;
            }
            set
            {
                offset = value;
            }
        }
        public double MeasurementTime
        {
            get
            {
                return measurementTime;
            }
            set
            {
                measurementTime = value;
                setMeasurementTime();
            }
        }


        private double slope;
        private double offset;
        private double measurementTime;

		public LeakageMonitor(CounterChannel clChannel, double slope, double offset, double measurementTime)
		{
			currentLeakageCounterChannel = clChannel;
			this.slope = slope;
			this.offset = offset;
            this.measurementTime = measurementTime;
			rn = new Random();
		}

		public void Initialize()
		{
			counterTask = new Task("");	
			if (!Environs.Debug)
			{
               
				this.counterTask.CIChannels.CreateFrequencyChannel(
					currentLeakageCounterChannel.PhysicalChannel,
					"",
					3000,
					6000,
					CIFrequencyStartingEdge.Rising,
					CIFrequencyMeasurementMethod.HighFrequencyTwoCounter,
                    measurementTime,
					10,			
					CIFrequencyUnits.Hertz
					);	
				counterTask.Stream.Timeout = (int)(1000 * measurementTime);
			}
			leakageReader = new CounterReader(counterTask.Stream);	
		}




		private double getRawCount()
		{
			double raw;
			if (!Environs.Debug)
			{
                try
                {
                    raw = leakageReader.ReadSingleSampleDouble();
                    counterTask.Control(TaskAction.Unreserve);
                }
                catch
                {
                    raw = 0;
                }
			}
			else
			{
				raw = rn.NextDouble() * 5000;
			}
			return raw;
		}

        public double GetCurrent()
        {
            return ((getRawCount() - offset) / slope);
        }


        public void Calibrate()
        {
            offset = getRawCount();
            return;
        }

        private void setMeasurementTime()
        {
            counterTask.Stream.Timeout = (int)(1000 * measurementTime);
            counterTask.CIChannels[0].FrequencyMeasurementTime = measurementTime;
        }        

	}
}
