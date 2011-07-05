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
        Parameters["PatternLength"] = 801;
        Parameters["CameraTriggerTime"] = Parameters["MOTLoadDuration"]; //Note: Camera triggers on the falling edge of the pulse.
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);

        p.Pulse(0, 0, 1, "AnalogPatternTrigger");  //NEVER CHANGE THIS!!!! IT TRIGGERS THE ANALOG PATTERN!
        
        p.Pulse((int)Parameters["CameraTriggerTime"] - 1, 0, 1, "CameraTrigger");
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
