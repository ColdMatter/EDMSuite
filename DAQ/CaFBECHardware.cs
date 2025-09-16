using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;

using NationalInstruments;
using NationalInstruments.DAQmx;
using DAQ.WavemeterLock;
using DAQ.Pattern;
using DAQ.Remoting;
using DAQ.TransferCavityLock2012;

namespace DAQ.HAL
{
    public class CaFBECHardware : DAQ.HAL.Hardware
    {

        public CaFBECHardware()
        {

            Boards.Add("patternGenerator", "/PXI1Slot3");  // generating molecular source and receive signals, PXI-6229 connector 0
            Boards.Add("tclInOut", "/PXI1Slot4");          // TCL analog inputs, PXI-6221
            Boards.Add("wmOutput", "/PXI1Slot2");         // TCL analog outputs, PXI-6722
            Boards.Add("hcInput", "/PXI1Slot6");
            Boards.Add("output6733", "/PXI1Slot5");

            string pgBoard = (string)Boards["patternGenerator"];
            string TCLInOut = (string)Boards["tclInOut"];
            string WMOutput = (string)Boards["wmOutput"];
            string hcInput = (string)Boards["hcInput"];
            string output6733 = (string)Boards["output6733"];

            // map the digital channels of the "pg" card
            AddDigitalOutputChannel("q", pgBoard, 0, 3);
            AddDigitalOutputChannel("analogPatternTrigger", pgBoard, 0, 0); 
            AddDigitalOutputChannel("flash", pgBoard, 0, 1);
            AddDigitalOutputChannel("blockTCL", pgBoard, 0, 12);
            AddDigitalOutputChannel("blockV00", pgBoard, 0, 13);

            AddDigitalOutputChannel("greenShutter", pgBoard, 0, 2);

            AddDigitalOutputChannel("V00R0AOM", pgBoard, 0, 27);                                           
            AddDigitalOutputChannel("BXAOM", pgBoard, 0, 28);                                           
            AddDigitalOutputChannel("RepumpAOM", pgBoard, 0, 29);                                     
            AddDigitalOutputChannel("RepumpBroadening", pgBoard, 0, 26);
            AddDigitalOutputChannel("V00R0EOM", pgBoard, 0, 16);
            AddDigitalOutputChannel("V00R1plusAOM", pgBoard, 0, 17);
            AddDigitalOutputChannel("V00R1plusAOMJump", pgBoard, 0, 25);
            AddDigitalOutputChannel("V00B1minusAOM", pgBoard, 0, 22);
            AddDigitalOutputChannel("V00B1minusAOMJump", pgBoard, 0, 20);
            AddDigitalOutputChannel("cameraTrigger", pgBoard, 0, 14);
            AddDigitalOutputChannel("camera2Trigger", pgBoard, 0, 15);
            AddDigitalOutputChannel("shutter", pgBoard, 0, 18);
            AddDigitalOutputChannel("BXSidebands", pgBoard, 0, 24);                                          
            AddDigitalOutputChannel("V00B2AOM", pgBoard, 0, 21);
            AddDigitalOutputChannel("DipoleTrapAOM", pgBoard, 0, 19);
            AddDigitalOutputChannel("MicrowaveSwitch", pgBoard, 0, 23);

            // map the analog output channels for "daq" card
            AddAnalogOutputChannel("BXChirp", output6733 + "/ao0", -5, 5);                             
            AddAnalogOutputChannel("V00R0AOMVCOFreq", output6733 + "/ao1", 0, 10);                             
            AddAnalogOutputChannel("motCoils", output6733 + "/ao2", -10, 10);
            AddAnalogOutputChannel("V00R0AOMVCOAmp", output6733 + "/ao3", 0, 10);                       
            AddAnalogOutputChannel("V00R0EOMVCOAmp", output6733 + "/ao4", -10, 10);
            AddAnalogOutputChannel("V00B2AOMAmp", output6733 + "/ao6", -10, 10);                               
            AddAnalogOutputChannel("V00R1plusAOMAmp", output6733 + "/ao7", -10, 10);

            AddAnalogOutputChannel("CavityRamp", output6733 + "/ao5", -5, 5);

            AddAnalogOutputChannel("SlowingBField", pgBoard + "/ao0", 0, 5);
            AddAnalogOutputChannel("ShimCoilX", pgBoard + "/ao1", -10, 10);
            AddAnalogOutputChannel("ShimCoilY", pgBoard + "/ao2", -10, 10);
            AddAnalogOutputChannel("ShimCoilZ", pgBoard + "/ao3", -10, 10);

            Info.Add("PGType", "integrated");
            Info.Add("PGClockCounter", "/ctr0");
            Info.Add("PGClockLine", pgBoard + "/PFI2");
            Info.Add("PatternGeneratorBoard", pgBoard);

            //WaveMeter Output Channels
            AddAnalogOutputChannel("BXLockWML", WMOutput + "/ao0", 0, 5);
            AddAnalogOutputChannel("v00LockWML", WMOutput + "/ao1", -5, 5);
            AddAnalogOutputChannel("v10LockWML", WMOutput + "/ao2", -1, 1);
            AddAnalogOutputChannel("v21LockWML", WMOutput + "/ao3", -1, 1);
            AddAnalogOutputChannel("v32LockWML", WMOutput + "/ao4", -1, 1);
            AddAnalogOutputChannel("RefLockWML", WMOutput + "/ao6", -1, 1);
            

            // Locking block flags
            AddDigitalInputChannel("blockBXflag", TCLInOut, 0, 3);
            AddDigitalInputChannel("blockv00flag", TCLInOut, 2, 6);
            AddDigitalInputChannel("blockv10flag", TCLInOut, 0, 4);
            AddDigitalInputChannel("blockv21flag", TCLInOut, 0, 2);
            AddDigitalInputChannel("blockv32flag", TCLInOut, 0, 7);
            
            //TCL Input/Output Channels
            AddAnalogInputChannel("ramp", TCLInOut + "/ai13", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("refPD", TCLInOut + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("v32PD", TCLInOut + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("v21PD", TCLInOut + "/ai3", AITerminalConfiguration.Rse);
            //AddAnalogOutputChannel("refLock", TCLInOut + "/ao0", 0, 5);
            //AddAnalogOutputChannel("v32Lock", TCLInOut + "/ao1", 0, 5);

            // Testing if both TCL and Wavemeter can use same AO board
            AddAnalogOutputChannel("refLock", WMOutput + "/ao5", 0, 10);
            AddAnalogOutputChannel("v32Lock", WMOutput + "/ao6", -1, 1);
            AddAnalogOutputChannel("v21Lock", WMOutput + "/ao7", -1, 1);

            // Hardware Controller Output channels
            AddDigitalOutputChannel("sf6Valve", hcInput, 2, 4);
            AddDigitalOutputChannel("heValve", hcInput, 2, 5);

            // Hardware Controller channels
            AddAnalogInputChannel("sf6FlowMonitor", hcInput + "/ai10", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("heFlowMonitor", hcInput + "/ai11", AITerminalConfiguration.Rse);
            AddAnalogOutputChannel("4KHeaterSwitch", hcInput + "/ao0");
            AddAnalogOutputChannel("40KHeaterSwitch", hcInput + "/ao1");

            //Flow Conversions for flow monitor in sccm per Volt. 0.2 sccm per V for Alicat
            Info.Add("flowConversionSF6", 0.2);
            Info.Add("flowConversionHe", 4.0);

            //ToF signals
            Info.Add("ToFPMTSignal", hcInput + "/ai2");
            Info.Add("ToFTrigger", hcInput + "/PFI0");
            //Info.Add("ToFAbsorptionSignal", hcInput + "/ai3");
            Info.Add("ToFAbsorptionSignal", TCLInOut + "/ai0");
            Info.Add("ToFAbsorptionTrigger", TCLInOut + "/PFI0");


            //WavemeterLock configuration
            WavemeterLockConfig wmlConfig = new WavemeterLockConfig("Default");
            wmlConfig.AddSlaveLaser("BX", "BXLockWML", 1);
            wmlConfig.AddSlaveLaser("v00", "v00LockWML", 2);
            wmlConfig.AddSlaveLaser("v10", "v10LockWML", 3);
            wmlConfig.AddSlaveLaser("Ref", "RefLockWML", 8);
            wmlConfig.AddLockBlock("BX", "blockBXflag");
            wmlConfig.AddLockBlock("v00", "blockv00flag");
            wmlConfig.AddLockBlock("v10", "blockv10flag");
            wmlConfig.AddLaserConfiguration("v00", 494.431885, -10, -800);
            wmlConfig.AddLaserConfiguration("BX", 564.582275, 10, 300);
            wmlConfig.AddLaserConfiguration("v10", 476.958910, 10, 500);
            wmlConfig.AddLaserConfiguration("Ref", 384.228115, 10, 500);
            Info.Add("Default", wmlConfig);


            // TCL configuration
            TCLConfig tclConfig = new TCLConfig("BEC Cavities");
            tclConfig.Trigger = TCLInOut + "/PFI0";
            tclConfig.BaseRamp = "ramp";
            tclConfig.TCPChannel = 1190;
            tclConfig.DefaultScanPoints = 1000;
            tclConfig.PointsToConsiderEitherSideOfPeakInFWHMs = 4;
            tclConfig.AnalogSampleRate = 55000;//62000
            tclConfig.AddCavity("North");
            tclConfig.Cavities["North"].AddSlaveLaser("v21Lock", "v21PD");
            tclConfig.Cavities["North"].AddLockBlocker("v21Lock", "blockv21flag");
            tclConfig.Cavities["North"].AddSlaveLaser("v32Lock", "v32PD");
            tclConfig.Cavities["North"].AddLockBlocker("v32Lock", "blockv32flag");
            tclConfig.Cavities["North"].MasterLaser = "refPD";
            tclConfig.Cavities["North"].RampOffset = "refLock";
            tclConfig.Cavities["North"].AddDefaultGain("Master", 1.0);
            tclConfig.Cavities["North"].AddDefaultGain("v21Lock", -0.5);
            tclConfig.Cavities["North"].AddFSRCalibration("v21Lock", 3.95); //This is an approximate guess
            tclConfig.Cavities["North"].AddDefaultGain("v32Lock", -0.5);
            tclConfig.Cavities["North"].AddFSRCalibration("v32Lock", 3.95); //This is an approximate guess
            
            Info.Add("TCLConfig", tclConfig);
            Info.Add("DefaultCavity", tclConfig);

            // MOTMaster configuration
            MMConfig mmConfig = new MMConfig(false, false, false, false);
            mmConfig.ExternalFilePattern = "*.tif";
            Info.Add("MotMasterConfiguration", mmConfig);

            // Info.Add("PGType", "dedicated");
            Info.Add("Element", "CaFBEC");

            Dictionary<string, string> analogBoards = new Dictionary<string, string>();
            analogBoards.Add("SecondAO", output6733);
            Info.Add("SecondAOPatternTrigger", output6733 + "/PFI6"); //PFI0 for pgBoard
            Info.Add("SecondAOClockLine", output6733 + "/PFI5"); //PFI6
            analogBoards.Add("AO", pgBoard);
            Info.Add("AOPatternTrigger", pgBoard + "/PFI0"); //PFI6
            Info.Add("AOClockLine", pgBoard + "/PFI6"); //PFI6
            Info.Add("AnalogBoards", analogBoards);

            Dictionary<string, string> additionalPatternBoards = new Dictionary<string, string>();
            //additionalPatternBoards.Add(digitalPatternBoardAddress, digitalPatternBoardAddress);
            Info.Add("AdditionalPatternGeneratorBoards", additionalPatternBoards);


            Instruments.Add("Lakeshore", new LakeShore336TemperatureController("ASRL4::INSTR"));
            Instruments.Add("Pfeiffer", new PfeifferPressureGauge("ASRL5::INSTR"));
            //Instruments.Add("Windfreak", new WindfreakSynth("ASRL30::INSTR"));
            Instruments.Add("Gigatronics", new Gigatronics7100Synth("GPIB0::7::INSTR"));
        }

        public override void ConnectApplications()
        {
            // ask the remoting system for access to TCL2012
            // Type t = Type.GetType("TransferCavityLock2012.Controller, TransferCavityLock");
            // System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownClientType(t, "tcp://localhost:1190/controller.rem");
        }

    }
 
}
