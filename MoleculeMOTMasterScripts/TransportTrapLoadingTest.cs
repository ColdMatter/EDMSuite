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
        Parameters["PatternLength"] = 400000;
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
        Parameters["MolassesHoldTime"] = 250;

        Parameters["Frame0Trigger"] = 4000;

        //Recapture to MOT:
        Parameters["MOTWaitBeforeImage"] = 50;

        //Microwaves:
        Parameters["MicrowavePulseDuration"] = 7;
        Parameters["SecondMicrowavePulseDuration"] = 7;

        // Camera
        Parameters["Frame0TriggerDuration"] = 1000;

        //
        Parameters["CaFMOTLoadDuration"] = 5000;

        //PMT
        Parameters["PMTTriggerDuration"] = 10;
        Parameters["PMTTrigger"] = 5000;

        //Optical pumping
        Parameters["OPDuration"] = 200;

        // Slowing
        Parameters["slowingAOMOnStart"] = 180; //started from 250
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1520;//started from 1520
        Parameters["slowingAOMOffDuration"] = 40000;
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1520;//1520
        Parameters["slowingRepumpAOMOffDuration"] = 300000;
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

        Parameters["xShimLoadCurrentOP"] = -1.35;//Bias field for Optical pumping
        Parameters["yShimLoadCurrentOP"] = 3.0;//-1.92;
        Parameters["zShimLoadCurrentOP"] = -0.22;

        //Shim fields for imaging
        Parameters["xShimImagingCurrent"] = -1.93;// -1.35 is zero
        Parameters["yShimImagingCurrent"] = -6.74;// -1.92 is zero
        Parameters["zShimImagingCurrent"] = -0.56;// -0.22 is zero

        // v0 Light Intensity
        Parameters["v0IntensityRampDuration"] = 300;
        Parameters["MOTHoldTime"] = 600;//1000;
        Parameters["v0IntensityRampStartValue"] = 5.6;
        Parameters["v0IntensityImagingInTweezer"] = 7.78;
        Parameters["v0IntensityRampEndValue"] = 7.78;
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
        Parameters["v0FrequencyResonanceValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyOPValue"] = 2.0;

        //v0aomCalibrationValues
        Parameters["calibGradient"] = 11.4;

        //Blow away:
        Parameters["BlowAwayDuration"] = 300;
        Parameters["PokeDetuningValue"] = -1.47;

        //Internal MQT Collisions Chamber:
        Parameters["MQTStartDelay"] = 50;
        Parameters["IntMQTHoldDuration"] = 1000;
        Parameters["MQTBField"] = 1.0;
        Parameters["MQTLowFieldHoldDuration"] = 1600;
       
        Parameters["TransferRampDurationInternalCoils"] = 10000;

        //Transport MQT:
        Parameters["ExtMQTHoldDuration"] = 180000;
        Parameters["TransferRampDurationExternalCoils"] = 13300;
        Parameters["ExternalMagTrapRampEndValue"] = 5.6;

        //Trasfer MQT tweezer chamber
        Parameters["RampDownTimeTransportTrap"] = 10000;//10000;// 14300; 
        Parameters["RampUpDelayIntMagTrapTweezer"] = 10;//10;//4300;
        Parameters["RampUpDurationIntMagTrapTweezer"] = 10000;//4420;
        Parameters["CurrentEndValueIntMagTrapTweezer"] = 0.625;//0.625;
        Parameters["DurationIntMagTrapTweezer"] = 1000;

        //Tweezer MOT
        Parameters["TweezerMOTDuration"] = 1000;//10000;

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

        //int rbMOTLoadingEndTime = (int)Parameters["TCLBlockStart"] + (int)Parameters["RbMOTLoadTime"];
        int cafMOTLoadEndTime = (int)Parameters["TCLBlockStart"] + (int)Parameters["CaFMOTLoadDuration"];
        //int firstImageTime = cafMOTLoadEndTime - 1000;
        //int firstImageTime = (int)Parameters["TCLBlockStart"];
        int V0IntensityRampEndTime = cafMOTLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        //int firstImageTime = cafMOTLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        int firstImageTime = cafMOTLoadEndTime - (int)Parameters["Frame0TriggerDuration"];
        int motSwitchOffTime = V0IntensityRampEndTime + (int)Parameters["MOTHoldTime"];
        int molassesStartTime = motSwitchOffTime + (int)Parameters["MolassesDelay"];
        int molassesEndTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];

        int OPbiasfieldSettleTime = molassesEndTime - 100;

        int mqtStartTime = molassesEndTime + (int)Parameters["OPDuration"];
        int mqtTransferToExternalStartTime = mqtStartTime + (int)Parameters["IntMQTHoldDuration"];
        int mqtTransferToExternalEndTime = mqtTransferToExternalStartTime + (int)Parameters["TransferRampDurationExternalCoils"];

        int mqtExternalEndTime = mqtTransferToExternalEndTime + (int)Parameters["ExtMQTHoldDuration"];
        int rampUpInternalTweezerMagTrap = mqtExternalEndTime + (int)Parameters["RampUpDelayIntMagTrapTweezer"];
        int InternalTweezerMagTrapEndTime = rampUpInternalTweezerMagTrap + (int)Parameters["RampUpDurationIntMagTrapTweezer"] + (int)Parameters["DurationIntMagTrapTweezer"];


        //int motRecaptureTime = mqtTransferToExternalEndTime + (int)Parameters["ExtMQTHoldDuration"];
        //int motRecaptureTime = InternalTweezerMagTrapEndTime; //V0 Light switches ON at this time
        //int motRecaptureTime = mqtExternalEndTime-10000; //V0 Light switches ON at this time
        //int motRecaptureTime = mqtTransferToExternalEndTime + (int)Parameters["ExtMQTHoldDuration"];
        //int motRecaptureTime = mqtStartTime;
        int tweezerMOTcoilsSwitchofftime = InternalTweezerMagTrapEndTime + (int)Parameters["TweezerMOTDuration"];

        

        //int finalImageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];
        int finalImageTime = tweezerMOTcoilsSwitchofftime + (int)Parameters["MOTWaitBeforeImage"];
        int motRecaptureTime = finalImageTime;

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

        for (int t = 100000; t < (int)Parameters["PatternLength"] - 50000; t += 100000) 
        { 
            p.Pulse((int)Parameters["TCLBlockStart"] + t, -(int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
            p.Pulse((int)Parameters["TCLBlockStart"] + t, 0, (int)Parameters["QSwitchPulseDuration"], "qSwitch"); //trigger the Q switch
        }

        //Transport Track:
        p.AddEdge("transportTrack", 0, true);
        p.AddEdge("transportTrack", mqtTransferToExternalEndTime, false);
        p.AddEdge("transportTrack", 250000, true);


        //tweezer light switch:
        //p.AddEdge("lightSwitch", 0, true);
        //p.AddEdge("lightSwitch", mqtStartTime, false);
       //p.AddEdge("lightSwitch", (int)Parameters["PatternLength"] - 10000, true);

        //V00 AOM switch:
        p.Pulse(0, motSwitchOffTime, (int)Parameters["MolassesDelay"], "v00MOTAOM"); // pulse off the MOT light whilst MOT fields are turning off and V00 detuning is jumped
        p.Pulse(0, molassesEndTime, motRecaptureTime - molassesEndTime, "v00MOTAOM"); // turn off the MOT light for optical pumping and magnetic trapping. 

        //Microwave pulse
        //p.Pulse(0, microwavePulseTime, (int)Parameters["MWDuration"], "microwaveB");

        p.AddEdge("dipoleTrapAOM", 0, true);
        p.AddEdge("dipoleTrapAOM", molassesEndTime, false);
        p.AddEdge("dipoleTrapAOM", molassesEndTime + (int)Parameters["OPDuration"], true);

        p.AddEdge("rbOPShutter", 0, false);
        p.AddEdge("rbOPShutter", mqtStartTime - 250, true);
        p.AddEdge("rbOPShutter", rbMQTImageTime + 1000, false);

        p.AddEdge("rb2DMOTShutter", 0, true);
        p.AddEdge("rb2DMOTShutter", mqtStartTime, false);

        // Blow away:
        //p.Pulse(0, blowAwayTime, (int)Parameters["BlowAwayDuration"], "bXSlowingAOM");

        //p.AddEdge("bXSlowingAOM", (int)Parameters["PatternLength"] - 1000, true); // send slowing aom high and hold it high
        //p.AddEdge("v10SlowingAOM", (int)Parameters["PatternLength"] - 1000, true);

        //Camera triggers:
        //p.Pulse(firstImageTime, (int)Parameters["Frame0Trigger"], (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); //camera trigger for first frame
        p.Pulse(0, firstImageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger for picture of MOT at full intensity
        //p.Pulse(0, finalImageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger
        p.Pulse(0, finalImageTime, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig"); // camera trigger
        //p.Pulse(0, mqtTransferToExternalStartTime - 1000, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger

        //Mechanical CaF shutters:
        p.Pulse(0, molassesStartTime - 1500, motRecaptureTime, "bXSlowingShutter"); //B-X shutter closed after blow away
        p.Pulse(0, molassesEndTime - 1200, motRecaptureTime - molassesEndTime - 900, "v00MOTShutter"); //V00 shutter closed after optical pumping an opened for recpature into MOT
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
        int V0IntensityRampEndTime = cafMOTLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        //int firstImageTime = cafMOTLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        int motSwitchOffTime = V0IntensityRampEndTime + (int)Parameters["MOTHoldTime"];
        int molassesStartTime = motSwitchOffTime + (int)Parameters["MolassesDelay"];
        int molassesEndTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];
        int OPbiasfieldSettleTime = molassesEndTime + 100;

        int mqtStartTime = OPbiasfieldSettleTime + (int)Parameters["OPDuration"];
        int mqtTransferToExternalStartTime = mqtStartTime + (int)Parameters["IntMQTHoldDuration"];
        int mqtTransferToExternalEndTime = mqtTransferToExternalStartTime + (int)Parameters["TransferRampDurationExternalCoils"];
        //int microwavePulseTime = mqtStartTime + (int)Parameters["MWDelay"];
        //int microwavePulseTime = molassesEndTime + (int)Parameters["OPDuration"];
        //int mqtStartTime = microwavePulseTime + (int)Parameters["MWDuration"];

        //int microwavePulseTime = molassesEndTime + (int)Parameters["OPDuration"];
        //int mqtStartTime = microwavePulseTime + (int)Parameters["MWDuration"];
        
        int mqtExternalEndTime = mqtTransferToExternalEndTime + (int)Parameters["ExtMQTHoldDuration"];
        int rampUpInternalTweezerMagTrap = mqtExternalEndTime + (int)Parameters["RampUpDelayIntMagTrapTweezer"];
        int InternalTweezerMagTrapEndTime = rampUpInternalTweezerMagTrap + (int)Parameters["RampUpDurationIntMagTrapTweezer"] + (int)Parameters["DurationIntMagTrapTweezer"];


        //int motRecaptureTime = mqtTransferToExternalEndTime + (int)Parameters["ExtMQTHoldDuration"] ;
        //int motRecaptureTime = InternalTweezerMagTrapEndTime; //V0 Light switches ON at this time
        //int motRecaptureTime = mqtExternalEndTime-10000; //V0 Light switches ON at this time
        //int motRecaptureTime = mqtStartTime;
        int tweezerMOTcoilsSwitchofftime = InternalTweezerMagTrapEndTime + (int)Parameters["TweezerMOTDuration"];

        //int finalImageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];
        int finalImageTime = tweezerMOTcoilsSwitchofftime + (int)Parameters["MOTWaitBeforeImage"];
        int motRecaptureTime = finalImageTime;

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
        p.AddChannel("transferCoils");
        p.AddChannel("lightSwitch");
        p.AddChannel("TweezerMOTCoils");

        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", cafMOTLoadEndTime + 1000, 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, 1.0);
        p.AddAnalogValue("MOTCoilsCurrent", motSwitchOffTime, -0.05);
        p.AddAnalogValue("MOTCoilsCurrent", mqtStartTime, (double)Parameters["MQTBField"]);

        
        p.AddAnalogValue("lightSwitch", 0, 5.0);
        p.AddAnalogValue("lightSwitch", mqtStartTime, 0.0);
        p.AddAnalogValue("lightSwitch", (int)Parameters["PatternLength"] - 10000, 5.0);

        //Ramp internal coils current down
        p.AddLinearRamp("MOTCoilsCurrent", mqtTransferToExternalStartTime, (int)Parameters["TransferRampDurationInternalCoils"], 0.0);

        //Ramp up the current of external coils
        p.AddLinearRamp("transferCoils", mqtTransferToExternalStartTime, (int)Parameters["TransferRampDurationExternalCoils"], (double)Parameters["ExternalMagTrapRampEndValue"]);

        //Transfer back to internal trap
        //p.AddLinearRamp("transferCoils", mqtTransferToExternalEndTime + (int)Parameters["ExtMQTHoldDuration"], (int)Parameters["TransferRampDurationExternalCoils"], 0.0);
        //p.AddLinearRamp("MOTCoilsCurrent", mqtTransferToExternalEndTime + (int)Parameters["ExtMQTHoldDuration"] + (int)Parameters["TransferRampDurationExternalCoils"] - (int)Parameters["TransferRampDurationInternalCoils"], (int)Parameters["TransferRampDurationInternalCoils"], (double)Parameters["MQTBField"]);
        p.AddLinearRamp("transferCoils", mqtExternalEndTime, (int)Parameters["RampDownTimeTransportTrap"], 0.0);

        //Internal trap Tweezer chamber ramp up
        p.AddLinearRamp("TweezerMOTCoils", rampUpInternalTweezerMagTrap, (int)Parameters["RampUpDurationIntMagTrapTweezer"], (double)Parameters["CurrentEndValueIntMagTrapTweezer"]);
        

        //Switch off all the coils
        p.AddAnalogValue("MOTCoilsCurrent", rbMQTImageTime - 150, 0.0);
        //p.AddAnalogValue("MOTCoilsCurrent", mqtTransferToExternalEndTime + (int)Parameters["ExtMQTHoldDuration"] + (int)Parameters["TransferRampDurationExternalCoils"], 0.0);
        p.AddAnalogValue("transferCoils", finalImageTime + 2000, 0.0);
        p.AddAnalogValue("TweezerMOTCoils", tweezerMOTcoilsSwitchofftime, -0.02);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        p.AddAnalogValue("xShimCoilCurrent", molassesEndTime, (double)Parameters["xShimLoadCurrentOP"]);
        p.AddAnalogValue("yShimCoilCurrent", molassesEndTime, (double)Parameters["yShimLoadCurrentOP"]);
        p.AddAnalogValue("zShimCoilCurrent", molassesEndTime, (double)Parameters["zShimLoadCurrentOP"]);

        //p.AddAnalogValue("xShimCoilCurrent", mqtStartTime, (double)Parameters["xShimLoadCurrent"]);

        //Shim fields for imaging
        //p.AddAnalogValue("xShimCoilCurrent", rbMQTImageTime - 1000, (double)Parameters["xShimImagingCurrent"]);
        //p.AddAnalogValue("yShimCoilCurrent", rbMQTImageTime - 1000, (double)Parameters["yShimImagingCurrent"]);
        //p.AddAnalogValue("zShimCoilCurrent", rbMQTImageTime - 1000, (double)Parameters["zShimImagingCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", cafMOTLoadEndTime, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", motSwitchOffTime, (double)Parameters["v0IntensityMolassesValue"]);
        //p.AddAnalogValue("v00Intensity", motRecaptureTime - 200, (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", motRecaptureTime, (double)Parameters["v0IntensityImagingInTweezer"]);

        // v0 EOM
        p.AddAnalogValue("v00EOMAmp", 0, (double)Parameters["v0EOMMOTValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, 10.0 - (double)Parameters["v0FrequencyStartValue"] / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", motSwitchOffTime, 10.0 - (double)Parameters["v0FrequencyNewValue"] / (double)Parameters["calibGradient"]);//jump to blue detuning for blue molasses
        //p.AddAnalogValue("v00Frequency", motRecaptureTime, 10.0 - (double)Parameters["v0FrequencyStartValue"] / (double)Parameters["calibGradient"]); //jump aom frequency back to normal for imaging
        p.AddAnalogValue("v00Frequency", motRecaptureTime, 10.0 - (double)Parameters["v0FrequencyResonanceValue"] / (double)Parameters["calibGradient"]); //jump aom frequency to resonance for imaging

        return p;
    }

}
