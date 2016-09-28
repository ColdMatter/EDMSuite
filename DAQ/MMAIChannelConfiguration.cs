using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.Analog
{
    public class MMAIChannelConfiguration
    {
        private double AILow;
        private double AIHigh;

        public MMAIChannelConfiguration(double aiLow, double aiHigh)
        {
            AILow = aiLow;
            AIHigh = aiHigh;
        }

        public Double AIRangeLow
        {
            get
            {
                return AILow;
            }
            set
            {
                AILow = value;
            }
        }

        public Double AIRangeHigh
        {
            get
            {
                return AIHigh;
            }
            set
            {
                AIHigh = value;
            }
        }
    }
}
