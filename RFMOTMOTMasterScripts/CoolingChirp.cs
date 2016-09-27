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
        Parameters["FinalValue"] = 1.1;
        Parameters["PatternLength"] = 50;
        Parameters["NumberOfFrames"] = 1;
        
        Parameters["AILow"] = -5.0;
        Parameters["AIHigh"] = 5.0;
        Parameters["AISampleRate"] = 100000;
        Parameters["AISamples"] = 1000;
    }

    public override Dictionary<string, PatternBuilder32> GetDigitalPatterns()
    {
        Dictionary<string, PatternBuilder32> p = new Dictionary<string,PatternBuilder32>();

        p["PXI"] = new PatternBuilder32();

        p["PCI"] = new PatternBuilder32();

        p["PXI"].AddEdge("AnalogPatternTrigger", 0, false);

        p["PXI"].AddEdge("AnalogPatternTrigger", 1, true);

        p["PCI"].AddEdge("PCIDOTest", 0, true);

        p["PXI"].AddEdge("camtrig", 0, false);

        p["PXI"].Pulse(2, 0, 1, "camtrig");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        p.AddChannel("coolsetpt");

        p.AddAnalogValue("coolsetpt", 0, (double)Parameters["startValue"]);

        p.AddAnalogValue("coolsetpt", 18, (double)Parameters["FinalValue"]);

        p.AddAnalogValue("coolsetpt", 36, (double)Parameters["startValue"]);
       // p.SwitchAllOffAtEndOfPattern();
        return p;
    }

    public override MMAIConfiguration GetAIConfiguration()
    {

        MMAIConfiguration p = new MMAIConfiguration();

        p.AddChannel("coolerrsig",(double)Parameters["AILow"], (double)Parameters["AIHigh"]);
        
        p.SampleRate = (int)Parameters["AISampleRate"];

        p.Samples = (int)Parameters["AISamples"];

        return p;

    }

}
