using MOTMaster;
using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


public class Patterns : MOTMasterScript
/*
 * This script is designed to create the basic MOT, nothing fancy
 * The time unit in this script is in multiple of 10 micro second.
 * The other unit is Volt.
 * */
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 350000;
        Parameters["TCLBlockStart"] = 5000;

        // Rb 2D MOT 
        Parameters["PushBeamONorOFF"] = 1.0;
        Parameters["Rb2DStart"] = -1500;
        Parameters["Rb2DDuration"] = 100000;
        Parameters["Rb2DCoolingVCOAmp"] = 0.58;
        Parameters["RbPushBeamStart"] = 0;
        Parameters["RbPushBeamDuration"] = 110000;
        Parameters["RbPushBeamAOMVCOAmpPush"] = 1.0;

        // Rb 3D MOT
        Parameters["Rb3DMOTStart"] = 0; // 3D Cooling AOM
        Parameters["Rb3DMOTDuration"] = 100000;
        Parameters["Rb3DMOTVCAAmp"] = 2.8;
        Parameters["RbFrequencyDummy"] = 10.0;
        Parameters["RbAmpDummy"] = 0.2;
        Parameters["RbShimDummy"] = 0.2;

        Parameters["Rb3DMOTBField"] = 1.9;            // ~3.6 V should be 1 A, 3.0 optimal recently
        Parameters["XShimCoilsMOTValue"] = -6.0;
        Parameters["YShimCoilsMOTValue"] = -4.0;
        Parameters["ZShimCoilsMOTValue"] = 1.0;

        // Rb CMOT
        Parameters["CMOTONorOFF"] = 10.0;
        Parameters["RbCMOTRampDuration"] = 1000; // 550 
        Parameters["RbCMOTDuration"] = 100;
        Parameters["RbCMOTVCAAmp"] = 1.75;
        Parameters["RbCMOTBField"] = 6.0; // 2.0 
        Parameters["XShimCoilsCMOTValue"] = -6.0; // -2.0 for no CMOT
        Parameters["YShimCoilsCMOTValue"] = -4.0; // -2.0 for no CMOT
        Parameters["ZShimCoilsCMOTValue"] = 1.0;

        // Rb Molasses
        Parameters["MolassesONorOFF"] = 10.0;
        Parameters["RbMOTFieldDecayDuration"] = 1;
        Parameters["RbMolassesRampDuration"] = 1;
        Parameters["RbMolassesHoldDuration"] = 2000; // 2000;
        Parameters["RbMolassesVCAAmpStart"] = 1.7;
        Parameters["RbMolassesVCAAmpEnd"] = 1.7;
        Parameters["XShimCoilsMolassesValue"] = -6.0;
        Parameters["YShimCoilsMolassesValue"] = -4.0;
        Parameters["ZShimCoilsMolassesValue"] = 1.0;
        Parameters["RbMolassesShimFieldDelay"] = 550;
        Parameters["RbMolassesShimFieldRampDuration"] = 100;

        // ODT
        Parameters["ODTONorOFF"] = 10.0;
        Parameters["ShuttersONorOFF"] = 10.0;
        Parameters["ODTOnlyDuration"] = 100; // duration not used yet
        Parameters["ODTVVAOnValue"] = 5.3; // not currently on because using offset value as baseline instead
        Parameters["ODTVVAOffValue"] = 0.0;
        Parameters["ODTDuration"] = 20000; // heating and non-heating part
        Parameters["ShutterOpenDelay"] = 8000;
        Parameters["ShutterCloseDelay"] = 4000;


        // parametric heating
        Parameters["ParametricHeatingONorOFF"] = 1.0;
        Parameters["ODTHeatingStart"] = 10000; // referenced to molasses end time, not used rn, instead total duration
        Parameters["ODTHeatingDuration"] = 70000; // not used right now, instead use number of cycles
        Parameters["ODTHeatingCycles"] = 500; 
        Parameters["ODTHeatingFrequency"] = 4000.0; // Hz
        Parameters["ODTHeatingAmp"] = 0.0; // fraction of max power
        Parameters["ODTHeatingOffset"] = 1.0; // fraction of max power
        Parameters["ODTHeatingPhase"] = 0.0; // start on maximum for no aprupt changes
        Parameters["ControlVoltageToPowerA"] = -3.6866;
        Parameters["ControlVoltageToPowerB"] = 0.9963;
        Parameters["ControlVoltageToPowerC"] = 5.1755;

        // Camera trigger properties
        Parameters["BackgroundImageONorOFF"] = 10.0;
        Parameters["NormalisationImageONorOFF"] = 1.0;
        Parameters["CameraTriggerStartDelay"] = -50; // delay after odtStop
        Parameters["NormalisationTriggerStartDelay"] = 10000;
        Parameters["CameraTriggerDuration"] = 20;
        Parameters["BackgroundImageTime"] = 320000; 
        Parameters["CameraExposureDelay"] = 0; // a constant of nature for Normal CCD
        Parameters["ThorlabsCameraExposureDelay"] = 2000; // a constant of nature for Normal CCD
        Parameters["ImagingLightDelay"] = 0;
        Parameters["Camera2TriggerStart"] = 7000;
        Parameters["Camera2TriggerDuration"] = 10;
        Parameters["RbImagingVCAAmp"] = 3.0;

        // Common and Finishing
        Parameters["MOTCoilsOffValue"] = 0.0;
        Parameters["XShimCoilsOffValue"] = 0.0;
        Parameters["YShimCoilsOffValue"] = 0.0;
        Parameters["ZShimCoilsOffValue"] = 0.0;

    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int patternStartBeforeQ = (int)Parameters["TCLBlockStart"];
        int Rb3DMOTStart = (int)Parameters["Rb3DMOTStart"];
        int Rb3DMOTStop = Rb3DMOTStart + (int)Parameters["Rb3DMOTDuration"];
        int RbCMOTStop = Rb3DMOTStop + (int)Parameters["RbCMOTRampDuration"] + (int)Parameters["RbCMOTDuration"];
        int RbMolassesRampStart = RbCMOTStop + (int)Parameters["RbMOTFieldDecayDuration"];
        int RbMolassesStart = RbMolassesRampStart + (int)Parameters["RbMolassesRampDuration"];
        int RbMolassesStop = RbMolassesStart + (int)Parameters["RbMolassesHoldDuration"];
        int RbShutterClose = RbMolassesStop - (int)Parameters["ShutterCloseDelay"];
        int odtStop = RbMolassesStop + (int)Parameters["ODTDuration"];
        int imagingLightStart = odtStop + (int)Parameters["CameraTriggerStartDelay"];
        int CameraStart = odtStop + (int)Parameters["CameraTriggerStartDelay"] - (int)Parameters["CameraExposureDelay"];
        int ThorlabsCameraStart = odtStop + (int)Parameters["CameraTriggerStartDelay"] - (int)Parameters["ThorlabsCameraExposureDelay"];
        int CameraStop = CameraStart + (int)Parameters["CameraTriggerDuration"];
        int RbShutterOpen = imagingLightStart - (int)Parameters["ShutterOpenDelay"];

        p.Pulse(patternStartBeforeQ, 0, 10, "analogPatternTrigger");
        p.EnforceTimeOrdering(false);

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["Rb2DStart"],
            (int)Parameters["Rb2DDuration"],
            "Rb2DCoolingAOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["Rb2DStart"],
            (int)Parameters["Rb2DDuration"],
            "Rb2DCoilsTTL"
        );
        if ((double)Parameters["PushBeamONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                (int)Parameters["RbPushBeamStart"], // RbMolassesStart
                (int)Parameters["RbPushBeamDuration"],
                "RbPushBeamAOM"
            );
        }

        // 3D MOT 
        p.Pulse(
            patternStartBeforeQ,
            Rb3DMOTStart,
            RbCMOTStop - Rb3DMOTStart,
            "Rb3DCoolingAOM"
        );

        if ((double)Parameters["CMOTONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                Rb3DMOTStop,
                RbCMOTStop - Rb3DMOTStop,
                "DDSTTLP0"
            );
            p.Pulse(
                patternStartBeforeQ,
                Rb3DMOTStop,
                RbCMOTStop - Rb3DMOTStop,
                "DDSTTLP1"
            );
        }

        // Rb molasses, DDS frequency 01
        if ((double)Parameters["MolassesONorOFF"] > 5.0)
        {
            p.Pulse(
            patternStartBeforeQ,
            RbMolassesRampStart,
            RbMolassesStop - RbMolassesRampStart,
            "Rb3DCoolingAOM"
        );

            p.Pulse(
                patternStartBeforeQ,
                RbMolassesRampStart,
                RbMolassesStop - RbMolassesRampStart,
                "DDSTTLP1"
            );
        }

        // odt
        if ((double)Parameters["ODTONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                0,
                odtStop,
                "DipoleTrapAOM"
            );

        }

        // if shutters being used, close them during odt

        if ((double)Parameters["ShuttersONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                RbShutterClose, // close when molasses ends
                RbShutterOpen - RbShutterClose, // open at end of pattern
                "BXShutter" //
            );
        }

        // normalisation imaging
        if ((double)Parameters["NormalisationImageONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                RbMolassesStop + (int)Parameters["NormalisationTriggerStartDelay"] - (int)Parameters["CameraExposureDelay"],
                (int)Parameters["CameraTriggerDuration"],
                "cameraTrigger"
            );
        }

        //p.Pulse(
        //    patternStartBeforeQ,
        //    RbMolassesStop + (int)Parameters["NormalisationTriggerStartDelay"],
        //    (int)Parameters["CameraTriggerDuration"],
        //    "Rb3DCoolingAOM"
        //);

        //p.Pulse(
        //    patternStartBeforeQ,
        //    RbMolassesStop + (int)Parameters["NormalisationTriggerStartDelay"],
        //    (int)Parameters["CameraTriggerDuration"],
        //    "DDSTTLP0"
        //);

        // imaging with the on-resonance light
        p.Pulse(
            patternStartBeforeQ,
            imagingLightStart,
            (int)Parameters["CameraTriggerDuration"],
            "Rb3DCoolingAOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            imagingLightStart,
            (int)Parameters["CameraTriggerDuration"],
            "DDSTTLP0"
        );

        p.Pulse(
            patternStartBeforeQ,
            CameraStart,
            (int)Parameters["CameraTriggerDuration"],
            "cameraTrigger"
        );

        p.Pulse(
            patternStartBeforeQ,
            ThorlabsCameraStart,
            (int)Parameters["CameraTriggerDuration"] + (int)Parameters["ThorlabsCameraExposureDelay"],
            "V00R0EOM"
        );

        p.Pulse(
            patternStartBeforeQ,
            (int)Parameters["Camera2TriggerStart"],
            (int)Parameters["Camera2TriggerDuration"],
            "camera2Trigger"
        );

        if ((double)Parameters["BackgroundImageONorOFF"] > 5.0)
        {
            p.Pulse(
                patternStartBeforeQ,
                (int)Parameters["BackgroundImageTime"],
                (int)Parameters["CameraTriggerDuration"],
                "Rb3DCoolingAOM"
            );
            p.Pulse(
                patternStartBeforeQ,
                (int)Parameters["BackgroundImageTime"],
                (int)Parameters["CameraTriggerDuration"],
                "DDSTTLP0"
            );
            p.Pulse(
                patternStartBeforeQ,
                (int)Parameters["BackgroundImageTime"] - (int)Parameters["CameraExposureDelay"],
                (int)Parameters["CameraTriggerDuration"],
                "cameraTrigger"
            );
        }

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("BXChirp");
        p.AddChannel("V00R0AOMVCOFreq");
        p.AddChannel("V00R0AOMVCOAmp");
        p.AddChannel("V00R1plusAOMAmp");
        p.AddChannel("V00R0EOMAmp");
        p.AddChannel("motCoils");
        p.AddChannel("ShimCoilX");
        p.AddChannel("ShimCoilY");
        p.AddChannel("ShimCoilZ");
        p.AddChannel("SlowingBField");
        p.AddChannel("ODTVVAControl");
        p.AddChannel("Rb3DCoolingAOMVCOAmp");
        p.AddChannel("Rb2DCoolingAOMVCOAmp");
        p.AddChannel("RbPushBeamAOMVCOAmp");

        int Rb3DMOTStart = (int)Parameters["Rb3DMOTStart"];
        int Rb3DMOTStop = Rb3DMOTStart + (int)Parameters["Rb3DMOTDuration"];
        int RbCMOTStop = Rb3DMOTStop + (int)Parameters["RbCMOTRampDuration"] + (int)Parameters["RbCMOTDuration"];
        int RbMolassesRampStart = RbCMOTStop + (int)Parameters["RbMOTFieldDecayDuration"];
        int RbMolassesStart = RbMolassesRampStart + (int)Parameters["RbMolassesRampDuration"];
        int RbMolassesStop = RbMolassesStart + (int)Parameters["RbMolassesHoldDuration"];
        int odtStop = RbMolassesStop + (int)Parameters["ODTDuration"];
        int imagingLightStart = odtStop + (int)Parameters["CameraTriggerStartDelay"];
        int CameraStart = odtStop + (int)Parameters["CameraTriggerStartDelay"] - (int)Parameters["CameraExposureDelay"];
        int CameraStop = CameraStart + (int)Parameters["CameraTriggerDuration"];

        // common
        p.AddAnalogValue(
            "Rb2DCoolingAOMVCOAmp",
            0,
            (double)Parameters["Rb2DCoolingVCOAmp"]
        );

        // 2D MOT
        p.AddAnalogValue(
            "RbPushBeamAOMVCOAmp",
            0,
            (double)Parameters["RbPushBeamAOMVCOAmpPush"]
        );

        p.AddAnalogValue(
            "RbPushBeamAOMVCOAmp",
            (int)Parameters["RbPushBeamDuration"],
            0.0
        );

        // 3D MOT
        p.AddAnalogValue(
            "motCoils",
            Rb3DMOTStart,
            (double)Parameters["Rb3DMOTBField"]
        );
        p.AddAnalogValue(
            "Rb3DCoolingAOMVCOAmp",
            Rb3DMOTStart,
            (double)Parameters["Rb3DMOTVCAAmp"]
        );
        p.AddAnalogValue(
            "ShimCoilX",
            Rb3DMOTStart,
            (double)Parameters["XShimCoilsMOTValue"]
        );
        p.AddAnalogValue(
            "ShimCoilY",
            Rb3DMOTStart,
            (double)Parameters["YShimCoilsMOTValue"]
        );
        /*p.AddAnalogValue(
            "ShimCoilZ",
            Rb3DMOTStart,
            (double)Parameters["ZShimCoilsMOTValue"]
        );*/

        // CMOT
        if ((double)Parameters["CMOTONorOFF"] > 5.0)
        {
            p.AddLinearRamp(
                "motCoils",
                Rb3DMOTStop,
                (int)Parameters["RbCMOTRampDuration"],
                (double)Parameters["RbCMOTBField"]
            );
            p.AddAnalogValue(
                "Rb3DCoolingAOMVCOAmp",
                Rb3DMOTStop,
                (double)Parameters["RbCMOTVCAAmp"]
            );
            p.AddAnalogValue(
                "ShimCoilX",
                Rb3DMOTStop,
                (double)Parameters["XShimCoilsCMOTValue"]
            );
            p.AddAnalogValue(
                "ShimCoilY",
                Rb3DMOTStop,
                (double)Parameters["YShimCoilsCMOTValue"]
            );
            /*p.AddAnalogValue(
                "ShimCoilZ",
                Rb3DMOTStop,
                (double)Parameters["ZShimCoilsCMOTValue"]
            );*/
        }

        // molasses
        p.AddAnalogValue(
            "motCoils",
            RbCMOTStop,
            0.0
        );
        p.AddAnalogValue(
            "Rb3DCoolingAOMVCOAmp",
            RbCMOTStop,
            (double)Parameters["RbMolassesVCAAmpStart"]
        );
        p.AddLinearRamp(
            "Rb3DCoolingAOMVCOAmp",
            RbMolassesRampStart,
            (int)Parameters["RbMolassesRampDuration"],
            (double)Parameters["RbMolassesVCAAmpEnd"]
        );

        p.AddLinearRamp(
            "ShimCoilX",
            RbCMOTStop + (int)Parameters["RbMolassesShimFieldDelay"],
            (int)Parameters["RbMolassesShimFieldRampDuration"],
            (double)Parameters["XShimCoilsMolassesValue"]
        );
        p.AddLinearRamp(
            "ShimCoilY",
            RbCMOTStop + (int)Parameters["RbMolassesShimFieldDelay"],
            (int)Parameters["RbMolassesShimFieldRampDuration"],
            (double)Parameters["YShimCoilsMolassesValue"]
        );
        p.AddLinearRamp(
            "ShimCoilZ",
            RbCMOTStop + (int)Parameters["RbMolassesShimFieldDelay"],
            (int)Parameters["RbMolassesShimFieldRampDuration"],
            (double)Parameters["ZShimCoilsMolassesValue"]
        );

/*
        // ODT on
        p.AddAnalogValue(
            "ShimCoilZ",
            0,
           (double)Parameters["ODTVVAOnValue"]
        );*/

        // PARAMETRIC HEATING
        
        double normfrequency = (double)Parameters["ODTHeatingFrequency"] * 10e-6; // assumes 1 timestep is 10 us

        int numberCycles = (int)Parameters["ODTHeatingCycles"];
        int heatingDuration = (int)(numberCycles / normfrequency); //(int)Parameters["ODTHeatingDuration"];
        // values for sine curve
        int odtDuration = (int)Parameters["ODTDuration"];
        int heatingStart = RbMolassesStop + odtDuration - heatingDuration; //(int)Parameters["ODTHeatingStart"];
        int heatingStop = heatingStart + heatingDuration;
        int steps = heatingDuration; // just duration because duration is already given in number of time steps
        double amplitude = (double)Parameters["ODTHeatingAmp"];
        double offset = (double)Parameters["ODTHeatingOffset"];
        double phase = (double)Parameters["ODTHeatingPhase"];

        // values for conversion to control voltage
        double a = (double)Parameters["ControlVoltageToPowerA"];
        double b = (double)Parameters["ControlVoltageToPowerB"];
        double c = (double)Parameters["ControlVoltageToPowerC"];

        // calculate control voltage for offset value
        // double offsetVoltage = a * Math.Log(Math.Abs(offset - b)) + c;
        double offsetVoltage = a * Math.Sqrt(Math.Abs(offset - b)) + c;

        // ODT on
        p.AddAnalogValue(
        "ODTVVAControl",
        0,
        offsetVoltage
        );

        if ((double)Parameters["ParametricHeatingONorOFF"] > 5.0)
        {

            // produce sine curve
            double[] sineArray = new double[steps]; // initialise a list
            for (int i = 0; i < steps - 1; i++)
            {
                double t = 1.0 * steps;
                double it = 1.0 * i;

                // calculate sine value at particular time step
                double value = amplitude * Math.Sin(2 * Math.PI * normfrequency * it + phase) + offset;

                // convert to control voltage
                double voltage = a * Math.Log(Math.Abs(value - b)) + c;

                // add to sine array
                sineArray[i] = voltage;
            }

            // output wave form
            p.AddArbitrary("ODTVVAControl", heatingStart, sineArray, 10.0, 0.0);
        
            p.AddAnalogValue(
                "ODTVVAControl",
                heatingStop,
               offsetVoltage
            );
        }

        p.AddAnalogValue(
            "Rb3DCoolingAOMVCOAmp",
            RbMolassesStop,
            (double)Parameters["RbImagingVCAAmp"]
        );

        // BG image and reset
        p.AddAnalogValue(
            "Rb3DCoolingAOMVCOAmp",
            (int)Parameters["BackgroundImageTime"] + (int)Parameters["CameraTriggerDuration"] + 100,
            3.0
        );
        p.AddAnalogValue(
            "ShimCoilX",
            (int)Parameters["BackgroundImageTime"],
            (double)Parameters["XShimCoilsOffValue"]
        );
        p.AddAnalogValue(
            "ShimCoilY",
            (int)Parameters["BackgroundImageTime"],
            (double)Parameters["YShimCoilsOffValue"]
        );
        p.AddAnalogValue(
            "ShimCoilZ",
            (int)Parameters["BackgroundImageTime"],
            (double)Parameters["ZShimCoilsOffValue"]
        );

        p.AddAnalogValue(
            "motCoils",
            CameraStop + 1000,
            (double)Parameters["MOTCoilsOffValue"]
        );

        // ODT off
        p.AddAnalogValue("ShimCoilZ",
            (int)Parameters["BackgroundImageTime"] + (int)Parameters["CameraTriggerDuration"],
            (double)Parameters["ODTVVAOffValue"]
        );

        return p;
    }

    public override AnalogStaticBuilder GetAnalogStatic()
    {
        AnalogStaticBuilder p = new AnalogStaticBuilder((int)Parameters["PatternLength"]);

        return p;
    }
}
