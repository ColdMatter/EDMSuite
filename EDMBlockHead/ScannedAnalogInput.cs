using System;

using DAQ.HAL;

namespace EDMBlockHead.Acquire.Input
{
	/// <summary>
	/// This class contains the configuration information for a scanned analog input.
	/// Ideally each input channel could have it's own scan parameters. But they can't.
	/// The DAQ board can only perform one scan at a time (it's multiplexed). So what we do
	/// is configure one big scan (see ScannedAnalogInputCollection) and then chop up the
	/// data for each of the input channels.
	/// 
	/// The big scan is as fast as the fastest scan and as long as the longest. Each channel
	/// is then reduced by either chopping the end off the data (as in the pmt) or binning the
	/// data up (the magnetometer).
	/// </summary>
	public class ScannedAnalogInput
	{
		public AnalogInputChannel Channel;
		public DataReductionMode ReductionMode;
        public int ChopStart;
		public int ChopLength;
		public int AverageEvery;
		public double LowLimit;
		public double HighLimit;
		public double Calibration;
        public int[] ChosenPoints = new int[1]; 

		public double[] Reduce(double[] rawData)
		{
			if (ReductionMode == DataReductionMode.Chop)
			{
				double[] data = new double[ChopLength];
				for (int q = 0 ; q < ChopLength ; q++) data[q] = rawData[q + ChopStart];
				return data;
			}

			if (ReductionMode == DataReductionMode.Average)
			{
				int numPoints = rawData.Length / AverageEvery;
				double[] data = new double[numPoints];

				for (int q = 0 ; q < numPoints ; q++) 
				{
					double avg = 0;
					for (int p = 0 ; p < AverageEvery ; p++) avg+= rawData[ q * AverageEvery + p];
					avg /= AverageEvery;
					data[q] = avg;
				}
				return data;
			}

            //This allows you to choose two points in the tof to serve as your detector. 
            //Designed for the rf reflected Amplitude 
            //if (ReductionMode == DataReductionMode.Select)
            //{
            //    int length = ChosenPoints.Length;
            //    double[] data = new double[length];
            //    foreach (int q in ChosenPoints) data[q] = rawData[q];
            //    return data;
            //}

			return rawData;
		}

		public int CalculateClockRate(int rawClockRate)
		{
			if (ReductionMode == DataReductionMode.Chop)
			{
				return rawClockRate;
			}

			if (ReductionMode == DataReductionMode.Average)
			{
				return rawClockRate / AverageEvery;
			}

			return rawClockRate;
		}
	}

	public enum DataReductionMode
	{
		None,
		Chop,
		Average,
        Select
	}
}
