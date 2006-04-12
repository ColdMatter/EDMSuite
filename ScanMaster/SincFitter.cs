using System;
using NationalInstruments.Analysis.Math;


namespace ScanMaster.Analyze
{
	/// <summary>
	/// A class to fit sinc-squareds. This sinc-squared has a peak height of 1.
	/// The w parameter that is returned is the FWHM.
	/// </summary>
	public class SincFitter : ScanMaster.Analyze.PeakFitter
	{

		public SincFitter()
		{
			Name = "Sinc squared";
			model = new ModelFunctionCallback(sinc);
		}

		// takes the parameters (in this order, in the double[])
		// N: background
		// Q: signal
		// c: centre
		// w: width
		private double sinc(double x, double[] parameters)
		{
			double n = parameters[0];
			double q = parameters[1];
			double c = parameters[2];
			double w = parameters[3];
			if (x == c) x += double.Epsilon; // watch out for divide by zero
			double z = 2.78 * (x - c) / w;
			return n + q * Math.Pow(Math.Sin(z) / z, 2);
		}

		public override string ParameterReport
		{
			get
			{
				return "n: " + lastFittedParameters[0].ToString("G3") +
					" q: " + lastFittedParameters[1].ToString("G3") +
					" c: " + lastFittedParameters[2].ToString("G6") +
					" w: " + lastFittedParameters[3].ToString("G3");
			}
		}
	}
}
