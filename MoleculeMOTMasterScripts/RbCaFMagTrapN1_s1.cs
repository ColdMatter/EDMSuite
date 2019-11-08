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
        Parameters["PatternLength"] = 250000; //100000
        Parameters["TCLBlockStart"] = 2000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 8000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["RbMOTLoadTime"] = 100000;
        Parameters["CaFMOTLoadingDuration"] = 4000;
        Parameters["CMOTHoldTime"] = 500;
        Parameters["LowIntensityMOTHoldTime"] = 400;
        Parameters["RbMolassesDuration"] = 1464;
        Parameters["RbOpticalPumpingDuration"] = 40;
        Parameters["CameraTriggerDelayAfterFirstImage"] = 5000;

        Parameters["MolassesDelay"] = 100;
        Parameters["MolassesHoldTime"] = 1000; //was 1400
        Parameters["MolassesRampDuration"] = 200;
        Parameters["v0F0PumpDuration"] = 10;
        Parameters["MOTPictureTriggerTime"] = 4000;
        Parameters["MicrowavePulseDuration"] = 7;
        Parameters["SecondMicrowavePulseDuration"] = 7;
        Parameters["ThirdMicrowavePulseDuration"] = 0;
        Parameters["MagTrapDuration"] = 30000;
        Parameters["WaitBeforeRecapture"] = 100;
        Parameters["MOTWaitBeforeImage"] = 500;
        Parameters["WaitBeforeImage"] = 100;
        Parameters["FreeExpansionTime"] = 0;

        // Camera
        Parameters["Frame0TriggerDuration"] = 10;

        //PMT
        Parameters["PMTTrigger"] = 4000;
        Parameters["PMTTriggerDuration"] = 10;

        // BX poke
        Parameters["PokeDetuningValue"] = -1.37;//-1.37
        Parameters["PokeDuration"] = 300;

        // Slowing
        Parameters["slowingAOMOnStart"] = 250;
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1500;
        Parameters["slowingAOMOffDuration"] = 40000;
        Parameters["slowingRepumpAOMOnStart"] = 250;
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1700;
        Parameters["slowingRepumpAOMOffDuration"] = 40000;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 340;
        Parameters["SlowingChirpDuration"] = 1160;
        Parameters["SlowingChirpStartValue"] = 0.0;
        Parameters["SlowingChirpEndValue"] = -1.25;
        Parameters["SlowingChirpHoldDuration"] = 8000;

        // Slowing field
        Parameters["slowingCoilsValue"] = 0.42;
        Parameters["slowingCoilsOffTime"] = 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsCurrentRbMOTLoadingValue"] = 0.5;
        Parameters["CaFMOTLoadGradient"] = 1.0;
        Parameters["MOTCoilsCurrentRampEndValue"] = 1.5;
        Parameters["MOTCoilsCurrentRampDuration"] = 9000;
        Parameters["MOTCoilsCurrentMolassesValue"] = -0.05; //-0.05
        Parameters["MOTCoilsCurrentMagTrapValue"] = 1.2;// 1.2;// 0.6;


        // Shim fields
        Parameters["xShimLoadCurrent"] = 6.0; //6
        Parameters["yShimLoadCurrent"] = 2.0;  //4
        Parameters["zShimLoadCurrent"] = 1.5;  //1.5

        // v0 Light Intensity
        Parameters["v0IntensityRampStartTime"] = 5500;
        Parameters["v0IntensityRampDuration"] = 400;
        Parameters["v0IntensityRampStartValue"] = 5.8;
        Parameters["v0IntensityRampEndValue"] = 8.465;
        Parameters["v0IntensityMolassesValue"] = 5.8;
        Parameters["v0IntensityF0PumpValue"] = 8.8;//9.33
        Parameters["v0IntensityImageValue"] = 6.0;

        // v0 Light Frequency
        Parameters["v0FrequencyMOTValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyMolassesValue"] = 20.0;//30.0 //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)
        Parameters["v0FrequencyF0PumpValue"] = 2.0; //0.0 //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)

        // v0 pumping EOM
        Parameters["v0EOMMOTValue"] = 4.7;
        Parameters["v0EOMPumpValue"] = 1.4; // 4.3; //3.5

        //v0aomCalibrationValues
        Parameters["lockAomFrequency"] = 114.1;
        Parameters["calibOffset"] = 64.2129;
        Parameters["calibGradient"] = 5.55075;

        //Rb light detunings:
        Parameters["ImagingFrequency"] = 2.75; //Resonance at aroun 2.65
        Parameters["MOTCoolingLoadingFrequency"] = 4.4;//5.4 usewd to be
        Parameters["MOTRepumpLoadingFrequency"] = 6.6; //6.9
        Parameters["RbCoolingFrequencyCMOT"] = 4.2;
        Parameters["RbRepumpFrequencyCMOT"] = 6.6;
        Parameters["RbMolassesEndDetuning"] = 2.2;

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

        int atomMOTLoadingStartTime = patternStartBeforeQ;
        int atomMOTLoadingEndTime = atomMOTLoadingStartTime + (int)Parameters["RbMOTLoadTime"];
        int moleculeMOTLoadingStartTime = atomMOTLoadingEndTime;
        int moleculeMOTLoadingEndTime = moleculeMOTLoadingStartTime + (int)Parameters["CaFMOTLoadingDuration"];
        int cMOTEndTime = moleculeMOTLoadingEndTime + (int)Parameters["MOTCoilsCurrentRampDuration"] + (int)Parameters["CMOTHoldTime"];
        int motSwitchOffTime = cMOTEndTime + (int)Parameters["v0IntensityRampDuration"] + (int)Parameters["LowIntensityMOTHoldTime"];

        int molassesStartTime = motSwitchOffTime + (int)Parameters["MolassesDelay"];
        int molassesRampTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];
        int rbMolassesEndTime = molassesStartTime + (int)Parameters["RbMolassesDuration"];

        int v0F0PumpStartTime = molassesRampTime + (int)Parameters["MolassesRampDuration"];
        int microwavePulseTime = v0F0PumpStartTime + (int)Parameters["v0F0PumpDuration"] + 80;
        int blowAwayTime = microwavePulseTime + (int)Parameters["MicrowavePulseDuration"];
        int secondMicrowavePulseTime = blowAwayTime + (int)Parameters["PokeDuration"];

        int magTrapStartTime = secondMicrowavePulseTime + (int)Parameters["SecondMicrowavePulseDuration"];
        int motRecaptureTime = magTrapStartTime + (int)Parameters["MagTrapDuration"] + (int)Parameters["WaitBeforeRecapture"];
        int imageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];

        int cameraTrigger1 = imageTime + 3100;
        int cameraTrigger2 = cameraTrigger1 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image
        int cameraTrigger3 = cameraTrigger2 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //bg

        /*
        int cameraTrigger1 = motRecaptureTime + (int)Parameters["WaitBeforeImage"] + (int)Parameters["FreeExpansionTime"];
        int cameraTrigger2 = cameraTrigger1 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image
        int cameraTrigger3 = cameraTrigger2 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //bg
        */
        int swtichAllOn = cameraTrigger3 + 5000;


        p.Pulse(patternStartBeforeQ, -(int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
        p.Pulse(patternStartBeforeQ, 0, (int)Parameters["QSwitchPulseDuration"], "qSwitch"); //trigger the Q switch
        p.Pulse(patternStartBeforeQ + 50000, -(int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
        p.Pulse(patternStartBeforeQ + 50000, 0, (int)Parameters["QSwitchPulseDuration"], "qSwitch"); //trigger the Q switch

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);  // This is how you load "preset" patterns. 


        /*
        for (int t = 150000; t < (int)Parameters["PatternLength"]; t += 50000)
        {
            p.Pulse(patternStartBeforeQ + t, -(int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
            p.Pulse(patternStartBeforeQ + t, 0, (int)Parameters["QSwitchPulseDuration"], "qSwitch"); //trigger the Q switch
        }
        */


        p.Pulse(0, microwavePulseTime, (int)Parameters["MicrowavePulseDuration"], "microwaveB");
        p.Pulse(0, secondMicrowavePulseTime, (int)Parameters["SecondMicrowavePulseDuration"], "microwaveA");
        //p.Pulse(patternStartBeforeQ, thirdMicrowavePulseTime, (int)Parameters["ThirdMicrowavePulseDuration"], "microwaveA");

        p.Pulse(0, motSwitchOffTime, (int)Parameters["MolassesDelay"], "v00MOTAOM"); // pulse off the MOT light whilst MOT fields are turning off
        p.Pulse(0, microwavePulseTime, magTrapStartTime - microwavePulseTime + 2000, "v00MOTAOM"); // turn off the MOT light for microwave pulse and leav it off until loading mag trap. Trun back on once light is shuttered mechanically

        p.Pulse(0, blowAwayTime, (int)Parameters["PokeDuration"], "bXSlowingAOM"); // Blow away

        p.AddEdge("bXSlowingAOM", atomMOTLoadingEndTime + (int)Parameters["slowingAOMOffStart"] + (int)Parameters["slowingAOMOffDuration"], true); // send slowing aom high and hold it high
        p.AddEdge("v10SlowingAOM", atomMOTLoadingEndTime + (int)Parameters["slowingRepumpAOMOffStart"] + (int)Parameters["slowingRepumpAOMOffDuration"], true); // send slowing repump aom high and hold it high


        p.AddEdge("bXSlowingShutter", secondMicrowavePulseTime - 1500, true);
        p.AddEdge("bXSlowingShutter", motRecaptureTime + 5000, false);
        p.AddEdge("v00MOTShutter", magTrapStartTime - 2060, true);
        p.AddEdge("v00MOTShutter", motRecaptureTime - 950, false);


        p.Pulse(0, moleculeMOTLoadingEndTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger for picture of initial MOT
        p.Pulse(0, imageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger

        //Rb:
        p.AddEdge("rb3DCooling", 0, false);
        p.AddEdge("rb3DCooling", rbMolassesEndTime, true);

        ////////////////////////

        //p.AddEdge("rbRepump", 0, true);
        
        p.AddEdge("rbRepump", 0, false);
        p.AddEdge("rbRepump", rbMolassesEndTime + (int)Parameters["RbOpticalPumpingDuration"], true);
        
        ////////////////////////

        p.AddEdge("rbOpticalPumpingAOM", 0, true);
        p.AddEdge("rbOpticalPumpingAOM", rbMolassesEndTime, false); // turn on to pump atoms
        p.AddEdge("rbOpticalPumpingAOM", rbMolassesEndTime + (int)Parameters["RbOpticalPumpingDuration"], true);

        p.AddEdge("rb2DCooling", 0, false);
        p.AddEdge("rb2DCooling", atomMOTLoadingEndTime, true);
        p.AddEdge("rbPushBeam", 0, false);
        p.AddEdge("rbPushBeam", atomMOTLoadingEndTime, true);

        p.AddEdge("rbAbsImagingBeam", 0, true); //Absorption imaging probe
        p.AddEdge("rbAbsImagingBeam", cameraTrigger1, false);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger1 + 15, true);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger2, false);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger2 + 15, true);
        //p.AddEdge("rbAbsImagingBeam", cameraTrigger2, false);
        //p.AddEdge("rbAbsImagingBeam", cameraTrigger2 + 15, true);

        //Rb mechanical shutters:
        p.AddEdge("rb2DMOTShutter", 0, true);
        //p.AddEdge("rb2DMOTShutter", atomMOTLoadingEndTime, true); //turn on probe beam to image cloud after holding in mag trap for some time
        //p.AddEdge("rb2DMOTShutter", swtichAllOn, false);

        p.AddEdge("rbPushBamAbsorptionShutter", 0, false);
        p.AddEdge("rbPushBamAbsorptionShutter", atomMOTLoadingEndTime - (int)Parameters["rbAbsorptionShutterClosingTime"], true);
        p.AddEdge("rbPushBamAbsorptionShutter", imageTime + 3100 - (int)Parameters["rbAbsorptionShutterOpeningTime"], false);

        p.AddEdge("rb3DMOTShutter", 0, false);
        p.AddEdge("rb3DMOTShutter", rbMolassesEndTime - (int)Parameters["coolingShutterClosingTime"], true);
        //p.AddEdge("rb3DMOTShutter", swtichAllOn, false);

        p.AddEdge("rbOPShutter", 0, false); //this shutter now shutters only the optical pumping light
        p.AddEdge("rbOPShutter", rbMolassesEndTime + (int)Parameters["RbOpticalPumpingDuration"] - (int)Parameters["repumpShutterClosingTime"], true);
        //p.AddEdge("rbOPShutter", swtichAllOn, false);

        //Rb camera:
        p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //1st camera frame
        //p.Pulse(0, cameraTrigger2, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //2nd camera frame
        //p.Pulse(0, cameraTrigger3, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //3rd camera frame

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);


        int atomMOTLoadingStartTime = 0;
        int atomMOTLoadingEndTime = atomMOTLoadingStartTime + (int)Parameters["RbMOTLoadTime"];
        int moleculeMOTLoadingStartTime = atomMOTLoadingEndTime;
        int moleculeMOTLoadingEndTime = moleculeMOTLoadingStartTime + (int)Parameters["CaFMOTLoadingDuration"];
        int cMOTEndTime = moleculeMOTLoadingEndTime + (int)Parameters["MOTCoilsCurrentRampDuration"] + (int)Parameters["CMOTHoldTime"];
        int motSwitchOffTime = cMOTEndTime + (int)Parameters["v0IntensityRampDuration"] + (int)Parameters["LowIntensityMOTHoldTime"];

        int molassesStartTime = motSwitchOffTime + (int)Parameters["MolassesDelay"];
        int molassesRampTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];

        int v0F0PumpStartTime = molassesRampTime + (int)Parameters["MolassesRampDuration"];
        int microwavePulseTime = v0F0PumpStartTime + (int)Parameters["v0F0PumpDuration"] + 80;
        int blowAwayTime = microwavePulseTime + (int)Parameters["MicrowavePulseDuration"];
        int secondMicrowavePulseTime = blowAwayTime + (int)Parameters["PokeDuration"];

        int magTrapStartTime = secondMicrowavePulseTime + (int)Parameters["SecondMicrowavePulseDuration"];
        int motRecaptureTime = magTrapStartTime + (int)Parameters["MagTrapDuration"] + (int)Parameters["WaitBeforeRecapture"];
        int imageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];

        // Add Analog Channels
        p.AddChannel("v00Intensity");
        p.AddChannel("v00Frequency");
        p.AddChannel("xShimCoilCurrent");
        p.AddChannel("yShimCoilCurrent");
        p.AddChannel("v00EOMAmp");
        p.AddChannel("zShimCoilCurrent");

        // Add Rb Analog channels
        p.AddChannel("rb3DCoolingFrequency");
        p.AddChannel("rb3DCoolingAttenuation");
        p.AddChannel("rbRepumpFrequency");
        p.AddChannel("rbRepumpAttenuation");
        p.AddChannel("rbAbsImagingFrequency");
        p.AddChannel("rbOffsetLock");

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", atomMOTLoadingEndTime, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", atomMOTLoadingEndTime + (int)Parameters["slowingCoilsOffTime"], 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentRbMOTLoadingValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", moleculeMOTLoadingStartTime, (double)Parameters["CaFMOTLoadGradient"]);
        p.AddLinearRamp("MOTCoilsCurrent", moleculeMOTLoadingEndTime, (int)Parameters["MOTCoilsCurrentRampDuration"], (double)Parameters["MOTCoilsCurrentRampEndValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", motSwitchOffTime, (double)Parameters["MOTCoilsCurrentMolassesValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", magTrapStartTime, (double)Parameters["MOTCoilsCurrentMagTrapValue"]);
        //p.AddAnalogValue("MOTCoilsCurrent", magTrapStartTime + (int)Parameters["MagTrapDuration"], 0.0); // switching off field earlier to measure Rb mag trap life time
        p.AddAnalogValue("MOTCoilsCurrent", motRecaptureTime, (double)Parameters["CaFMOTLoadGradient"]);
        p.AddAnalogValue("MOTCoilsCurrent", imageTime + 3000, 0.0);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", atomMOTLoadingStartTime, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", atomMOTLoadingStartTime, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", atomMOTLoadingStartTime, (double)Parameters["zShimLoadCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", atomMOTLoadingStartTime, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", cMOTEndTime, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", molassesStartTime, (double)Parameters["v0IntensityMolassesValue"]);
        p.AddAnalogValue("v00Intensity", molassesRampTime + 50, 7.4);
        p.AddAnalogValue("v00Intensity", molassesRampTime + 100, 7.83);
        p.AddAnalogValue("v00Intensity", molassesRampTime + 150, 8.09);
        p.AddAnalogValue("v00Intensity", v0F0PumpStartTime, (double)Parameters["v0IntensityF0PumpValue"]);
        p.AddAnalogValue("v00Intensity", motRecaptureTime, (double)Parameters["v0IntensityImageValue"]);

        // v0 EOM
        p.AddAnalogValue("v00EOMAmp", 0, (double)Parameters["v0EOMMOTValue"]);
        p.AddAnalogValue("v00EOMAmp", v0F0PumpStartTime, (double)Parameters["v0EOMPumpValue"]);
        p.AddAnalogValue("v00EOMAmp", secondMicrowavePulseTime, (double)Parameters["v0EOMMOTValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyMOTValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]);
        p.AddAnalogValue(
            "v00Frequency",
            molassesStartTime,
            ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyMolassesValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]
        );
        p.AddAnalogValue(
            "v00Frequency",
            v0F0PumpStartTime,
            ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyF0PumpValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]
        );
        p.AddAnalogValue(
            "v00Frequency",
            motRecaptureTime,
            ((double)Parameters["lockAomFrequency"] - (double)Parameters["v0FrequencyMOTValue"] / 2 - (double)Parameters["calibOffset"]) / (double)Parameters["calibGradient"]
        );

        //Rb:
        p.AddAnalogValue("rb3DCoolingFrequency", atomMOTLoadingStartTime, (double)Parameters["MOTCoolingLoadingFrequency"]);//Rb MOT loading
        p.AddAnalogValue("rbRepumpFrequency", atomMOTLoadingStartTime, (double)Parameters["MOTRepumpLoadingFrequency"]);
        p.AddAnalogValue("rbAbsImagingFrequency", atomMOTLoadingStartTime, (double)Parameters["ImagingFrequency"]);

        p.AddAnalogValue("rb3DCoolingFrequency", moleculeMOTLoadingEndTime, (double)Parameters["RbCoolingFrequencyCMOT"]);//Rb CMOT
        p.AddAnalogValue("rbRepumpFrequency", moleculeMOTLoadingEndTime, (double)Parameters["RbRepumpFrequencyCMOT"]);

        p.AddLinearRamp("rb3DCoolingFrequency", molassesStartTime, (int)Parameters["RbMolassesDuration"], (double)Parameters["RbMolassesEndDetuning"]);//Rb molasses
        p.AddLinearRamp("rb3DCoolingFrequency", molassesStartTime + (int)Parameters["RbMolassesDuration"], (int)Parameters["RbMolassesDuration"], (double)Parameters["MOTCoolingLoadingFrequency"]);//Jump back frequency to MOT loading value after molasses

        p.AddAnalogValue("rbOffsetLock", 0, 1.1);

        return p;
    }

}
