using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


//This is a release and recapture script that can be used to measure the temperature of the MOT. It switches on the MOT,
// takes an image, switches off the coils to release the MOT, switches them back on to recapture the MOT and then 
//takes another image of the cloud. Note this method requires light levels to be the same for all images taken   
public class Patterns : MOTMasterScript
{
   

    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["startValue"] = 0.5;
        Parameters["RampLength"] = 100;
        Parameters["FinalValue"] = 4.0;
        Parameters["PatternLength"] = 5004;
        Parameters["NumberOfFrames"] = 1;
        Parameters["Hold"] = 500;
        Parameters["AILow"] = 0.0;
        Parameters["AIHigh"] = 10.0;
        Parameters["AISampleRate"] = 1000;
        Parameters["AISamples"] = 100;
    }

    public override Dictionary<string, PatternBuilder32> GetDigitalPatterns()
    {
        Dictionary<string, PatternBuilder32> p = new Dictionary<string,PatternBuilder32>();

        p["PXI"] = new PatternBuilder32();

        p["PCI"] = new PatternBuilder32();

        p["PCI"].AddEdge("PCIDOTest", 0, true);

        p["PXI"].AddEdge("AnalogPatternTrigger", 0, false);

        p["PXI"].Pulse(1, 0, 100, "AnalogPatternTrigger");  //NEVER CHANGE THIS!!!! IT TRIGGERS THE ANALOG PATTERN!

        p["PXI"].AddEdge("aommode", 0, true);

        p["PXI"].AddEdge("aommode",3 , false);

       // p["PCI"].AddEdge("PCIDOTest", 0, true);

        p["PCI"].AddEdge("PCIDOTest", 3, false);

        p["PCI"].Pulse(5, 0, 5, "PCIDOTest");

        p["PXI"].Pulse(5, 0, 5, "aommode");

        p["PCI"].Pulse(12, 0, 3, "PCIDOTest");

        p["PXI"].Pulse(12, 0, 3, "aommode");

        p["PXI"].AddEdge("camtrig", 0, false);

        p["PXI"].AddEdge("camtrig", 100, true);

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        p.AddChannel("coolsetpt");

        p.AddAnalogValue("coolsetpt", 1, (double)Parameters["startValue"]);

        p.AddLinearRamp("coolsetpt", 2, (int)Parameters["RampLength"],(double)Parameters["FinalValue"]);

        p.AddLinearRamp("coolsetpt", 2+(int)Parameters["RampLength"] + (int)Parameters["Hold"], (int)Parameters["RampLength"], (double)Parameters["startValue"]);

       // p.SwitchAllOffAtEndOfPattern();
        return p;
    }

    public override MMAIConfiguration GetAIConfiguration()
    {

        MMAIConfiguration p = new MMAIConfiguration();

        p.AddChannel("multiDAQAI1",(double)Parameters["AILow"], (double)Parameters["AIHigh"]);

        p.AddChannel("multiDAQAI2", (double)Parameters["AILow"], (double)Parameters["AIHigh"]);

        p.SampleRate = (int)Parameters["AISampleRate"];

        p.Samples = (int)Parameters["AISamples"];

        return p;

    }

}
