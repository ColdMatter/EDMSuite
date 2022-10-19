using System;
using System.Collections;
using System.Runtime.Remoting;
using NationalInstruments.DAQmx;
using DAQ.WavemeterLock;
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
            Boards.Add("doBoard", "/PXI1Slot11");
            Boards.Add("analogIn2", "/PXI1Slot17");
            Boards.Add("counter", "/PXI1Slot16");
            Boards.Add("aoBoard", "/PXI1Slot2");
            Boards.Add("usbDAQ1", "/Dev6");         // this is for the magnetic field feedback
            Boards.Add("analogIn", "/PXI1Slot15");
            //Boards.Add("usbDAQ2", "/Dev4");
            Boards.Add("usbDAQ3", "/Dev1");
            Boards.Add("usbDAQ4", "/Dev3");

            //Boards.Add("tclBoardPump", "/PXI1Slot17");
            //Boards.Add("tclBoardProbe", "/PXI1Slot9");
            string rfAWG = (string)Boards["rfAWG"];
            string pgBoard = (string)Boards["pg"];
            string daqBoard = (string)Boards["daq"];
            string analogIn2 = (string)Boards["analogIn2"];
            string counterBoard = (string)Boards["counter"];
            string aoBoard = (string)Boards["aoBoard"];
            string usbDAQ1 = (string)Boards["usbDAQ1"];
            string analogIn = (string)Boards["analogIn"];
            //string usbDAQ2 = (string)Boards["usbDAQ2"];
            string usbDAQ3 = (string)Boards["usbDAQ3"];
            string usbDAQ4 = (string)Boards["usbDAQ4"];
            string doBoard = (string)Boards["doBoard"];
            //string tclBoardPump = (string)Boards["tclBoardPump"];
            //string tclBoardProbe = (string)Boards["tclBoardProbe"];

            // add things to the info
            // the analog triggers
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

            Info.Add("PGTriggerLine", pgBoard + "/PFI5"); //Mapped to PFI7 on 6533 connector

            // YAG laser
            yag = new BrilliantLaser("ASRL13::INSTR");

            // add the GPIB/RS232/USB instruments
            Instruments.Add("green", new HP8657ASynth("GPIB0::7::INSTR"));
            //Instruments.Add("gigatronix", new Gigatronics7100Synth("GPIB0::19::INSTR"));
            Instruments.Add("red", new HP3325BSynth("ASRL12::INSTR"));
            Instruments.Add("4861", new ICS4861A("GPIB0::4::INSTR"));
            Instruments.Add("bCurrentMeter", new HP34401A("GPIB0::12::INSTR"));
            Instruments.Add("rfCounter", new Agilent53131A("GPIB0::3::INSTR"));
            //Instruments.Add("rfCounter2", new Agilent53131A("GPIB0::5::INSTR"));
            Instruments.Add("rfPower", new HP438A("GPIB0::13::INSTR"));
            Instruments.Add("BfieldController", new SerialDAQ("ASRL19::INSTR"));
            Instruments.Add("rfCounter2", new SerialAgilent53131A("ASRL17::INSTR"));
            Instruments.Add("probePolControl", new SerialMotorControllerBCD("ASRL8::INSTR"));
            Instruments.Add("pumpPolControl", new SerialMotorControllerBCD("ASRL11::INSTR"));
            Instruments.Add("anapico", new AnapicoSynth("USB0::1003::45055::321-028100000-0168::0::INSTR"));//old anapico 1 channel
            Instruments.Add("anapicoSYN420", new AnapicoSynth("USB0::0x03EB::0xAFFF::322-03A100005-0539::INSTR"));// new 2 channel anapico
            Instruments.Add("rfAWG", new NIPXI5670("PXI1Slot4"));

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
            AddDigitalOutputChannel("fmSelect", pgBoard, 1, 0);      // This line selects which fm voltage is
            // sent to the synth.
            AddDigitalOutputChannel("attenuatorSelect", pgBoard, 0, 5);    // This line selects the attenuator voltage
            // sent to the voltage-controlled attenuator.
            AddDigitalOutputChannel("piFlip", pgBoard, 1, 1);
            AddDigitalOutputChannel("bSwitch", pgBoard, 1, 3);
            AddDigitalOutputChannel("ttlSwitch", pgBoard, 3, 5);	// This is the output that the pg board outputs if we are switch-scanning
            AddDigitalOutputChannel("scramblerEnable", pgBoard, 1, 4);
            
            //RF Counter Control (single pole 4 throw)
            //AddDigitalOutputChannel("rfCountSwBit1", pgBoard, 3, 5);
            //AddDigitalOutputChannel("rfCountSwBit2", pgBoard, 3, 6);
            
            // new rf amp blanking
            AddDigitalOutputChannel("rfAmpBlanking", pgBoard, 1, 5);
            AddDigitalOutputChannel("mwEnable", pgBoard, 3, 3);
            AddDigitalOutputChannel("mwSelectPumpChannel", pgBoard, 3, 6);
            AddDigitalOutputChannel("mwSelectTopProbeChannel", pgBoard, 3, 2);
            AddDigitalOutputChannel("mwSelectBottomProbeChannel", pgBoard, 2, 4);
            AddDigitalOutputChannel("pumprfSwitch", pgBoard, 3, 4);
            
            // rf awg test
            AddDigitalOutputChannel("rfAWGTestTrigger", doBoard, 0, 1);

            // these channel are usually software switched - they are on the AO board
            AddDigitalOutputChannel("b", aoBoard, 0, 0);
            AddDigitalOutputChannel("notB", aoBoard, 0, 1);

            AddDigitalOutputChannel("db", aoBoard, 0, 2);
            AddDigitalOutputChannel("notDB", aoBoard, 0, 3);                                                        
            AddDigitalOutputChannel("piFlipEnable", aoBoard, 0, 4);
            AddDigitalOutputChannel("notPIFlipEnable", aoBoard, 0, 5); //not connected to anything
            AddDigitalOutputChannel("mwSwitching", aoBoard, 0, 6);

            // these digitial outputs are switched slowly during the pattern
            AddDigitalOutputChannel("ePol", usbDAQ4, 0, 4);
            AddDigitalOutputChannel("notEPol", usbDAQ4, 0, 5);
            AddDigitalOutputChannel("eBleed", usbDAQ4, 0, 6);
            AddDigitalOutputChannel("eSwitching", usbDAQ4, 0, 7);
            AddDigitalOutputChannel("eSwitch", usbDAQ4, 0, 0);


            // these digitial outputs are are not switched during the pattern
            //AddDigitalOutputChannel("argonShutter", usbDAQ4, 0, 0);
            AddDigitalOutputChannel("patternTTL", usbDAQ4, 0, 2);
            AddDigitalOutputChannel("rfPowerAndFreqSelectSwitch", usbDAQ4, 0, 3);
            AddDigitalOutputChannel("targetStepper", usbDAQ4, 0, 1); ;

            // for test shield measurement July 2021
            AddDigitalOutputChannel("testPlateVoltageGate", doBoard, 0, 1);
            AddDigitalOutputChannel("testPlateVoltageTTL", doBoard, 0, 2);


            // map the analog channels
            // These channels are on the daq board. Used mainly for diagnostic purposes.
            AddAnalogInputChannel("iodine", daqBoard + "/ai2", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("valveMonVoltage", daqBoard + "/ai4", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("cavity", daqBoard + "/ai3", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("topPD", daqBoard + "/ai3", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("bottomPD", daqBoard + "/ai5", AITerminalConfiguration.Nrse);
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
            AddAnalogInputChannel("reflectedrfAmplitude", analogIn + "/ai5", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("incidentrfAmplitude", analogIn + "/ai6", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("rfCurrent", analogIn + "/ai7 ", AITerminalConfiguration.Differential);

            //AddAnalogInputChannel("quSpinFU_Y", analogIn + "/ai6", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("quSpinFU_Z", analogIn + "/ai5", AITerminalConfiguration.Differential);

            AddAnalogInputChannel("middlePenningGauge", daqBoard + "/ai15", AITerminalConfiguration.Rse); //nothing is connected here; only here bc hardware controller needs it to build

            //temp inputs used for magnetic noise diagnosis in test shield, cables are labelled "00EW"
            //AddAnalogInputChannel("quSpinFS_Y", analogIn + "/ai5", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("quSpinFS_Z", analogIn + "/ai6", AITerminalConfiguration.Differential);
            //mag inputs for quspins inside the chamber
            //AddAnalogInputChannel("quSpinFV_Y", analogIn2 + "/ai2", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("quSpinFV_Z", analogIn2 + "/ai3", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinEW_Y", analogIn2 + "/ai2", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinEW_Z", analogIn2 + "/ai3", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinB0_Y", analogIn2 + "/ai4", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinB0_Z", analogIn2 + "/ai5", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("quSpinEX_Y", analogIn2 + "/ai4", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("quSpinEX_Z", analogIn2 + "/ai5", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinEV_Y", analogIn2 + "/ai6", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinEV_Z", analogIn2 + "/ai7", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("battery2", analogIn2 + "/ai7", AITerminalConfiguration.Differential);//temp move to have battery on both DAQs to monitor

            
            //AddAnalogInputChannel("Bart200_Z", analogIn + "/ai5", AITerminalConfiguration.Differential);//this was stolen from penning gauge monitor
            //AddAnalogInputChannel("Bart200_X", daqBoard + "/ai3", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("Bart200_Y", daqBoard + "/ai15", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("Bart200_Z", daqBoard + "/ai5", AITerminalConfiguration.Rse);//stolen from pump and probe mon pds, which are legacy so can be replaced anyway

            //This analog input is broken, we assign this as a dummy so we don't break the rest of the code
            AddAnalogInputChannel("laserPowerMeter", analogIn2 + "/ai0", AITerminalConfiguration.Differential);
            
            AddAnalogOutputChannel("piFlipVoltage", aoBoard + "/ao20");
            AddAnalogOutputChannel("phaseScramblerVoltage", aoBoard + "/ao10");
            AddAnalogOutputChannel("bScan", aoBoard + "/ao2");

            //Coherent 899 dye laser ctrl voltage
            AddAnalogOutputChannel("Coherent899ControlVoltage", aoBoard + "/ao12", -10, 10);

            // B field control
            //AddAnalogOutputChannel("steppingBBias", usbDAQ4 + "/ao0", 0, 5);
            // Add B field stepping box bias analog output to this board for now (B field controller is not connected to anything!)
            AddAnalogOutputChannel("steppingBBias", aoBoard + "/ao8", -10, 10);

            // rf rack control
            //AddAnalogInputChannel("rfPower", usbDAQ1 + "/ai0", AITerminalConfiguration.Rse);

            AddAnalogOutputChannel("rf1Attenuator", aoBoard + "/ao26", 0, 5);
            AddAnalogOutputChannel("rf2Attenuator", aoBoard + "/ao27", 0, 5);
            AddAnalogOutputChannel("rf1FM", aoBoard + "/ao21", -5, 5);
            AddAnalogOutputChannel("rf2FM", aoBoard + "/ao22", -5, 5);

            //Source control
            AddAnalogOutputChannel("valveCtrlVoltage", aoBoard+"/ao14", 0,8);

            //ECDL piezo control
            AddAnalogOutputChannel("blueECDLPiezoVoltage", aoBoard + "/ao30", 0, 10);
            WavemeterLockConfig wmlConfig = new WavemeterLockConfig("WMLServer");
            wmlConfig.AddSlaveLaser("BlueECDL", "blueECDLPiezoVoltage", 3);//name, analog, wavemeter channel

            Info.Add("WMLServer", wmlConfig);

            // E field control and monitoring
            AddAnalogInputChannel("cPlusMonitor", usbDAQ3 + "/ai1", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("cMinusMonitor", usbDAQ3 + "/ai2", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("cPlusMonitor", daqBoard + "/ai0", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("cMinusMonitor", daqBoard + "/ai1", AITerminalConfiguration.Differential);

            AddAnalogOutputChannel("cPlus", usbDAQ3 + "/ao1", 0, 10);
            AddAnalogOutputChannel("cMinus", usbDAQ3 + "/ao0", 0, 10); //Use these two lines for the applied kilovolts supply which provides 1kV/V 

            //AddAnalogOutputChannel("cPlus", usbDAQ3 + "/ao1", 0,3.5);//these last two are for use with the bertan HV supply which requires 0 to -5V control voltage for 0 to +15kV output on the positive box
            //AddAnalogOutputChannel("cMinus", usbDAQ3 + "/ao0", -3.5,0);

            //Degauss output
            AddAnalogOutputChannel("DegaussCoil1", aoBoard + "/ao31", -10, 10);

            //Magnetic field feedback
            AddAnalogInputChannel("feedbackMagnetometer", analogIn2 + "/ai1", AITerminalConfiguration.Differential);
            //AddAnalogOutputChannel("feedbackCoils", aoBoard + "/ao31", -10, 10);

            // map the counter channels
            AddCounterChannel("phaseLockOscillator", counterBoard + "/ctr7");
            AddCounterChannel("phaseLockReference", counterBoard + "/pfi10");
            //AddCounterChannel("northLeakage", counterBoard + "/ctr0");
            //AddCounterChannel("southLeakage", counterBoard + "/ctr1");

            // magnetic field feedback
            AddAnalogInputChannel("bFieldFeedbackInput", usbDAQ1 + "/ai1", AITerminalConfiguration.Differential);
            AddAnalogOutputChannel("bFieldFeedbackOutput", usbDAQ1 + "/ao1", 0, 5);
            AddCounterChannel("bFieldFeedbackClock", usbDAQ1 + "/pfi0");


            // Cavity inputs for the cavity that controls the Pump lasers
            //AddAnalogInputChannel("PumpCavityRampVoltage", tclBoardPump + "/ai8", AITerminalConfiguration.Rse); //tick
            //AddAnalogInputChannel("Pumpmaster", tclBoardPump + "/ai1", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("Pumpp1", tclBoardPump + "/ai2", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("Pumpp2", tclBoardPump + "/ai3", AITerminalConfiguration.Rse); 

            // Lasers locked to pump cavity
            //AddAnalogOutputChannel("KeopsysDiodeLaser", tclBoardPump + "/ao2", -4, 4); //tick
            //AddAnalogOutputChannel("MenloPZT", tclBoardPump + "/ao0", 0, 10); //tick

            // Length stabilisation for pump cavity
            //AddAnalogOutputChannel("PumpCavityLengthVoltage", tclBoardPump + "/ao1", -10, 10); //tick

            //TCL configuration for pump cavity
            //TCLConfig tcl1 = new TCLConfig("Pump Cavity");
            //tcl1.AddLaser("MenloPZT", "Pumpp1");
            //tcl1.AddLaser("KeopsysDiodeLaser", "Pumpp2");
            //tcl1.Trigger = tclBoardPump + "/PFI0";
            //tcl1.Cavity = "PumpCavityRampVoltage";
            //tcl1.MasterLaser = "Pumpmaster";
            //tcl1.Ramp = "PumpCavityLengthVoltage";
            //tcl1.TCPChannel = 1190;
            //tcl1.AnalogSampleRate = 61250;
            //tcl1.DefaultScanPoints = 500;
            //tcl1.MaximumNLMFSteps = 20;
            //tcl1.PointsToConsiderEitherSideOfPeakInFWHMs = 2.5;
            //tcl1.TriggerOnRisingEdge = false;
            //tcl1.AddFSRCalibration("MenloPZT", 3.84);
            //tcl1.AddFSRCalibration("KeopsysDiodeLaser", 3.84);
            //tcl1.AddDefaultGain("Master", 0.3);
            //tcl1.AddDefaultGain("MenloPZT", -0.2);
            //tcl1.AddDefaultGain("KeopsysDiodeLaser", 4);
            //Info.Add("PumpCavity", tcl1);
            //Info.Add("DefaultCavity", tcl1);

            // Probe cavity inputs
            //AddAnalogInputChannel("ProbeRampVoltage", tclBoardProbe + "/ai0", AITerminalConfiguration.Rse); //tick
            //AddAnalogInputChannel("Probemaster", tclBoardProbe + "/ai1", AITerminalConfiguration.Rse); //tick
            //AddAnalogInputChannel("Probep1", tclBoardProbe + "/ai2", AITerminalConfiguration.Rse); //tick

            // Lasers locked to Probe cavity
            //AddAnalogOutputChannel("TopticaSHGPZT", tclBoardProbe + "/ao0", -4, 4); //tick
            //AddAnalogOutputChannel("ProbeCavityLengthVoltage", tclBoardProbe + "/ao1", -10, 10); //tick

            // TCL configuration for Probe cavity
            //TCLConfig tcl2 = new TCLConfig("Probe Cavity");
            //tcl2.AddLaser("TopticaSHGPZT", "Probep1");
            //tcl2.Trigger = tclBoardProbe + "/PFI0";
            //tcl2.Cavity = "ProbeRampVoltage";
            //tcl2.MasterLaser = "Probemaster";
            //tcl2.Ramp = "ProbeCavityLengthVoltage";
            //tcl2.TCPChannel = 1191;
            //tcl2.AnalogSampleRate = 61250/2;
            //tcl2.DefaultScanPoints = 250;
            //tcl2.MaximumNLMFSteps = 20;
            //tcl2.PointsToConsiderEitherSideOfPeakInFWHMs = 12;
            //tcl2.AddFSRCalibration("TopticaSHGPZT", 3.84);
            //tcl2.TriggerOnRisingEdge = false;
            //tcl2.AddDefaultGain("Master", 0.4);
            //tcl2.AddDefaultGain("TopticaSHGPZT", 0.04);
            //Info.Add("ProbeCavity", tcl2);
            //Info.Add("DefaultCavity", tcl2);
            
            //probe AOM control
            AddAnalogOutputChannel("probeAOM", aoBoard + "/ao29", -10, 10);
            AddAnalogOutputChannel("probeAOMamp", aoBoard + "/ao28", 0, 10);

            //Obselete Laser control
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