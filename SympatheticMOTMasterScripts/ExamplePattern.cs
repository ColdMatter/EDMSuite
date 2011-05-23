using MOTMaster;
using System;

using DAQ.Pattern;
using DAQ.Analog;


public class Patterns : MOTMasterScript
{
    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        // AddEdge[ channel, time, value] 
        p.AddEdge(1, 0, true);
        p.AddEdge(1, 1, false);
        //p.AddEdge(1, 2, true);
        //p.AddEdge(1, 3, false);
        //p.AddEdge(1, 4, true);
        //p.AddEdge(1, 5, false);
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder(2000);

        p.AddChannel("cavity");
        p.AddChannel("laser");

        //		p.AddAnalogValue("cavity", 0, 1);
        //		p.AddAnalogValue("cavity",2, 0);
        //		

        p.AddAnalogValue("cavity", 0, 4);
        p.AddAnalogValue("cavity", 1, 2);
        p.AddAnalogValue("cavity", 3, 4);
        p.AddAnalogValue("cavity", 4, 0);

        p.AddLinearRamp("cavity", 5, 5, 1);
        p.AddLinearRamp("cavity", 11, 3, 0);

        p.AddAnalogValue("laser", 1, 4);
        p.AddAnalogValue("laser", 4, 2);
        p.AddAnalogValue("laser", 5, -2);
        p.AddAnalogValue("laser", 6, 0);

        return p;
    }

}
