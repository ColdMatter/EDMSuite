using System;
using System.Collections.Generic;
using System.Text;


namespace DAQ.Analyze
{
	/// <summary>
	/// A helper class that implements the initial parameter estimation for the fitters that
	/// fit to peaky functions.
	/// </summary>
	public abstract class exponentialPeakFitter : DAQ.Analyze.Fitter
	{
		public override double[] SuggestParameters(double[] xDat, double[] yDat, double scanStart, double scanEnd)
		{
			double bgGuess;
			double ampGuess;
			double tauGuess;
			double yMax = 0;
			double yMin = yDat[0];
			double xMin = xDat[0];
			double xMax = xDat[0];
			int yMaxIndex = 0;
			// calculate the maxima, the minima
			for (int i = 0; i < yDat.Length; i++)
			{
				
				if (yDat[i] > yMax)
				{
					yMax = yDat[i];
					yMaxIndex = i;
				}
				if (yDat[i] < yMin) yMin = yDat[i];
				if (xDat[i] < xMin) xMin = xDat[i];
				if (xDat[i] > xMax) xMax = xDat[i];
			}

			//nGuess = yDat[0];
			bgGuess = yMin;
			ampGuess = yMax-yMin;
			tauGuess = (xMax-xMin)/3; // we typically scan more than three lifetime for a duration scan

			double[] guess = new double[] { bgGuess, ampGuess, tauGuess};
			return guess;
		}
	}
}