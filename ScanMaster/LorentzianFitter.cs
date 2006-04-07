using System;
using NationalInstruments.Analysis.Math;

using Data.Scans;

namespace ScanMaster.Analyze
{
	/// <summary>
	/// 
	/// </summary>
	public class LorentzianFitter : ScanMaster.Analyze.Fitter
	{

		public LorentzianFitter()
		{
			Name = "Lorentzian";
			model = new ModelFunctionCallback(lorentzian);
		}

		// takes the parameters (in this order, in the double[])
		// N: background
		// Q: signal
		// c: centre
		// w: width
		private double lorentzian(double x, double[] parameters)
		{
			double n = parameters[0];
			double q = parameters[1];
			double c = parameters[2];
			double w = parameters[3];

			return n + q * ( 1 / ( 1 + (((x - c)*(x - c)) / w*w)));
		}

		public override double[] SuggestParameters(Scan scanToFit)
		{
			return new double[] {100,400,5,2};
		}

		public override string ParameterReport
		{
			get
			{
				return "n: " + lastFittedParameters[0] +
					" q: " + lastFittedParameters[0] +
					" c: " + lastFittedParameters[0] +
					" w: " + lastFittedParameters[0];
			}
		}


	}
}
