using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;
using DAQ.TransferCavityLock2012;

namespace DAQ.HAL
{
    public class BufferClassicHardware : DAQ.HAL.Hardware
    {
        public BufferClassicHardware()
        {
            // add the boards
            Boards.Add("daq", "/DAQ_PXIe_6363");
            Boards.Add("tcl", "/PXI1Slot4");
            Boards.Add("pg", "/PXI1Slot6");
            Boards.Add("UEDMHardwareController", "/UEDM_Hardware_Controller_PXI_6229");  
            string daqBoard = (string)Boards["daq"];
            string pgBoard = (string)Boards["pg"];
            string TCLBoard = (string)Boards["tcl"];
            string UEDMHardwareControllerBoard = (string)Boards["UEDMHardwareController"];

            // map the digital channels of the "pg" card
            AddDigitalOutputChannel("q", pgBoard, 0, 6);//Pin 10
            AddDigitalOutputChannel("aom", pgBoard, 0, 9);//`
            AddDigitalOutputChannel("aom2", pgBoard, 0, 2);//
            AddDigitalOutputChannel("shutter1", pgBoard, 0, 3);//
            AddDigitalOutputChannel("shutter2", pgBoard, 0, 5);//
            AddDigitalOutputChannel("flash", pgBoard, 0, 4);//Pin 45
            AddDigitalOutputChannel("chirpTrigger", pgBoard, 0, 7);
            //(0,3) pin 12 is unconnected
            //AddDigitalOutputChannel("heatersS1TriggerDigitalOutputTask", pgBoard, 1, 6);// Pin 21, used to be "shutterTrig1" (triggers camera for on-shots (not wired up))
            //AddDigitalOutputChannel("heatersS2TriggerDigitalOutputTask", pgBoard, 1, 7);// Pin 22, used to be "shutterTrig2" (triggers camera for off-shots (not wired up))
            AddDigitalOutputChannel("probe", pgBoard, 0, 8);//Pin 44 previously connected to aom (not wired up)
            AddDigitalOutputChannel("valve", pgBoard, 0, 0);//
            AddDigitalOutputChannel("detector", pgBoard, 1, 0); //Pin 16 (onShot)from pg to daq
            AddDigitalOutputChannel("detectorprime", pgBoard, 0, 7); //Pin 15 (OffShot)from pg to daq
            //digital output P 0.6 wired up, not used (Pin 48)

            // map the digital channels of the "daq" card
            // this is the digital output from the daq board that the TTlSwitchPlugin wil switch
            AddDigitalOutputChannel("digitalSwitchChannel", daqBoard, 0, 0);//enable for camera
            //AddDigitalOutputChannel("cryoTriggerDigitalOutputTask", daqBoard, 0, 0);// cryo cooler digital logic

           
            // add things to the info
            // the analog triggers
            //Info.Add("analogTrigger0", daqBoard + "/PFI0");
            //Info.Add("analogTrigger1", daqBoard + "/PFI1");
            //Info.Add("phaseLockControlMethod", "analog");
            //Info.Add("PGClockLine", Boards["pg"] + "/PFI4");
            Info.Add("PatternGeneratorBoard", pgBoard);
            Info.Add("PGType", "integrated");
            Info.Add("PGClockCounter", "/ctr0");

            // external triggering control
            //Info.Add("PGTrigger", pgBoard + "/PFI1"); //Mapped to PFI7 on 6533 connector


            // map the analog input channels for "daq" card
            AddAnalogInputChannel("Temp1", daqBoard + "/ai0", AITerminalConfiguration.Rse);//Pin 31
            AddAnalogInputChannel("pressureGauge_beamline", daqBoard + "/ai1", AITerminalConfiguration.Rse);//Pin 31. Used to be "Temp2"   unused at the moment, should be renamed
            AddAnalogInputChannel("TempRef", daqBoard + "/ai2", AITerminalConfiguration.Rse);//Pin 66
            //AddAnalogInputChannel("pressureGauge_source", daqBoard + "/ai3", AITerminalConfiguration.Rse);//Pin 33 pressure reading at the moment
            AddAnalogInputChannel("detector1", daqBoard + "/ai4", AITerminalConfiguration.Rse);//Pin 68
            //AddAnalogInputChannel("detector1", TCLBoard + "/ai6", AITerminalConfiguration.Rse); //trying another card because of cross talks
            //AddAnalogInputChannel("detector1", UEDMHardwareControllerBoard + "/ai10", AITerminalConfiguration.Rse); //trying another card because of cross talks
            AddAnalogInputChannel("detector2", daqBoard + "/ai5", AITerminalConfiguration.Rse);//Pin 
            AddAnalogInputChannel("detector3", daqBoard + "/ai6", AITerminalConfiguration.Rse);//Pin 34
            AddAnalogInputChannel("cavitylong", daqBoard + "/ai7", AITerminalConfiguration.Rse);//Pin 28
            //AddAnalogInputChannel("cellTemperatureMonitor", daqBoard + "/ai8", AITerminalConfiguration.Rse);//Pin 60 used to be "cavityshort"

            // map the analog output channels for "daq" card
            AddAnalogOutputChannel("IRrampfb", daqBoard + "/ao0");//Pin 22
            AddAnalogOutputChannel("v2laser", daqBoard + "/ao1"); //pin 21

            // map the analog input channels for the "UEDMHardwareControllerBoard" card
            AddAnalogInputChannel("cellTemperatureMonitor", UEDMHardwareControllerBoard + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("S1TemperatureMonitor", UEDMHardwareControllerBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("S2TemperatureMonitor", UEDMHardwareControllerBoard + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("SF6TemperatureMonitor", UEDMHardwareControllerBoard + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("pressureGaugeSource", UEDMHardwareControllerBoard + "/ai4", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("pressureGaugeBeamline", UEDMHardwareControllerBoard + "/ai5", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("pressureGaugeDetection", UEDMHardwareControllerBoard + "/ai6", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("AI11", UEDMHardwareControllerBoard + "/ai11", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("AI12", UEDMHardwareControllerBoard + "/ai12", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("AI13", UEDMHardwareControllerBoard + "/ai13", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("AI14", UEDMHardwareControllerBoard + "/ai14", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("AI15", UEDMHardwareControllerBoard + "/ai15", AITerminalConfiguration.Rse);

            // map the digital channels of the "UEDMHardwareControllerBoard" card
            AddDigitalOutputChannel("Port00", UEDMHardwareControllerBoard, 0, 0);
            AddDigitalOutputChannel("Port01", UEDMHardwareControllerBoard, 0, 1);
            AddDigitalOutputChannel("Port02", UEDMHardwareControllerBoard, 0, 2);
            AddDigitalOutputChannel("Port03", UEDMHardwareControllerBoard, 0, 3);
            AddDigitalOutputChannel("heatersS2TriggerDigitalOutputTask", UEDMHardwareControllerBoard, 0, 9);
            AddDigitalOutputChannel("heatersS1TriggerDigitalOutputTask", UEDMHardwareControllerBoard, 0, 8);

            // map the analog output channels for the "UEDMHardwareControllerBoard" card
            //AddAnalogOutputChannel("laser", Unnamed + "/ao0");
            //AddAnalogOutputChannel("phaseLockAnalogOutput", Unnamed + "/ao1")

            // map the digital channels of the "UEDMHardwareControllerBoard" card
            //AddDigitalOutputChannel("cryoTriggerDigitalOutputTask", UEDMHardwareControllerBoard, 0, 0);// cryo cooler digital logic

            // map the analog input channels for the "tcl" card
            AddAnalogInputChannel("VISmaster", TCLBoard + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("VIScavityRampMonitor", TCLBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("VISp1_v1laser", TCLBoard + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("VISp2_probelaser", TCLBoard + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("VISp3_v0laser", TCLBoard + "/ai4", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("xxx", TCLBoard + "/ai5", AITerminalConfiguration.Rse); unused
            //AddAnalogInputChannel("xxx", TCLBoard + "/ai6", AITerminalConfiguration.Rse); unused
            //AddAnalogInputChannel("xxx", TCLBoard + "/ai7", AITerminalConfiguration.Rse); unused
            //AddAnalogInputChannel("xxx", TCLBoard + "/ai8", AITerminalConfiguration.Rse); unused
            //AddAnalogInputChannel("xxx", TCLBoard + "/ai9", AITerminalConfiguration.Rse); unused
            AddAnalogInputChannel("IRp1_v2laser", TCLBoard + "/ai10", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("IRmaster", TCLBoard + "/ai11", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("xxx", TCLBoard + "/ai16", AITerminalConfiguration.Rse); unused
            //AddAnalogInputChannel("xxx", TCLBoard + "/ai17", AITerminalConfiguration.Rse); unused
            //AddAnalogInputChannel("xxx", TCLBoard + "/ai18", AITerminalConfiguration.Rse); unused
            //AddAnalogInputChannel("xxx", TCLBoard + "/ai19", AITerminalConfiguration.Rse); unused
            //AddAnalogInputChannel("xxx", TCLBoard + "/ai20", AITerminalConfiguration.Rse); unused

            // map the analog output channels for the tcl card
            AddAnalogOutputChannel("VISrampfb", TCLBoard + "/ao0");
            AddAnalogOutputChannel("v1laser", TCLBoard + "/ao1");
            AddAnalogOutputChannel("probelaser", TCLBoard + "/ao2", 0, 10);
            AddAnalogOutputChannel("v0laser", TCLBoard + "/ao3", 0, 10);

            // add the GPIB/RS232/USB instruments
            Instruments.Add("tempController", new LakeShore336TemperatureController("ASRL3::INSTR"));
            Instruments.Add("neonFlowController", new FlowControllerMKSPR4000B("ASRL4::INSTR"));
            

// TCL, we can now put many cavities in a single instance of TCL (thanks to Luke)
// multiple cavities share a single ramp (BaseRamp analog input) + trigger
// Hardware limitation that all read photodiode/ramp signals must share the same hardware card (hardware configured triggered read)
            TCLConfig tclConfig = new TCLConfig("TCL");
            tclConfig.Trigger = TCLBoard + "/PFI0";
            tclConfig.BaseRamp = "VIScavityRampMonitor";
            tclConfig.TCPChannel = 1190;
            tclConfig.DefaultScanPoints = 1000;
            tclConfig.AnalogSampleRate = 15000;
            tclConfig.SlaveVoltageLowerLimit = 0.0;
            tclConfig.SlaveVoltageUpperLimit = 10.0;
            tclConfig.PointsToConsiderEitherSideOfPeakInFWHMs = 4;
            tclConfig.MaximumNLMFSteps = 20;
          
            string VISCavity = "VISCavity";
            tclConfig.AddCavity(VISCavity);
            tclConfig.Cavities[VISCavity].RampOffset = "VISrampfb";
            tclConfig.Cavities[VISCavity].MasterLaser = "VISmaster";
            tclConfig.Cavities[VISCavity].AddDefaultGain("VISmaster", 0.2);
            tclConfig.Cavities[VISCavity].AddSlaveLaser("v1laser", "VISp1_v1laser");
            tclConfig.Cavities[VISCavity].AddDefaultGain("v1laser", 0.2);
            tclConfig.Cavities[VISCavity].AddFSRCalibration("v1laser", 3.84);
            tclConfig.Cavities[VISCavity].AddSlaveLaser("probelaser", "VISp2_probelaser");
            tclConfig.Cavities[VISCavity].AddDefaultGain("probelaser", 0.2);
            tclConfig.Cavities[VISCavity].AddFSRCalibration("probelaser", 3.84);
            tclConfig.Cavities[VISCavity].AddSlaveLaser("v0laser", "VISp3_v0laser");
            tclConfig.Cavities[VISCavity].AddDefaultGain("v0laser", 0.2);
            tclConfig.Cavities[VISCavity].AddFSRCalibration("v0laser", 3.84);

            
            string IRCavity = "IRCavity";
            tclConfig.AddCavity(IRCavity);
            tclConfig.Cavities[IRCavity].RampOffset = "IRrampfb";
            tclConfig.Cavities[IRCavity].MasterLaser = "IRmaster";
            tclConfig.Cavities[IRCavity].AddDefaultGain("IRmaster", 0.2);
            tclConfig.Cavities[IRCavity].AddSlaveLaser("v2laser", "IRp1_v2laser");
            tclConfig.Cavities[IRCavity].AddDefaultGain("v2laser", 0.2);
            tclConfig.Cavities[IRCavity].AddFSRCalibration("v2laser", 3.84);
            
            Info.Add("TCLConfig", tclConfig);
            Info.Add("DefaultCavity", tclConfig);

            //These need to be activated for the phase lock
            //AddCounterChannel("phaseLockOscillator", daqBoard + "/ctr0"); //This should be the source pin of a counter PFI 8
            //AddCounterChannel("phaseLockReference", daqBoard + "/PFI9"); //This should be the gate pin of the same counter - need to check it's name
        }

        public override void ConnectApplications()
        {
            // ask the remoting system for access to TCL2012
           // Type t = Type.GetType("TransferCavityLock2012.Controller, TransferCavityLock");
           // System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownClientType(t, "tcp://localhost:1190/controller.rem");
        }
    }
}
