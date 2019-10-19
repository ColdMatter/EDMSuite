using System;
using System.Collections.Generic;
using System.Text;

using Analysis;

namespace Analysis.EDM
{
    public class TOFChannelAccumulator : Channel<TOFAccumulator>
    {
        public TOFChannelAccumulator()
        {
            On = new TOFAccumulator();
            Off = new TOFAccumulator();
            Difference = new TOFAccumulator();
        }

        public void Add(TOFChannel val)
        {
            On.Add(val.On);
            Off.Add(val.Off);
            Difference.Add(val.Difference);
        }

        public TOFWithErrorChannel GetResult()
        {
            TOFWithErrorChannel tc = new TOFWithErrorChannel();
            tc.On = On.GetResult();
            tc.Off = Off.GetResult();
            tc.Difference = Difference.GetResult();
            return tc;
        }
    }
}
