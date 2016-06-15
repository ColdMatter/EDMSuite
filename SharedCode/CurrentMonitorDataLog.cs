using System;
using System.Collections.Generic;
using System.Text;

namespace Data 
{
    public class CurrentMonitorDataLog : DataLog
    {
        public int PollPeriod { get; set; }
        public double NorthVoltToFreqSlope { get; set; }
        public double SouthVoltToFreqSlope { get; set; }
        public double FreqToCurrentSlope { get; set; }
        public double NorthOffset { get; set; }
        public double SouthOffset { get; set; }
        public double NorthCurrent { get; set; }
        public double SouthCurrent { get; set; }

        public CurrentMonitorDataLog() { }
        public CurrentMonitorDataLog(CurrentMonitorDataLog log)
            : base(log.TimeStamp)
        {
            PollPeriod = log.PollPeriod;
            NorthVoltToFreqSlope = log.NorthVoltToFreqSlope;
            SouthVoltToFreqSlope = log.SouthVoltToFreqSlope;
            FreqToCurrentSlope = log.FreqToCurrentSlope;
            NorthOffset = log.NorthOffset;
            SouthOffset = log.SouthOffset;
            NorthCurrent = log.NorthCurrent;
            SouthCurrent = log.SouthCurrent;
        }
        public CurrentMonitorDataLog(DateTime timeStamp, int lp, double nv2fs, double sv2fs, double f2cs, double no, double so, double nc, double sc)
            : base(timeStamp)
        {
            PollPeriod = lp;
            NorthVoltToFreqSlope = nv2fs;
            SouthVoltToFreqSlope = sv2fs;
            FreqToCurrentSlope = f2cs;
            NorthOffset = no;
            SouthOffset = so;
            NorthCurrent = nc;
            SouthCurrent = sc;
        }

        public override string ToString()
        {
            return base.ToString() + ",\r\n" +
                " \"PollPeriod\" : " + PollPeriod.ToString() + ",\r\n" +
                " \"NorthVoltToFreqSlope\" : " + NorthVoltToFreqSlope.ToString() + ",\r\n" +
                " \"SouthVoltToFreqSlope\" : " + SouthVoltToFreqSlope.ToString() + ",\r\n" +
                " \"FreqToCurrentSlope\" : " + FreqToCurrentSlope.ToString() + ",\r\n" +
                " \"NorthOffset\" : " + NorthOffset.ToString() + ",\r\n" +
                " \"SouthOffset\" : " + SouthOffset.ToString() + ",\r\n" +
                " \"NorthCurrent\" : " + NorthCurrent.ToString() + ",\r\n" +
                " \"SouthCurrent\" : " + SouthCurrent.ToString();
        }
    }
}
