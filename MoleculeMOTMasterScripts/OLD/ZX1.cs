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
        Parameters["PatternLength"] = 340000;
        Parameters["TCLBlockStart"] = 2000; // This is a time before the Q switch
        Parameters["TCLBlockDuration"] = 8000;
        Parameters["FlashToQ"] = 16; // This is a time before the Q switch
        Parameters["QSwitchPulseDuration"] = 10;
        Parameters["FlashPulseDuration"] = 10;
        Parameters["HeliumShutterToQ"] = 100;
        Parameters["HeliumShutterDuration"] = 1550;

        Parameters["RbMOTLoadTime"] = 200000;

        //Blue molasses:
        Parameters["MolassesDelay"] = 100;
        Parameters["MolassesHoldTime"] = 500;

        //OP CaF:
        Parameters["v0F0PumpDuration"] = 100;

        //Recapture to MOT:
        Parameters["WaitBeforeRecapture"] = 100;
        Parameters["MOTWaitBeforeImage"] = 1000;

        // Camera
        Parameters["Frame0TriggerDuration"] = 10;

        //
        Parameters["CaFMOTLoadDuration"] = 5000;

        // Slowing
        Parameters["slowingAOMOnStart"] = 180; //started from 250
        Parameters["slowingAOMOnDuration"] = 45000;
        Parameters["slowingAOMOffStart"] = 1520;//started from 1520
        Parameters["slowingAOMOffDuration"] = 40000;
        Parameters["slowingRepumpAOMOnStart"] = 0;//started from 0
        Parameters["slowingRepumpAOMOnDuration"] = 45000;
        Parameters["slowingRepumpAOMOffStart"] = 1520;//1520
        Parameters["slowingRepumpAOMOffDuration"] = 35000;
        Parameters["SlowingChirpHoldDuration"] = 200;

        // Slowing Chirp
        Parameters["SlowingChirpStartTime"] = 380;// 380;
        Parameters["SlowingChirpDuration"] = 1160; //1160
        Parameters["SlowingChirpStartValue"] = 0.0;//0.0
        Parameters["SlowingChirpEndValue"] = -1.25; //-1.25

        // Slowing field
        Parameters["slowingCoilsValue"] = 0.42; //0.42
        Parameters["slowingCoilsOffTime"] = 1500;

        // B Field
        Parameters["MOTCoilsSwitchOn"] = 0;
        Parameters["MOTCoilsCurrentRampStartValue"] = 1.0;
        Parameters["MOTCoilsCurrentMolassesValue"] = -0.05;// -0.01; //0.21

        // External MOT coils:
        Parameters["TransferRampDurationExternalCoils"] = 10000;
        Parameters["ExternalMagTrapRampEndValue"] = 5.6;

        // Shim fields
        Parameters["xShimLoadCurrent"] = -1.35;// -1.35 is zero
        Parameters["yShimLoadCurrent"] = -1.92;// -1.92 is zero
        Parameters["zShimLoadCurrent"] = -0.22;// -0.22 is zero

        //Shim fields for OP
        Parameters["xShimOPCurrent"] = -2.0;// 5.0
        Parameters["yShimOPCurrent"] = 10.0;// -1.92
        Parameters["zShimOPCurrent"] = -3.0;// -0.22 is zero

        //z shim for MW pulse:
        Parameters["zShimMWCurrent"] = 10.0;

        // v0 Light Intensity
        Parameters["v0IntensityRampDuration"] = 300;
        Parameters["MOTHoldTime"] = 1000;
        Parameters["v0IntensityRampStartValue"] = 5.6;
        Parameters["v0IntensityRampEndValue"] = 8.0; //8.2
        Parameters["v0IntensityMolassesValue"] = 5.6;
        Parameters["v0IntensityBlowUpValue"] = 5.6;
        Parameters["v0IntensityMolassesLowValue"] = 7.0;

        // v0 Light Frequency
        Parameters["v0FrequencyMOTValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyMolassesValue"] = 20.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)

        // v0 pumping EOM
        Parameters["v0EOMMOTValue"] = 4.8;

        // v0 Light Frequency
        Parameters["v0FrequencyStartValue"] = 0.0; //set this to 0.0 for 114.1MHz 
        Parameters["v0FrequencyNewValue"] = 22.0; //set this to MHz detuning desired if doing frequency jump (positive for blue detuning)
        Parameters["v0FrequencyBlowUpValue"] = 0.0;

        //v0aomCalibrationValues
        Parameters["calibGradient"] = 11.4;

        //Blow away:
        Parameters["BlowAwayDuration"] = 0;
        Parameters["PokeDetuningValue"] = -1.25;
        Parameters["BlowUpDuration"] = 1000;

        //MQT:
        Parameters["MOTBField"] = 1.0;
        Parameters["MQTStartDelay"] = 10;
        Parameters["MQTHoldDuration"] = 90000;
        Parameters["MQTBField"] = 1.0;
        Parameters["MQTLowFieldHoldDuration"] = 1600;
        Parameters["MQTFieldRampDuration"] = 1600;

        //Rb light:
        Parameters["ImagingFrequency"] = 2.8;
        Parameters["MOTCoolingLoadingFrequency"] = 4.6;
        Parameters["MOTRepumpLoadingFrequency"] = 6.6;

        //Rb prep for MQT:
        Parameters["MolassesFrequnecyRampDuration"] = 1000;
        Parameters["MolassesEndFrequency"] = 0.5;
        Parameters["RbOPDuration"] = 150;
        Parameters["RbRepumpOPDetuning"] = 8.2;

        Parameters["RbRepumpSwitch"] = 0.0; // 0.0 will keep it on and 10.0 will switch it off

        //CaF OP
        Parameters["CaFOPDuration"] = 200;

        //MW pulse:
        Parameters["MicrowavePulseDuration"] = 9;
        Parameters["SecondMicrowavePulseDuration"] = 9;


    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();

        int rbMOTLoadingEndTime = (int)Parameters["TCLBlockStart"] + (int)Parameters["RbMOTLoadTime"];
        int cafMOTLoadEndTime = rbMOTLoadingEndTime + (int)Parameters["CaFMOTLoadDuration"];
        int firstImageTime = cafMOTLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        int motSwitchOffTime = firstImageTime + (int)Parameters["MOTHoldTime"];
        int molassesStartTime = motSwitchOffTime + (int)Parameters["MolassesDelay"];
        int molassesEndTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];
        int cafOPStartTime = molassesEndTime + 50;
        int firstMWPulseTime = cafOPStartTime + (int)Parameters["CaFOPDuration"] + 50;
        int mqtStartTime = firstMWPulseTime + (int)Parameters["MicrowavePulseDuration"] + (int)Parameters["MQTStartDelay"];
        int rbOPStartTime = mqtStartTime - (int)Parameters["RbOPDuration"];
        int blowUpTime = mqtStartTime + 5000;
        int mqtSwitchOffTime = mqtStartTime + (int)Parameters["MQTHoldDuration"];
        int secondMWPulseTime = mqtSwitchOffTime + 200;
        int motRecaptureTime = secondMWPulseTime + (int)Parameters["SecondMicrowavePulseDuration"];
        int finalImageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];
        int rbMQTImageTime = finalImageTime + 1100;

        //Dummy Yag shots to cheat the source:
        for (int t = 0; t < (int)Parameters["RbMOTLoadTime"]; t += 50000)
        {
            p.Pulse((int)Parameters["TCLBlockStart"] + t, -(int)Parameters["FlashToQ"], (int)Parameters["QSwitchPulseDuration"], "flashLamp"); //trigger the flashlamp
            p.Pulse((int)Parameters["TCLBlockStart"] + t, 0, (int)Parameters["QSwitchPulseDuration"], "qSwitch"); //trigger the Q switch
        }


        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);  // This is how you load "preset" patterns. 


        ///////////////////Rb//////////////////////

        //Rb MOT and molasses:
        p.AddEdge("rb3DCooling", 0, false); //turn on cooling light at start of sequence
        p.AddEdge("rb3DCooling", rbOPStartTime - 20, true); //switch off cooling light after molasses and before the optical pumping
        p.AddEdge("rb3DCooling", finalImageTime, false);
        p.AddEdge("rb3DCooling", finalImageTime + 50, true);

        p.AddEdge("rb2DCooling", 0, false); //turn on 2D MOT
        p.AddEdge("rb2DCooling", rbMOTLoadingEndTime, true); //turn off 2D MOT

        p.AddEdge("rbPushBeam", 0, false); //turn on push beam
        p.AddEdge("rbPushBeam", rbMOTLoadingEndTime - 200, true); //turn off push beam

        p.AddEdge("rbRepump", 0, false); //turn on Rb repump
        //p.AddEdge("rbRepump", mqtStartTime, true); // Rb repump stays on until atoms are transferred to MQT

        //Rb optical pumping:
        //p.AddEdge("rbOpticalPumpingAOM", 0, false);

        p.AddEdge("rbOpticalPumpingAOM", 0, true);
        p.AddEdge("rbOpticalPumpingAOM", rbOPStartTime, false); // turn on OP beam to pump atoms after switching off molasses
        p.AddEdge("rbOpticalPumpingAOM", mqtStartTime, true); // turn OP beam off to load MQT

        //Rb absorption imaging:
        p.AddEdge("rbAbsImagingBeam", 0, true);
        //p.AddEdge("rbAbsImagingBeam", finalImageTime, false);
        //p.AddEdge("rbAbsImagingBeam", finalImageTime + 15, true);
        //p.AddEdge("rbAbsImagingBeam", finalImageTime + 8000, false);
        //p.AddEdge("rbAbsImagingBeam", finalImageTime + 8000 + 15, true);

        //p.Pulse(0, rbMQTImageTime, 15, "rbAbsImgCamTrig");
        //p.Pulse(0, rbMQTImageTime + 8000, 15, "rbAbsImgCamTrig");
        //p.Pulse(0, rbMQTImageTime + 16000, 15, "rbAbsImgCamTrig");
        //p.AddEdge("rbAbsImgCamTrig", finalImageTime, true);
        //p.AddEdge("rbAbsImgCamTrig", finalImageTime + 5000, false);


        //Rb mechanical shutters:
        p.AddEdge("rb3DMOTShutter", 0, true);
        p.AddEdge("rb3DMOTShutter", mqtStartTime, false);
        p.AddEdge("rb3DMOTShutter", finalImageTime - 3200, true);

        p.AddEdge("rbPushBamAbsorptionShutter", 0, false);
        //p.AddEdge("rbPushBamAbsorptionShutter",  molassesEndTime - 370, true);
        //p.AddEdge("rbPushBamAbsorptionShutter", rbMQTImageTime - 170, false);

        p.AddEdge("rbOPShutter", 0, false);
        p.AddEdge("rbOPShutter", mqtStartTime - 250, true);
        p.AddEdge("rbOPShutter", rbMQTImageTime + 1000, false);

        ///////////////////CaF//////////////////////

        //Microwave pulse:
        //p.Pulse(0, microwavePulseTime, (int)Parameters["MicrowavePulseDuration"], "microwaveA"); //1st pulse
        //p.Pulse(0, secondMicrowavePulseTime, (int)Parameters["SecondMicrowavePulseDuration"], "microwaveB"); //2nd pulse

        //V00 AOM switch:
        p.Pulse(0, motSwitchOffTime, (int)Parameters["MolassesDelay"], "v00MOTAOM"); // pulse off the MOT light whilst MOT fields are turning off and V00 detuning is jumped
        p.AddEdge("v00MOTAOM", molassesEndTime, true);
        p.AddEdge("v00MOTAOM", blowUpTime, false);
        p.AddEdge("v00MOTAOM", blowUpTime + (int)Parameters["BlowUpDuration"], true);
        p.AddEdge("v00MOTAOM", finalImageTime, false);


        // Blow away:
        //p.Pulse(0, blowAwayTime, (int)Parameters["BlowAwayDuration"], "bXSlowingAOM");

        //p.AddEdge("bXSlowingAOM", rbMOTLoadingEndTime + (int)Parameters["slowingAOMOffStart"] + (int)Parameters["slowingAOMOffDuration"], true); // send slowing aom high and hold it high
        //p.AddEdge("v10SlowingAOM", rbMOTLoadingEndTime + (int)Parameters["slowingRepumpAOMOffStart"] + (int)Parameters["slowingRepumpAOMOffDuration"], true);

        //Camera triggers:
        //p.Pulse(0, firstImageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger for picture of MOT at 20 percent intensity
        p.Pulse(0, finalImageTime, (int)Parameters["Frame0TriggerDuration"], "cameraTrigger"); // camera trigger
        p.Pulse(0, finalImageTime, (int)Parameters["Frame0TriggerDuration"], "rbAbsImgCamTrig");

        //Mechanical CaF shutters:
        p.Pulse(0, cafMOTLoadEndTime - 1500, rbMQTImageTime - motSwitchOffTime + 1500, "bXSlowingShutter"); //B-X shutter closed after blow away
        //p.Pulse(0, molassesEndTime - 1950, motRecaptureTime - molassesEndTime - 900, "v00MOTShutter"); //V00 shutter closed after optical pumping an opened for recpature into MOT
        //p.AddEdge("rb2DMOTShutter", 0, false);
        //p.AddEdge("rb2DMOTShutter", cafOPStartTime + (int)Parameters["CaFOPDuration"], true);
        p.AddEdge("cafOptPumpingShutter", 0, true);
        p.AddEdge("cafOptPumpingShutter", cafOPStartTime + (int)Parameters["CaFOPDuration"] - 1000, false);
        p.AddEdge("TransverseCoolingShutter", 0, false);
        //p.AddEdge("TransverseCoolingShutter", cafMOTLoadEndTime - 2000, true);
        //p.AddEdge("TransverseCoolingShutter", finalImageTime, false);

        //p.AddEdge("cafOptPumpingAOM", 0, true);
        p.AddEdge("cafOptPumpingAOM", 0, false);
        p.AddEdge("cafOptPumpingAOM", cafOPStartTime, true);
        p.AddEdge("cafOptPumpingAOM", cafOPStartTime + (int)Parameters["CaFOPDuration"], false);

        //MW pulses:
        p.Pulse(0, firstMWPulseTime, (int)Parameters["MicrowavePulseDuration"], "microwaveA");
        p.Pulse(0, secondMWPulseTime, (int)Parameters["SecondMicrowavePulseDuration"], "microwaveA");

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        MOTMasterScriptSnippet lm = new LoadMoleculeMOTNoSlowingEdge(p, Parameters);
        int rbMOTLoadingEndTime = (int)Parameters["RbMOTLoadTime"];
        int cafMOTLoadEndTime = rbMOTLoadingEndTime + (int)Parameters["CaFMOTLoadDuration"];
        int firstImageTime = cafMOTLoadEndTime + (int)Parameters["v0IntensityRampDuration"];
        int motSwitchOffTime = firstImageTime + (int)Parameters["MOTHoldTime"];
        int molassesStartTime = motSwitchOffTime + (int)Parameters["MolassesDelay"];
        int molassesEndTime = molassesStartTime + (int)Parameters["MolassesHoldTime"];
        int cafOPStartTime = molassesEndTime + 50;
        int firstMWPulseTime = cafOPStartTime + (int)Parameters["CaFOPDuration"] + 50;
        int mqtStartTime = firstMWPulseTime + (int)Parameters["MicrowavePulseDuration"] + (int)Parameters["MQTStartDelay"];
        int blowUpTime = mqtStartTime + 5000;
        int rbOPStartTime = mqtStartTime - (int)Parameters["RbOPDuration"];
        int mqtSwitchOffTime = mqtStartTime + (int)Parameters["MQTHoldDuration"];
        int secondMWPulseTime = mqtSwitchOffTime + 200;
        int motRecaptureTime = secondMWPulseTime + (int)Parameters["SecondMicrowavePulseDuration"];
        int finalImageTime = motRecaptureTime + (int)Parameters["MOTWaitBeforeImage"];
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


        // Slowing field
        p.AddAnalogValue("slowingCoilsCurrent", 0, (double)Parameters["slowingCoilsValue"]);
        p.AddAnalogValue("slowingCoilsCurrent", cafMOTLoadEndTime, 0.0);

        // B Field
        p.AddAnalogValue("MOTCoilsCurrent", 0, (double)Parameters["MOTBField"]);
        p.AddAnalogValue("MOTCoilsCurrent", motSwitchOffTime, -0.05);
        p.AddAnalogValue("MOTCoilsCurrent", mqtStartTime, (double)Parameters["MQTBField"]);
        p.AddAnalogValue("MOTCoilsCurrent", mqtSwitchOffTime, -0.05);
        //p.AddAnalogValue("MOTCoilsCurrent", motRecaptureTime, (double)Parameters["MOTBField"]);
        //p.AddAnalogValue("MOTCoilsCurrent", rbMQTImageTime - 150, 0.0);

        // External MOT coils:
        //p.AddLinearRamp("transferCoils", mqtStartTime, (int)Parameters["TransferRampDurationExternalCoils"], (double)Parameters["ExternalMagTrapRampEndValue"]);
        //p.AddAnalogValue("transferCoils", mqtSwitchOffTime, -0.1);

        // Shim Fields
        p.AddAnalogValue("xShimCoilCurrent", 0, (double)Parameters["xShimLoadCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", 0, (double)Parameters["yShimLoadCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", 0, (double)Parameters["zShimLoadCurrent"]);

        //Shim fields for OP
        p.AddAnalogValue("xShimCoilCurrent", molassesEndTime, (double)Parameters["xShimOPCurrent"]);
        p.AddAnalogValue("yShimCoilCurrent", molassesEndTime, (double)Parameters["yShimOPCurrent"]);
        p.AddAnalogValue("zShimCoilCurrent", molassesEndTime, (double)Parameters["zShimOPCurrent"]);

        // Jump z shim for MW pulse
        //p.AddAnalogValue("zShimCoilCurrent", firstMWPulseTime - 50, (double)Parameters["zShimMWCurrent"]);

        // Jump back z shim to zero lab field:
        //p.AddAnalogValue("zShimCoilCurrent", blowUpTime, (double)Parameters["zShimLoadCurrent"]);

        // v0 Intensity Ramp
        p.AddAnalogValue("v00Intensity", 0, (double)Parameters["v0IntensityRampStartValue"]);
        p.AddLinearRamp("v00Intensity", cafMOTLoadEndTime, (int)Parameters["v0IntensityRampDuration"], (double)Parameters["v0IntensityRampEndValue"]);
        p.AddAnalogValue("v00Intensity", motSwitchOffTime, (double)Parameters["v0IntensityMolassesValue"]);
        p.AddLinearRamp("v00Intensity", molassesStartTime + 200, 300, (double)Parameters["v0IntensityMolassesLowValue"]);
        //p.AddAnalogValue("v00Intensity", molassesStartTime + 300, (double)Parameters["v0IntensityMolassesLowValue"]);
        p.AddAnalogValue("v00Intensity", blowUpTime, (double)Parameters["v0IntensityBlowUpValue"]);
        p.AddAnalogValue("v00Intensity", motRecaptureTime - 200, (double)Parameters["v0IntensityRampStartValue"]);

        // v0 EOM
        p.AddAnalogValue("v00EOMAmp", 0, (double)Parameters["v0EOMMOTValue"]);

        // v0 Frequency Ramp
        p.AddAnalogValue("v00Frequency", 0, 10.0 - (double)Parameters["v0FrequencyStartValue"] / (double)Parameters["calibGradient"]);
        p.AddAnalogValue("v00Frequency", motSwitchOffTime, 10.0 - (double)Parameters["v0FrequencyNewValue"] / (double)Parameters["calibGradient"]);//jump to blue detuning for blue molasses
        p.AddAnalogValue("v00Frequency", blowUpTime, 10.0 - (double)Parameters["v0FrequencyBlowUpValue"] / (double)Parameters["calibGradient"]);//jump to blue detuning for blue molasses
        p.AddAnalogValue("v00Frequency", motRecaptureTime - 200, 10.0 - (double)Parameters["v0FrequencyStartValue"] / (double)Parameters["calibGradient"]); //jump aom frequency back to normal for imaging

        //Rb Laser intensities
        p.AddAnalogValue("rbRepumpAttenuation", 0, (double)Parameters["RbRepumpSwitch"]);
        p.AddAnalogValue("rb3DCoolingAttenuation", 0, 0.0);

        //Rb Laser detunings
        p.AddAnalogValue("rb3DCoolingFrequency", 0, (double)Parameters["MOTCoolingLoadingFrequency"]);
        p.AddAnalogValue("rbRepumpFrequency", 0, (double)Parameters["MOTRepumpLoadingFrequency"]);
        p.AddAnalogValue("rbAbsImagingFrequency", 0, (double)Parameters["ImagingFrequency"]);
        p.AddAnalogValue("rbRepumpFrequency", rbOPStartTime, (double)Parameters["RbRepumpOPDetuning"]);

        //Rb molasses detuning ramp:
        p.AddLinearRamp("rb3DCoolingFrequency", molassesStartTime, (int)Parameters["MolassesFrequnecyRampDuration"], (double)Parameters["MolassesEndFrequency"]);
        p.AddAnalogValue("rb3DCoolingFrequency", finalImageTime - 200, (double)Parameters["MOTCoolingLoadingFrequency"]);
        return p;
    }

}
