using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data
{
    public class TCLDataLog : DataLog
    {
        public string SlaveName { get; set; }
        public double MasterPosition { get; set; }
        public double SlavePosition { get; set; }
        public double LaserSetPoint { get; set; }
        public double VoltageErr { get; set; }
        public double Gain { get; set; }
        public double VoltageOut { get; set; }

        public TCLDataLog() { }
        public TCLDataLog(TCLDataLog log) : base(log.TimeStamp)
        {
            SlaveName = log.SlaveName;
            MasterPosition = log.MasterPosition;
            SlavePosition = log.SlavePosition;
            LaserSetPoint = log.LaserSetPoint;
            VoltageErr = log.VoltageErr;
            Gain = log.Gain;
            VoltageOut = log.VoltageOut;
        }
        public TCLDataLog(DateTime timeStamp, string sn, double mp, double sp, double lsp, double verr, double gain, double vo) : base(timeStamp)
        {
            SlaveName = sn;
            MasterPosition = mp;
            SlavePosition = sp;
            LaserSetPoint = lsp;
            VoltageErr = verr;
            Gain = gain;
            VoltageOut = vo;
        }

        public override string ToString()
        {
            return base.ToString() + ",\r\n" +
                " \"SlaveName\" : " + "\"" + SlaveName.ToString() + "\"" + ",\r\n" +
                " \"MasterPosition\" : " + MasterPosition.ToString() + ",\r\n" +
                " \"SlavePosition\" : " + SlavePosition.ToString() + ",\r\n" +
                " \"LaserSetPoint\" : " + LaserSetPoint.ToString() + ",\r\n" +
                " \"VoltageErr\" : " + VoltageErr.ToString() + ",\r\n" +
                " \"Gain\" : " + Gain.ToString() + ",\r\n" +
                " \"VoltageOut\" : " + VoltageOut.ToString();
        }
    }
}
