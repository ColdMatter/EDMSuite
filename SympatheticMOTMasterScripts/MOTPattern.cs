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
        Parameters["PatternLength"] = 10000;
        Parameters["NumberOfFrames"] = 3;
        Parameters["Frame0Trigger"] = 10;
        Parameters["Frame0Exposure"] =  1;
        Parameters["Frame1Trigger"] = 500;
        Parameters["Frame1Exposure"] = 1;
        Parameters["Frame2Trigger"] = 1000;
        Parameters["Frame2Exposure"] = 1;
        Parameters["MOTCoilsCurrent"] = 15.0;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        MOTMasterScriptSnippet lm = new SHLoadMOT(p, Parameters);  // This is how you load "preset" patterns.

        p.Pulse(0, 0, 1, "AnalogPatternTrigger");  //NEVER CHANGE THIS!!!! IT TRIGGERS THE ANALOG PATTERN!

        p.AddEdge("CameraTrigger", 0, true);
        p.DownPulse((int)Parameters["Frame0Trigger"], 0, (int)Parameters["Frame0Exposure"], "CameraTrigger");
        p.DownPulse((int)Parameters["Frame1Trigger"], 0, (int)Parameters["Frame1Exposure"], "CameraTrigger");
        p.DownPulse((int)Parameters["Frame2Trigger"], 0, (int)Parameters["Frame2Exposure"], "CameraTrigger");

        p.DownPulse(2000, 0, 50, "CameraTrigger");
        p.DownPulse(3000, 0, 50, "CameraTrigger");


        //p.AddEdge("CameraTrigger", 1, false);
        //p.Pulse((int)Parameters["Frame0Trigger"], 0, (int)Parameters["Frame0Exposure"], "CameraTrigger");
        //p.Pulse((int)Parameters["Frame1Trigger"], 0, (int)Parameters["Frame1Exposure"], "CameraTrigger");
        //p.Pulse((int)Parameters["Frame2Trigger"], 0, (int)Parameters["Frame2Exposure"], "CameraTrigger");

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
