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
    /// This is the specific hardware that the lattice edm machine has. This class conforms
    /// to the Hardware interface.
    /// </summary>
    public class PHULTRAEDMHardware : DAQ.HAL.Hardware
    {
        public override void ConnectApplications()
        {
        }


        public PHULTRAEDMHardware()
        {
            //Add the boards

            string digitalPatternBoardName = "pg";//NI PXIe-6535
            string digitalPatternBoardAddress = "/PXI1Slot6";
            string ExtraBoard = "/PXI1Slot5";
            //string usbbreakout = "/Dev4";

            Boards.Add("tclBoardProbe", "/PXI1Slot5");
            Boards.Add("pg", "/PXI1Slot6");
            //Boards.Add("tclBoardPump", "/PXI1Slot6");

            //string tclBoardPump = (string)Boards["tclBoardPump"];
            string tclBoardProbe = (string)Boards["tclBoardProbe"];
            string pgBoard = (string)Boards["pg"];

            Info.Add("PGType", "integrated");
            Info.Add("PatternGeneratorBoard", digitalPatternBoardAddress);
            Info.Add("PGClockCounter", "/ctr0");

            Instruments.Add("tempController", new LakeShore336TemperatureController("ASRL4::INSTR"));

            // AddAnalogInputChannel("pressure", usbbreakout + "/ai3", AITerminalConfiguration.Rse);
            // AddAnalogInputChannel("S1TemperatureMonitor", usbbreakout + "/ai4", AITerminalConfiguration.Rse);
            // AddAnalogInputChannel("S2TemperatureMonitor", usbbreakout + "/ai5", AITerminalConfiguration.Rse);
            // AddAnalogInputChannel("SF6TemperatureMonitor", usbbreakout + "/ai6", AITerminalConfiguration.Rse);
            // AddAnalogInputChannel("pressureGaugeSource", usbbreakout + "/ai7", AITerminalConfiguration.Rse);
            // AddAnalogInputChannel("pressureGaugeBeamline", usbbreakout + "/ai8", AITerminalConfiguration.Rse);
            // AddAnalogInputChannel("pressureGaugeDetection", usbbreakout + "/ai9", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("AI11", usbbreakout + "/ai11", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("AI12", usbbreakout + "/ai12", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("AI13", usbbreakout + "/ai13", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("AI14", usbbreakout + "/ai14", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("AI15", usbbreakout + "/ai15", AITerminalConfiguration.Rse);
            //AddDigitalOutputChannel("Port00", usbbreakout, 0, 0);
            //AddDigitalOutputChannel("Port01", usbbreakout, 0, 1);
            //AddDigitalOutputChannel("Port02", usbbreakout, 0, 2);
            //AddDigitalOutputChannel("Port03", usbbreakout, 0, 3);
            //AddDigitalOutputChannel("heatersS2TriggerDigitalOutputTask", usbbreakout, 0, 4);
            //AddDigitalOutputChannel("heatersS1TriggerDigitalOutputTask", usbbreakout, 0, 5);

            // Probe cavity inputs
            AddAnalogInputChannel("ProbeCavityRampVoltage", digitalPatternBoardAddress + "/ai8", AITerminalConfiguration.Rse); //tick this is the ramp impute
            AddAnalogInputChannel("Probemaster", digitalPatternBoardAddress + "/ai14", AITerminalConfiguration.Rse); //tick this is the 780nm photodiode
            AddAnalogInputChannel("Probep1", digitalPatternBoardAddress + "/ai12", AITerminalConfiguration.Rse); //tick //this is the probe laser photodiode input
            AddAnalogInputChannel("Probep2", digitalPatternBoardAddress + "/ai10", AITerminalConfiguration.Rse); //tick //this is the probe laser photodiode input - slowing laser


            // Lasers locked to Probe cavity
            AddAnalogOutputChannel("slowingLaser", digitalPatternBoardAddress + "/ao2", 0, 10);
            AddAnalogOutputChannel("LatticeProbeLaser", digitalPatternBoardAddress + "/ao1", 0, 10); //tick //this is the analogue ouput port on the DAQ card for the frequency feedback of the laser (piezo in our case)
            AddAnalogOutputChannel("ProbeCavityLengthVoltage", digitalPatternBoardAddress + "/ao0", 0, 10); //tick //this is the voltage that stabilises the length of the cavity

            //



            //TCL coniguration for Lattice EDM
            TCLConfig tclConfigProbe = new TCLConfig("Probe");
            tclConfigProbe.Trigger = digitalPatternBoardAddress + "/PFI0";
            tclConfigProbe.BaseRamp = "ProbeCavityRampVoltage";
            tclConfigProbe.TCPChannel = 1190;
            tclConfigProbe.DefaultScanPoints = 2400 * 1 / 4;
            tclConfigProbe.PointsToConsiderEitherSideOfPeakInFWHMs = 12;
            tclConfigProbe.AnalogSampleRate = 245000 * 1 / 8;//reduce number 12/3/21 by factr 10
            tclConfigProbe.MaximumNLMFSteps = 20;
            tclConfigProbe.TriggerOnRisingEdge = true;
            string probe = "ProbeCavity";

            tclConfigProbe.AddCavity(probe);
            tclConfigProbe.Cavities[probe].AddSlaveLaser("LatticeProbeLaser", "Probep1");
            tclConfigProbe.Cavities[probe].AddSlaveLaser("slowingLaser", "Probep2");

            tclConfigProbe.Cavities[probe].MasterLaser = "Probemaster";
            tclConfigProbe.Cavities[probe].RampOffset = "ProbeCavityLengthVoltage";
            tclConfigProbe.Cavities[probe].AddDefaultGain("Master", 0.4);
            tclConfigProbe.Cavities[probe].AddDefaultGain("LatticeProbeLaser", 1.00);
            tclConfigProbe.Cavities[probe].AddFSRCalibration("LatticeProbeLaser", 3.84);
            tclConfigProbe.Cavities[probe].AddDefaultGain("slowingLaser", 0.1);
            tclConfigProbe.Cavities[probe].AddFSRCalibration("slowingLaser", 3.84);

            //Info.Add("TCLConfigPump", tclConfigPump);
            Info.Add("TCLConfigProbe", tclConfigProbe);
            Info.Add("DefaultCavity", tclConfigProbe);
            Info.Add("TCLConfig", tclConfigProbe);
            Info.Add("tclConfigPump", tclConfigProbe);


            //TCL configuration for pump cavity: 13/01/2021 (Chris)
            //TCLConfig tclConfigPump = new TCLConfig("Pump");
            //tclConfigPump.Trigger = digitalPatternBoardAddress + "/PFI0";
            //tclConfigPump.BaseRamp = "ProbeCavityRampVoltage";
            //tclConfigPump.TCPChannel = 1191;
            //tclConfigPump.DefaultScanPoints = 500;
            //tclConfigPump.PointsToConsiderEitherSideOfPeakInFWHMs = 12;
            //tclConfigPump.AnalogSampleRate = 61250;
            //tclConfigPump.MaximumNLMFSteps = 20;
            //tclConfigPump.TriggerOnRisingEdge = true; // changed this 09/3/21
            //string pump = "PumpCavity";

            //tclConfigPump.AddCavity(pump);
            //tclConfigPump.Cavities[pump].AddSlaveLaser("slowingLaser", "Probep2");
            //tclConfigPump.Cavities[pump].AddSlaveLaser("KeopsysDiodeLaser", "Pumpp2");
            //tclConfigPump.Cavities[pump].MasterLaser = "Probemaster";
            //tclConfigPump.Cavities[pump].RampOffset = "ProbeCavityLengthVoltage";
            //tclConfigPump.Cavities[pump].AddDefaultGain("Master", 0.3);
            //tclConfigPump.Cavities[pump].AddDefaultGain("NewKeopsysDiodeLaser", -0.2);
            //tclConfigPump.Cavities[pump].AddDefaultGain("slowingLaser", 1);
            //tclConfigPump.Cavities[pump].AddFSRCalibration("slowingLaser", 3.84);
            //tclConfigPump.Cavities[pump].AddFSRCalibration("KeopsysDiodeLaser", 3.84);

            //TCL configuration of probe cavity: 27/06/2019 (Chris)
            //TCLConfig tclConfigProbe = new TCLConfig("Probe");
            //tclConfigProbe.Trigger = digitalPatternBoardAddress + "/PFI0"; 
            //tclConfigProbe.BaseRamp = "ProbeCavityRampVoltage";
            //tclConfigProbe.TCPChannel = 1190;
            //tclConfigProbe.DefaultScanPoints = 500 * 1 / 4;
            //tclConfigProbe.PointsToConsiderEitherSideOfPeakInFWHMs = 12;
            //tclConfigProbe.AnalogSampleRate = 61250 * 1 / 2;//reduce number 12/3/21 by factr 10
            //tclConfigProbe.MaximumNLMFSteps = 20;
            //tclConfigProbe.TriggerOnRisingEdge = true;
            //string probe = "ProbeCavity";

            //tclConfigProbe.AddCavity(probe);
            //tclConfigProbe.Cavities[probe].AddSlaveLaser("TopticaSHGPZT", "Probep1");
            //tclConfigProbe.Cavities[probe].MasterLaser = "Probemaster";
            //tclConfigProbe.Cavities[probe].RampOffset = "ProbeCavityLengthVoltage";
            //tclConfigProbe.Cavities[probe].AddDefaultGain("Master", 0.4);
            //tclConfigProbe.Cavities[probe].AddDefaultGain("TopticaSHGPZT", 0.04);
            //tclConfigProbe.Cavities[probe].AddFSRCalibration("TopticaSHGPZT", 3.84);

            //Info.Add("TCLConfigPump", tclConfigPump);
            //Info.Add("TCLConfigProbe", tclConfigProbe);
            //Info.Add("DefaultCavity", tclConfigProbe);
            //Info.Add("TCLConfig", tclConfigProbe);





            ////Boards.Add(digitalPatternBoardName, digitalPatternBoardAddress);

            ////Boards.Add("tclBoardProbe", "/PXI1Slot5");
            //string tclBoardProbe = (string)Boards["tclBoardProbe"];


            ////string analogPatternBoardName = "analogPattern";//NI PXIe-6229
            ////string analogPatternBoardAddress = "/PXI1Slot6";
            ////Boards.Add(analogPatternBoardName, analogPatternBoardAddress);


            ////string pgBoard = (string)Boards["pg"];




            ////lattice scanmaster
            AddDigitalOutputChannel("q", digitalPatternBoardAddress, 0, 6);
            AddDigitalOutputChannel("flash", digitalPatternBoardAddress, 0, 3);
            
            //AddDigitalOutputChannel("analogPatternTrigger", digitalPatternBoardAddress, 0, 8);//connect to daq board PFI 0 - not needed 01/2/22
            
            //AddDigitalOutputChannel("analogtriggertest0", digitalPatternBoardAddress, 0, 4);
            //AddDigitalOutputChannel("sourceHeater", digitalPatternBoardAddress, 0, 5);
            //AddDigitalOutputChannel("cryoCooler", digitalPatternBoardAddress, 0, 9);
            //AddDigitalOutputChannel("unused", digitalPatternBoardAddress, 0, 2);
            AddDigitalOutputChannel("valve", digitalPatternBoardAddress, 0, 11);//it seeems to like having this here
            AddDigitalOutputChannel("detector", digitalPatternBoardAddress, 0, 2);
            AddDigitalOutputChannel("detectorprime", digitalPatternBoardAddress, 0, 1);
            AddDigitalOutputChannel("shutter2on", digitalPatternBoardAddress, 0, 4);
            AddDigitalOutputChannel("shutter2off", digitalPatternBoardAddress, 0, 5);
            AddDigitalOutputChannel("shutter1off", digitalPatternBoardAddress, 0, 7);//new 24/09/21 
            // shutter 1 on is done in the switch line of the pattern (port 0 0)
            AddDigitalOutputChannel("shutterslow", digitalPatternBoardAddress, 0, 8);
            AddDigitalOutputChannel("shutterv1", digitalPatternBoardAddress, 0, 9);
            AddDigitalOutputChannel("shutterv2", digitalPatternBoardAddress, 0, 10);
            ////AddAnalogInputChannel("4Kthermistor", analogPatternBoardAddress + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("pmt", ExtraBoard + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("pmt2", ExtraBoard + "/ai1", AITerminalConfiguration.Rse);
            AddDigitalOutputChannel("shutterslow2", digitalPatternBoardAddress, 0, 12);
            ////AddAnalogInputChannel("VISp2_probelaser", tclBoardProbe + "/ai3", AITerminalConfiguration.Rse);
            ////AddAnalogOutputChannel("probelaser", tclBoardProbe + "/ao2", 0, 10);

            //// Probe cavity inputs
            //AddAnalogInputChannel("ProbeCavityRampVoltage", tclBoardProbe + "/ai0", AITerminalConfiguration.Rse); //tick
            //AddAnalogInputChannel("Probemaster", tclBoardProbe + "/ai1", AITerminalConfiguration.Rse); //tick
            //AddAnalogInputChannel("VIS2_probelaser", tclBoardProbe + "/ai2", AITerminalConfiguration.Rse); //tick

            //// Lasers locked to Probe cavity
            ////AddAnalogOutputChannel("TopticaSHGPZT", tclBoardProbe + "/ao0", -4, 4); //tick
            //AddAnalogOutputChannel("ProbeCavityLengthVoltage", tclBoardProbe + "/ao1", -10, 10); //tick


            //// ScanMaster configuration

            Info.Add("analogTrigger0", ExtraBoard + "/PFI0");
            Info.Add("analogTrigger1", ExtraBoard + "/PFI1");

            //Info.Add("defaultTOFRange", new double[] { 4000, 12000 }); // these entries are the two ends of the range for the upper TOF graph
            //Info.Add("defaultTOF2Range", new double[] { 0, 1000 }); // these entries are the two ends of the range for the middle TOF graph
            //Info.Add("defaultGate", new double[] { 6000, 2000 }); // the first entry is the centre of the gate, the second is the half width of the gate (upper TOF graph)


            //Counter Channels
            AddCounterChannel("westLeakage", digitalPatternBoardAddress+"/ctr0");
            AddCounterChannel("eastLeakage", digitalPatternBoardAddress+"/ctr1");

            AddAnalogInputChannel("cPlusMonitor", digitalPatternBoardAddress + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("cMinusMonitor", digitalPatternBoardAddress + "/ai3", AITerminalConfiguration.Rse);

            AddAnalogOutputChannel("cMinusPlate", digitalPatternBoardAddress + "/ao1");


            //USB Instruments
            Instruments.Add("FlowControllers", new AlicatFlowController("ASRL12::INSTR"));
            Instruments.Add("LatticeYAG", new BigSkyYAG("ASRL9::INSTR"));

            // Analog inputs
            //AddAnalogInputChannel("CavityRampVoltage", tclBoardProbe + "/ai0", AITerminalConfiguration.Rse); //tick
            //AddAnalogInputChannel("Pumpmaster", tclBoardProbe + "/ai3", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("Pumpp1", tclBoardProbe + "/ai4", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("Pumpp2", tclBoardProbe + "/ai5", AITerminalConfiguration.Rse);

            // Change on 27/06/2019: I move the pump PD inputs to tclBoardPump 
            // to increase the sample rate on tclBoardProbe
            //AddAnalogInputChannel("PumpCavityRampVoltage", digitalPatternBoardAddress + "/ai0", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("Pumpmaster", digitalPatternBoardAddress + "/ai2", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("Pumpp1", digitalPatternBoardAddress + "/ai4", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("Pumpp2", digitalPatternBoardAddress + "/ai6", AITerminalConfiguration.Rse);

            // Lasers locked to pump cavity
            //AddAnalogOutputChannel("KeopsysDiodeLaser", tclBoardProbe + "/ao4", -4, 4); //tick
            //AddAnalogOutputChannel("NewKeopsysDiodeLaser", tclBoardProbe + "/ao2", -4, 4); //tick

            // Length stabilisation for pump cavity
            //AddAnalogOutputChannel("PumpCavityLengthVoltage", digitalPatternBoardAddress + "/ao3", -10, 10); //tick

            ////TCL configuration of probe cavity
            //TCLConfig tclConfigProbe = new TCLConfig("Probe");
            //tclConfigProbe.Trigger = tclBoardProbe + "/PFI0";
            //tclConfigProbe.BaseRamp = "ProbeCavityRampVoltage";
            //tclConfigProbe.TCPChannel = 1190;
            //tclConfigProbe.DefaultScanPoints = 1000;
            //tclConfigProbe.AnalogSampleRate = 15000;
            //tclConfigProbe.SlaveVoltageLowerLimit = 0.0;
            //tclConfigProbe.SlaveVoltageUpperLimit = 10.0;
            //tclConfigProbe.PointsToConsiderEitherSideOfPeakInFWHMs = 4;
            //tclConfigProbe.MaximumNLMFSteps = 20;
            //tclConfigProbe.TriggerOnRisingEdge = false;


            //string probe = "ProbeCavity";

            //tclConfigProbe.AddCavity(probe);
            //tclConfigProbe.Cavities[probe].AddSlaveLaser("probelaser", "VIS2_probelaser");
            //tclConfigProbe.Cavities[probe].MasterLaser = "Probemaster";
            //tclConfigProbe.Cavities[probe].RampOffset = "ProbeCavityLengthVoltage";
            //tclConfigProbe.Cavities[probe].AddDefaultGain("Master", 0.2);
            //tclConfigProbe.Cavities[probe].AddDefaultGain("probelaser", 0.2);
            //tclConfigProbe.Cavities[probe].AddFSRCalibration("probelaser", 3.84);

            //Info.Add("DefaultCavity", tclConfigProbe);



        }

    }
}