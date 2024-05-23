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
        Parameters["PatternLength"] = 150000;
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 15000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["TurnAllLightOn"] = 10000;


        Parameters["RepumpSwitchOffDelay"] = 0;
 



        // Camera
        Parameters["MOTLoadTime"] = 100000;
        Parameters["ProbeImageDelay"] = 5000;
        Parameters["BackgroundImageDelay"] = 5000;
        Parameters["Frame0TriggerDuration"] = 15;


        //Rb light
        Parameters["ImagingFrequency"] = 2.3; //2.1
        Parameters["MOTCoolingLoadingFrequency"] = 4.4;
        Parameters["MOTRepumpLoadingFrequency"] = 6.9; //6.9
        Parameters["OPRepumpFrequency"] = 7.9;


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
        Parameters["CMOTCurrentValue"] = 1.0;
        Parameters["magneticTrapDuration"] = 4200;
        Parameters["magneticTraploadingCurrent"] = 1.7;
        Parameters["MagneticTrapRampDuration"] = 0;

        // Shim fields
        Parameters["xShimLoadCurrent"] = 0.0;//3.6
        Parameters["yShimLoadCurrent"] = -5.0;//-0.12
        Parameters["zShimLoadCurrent"] = 0.0;//-5.35
        Parameters["OpticalPumpingShimField"] = -10.0;
        Parameters["OpticalPumpingShimFieldX"] = 0.0;
        Parameters["OPShimFieldDelay"] = 100;


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
        Parameters["coolingShutterClosingTime"] = 1690; // 1690 to fully close
        Parameters["coolingShutterOpeningTime"] = 1200; // this is a guess
        Parameters["repumpShutterClosingTime"] = 150; //to fully close
        Parameters["repumpShutterOpeningTime"] = 240; //to fully open
        Parameters["rbAbsorptionShutterClosingTime"] = 300; //to fully close
        Parameters["rbAbsorptionShutterOpeningTime"] = 296; //to fully close
        Parameters["rbOpticalPumpingAnd2DMOTClosingTime"] = 1500; //2360 to fully close




    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);  // This is how you load "preset" patterns.    

        //Trigger camera for the first time after the MOT is loaded and the field is ramped up

        int swtichAllOn = (int)Parameters["PatternLength"] - (int)Parameters["TurnAllLightOn"];

        int rbCloudPrep = (int)Parameters["MOTLoadTime"];

        int firstCameraTrigger = rbCloudPrep;

        int secondCameraTrigger = firstCameraTrigger + (int)Parameters["magneticTrapDuration"];

        //int thirdCameraTrigger = secondCameraTrigger + (int)Parameters["BackgroundImageDelay"];
        
        //Rb cooling light
        p.AddEdge("rb3DCooling", 0, false);
        p.AddEdge("rb3DCooling", rbCloudPrep, true); //switch off cooling light for magnetic trap
        p.AddEdge("rb3DCooling", secondCameraTrigger, false); //switch light back on to take a fluorescence image of the cloud in the magnetic trap
        

        //Rb repump light
        p.AddEdge("rbRepump", 0, false);
        p.AddEdge("rbRepump", rbCloudPrep - (int)Parameters["RepumpSwitchOffDelay"], true); //switch off repump light for magnetic trap
        p.AddEdge("rbRepump", secondCameraTrigger , false); //switch light back on to take a fluorescence image of the cloud in the magnetic trap

        p.AddEdge("rbOpticalPumpingAOM", 0, false);
         
        //2D MOT light
        p.AddEdge("rb2DCooling", 0, false);
        p.AddEdge("rb2DCooling", (int)Parameters["MOTLoadTime"], true);
        p.AddEdge("rbPushBeam", 0, false);
        p.AddEdge("rbPushBeam", (int)Parameters["MOTLoadTime"], true);

        //Absorption probe
        p.AddEdge("rbAbsImagingBeam", 0, true);
        //p.AddEdge("rbAbsImagingBeam", firstCameraTrigger, false); //turn on probe beam to image cloud after holding in mag trap for some time
        //p.AddEdge("rbAbsImagingBeam", thirdCameraTrigger - 100, true);
        //p.AddEdge("rbAbsImagingBeam", secondCameraTrigger + (int)Parameters["TriggerJitter"], false); //turn on probe beam to image what is left in the magnetic trap
        //p.AddEdge("rbAbsImagingBeam", secondCameraTrigger + (int)Parameters["TriggerJitter"] + (int)Parameters["Frame0TriggerDuration"], true);

        //Camera
        //p.Pulse(0, firstCameraTrigger-500, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //1st camera frame (normalization image)
        p.Pulse(0, secondCameraTrigger  , (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //2nd camera frame
        //p.Pulse(0, secondCameraTrigger, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //2nd camera frame
        //p.Pulse(0, thirdCameraTrigger, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //3rd camera frame




        //Rb mechanical shutters
        /*
        p.AddEdge("rbPushBamAbsorptionShutter", 0, false); //start with ON
        p.AddEdge("rbPushBamAbsorptionShutter", rbCloudPrep - (int)Parameters["rbAbsorptionShutterClosingTime"], true); //close for mag trap
        
        p.AddEdge("rbCoolingShutter", 0, false); //start with ON
        p.AddEdge("rbCoolingShutter", rbCloudPrep - (int)Parameters["coolingShutterClosingTime"], true); //turn OFF for magnetic trap
        p.AddEdge("rbCoolingShutter", secondCameraTrigger  - (int)Parameters["coolingShutterOpeningTime"], false); //turn OFF for magnetic trap
        
        p.AddEdge("rbOP2DShutter", 0, false);
        p.AddEdge("rbOP2DShutter", rbCloudPrep - (int)Parameters["rbOpticalPumpingAnd2DMOTClosingTime"], true);
        p.AddEdge("rbOP2DShutter", swtichAllOn, false); //switch on rb 2D MOT + optical pumping shutter just before the end of sequence
        
        p.AddEdge("rbRepumpShutter", 0, false); 
        p.AddEdge("rbRepumpShutter", rbCloudPrep - (int)Parameters["RepumpSwitchOffDelay"] - (int)Parameters["repumpShutterClosingTime"], true);
        p.AddEdge("rbRepumpShutter", secondCameraTrigger  - (int)Parameters["repumpShutterOpeningTime"], false);
        */
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters);

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
        //p.AddChannel("rbRepumpFrequency");
        //p.AddChannel("rbRepumpAttenuation");
        p.AddChannel("rbAbsImagingFrequency");

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);



        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, 0.5);
        //p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTLoadTime"], -0.0); //switch off coils for molasses and optical pumping stage
        //p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTLoadTime"] + (int)Parameters["MolassesDuration"] + (int)Parameters["OpticalPumpingDuration"], (double)Parameters["magneticTraploadingCurrent"]); //turn on mag trap
        //p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTLoadTime"] + (int)Parameters["MolassesDuration"] + (int)Parameters["OpticalPumpingDuration"] + (int)Parameters["magneticTrapDuration"], -0.0); //switch off coils after holding atoms for some time in magnetic trap
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTLoadTime"]-1000, 0.0); //Jump field to levitation gradient
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTLoadTime"], 1.5); //Jump field to levitation gradient
        p.AddAnalogValue("MOTCoilsCurrent", (int)Parameters["MOTLoadTime"]+(int)Parameters["magneticTrapDuration"], 0.0);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);


        //p.AddAnalogValue("yShimCoilCurrent", (int)Parameters["MOTLoadTime"] + (int)Parameters["MolassesDuration"] - 500, (double)Parameters["OpticalPumpingShimField"]);
        //p.AddAnalogValue("xShimCoilCurrent", (int)Parameters["MOTLoadTime"] + (int)Parameters["MolassesDuration"] - 500, (double)Parameters["OpticalPumpingShimFieldX"]);

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


        //Rb Laser detunings
        p.AddAnalogValue("rb3DCoolingFrequency", 0, (double)Parameters["MOTCoolingLoadingFrequency"]);
        //p.AddAnalogValue("rbRepumpFrequency", 0, (double)Parameters["MOTRepumpLoadingFrequency"]);
        p.AddAnalogValue("rbAbsImagingFrequency", 0, (double)Parameters["ImagingFrequency"]);
        p.AddAnalogValue("rb3DCoolingAttenuation", 0, 0.0);

       
        return p;
    }

}


