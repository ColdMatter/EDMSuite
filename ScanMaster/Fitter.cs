using System;
using NationalInstruments.Analysis.Math;

using Data.Scans;

namespace ScanMaster.Analyze
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class Fitter
	{
		public string Name;
		double[] fittedValues;
		protected double[] lastFittedParameters;

		// Fit the data provided. The parameters list should contain an initial
		// guess. On return it will contain the fitted parameters.
		public void Fit(double[] xdata, double[] ydata, double[] parameters)
		{
			double mse = 0.0;
			try
			{
				fittedValues = CurveFit.NonLinearFit(xdata, ydata, model, parameters, out mse);
				lastFittedParameters = parameters;
			} 
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}

		}

		public abstract double[] SuggestParameters(Scan scanToFit);

		public abstract string ParameterReport
		{
			get;
		}

		// This returns the y-values for the model at the x-data points it was evaluated at.
		public double[] FittedValues
		{
			get
			{
				return fittedValues;
			}
		}

		public override string ToString()
		{
			return Name;
		}


		protected ModelFunctionCallback model;
	}
}
