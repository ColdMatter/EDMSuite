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
        Parameters["PatternLength"] = 100000;
        //Parameters["TCLBlockStart"] = 2000; // This is a time before the Q switch
        //Parameters["TCLBlockDuration"] = 8000;
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 15000;
        
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["RbMOTLoadTime"] = 0;

        //Blue molasses:
        Parameters["MolassesDelay"] = 100;
        Parameters["MolassesHoldTime"] = 500;

        Parameters["Frame0Trigger"] = 4000;
        
        //Recapture to MOT:
        Parameters["WaitBeforeImage"] = 500;

        //Microwaves:
        Parameters["MicrowavePulseDuration"] = 7;
        Parameters["SecondMicrowavePulseDuration"] = 7;

        // Camera
        Parameters["Frame0TriggerDuration"] = 100;

        //
        Parameters["CaFMOTLoadDuration"] = 5000;

        //PMT
        Parameters["PMTTriggerDuration"] = 10;
        Parameters["PMTTrigger"] = 5000;

        // Slowing
        Parameters["slowingAOMOnStart"] = 180; //started from 250
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1520;//started from 1520
        Parameters["slowingAOMOffDuration"] = 40000;
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1520;//1520
        Parameters["slowingRepumpAOMOffDuration"] = 35000;
        Parameters["SlowingChirpHoldDuration"] = 8000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 380;// 380;
        Parameters["SlowingChirpDuration"] = 1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -1.25; //-1.25

        // Slowing field
        Parameters["slowingCoilsValue"] = 0.42; //0.1
        Parameters["slowingCoilsOffTime"] = 1000;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsCurrentRampStartValue"] = 0.625;
        Parameters["MOTCoilsCurrentMolassesValue"] = -0.1;// -0.01; //0.21

        // Shim fields
        Parameters["xShimLoadCurrent"] = -1.35;//-1.35;old values// -1.35 is zero
        Parameters["yShimLoadCurrent"] = -1.92;//2.0;//old value// -1.92 is zero
        Parameters["zShimLoadCurrent"] = -0.22;//-0.22;//old value// -0.22 is zero

        Parameters["xShimLoadCurrentOP"] = -1.35;//Bias field for Optical pumping
        Parameters["yShimLoadCurrentOP"] = -1.92;
        Parameters["zShimLoadCurrentOP"] = -0.22;

        //Shim fields for imaging
        Parameters["xShimImagingCurrent"] = -1.93;// -1.35 is zero
        Parameters["yShimImagingCurrent"] = -6.74;// -1.92 is zero
        Parameters["zShimImagingCurrent"] = -0.56;// -0.22 is zero

        // v0 Light Intensity
        Parameters["v0IntensityRampDuration"] = 300;
        Parameters["MOTHoldTime"] = 1000;
        Parameters["v0IntensityRampStartValue"] = 5.6;//6.7;
        Parameters["v0IntensityRampEndValue"] = 8.4; //7.78;
        Parameters["v0IntensityMolassesValue"] = 5.6;
        Parameters["v0IntensityF0PumpValue"] = 9.3;

        // v0 Light Frequency
        Parameters["v0FrequencyMOTValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyMolassesValue"] = 20.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)
        Parameters["v0FrequencyF0PumpValue"] = 2.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)

        // v0 pumping EOM
        Parameters["v0EOMMOTValue"] = 4.85;
        Parameters["v0EOMPumpValue"] = 1.9;

        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyNewValue"] = 20.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)
        Parameters["v0FrequencyOPValue"] = 2.0;

        //v0aomCalibrationValues
        Parameters["calibGradient"] = 11.4;

        //Blow away:
        Parameters["BlowAwayDuration"] = 300;
        Parameters["PokeDetuningValue"] = -1.47;

        //MQT:
        Parameters["MQTStartDelay"] = 50;
        Parameters["MQTHoldDuration"] =  10000;
        Parameters["MQTBField"] = 0.625;
        Parameters["MQTLowFieldHoldDuration"] = 1600;
        Parameters["MQTFieldRampDuration"] = 1600;

        // CMOT
        Parameters["CMOTFieldValue"] = 1.6;
        Parameters["CMOTRampDuration"] = 1000;
        Parameters["CMOTHoldDuration"] = 1500;

        //Rb light:
        Parameters["ImagingFrequency"] = 1.5;
        Parameters["MOTCoolingLoadingFrequency"] = 4.6;
        Parameters["MOTRepumpLoadingFrequency"] = 6.6;

        //Rb prep for MQT:
        Parameters["MolassesFrequnecyRampDuration"] = 1000;
        Parameters["MolassesEndFrequency"] = 1.5;
        Parameters["RbOPDuration"] = 150;
        Parameters["RbRepumpOPDetuning"] = 8.2;

        Parameters["RbRepumpSwitch"] = 5.0; // 0.0 will keep it on and 10.0 will switch it off

        Parameters["MWDuration"] = 9;
        Parameters["MWDelay"] = 500;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        int cafMOTLoadEndTime = (int)Parameters["TCLBlockStart"] + (int)Parameters["CaFMOTLoadDuration"];
        int V0IntensityRampEndTime = cafMOTLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        //int firstImageTime = cafMOTLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        int cmotStartTime = V0IntensityRampEndTime + (int)Parameters["MOTHoldTime"];
        int motSwitchOffTime = cmotStartTime + (int)Parameters["CMOTHoldDuration"];
        int finalImageTime = cmotStartTime + (int)Parameters["WaitBeforeImage"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        
        p.Pulse(0, finalImageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger

        //Mechanical CaF shutters:
        //p.Pulse(0, molassesStartTime - 1500, motRecaptureTime, "bXSlowingShutter"); //B-X shutter closed after blow away
        //p.Pulse(0, molassesEndTime - 1200, motRecaptureTime - molassesEndTime - 900, "v00MOTShutter"); //V00 shutter closed after optical pumping an opened for recpature into MOT
        //p.Pulse(0, molassesEndTime - 1200, motRecaptureTime - molassesEndTime - 900, "transportTrack"); // BX optical pumping shutter. Currently using the same digital channel as the transport track! 




        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        //MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);
        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);
        //int rbMOTLoadingEndTime = (int)Parameters["RbMOTLoadTime"];
        
        int cafMOTLoadEndTime = (int)Parameters["CaFMOTLoadDuration"];
        int V0IntensityRampEndTime = cafMOTLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        //int firstImageTime = cafMOTLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        int cmotStartTime = V0IntensityRampEndTime + (int)Parameters["MOTHoldTime"];
        int motSwitchOffTime = cmotStartTime + (int)Parameters["CMOTHoldDuration"];
        int finalImageTime = cmotStartTime + (int)Parameters["WaitBeforeImage"];

        // Add Analog Channels
        p.AddChannel("v00Intensity");
        p.AddChannel("v00Frequency");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("v00EOMAmp");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("rb3DCoolingFrequency");
        p.AddChannel("rb3DCoolingAttenuation");
        p.AddChannel("rbRepumpFrequency");
        p.AddChannel("rbRepumpAttenuation");
        p.AddChannel("rbAbsImagingFrequency");


        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", cafMOTLoadEndTime + 1000, 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentRampStartValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", cmotStartTime, (double)Parameters["CMOTFieldValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", motSwitchOffTime, 0.0);
        
        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        
        
        //Shim fields for imaging
        //p.AddAnalogValue("xShimCoilCurrent", rbMQTImageTime - 1000, (double)Parameters["xShimImagingCurrent"]);
        //p.AddAnalogValue("yShimCoilCurrent", rbMQTImageTime - 1000, (double)Parameters["yShimImagingCurrent"]);
        //p.AddAnalogValue("zShimCoilCurrent", rbMQTImageTime - 1000, (double)Parameters["zShimImagingCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", cafMOTLoadEndTime, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", motSwitchOffTime, (double)Parameters["v0IntensityRampStartValue"]);
        

        // v0 EOM
        p.AddAnalogValue("v00EOMAmp", 0, (double)Parameters["v0EOMMOTValue"]);
        
        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, 10.0 - (double)Parameters["v0FrequencyStartValue"] / (double)Parameters["calibGradient"]);
        
        return p;
    }

}
