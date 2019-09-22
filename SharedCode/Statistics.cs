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
            double[] squaredValues = new double[values.Length];
            for (int i = 1; i < values.Length; i++) squaredValues[i] = values[i] * values[i];

            double squaredMean = Mean(squaredValues);
            double meanSquared = Mean(values) * Mean(values);

            return squaredMean - meanSquared;
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

            for (int i = 0; i < trim; i++) temp.RemoveAt(length - 1); // removes the largest values one by one
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
            for (int i = 1; i < numReplicates; i++) trimmedMeans[i] = Mean(BootstrapReplicate(values));

            double[] tME = new double[2];
            tME[0] = Mean(trimmedMeans);
            tME[1] = StandardDeviation(trimmedMeans);

            return tME;
        }
    }
}
