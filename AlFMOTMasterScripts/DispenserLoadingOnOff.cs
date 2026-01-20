using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;
using System.Threading;
using DAQ.Environment;

// This script is supposed to be the basic script for loading a molecule MOT.
// Note that times are all in units of the clock periods of the two pattern generator boards (at present, both are 10us).
// All times are relative to the Q switch, though note that this is not the first event in the pattern.
public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 500000; // 5s
        Parameters["Void"] = 0;
        Parameters["Delay"] = 0;
        Parameters["OnFrequency"] = 327.466161;
        Parameters["OffFrequency"] = 327.466111;
        Parameters["Laser"] = "VECSEL2";
        Parameters["WaitTime"] = 5000;
        Parameters["Switch"] = true;

        switchConfiguration = new Dictionary<string, List<object>>
            {
                {"Switch", new List<object>{true, false}}
            };
    }

    private void prePatternSetup()
    {
        EnvironsHelper eHelper = new EnvironsHelper((String)System.Environment.GetEnvironmentVariables()["COMPUTERNAME"]);
        WavemeterLock.Controller wmlController = (WavemeterLock.Controller)(Activator.GetObject(typeof(WavemeterLock.Controller), "tcp://" + (String)System.Environment.GetEnvironmentVariables()["COMPUTERNAME"] + ":" + eHelper.wavemeterLockTCPChannel.ToString() + "/controller.rem"));
        wmlController.setSlaveFrequency((string)Parameters["Laser"], (bool)Parameters["Switch"] ? (double)Parameters["OnFrequency"] : (double)Parameters["OffFrequency"]);
        Thread.Sleep((int)Parameters["WaitTime"]);
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        prePatternSetup();

        PatternBuilder32 p = new PatternBuilder32();

        //MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters); // This is how you load "preset" patterns.          
        //   p.AddEdge("v00Shutter", 0, true);
        //p.Pulse(patternStartBeforeQ, 3000 - 1400, 10000, "bXSlowingShutter"); //Takes 14ms to start closing

        //p.Pulse(patternStartBeforeQ, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame

        //p.AddEdge("bXSlowingShutter", patternStartBeforeQ + (int)Parameters["slowingAOMOnStart"] + (int)Parameters["slowingAOMOffStart"] - 1650, true);
        //p.AddEdge("bXSlowingShutter", patternStartBeforeQ + (int)Parameters["slowingAOMOffStart"] + (int)Parameters["slowingAOMOffDuration"], false);
        p.AddEdge("q", Convert.ToInt32(Parameters["Delay"]), true);
        p.AddEdge("q", 99999, false);

        if ((bool)Parameters["Switch"])
        {
            p.AddEdge("VECSEL2_Shutter", 0, true);
            p.AddEdge("VECSEL2_Shutter", 1, false);
        }
        else
        {
            p.AddEdge("VECSEL2_Shutter", Convert.ToInt32(Parameters["PatternLength"])-1, true);
        }


        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("AOM1_VCA");
        p.AddAnalogValue("AOM1_VCA", 0, 0);
        //p.AddAnalogValue("VECSEL2_PZO", 0, 2);

        return p;
   }

}
