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
            // add the boards
            string PatternBoardName = "pg";
            string PatternBoardAddress = "/PXI1Slot4";
            Boards.Add(PatternBoardName, PatternBoardAddress);
            Boards.Add("pg2", "/PXI1Slot5");
            Info.Add("PatternGeneratorBoard", PatternBoardAddress);
            Info.Add("PGType", "integrated");
            Info.Add("PGClockCounter", "/ctr0");
            Info.Add("PGClockLine", PatternBoardAddress + "/PFI4");


            // Input signals
            AddAnalogInputChannel("tclCavityRampVoltage", PatternBoardAddress + "/ai6", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("MBRLaser", PatternBoardAddress + "/ai4", AITerminalConfiguration.Rse, true); // Change back to Rb
            AddAnalogInputChannel("RbReferenceLaser", PatternBoardAddress + "/ai7", AITerminalConfiguration.Rse, true);

            // Output signals
            AddAnalogOutputChannel("tclOut", PatternBoardAddress + "/ao0", -4, 4);
            AddAnalogOutputChannel("tclCavityLengthVoltage", PatternBoardAddress + "/ao0", -10, 10);
            AddAnalogOutputChannel("testOut", PatternBoardAddress + "/ao1", -10, 10);
            AddAnalogOutputChannel("WMLOut", PatternBoardAddress + "/ao1", 0, 10);
            AddAnalogOutputChannel("VECSEL2_PZO", "PXI1Slot5/ao0", 0, 10);

            //WavemeterLockConfig
            WavemeterLockConfig wmlConfig = new WavemeterLockConfig("Default");
            
            //TCLConfig
            TCLConfig tclConfigMBR = new TCLConfig("MBR-Ref");
            tclConfigMBR.Trigger = PatternBoardAddress + "/PFI8";
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
            Info.Add("Default", wmlConfig);
            Info.Add("TCLDefault", tclConfigMBR);
            Info.Add("DefaultCavity", tclConfigMBR);

            // map the digital channels of the "pg" card
            AddDigitalOutputChannel("q", PatternBoardAddress, 0, 1);//Pin 
            AddDigitalOutputChannel("flash", PatternBoardAddress, 0, 2);
            AddDigitalOutputChannel("valve", PatternBoardAddress, 0, 3);
            AddDigitalOutputChannel("detector", PatternBoardAddress, 0, 4);
            AddDigitalOutputChannel("ttlSwitch", PatternBoardAddress, 0, 5);
            AddDigitalOutputChannel("detectorprime", PatternBoardAddress, 0, 6);

        }

        public override void ConnectApplications()
        {
            // ask the remoting system for access to TCL2012
            // Type t = Type.GetType("TransferCavityLock2012.Controller, TransferCavityLock");
            // System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownClientType(t, "tcp://localhost:1190/controller.rem");
        }
    }
}
