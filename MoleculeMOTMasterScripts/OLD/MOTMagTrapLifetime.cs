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
        Parameters["TCLBlockDuration"] = 8000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;

        Parameters["MOTSwitchOffTime"] = 7500;
        Parameters["magTrapLifetime"] = 35000;

        // Camera
        //Parameters["Frame0Trigger"] = 8000;
        Parameters["Frame0TriggerDuration"] = 10;

        // Slowing
        Parameters["slowingAOMOnStart"] = 250;
        Parameters["slowingAOMOnDuration"] = 48000;
        Parameters["slowingAOMOffStart"] = 1500;
        Parameters["slowingAOMOffDuration"] = 45000;
        Parameters["slowingRepumpAOMOnStart"] = 250;
        Parameters["slowingRepumpAOMOnDuration"] = 48000;
        Parameters["slowingRepumpAOMOffStart"] = 1500;
        Parameters["slowingRepumpAOMOffDuration"] = 45000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 340;
        Parameters["SlowingChirpDuration"] = 1160;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = -1.2;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsSwitchOff"] = 45000;
        Parameters["MOTCoilsRampActive"] = false;
        Parameters["MOTCoilsCurrentRampStartTime"] = 6000;
        Parameters["MOTCoilsCurrentRampDuration"] = 2000;
        Parameters["MOTCoilsCurrentRampStartValue"] = 0.75;
        Parameters["MOTCoilsCurrentRampEndValue"] = 1.2;

        // v0 Light Switch
        //Parameters["MOTAOMStartTime"] = 8000;
        //Parameters["MOTAOMDuration"] = Parameters["ExpansionTime"];

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5000;
        Parameters["v0IntensityRampDuration"] = 2000;
        Parameters["v0IntensityRampStartValue"] = 5.0;
        Parameters["v0IntensityRampEndValue"] = 8.75;

        //ramp extension
        //Parameters["v0IntensityRampDuration2"] = 500;
        //Parameters["v0IntensityRampEndValue2"] = 9.59;


        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyNewValue"] = 0.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)

        //v0aomCalibrationValues
        Parameters["lockAomFrequency"] = 114.1;
        Parameters["calibOffset"] = 64.2129;
        Parameters["calibGradient"] = 5.55075;
       
        
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.          

        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTSwitchOffTime"], (int)Parameters["magTrapLifetime"], "motAOM"); //pulse off the MOT light to release the cloud
        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTCoilsSwitchOn"], (int)Parameters["MOTCoilsSwitchOff"] - (int)Parameters["MOTCoilsSwitchOn"], "bTrigger"); //// B Field pulse for the BOP (top MOT coil) - bottom coil is in analog section
        p.Pulse(patternStartBeforeQ, (int)Parameters["MOTSwitchOffTime"] + (int)Parameters["magTrapLifetime"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        // Add Analog Channels
        
        p.AddChannel("v0IntensityRamp");
        p.AddChannel("v0FrequencyRamp");


        // B Field
        // For the delta electronica box (bottom MOT coil) - top coil is in digital section
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOn"], (double)Parameters["MOTCoilsCurrentRampStartValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTCoilsSwitchOff"], 0);

        // v0 Intensity Ramp
        p.AddAnalogValue("v0IntensityRamp", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v0IntensityRamp", (int)Parameters["v0IntensityRampStartTime"], (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        //ramp extension below
       // p.AddLinearRamp("v0IntensityRamp", (int)Parameters["v0IntensityRampStartTime"] + (int)Parameters["v0IntensityRampDuration"]+1, (int)Parameters["v0IntensityRampDuration2"], (double)Parameters["v0IntensityRampEndValue2"]);
        p.AddAnalogValue("v0IntensityRamp", (int)Parameters["MOTSwitchOffTime"], (double)Parameters["v0IntensityRampStartValue"]);
        //(int)Parameters["MOTAOMStartTime"]

        // v0 Frequency Ramp
        //p.AddAnalogValue("v0FrequencyRamp", 0, (double)Parameters["v0FrequencyRampStartValue"]);
        //p.AddAnalogValue("v0FrequencyRamp", (int)Parameters["MOTSwitchOffTime"], (double)Parameters["v0FrequencyRampEndValue"]); //jump aom frequency back to normal for imaging 

        // v0 Frequency Ramp
        p.AddAnalogValue("v0FrequencyRamp", 0, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyStartValue"] / 2 - (double)Parameters["calibOffset"])/(double)Parameters["calibGradient"]) ;
        p.AddAnalogValue("v0FrequencyRamp", 4800, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyNewValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v0FrequencyRamp", (int)Parameters["MOTSwitchOffTime"], ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyStartValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]); //jump aom frequency back to normal for imaging 



        p.SwitchAllOffAtEndOfPattern();
        return p;
   }

}
