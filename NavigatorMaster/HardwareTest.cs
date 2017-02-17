using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


namespace NavigatorMaster
{
    public class Patterns : MOTMasterScript
    {
        public Patterns()
        {
            Parameters = new Dictionary<string, object>();
            //This is the length of the analogue pattern. The digital pattern will be much longer since it has a higher clock frequency
            Parameters["PatternLength"] = 100000;
            Parameters["AnalogLength"] = 1000;
        }

        public override HSDIOPatternBuilder GetHSDIOPattern()
        {
            HSDIOPatternBuilder hs = new HSDIOPatternBuilder();

            hs.Pulse(0, 0, 10000, "motTTL");
            hs.Pulse(0, 20000, 10000, "motTTL");
            hs.Pulse(0, 35000, 10000, "motTTL");
            return hs;
        }

        public override AnalogPatternBuilder GetAnalogPattern()
        {
            AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["AnalogLength"]);

            p.AddChannel("analogTest");
            p.AddAnalogValue("analogTest", 20, 1);
            p.AddLinearRamp("analogTest", 30, 10, 2);
            return p;
        }

        public override PatternBuilder32 GetDigitalPattern()
        {
            throw new NotImplementedException();
        }

        public override MMAIConfiguration GetAIConfiguration()
        {
            return null;
        }
    }
}
