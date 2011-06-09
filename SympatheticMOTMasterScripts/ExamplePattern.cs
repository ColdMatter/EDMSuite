using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;



public class Patterns : MOTMasterScript
{
    
    
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["MOTLoadTime"] = 5;
        
        Parameters["PatternLength"] = 25;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);

        // AddEdge[int channel, int time, bool values] 
        //p.AddEdge("CameraTrigger", 0, true);

        //Pulse(int startTime, int delay (Don't know what that's for), int duration, int channel )
        //p.Pulse(2, 0, 1, "CameraTrigger");
        
        //DownPulse(int startTime, int delay, int duration, int channel )
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        p.AddChannel("laser");
        //p.AddAnalogPulse("laser", 1, 2, 4, 2);
        //p.AddAnalogValue("laser", 5, -2);
        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);

        p.AddChannel("cavity");
        p.AddAnalogValue("cavity", 0, 4);
        p.AddAnalogValue("cavity", 1, 2);
        p.AddAnalogValue("cavity", 3, 4);
        p.AddAnalogValue("cavity", 4, 0);
        p.AddLinearRamp("cavity", (int)Parameters["MOTLoadTime"], 5, 1);


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
       
        //p.AddLinearRamp("cavity", (int)Parameters["MOTLoadTime"] + 12, 3, 0);
       
        

        
        //AddLinearRamp(string channel, int time, int numberOfSteps, double finalValue)

    //    p.AddAnalogValue("cavity", 15, 5);
        //p.AddAnalogValue("cavity", 16, 0);


        p.SwitchAllOffAtEndOfPattern();
        return p;
    }

}
