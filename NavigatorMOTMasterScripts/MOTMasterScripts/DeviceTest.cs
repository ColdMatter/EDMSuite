using System;
using System.Collections.Generic;


using MOTMaster;
using MOTMaster.SnippetLibrary;

using DAQ.Pattern;
using DAQ.Environment;
using DAQ.Analog;

public class Patterns : MOTMasterScript
{
    
    public Patterns()
    {
        multipleCards = true;
        Parameters = new Dictionary<string, object>();
        Parameters["Duration"] = 0.01;
        Parameters["PatternLength"] = (int)((double)Parameters["Duration"] * 100000);
       
    }
    public override PatternBuilder32 GetDigitalPattern()
    {
        return null;
    }
    public override AnalogPatternBuilder GetAnalogPattern()
    {
        return null;
    }
    public override List<PatternBuilder32> GetDigitalPatterns()
    {
        List<PatternBuilder32> hsList = new List<PatternBuilder32>();
        HSDIOPatternBuilder hs = new HSDIOPatternBuilder();
        //Pulse two channels for 1ms, separated by 0.5ms
        hs.Pulse(0, 0, (int)(0.001 * 25000000), "motTTL");
        hs.Pulse((int)(0.0005 * 25000000), 0, (int)(0.001 * 25000000), "ramanTTL");
        //Pulse a 20us pulse on the third channel
        hs.Pulse((int)(0.0005 * 25000000), 0, (int)(0.00002 * 25000000), "mphiTTL");
        hsList.Add(hs);
        return hsList;
    }
    public override List<AnalogPatternBuilder> GetAnalogPatterns()
    {
        List<AnalogPatternBuilder> p = new List<AnalogPatternBuilder>();
        AnalogPatternBuilder apg = new AnalogPatternBuilder((int)((double)Parameters["Duration"] * 1000000));
        p.Add(apg);

        apg.AddChannel("motCTRL");
        apg.AddChannel("ramanCTRL");

        apg.AddLinearRamp("motCTRL", 0, (int)(0.001 * 100000), 5.0);
        apg.AddAnalogPulse("ramanCTRL", (int)(0.0005 * 100000), (int)(0.001 * 100000), 0.0, 5.0);

       
        return p;
    }
}
