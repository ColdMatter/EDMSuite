using System;
using System.Collections;

using NationalInstruments.DAQmx;
using DAQ.WavemeterLock;
using DAQ.Pattern;
using DAQ.TransferCavityLock2012;
using DAQ.DigitalTransferCavityLock;
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
            Boards.Add("daq", "/PXI1Slot4");
            Boards.Add("pg", "/PXI1Slot5");
            Boards.Add("analog", "/PXI1Slot6");
            Info.Add("PatternGeneratorBoard", "/PXI1Slot5");
            Info.Add("PGType", "integrated");
            Info.Add("PGClockCounter", "/ctr0");
            Info.Add("PGClockLine", Boards["pg"] + "/PFI15");
            Info.Add("PGTriggerLine", Boards["pg"] + "/PFI0");
            Info.Add("analogTrigger0", (string)Boards["pg"] + "/PFI0");
            Info.Add("ScanMasterConfig", "C:\\Users\\alfultra\\OneDrive - Imperial College London\\Desktop\\ScanProfils.xml");
            Info.Add("MacroConfig", "C:\\Data\\Macros.xml");


            Dictionary<string, string> analogBoards = new Dictionary<string, string>();
            analogBoards.Add("AO", (string)Boards["analog"]);
            //Info.Add("AOPatternTrigger", Boards["pg"] + "/PFI15");
            Info.Add("AOPatternTrigger", Boards["pg"] + "/do/StartTrigger");
            //Info.Add("AOPatternTrigger", (string)Boards["analog"] + "/PFI0");
            Info.Add("AOClockLine", (string)Boards["analog"] + "/PFI1");
            Info.Add("AnalogBoards", analogBoards);


            // Input signals
            AddAnalogInputChannel("tclCavityRampVoltage", (string)Boards["daq"] + "/ai6", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("MBRLaser", (string)Boards["daq"] + "/ai7", AITerminalConfiguration.Rse, true);
            AddAnalogInputChannel("RbReferenceLaser", (string)Boards["daq"] + "/ai4", AITerminalConfiguration.Rse, true);
            AddAnalogInputChannel("PMT", (string)Boards["pg"] + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("UV_I", (string)Boards["pg"] + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("UV_Circ_Pow", (string)Boards["pg"] + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("Spec_PMT", (string)Boards["pg"] + "/ai4", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("V1_Blue_Circ_Pow", (string)Boards["pg"] + "/ai5", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("V1_UV_Circ_Pow", (string)Boards["pg"] + "/ai6", AITerminalConfiguration.Rse);

            // Output signals
            AddAnalogOutputChannel("tclOut", (string)Boards["daq"] + "/ao0", -10, 10);
            AddAnalogOutputChannel("HeFlow", (string)Boards["daq"] + "/ao0", 0, 5);
            //AddAnalogOutputChannel("tclCavityLengthVoltage", (string)Boards["daq"] + "/ao1", -10, 10);
            //AddAnalogOutputChannel("testOut", (string)Boards["daq"] + "/ao1", -10, 10);
            //AddAnalogOutputChannel("WMLOut", (string)Boards["pg"] + "/ao1", 0, 10);
            AddAnalogOutputChannel("VECSEL1_PZO", (string)Boards["daq"]+"/ao1", 0, 10);
            AddAnalogOutputChannel("VECSEL2_PZO", (string)Boards["pg"]+"/ao1", 0, 10);
            AddAnalogOutputChannel("VECSEL3_PZO", (string)Boards["pg"]+"/ao0", 0, 10);
            //AddAnalogOutputChannel("DTCLRampOut", (string)Boards["pg"]+"/ao0", 0, 10);
            AddAnalogOutputChannel("AOM1_VCA", (string)Boards["analog"]+"/ao0",0,10);
            AddAnalogOutputChannel("AOM2_VCA", (string)Boards["analog"] + "/ao3", 0, 10);
            AddAnalogOutputChannel("VECSEL1_CHIRP", (string)Boards["analog"]+"/ao1",0,10);
            AddAnalogOutputChannel("VECSEL2_CHIRP", (string)Boards["analog"]+"/ao2",0,10);
            AddAnalogOutputChannel("VECSEL3_CHIRP", (string)Boards["analog"] + "/ao4", 0, 10);


            // Add Digital Output Channels
            AddDigitalOutputChannel("flash", (string)Boards["pg"], 0, 0);
            AddDigitalOutputChannel("q", (string)Boards["pg"], 0, 1);// Loop back to PFI0
            AddDigitalOutputChannel("valve", (string)Boards["pg"], 0, 2);
            AddDigitalOutputChannel("detector", (string)Boards["pg"], 0, 3);
            AddDigitalOutputChannel("VECSEL2_Shutter", (string)Boards["pg"], 0, 4);
            AddDigitalOutputChannel("He_Shutter", (string)Boards["pg"], 0, 5);
            AddDigitalOutputChannel("VECSEL1_Block", (string)Boards["pg"], 0, 6); // Wired to /PXI1Slot4/PFI6
            AddDigitalOutputChannel("VECSEL3_Block", (string)Boards["pg"], 0, 7); // Wired to /PXI1Slot4/PFI7
            //AddDigitalOutputChannel("discharge", (string)Boards["pg"], 0, 4);
            //AddDigitalOutputChannel("valve2", (string)Boards["pg"], 0, 5);
            //AddDigitalOutputChannel("ttlSwitch", (string)Boards["pg"], 0, 4);
            //AddDigitalOutputChannel("detectorprime", (string)Boards["pg"], 0, 5);
            AddDigitalOutputChannel("VECSEL3_Shutter", (string)Boards["pg"], 1, 1);

            // Add Digital Input Channels
            AddDigitalInputChannel("VECSEL1_Block_Flag", (string)Boards["daq"], 1, 6);
            AddDigitalInputChannel("VECSEL3_Block_Flag", (string)Boards["daq"], 1, 7);


            // Misc channels
            //AddCounterChannel("RbReferenceLaser", "/PXI1Slot5/PFI1");
            //AddCounterChannel("MBRLaser", "/PXI1Slot5/PFI15");
            //AddCounterChannel("10MHzRefClock", "/PXI1Slot5/10MHzRefClock");
            //AddCounterChannel("20MHzTimebase", "/PXI1Slot5/20MHzTimebase");
            //AddCounterChannel("SyncCounter", "/PXI1Slot5/ctr1");
            //AddCounterChannel("RbCounter", "/PXI1Slot5/ctr2");
            //AddCounterChannel("MBRCounter", "/PXI1Slot5/ctr3");
            //AddCounterChannel("ResetOut", "/PXI1Slot5/PFI2");
            AddCounterChannel("PMT_Edges", "/PXI1Slot5/ctr3");
            AddCounterChannel("sample clock", "/PXI1Slot5/ctr2");

            // MOT Master config

            MMConfig mmConfig = new MMConfig(false, false, true, false);
            mmConfig.ExternalFilePattern = "*.dng";
            Info.Add("MotMasterConfiguration", mmConfig);
            Info.Add("AdditionalPatternGeneratorBoards", new Dictionary<string, string>());

            // Analog inputs for MOTMasterStuff
            List<string> MMAI = new List<string>();
            MMAI.Add("PMT");
            MMAI.Add("Spec_PMT");
            MMAI.Add("UV_Circ_Pow");
            MMAI.Add("V1_Blue_Circ_Pow");
            MMAI.Add("V1_UV_Circ_Pow");
            MMAI.Add("UV_I");
            Info.Add("MMAnalogInputs", MMAI);
            Info.Add("MMAITrigger", (string)Boards["pg"] + "/PFI0");

            // Counter inputs for MOTMasterStuff
            List<string> MMCtr = new List<string> { };
            MMCtr.Add("PMT_Edges");
            Info.Add("MMCtrInputs", MMCtr);
            Info.Add("MMCtrTrigger", (string)Boards["pg"] + "/PFI0");
            Info.Add("MMCtrSampleClock", (string)Boards["pg"] + "/ai/SampleClock");

            // Shutters
            // Name of shutter, Name of shutter channel, invert
            List<Tuple<string, string, bool>> shutters = new List<Tuple<string, string, bool>>();
            shutters.Add(new Tuple<string, string, bool>("VECSEL2", "VECSEL2_Shutter", false ));
            Info.Add("Shutters", shutters);

            //WavemeterLockConfig
            WavemeterLockConfig wmlConfig = new WavemeterLockConfig("Default");
            
            //TCLConfig
            TCLConfig tclConfigMBR = new TCLConfig("MBR-Ref");
            tclConfigMBR.Trigger = Boards["daq"] + "/PFI8";
            tclConfigMBR.BaseRamp = "tclCavityRampVoltage";
            tclConfigMBR.TCPChannel = 1192;
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



            wmlConfig.AddSlaveLaser("VECSEL1", "VECSEL1_PZO", 5);
            wmlConfig.AddLaserConfiguration("VECSEL1", 323.449904, -2000, -1600);
            wmlConfig.AddLockBlock("VECSEL1", "VECSEL1_Block_Flag");
            wmlConfig.AddSlaveLaser("VECSEL2", "VECSEL2_PZO", 6);
            //wmlConfig.AddLaserConfiguration("VECSEL2", 329.390872, -2000,-1600); //329.3907327752221`
            wmlConfig.AddLaserConfiguration("VECSEL2", 329.390872, -1000, -800);
            wmlConfig.AddSlaveLaser("VECSEL3", "VECSEL3_PZO", 7);
            wmlConfig.AddLockBlock("VECSEL3", "VECSEL3_Block_Flag");
            wmlConfig.AddLaserConfiguration("VECSEL3", 654.932482, -1000, -800);
            wmlConfig.AddSlaveLaser("MBR", "tclOut", 5);
            wmlConfig.AddLaserConfiguration("MBR", 384.234493, 500, 2000);

            Info.Add("Default", wmlConfig);
            Info.Add("TCLDefault", tclConfigMBR);
            Info.Add("DefaultCavity", tclConfigMBR);

            DTCLConfig dtclconfig = new DTCLConfig("SyncCounter");
            dtclconfig.rampOut = "DTCLRampOut";
            dtclconfig.timebaseChannel = "20MHzTimebase";
            dtclconfig.timebaseFrequency = 20000000;
            //dtclconfig.resetOut = "ResetOut";

            dtclconfig.AddCavity("tclCavity");
            dtclconfig.cavities["tclCavity"].ConfigureMasterLaser("RbReferenceLaser", "tclCavityLengthVoltage", "RbCounter", "10MHzRefClock", 10000000);
            dtclconfig.cavities["tclCavity"].AddSlaveLaser("MBR","MBRLaser", "tclOut", "MBRCounter", "10MHzRefClock", 10000000);

            Info.Add("DTCLConfig", dtclconfig);

            Instruments.Add("Lakeshore", new LakeShore336TemperatureController("ASRL8::INSTR"));
            Instruments.Add("YAG", new BigSkyYAG("ASRL6::INSTR"));
            Instruments.Add("LeyboldGraphix", new LeyboldGraphixController("ASRL11::INSTR"));
            Instruments.Add("Eurotherm", new Eurotherm3504Instrument("ASRL9::INSTR", 0x1));
            ((Eurotherm3504Instrument)Instruments["Eurotherm"]).AddLoop(379,0x2,0x3,273,0x4,4963);
            ((Eurotherm3504Instrument)Instruments["Eurotherm"]).AddLoop(0x400,4964);


        }

        public override void ConnectApplications()
        {
            // ask the remoting system for access to TCL2012
            // Type t = Type.GetType("TransferCavityLock2012.Controller, TransferCavityLock");
            // System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownClientType(t, "tcp://localhost:1190/controller.rem");
        }
    }
}
