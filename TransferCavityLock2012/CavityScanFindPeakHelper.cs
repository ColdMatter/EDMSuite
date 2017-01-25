using System;
using System.Collections.Generic;
using System.Text;

using DAQ.Analyze;

namespace TransferCavityLock2012
{
	class CavityScanFindPeakHelper
	{

		public static double[] FindPeak(double[] voltages, double[] signal)
		{

			double high = getMax(voltages).value;
			double low = getMin(voltages).value;
			System.Diagnostics.Debug.WriteLine("   " + low.ToString() + "   " + high.ToString());
			PeakFinder peakFinder = new PeakFinder();

			dataPoint max = getMax(signal);
			dataPoint min = getMin(signal);
			double[] coefficients = peakFinder.FindPeakLocation(voltages, signal);	// returns {offset, scaling, zeroCrossing, width}

			return new double[] { coefficients[3], coefficients[2], coefficients[1], coefficients[0] }; //to be consistent with old convention.
		}

		private class dataPoint
		{
			public double value;
			public int index;
		}
		private static dataPoint getMax(double[] data)
		{
			dataPoint max = new dataPoint();
			max.value = 0;
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] > max.value)
				{
					max.value = data[i];
					max.index = i;
				}
			}
			return max;
		}

		private static dataPoint getMin(double[] data)
		{
			dataPoint min = new dataPoint();
			min.value = data[0];
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] < min.value)
				{
					min.value = data[i];
					min.index = i;
				}
			}
			return min;
		}

	}
}
