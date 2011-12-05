//using System;
//using NationalInstruments.Analysis.Math;


//namespace ScanMaster.Analyze
//{
//    /// <summary>
//    /// This class uses is the bsae class for fitters. A fitter is specialized to a particular fitting
//    /// function by deriving from this class and overriding abtract methods to provide the function's
//    /// value, initial parameter estimates, and a parameter report. This class takes care of the mechanics
//    /// of doing the fit and providing fitted data.
//    /// </summary>
//    public abstract class Fitter
//    {
//        public string Name;
//        double[] fittedValues;
//        protected double[] lastFittedParameters;
//        double[] lastFittedValues;

//        // Fit the data provided. The parameters list should contain an initial
//        // guess. On return it will contain the fitted parameters.
//        public void Fit(double[] xdata, double[] ydata, double[] parameters)
//        {
//            double mse = 0.0;
//            try
//            {
//                fittedValues = CurveFit.NonLinearFit(xdata, ydata, model, parameters, out mse, 1000);
//                // save the fitted values, in case the fit fails next time
//                if (Double.IsNaN(fittedValues[0])) repairFittedValues(xdata, ydata);
//                lastFittedValues = fittedValues;
//                lastFittedParameters = parameters;
//            } 
//            catch (Exception e)
//            {
//                // need to fill the data array with something if the fit method fails
//                repairFittedValues(xdata, ydata);
//                lastFittedParameters = new double[parameters.Length];
//                Console.WriteLine(e.ToString());
//            }

//        }

//        private void repairFittedValues(double[] xdata, double[] ydata)
//        {
//            // need to fill the data array with something if the fit method fails
//            if (lastFittedValues != null)
//            {
//                int lfvLength = lastFittedValues.Length;
//                Array.Resize(ref lastFittedValues, ydata.Length);
//                fittedValues = lastFittedValues;
//                for (int i = lfvLength; i < fittedValues.Length; i++)
//                    fittedValues[i] = fittedValues[lfvLength - 1];
//            }
//            else
//            {
//                fittedValues = new double[ydata.Length];
//                for (int i = 0; i < ydata.Length; i++) fittedValues[i] = 1;
//            }
//        }

//        public abstract double[] SuggestParameters(double[] xDat, double[] yDat, double scanStart, double scanEnd);

//        public abstract string ParameterReport
//        {
//            get;
//        }

//        // This returns the y-values for the model at the x-data points it was evaluated at.
//        public double[] FittedValues
//        {
//            get
//            {
//                return fittedValues;
//            }
//        }

//        public override string ToString()
//        {
//            return Name;
//        }


//        protected ModelFunctionCallback model;
//    }
//}
