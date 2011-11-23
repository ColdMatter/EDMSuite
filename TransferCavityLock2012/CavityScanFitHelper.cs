using System;
using System.Collections.Generic;
using System.Text;

using DAQ.Analyze;

namespace TransferCavityLock2012
{
    class CavityScanFitHelper
    {


        public static double[] FitLorenzianToData(double[] voltages, double[] signal)
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
        public static double[] CreatePointsFromFit(double[] cavityVoltages, double[] fitCoefficients)
        {
            double[] fitPoints = new double[cavityVoltages.Length];
            double n = fitCoefficients[3];
            double q = fitCoefficients[2];
            double c = fitCoefficients[1];
            double w = fitCoefficients[0];
            for (int i = 0; i < cavityVoltages.Length; i++)
            {
                if (w == 0) w = 0.001; // watch out for divide by zero
                fitPoints[i] = n + q * (1 / (1 + (((cavityVoltages[i] - c) * (cavityVoltages[i] - c)) / ((w / 2) * (w / 2)))));
            }
            return fitPoints;
        }
    }
}
