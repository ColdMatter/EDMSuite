using System;
using System.Collections.Generic;
using DAQ.Pattern;
using DAQ.Analog;
using MOTMaster;
using MOTMaster.SnippetLibrary;

namespace MOTMaster.SnippetLibrary.CaF
{
    // Fire the YAG, slow the molecules and load the MOT.
    // Starts the shot: the Q-switch is at t = 0 (TCLBlockStart into the digital pattern).
    // Reads bXSlowingShutterDuration, which scripts work out in their timeline.
    public class LoadMOT : MOTMasterScriptSnippet
    {
        public LoadMOT(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            AddDigitalSnippet(p, parameters);
        }
        public LoadMOT(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddAnalogSnippet(p, parameters);
        }
        public LoadMOT(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
            AddDDSSnippet(p, parameters);
        }

        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> parameters)
        {
            int patternStartBeforeQ = (int)parameters["TCLBlockStart"];
            int slowingEnd = (int)parameters["SlowingChirpStartTime"] + (int)parameters["SlowingChirpDuration"];

            new LoadMoleculeMOTNoSlowingEdge(p, parameters);
            p.Pulse(patternStartBeforeQ, 0, (int)parameters["QSwitchPulseDuration"], "DDS_Analog_Trg");

            // Time of flight PMT trigger
            p.Pulse(patternStartBeforeQ, 2000, 10, "tofTrigger");

            // Slowing AOMs
            p.Pulse(patternStartBeforeQ, (int)parameters["SlowingChirpStartTime"], (2 * (int)parameters["SlowingChirpDuration"]) + 20000, "bXLockBlock"); // Want it to be blocked for whole time that bX laser is moved
            p.Pulse(patternStartBeforeQ, (int)parameters["SlowingChirpStartTime"] - 100, (int)parameters["SlowingChirpDuration"] + 100, "bXSlowingAOM");
            p.Pulse(patternStartBeforeQ, (int)parameters["slowingRepumpAOMOnStart"], slowingEnd - (int)parameters["slowingRepumpAOMOnStart"], "v10SlowingAOM");

            // BX Shutter
            p.Pulse(patternStartBeforeQ, slowingEnd + 200, (int)parameters["bXSlowingShutterDuration"], "bXSlowingShutter");
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> parameters)
        {
            int slowingEnd = (int)parameters["SlowingChirpStartTime"] + (int)parameters["SlowingChirpDuration"];

            new LoadMoleculeMOTNoSlowingEdge(p, parameters);

            p.AddChannel("TCoolSidebandVCO");
            p.AddChannel("BXAttenuation");
            p.AddChannel("xShimCoilCurrent");
            p.AddChannel("yShimCoilCurrent");
            p.AddChannel("zShimCoilCurrent");
            p.AddChannel("SlowingRepumpAttenuation");
            p.AddChannel("BXAOM1att");
            p.AddChannel("BXAOM2att");
            p.AddChannel("ODT90att");
            p.AddChannel("ODT70att");

            // Shim Fields
            p.AddAnalogValue("xShimCoilCurrent", 0, (double)parameters["xShimLoadCurrent"]);
            p.AddAnalogValue("yShimCoilCurrent", 0, (double)parameters["yShimLoadCurrent"]);
            p.AddAnalogValue("zShimCoilCurrent", 0, (double)parameters["zShimLoadCurrent"]);

            // Slowing
            p.AddAnalogValue("BXAOM1att", 0, (double)parameters["BXAOM1att"]);
            p.AddAnalogValue("BXAOM2att", 0, (double)parameters["BXAOM2att"]);
            p.AddAnalogValue("BXAOM1att", slowingEnd, 10.0);
            p.AddAnalogValue("BXAOM2att", slowingEnd, 10.0);
            p.AddAnalogValue("SlowingRepumpAttenuation", 0, (double)parameters["SlowingRepumpAttenuation"]);

            // Slowing B field
            p.AddAnalogValue("slowingCoilsCurrent", 0, (double)parameters["slowingCoilsValue"]);
            p.AddAnalogValue("slowingCoilsCurrent", slowingEnd, 0.0);

            // TCOOL
            p.AddAnalogValue("TCoolSidebandVCO", 0, (double)parameters["TCoolSidebandVCOValue"]);

            // MOT B field
            p.AddAnalogValue("MOTCoilsCurrent", 0, (double)parameters["MOTCoilsCurrentValue"]);
        }

        public void AddDDSSnippet(DDSPatternBuilder p, Dictionary<String, Object> parameters)
        {
            p.AddEvent("MOT", 0, CaFDDS.MOTFrequencies(parameters), CaFDDS.MOTAmplitudes(parameters));
        }
    }
}
