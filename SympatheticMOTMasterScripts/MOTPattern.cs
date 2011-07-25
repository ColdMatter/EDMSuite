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
        Parameters["MOTLoadDuration"] = 800;
        Parameters["PatternLength"] = 900;
        Parameters["numberOfFrames"] = 3;
        Parameters["CameraTrigger1"] = 400;
        Parameters["CameraTrigger2"] = 796;
        Parameters["CameraTrigger3"] = 800;
        Parameters["MOTCoilsCurrent"] = 15.0;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);  // This is how you load "preset" patterns.

        p.Pulse(0, 0, 1, "AnalogPatternTrigger");  //NEVER CHANGE THIS!!!! IT TRIGGERS THE ANALOG PATTERN!
        
        p.Pulse((int)Parameters["CameraTrigger1"], 0, 1, "CameraTrigger");
        p.Pulse((int)Parameters["CameraTrigger2"], 0, 1, "CameraTrigger");
        p.Pulse((int)Parameters["CameraTrigger3"], 0, 1, "CameraTrigger");

        // AddEdge[int channel, int time, bool values] 

        //Pulse(int startTime, int delay (Don't know what that's for), int duration, int channel )

        //DownPulse(int startTime, int delay, int duration, int channel )
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);



        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);


        p.SwitchAllOffAtEndOfPattern();
        return p;
    }

}
