using System;
using System.Collections.Generic;
using System.Text;

using Data;

namespace Analysis
{
    /// <summary>
    /// This class  takes the average of a number of TOF curves. It keeps track of
    /// the mean and standard error for each point on the TOF. It keeps a running average
    /// and variance, so adding a TOF does not increase the memory usage.
    /// </summary>
    public class TOFAccumulator : IAccumulator<TOFWithError>
    {
        private RunningStatistics[] stats;
        private int gateStartTime;
        private int clockPeriod;
        private double calibration;
        private bool initialised = false;

        // Note well that the incoming errors are ignored by the accumulator. The reason it
        // doesn't take a simple TOF is to keep the code structure more uniform (in
        // particular it allows this class to implement IAccumulator).
        public void Add(TOFWithError t)
        {
            if (!initialised)
            {
                // the first TOF to be added defines the parameters
                gateStartTime = t.GateStartTime;
                clockPeriod = t.ClockPeriod;
                calibration = t.Calibration;
                stats = new RunningStatistics[t.Length];
                for (int i = 0; i < Length; i++) stats[i] = new RunningStatistics();
                initialised = true;
            }

            // add the TOF data - very minimal error checking: just check the lengths
            if (t.Length == Length) for (int i = 0; i < Length; i++) stats[i].Push(t.Data[i]);
            else throw new TOFAccumulatorException();
        }

        public TOFWithError GetResult()
        {
                TOFWithError temp =  new TOFWithError();
                temp.Calibration = calibration;
                temp.ClockPeriod = clockPeriod;
                temp.GateStartTime = gateStartTime;
                temp.Data = Data;
                temp.Errors = Errors;
                return temp;
        }

        private int Length
        {
            get
            {
                return stats.Length;
            }
        }

        private double[] Data
        {
            get
            {
                double[] temp = new double[Length];
                for (int i = 0; i < Length; i++) temp[i] = stats[i].Mean;
                return temp;
            }
        }

        private double[] Errors
        {
            get
            {
                double[] temp = new double[Length];
                for (int i = 0; i < Length; i++) temp[i] = stats[i].StandardDeviation;
                return temp;
            }
        }

        public class TOFAccumulatorException : Exception {};

    }
}
