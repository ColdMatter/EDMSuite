using MOTMaster;
using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


public class Patterns : MOTMasterScript
{
    /// IMPORTANT NOTE ABOUT WRITING SCRIPTS: At the moment, THERE IS NO AUTOMATIC TIME ORDERING FOR ANALOG
    /// CHANNELS. IT WILL BUILD A PATTERN FOLLOWING THE ORDER IN WHICH YOU CALL AddAnalogValue / AddLinearRamp!!
    /// ---> Stick to writing out the pattern in the correct time order to avoid weirdo behaviour.
    
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["MOTLoadTime"] = 5;
        Parameters["PatternLength"] = 2000;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        // AddEdge[int channel, int time, bool values] 
        //p.AddEdge("CameraTrigger", 0, true);

        //Pulse(int startTime, int delay (Don't know what that's for), int duration, int channel )
        p.Pulse(0, 0, 1, "CameraTrigger");
        //DownPulse(int startTime, int delay, int duration, int channel )
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        p.AddChannel("cavity");
        p.AddChannel("laser");

        p.AddChannel("aom1amplitude");
        p.AddChannel("aom0frequency");
        p.AddChannel("aom0amplitude");
        p.AddChannel("aom1frequency");
        p.AddChannel("aom2amplitude");
        p.AddChannel("aom2frequency");
        p.AddChannel("aom3amplitude");
        p.AddChannel("aom3frequency");


        //p = loadmot(p, deadtime);
        //AddAnalogValue(string channel, int time, double values)
        p.AddAnalogValue("cavity", 0, 4);
        p.AddAnalogValue("cavity", 1, 2);
        p.AddAnalogValue("cavity", 3, 4);
        p.AddAnalogValue("cavity", 4, 0);
        
        //AddLinearRamp(string channel, int time, int numberOfSteps, double finalValue)
        p.AddLinearRamp("cavity", (int)Parameters["MOTLoadTime"], 5, 1);
        p.AddLinearRamp("cavity", (int)Parameters["MOTLoadTime"] + 5, 3, 0);

        p.AddAnalogPulse("laser", 1, 2, 4, 2);

        p.AddAnalogValue("laser", 5, -2);
        p.AddAnalogValue("laser", 6, 0);

        return p;
    }

}
