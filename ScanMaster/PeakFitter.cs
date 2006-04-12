using System;
using System.Collections.Generic;
using System.Text;


namespace ScanMaster.Analyze
{
	/// <summary>
	/// A helper class that implements the initial parameter estimation for the fitters that
	/// fit to peaky functions.
	/// </summary>
	public abstract class PeakFitter : ScanMaster.Analyze.Fitter
	{
		public override double[] SuggestParameters(double[] xDat, double[] yDat, double scanStart, double scanEnd)
		{
			double cGuess;
			double qGuess;
			double wGuess;
			double nGuess;
			double ySum = 0;
			double yMax = 0;
			int yMaxIndex = 0;
			// calculate the maximum and the integral
			for (int i = 0; i < yDat.Length; i++)
			{
				ySum += yDat[i];
				if (yDat[i] > yMax)
				{
					yMax = yDat[i];
					yMaxIndex = i;
				}
			}

			nGuess = yDat[0];
			cGuess = xDat[yMaxIndex];
			qGuess = yMax - nGuess;
			wGuess = 0.25 * (xDat[1] - xDat[0]) * (ySum - (yDat.Length * nGuess)) / qGuess; // the 0.25 is chosen fairly arbitrarily

			double[] guess = new double[] { nGuess, qGuess, cGuess, wGuess };
			return guess;
		}
	}
}
