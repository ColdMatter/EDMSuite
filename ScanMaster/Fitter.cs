using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScanMaster.Analyze
{
    /// <summary>
    /// This class uses is the bsae class for fitters. A fitter is specialized to a particular fitting
    /// function by deriving from this class and overriding abtract methods to provide the function's
    /// value, initial parameter estimates, and a parameter report. This class takes care of the mechanics
    /// of doing the fit and providing fitted data.
    /// </summary>
    public abstract class Fitter
    {
        public string Name;
        double[] fittedValues;
        protected double[] lastFittedParameters;
        double[] lastFittedValues;

        // Fit the data provided. The parameters list should contain an initial
        // guess. On return it will contain the fitted parameters.
        public void Fit(double[] xdata, double[] ydata, double[] parameters)
        {
            try
            {
                 // massage the x-data into the format alglib likes
                double[,] xDataAlg = new double[xdata.Length, 1];

                for (int i = 0; i < xdata.Length; i++) xDataAlg[i,0] = xdata[i];
                double epsf = 0;
                double epsx = 0.000000001;
                int maxits = 0;
                int info;
                alglib.lsfitstate state;
                alglib.lsfitreport rep;
                double diffstep = 0.001;
                alglib.lsfitcreatef(xDataAlg, ydata, parameters, diffstep, out state);
                alglib.lsfitsetcond(state, epsf, epsx, maxits);
                alglib.lsfitfit(state, model, null, null);
                alglib.lsfitresults(state, out info, out parameters, out rep);

                //calculate the fitted values
                fittedValues = new double[xdata.Length];
                for (int i = 0; i < xdata.Length; i++)
                {
                    double yValue = 0;
                    double[] xValueArr = new double[] { xdata[i] };
                    model(parameters, xValueArr, ref yValue, null);
                    fittedValues[i] = yValue;
                }

                // save the fitted values, in case the fit fails next time
                if (Double.IsNaN(fittedValues[0])) repairFittedValues(xdata, ydata);
                lastFittedValues = fittedValues;
                lastFittedParameters = parameters;
            }
            catch (Exception e)
            {
                // need to fill the data array with something if the fit method fails
                repairFittedValues(xdata, ydata);
                lastFittedParameters = new double[parameters.Length];
                Console.WriteLine(e.ToString());
            }

        }

        private void repairFittedValues(double[] xdata, double[] ydata)
        {
            // need to fill the data array with something if the fit method fails
            if (lastFittedValues != null)
            {
                int lfvLength = lastFittedValues.Length;
                Array.Resize(ref lastFittedValues, ydata.Length);
                fittedValues = lastFittedValues;
                for (int i = lfvLength; i < fittedValues.Length; i++)
                    fittedValues[i] = fittedValues[lfvLength - 1];
            }
            else
            {
                fittedValues = new double[ydata.Length];
                for (int i = 0; i < ydata.Length; i++) fittedValues[i] = 1;
            }
        }

        public abstract double[] SuggestParameters(double[] xDat, double[] yDat, double scanStart, double scanEnd);

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


        protected alglib.ndimensional_pfunc model;
    }
}
