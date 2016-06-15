using System;
using System.Collections.Generic;
using System.Text;

namespace Data
{
    public class DataLog
    {
        public DataLog() { }
        public DataLog(DataLog log)
        {
            TimeStamp = log.TimeStamp;
        }
        public DataLog(DateTime timeStamp)
        {
            TimeStamp = timeStamp;
        }
        public DateTime TimeStamp { get; set; }
        public override string ToString()
        {
            return " \"TimeStamp\" : " + "\"" + TimeStamp.ToString("o") + "\"";
        }
    }
}
