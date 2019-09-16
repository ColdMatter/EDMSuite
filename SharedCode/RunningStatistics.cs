using System;
using System.Collections.Generic;
using System.Text;

namespace Analysis
{
    /// <summary>
    /// This class keeps track of the mean and variance of a stream of
    /// numbers. It keeps "running" tallies of these quantities, so its
    /// memory consumption does not increase as more values are added.
    /// 
    /// This implementation is a c# port of the c++ code presented at
    /// http://www.johndcook.com/standard_deviation.html
    /// </summary>
    class RunningStatistics
    {
        private int m_n;
        private double m_oldM, m_newM, m_oldS, m_newS;

        public void Clear()
        {
            m_n = 0;
        }

        public void Push(double x)
        {
            m_n++;

            // See Knuth TAOCP vol 2, 3rd edition, page 232
            if (m_n == 1)
            {
                m_oldM = m_newM = x;
                m_oldS = 0.0;
            }
            else
            {
                m_newM = m_oldM + (x - m_oldM) / m_n;
                m_newS = m_oldS + (x - m_oldM) * (x - m_newM);

                // set up for next iteration
                m_oldM = m_newM;
                m_oldS = m_newS;
            }
        }

        public int Count
        {
            get
            {
                return m_n;
            }
        }

        public double Mean
        {
            get
            {
                return (m_n > 0) ? m_newM : 0.0;
            }
        }

        public double Variance
        {
            get
            {
                return ((m_n > 1) ? m_newS / (m_n - 1) : 0.0);
            }
        }

        public double StandardDeviation
        {
            get
            {
                return Math.Sqrt(Variance);
            }
        }

        public double StandardErrorOfSampleMean
        {
            get
            {
                return StandardDeviation / Math.Sqrt(Count);
            }
        }
    }
}
