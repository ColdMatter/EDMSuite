using System;
using System.Linq;


namespace DAQ.Analyze
{
	/// <summary>
	/// A class to determine the tallest point of a mathematically peaked function, based on the derivative zero-crossing.
	/// </summary>
	public class PeakFinder
	{

	double[] lastParameters;				// a place to store the last good peak location
											// same format as PeakFitter class
		public PeakFinder()
		{
		}

		public double[] FindPeakLocation(double[] domain, double[] range)
		{
			double rise, run, maxPosition=0, maxDerivative=0, minPosition=0, minDerivative=0;
			int maxIndex = 0, minIndex = 0, zeroCrossingIndex = 0;
			double offset = range.Min();
			double[] derivative = new double[domain.Length];


			for (int i = 0; i < Math.Min(domain.Length,range.Length)-1; i++)
			{
				rise = range[i + 1] - range[i];
				run = domain[i + 1] - domain[i];
				if (run == 0) { run = 1; }			// watch out for divide by zero giving an infinite derivative (or really, a runtime exception)
				derivative[i] = rise / run;
				if (derivative[i] > maxDerivative)
				{
					maxDerivative = derivative[i];
					maxPosition = domain[i];
					maxIndex = i;
				}
				if (derivative[i] < minDerivative)
				{
					minDerivative = derivative[i];
					minPosition = domain[i];
					minIndex = i;
				}
			}
			double width = Math.Abs(domain[minIndex] - domain[maxIndex]);

			double zeroCrossing = 0, slope = 0;
			for (int i = maxIndex; i < minIndex + 1; i++)
			{
				if (Math.Sign(derivative[i]) != Math.Sign(derivative[i + 1]))	// if the derivative sign changes
				{
					slope = (derivative[i + 1] - derivative[i]) / (domain[i + 1] - domain[i]);
					zeroCrossingIndex = i;
					zeroCrossing = domain[i] + range[i]/slope;					// simple linear interpolation between the two derivative values above/below zero
					break;
				}
			}

			double[] peakParameters = new double[] { offset, 1, zeroCrossing, width };	// dc offset, scaling (0..1), center, width
			return peakParameters;
		}


		public double[] Parameters
		{
			get
			{
				return lastParameters;
			}
		}

	}
}
