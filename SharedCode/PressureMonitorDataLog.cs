using System;
using System.Collections.Generic;
using System.Text;

namespace Data 
{
    public class PressureMonitorDataLog : DataLog
    {
        public int PollPeriod { get; set; }
        public double Pressure { get; set; }

        public PressureMonitorDataLog() { }
        public PressureMonitorDataLog(PressureMonitorDataLog log)
            : base(log.TimeStamp)
        {
            PollPeriod = log.PollPeriod;
            Pressure = log.Pressure;
        }
        public PressureMonitorDataLog(DateTime timeStamp, int lp, double pressure)
            : base(timeStamp)
        {
            PollPeriod = lp;
            Pressure = pressure;
        }

        public override string ToString()
        {
            return base.ToString() + ",\r\n" +
                " \"PollPeriod\" : " + PollPeriod.ToString() + ",\r\n" +
                " \"PressureInMbar\" : " + Pressure.ToString();
        }
    }
}
