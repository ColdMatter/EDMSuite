using System;
using System.Collections.Generic;
using System.Text;

namespace Analysis
{
    /// <summary>
    /// This static class provides useful statistical functions
    /// such as mean, standard deviation, variance, trimmed mean,
    /// and bootstrapping.
    /// </summary>
    public static class Statistics
    {
        public static double Mean(double[] values)
        {
            double total = 0.0;
            for (int i = 1; i < values.Length; i++) total += values[i];
            return total /= values.Length;
        }

        public static double Variance(double[] values)
        {
            double v = 0.0;
            double mean = Mean(values);
            for (int i = 1; i < values.Length; i++) v += Math.Pow(values[i] - mean, 2);

            v /= values.Length - 1;

            return v;
        }

        public static double StandardDeviation(double[] values)
        {
            return Math.Sqrt(Variance(values));
        }

        public static double TrimmedMean(double[] values, double trimLevel)
        {
            List<double> temp = new List<double>(values);
            int length = temp.Count;
            int trim = (int)Math.Floor(trimLevel * length);
            temp.Sort();

            for (int i = 0; i < trim; i++) temp.RemoveAt(temp.Count - 1); // removes the largest values one by one
            for (int i = 0; i < trim; i++) temp.RemoveAt(0); // removes the smallest values one by one

            return Mean(temp.ToArray());
        }

        public static double[] BootstrapReplicate(double[] values)
        {
            Random r = new Random();
            double[] replicate = new double[values.Length];
            for (int i = 0; i < values.Length; i++) replicate[i] = values[r.Next(values.Length)];
            return replicate;
        }

        public static double[] BootstrappedTrimmedMeanAndError(double[] values, double trimLevel, int numReplicates)
        {
            double[] trimmedMeans = new double[numReplicates];
            for (int i = 1; i < numReplicates; i++) trimmedMeans[i] = TrimmedMean(BootstrapReplicate(values), trimLevel);

            double[] tME = new double[2];
            tME[0] = Mean(trimmedMeans);
            tME[1] = StandardDeviation(trimmedMeans);

            return tME;
        }

        public static double[] WeightedMeanAndError(double[] values, double[] errors)
        {
            double wMean = 0.0;
            double wVar = 0.0;

            for (int i = 0; i < errors.Length; i++) wVar += Math.Pow(errors[i], -2);
            wVar = Math.Pow(wVar, -1);

            for (int i = 0; i < values.Length; i++) wMean += values[i] * Math.Pow(errors[i], -2);
            wMean *= wVar;

            return new double[2] { wMean, Math.Sqrt(wVar) };
        }
    }
}
