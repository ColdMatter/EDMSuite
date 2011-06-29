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
        Parameters["MOTLoadTime"] = 8000;
        Parameters["PatternLength"] = 8001;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);

        p.Pulse(0, 0, 1, "AnalogPatternTrigger");  //NEVER CHANGE THIS!!!!
        
        p.Pulse((int)Parameters["MOTLoadTime"] - 10, 0, 10, "CameraTrigger");
        // AddEdge[int channel, int time, bool values] 

        //Pulse(int startTime, int delay (Don't know what that's for), int duration, int channel )

        //DownPulse(int startTime, int delay, int duration, int channel )
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        p.AddChannel("coil0Current");

        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);


        p.SwitchAllOffAtEndOfPattern();
        return p;
    }

}
