using System;
using System.Collections;

using NationalInstruments.DAQmx;
using DAQ.WavemeterLock;
using DAQ.Pattern;
using DAQ.TransferCavityLock2012;
using DAQ.Remoting;
using System.Runtime.Remoting;
using System.Collections.Generic;

namespace DAQ.HAL
{
    public class AlFHardware : DAQ.HAL.Hardware
    {
        public AlFHardware()
        {
            // TO ADD: "analogTrigger0" "BrightnessPD", "laser"[maybe rename this!]

            // add the boards
            Boards.Add("daq", "/PXI1Slot4");
            Boards.Add("pg", "/PXI1Slot5");
            Info.Add("PatternGeneratorBoard", "/PXI1Slot5");
            Info.Add("PGType", "integrated");
            Info.Add("PGClockCounter", "/ctr0");
            Info.Add("PGClockLine", Boards["pg"] + "/10MHzRefClock");


            // Input signals
            AddAnalogInputChannel("tclCavityRampVoltage", (string)Boards["daq"] + "/ai6", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("MBRLaser", (string)Boards["daq"] + "/ai7", AITerminalConfiguration.Rse, true);
            AddAnalogInputChannel("RbReferenceLaser", (string)Boards["daq"] + "/ai4", AITerminalConfiguration.Rse, true);

            // Output signals
            AddAnalogOutputChannel("tclOut", (string)Boards["daq"] + "/ao0", -4, 4);
            AddAnalogOutputChannel("tclCavityLengthVoltage", (string)Boards["daq"] + "/ao1", -10, 10);
            AddAnalogOutputChannel("testOut", (string)Boards["daq"] + "/ao1", -10, 10);
            AddAnalogOutputChannel("WMLOut", (string)Boards["daq"] + "/ao1", 0, 10);
            AddAnalogOutputChannel("VECSEL2_PZO", (string)Boards["daq"]+"/ao0", 0, 10);

            //WavemeterLockConfig
            WavemeterLockConfig wmlConfig = new WavemeterLockConfig("Default");
            
            //TCLConfig
            TCLConfig tclConfigMBR = new TCLConfig("MBR-Ref");
            tclConfigMBR.Trigger = Boards["daq"] + "/PFI8";
            tclConfigMBR.BaseRamp = "tclCavityRampVoltage";
            tclConfigMBR.TCPChannel = 1191;
            tclConfigMBR.DefaultScanPoints = 500;
            tclConfigMBR.PointsToConsiderEitherSideOfPeakInFWHMs = 12;
            tclConfigMBR.AnalogSampleRate = 61250;
            tclConfigMBR.MaximumNLMFSteps = 20;
            tclConfigMBR.TriggerOnRisingEdge = false;
            string tclCavity = "tclCavity";

            tclConfigMBR.AddCavity(tclCavity);
            tclConfigMBR.Cavities[tclCavity].AddSlaveLaser("tclOut", "MBRLaser");
            tclConfigMBR.Cavities[tclCavity].MasterLaser = "RbReferenceLaser";
            tclConfigMBR.Cavities[tclCavity].RampOffset = "tclCavityLengthVoltage";
            tclConfigMBR.Cavities[tclCavity].AddDefaultGain("Master", 0.3);
            tclConfigMBR.Cavities[tclCavity].AddDefaultGain("tclOut", -0.2);
            tclConfigMBR.Cavities[tclCavity].AddFSRCalibration("tclOut", 3.84);
       


            wmlConfig.AddSlaveLaser("VECSEL", "WMLOut", 5);//name, analog, wavemeter channel
            wmlConfig.AddSlaveLaser("VECSEL2", "VECSEL2_PZO", 6);
            wmlConfig.AddSlaveLaser("MBR", "tclOut", 6);
            Info.Add("Default", wmlConfig);
            Info.Add("TCLDefault", tclConfigMBR);
            Info.Add("defaultcavity", tclConfigMBR);

            // map the digital channels of the "pg" card
            AddDigitalOutputChannel("q", (string)Boards["pg"], 0, 1);//Pin 
            AddDigitalOutputChannel("flash", (string)Boards["pg"], 0, 0);
            AddDigitalOutputChannel("valve", (string)Boards["pg"], 0, 2);
            AddDigitalOutputChannel("detector", (string)Boards["pg"], 0, 3);
            AddDigitalOutputChannel("ttlSwitch", (string)Boards["pg"], 0, 4);
            AddDigitalOutputChannel("detectorprime", (string)Boards["pg"], 0, 5);



            Instruments.Add("Lakeshore", new LakeShore336TemperatureController("ASRL8::INSTR"));
            Instruments.Add("LeyboldGraphix", new LeyboldGraphixController("ASRL11::INSTR"));
            Instruments.Add("Eurotherm", new Eurotherm3504Instrument("ASRL9::INSTR", 0x1));
            ((Eurotherm3504Instrument)Instruments["Eurotherm"]).AddLoop(379,0x2,0x3,273,0x4);
            ((Eurotherm3504Instrument)Instruments["Eurotherm"]).AddLoop(0x400);


        }

        public override void ConnectApplications()
        {
            // ask the remoting system for access to TCL2012
            // Type t = Type.GetType("TransferCavityLock2012.Controller, TransferCavityLock");
            // System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownClientType(t, "tcp://localhost:1190/controller.rem");
        }
    }
}
