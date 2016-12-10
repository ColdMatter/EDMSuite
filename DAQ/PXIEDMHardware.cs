using System;
using System.Collections;
using System.Runtime.Remoting;
using NationalInstruments.DAQmx;

using DAQ.Pattern;
using System.Collections.Generic;
using DAQ.TransferCavityLock2012;
using DAQ.Remoting;

namespace DAQ.HAL
{
    /// <summary>
    /// This is the specific hardware that the edm machine has. This class conforms
    /// to the Hardware interface.
    /// </summary>
    public class PXIEDMHardware : DAQ.HAL.Hardware
    {
                public override void ConnectApplications()
       {
           //RemotingHelper.ConnectEDMHardwareControl();
           //RemotingHelper.ConnectPhaseLock();
           //Type t = Type.GetType("EDMHardwareControl.Controller, EDMHardwareControl");
          // Type t = Type.GetType("MarshalByRefObject"); 
                  // ask the remoting system for access to TCL2012
          // Type t = Type.GetType("TransferCavityLock2012.Controller, TransferCavityLock");
         //RemotingConfiguration.RegisterWellKnownClientType(t, "tcp://localhost:1172/controller.rem");
       }
 

        public PXIEDMHardware()
        {

            // add the boards
            Boards.Add("daq", "/PXI1Slot18");
            Boards.Add("pg", "/PXI1Slot10");
            Boards.Add("counter", "/PXI1Slot3");
            Boards.Add("aoBoard", "/PXI1Slot4");
            // this drives the rf attenuators
            Boards.Add("usbDAQ1", "/Dev6");
            Boards.Add("analogIn", "/PXI1Slot2");
            Boards.Add("usbDAQ2", "/Dev1");
            Boards.Add("usbDAQ3", "/Dev2");
            Boards.Add("usbDAQ4", "/Dev5");
            Boards.Add("tclBoardPump", "/PXI1Slot17");
            Boards.Add("tclBoardProbe", "/PXI1Slot9");
            string pgBoard = (string)Boards["pg"];
            string daqBoard = (string)Boards["daq"];
            string counterBoard = (string)Boards["counter"];
            string aoBoard = (string)Boards["aoBoard"];
            string usbDAQ1 = (string)Boards["usbDAQ1"];
            string analogIn = (string)Boards["analogIn"];
            string usbDAQ2 = (string)Boards["usbDAQ2"];
            string usbDAQ3 = (string)Boards["usbDAQ3"];
            string usbDAQ4 = (string)Boards["usbDAQ4"];
            string tclBoardPump = (string)Boards["tclBoardPump"];
            string tclBoardProbe = (string)Boards["tclBoardProbe"];

            // add things to the info
            // the analog triggersf
            Info.Add("analogTrigger0", (string)Boards["analogIn"] + "/PFI0");
            Info.Add("analogTrigger1", (string)Boards["analogIn"] + "/PFI1");

            Info.Add("sourceToDetect", 1.3);
            Info.Add("moleculeMass", 193.0);
            Info.Add("machineLengthRatio", 3.842);
            Info.Add("defaultGate",new double[] {2190, 80});


            Info.Add("phaseLockControlMethod", "synth");
            Info.Add("PGClockLine", pgBoard + "/PFI4"); //Mapped to PFI2 on 6533 connector
            Info.Add("PatternGeneratorBoard", pgBoard);
            Info.Add("PGType", "dedicated");
            // rf counter switch control seq``
            Info.Add("IodineFreqMon", new bool[] { false, false }); // IN 1
            Info.Add("pumpAOMFreqMon", new bool[] { false, true }); // IN 2
            Info.Add("FLModulationFreqMon", new bool[] { true, false }); // IN 3

            Info.Add("PGTrigger", pgBoard + "/PFI5"); //Mapped to PFI7 on 6533 connector

            // YAG laser
            yag = new BrilliantLaser("ASRL9::INSTR");

            // add the GPIB/RS232 instruments
            Instruments.Add("green", new HP8657ASynth("GPIB0::7::INSTR"));
            Instruments.Add("gigatronix", new Gigatronics7100Synth("GPIB0::19::INSTR"));
            Instruments.Add("red", new HP3325BSynth("GPIB0::12::INSTR"));
            Instruments.Add("4861", new ICS4861A("GPIB0::4::INSTR"));
            Instruments.Add("bCurrentMeter", new HP34401A("GPIB0::22::INSTR"));
            Instruments.Add("rfCounter", new Agilent53131A("GPIB0::3::INSTR"));
            //Instruments.Add("rfCounter2", new Agilent53131A("GPIB0::5::INSTR"));
            Instruments.Add("rfPower", new HP438A("GPIB0::13::INSTR"));
            Instruments.Add("BfieldController", new SerialDAQ("ASRL7::INSTR"));
            Instruments.Add("rfCounter2", new SerialAgilent53131A("ASRL14::INSTR"));
            Instruments.Add("probePolControl", new SerialMotorControllerBCD("ASRL8::INSTR"));
            Instruments.Add("pumpPolControl", new SerialMotorControllerBCD("ASRL11::INSTR"));


            // map the digital channels
            // these channels are generally switched by the pattern generator
            // they're all in the lower half of the pg
            AddDigitalOutputChannel("valve", pgBoard, 0, 0);
            AddDigitalOutputChannel("flash", pgBoard, 0, 1);
            AddDigitalOutputChannel("q", pgBoard, 0, 2);
            AddDigitalOutputChannel("detector", pgBoard, 0, 3);
            AddDigitalOutputChannel("detectorprime", pgBoard, 1, 2); // this trigger is for switch scanning
            // see ModulatedAnalogShotGatherer.cs
            // for details.
            AddDigitalOutputChannel("rfSwitch", pgBoard, 0, 4);
            AddDigitalOutputChannel("pumprfSwitch", pgBoard, 3, 4);
            AddDigitalOutputChannel("fmSelect", pgBoard, 1, 0);      // This line selects which fm voltage is
            // sent to the synth.
            AddDigitalOutputChannel("attenuatorSelect", pgBoard, 0, 5);    // This line selects the attenuator voltage
            // sent to the voltage-controlled attenuator.
            AddDigitalOutputChannel("piFlip", pgBoard, 1, 1);
            AddDigitalOutputChannel("ttlSwitch", pgBoard, 1, 3);	// This is the output that the pg
            // will switch if it's switch scanning.
            AddDigitalOutputChannel("scramblerEnable", pgBoard, 1, 4);
            
            //RF Counter Control (single pole 4 throw)
            //AddDigitalOutputChannel("rfCountSwBit1", pgBoard, 3, 5);
            //AddDigitalOutputChannel("rfCountSwBit2", pgBoard, 3, 6);
            
            // new rf amp blanking
            AddDigitalOutputChannel("rfAmpBlanking", pgBoard, 1, 5);

            // these channel are usually software switched - they should not be in
            // the lower half of the pattern generator
            AddDigitalOutputChannel("b", pgBoard, 2, 0);
            AddDigitalOutputChannel("notB", pgBoard, 2, 1);
            AddDigitalOutputChannel("db", pgBoard, 2, 2);
            AddDigitalOutputChannel("notDB", pgBoard, 2, 3);
            //			AddDigitalOutputChannel("notEOnOff", pgBoard, 2, 4);  // this line seems to be broken on our pg board
            // 			AddDigitalOutputChannel("eOnOff", pgBoard, 2, 5);  // this and the above are not used now we have analog E control
            
            AddDigitalOutputChannel("ePol", pgBoard, 2, 6);
            AddDigitalOutputChannel("notEPol", pgBoard, 2, 7);
            AddDigitalOutputChannel("eBleed", pgBoard, 3, 0);
            AddDigitalOutputChannel("eSwitching", aoBoard, 0, 3);
            AddDigitalOutputChannel("piFlipEnable", pgBoard, 3, 1);
            AddDigitalOutputChannel("notPIFlipEnable", pgBoard, 3, 5);
            AddDigitalOutputChannel("mwEnable", pgBoard, 3, 3);
            AddDigitalOutputChannel("mwSelectPumpChannel", pgBoard, 3, 6);
            AddDigitalOutputChannel("mwSelectTopProbeChannel", pgBoard, 3, 2);
            AddDigitalOutputChannel("mwSelectBottomProbeChannel", pgBoard, 2, 4);
            
            // these digitial outputs are are not switched during the pattern
            AddDigitalOutputChannel("argonShutter", aoBoard, 0, 0);
            AddDigitalOutputChannel("patternTTL", aoBoard, 0, 7);
            AddDigitalOutputChannel("rfPowerAndFreqSelectSwitch", aoBoard, 0, 1);
            AddDigitalOutputChannel("targetStepper", aoBoard, 0, 2); ;


            // map the analog channels
            // These channels are on the daq board. Used mainly for diagnostic purposes.
            AddAnalogInputChannel("iodine", daqBoard + "/ai2", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("cavity", daqBoard + "/ai3", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("probePD", daqBoard + "/ai4", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("pumpPD", daqBoard + "/ai5", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("northLeakage", daqBoard + "/ai6", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("southLeakage", daqBoard + "/ai7", AITerminalConfiguration.Nrse);
            //AddAnalogInputChannel("northLeakage", usbDAQ4 + "/ai0", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("southLeakage", usbDAQ4 + "/ai1", AITerminalConfiguration.Rse);

            // Used ai13,11 & 12 over 6,7 & 8 for miniFluxgates, because ai8, 9 have an isolated ground. 
            AddAnalogInputChannel("miniFlux1", daqBoard + "/ai13", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("miniFlux2", daqBoard + "/ai11", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("miniFlux3", daqBoard + "/ai12", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("ground", daqBoard + "/ai14", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("piMonitor", daqBoard + "/ai10", AITerminalConfiguration.Nrse);
            //AddAnalogInputChannel("diodeLaserRefCavity", daqBoard + "/ai13", AITerminalConfiguration.Nrse);
            // Don't use ai10, cross talk with other channels on this line

            // high quality analog inputs (will be) on the S-series analog in board
            // The last number in AddAnalogInputChannel is an optional calibration which turns VuS and MHz 
            AddAnalogInputChannel("topProbe", analogIn + "/ai0", AITerminalConfiguration.Differential, 0.1);
            AddAnalogInputChannel("bottomProbe", analogIn + "/ai1", AITerminalConfiguration.Differential, 0.02);
            AddAnalogInputChannel("magnetometer", analogIn + "/ai2", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("gnd", analogIn + "/ai3", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("battery", analogIn + "/ai4", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("piMonitor", analogIn + "/ai5", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("bFieldCurrentMonitor", analogIn + "/ai6", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("reflectedrf1Amplitude", analogIn + "/ai5", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("reflectedrf2Amplitude", analogIn + "/ai6", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("rfCurrent", analogIn + "/ai7 ", AITerminalConfiguration.Differential);

            AddAnalogOutputChannel("phaseScramblerVoltage", aoBoard + "/ao10");
            AddAnalogOutputChannel("b", aoBoard + "/ao2");


            // rf rack control
            //AddAnalogInputChannel("rfPower", usbDAQ1 + "/ai0", AITerminalConfiguration.Rse);

            AddAnalogOutputChannel("rf1Attenuator", usbDAQ1 + "/ao0", 0, 5);
            AddAnalogOutputChannel("rf2Attenuator", usbDAQ1 + "/ao1", 0, 5);
            AddAnalogOutputChannel("rf1FM", usbDAQ2 + "/ao0", 0, 5);
            AddAnalogOutputChannel("rf2FM", usbDAQ2 + "/ao1", 0, 5);

            // E field control and monitoring
            //AddAnalogInputChannel("cPlusMonitor", usbDAQ3 + "/ai1", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("cMinusMonitor", usbDAQ3 + "/ai2", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("cPlusMonitor", daqBoard + "/ai0", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("cMinusMonitor", daqBoard + "/ai1", AITerminalConfiguration.Differential);

            AddAnalogOutputChannel("cPlus", usbDAQ3 + "/ao0", 0, 10);
            AddAnalogOutputChannel("cMinus", usbDAQ3 + "/ao1", 0, 10);


            
            // B field control
            //AddAnalogOutputChannel("steppingBBias", usbDAQ4 + "/ao0", 0, 5);


            // map the counter channels
            AddCounterChannel("phaseLockOscillator", counterBoard + "/ctr7");
            AddCounterChannel("phaseLockReference", counterBoard + "/pfi10");
            //AddCounterChannel("northLeakage", counterBoard + "/ctr0");
            //AddCounterChannel("southLeakage", counterBoard + "/ctr1");


            // Cavity inputs for the cavity that controls the Pump lasers
            AddAnalogInputChannel("PumpCavityRampVoltage", tclBoardPump + "/ai8", AITerminalConfiguration.Rse); //tick
            AddAnalogInputChannel("Pumpmaster", tclBoardPump + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("Pumpp1", tclBoardPump + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("Pumpp2", tclBoardPump + "/ai3", AITerminalConfiguration.Rse); 

            // Lasers locked to pump cavity
            AddAnalogOutputChannel("899ExternalScan", tclBoardPump + "/ao2", -5, 5); //tick
            AddAnalogOutputChannel("MenloPZT", tclBoardPump + "/ao0", 0, 10); //tick

            // Length stabilisation for pump cavity
            AddAnalogOutputChannel("PumpCavityLengthVoltage", tclBoardPump + "/ao1", -10, 10); //tick

            //TCL configuration for pump cavity
            TCLConfig tcl1 = new TCLConfig("Pump Cavity");
            tcl1.AddLaser("MenloPZT", "Pumpp1");
            tcl1.AddLaser("899ExternalScan", "Pumpp2");
            tcl1.Trigger = tclBoardPump + "/PFI0";
            tcl1.Cavity = "PumpCavityRampVoltage";
            tcl1.MasterLaser = "Pumpmaster";
            tcl1.Ramp = "PumpCavityLengthVoltage";
            tcl1.TCPChannel = 1190;
            tcl1.AnalogSampleRate = 61250;
            tcl1.DefaultScanPoints = 500;
            tcl1.MaximumNLMFSteps = 20;
            tcl1.PointsToConsiderEitherSideOfPeakInFWHMs = 2.5;
            tcl1.TriggerOnRisingEdge = false;
            tcl1.AddFSRCalibration("MenloPZT", 3.84);
            tcl1.AddFSRCalibration("899ExternalScan", 3.84);
            tcl1.AddDefaultGain("Master", 0.3);
            tcl1.AddDefaultGain("MenloPZT", -0.2);
            tcl1.AddDefaultGain("899ExternalScan", 4);
            Info.Add("PumpCavity", tcl1);
            Info.Add("DefaultCavity", tcl1);

            // Probe cavity inputs
            AddAnalogInputChannel("ProbeRampVoltage", tclBoardProbe + "/ai0", AITerminalConfiguration.Rse); //tick
            AddAnalogInputChannel("Probemaster", tclBoardProbe + "/ai1", AITerminalConfiguration.Rse); //tick
            AddAnalogInputChannel("Probep1", tclBoardProbe + "/ai2", AITerminalConfiguration.Rse); //tick

            // Lasers locked to Probe cavity
            AddAnalogOutputChannel("TopticaSHGPZT", tclBoardProbe + "/ao0", -4, 4); //tick
            AddAnalogOutputChannel("ProbeCavityLengthVoltage", tclBoardProbe + "/ao1", -10, 10); //tick

            // TCL configuration for Probe cavity
            TCLConfig tcl2 = new TCLConfig("Probe Cavity");
            tcl2.AddLaser("TopticaSHGPZT", "Probep1");
            tcl2.Trigger = tclBoardProbe + "/PFI0";
            tcl2.Cavity = "ProbeRampVoltage";
            tcl2.MasterLaser = "Probemaster";
            tcl2.Ramp = "ProbeCavityLengthVoltage";
            tcl2.TCPChannel = 1191;
            tcl2.AnalogSampleRate = 61250/2;
            tcl2.DefaultScanPoints = 250;
            tcl2.MaximumNLMFSteps = 20;
            tcl2.PointsToConsiderEitherSideOfPeakInFWHMs = 6;
            tcl2.AddFSRCalibration("TopticaSHGPZT", 3.84);
            tcl2.TriggerOnRisingEdge = false;
            tcl2.AddDefaultGain("Master", 0.4);
            tcl2.AddDefaultGain("TopticaSHGPZT", 0.04);
            Info.Add("ProbeCavity", tcl2);
            //Info.Add("DefaultCavity", tcl2);


            // Obsolete Laser control
            AddAnalogOutputChannel("probeAOM", aoBoard + "/ao19", 0, 10);
            AddAnalogOutputChannel("pumpAOM", aoBoard + "/ao20", 0, 10);
            AddAnalogOutputChannel("fibreAmpPwr", aoBoard + "/ao3");
            AddAnalogOutputChannel("I2LockBias", aoBoard + "/ao5", 0, 5);

            //Microwave Control Channels
            AddAnalogOutputChannel("uWaveDCFM", aoBoard + "/ao11", -2.5, 2.5);
            //AddAnalogOutputChannel("uWaveMixerV", aoBoard + "/ao12", 0, 10);
            AddAnalogOutputChannel("pumpMixerV", aoBoard + "/ao19", 0, 5);
            AddAnalogOutputChannel("bottomProbeMixerV", aoBoard + "/ao24", 0, 5);
            AddAnalogOutputChannel("topProbeMixerV", aoBoard + "/ao25", 0, 5);



            //RF control Channels
            AddAnalogOutputChannel("VCO161Amp", aoBoard + "/ao13", 0, 10);
            AddAnalogOutputChannel("VCO161Freq", aoBoard + "/ao14", 0, 10);
            AddAnalogOutputChannel("VCO30Amp", aoBoard + "/ao15", 0, 10);
            AddAnalogOutputChannel("VCO30Freq", aoBoard + "/ao16", 0, 10);
            AddAnalogOutputChannel("VCO155Amp", aoBoard + "/ao17", 0, 10);
            AddAnalogOutputChannel("VCO155Freq", aoBoard + "/ao18", 0, 10);

        }

    }
}