using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

// This script is supposed to be the basic script for loading a molecule MOT.
// Note that times are all in units of the clock periods of the two pattern generator boards (at present, both are 10us).
// All times are relative to the Q switch, though note that this is not the first event in the pattern.
public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 50000;
        Parameters["TCLBlockStart"] = 2000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 15000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;

        // Camera
        //Parameters["Frame0Trigger"] = 4000;
        Parameters["Frame0TriggerDuration"] = 10;


        // Slowing
        Parameters["slowingAOMOnStart"] = 250;
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1500;
        Parameters["slowingAOMOffDuration"] = 35000;
        Parameters["slowingRepumpAOMOnStart"] = 250;
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1700;
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 340;
        Parameters["SlowingChirpDuration"] = 1160;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = -1.3;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsSwitchOff"] = 20000;
        Parameters["MOTCoilsRampActive"] = false;
        Parameters["MOTCoilsCurrentRampStartTime"] = 1500;
        Parameters["MOTCoilsCurrentRampDuration"] = 1000;
        Parameters["MOTCoilsCurrentRampStartValue"] = 0.8;
        Parameters["MOTCoilsCurrentRampEndValue"] = 1.2;

        Parameters["MOTBOPCoilsCurrentStartValue"] = 10.0;
        Parameters["MOTBOPCoilsCurrentMolassesValue"] = 0.2;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -3.9;
        Parameters["zShimLoadCurrent"] = 1.9;

        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5000;
        Parameters["v0IntensityRampDuration"] = 2000;
        Parameters["v0IntensityRampStartValue"] = 5.0;
        Parameters["v0IntensityRampEndValue"] = 5.0;

        // v0 Light Frequency
        Parameters["v0FrequencyRampStartTime"] = 10000;
        Parameters["v0FrequencyRampDuration"] = 2000;
        Parameters["v0FrequencyRampStartValue"] = 9.0;
        Parameters["v0FrequencyRampEndValue"] = 9.0; 
       
        //b-x "poke" 
        Parameters["PokeStartTime"] = 7500;
        Parameters["PrePokeTime"] = 20;
        Parameters["PokeDuration"] = 50;
        Parameters["PostPokeTime"] = 50;
        Parameters["OscillationTime"] = 150;
        Parameters["PokeDetuningValue"] = -1.35;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);  // This is how you load "preset" patterns.  
        
        p.Pulse(patternStartBeforeQ, (int)Parameters["PokeStartTime"], (int)Parameters["PokeDuration"], "aom"); //poke pulse to MOT 
        p.AddEdge("aom", patternStartBeforeQ + (int)Parameters["slowingAOMOffStart"] + (int)Parameters["slowingAOMOffDuration"], true); // send slowing aom high and hold it high
        p.Pulse(patternStartBeforeQ, (int)Parameters["PokeStartTime"] - (int)Parameters["PrePokeTime"], (int)Parameters["PrePokeTime"] + (int)Parameters["PokeDuration"] + (int)Parameters["PostPokeTime"], "motAOM"); //mot light off during poke
        p.Pulse(patternStartBeforeQ, (int)Parameters["PokeStartTime"] + (int)Parameters["PokeDuration"] + (int)Parameters["PostPokeTime"] + (int)Parameters["OscillationTime"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);

        // Add Analog Channels
        
        p.AddChannel("v0IntensityRamp");
        p.AddChannel("v0FrequencyRamp");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");


        // B Field
        // For the delta electronica box (bottom MOT coil) - top coil is in digital section
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTCoilsCurrentRampStartValue"]);
        // Turn off single MOT coil for Poke
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["PokeStartTime"] - (int)Parameters["PrePokeTime"], 0);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["PokeStartTime"] + (int)Parameters["PokeDuration"], (double)Parameters["MOTCoilsCurrentRampStartValue"]);

        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOff"], 0);

        // For BOP
        p.AddAnalogValue("MOTBOPCoilsCurrent", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTBOPCoilsCurrentStartValue"]);
        p.AddAnalogValue("MOTBOPCoilsCurrent", (int)Parameters["MOTCoilsSwitchOff"], (double)Parameters["MOTBOPCoilsCurrentMolassesValue"]);


        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v0IntensityRamp", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v0IntensityRamp", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);

        p.AddAnalogValue("v0IntensityRamp", (int)Parameters["PokeStartTime"] + (int)Parameters["PokeDuration"] + (int)Parameters["OscillationTime"], (double)Parameters["v0IntensityRampStartValue"]);

        
        // v0 Frequency Ramp
        p.AddAnalogValue("v0FrequencyRamp", 0, (double)Parameters["v0FrequencyRampStartValue"]);


        
        p.SwitchAllOffAtEndOfPattern();
        return p;
   }

}
