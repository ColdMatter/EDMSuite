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
        Parameters["PatternLength"] = 500000;
        Parameters["TCLBlockStart"] = 4000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 15000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["TurnAllLightOn"] = 10000;


        Parameters["OpticalPumpingDuration"] = 400;//40;



        // Camera
        Parameters["MOTLoadTime"] = 100000;
        Parameters["ProbeImageDelay"] = 5000;
        Parameters["BackgroundImageDelay"] = 5000;
        Parameters["Frame0TriggerDuration"] = 15;//15
        Parameters["FirstImageDelay"] = 200;
        Parameters["FreeExpansionTime"] = 0;
        Parameters["WaitBeforeImage"] = 100;
        Parameters["DelayFromTransferStart"] = 0;//10000;//210000;//180000;


        //Rb light
        Parameters["ImagingFrequency"] = 2.58;//old value :2.58; //Resonance at aroun 2.65
        Parameters["MOTCoolingLoadingFrequency"] = 5.2;//5.4 usewd to be
        Parameters["MOTRepumpLoadingFrequency"] = 6.6; //6.9
        Parameters["RbCoolingFrequencyCMOT"] = 3.8;
        Parameters["RbRepumpFrequencyCMOT"] = 6.6;
        Parameters["RbMolassesDelay"] = 100;
        Parameters["RbMolassesDuration"] = 1460;
        Parameters["RbMolassesEndDetuning"] = 1.7;

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
        Parameters["MagTrapInternalGradient"] = 1.2;
        Parameters["TransferRampDurationInternalCoils"] = 10000;//10000;
        Parameters["TransferRampDurationExternalCoils"] = 11600;
        Parameters["ExternalMagTrapRampEndValue"] = 5.6;//5.6;
        Parameters["MagneticTrapDuration"] = 120000; //155000;//45750; // 87750;//180000;
        //Parameters["SpeedBumpCoilsOn"] = 96000-200;
        //Parameters["SpeedBumpCoilsOn"] = 116000- 200;

        Parameters["MotionDelay"] = 5000;

        Parameters["RampDownTimeTransportTrap"] = 10000;
        Parameters["RampUpDelayIntMagTrapTweezer"] = 10;
        Parameters["RampUpDurationIntMagTrapTweezer"] = 10000;//4420;
        Parameters["CurrentEndValueIntMagTrapTweezer"] =0.625;
        Parameters["DurationIntMagTrapTweezer"] =10000;
        Parameters["SpeedBumpRamp1"] = 3200;//2250;
        Parameters["SpeedBumpRamp2"] = 2000;// 28750;
        Parameters["SpeedBumpRamp3"] = 1000;
        Parameters["SpeedBumpRamp4"] = 2000;
        




        //arijit: added
        Parameters["RbFirstCMOTHoldTime"] = 3500;
        Parameters["RbMOTHoldTime"] = 500;
        Parameters["CameraTriggerDelayAfterFirstImage"] = 25000;



        // Shim fields
        Parameters["xShimLoadCurrent"] = 6.0;//3.6
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
        Parameters["coolingShutterClosingTime"] = 1680; // 1680
        Parameters["repumpShutterClosingTime"] = 150; //this shutter now shutters the optical pumping light  !!!
        Parameters["repumpShutterOpeningTime"] = 216; //this shutter now shutters the optical pumping light !!!
        Parameters["rbAbsorptionShutterClosingTime"] = 370; //to fully close
        Parameters["rbAbsorptionShutterOpeningTime"] = 162; //to fully close
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
        //int rbMagnteticTrapStartTime = rbMolassesEndTime + (int)Parameters["OpticalPumpingDuration"];
        int rbMagnteticTrapStartTime = rbMolassesEndTime;
        int rbMagnteticTrapTransferToExternalStartTime = rbMagnteticTrapStartTime + 10000;


        int cameraTrigger1 = rbMagnteticTrapTransferToExternalStartTime + (int)Parameters["DelayFromTransferStart"];
        //int cameraTrigger1 = startMotionTime + (int)Parameters["DelayFromTransferStart"];

        int cameraTrigger2 = cameraTrigger1 + (int)Parameters["WaitBeforeImage"] + (int)Parameters["FreeExpansionTime"] + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image, //image before transfer to external coils
        int cameraTrigger3 = cameraTrigger2 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //bg

        int absprobeswitchoff = cameraTrigger2 + (int)Parameters["Frame0TriggerDuration"] + 20; //bg
        int coolingimgswitchoff = cameraTrigger1 + 20; //bg
        int swtichAllOn = cameraTrigger3 + 5000;


        //Rb cooling light
        p.AddEdge("rb3DCooling", 0, false);
        p.AddEdge("rb3DCooling", rbMolassesEndTime, true); //switch off cooling light for magnetic trap
        //p.AddEdge("rb3DCooling", rbMolassesEndTime + (int)Parameters["DelayFromTransferStart"], false);
        //p.AddEdge("rb3DCooling", rbMolassesEndTime + (int)Parameters["DelayFromTransferStart"]+5, true); 
        //p.AddEdge("rb3DCooling", cameraTrigger1, false);
        //p.AddEdge("rb3DCooling", coolingimgswitchoff, true); 
        //p.AddEdge("rb3DCooling", swtichAllOn, false); //switch on cooling light just before the end of sequence


        //Rb repump light
        p.AddEdge("rbRepump", 0, false);
        p.AddEdge("rbRepump", rbMolassesEndTime, true); //switch off repump light for magnetic trap
        //p.AddEdge("rbRepump", cameraTrigger1, false);
        //p.AddEdge("rbRepump", coolingimgswitchoff, true); 

        //p.AddEdge("rbRepump", swtichAllOn, false);


        //Rb optical pumping light
        p.AddEdge("rbOpticalPumpingAOM", 0, false);
        //p.AddEdge("rbOpticalPumpingAOM", rbMolassesEndTime, false); // turn on to pump atoms
        //p.AddEdge("rbOpticalPumpingAOM", rbMolassesEndTime + (int)Parameters["OpticalPumpingDuration"], true);


        //2D MOT light
        p.AddEdge("rb2DCooling", 0, false);
        p.AddEdge("rb2DCooling", rbMOTLoadTime, true);
        p.AddEdge("rbPushBeam", 0, false);
        p.AddEdge("rbPushBeam", rbMOTLoadTime, true);

        //Absorption probe
        p.AddEdge("rbAbsImagingBeam", 0, true);
        //p.AddEdge("rbAbsImagingBeam", cameraTrigger0, false); //turn on probe beam to image cloud after holding in mag trap for some time
        //p.AddEdge("rbAbsImagingBeam", cameraTrigger0 + 500, true);
        p.AddEdge("rbAbsImagingBeam", cameraTrigger1, false); //turn on probe beam to image cloud after holding in mag trap for some time
        p.AddEdge("rbAbsImagingBeam", cameraTrigger2 + 100, true);
        //p.AddEdge("rbAbsImagingBeam", secondCameraTrigger + (int)Parameters["TriggerJitter"], false); //turn on probe beam to image what is left in the magnetic trap
        //p.AddEdge("rbAbsImagingBeam", secondCameraTrigger + (int)Parameters["TriggerJitter"] + (int)Parameters["Frame0TriggerDuration"], true);

        //Camera
        //p.Pulse(0, rbCloudPrep + 10, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //1st camera frame
        //p.Pulse(0, cameraTrigger0, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //0st camera frame
        p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //1st camera frame
        p.Pulse(0, cameraTrigger2, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //2nd camera frame
        p.Pulse(0, cameraTrigger3, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); //3rd camera frame

        //p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger");

        p.Pulse(0, cameraTrigger1, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //1st camera frame
        p.Pulse(0, cameraTrigger2, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //2nd camera frame
        p.Pulse(0, cameraTrigger3, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //3rd camera frame


        //Rb mechanical shutters


        p.AddEdge("rbPushBamAbsorptionShutter", 0, false);
        //p.AddEdge("rbPushBamAbsorptionShutter", rbMolassesStartTime - (int)Parameters["rbAbsorptionShutterClosingTime"], true);
        //p.AddEdge("rbPushBamAbsorptionShutter", cameraTrigger1 - (int)Parameters["rbAbsorptionShutterOpeningTime"], false);

        p.AddEdge("rb3DMOTShutter", 0, false);
        p.AddEdge("rb3DMOTShutter", rbMagnteticTrapStartTime - (int)Parameters["coolingShutterClosingTime"], true);
        p.AddEdge("rb3DMOTShutter", swtichAllOn, false);

        p.AddEdge("rb2DMOTShutter", 0, false); //this shutter now closes only the 2D MOT light
        p.AddEdge("rb2DMOTShutter", (int)Parameters["MOTLoadTime"], true);
        p.AddEdge("rb2DMOTShutter", swtichAllOn, false);

        p.AddEdge("rbOPShutter", 0, false); //this shutter now shutters only the optical pumping light
        //p.AddEdge("rbOPShutter", rbMagnteticTrapStartTime + (int)Parameters["OpticalPumpingDuration"] - (int)Parameters["repumpShutterClosingTime"], true);
        //p.AddEdge("rbOPShutter", swtichAllOn, false);

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
        //int rbMagnteticTrapStartTime = rbMolassesEndTime + (int)Parameters["OpticalPumpingDuration"];
        int rbMagnteticTrapStartTime = rbMolassesEndTime;
        int rbMagnteticTrapTransferToExternalStartTime = rbMagnteticTrapStartTime + 10000;
        int rbMagnteticTrapTransferToExternalEndTime = rbMagnteticTrapTransferToExternalStartTime + (int)Parameters["TransferRampDurationExternalCoils"];
        int startMotionTime = rbMagnteticTrapTransferToExternalEndTime + (int)Parameters["MotionDelay"];
        int rbMagnteticTrapEndTime = startMotionTime + (int)Parameters["MagneticTrapDuration"];
        //int speedbumpon = startMotionTime + (int)Parameters["SpeedBumpCoilsOn"];
        // int cameraTrigger1 = rbMagnteticTrapEndTime + (int)Parameters["WaitBeforeImage"] + (int)Parameters["FreeExpansionTime"]; //image after transfer to external coils
        //int cameraTrigger0 = rbMagnteticTrapStartTime + (int)Parameters["DelayFromTransferStart"];//image before transfer to external coils
        //int cameraTrigger2 = cameraTrigger1 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image, //image after transfer to external coils
        //int cameraTrigger1 = rbMagnteticTrapStartTime + (int)Parameters["DelayFromTransferStart"];
        
        int rampUpInternalTweezerMagTrap = rbMagnteticTrapEndTime + (int)Parameters["RampUpDelayIntMagTrapTweezer"];
        int InternalTweezerMagTrapEndTime = rampUpInternalTweezerMagTrap+(int)Parameters["RampUpDurationIntMagTrapTweezer"]+(int)Parameters["DurationIntMagTrapTweezer"];

        int cameraTrigger1 = rbMagnteticTrapTransferToExternalEndTime + (int)Parameters["DelayFromTransferStart"];
        //int cameraTrigger1 = rbMolassesStartTime + (int)Parameters["DelayFromTransferStart"];
        //int cameraTrigger1 = rbMolassesEndTime + (int)Parameters["DelayFromTransferStart"];
        //int cameraTrigger2 = rbMagnteticTrapEndTime + (int)Parameters["WaitBeforeImage"] + (int)Parameters["FreeExpansionTime"] + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image, //image before transfer to external coils
        //int cameraTrigger3 = cameraTrigger2 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //bg
        int cameraTrigger2 = cameraTrigger1 + (int)Parameters["WaitBeforeImage"] + (int)Parameters["FreeExpansionTime"] + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //probe image, //image before transfer to external coils
        int cameraTrigger3 = cameraTrigger2 + (int)Parameters["CameraTriggerDelayAfterFirstImage"]; //bg


        int absprobeswitchoff = cameraTrigger2 + (int)Parameters["Frame0TriggerDuration"] + 20; //bg
        int coolingimgswitchoff = cameraTrigger1 + 20; //bg
        int swtichAllOn = cameraTrigger3 + 5000;
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
        p.AddChannel("transferCoils");
        p.AddChannel("transferCoilsShunt1");
        p.AddChannel("transferCoilsShunt2");
        p.AddChannel("speedbumpCoils");
        p.AddChannel("TweezerMOTCoils");

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", (int)Parameters["slowingCoilsOffTime"], 0.0);



        // B Field
        p.AddAnalogValue("speedbumpCoils", 0, -0.02);
        p.AddAnalogValue("TweezerMOTCoils", 0, -0.02);

        //p.AddAnalogValue("transferCoilsShunt1", 0, -0.02);
        //p.AddAnalogValue("transferCoilsShunt2", 0, -0.02);

        p.AddAnalogValue("transferCoils", 0, 0.0); //start with external coils off
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTCoilsCurrentValue"]); //switch on MOT coils to load Rb MOT
        p.AddAnalogValue("MOTCoilsCurrent", rbFirstFieldJump, (double)Parameters["CaFMOTLoadGradient"]);
        p.AddLinearRamp("MOTCoilsCurrent", rbCMOTStartTime, (int)Parameters["CMOTRampDuration"], (double)Parameters["CMOTEndValue"]);
        p.AddAnalogValue("MOTCoilsCurrent", rbMOTswitchOffTime, -0.05); //switch off coils after MOT is loaded
        p.AddAnalogValue("MOTCoilsCurrent", rbMagnteticTrapStartTime, (double)Parameters["MagTrapInternalGradient"]);
        p.AddAnalogValue("MOTCoilsCurrent", rbMagnteticTrapTransferToExternalStartTime,0.0);

        //p.AddLinearRamp("MOTCoilsCurrent", rbMagnteticTrapTransferToExternalStartTime, (int)Parameters["TransferRampDurationInternalCoils"], 0.0);
        
        p.AddLinearRamp("transferCoils", rbMagnteticTrapTransferToExternalStartTime, (int)Parameters["TransferRampDurationExternalCoils"], (double)Parameters["ExternalMagTrapRampEndValue"]);

        p.AddLinearRamp("transferCoils", rampUpInternalTweezerMagTrap, (int)Parameters["RampDownTimeTransportTrap"], 0.0);

        p.AddLinearRamp("TweezerMOTCoils", rampUpInternalTweezerMagTrap, (int)Parameters["RampUpDurationIntMagTrapTweezer"], (double)Parameters["CurrentEndValueIntMagTrapTweezer"]);


        p.AddAnalogValue("TweezerMOTCoils", InternalTweezerMagTrapEndTime, -0.02);

        
        p.AddAnalogValue("transferCoilsShunt1", rbMagnteticTrapTransferToExternalEndTime + 1000, 0.1);
        p.AddAnalogValue("transferCoilsShunt2", rbMagnteticTrapTransferToExternalEndTime + 1000, 0.1);

        

        // [131,135,140, 145,150,156, 162, 166, 170, 175, 180,186, 190, 195, 200, 205, 209, 216, 220]
        // [ 57500., 58000., 58750., 59500., 60250., 61125., 62000., 62625.,63125., 63875. 64625., 65500., 66125., 66875., 67500., 68250., 68875., 69875., 70500.]
        
        p.AddAnalogValue("speedbumpCoils", startMotionTime + 58000, -0.08);
        p.AddAnalogValue("speedbumpCoils", startMotionTime + 59000, -0.15);
        p.AddAnalogValue("speedbumpCoils", startMotionTime + 60000, -0.3);
        p.AddAnalogValue("speedbumpCoils", startMotionTime + 61000, -0.25);
        p.AddAnalogValue("speedbumpCoils", startMotionTime + 62000, -0.2);

        p.AddAnalogValue("speedbumpCoils", startMotionTime + 64000, 0.4);
        p.AddAnalogValue("speedbumpCoils", startMotionTime + 65000, 0);
        //p.AddAnalogValue("speedbumpCoils", startMotionTime + 65000, 0.1);
        //p.AddAnalogValue("speedbumpCoils", startMotionTime + 66000, 0);
        //p.AddAnalogValue("speedbumpCoils", startMotionTime + 68000, -0.3);
        p.AddAnalogValue("speedbumpCoils", startMotionTime + 68500, -0.45);

        p.AddAnalogValue("speedbumpCoils", startMotionTime + 70000, -0.55);

        p.AddAnalogValue("speedbumpCoils", startMotionTime + 72000, 0);

        
        p.AddAnalogValue("transferCoilsShunt1", startMotionTime + 58000, 0.06);
        p.AddAnalogValue("transferCoilsShunt1", startMotionTime + 60000, 0.18);
        p.AddAnalogValue("transferCoilsShunt1", startMotionTime + 61000, 0.2);
        p.AddAnalogValue("transferCoilsShunt1", startMotionTime + 61500, 0.25);
        p.AddAnalogValue("transferCoilsShunt1", startMotionTime + 62000, 0.45);

        p.AddAnalogValue("transferCoilsShunt1", startMotionTime + 64000, 0.55);
        p.AddAnalogValue("transferCoilsShunt1", startMotionTime + 65000, 0.4);
        p.AddAnalogValue("transferCoilsShunt1", startMotionTime + 66000, 0.25);
        p.AddAnalogValue("transferCoilsShunt1", startMotionTime + 68500, 0.2);
        p.AddAnalogValue("transferCoilsShunt1", startMotionTime + 70500, 0.25);

        p.AddAnalogValue("transferCoilsShunt1", startMotionTime + 73000, 0.0);
        p.AddAnalogValue("transferCoilsShunt2", startMotionTime + 73000, 0.05);
        p.AddAnalogValue("transferCoilsShunt2", startMotionTime + 74500, 0.0);

        p.AddAnalogValue("transferCoilsShunt1", startMotionTime + 76000, 0.2);
        p.AddAnalogValue("transferCoilsShunt1", startMotionTime + 78000, 0.4);
        



        /* **************** Accel 2200 TP 175 ************************ 
        double Vfact = -0.1;
        
       p.AddLinearRamp("speedbumpCoils", startMotionTime + 59375, 1000, -0.012 + (Vfact * 0));//-0.149
       p.AddLinearRamp("speedbumpCoils", startMotionTime + 60375, 1375, -0.012 + (Vfact * 0));//-0.179
       p.AddLinearRamp("speedbumpCoils", startMotionTime + 61750, 1374, -0.012 + (Vfact * 0.8));//0.353
       p.AddLinearRamp("speedbumpCoils", startMotionTime + 63125, 1500, -0.012 + (Vfact * 1.449));
       p.AddLinearRamp("speedbumpCoils", startMotionTime + 64625, 2000, -0.012 + (Vfact * 1.795));
       p.AddLinearRamp("speedbumpCoils", startMotionTime + 66625, 2500, -0.012 + (Vfact * 1.42));
       p.AddLinearRamp("speedbumpCoils", startMotionTime + 69125, 2000, -0.012 + (Vfact * 1.038));
       p.AddLinearRamp("speedbumpCoils", startMotionTime + 71125, 2874, -0.012 + (Vfact * -0.11));
       p.AddLinearRamp("speedbumpCoils", startMotionTime + 74000, 16500, -0.012 + (Vfact * 0.257));
       
        double Vfact = 1;

       p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 59375, 1000, -0.012 + (Vfact * 0.001));
       p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 60375, 1375, -0.012 + (Vfact * 0.003));
       p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 61750, 1374, -0.012 + (Vfact * 0.02));
       p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 63125, 1500, -0.012 + (Vfact * 0.08));//0.006
       p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 64625, 2000, -0.012 + (Vfact * 0.186));
       p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 66625, 2500, -0.012 + (Vfact * 0.441));
       p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 69125, 2000, -0.012 + (Vfact * 0.37));
       p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 71125, 2874, -0.012 + (Vfact * 0.448));
       p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 74000, 16500, -0.012 + (Vfact * 0.467));

       */


        /* ***************** Accel 2200 TP 380 ****************************
        double Vfact = -0.1;
        p.AddLinearRamp("speedbumpCoils", startMotionTime + 57500, 499, -0.012 + (Vfact * 0));
        p.AddLinearRamp("speedbumpCoils", startMotionTime + 57999, 750, -0.012 + (Vfact * 0));
        p.AddLinearRamp("speedbumpCoils", startMotionTime + 58750, 749, -0.012 + (Vfact * 0.8));
        p.AddLinearRamp("speedbumpCoils", startMotionTime + 59500, 750, -0.012 + (Vfact * 1.449));
        p.AddLinearRamp("speedbumpCoils", startMotionTime + 60250, 874, -0.012 + (Vfact * 1.795));
        p.AddLinearRamp("speedbumpCoils", startMotionTime + 61124, 875, -0.012 + (Vfact * 1.42));
        p.AddLinearRamp("speedbumpCoils", startMotionTime + 62000, 624, -0.012 + (Vfact * 1.038));
        p.AddLinearRamp("speedbumpCoils", startMotionTime + 62625, 500, -0.012 + (Vfact * -0.11));
        p.AddLinearRamp("speedbumpCoils", startMotionTime + 63125, 750, -0.012 + (Vfact * 0.257));
         
        Vfact = 1;
        p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 57500, 499, -0.012 + (Vfact * 0.001));
        p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 57999, 750, -0.012 + (Vfact * 0.003));
        p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 58750, 749, -0.012 + (Vfact * 0.02));
        p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 59500, 750, -0.012 + (Vfact * 0.08));
        p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 60250, 874, -0.012 + (Vfact * 0.186));
        p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 61124, 875, -0.012 + (Vfact * 0.441));
        p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 62000, 624, -0.012 + (Vfact * 0.37));
        p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 62625, 500, -0.012 + (Vfact * 0.448));
        p.AddLinearRamp("transferCoilsShunt1", startMotionTime + 63125, 750, -0.012 + (Vfact * 0.467));
        
       */

        // Shim Fields
        //p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        //p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        //p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);


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


        p.AddAnalogValue("yShimCoilCurrent", 0, 0.0);
        p.AddAnalogValue("yShimCoilCurrent", startMotionTime, 5.0);//This is now used to trigger the translation stage of the transport coils
        p.AddAnalogValue("yShimCoilCurrent", startMotionTime + 100, 0.0);




        return p;
    }

}


