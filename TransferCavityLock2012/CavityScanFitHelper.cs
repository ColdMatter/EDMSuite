using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using DAQ.Analyze;

namespace TransferCavityLock2012
{
    class CavityScanFitHelper
    {
        public static LorentzianFit FitLorentzianToData(double[] voltages, double[] signal, LorentzianFit bestGuess)
        {
            LorentzianFitter lorentzianFitter = new LorentzianFitter();
            lorentzianFitter.Fit(voltages, signal, new double[] { bestGuess.Background, bestGuess.Amplitude, bestGuess.Centre, bestGuess.Width });
            double[] coefficients = lorentzianFitter.Parameters;

            LorentzianFit fit = new LorentzianFit(coefficients[0], coefficients[1], coefficients[2], coefficients[3]);

            double minCentrePosition = voltages.Min();
            double maxCentrePosition = voltages.Max();
            fit = fitFailSafe(fit, minCentrePosition, maxCentrePosition);

            return fit;
        }

        private static LorentzianFit fitFailSafe(LorentzianFit fit, double lowerLimit, double upperLimit)
        {
            if (fit.Centre < lowerLimit)
            {
                fit.Centre = lowerLimit;
            }
            else if (fit.Centre > upperLimit)
            {
                fit.Centre = upperLimit;
            }
            return fit;
        }


        public static LorentzianFit FitLorentzianToData(double[] voltages, double[] signal)
        {
            double high = getMax(voltages).value;
            double low = getMin(voltages).value;
            System.Diagnostics.Debug.WriteLine("   " + low.ToString() + "   " + high.ToString());
            LorentzianFitter lorentzianFitter = new LorentzianFitter();

            // takes the parameters (in this order, in the double[])
            // N: background
            // Q: signal
            // c: centre
            // w: width

            dataPoint max = getMax(signal);
            dataPoint min = getMin(signal);
            lorentzianFitter.Fit(voltages, signal,
                new double[] {min.value, max.value - min.value,
                    voltages[max.index], (high - low)/10,
                });

            double[] coefficients = lorentzianFitter.Parameters;

            fitFailSafe(coefficients, low, high);

            return new LorentzianFit(coefficients[0], coefficients[1], coefficients[2], coefficients[3]);
        }

        // This function speeds up the fitting process by passing the last fitted parameters as a guess, 
        // and by restricting the fit to only those points near to the peak
        public static LorentzianFit FitLorentzianToData(double[] voltages, double[] signal, double[] parameters, double pointsToConsiderEitherSideOfPeakInFWHMs, int maximumNLMFSteps)
        {

            double high = getMax(voltages).value;
            double low = getMin(voltages).value;
            double pointsToConsider = 0;
            System.Diagnostics.Debug.WriteLine("   " + low.ToString() + "   " + high.ToString());
            LorentzianFitter lorentzianFitter = new LorentzianFitter();

            double[][] allxypairs = new double[voltages.Length][];

            int j = 0;

            for (int i = 0; i < voltages.Length; i++)
            {
                allxypairs[i] = new double[2];
                allxypairs[i][0] = voltages[i];
                allxypairs[i][1] = signal[i];
            }


            if (pointsToConsiderEitherSideOfPeakInFWHMs * parameters[0] < 10)
            {
                pointsToConsider = 10;
            }
            else
            {
                pointsToConsider = pointsToConsiderEitherSideOfPeakInFWHMs * parameters[0];
            }

            for (int i = 0; i < voltages.Length; i++)
            {
                if ((allxypairs[i][0] > (parameters[1] - pointsToConsider)) && (allxypairs[i][0] < (parameters[1] + pointsToConsider)))
                {
                    j++;
                }
            }

            double[] selectedvoltages = new double[j];
            double[] selectedsignal = new double[j];


            for (int i = 0, k = 0; i < voltages.Length; i++)
            {
                if ((allxypairs[i][0] > (parameters[1] - pointsToConsider)) && (allxypairs[i][0] < (parameters[1] + pointsToConsider)))
                {
                    selectedvoltages[k] = allxypairs[i][0];
                    selectedsignal[k] = allxypairs[i][1];
                    k++;
                }
            }
            // takes the parameters (in this order, in the double[])
            // N: background
            // Q: signal
            // c: centre
            // w: width

            dataPoint max = getMax(signal);
            dataPoint min = getMin(signal);
            lorentzianFitter.Fit(selectedvoltages, selectedsignal,
                new double[] { parameters[3], parameters[2], parameters[1], parameters[0] }, 0, 0, maximumNLMFSteps);

            double[] coefficients = lorentzianFitter.Parameters;

            fitFailSafe(coefficients, low, high);

            return new LorentzianFit(coefficients[0], coefficients[1], coefficients[2], coefficients[3]);
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

        private static double[] fitFailSafe(double[] coefficients, double limitLow, double limitHigh)
        {
            if (coefficients[2] < limitLow)
            { coefficients[2] = limitLow; }
            else if (coefficients[2] > limitHigh)
            { coefficients[2] = limitHigh; }

            return coefficients;
        }

        /// <summary>
        /// A helper function. generates a bunch of points from a some fit coefficients.
        /// </summary>
        /// <param name="cavityVoltages"></param>
        /// <param name="fitCoefficients"></param>
        /// <returns></returns>
        public static double[] CreatePointsFromFit(double[] cavityVoltages, LorentzianFit fit)
        {
            double[] fitPoints = new double[cavityVoltages.Length];
            double n = fit.Background;
            double q = fit.Amplitude;
            double c = fit.Centre;
            double w = fit.Width;
            for (int i = 0; i < cavityVoltages.Length; i++)
            {
                if (w == 0) w = 0.001; // watch out for divide by zero
                fitPoints[i] = n + q * (1 / (1 + (((cavityVoltages[i] - c) * (cavityVoltages[i] - c)) / ((w / 2) * (w / 2)))));
            }
            return fitPoints;
        }
    }
}
