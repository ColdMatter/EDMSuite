using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;

using NationalInstruments;
using NationalInstruments.DAQmx;

using DAQ.Pattern;
using DAQ.Remoting;
using DAQ.TransferCavityLock2012;

namespace DAQ.HAL
{
    public class CaFBECHardware : DAQ.HAL.Hardware
    {

        public CaFBECHardware()
        {

            Boards.Add("patternGenerator", "/PXI1Slot5");  // generating molecular source and receive signals, PXI-6229 connector 0
            Boards.Add("tclInput", "/PXI1Slot6");  // TCL analog inputs, PXI-6221
            Boards.Add("tclOutput", "/PXI1Slot4");  // TCL analog outputs, PXI-6722

            string pgBoard = (string)Boards["patternGenerator"];
            string TCLInput = (string)Boards["tclInput"];
            string TCLOutput = (string)Boards["tclOutput"];

            // string TCLBoard = (string)Boards["tcl"];
            // string UEDMHardwareControllerBoard = (string)Boards["UEDMHardwareController"];

            // map the digital channels of the "pg" card
            AddDigitalOutputChannel("q", pgBoard, 0, 3);
            AddDigitalOutputChannel("analogPatternTrigger", pgBoard, 0, 0); // this is the digital output from the daq board that the TTlSwitchPlugin wil switch
            AddDigitalOutputChannel("flash", pgBoard, 0, 1);
            AddDigitalOutputChannel("slowingSwitch", pgBoard, 0, 31);
            AddDigitalOutputChannel("blockTCL", pgBoard, 0, 26);  // send a TTL during which the TCL feedback is blocked, and the chirping voltage is summed up.
            AddDigitalOutputChannel("ttl1", pgBoard, 1, 3);
            AddDigitalOutputChannel("ttl2", pgBoard, 1, 6);
            AddDigitalOutputChannel("531aom", pgBoard, 0, 29);

            // AddDigitalOutputChannel("sourceHeater", digitalPatternBoardAddress, 2, 5);
            // AddDigitalOutputChannel("cryoCooler", digitalPatternBoardAddress, 0, 5);

            // map the digital channels of the "daq" card
            // this is the digital output from the daq board that the TTlSwitchPlugin wil switch
            //AddDigitalOutputChannel("digitalSwitchChannel", daqBoard, 0, 0);//enable for camera
            //AddDigitalOutputChannel("cryoTriggerDigitalOutputTask", daqBoard, 0, 0);// cryo cooler digital logic


            // map the analog input channels for "pg" card
            // AddAnalogInputChannel("HeliumIn", pgBoard + "/ai0", AITerminalConfiguration.Rse);
            // AddAnalogInputChannel("SF6In", pgBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("pmt", pgBoard + "/ai2", AITerminalConfiguration.Rse);

            // map the analog output channels for "daq" card
            AddAnalogOutputChannel("laser", pgBoard + "/ao0", -10, 10);

            Info.Add("PGType", "integrated");
            Info.Add("PGClockCounter", "/ctr0");
            Info.Add("analogTrigger0", pgBoard + "/PFI0");
            Info.Add("PGClockLine", pgBoard + "/PFI2");
            Info.Add("PatternGeneratorBoard", pgBoard);

            //TCL Input channels
            AddAnalogInputChannel("sumVolt", TCLInput + "/ai5", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("northSignal", TCLInput + "/ai1", AITerminalConfiguration.Rse);    
            AddAnalogInputChannel("v00Signal", TCLInput + "/ai3", AITerminalConfiguration.Rse);    
            AddAnalogInputChannel("v10Signal", TCLInput + "/ai10", AITerminalConfiguration.Rse);    
            AddAnalogInputChannel("bXSignal", TCLInput + "/ai15", AITerminalConfiguration.Rse);

            AddAnalogInputChannel("southSignal", TCLInput + "/ai6", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("v21Signal", TCLInput + "/ai4", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("v32Signal", TCLInput + "/ai7", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("v11Signal", TCLInput + "/ai2", AITerminalConfiguration.Rse);

            AddDigitalInputChannel("blockBXflag", TCLInput, 0, 1);  //J17 J50
            AddDigitalInputChannel("blockV10flag", TCLInput, 0, 2);  // J49 J50

            //TCL Output Channels
            AddAnalogOutputChannel("northOffset", TCLOutput + "/ao2");
            AddAnalogOutputChannel("v00Lock", TCLOutput + "/ao3", -1, 1);
            AddAnalogOutputChannel("v10Lock", TCLOutput + "/ao4", - 1, 1);
            AddAnalogOutputChannel("bXLock", TCLOutput + "/ao7", 0, 10);
            AddAnalogOutputChannel("southOffset", TCLOutput + "/ao1");
            AddAnalogOutputChannel("v21Lock", TCLOutput + "/ao5", -1, 1);
            AddAnalogOutputChannel("v32Lock", TCLOutput + "/ao6", -1, 1);
            AddAnalogOutputChannel("v11Lock", TCLOutput + "/ao0", -1, 1);

            TCLConfig tclConfig = new TCLConfig("TCL");
            tclConfig.Trigger = TCLInput + "/PFI0";
            // tclConfig.Trigger = "analogTrigger0";
            tclConfig.BaseRamp = "sumVolt";
            tclConfig.TCPChannel = 1190;
            tclConfig.DefaultScanPoints =  1000;
            tclConfig.AnalogSampleRate = 15000;
            tclConfig.SlaveVoltageLowerLimit = 0.0;
            tclConfig.SlaveVoltageUpperLimit = 4.0;
            tclConfig.PointsToConsiderEitherSideOfPeakInFWHMs = 4;
            tclConfig.MaximumNLMFSteps = 20;
            tclConfig.TriggerOnRisingEdge = true;

            string northCavity = "NorthCavity";
            tclConfig.AddCavity(northCavity);
            tclConfig.Cavities[northCavity].RampOffset = "northOffset";
            tclConfig.Cavities[northCavity].MasterLaser = "northSignal";
            //Use format: AddSlaveLaser(analog output channel name, analog input channel name)
            tclConfig.Cavities[northCavity].AddSlaveLaser("v00Lock", "v00Signal");
            tclConfig.Cavities[northCavity].AddDefaultGain("v00Lock", 0.2);
            tclConfig.Cavities[northCavity].AddFSRCalibration("v00Lock", 3.84);
            tclConfig.Cavities[northCavity].AddSlaveLaser("v10Lock", "v10Signal");
            tclConfig.Cavities[northCavity].AddDefaultGain("v10Lock", 0.2);
            tclConfig.Cavities[northCavity].AddFSRCalibration("v10Lock", 3.84);
            tclConfig.Cavities[northCavity].AddLockBlocker("v10Lock", "blockV10flag");
            tclConfig.Cavities[northCavity].AddSlaveLaser("bXLock", "bXSignal");
            tclConfig.Cavities[northCavity].AddDefaultGain("bXLock", 0.2);
            tclConfig.Cavities[northCavity].AddFSRCalibration("bXLock", 3.84);
            tclConfig.Cavities[northCavity].AddLockBlocker("bXLock", "blockBXflag");
            // tclConfig.Cavities[northCavity].AddLockBlocker(TCLOutput + "/ao3", "blockBXflag");    // for test

            string southCavity = "SouthCavity";
            tclConfig.AddCavity(southCavity);
            tclConfig.Cavities[southCavity].RampOffset = "southOffset";
            tclConfig.Cavities[southCavity].MasterLaser = "southSignal";
            tclConfig.Cavities[southCavity].AddSlaveLaser("v21Lock", "v21Signal"); //analog output channel name, analog input channel name
            tclConfig.Cavities[southCavity].AddDefaultGain("v21Lock", 0.2);
            tclConfig.Cavities[southCavity].AddFSRCalibration("v21Lock", 3.84);
            tclConfig.Cavities[southCavity].AddSlaveLaser("v32Lock", "v32Signal");
            tclConfig.Cavities[southCavity].AddDefaultGain("v32Lock", 0.2);
            tclConfig.Cavities[southCavity].AddFSRCalibration("v32Lock", 3.84);
            tclConfig.Cavities[southCavity].AddSlaveLaser("v11Lock", "v11Signal");
            tclConfig.Cavities[southCavity].AddDefaultGain("v11Lock", 0.2);
            tclConfig.Cavities[southCavity].AddFSRCalibration("v11Lock", 3.84);


            Info.Add("TCLConfig", tclConfig);
            Info.Add("DefaultCavity", tclConfig);
            // Info.Add("analogTrigger0", TCLInput + "/PFI0");





            //These need to be activated for the phase lock
            //AddCounterChannel("phaseLockOscillator", daqBoard + "/ctr0"); //This should be the source pin of a counter PFI 8
            //AddCounterChannel("phaseLockReference", daqBoard + "/PFI9"); //This should be the gate pin of the same counter - need to check it's name


            // MOTMaster configuration
            MMConfig mmConfig = new MMConfig(false, false, false, false);
            mmConfig.ExternalFilePattern = "*.tif";
            Info.Add("MotMasterConfiguration", mmConfig);

            // Info.Add("PGType", "dedicated");
            Info.Add("Element", "CaFBEC");

            // Info.Add("AOPatternTrigger", pgBoard + "/PFI4"); //PFI6
            // Info.Add("AOClockLine", pgBoard + "/PFI6"); //PFI6
            // Info.Add("SecondAOPatternTrigger", pgBoard + "/PFI6");
            // Info.Add("SecondAOClockLine", pgBoard + "/PFI3");

            Dictionary<string, string> analogBoards = new Dictionary<string, string>();
            // analogBoards.Add("AO", aoBoard);
            // analogBoards.Add("SecondAO", aoBoard2);
            Info.Add("AnalogBoards", analogBoards);

            // Info.Add("PGClockLine", pgBoard + "/PFI4");
            // Info.Add("PGTriggerLine", pgBoard + "/PFI3");
            Dictionary<string, string> additionalPatternBoards = new Dictionary<string, string>();
            //additionalPatternBoards.Add(digitalPatternBoardAddress, digitalPatternBoardAddress);
            Info.Add("AdditionalPatternGeneratorBoards", additionalPatternBoards);
            // Info.Add("PGSlave0ClockLine", digitalPatternBoardAddress + "/PFI2");
            // Info.Add("PGSlave0TriggerLine", digitalPatternBoardAddress + "/PFI6");
        }

        public override void ConnectApplications()
        {
            // ask the remoting system for access to TCL2012
            // Type t = Type.GetType("TransferCavityLock2012.Controller, TransferCavityLock");
            // System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownClientType(t, "tcp://localhost:1190/controller.rem");
        }









    }
 
}
