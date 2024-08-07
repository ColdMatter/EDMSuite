using System;
using System.Collections;
using System.Runtime.Remoting;
using NationalInstruments.DAQmx;

using DAQ.Pattern;
using System.Collections.Generic;
using DAQ.TransferCavityLock2012;
using DAQ.Remoting;
using DAQ.WavemeterLock;

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
            string Analogboard = "/PXI1Slot4";
            string usbbreakout = "/Dev5";
            string smallUSBBoard = "/Dev6";

            //Info.Add("ScanMasterConfig", "D:\\EDM Suite Files\\Settings\\ScanMaster\\2024Mar06.xml");

            Boards.Add("daq", "/PXI1Slot5");
            Boards.Add("tclBoardProbe", "/PXI1Slot5");
            Boards.Add("pg", "/PXI1Slot6");
            Boards.Add("analog", "/PXI1Slot4");
            Boards.Add("smallUSBAnalog", "/Dev6");
            //Boards.Add("tclBoardPump", "/PXI1Slot6");

            //string tclBoardPump = (string)Boards["tclBoardPump"];
            string tclBoardProbe = (string)Boards["tclBoardProbe"];
            string pgBoard = (string)Boards["pg"];

            Info.Add("PGType", "integrated");
            Info.Add("PatternGeneratorBoard", digitalPatternBoardAddress);
            Info.Add("PGClockCounter", "/ctr0");

            Instruments.Add("tempController", new LakeShore336TemperatureController("ASRL4::INSTR"));

            #region Depreciated Code
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
            #endregion

            // Probe cavity inputs
            AddAnalogInputChannel("ProbeCavityRampVoltage", digitalPatternBoardAddress + "/ai8", AITerminalConfiguration.Rse); //tick this is the ramp impute
            AddAnalogInputChannel("Probemaster", digitalPatternBoardAddress + "/ai14", AITerminalConfiguration.Rse); //tick this is the 780nm photodiode
            AddAnalogInputChannel("Probep1", digitalPatternBoardAddress + "/ai12", AITerminalConfiguration.Rse); //tick //this is the probe laser photodiode input
            AddAnalogInputChannel("Probep2", digitalPatternBoardAddress + "/ai10", AITerminalConfiguration.Rse); //tick //this is the probe laser photodiode input - v3
            AddAnalogInputChannel("Probep3", digitalPatternBoardAddress + "/ai11", AITerminalConfiguration.Rse); //slowing photothis photodiode does not exist but because we want to be able to scan the voltage of this laser through tcl we need this here
            AddAnalogInputChannel("Probep4", digitalPatternBoardAddress + "/ai13", AITerminalConfiguration.Rse); //now v1 //v2 photodiode slowing photothis photodiode does not exist but because we want to be able to scan the voltage of this laser through tcl we need this here
            AddAnalogInputChannel("Probep5", digitalPatternBoardAddress + "/ai19", AITerminalConfiguration.Rse); //used for the attisse
            AddAnalogInputChannel("Probep6", digitalPatternBoardAddress + "/ai16", AITerminalConfiguration.Rse); //placeholder for v2 aom

            // Lasers locked to Probe cavity
            AddAnalogOutputChannel("slowingLaser", digitalPatternBoardAddress + "/ao2", -4, 4); // connected to slowing v0 now
            AddAnalogOutputChannel("LatticeProbeLaser", digitalPatternBoardAddress + "/ao1", 0, 10); //tick //this is the analogue ouput port on the DAQ card for the frequency feedback of the laser (piezo in our case)
            AddAnalogOutputChannel("ProbeCavityLengthVoltage", digitalPatternBoardAddress + "/ao0", -9, 0); //tick //this is the voltage that stabilises the length of the cavity (needs to have 0 because thats where its intialises)
            AddAnalogOutputChannel("v3laser", digitalPatternBoardAddress + "/ao3", 0, 10);
            AddAnalogOutputChannel("mattisse", Analogboard + "/ao1", 0, 10);
            AddAnalogOutputChannel("irecdl", Analogboard + "/ao13", 0, 10);
            AddAnalogOutputChannel("v2aom", Analogboard + "/ao2", -10, 10);
            AddAnalogOutputChannel("v1laser", smallUSBBoard + "/ao1", -10, 10);
            AddAnalogOutputChannel("ClassicECDL", Analogboard + "/ao4", -10, 10);
            //

            // V2 cavity inputs
            AddAnalogInputChannel("V2CavityRampVoltage", digitalPatternBoardAddress + "/ai4", AITerminalConfiguration.Rse); //tick this is the ramp input
            AddAnalogInputChannel("V2CavityMaster", digitalPatternBoardAddress + "/ai3", AITerminalConfiguration.Rse); //tick this is the 780nm photodiode
            AddAnalogInputChannel("V2CavityP1", digitalPatternBoardAddress + "/ai5", AITerminalConfiguration.Rse); //tick //this is the v2 laser photodiode input

            //Outputs for V2 cavity
            AddAnalogOutputChannel("V2CavityLengthVoltage", ExtraBoard + "/ao1", -10, 10);  //this is the voltage that stabilises the length of the cavity
            AddAnalogOutputChannel("v2laser", ExtraBoard + "/ao0", 0, 10);


            //Configuration for wavemeterlock
            WavemeterLockConfig wmlConfig = new WavemeterLockConfig("Default");
            /*wmlConfig.AddSlaveLaser("LatticeProbeLaser", "LatticeProbeLaser", 5);//Laser name, analog channel, wavemeter channel
            wmlConfig.AddLaserConfiguration("LatticeProbeLaser", 542.809112, 5, 1); //("YourLaserName", SetFrequencyInTHz, PGain, IGain)*/
            wmlConfig.AddSlaveLaser("IR-ECDL", "irecdl", 5);//Laser name, analog channel, wavemeter channel
            wmlConfig.AddSlaveLaser("Classic-ECDL", "ClassicECDL", 5);//Laser name, analog channel, wavemeter channel
            wmlConfig.AddLaserConfiguration("IR-ECDL", 288.7301, -100, -100); //("YourLaserName", SetFrequencyInTHz, PGain, IGain)
            wmlConfig.AddLaserConfiguration("Classic-ECDL", 446.7996977, -100, -100); //("YourLaserName", SetFrequencyInTHz, PGain, IGain)
            Info.Add("Default", wmlConfig);

            //TCL coniguration for Lattice EDM
            TCLConfig tclConfigProbe = new TCLConfig("Probe");
            tclConfigProbe.Trigger = digitalPatternBoardAddress + "/PFI0";
            tclConfigProbe.BaseRamp = "ProbeCavityRampVoltage";
            tclConfigProbe.TCPChannel = 1190;
            tclConfigProbe.DefaultScanPoints = 700;
            tclConfigProbe.PointsToConsiderEitherSideOfPeakInFWHMs = 12;
            tclConfigProbe.AnalogSampleRate = 20000;// 245000 * 1 / 8;//reduce number 12/3/21 by factr 10
            tclConfigProbe.MaximumNLMFSteps = 20;
            tclConfigProbe.TriggerOnRisingEdge = true;
            
            string probe = "ProbeCavity"; 
            tclConfigProbe.AddCavity(probe);
            tclConfigProbe.Cavities[probe].AddSlaveLaser("LatticeProbeLaser", "Probep1");
            tclConfigProbe.Cavities[probe].AddSlaveLaser("slowingLaser", "Probep3");
            tclConfigProbe.Cavities[probe].AddSlaveLaser("v3laser", "Probep2");
            tclConfigProbe.Cavities[probe].AddSlaveLaser("mattisse", "Probep5");
            tclConfigProbe.Cavities[probe].AddSlaveLaser("v1laser", "Probep4");

            tclConfigProbe.Cavities[probe].MasterLaser = "Probemaster";
            tclConfigProbe.Cavities[probe].RampOffset = "ProbeCavityLengthVoltage";
            tclConfigProbe.Cavities[probe].AddDefaultGain("Master", -0.04);
            tclConfigProbe.Cavities[probe].AddDefaultGain("LatticeProbeLaser", 1.00);
            tclConfigProbe.Cavities[probe].AddFSRCalibration("LatticeProbeLaser", 3.84);
            tclConfigProbe.Cavities[probe].AddDefaultGain("slowingLaser", 0.1);
            tclConfigProbe.Cavities[probe].AddFSRCalibration("slowingLaser", 3.84);
            tclConfigProbe.Cavities[probe].AddDefaultGain("v3laser", 0.1);
            tclConfigProbe.Cavities[probe].AddFSRCalibration("v3laser", 3.84);
            tclConfigProbe.Cavities[probe].AddDefaultGain("mattisse", 0.1);
            tclConfigProbe.Cavities[probe].AddFSRCalibration("mattisse", 3.84);
            tclConfigProbe.Cavities[probe].AddDefaultGain("v1laser", 0.1);
            tclConfigProbe.Cavities[probe].AddFSRCalibration("v1laser", 3.84);

            string v2cavity = "V2Cavity";
            tclConfigProbe.AddCavity(v2cavity);
            tclConfigProbe.Cavities[v2cavity].AddSlaveLaser("v2laser", "V2CavityP1");
            tclConfigProbe.Cavities[v2cavity].AddSlaveLaser("v2aom", "Probep6");
            

            tclConfigProbe.Cavities[v2cavity].MasterLaser = "V2CavityMaster";
            tclConfigProbe.Cavities[v2cavity].RampOffset = "V2CavityLengthVoltage";
            tclConfigProbe.Cavities[v2cavity].AddDefaultGain("Master", -0.04);
            tclConfigProbe.Cavities[v2cavity].AddDefaultGain("v2laser", 1.00);
            tclConfigProbe.Cavities[v2cavity].AddFSRCalibration("v2laser", 3.84);
            tclConfigProbe.Cavities[v2cavity].AddDefaultGain("v2aom", 0.1);
            tclConfigProbe.Cavities[v2cavity].AddFSRCalibration("v2aom", 3.84);



            //Info.Add("TCLConfigPump", tclConfigPump);
            Info.Add("TCLConfigProbe", tclConfigProbe);
            Info.Add("DefaultCavity", tclConfigProbe);
            Info.Add("TCLConfig", tclConfigProbe);
            Info.Add("tclConfigPump", tclConfigProbe);

            #region Depreciated Code
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
            #endregion


            #region Lattice scanmaster
            ////
            AddDigitalOutputChannel("q", digitalPatternBoardAddress, 0, 6);
            AddDigitalOutputChannel("flash", digitalPatternBoardAddress, 0, 3);
            AddDigitalOutputChannel("q2", digitalPatternBoardAddress, 0, 14);
            //AddDigitalOutputChannel("flash2", digitalPatternBoardAddress, 0, 13);
            AddDigitalOutputChannel("magswitch", digitalPatternBoardAddress, 0, 13);
            AddDigitalOutputChannel("scopetrigger", digitalPatternBoardAddress, 0, 15);
            AddDigitalOutputChannel("camerashutter", digitalPatternBoardAddress, 0, 16);
            //AddDigitalOutputChannel("analogPatternTrigger", digitalPatternBoardAddress, 0, 8);//connect to daq board PFI 0 - not needed 01/2/22

            //AddDigitalOutputChannel("analogtriggertest0", digitalPatternBoardAddress, 0, 4);
            //AddDigitalOutputChannel("sourceHeater", digitalPatternBoardAddress, 0, 5);
            //AddDigitalOutputChannel("cryoCooler", digitalPatternBoardAddress, 0, 9);
            //AddDigitalOutputChannel("unused", digitalPatternBoardAddress, 0, 2);
            AddDigitalOutputChannel("valve", digitalPatternBoardAddress, 0, 11);//it seeems to like having this here
            AddDigitalOutputChannel("detector", digitalPatternBoardAddress, 0, 2);
            AddDigitalOutputChannel("detectorprime", digitalPatternBoardAddress, 0, 1);
            AddDigitalOutputChannel("shutter2on", digitalPatternBoardAddress, 0, 4); //NOT A SHUTTER, V0 Slowing AOM Near
            AddDigitalOutputChannel("shutter2off", digitalPatternBoardAddress, 0, 5);//Unused it seems
            AddDigitalOutputChannel("shutterSTEVE1off", digitalPatternBoardAddress, 0, 7);//STEVE newport Shutter A Signal (closure TTL)
            // shutter 1 on is done in the switch line of the pattern (port 0 0)
            AddDigitalOutputChannel("shutterslow", digitalPatternBoardAddress, 0, 8); //UNKNOWN
            AddDigitalOutputChannel("shutterSTEVE2", digitalPatternBoardAddress, 0, 9); //Thorlabs Shutter for V1 and V2 slowing beams
            AddDigitalOutputChannel("shutterv2", digitalPatternBoardAddress, 0, 10); //NOT A SHUTTER ITS THE V2 AOM
            ////AddAnalogInputChannel("4Kthermistor", analogPatternBoardAddress + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("pmt", ExtraBoard + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("pmt2", ExtraBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("photodiode", ExtraBoard + "/ai6", AITerminalConfiguration.Differential);
            AddDigitalOutputChannel("shutterslow2", digitalPatternBoardAddress, 0, 12); //V0 Uniblitz Shutter
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

            AddCounterChannel("pmtCounter", ExtraBoard + "/ctr0");  // channel used for photon counting PFI8 is source, PFI9 is gate 
            AddCounterChannel("sample clock", ExtraBoard + "/ctr1"); // channel used for photon counting PFI13 is output 


            AddCounterChannel("westLeakage", digitalPatternBoardAddress+"/ctr0");
            AddCounterChannel("eastLeakage", digitalPatternBoardAddress+"/ctr1");

            AddAnalogInputChannel("cPlusMonitor", digitalPatternBoardAddress + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("cMinusMonitor", digitalPatternBoardAddress + "/ai7", AITerminalConfiguration.Rse);

            AddAnalogOutputChannel("cMinusPlate", digitalPatternBoardAddress + "/ao1");

            //Things for the hardware controller
            AddAnalogInputChannel("pressureGaugeS", usbbreakout + "/ai1", AITerminalConfiguration.Rse); //Source pressure
            AddAnalogInputChannel("Pressure_Downstream", usbbreakout + "/ai3", AITerminalConfiguration.Rse); //Downstream pressure

            //USB Instruments
            Instruments.Add("FlowControllers", new AlicatFlowController("ASRL12::INSTR"));
            Instruments.Add("LatticeYAG", new BigSkyYAG("ASRL9::INSTR"));
            #region Depreciated Code
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
            #endregion
            #endregion



        }

    }
}