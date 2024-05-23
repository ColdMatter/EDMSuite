using MOTMaster;
using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


public class Patterns : MOTMasterScript
/*
 * This is a test script
 * */
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 50000;
        Parameters["TCLBlockStart"] = 1000;
        Parameters["FlashToQ"] = 16;
        Parameters["QSwitchPulseDuration"] = 10;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        p.Pulse(0, 0, 10, "aoTrigger");
        p.Pulse(1000, 0, 1000, "cameraTrigger");
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("testChannel");
        p.AddAnalogValue("testChannel", 1000, 1.0);
        p.AddAnalogValue("testChannel", 30000, 2.0);
        return p;
    }

    /*
    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }*/
    
}
