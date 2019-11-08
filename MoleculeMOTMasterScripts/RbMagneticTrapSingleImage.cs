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
        Parameters["PatternLength"] = 200000;
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 15000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["TurnAllLightOn"] = 10000;


        Parameters["OpticalPumpingDuration"] = 40;



        // Camera
        Parameters["MOTLoadTime"] = 100000;
        Parameters["ProbeImageDelay"] = 5000;
        Parameters["BackgroundImageDelay"] = 5000;
        Parameters["Frame0TriggerDuration"] = 15;
        Parameters["FirstImageDelay"] = 200;
        Parameters["FreeExpansionTime"] = 0;
        Parameters["WaitBeforeImage"] = 100;


        //Rb light
        Parameters["ImagingFrequency"] = 3.0; //Resonance at aroun 2.65
        Parameters["MOTCoolingLoadingFrequency"] = 4.4;//5.4 usewd to be
        Parameters["MOTRepumpLoadingFrequency"] = 6.6; //6.9
        Parameters["RbCoolingFrequencyCMOT"] = 3.8;
        Parameters["RbRepumpFrequencyCMOT"] = 6.6;
        Parameters["RbMolassesDelay"] = 100;
        Parameters["RbMolassesDuration"] = 1460;
        Parameters["RbMolassesEndDetuning"] = 1.8;

        //PMT
        Parameters["PMTTrigger"] = 5000;
        Parameters["PMTTriggerDuration"] = 10;

        // Slowing
        Parameters["slowingAOMOnStart"] = 250; //started from 250
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1500;//started from 1500
        Parameters["slowingAOMOffDuration"] = 40000;
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1520;
        Parameters["slowingRepumpAOMOffDuration"] = 35000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 340;// 340;
        Parameters["SlowingChirpDuration"] = 1160;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = -1.25;

        // Slowing field
        Parameters["slowingCoilsValue"] = 8.0; //1.05;
        Parameters["slowingCoilsOffTime"] = 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsCurrentValue"] = 0.5;//1.0; // 0.65;
        Parameters["CaFMOTLoadGradient"] = 1.0;
        Parameters["CMOTRampDuration"] = 1000;
        Parameters["CMOTEndValue"] = 1.5;
        Parameters["RbCMOTHoldTime"] = 1300;
        Parameters["MagTrapGradient"] = 1.2;
        Parameters["MagneticTrapDuration"] = 30000;

        //arijit: added
        Parameters["RbFirstCMOTHoldTime"] = 3500;
        Parameters["RbMOTHoldTime"] = 500;
        Parameters["CameraTriggerDelayAfterFirstImage"] = 5000;
        


        // Shim fields
        Parameters["xShimLoadCurrent"] =6.0;//3.6
        Parameters["yShimLoadCurrent"] = 4.0;//-0.12
        Parameters["zShimLoadCurrent"] = 1.5;//-5.35


        // v0 Light Switch
        Parameters["MOTAOMStartTime"] = 15000;
        Parameters["MOTAOMDuration"] = 500;

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5000;
        Parameters["v0IntensityRampDuration"] = 2000;
        Parameters["v0IntensityRampStartValue"] = 5.8;
        Parameters["v0IntensityMolassesValue"] = 5.8;

        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 9.0;


        // v0 F=1 (dodgy code using an analogue output to control a TTL)
        Parameters["v0F1AOMStartValue"] = 5.0;
        Parameters["v0F1AOMOffValue"] = 0.0;

        //Rb mechanical shutter closing times:
        Parameters["coolingShutterClosingTime"] = 1600; // 1690 to fully close
        Parameters["repumpShutterClosingTime"] = 150; //this shutter now shutters the optical pumping light  !!!
        Parameters["repumpShutterOpeningTime"] = 216; //this shutter now shutters the optical pumping light !!!
        Parameters["rbAbsorptionShutterClosingTime"] = 300; //to fully close
        Parameters["rbAbsorptionShutterOpeningTime"] = 296; //to fully close
        Parameters["rbOpticalPumpingAnd2DMOTClosingTime"] = 1500; //this shutter now closes only the 2D MOT light!!!




    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.    

        //Trigger camera for the first time after the MOT is loaded and the field is ramped up
        
        
        int rbMOTLoadTime = (int)Parameters["TCLBlockStart"] + (int)Parameters["MOTLoadTime"];
        int rbFirstFieldJump = rbMOTLoadTime + (int)Parameters["RbMOTHoldTime"];
        int rbCMOTStartTime = rbFirstFieldJump + (int)Parameters["RbFirstCMOTHoldTime"];
        int rbMOTswitchOffTime = rbCMOTStartTime + (int)Parameters["CMOTRampDuration"] + (int)Parameters["RbCMOTHoldTime"];
        int rbMolassesStartTime = rbMOTswitchOffTime + (int)Parameters["RbMolassesDelay"];
        int rbMolassesEndTime = rbMolassesStartTime + (int)Parameters["RbMolassesDuration"];
        int rbMagnteticTrapStartTime = rbMolassesEndTime + (int)Parameters["OpticalPumpingDuration"];
        int rbMagnteticTrapEndTime = rbMagnteticTrapStartTime + (int)Parameters["MagneticTrapDuration"];
        int cameraTrigger1 = rbMagnteticTrapEndTime + (int)Parameters["WaitBeforeImage"] + (int)Parameters["FreeExpansionTime"];
        int cameraTrigger2 = cameraTrigger1 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image
        int cameraTrigger3 = cameraTrigger2 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //bg

        int swtichAllOn = cameraTrigger3 + 5000;
        
        //Rb cooling light
        p.AddEdge("rb3DCooling", 0, false);
        p.AddEdge("rb3DCooling", rbMolassesEndTime, true); //switch off cooling light for magnetic trap
        //p.AddEdge("rb3DCooling", swtichAllOn, false); //switch on cooling light just before the end of sequence
        
        
        //Rb repump light
        p.AddEdge("rbRepump", 0, false);
        p.AddEdge("rbRepump", rbMolassesEndTime + (int)Parameters["OpticalPumpingDuration"], true); //switch off repump light for magnetic trap
        //p.AddEdge("rbRepump", swtichAllOn, false);
        

        //Rb optical pumping light
        p.AddEdge("rbOpticalPumpingAOM", 0, true);
        p.AddEdge("rbOpticalPumpingAOM", rbMolassesEndTime, false); // turn on to pump atoms
        p.AddEdge("rbOpticalPumpingAOM", rbMolassesEndTime + (int)Parameters["OpticalPumpingDuration"], true);


        //2D MOT light
        p.AddEdge("rb2DCooling", 0, false);
        p.AddEdge("rb2DCooling", rbMOTLoadTime, true);
        p.AddEdge("rbPushBeam", 0, false);
        p.AddEdge("rbPushBeam", rbMOTLoadTime, true);
        
        //Absorption probe
        p.AddEdge("rbAbsImagingBeam", 0, true);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger1, false); //turn on probe beam to image cloud after holding in mag trap for some time
        p.AddEdge("rbAbsImagingBeam", cameraTrigger3, true);
        //p.AddEdge("rbAbsImagingBeam", secondCameraTrigger + (int)Parameters["TriggerJitter"], false); //turn on probe beam to image what is left in the magnetic trap
        //p.AddEdge("rbAbsImagingBeam", secondCameraTrigger + (int)Parameters["TriggerJitter"] + (int)Parameters["Frame0TriggerDuration"], true);

        //Camera
        //p.Pulse(0, rbCloudPrep + 10, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //1st camera frame
        p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //1st camera frame
        //p.Pulse(0, cameraTrigger2, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //2nd camera frame
        //p.Pulse(0, cameraTrigger3, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //3rd camera frame




        //Rb mechanical shutters
        
        
        p.AddEdge("rbPushBamAbsorptionShutter", 0, false);
        p.AddEdge("rbPushBamAbsorptionShutter", rbMagnteticTrapStartTime - (int)Parameters["rbAbsorptionShutterClosingTime"], true); 
        p.AddEdge("rbPushBamAbsorptionShutter", cameraTrigger1 - (int)Parameters["rbAbsorptionShutterOpeningTime"], false);

        p.AddEdge("rb3DMOTShutter", 0, false);
        p.AddEdge("rb3DMOTShutter", rbMagnteticTrapStartTime - (int)Parameters["coolingShutterClosingTime"], true);
        p.AddEdge("rb3DMOTShutter", swtichAllOn, false);

        p.AddEdge("rb2DMOTShutter", 0, true); //this shutter now closes only the 2D MOT light
        p.AddEdge("rb2DMOTShutter", (int)Parameters["MOTLoadTime"] - (int)Parameters["rbOpticalPumpingAnd2DMOTClosingTime"], false);
        p.AddEdge("rb2DMOTShutter", swtichAllOn, true);

        p.AddEdge("rbOPShutter", 0, false); //this shutter now shutters only the optical pumping light
        p.AddEdge("rbOPShutter", rbMagnteticTrapStartTime + (int)Parameters["OpticalPumpingDuration"] - (int)Parameters["repumpShutterClosingTime"], true);
        p.AddEdge("rbOPShutter", swtichAllOn, false);
        
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);
        
        int rbMOTLoadTime = (int)Parameters["MOTLoadTime"];
        int rbFirstFieldJump = rbMOTLoadTime + (int)Parameters["RbMOTHoldTime"];
        int rbCMOTStartTime = rbFirstFieldJump + (int)Parameters["RbFirstCMOTHoldTime"];
        int rbMOTswitchOffTime = rbCMOTStartTime + (int)Parameters["CMOTRampDuration"] + (int)Parameters["RbCMOTHoldTime"];
        int rbMolassesStartTime = rbMOTswitchOffTime + (int)Parameters["RbMolassesDelay"];
        int rbMolassesEndTime = rbMolassesStartTime + (int)Parameters["RbMolassesDuration"];
        int rbMagnteticTrapStartTime = rbMolassesEndTime + (int)Parameters["OpticalPumpingDuration"];
        int rbMagnteticTrapEndTime = rbMagnteticTrapStartTime + (int)Parameters["MagneticTrapDuration"];
        int cameraTrigger1 = rbMagnteticTrapEndTime + (int)Parameters["WaitBeforeImage"] + (int)Parameters["FreeExpansionTime"];
        int cameraTrigger2 = cameraTrigger1 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image
        int cameraTrigger3 = cameraTrigger2 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //bg
        // Add Analog Channels

        p.AddChannel("v00Intensity");
        p.AddChannel("v00Frequency");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("zShimCoilCurrent");
        p.AddChannel("v00EOMAmp");
        p.AddChannel("v00Chirp");

        // Add Rb Analog channels
        p.AddChannel("rb3DCoolingFrequency");
        p.AddChannel("rb3DCoolingAttenuation");
        p.AddChannel("rbRepumpFrequency");
        p.AddChannel("rbRepumpAttenuation");
        p.AddChannel("rbAbsImagingFrequency");
        p.AddChannel("rbOffsetLock");
        
        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);

        

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]); //switch on MOT coils to load Rb MOT
        p.AddAnalogValue("MOTCoilsCurrent", rbFirstFieldJump, (double)Parameters["CaFMOTLoadGradient"]);
        p.AddLinearRamp("MOTCoilsCurrent", rbCMOTStartTime, (int)Parameters["CMOTRampDuration"], (double)Parameters["CMOTEndValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", rbMOTswitchOffTime, 0.0); //switch off coils after MOT is loaded
        p.AddAnalogValue("MOTCoilsCurrent", rbMagnteticTrapStartTime, (double)Parameters["MagTrapGradient"]);
        p.AddAnalogValue("MOTCoilsCurrent", rbMagnteticTrapEndTime, 0.0);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);
        

        //p.AddAnalogValue("yShimCoilCurrent", (int)Parameters["MOTLoadTime"] + (int)Parameters["MolassesDuration"] - 200, (double)Parameters["OpticalPumpingShimField"]);
        //p.AddAnalogValue("xShimCoilCurrent", (int)Parameters["MOTLoadTime"]+ (int)Parameters["MolassesDuration"] - 500, (double)Parameters["OpticalPumpingShimFieldX"]);

        //p.AddAnalogValue("yShimCoilCurrent", (int)Parameters["MOTLoadTime"] + (int)Parameters["MOTCoilsCurrentRampDuration"] + (int)Parameters["MolassesDuration"] + 1000, 0.0);
        //p.AddAnalogValue("xShimCoilCurrent", (int)Parameters["MOTLoadTime"] + (int)Parameters["MOTCoilsCurrentRampDuration"] + (int)Parameters["MolassesDuration"] + 1000, 0.0);


        // F=0
        p.AddAnalogValue("v00EOMAmp", 0, 5.2);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, (double)Parameters["v0FrequencyStartValue"]);

        //v0 chirp
        p.AddAnalogValue("v00Chirp", 0, 0.0);

        //Rb Laser detunings for loading the MOT
        p.AddAnalogValue("rb3DCoolingFrequency", 0, (double)Parameters["MOTCoolingLoadingFrequency"]);
        p.AddAnalogValue("rbRepumpFrequency", 0, (double)Parameters["MOTRepumpLoadingFrequency"]);
        p.AddAnalogValue("rbAbsImagingFrequency", 0, (double)Parameters["ImagingFrequency"]);

        //Rb laser detunings for CMOT steps:
        p.AddAnalogValue("rb3DCoolingFrequency", rbCMOTStartTime, (double)Parameters["RbCoolingFrequencyCMOT"]);
        p.AddAnalogValue("rbRepumpFrequency", rbCMOTStartTime, (double)Parameters["RbRepumpFrequencyCMOT"]);

        //Rb molasses:
        p.AddLinearRamp("rb3DCoolingFrequency", rbMolassesStartTime, (int)Parameters["RbMolassesDuration"], (double)Parameters["RbMolassesEndDetuning"]);

        p.AddAnalogValue("rbOffsetLock", 0, 1.1);
        
        return p;
    }

}


