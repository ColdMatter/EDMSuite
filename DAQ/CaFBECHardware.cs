using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;

using NationalInstruments;
using NationalInstruments.DAQmx;

using DAQ.Pattern;
using DAQ.TransferCavityLock2012;

namespace DAQ.HAL
{
    public class CaFBECHardware : DAQ.HAL.Hardware
    {

        public CaFBECHardware()
        {

            Boards.Add("pg", "/PXI1Slot5");  // generating molecular source and receive signals, PXI-6229 connector 0
            Boards.Add("tcl", "/PXI1Slot6");  // TCL analog inputs, PXI-6221
            Boards.Add("tclOutput", "/PXI1Slot4");  // TCL analog outputs, PXI-6722

            string pgBoard = (string)Boards["pg"];
            string TCLBoard = (string)Boards["tcl"];
            string TCLOutput = (string)Boards["tclOutput"];

            // string TCLBoard = (string)Boards["tcl"];
            // string UEDMHardwareControllerBoard = (string)Boards["UEDMHardwareController"];

            // map the digital channels of the "pg" card
            AddDigitalOutputChannel("q", pgBoard, 0, 3);
            AddDigitalOutputChannel("digitalSwitchChannel", pgBoard, 0, 0); // this is the digital output from the daq board that the TTlSwitchPlugin wil switch
            AddDigitalOutputChannel("flash", pgBoard, 0, 1);
            // AddDigitalOutputChannel("sourceHeater", digitalPatternBoardAddress, 2, 5);
            // AddDigitalOutputChannel("cryoCooler", digitalPatternBoardAddress, 0, 5);

         
            // map the digital channels of the "daq" card
            // this is the digital output from the daq board that the TTlSwitchPlugin wil switch
            //AddDigitalOutputChannel("digitalSwitchChannel", daqBoard, 0, 0);//enable for camera
            //AddDigitalOutputChannel("cryoTriggerDigitalOutputTask", daqBoard, 0, 0);// cryo cooler digital logic


            // add things to the info
            // the analog triggers
            Info.Add("analogTrigger0", TCLBoard + "/PFI0");
            Info.Add("PatternGeneratorBoard", pgBoard);

      

            // map the analog input channels for "pg" card
            AddAnalogInputChannel("HeliumIn", pgBoard + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("SF6In", pgBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("pmt", pgBoard + "/ai2", AITerminalConfiguration.Rse);



            // map the analog output channels for "daq" card
            AddAnalogOutputChannel("HeliumOut", pgBoard + "/ao0");
            AddAnalogOutputChannel("SF6Out", pgBoard + "/ao1"); 



            // Counter Channels
            // AddCounterChannel("westLeakage", UEDMHardwareControllerBoard + "/ctr0");
            // AddCounterChannel("eastLeakage", UEDMHardwareControllerBoard + "/ctr1");



            // map the analog input channels for the tcl card
            AddAnalogInputChannel("sumVolt", TCLBoard + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("mOTv023master", TCLBoard + "/ai10", AITerminalConfiguration.Rse);    // PD A, reference laser
            AddAnalogInputChannel("mOTv0", TCLBoard + "/ai1", AITerminalConfiguration.Rse);    // PD B, 606 nm, also the probe laser for molecules
            AddAnalogInputChannel("mOTv2", TCLBoard + "/ai4", AITerminalConfiguration.Rse);    // PD E, 628 nm
            AddAnalogInputChannel("mOTv3", TCLBoard + "/ai5", AITerminalConfiguration.Rse);    // PD D, 628 nm

            

            // map the analog output channels for the tcl card
            AddAnalogOutputChannel("cavityOffset", TCLOutput + "/ao0");
            AddAnalogOutputChannel("mOTv0Laser", TCLOutput + "/ao1");    
            AddAnalogOutputChannel("mOTv2Laser", TCLOutput + "/ao2");
            AddAnalogOutputChannel("mOTv3Laser", TCLOutput + "/ao3");


            // add the GPIB/RS232/USB instruments
            // Instruments.Add("tempController", new LakeShore336TemperatureController("ASRL3::INSTR"));
            // Instruments.Add("WindfreakOpticalPumping", new WindfreakSynthHD("ASRL6::INSTR"));
            // Instruments.Add("WindfreakDetection", new WindfreakSynthHD("ASRL9::INSTR"));
            // Instruments.Add("neonFlowController", new FlowControllerMKSPR4000B("ASRL4::INSTR"));
            // Instruments.Add("AD9850DDS", new AD9850DDS("ASRL8::INSTR"));


            // TCL, we can now put many cavities in a single instance of TCL (thanks to Luke)
            // multiple cavities share a single ramp (BaseRamp analog input) + trigger
            // Hardware limitation that all read photodiode/ramp signals must share the same hardware card (hardware configured triggered read)

            TCLConfig tclConfig = new TCLConfig("TCL");
            tclConfig.Trigger = TCLBoard + "/PFI0";
            //tclConfig.Trigger = "analogTrigger0";
            tclConfig.BaseRamp = "sumVolt";
            tclConfig.TCPChannel = 1190;
            tclConfig.DefaultScanPoints =  1000;
            tclConfig.AnalogSampleRate = 15000;
            tclConfig.SlaveVoltageLowerLimit = 0.0;
            tclConfig.SlaveVoltageUpperLimit = 10.0;
            tclConfig.PointsToConsiderEitherSideOfPeakInFWHMs = 4;
            tclConfig.MaximumNLMFSteps = 20;
            //tclConfig.TriggerOnRisingEdge = true;

            string MOTv023Cavity = "MOTv023Cavity";
            tclConfig.AddCavity(MOTv023Cavity);
            tclConfig.Cavities[MOTv023Cavity].RampOffset = "cavityOffset";
            tclConfig.Cavities[MOTv023Cavity].MasterLaser = "mOTv023master";
            // tclConfig.Cavities[MOTv023Cavity].AddDefaultGain("MOTv023master", 0.2);
            tclConfig.Cavities[MOTv023Cavity].AddSlaveLaser("mOTv0Laser", "mOTv0"); //analog output channel name, analog input channel name
            tclConfig.Cavities[MOTv023Cavity].AddDefaultGain("mOTv0Laser", 0.2);
            tclConfig.Cavities[MOTv023Cavity].AddFSRCalibration("mOTv0Laser", 3.84);
            tclConfig.Cavities[MOTv023Cavity].AddSlaveLaser("mOTv2Laser", "mOTv2");
            tclConfig.Cavities[MOTv023Cavity].AddDefaultGain("mOTv2Laser", 0.2);
            tclConfig.Cavities[MOTv023Cavity].AddFSRCalibration("mOTv2Laser", 3.84);
            tclConfig.Cavities[MOTv023Cavity].AddSlaveLaser("mOTv3Laser", "mOTv3");
            tclConfig.Cavities[MOTv023Cavity].AddDefaultGain("mOTv3Laser", 0.2);
            tclConfig.Cavities[MOTv023Cavity].AddFSRCalibration("mOTv3Laser", 3.84);


            // string MOTv1Cavity = "MOTv1Cavity";
            // tclConfig.AddCavity(MOTv1Cavity);
            // tclConfig.Cavities[MOTv1Cavity].RampOffset = "IRrampfb";
            // tclConfig.Cavities[MOTv1Cavity].MasterLaser = "IRmaster";
            // tclConfig.Cavities[MOTv1Cavity].AddDefaultGain("IRmaster", 0.2);
            // tclConfig.Cavities[MOTv1Cavity].AddSlaveLaser("v2laser", "IRp1_v2laser");
            // tclConfig.Cavities[MOTv1Cavity].AddDefaultGain("v2laser", 0.2);
            // tclConfig.Cavities[MOTv1Cavity].AddFSRCalibration("v2laser", 3.84);
            // tclConfig.Cavities[MOTv1Cavity].AddSlaveLaser("v3laser", "IRp2_v3laser");
            // tclConfig.Cavities[MOTv1Cavity].AddDefaultGain("v3laser", 0.1);
            // tclConfig.Cavities[MOTv1Cavity].AddFSRCalibration("v3laser", 3.84);

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
