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
        Parameters["MolassesDelay"] = 2000;
        Parameters["MolassesHoldTime"] = 500;

        Parameters["Frame0Trigger"] = 4000;
        
        //Recapture to MOT:
        Parameters["MOTWaitBeforeImage"] = 300;

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

        //Optical pumping
        Parameters["OPDuration"] = 40;

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
        Parameters["MOTCoilsCurrentRampStartValue"] = 1.0;
        Parameters["MOTCoilsCurrentMolassesValue"] = -0.05;// -0.01; //0.21

        // Shim fields
        Parameters["xShimLoadCurrent"] = -1.35;//-1.35;old values// -1.35 is zero
        Parameters["yShimLoadCurrent"] = -1.92;//2.0;//old value// -1.92 is zero
        Parameters["zShimLoadCurrent"] = -0.22;//-0.22;//old value// -0.22 is zero

        Parameters["xShimLoadCurrentOP"] = 5.0;//Bias field for Optical pumping
        Parameters["yShimLoadCurrentOP"] = -1.92;
        Parameters["zShimLoadCurrentOP"] = -0.22;

        //Shim fields for imaging
        Parameters["xShimImagingCurrent"] = -1.93;// -1.35 is zero
        Parameters["yShimImagingCurrent"] = -6.74;// -1.92 is zero
        Parameters["zShimImagingCurrent"] = -0.56;// -0.22 is zero

        // v0 Light Intensity
        Parameters["v0IntensityRampDuration"] = 300;
        Parameters["MOTHoldTime"] = 600;//1000;
        Parameters["v0IntensityRampStartValue"] = 6.9;
        Parameters["v0IntensityRampEndValue"] = 7.8; //7.78;
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
        Parameters["MQTBField"] = 1.0;
        Parameters["MQTLowFieldHoldDuration"] = 1600;
        Parameters["MQTFieldRampDuration"] = 1600;

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

        //Ballistic Expansion
        Parameters["FreeExpansionTime"] = 500;
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        //int rbMOTLoadingEndTime = (int)Parameters["TCLBlockStart"] + (int)Parameters["RbMOTLoadTime"];
        int cafMOTLoadEndTime = (int)Parameters["TCLBlockStart"] + (int)Parameters["CaFMOTLoadDuration"];
        //int firstImageTime = cafMOTLoadEndTime - 1000;
        //int firstImageTime = (int)Parameters["TCLBlockStart"];
        int V0IntensityRampStartTime = cafMOTLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        //int firstImageTime = cafMOTLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        int firstImageTime = cafMOTLoadEndTime -1000- (int)Parameters["Frame0TriggerDuration"];
        int motSwitchOffTime = V0IntensityRampStartTime + (int)Parameters["MOTHoldTime"];

        int finalImageTime = motSwitchOffTime + (int)Parameters["FreeExpansionTime"];

        int molassesStartTime = motSwitchOffTime + (int)Parameters["MolassesDelay"];
        int molassesEndTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];
        
        int OPbiasfieldSettleTime = molassesEndTime - 100;

        int mqtStartTime = molassesEndTime + (int)Parameters["OPDuration"];
        int microwavePulseTime = mqtStartTime + (int)Parameters["MWDelay"];

        //int microwavePulseTime = molassesEndTime + (int)Parameters["OPDuration"];
        //int mqtStartTime = microwavePulseTime + (int)Parameters["MWDuration"];
        
        //int motRecaptureTime = mqtStartTime + (int)Parameters["MQTHoldDuration"];
        int motRecaptureTime = mqtStartTime;
        //int finalImageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];
        int rbMQTImageTime = finalImageTime + 1100;
        

        //Dummy Yag shots to cheat the source:
        /*
        for (int t = 0; t < (int)Parameters["RbMOTLoadTime"]; t += 50000)
        {
            p.Pulse((int)Parameters["TCLBlockStart"] + t, -(int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
            p.Pulse((int)Parameters["TCLBlockStart"] + t, 0, (int)Parameters["QSwitchPulseDuration"], "qSwitch"); //trigger the Q switch
        }
        */
        //MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);  // This is how you load "preset" patterns. 
        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

        //Microwave pulse:
        //p.Pulse(0, microwavePulseTime, (int)Parameters["MicrowavePulseDuration"], "microwaveA"); //1st pulse
        //p.Pulse(0, secondMicrowavePulseTime, (int)Parameters["SecondMicrowavePulseDuration"], "microwaveB"); //2nd pulse

        //V00 AOM switch:
        p.Pulse(0, motSwitchOffTime, (int)Parameters["FreeExpansionTime"], "v00MOTAOM"); // pulse off the MOT light whilst MOT fields are turning off and V00 detuning is jumped
        p.Pulse(0, molassesEndTime, motRecaptureTime - molassesEndTime, "v00MOTAOM"); // turn off the MOT light for optical pumping and magnetic trapping. 

        //Microwave pulse
        //p.Pulse(0, microwavePulseTime, (int)Parameters["MWDuration"], "microwaveB");

        p.AddEdge("dipoleTrapAOM", 0, true);
        p.AddEdge("dipoleTrapAOM", molassesEndTime, false);
        p.AddEdge("dipoleTrapAOM", molassesEndTime + (int)Parameters["OPDuration"], true);

        // Blow away:
        //p.Pulse(0, blowAwayTime, (int)Parameters["BlowAwayDuration"], "bXSlowingAOM");

        //p.AddEdge("bXSlowingAOM", (int)Parameters["PatternLength"] - 1000, true); // send slowing aom high and hold it high
        //p.AddEdge("v10SlowingAOM", (int)Parameters["PatternLength"] - 1000, true);

        //Camera triggers:
        //p.Pulse(firstImageTime, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        p.Pulse(0, firstImageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger for picture of MOT at full intensity
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
        int firstImageTime = cafMOTLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        int V0IntensityRampStartTime = cafMOTLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        //int firstImageTime = cafMOTLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        int motSwitchOffTime = V0IntensityRampStartTime + (int)Parameters["MOTHoldTime"];

        int finalImageTime = motSwitchOffTime + (int)Parameters["FreeExpansionTime"];

        int molassesStartTime = motSwitchOffTime + (int)Parameters["MolassesDelay"];
        int molassesEndTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];
        int OPbiasfieldSettleTime = molassesEndTime - 100;

        int mqtStartTime = molassesEndTime + (int)Parameters["OPDuration"];
        int microwavePulseTime = mqtStartTime + (int)Parameters["MWDelay"];

        //int microwavePulseTime = molassesEndTime + (int)Parameters["OPDuration"];
        //int mqtStartTime = microwavePulseTime + (int)Parameters["MWDuration"];
        
        //int rbOPStartTime = mqtStartTime - (int)Parameters["RbOPDuration"];

        //int motRecaptureTime = mqtStartTime + (int)Parameters["MQTHoldDuration"];
        int motRecaptureTime = mqtStartTime;
        //int finalImageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];
        int rbMQTImageTime = finalImageTime + 1100;

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
        p.AddAnalogValue("MOTCoilsCurrent", 0, 1.0);
        p.AddAnalogValue("MOTCoilsCurrent", motSwitchOffTime, -0.05);
        p.AddAnalogValue("MOTCoilsCurrent", mqtStartTime, (double)Parameters["MQTBField"]);
        p.AddAnalogValue("MOTCoilsCurrent", rbMQTImageTime - 150, 0.0);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        p.AddAnalogValue("xShimCoilCurrent", OPbiasfieldSettleTime, (double)Parameters["xShimLoadCurrentOP"]);
        p.AddAnalogValue("yShimCoilCurrent", OPbiasfieldSettleTime, (double)Parameters["yShimLoadCurrentOP"]);
        p.AddAnalogValue("zShimCoilCurrent", OPbiasfieldSettleTime, (double)Parameters["zShimLoadCurrentOP"]);

        p.AddAnalogValue("xShimCoilCurrent", mqtStartTime, (double)Parameters["xShimLoadCurrent"]);
        
        //Shim fields for imaging
        //p.AddAnalogValue("xShimCoilCurrent", rbMQTImageTime - 1000, (double)Parameters["xShimImagingCurrent"]);
        //p.AddAnalogValue("yShimCoilCurrent", rbMQTImageTime - 1000, (double)Parameters["yShimImagingCurrent"]);
        //p.AddAnalogValue("zShimCoilCurrent", rbMQTImageTime - 1000, (double)Parameters["zShimImagingCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", cafMOTLoadEndTime, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", finalImageTime, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddAnalogValue("v00Intensity", motSwitchOffTime, (double)Parameters["v0IntensityMolassesValue"]);
        


        // v0 EOM
        p.AddAnalogValue("v00EOMAmp", 0, (double)Parameters["v0EOMMOTValue"]);
        
        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, 10.0 - (double)Parameters["v0FrequencyStartValue"] / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", motSwitchOffTime, 10.0 - (double)Parameters["v0FrequencyNewValue"] / (double)Parameters["calibGradient"]);//jump to blue detuning for blue molasses
        p.AddAnalogValue("v00Frequency", motRecaptureTime, 10.0 - (double)Parameters["v0FrequencyStartValue"] / (double)Parameters["calibGradient"]); //jump aom frequency back to normal for imaging

        return p;
    }

}
