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
        public double VoltageOut { get; set; }

        public TCLDataLog() { }
        public TCLDataLog(TCLDataLog log) : base(log.TimeStamp)
        {
            SlaveName = log.SlaveName;
            MasterPosition = log.MasterPosition;
            SlavePosition = log.SlavePosition;
            VoltageOut = log.VoltageOut;
        }
        public TCLDataLog(DateTime timeStamp, string sn, double mp, double sp, double vo) : base(timeStamp)
        {
            SlaveName = sn;
            MasterPosition = mp;
            SlavePosition = sp;
            VoltageOut = vo;
        }

        public override string ToString()
        {
            return base.ToString() + ",\r\n" +
                " \"SlaveName\" : " + SlaveName + ",\r\n" +
                " \"MasterPosition\" : " + MasterPosition.ToString() + ",\r\n" +
                " \"SlavePosition\" : " + SlavePosition.ToString() + ",\r\n" +
                " \"VoltageOut\" : " + VoltageOut.ToString();
        }
    }
}
