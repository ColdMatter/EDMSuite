using System;
using System.Collections.Generic;
using System.Text;

using Analysis;

namespace Analysis.EDM
{
    public class TOFChannelAccumulator : Channel<TOFAccumulator>, IAccumulator<TOFChannel>
    {
        public TOFChannelAccumulator()
        {
            On = new TOFAccumulator();
            Off = new TOFAccumulator();
        }

        public void Add(TOFChannel val)
        {
            On.Add(val.On);
            Off.Add(val.Off);
        }

        public TOFChannel GetResult()
        {
            TOFChannel tc = new TOFChannel();
            tc.On = On.GetResult();
            tc.Off = Off.GetResult();
            return tc;
        }
    }
}
