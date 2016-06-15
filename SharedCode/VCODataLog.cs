using System;
using System.Collections.Generic;
using System.Text;

namespace Data 
{
    public class VCODataLog : DataLog
    {
        public double CurrentFrequency { get; set; }
        public double SetPoint { get; set; }
        public double Error { get; set; }
        public double ProportionalTerm { get; set; }
        public double IntegralTerm { get; set; }
        public double VoltageOut { get; set; }

        public VCODataLog() { }
        public VCODataLog(VCODataLog log)
            : base(log.TimeStamp)
        {
            CurrentFrequency = log.CurrentFrequency;
            SetPoint = log.SetPoint;
            Error = log.Error;
            ProportionalTerm = log.ProportionalTerm;
            IntegralTerm = log.IntegralTerm;
            VoltageOut = log.VoltageOut;
        }
        public VCODataLog(DateTime timeStamp, double cf, double sp, double p, double i, double vo)
            : base(timeStamp)
        {
            CurrentFrequency = cf;
            SetPoint = sp;
            Error = cf - sp;
            ProportionalTerm = p;
            IntegralTerm = i;
            VoltageOut = vo;
        }

        public override string ToString()
        {
            return base.ToString() + ",\r\n" +
                " \"CurrentFrequency\" : " + CurrentFrequency.ToString() + ",\r\n" +
                " \"SetPoint\" : " + SetPoint.ToString() + ",\r\n" +
                " \"Error\" : " + Error.ToString() + ",\r\n" +
                " \"ProportionalTerm\" : " + ProportionalTerm.ToString() + ",\r\n" +
                " \"IntegralTerm\" : " + IntegralTerm.ToString() + ",\r\n" +
                " \"VoltageOut\" : " + VoltageOut.ToString();
        }
    }
}
