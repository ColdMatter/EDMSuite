using System;
using System.Collections;

using NationalInstruments.DAQmx;
using NationalInstruments;

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
            Boards.Add("pg", "/PG_PXIe_6535");
            Boards.Add("tcl", "/TCL_PXI_6229");
            Boards.Add("tclout", "/TCL_OUT_PXI_6722");
            Boards.Add("UEDMHardwareController", "/UEDM_Hardware_Controller_PXI_6229");
            Boards.Add("counter", "/COUNTER_PXI_6602");
            Boards.Add("mag", "/MAG_PXI_6229");
            Boards.Add("usbDAQ1", "/Dev3");         // this is for the magnetic field feedback
            Boards.Add("usbDAQ2", "/Dev4");         // this is temporarily for the B switch digital channels
            Boards.Add("usbTherm", "/Dev7");
            string daqBoard = (string)Boards["daq"];
            string pgBoard = (string)Boards["pg"];
            string TCLBoard = (string)Boards["tcl"];
            string TCLOutBoard = (string)Boards["tclout"];
            string UEDMHardwareControllerBoard = (string)Boards["UEDMHardwareController"];
            string counterBoard = (string)Boards["counter"];
            string magBoard = (string)Boards["mag"];
            string usbDAQ1 = (string)Boards["usbDAQ1"];
            string usbDAQ2 = (string)Boards["usbDAQ2"];
            string usbTherm = (string)Boards["usbTherm"];

            //machine information
            Info.Add("sourceToDetect", 3.5);
            Info.Add("moleculeMass", 193.0);
            Info.Add("machineLengthRatio", 3.842);
            Info.Add("defaultGate", new double[] { 2190, 800 });

            // map the digital channels of the "pg" card
            AddDigitalOutputChannel("q", pgBoard, 0, 0);//Pin 10
            AddDigitalOutputChannel("probe", pgBoard, 0, 1);
            AddDigitalOutputChannel("flash", pgBoard, 0, 2);//Pin 45
            AddDigitalOutputChannel("digitalSwitchChannel", pgBoard, 0, 5); // this is the digital output from the daq board that the TTlSwitchPlugin will switch
            AddDigitalOutputChannel("valve", pgBoard, 0, 6);

            AddDigitalOutputChannel("detectorprime", pgBoard, 0, 7);    //Pin 15 (OffShot)from pg to daq
            AddDigitalOutputChannel("detector", pgBoard, 1, 0);         //Pin 16 (onShot)from pg to daq
            AddDigitalOutputChannel("ccdtrigger", pgBoard, 2, 0);         //Pin 23 from pg to daq (both on and off shot)


            AddDigitalOutputChannel("ccd1", pgBoard, 1, 1);         // previously "aom"         if problem, change in the plugin, not here
            AddDigitalOutputChannel("ccd2", pgBoard, 1, 2);         // previously "aom2"        if problem, change in the plugin, not here
            AddDigitalOutputChannel("ttl1", pgBoard, 1, 3);         // previously "shutter1"    if problem, change in the plugin, not here
            AddDigitalOutputChannel("ttl2", pgBoard, 1, 4);         // previously "shutter2"    if problem, change in the plugin, not here
            AddDigitalOutputChannel("ttl3", pgBoard, 1, 5);         // previously "ttl1"        if problem, change in the plugin, not here
            AddDigitalOutputChannel("ttl4", pgBoard, 1, 6);
            AddDigitalOutputChannel("ttl5", pgBoard, 1, 7);



            // map the digital channels of the "daq" card
            // this is the digital output from the daq board that the TTlSwitchPlugin wil switch
            //AddDigitalOutputChannel("digitalSwitchChannel", daqBoard, 0, 0);//enable for camera
            //AddDigitalOutputChannel("cryoTriggerDigitalOutputTask", daqBoard, 0, 0);// cryo cooler digital logic


            // add things to the info
            // the analog triggers
            Info.Add("analogTrigger0", daqBoard + "/PFI0");
            Info.Add("analogTrigger1", daqBoard + "/PFI1");
            Info.Add("analogTrigger2", daqBoard + "/PFI2");
            Info.Add("pfiTrigger3", daqBoard + "/PFI3"); //rhys add 08/07
            Info.Add("phaseLockControlMethod", "usb");
            Info.Add("PGClockLine", pgBoard + "/PFI4");
            Info.Add("PatternGeneratorBoard", pgBoard);
            Info.Add("PGType", "dedicated");
            //Info.Add("ccdDigitalIn", daqBoard + "/port0/line1"); //rhys add 20/07
            Info.Add("ccdDigitalIn", daqBoard + "/port0/line0:1"); //rhys add 28/07 - Combine both CCD status lines
            AddCounterChannel("cameraEnabler", daqBoard + "/ctr0");//, 0, 19); //labelled as PFI12 - this is the counter channel for PXIe 6363

            // Scanmaster config
            Info.Add("ScanMasterConfig", "C:\\Users\\UEDM\\Documents\\EDM Suite Files\\Settings\\Scanmaster\\2024_July_Rhys.xml");

            // external triggering control
            Info.Add("PGTriggerLine", pgBoard + "/PFI1"); //Mapped to no where in particular


            // map the analog input channels for "daq" card
            AddAnalogInputChannel("Temp1", daqBoard + "/ai0", AITerminalConfiguration.Rse);//Pin 31 //Note on 29/07, this port isnt connected to anything. 
            AddAnalogInputChannel("pressureGauge_beamline", daqBoard + "/ai1", AITerminalConfiguration.Rse);//Pin 31. Used to be "Temp2"   unused at the moment, should be renamed //Note on 29/07, this port isnt connected to anything. 
            AddAnalogInputChannel("TempRef", daqBoard + "/ai2", AITerminalConfiguration.Rse);//Pin 66
            //AddAnalogInputChannel("pressureGauge_source", daqBoard + "/ai3", AITerminalConfiguration.Rse);//Pin 33 pressure reading at the moment //Note on 29/07, this port isnt connected to anything. 
            AddAnalogInputChannel("upstreamPMT", daqBoard + "/ai4", AITerminalConfiguration.Rse);//Pin 68   Used to be detector1 //Note on 29/07, this port is labelled as PMT1
            //AddAnalogInputChannel("detector1", TCLBoard + "/ai6", AITerminalConfiguration.Rse); //trying another card because of cross talks
            //AddAnalogInputChannel("detector1", UEDMHardwareControllerBoard + "/ai10", AITerminalConfiguration.Rse); //trying another card because of cross talks
            AddAnalogInputChannel("detectorA", daqBoard + "/ai6", AITerminalConfiguration.Rse);//Pin 34 Used to be detector3 //Note on 29/07, this port is labelled as PMT3
            AddAnalogInputChannel("detectorB", daqBoard + "/ai5", AITerminalConfiguration.Rse);//Pin    Used to be detector2 //Note on 29/07, this port is labelled as PMT2
            AddAnalogInputChannel("cavitylong", daqBoard + "/ai7", AITerminalConfiguration.Rse);//Pin 28 
            //AddAnalogInputChannel("CCDA", daqBoard + "/ai8", AITerminalConfiguration.Rse);//Pin 28
            //AddAnalogInputChannel("cellTemperatureMonitor", daqBoard + "/ai8", AITerminalConfiguration.Rse);//Pin 60 used to be "cavityshort"
            AddAnalogInputChannel("miniFlux1", daqBoard + "/ai9", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("bartington_Y", daqBoard + "/ai11", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("battery", daqBoard + "/ai10", AITerminalConfiguration.Rse);

            // map the analog input channels for "mag" card (magnetometers and coil currents)
            AddAnalogInputChannel("quSpinHM_Y", magBoard + "/ai0", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("bartington_Y", magBoard + "/ai1", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("quSpinHO_Y", magBoard + "/ai1", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("battery", magBoard + "/ai2", AITerminalConfiguration.Differential); 
            AddAnalogInputChannel("quSpinHP_Y", magBoard + "/ai2", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinHQ_Y", magBoard + "/ai3", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinHR_Y", magBoard + "/ai4", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinHS_Y", magBoard + "/ai5", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinHT_Y", magBoard + "/ai6", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinFV_Y", magBoard + "/ai7", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinHM_Z", magBoard + "/ai16", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("bartington_Z_nearRelay", magBoard + "/ai17", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("quSpinHO_Z", magBoard + "/ai17", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinHP_Z", magBoard + "/ai18", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinHQ_Z", magBoard + "/ai19", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinHR_Z", magBoard + "/ai20", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinHS_Z", magBoard + "/ai21", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinHT_Z", magBoard + "/ai22", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("quSpinFV_Z", magBoard + "/ai23", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("bartington_X", daqBoard + "/ai22", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("bartington_Y", magBoard + "/ai30", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("coilCurrent_after", magBoard + "/ai31", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("coilCurrent_before", daqBoard + "/ai8", AITerminalConfiguration.Rse);//Pin 28

            // map the analog output channels for "mag" card
            AddAnalogOutputChannel("steppingBBias", magBoard + "/ao0");

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
            //AddAnalogInputChannel("AI14", UEDMHardwareControllerBoard + "/ai14", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("AI15", UEDMHardwareControllerBoard + "/ai15", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("cPlusMonitor", UEDMHardwareControllerBoard + "/ai7", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("cMinusMonitor", UEDMHardwareControllerBoard + "/ai8", AITerminalConfiguration.Rse);

            //map the analog output channels for the "UEDMHardwareControllerBoard" card
            AddAnalogOutputChannel("cPlusPlate", UEDMHardwareControllerBoard + "/ao0");
            AddAnalogOutputChannel("cMinusPlate", UEDMHardwareControllerBoard + "/ao1");
            AddAnalogOutputChannel("DegaussCoil1", UEDMHardwareControllerBoard + "/ao2");
            AddAnalogOutputChannel("BScan", UEDMHardwareControllerBoard + "/ao3");
            
            // map the digital channels of the "UEDMHardwareControllerBoard" card
            AddDigitalOutputChannel("Port00", UEDMHardwareControllerBoard, 0, 0);
            AddDigitalOutputChannel("Port01", UEDMHardwareControllerBoard, 0, 1);
            AddDigitalOutputChannel("Port02", UEDMHardwareControllerBoard, 0, 2);
            AddDigitalOutputChannel("Port03", UEDMHardwareControllerBoard, 0, 3);
            AddDigitalOutputChannel("heatersS2TriggerDigitalOutputTask", UEDMHardwareControllerBoard, 0, 4);
            AddDigitalOutputChannel("heatersS1TriggerDigitalOutputTask", UEDMHardwareControllerBoard, 0, 5);
            AddDigitalOutputChannel("ePol", UEDMHardwareControllerBoard, 0, 1);
            AddDigitalOutputChannel("notEPol", UEDMHardwareControllerBoard, 0, 3);
            AddDigitalOutputChannel("eBleed", UEDMHardwareControllerBoard, 0, 2);
            AddDigitalOutputChannel("eConnect", usbDAQ2, 0, 5);
            AddDigitalOutputChannel("bSwitch", usbDAQ2, 0, 0);
            AddDigitalOutputChannel("notB", usbDAQ2, 0, 1);
            AddDigitalOutputChannel("dB", usbDAQ2, 0, 2);
            AddDigitalOutputChannel("notDB", usbDAQ2, 0, 3);
            AddDigitalOutputChannel("targetStepperStep", usbDAQ2, 0, 4);
            AddDigitalOutputChannel("targetStepperDirection", usbDAQ2, 0, 6);
            //AddDigitalOutputChannel("cameraEnabler", usbDAQ2, 0, 6);

            //UsbThermocouple channels
            AddAnalogInputThermocoupleChannel("FeedthroughTempInput", usbTherm + "/ai0", AITerminalConfiguration.Differential, AIThermocoupleType.K);

            //Magnetic feedback channels
            AddAnalogInputChannel("bFieldFeedbackInput", usbDAQ2 + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogOutputChannel("bFieldFeedbackOutput", usbDAQ2 + "/ao1", 0, 5);
            //AddCounterChannel("bFieldFeedbackClock", UEDMHardwareControllerBoard + "/pfi3");
            AddCounterChannel("bFieldFeedbackClock", usbDAQ2 + "/pfi0");

            //Counter Channels
            AddCounterChannel("westLeakage", counterBoard + "/ctr6"); //UEDMHardwareControllerBoard + "/ctr0");//
            AddCounterChannel("eastLeakage", counterBoard + "/ctr5"); //UEDMHardwareControllerBoard + "/ctr1");//

            //Phase Lock
            AddCounterChannel("phaseLockOscillator", counterBoard + "/ctr1");
            AddCounterChannel("phaseLockReference", counterBoard + "/pfi11");
            AddAnalogOutputChannel("phaseLockAnalogOutput", TCLOutBoard + "/ao5", 0, 5);

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
            //AddAnalogInputChannel("xxx", TCLBoard + "/ai7", AITerminalConfiguration.Rse); unused
            //AddAnalogInputChannel("xxx", TCLBoard + "/ai8", AITerminalConfiguration.Rse); unused
            //AddAnalogInputChannel("xxx", TCLBoard + "/ai9", AITerminalConfiguration.Rse); unused
            AddAnalogInputChannel("IRp1_STIRAP", TCLBoard + "/ai10", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("IRp2_Q1", TCLBoard + "/ai16", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("IRp3_v2laser", TCLBoard + "/ai6", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("IRmaster", TCLBoard + "/ai11", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("OPmaster", TCLBoard + "/ai17", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("OPp1_Q0", TCLBoard + "/ai5", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("OPp2_P12", TCLBoard + "/ai18", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("xxx", TCLBoard + "/ai19", AITerminalConfiguration.Rse); unused
            //AddAnalogInputChannel("xxx", TCLBoard + "/ai20", AITerminalConfiguration.Rse); unused

            // map the analog output channels for the tcl card
            AddAnalogOutputChannel("VISrampfb", TCLBoard + "/ao0",-10,10);
            AddAnalogOutputChannel("v1laser", TCLBoard + "/ao1");
            AddAnalogOutputChannel("probelaser", TCLBoard + "/ao2", 0,8);
            AddAnalogOutputChannel("v0laser", TCLBoard + "/ao3", 0, 10);
            AddAnalogOutputChannel("OPrampfb", TCLOutBoard + "/ao0");
            AddAnalogOutputChannel("Q0", TCLOutBoard + "/ao1", 0, 3);
            AddAnalogOutputChannel("P12", TCLOutBoard + "/ao2", 0, 3);
            AddAnalogOutputChannel("Q1", TCLOutBoard + "/ao3", 0, 5);
            //AddAnalogOutputChannel("v2laser", TCLOutBoard + "/ao4", 0, 5);

            AddAnalogOutputChannel("IRrampfb", daqBoard + "/ao0");//Pin 22
            AddAnalogOutputChannel("STIRAP", daqBoard + "/ao1",0,5); //pin 21 ////Note on 29/07, this port is labelled as V2 laser

            // add the GPIB/RS232/USB instruments
            Instruments.Add("tempController", new LakeShore336TemperatureController("ASRL3::INSTR"));
            Instruments.Add("WindfreakOpticalPumping", new WindfreakSynthHD("ASRL6::INSTR"));
            Instruments.Add("WindfreakDetection", new WindfreakSynthHD("ASRL9::INSTR"));
            Instruments.Add("neonFlowController", new FlowControllerMKSPR4000B("ASRL24::INSTR"));
            Instruments.Add("sf6FlowController", new AlicatFlowController("ASRL11::INSTR"));
            Instruments.Add("AD9850DDS", new AD9850DDS("ASRL8::INSTR"));
            Instruments.Add("bCurrentMeter", new HP34401A("GPIB0::12::INSTR"));
            Instruments.Add("rfCounter", new Agilent53131A("GPIB0::5::INSTR"));
            Instruments.Add("rigolWavGen", new RigolDG811("USB0::0x1AB1::0x0643::DG8A250800641::INSTR"));
            Instruments.Add("green", new HP8657ASynth("GPIB0::7::INSTR"));
            Instruments.Add("targetStepperControl", new StepperMotorController("ASRL4::INSTR"));
            Instruments.Add("bCurrentSource", new TwinleafCSB("ASRL13::INSTR"));


            // TCL, we can now put many cavities in a single instance of TCL (thanks to Luke)
            // multiple cavities share a single ramp (BaseRamp analog input) + trigger
            // Hardware limitation that all read photodiode/ramp signals must share the same hardware card (hardware configured triggered read)
            TCLConfig tclConfig = new TCLConfig("UEDM TCL");
            tclConfig.Trigger = TCLBoard + "/PFI0";
            tclConfig.BaseRamp = "VIScavityRampMonitor";
            tclConfig.TCPChannel = 1190;
            tclConfig.DefaultScanPoints = 250;//previous value 1000 (28/11/24)
            tclConfig.AnalogSampleRate = 20000; //previous value 15000 (28/11/24)
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
            tclConfig.Cavities[VISCavity].AddDefaultGain("probelaser", -0.2);
            tclConfig.Cavities[VISCavity].AddFSRCalibration("probelaser", 3.84);
            tclConfig.Cavities[VISCavity].AddSlaveLaser("v0laser", "VISp3_v0laser");
            tclConfig.Cavities[VISCavity].AddDefaultGain("v0laser", 0.2);
            tclConfig.Cavities[VISCavity].AddFSRCalibration("v0laser", 3.84);

            
            string IRCavity = "IRCavity";
            tclConfig.AddCavity(IRCavity);
            tclConfig.Cavities[IRCavity].RampOffset = "IRrampfb";
            tclConfig.Cavities[IRCavity].MasterLaser = "IRmaster";
            tclConfig.Cavities[IRCavity].AddDefaultGain("IRmaster", 0.2);
            tclConfig.Cavities[IRCavity].AddSlaveLaser("STIRAP", "IRp1_STIRAP");
            tclConfig.Cavities[IRCavity].AddDefaultGain("STIRAP", 0.2);
            tclConfig.Cavities[IRCavity].AddFSRCalibration("STIRAP", 3.84);
            tclConfig.Cavities[IRCavity].AddSlaveLaser("Q1", "IRp2_Q1");
            tclConfig.Cavities[IRCavity].AddDefaultGain("Q1", 0.1);
            tclConfig.Cavities[IRCavity].AddFSRCalibration("Q1", 3.84);
            //tclConfig.Cavities[IRCavity].AddSlaveLaser("v2laser", "IRp3_v2laser");
            //tclConfig.Cavities[IRCavity].AddDefaultGain("v2laser", 0.1);
            //tclConfig.Cavities[IRCavity].AddFSRCalibration("v2laser", 3.84);

            string OPCavity = "OPCavity";
            tclConfig.AddCavity(OPCavity);
            tclConfig.Cavities[OPCavity].RampOffset = "OPrampfb";
            tclConfig.Cavities[OPCavity].MasterLaser = "OPmaster";
            tclConfig.Cavities[OPCavity].AddDefaultGain("OPmaster", 0.2);
            tclConfig.Cavities[OPCavity].AddSlaveLaser("Q0", "OPp1_Q0");
            tclConfig.Cavities[OPCavity].AddDefaultGain("Q0", 0.1);
            tclConfig.Cavities[OPCavity].AddFSRCalibration("Q0", 3.84);
            tclConfig.Cavities[OPCavity].AddSlaveLaser("P12", "OPp2_P12");
            tclConfig.Cavities[OPCavity].AddDefaultGain("P12", 0.2);
            tclConfig.Cavities[OPCavity].AddFSRCalibration("P12", 3.84);

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
