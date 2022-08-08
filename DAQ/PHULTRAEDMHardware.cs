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

            Boards.Add("tclBoardProbe", "/PXI1Slot4");
            Boards.Add("pg", "/PXI1Slot6");
            //Boards.Add("tclBoardPump", "/PXI1Slot6");

            //string tclBoardPump = (string)Boards["tclBoardPump"];
            string tclBoardProbe = (string)Boards["tclBoardProbe"];
            string pgBoard = (string)Boards["pg"];

            Info.Add("PGType", "integrated");
            Info.Add("PatternGeneratorBoard", digitalPatternBoardAddress);
            Info.Add("PGClockCounter", "/ctr0");
            // Analog inputs
            //AddAnalogInputChannel("CavityRampVoltage", tclBoardProbe + "/ai0", AITerminalConfiguration.Rse); //tick
            //AddAnalogInputChannel("Pumpmaster", tclBoardProbe + "/ai3", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("Pumpp1", tclBoardProbe + "/ai4", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("Pumpp2", tclBoardProbe + "/ai5", AITerminalConfiguration.Rse);

            // Change on 27/06/2019: I move the pump PD inputs to tclBoardPump 
            // to increase the sample rate on tclBoardProbe
            AddAnalogInputChannel("PumpCavityRampVoltage", digitalPatternBoardAddress + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("Pumpmaster", digitalPatternBoardAddress + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("Pumpp1", digitalPatternBoardAddress + "/ai4", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("Pumpp2", digitalPatternBoardAddress + "/ai6", AITerminalConfiguration.Rse);

            // Lasers locked to pump cavity
            AddAnalogOutputChannel("KeopsysDiodeLaser", tclBoardProbe + "/ao4", -4, 4); //tick
            AddAnalogOutputChannel("NewKeopsysDiodeLaser", tclBoardProbe + "/ao2", -4, 4); //tick

            // Length stabilisation for pump cavity
            AddAnalogOutputChannel("PumpCavityLengthVoltage", digitalPatternBoardAddress + "/ao3", -10, 10); //tick

            // Probe cavity inputs
            AddAnalogInputChannel("ProbeCavityRampVoltage", digitalPatternBoardAddress + "/ai8", AITerminalConfiguration.Rse); //tick this is the ramp impute
            AddAnalogInputChannel("Probemaster", digitalPatternBoardAddress + "/ai10", AITerminalConfiguration.Rse); //tickthis is the 780nm photodiode
            AddAnalogInputChannel("Probep1", digitalPatternBoardAddress + "/ai12", AITerminalConfiguration.Rse); //tick

            // Lasers locked to Probe cavity
            AddAnalogOutputChannel("TopticaSHGPZT", digitalPatternBoardAddress + "/ao1", -0.2, 0.2); //tick
            AddAnalogOutputChannel("ProbeCavityLengthVoltage", digitalPatternBoardAddress + "/ao0", 0, 10); //tick

            //TCL configuration for pump cavity: 13/01/2021 (Chris)
            TCLConfig tclConfigPump = new TCLConfig("Pump");
            tclConfigPump.Trigger = digitalPatternBoardAddress + "/PFI0";
            tclConfigPump.BaseRamp = "PumpCavityRampVoltage";
            tclConfigPump.TCPChannel = 1191;
            tclConfigPump.DefaultScanPoints = 500;
            tclConfigPump.PointsToConsiderEitherSideOfPeakInFWHMs = 12;
            tclConfigPump.AnalogSampleRate = 61250;
            tclConfigPump.MaximumNLMFSteps = 20;
            tclConfigPump.TriggerOnRisingEdge = true; // changed this 09/3/21
            string pump = "PumpCavity";

            tclConfigPump.AddCavity(pump);
            tclConfigPump.Cavities[pump].AddSlaveLaser("NewKeopsysDiodeLaser", "Pumpp1");
            tclConfigPump.Cavities[pump].AddSlaveLaser("KeopsysDiodeLaser", "Pumpp2");
            tclConfigPump.Cavities[pump].MasterLaser = "Pumpmaster";
            tclConfigPump.Cavities[pump].RampOffset = "PumpCavityLengthVoltage";
            tclConfigPump.Cavities[pump].AddDefaultGain("Master", 0.3);
            tclConfigPump.Cavities[pump].AddDefaultGain("NewKeopsysDiodeLaser", -0.2);
            tclConfigPump.Cavities[pump].AddDefaultGain("KeopsysDiodeLaser", 1);
            tclConfigPump.Cavities[pump].AddFSRCalibration("NewKeopsysDiodeLaser", 3.84);
            tclConfigPump.Cavities[pump].AddFSRCalibration("KeopsysDiodeLaser", 3.84);

            //TCL configuration of probe cavity: 27/06/2019 (Chris)
            TCLConfig tclConfigProbe = new TCLConfig("Probe");
            tclConfigProbe.Trigger = digitalPatternBoardAddress + "/PFI0"; 
            tclConfigProbe.BaseRamp = "ProbeCavityRampVoltage";
            tclConfigProbe.TCPChannel = 1190;
            tclConfigProbe.DefaultScanPoints = 500 * 1 / 4;
            tclConfigProbe.PointsToConsiderEitherSideOfPeakInFWHMs = 12;
            tclConfigProbe.AnalogSampleRate = 61250 * 1 / 2;//reduce number 12/3/21 by factr 10
            tclConfigProbe.MaximumNLMFSteps = 20;
            tclConfigProbe.TriggerOnRisingEdge = true;
            string probe = "ProbeCavity";

            tclConfigProbe.AddCavity(probe);
            tclConfigProbe.Cavities[probe].AddSlaveLaser("TopticaSHGPZT", "Probep1");
            tclConfigProbe.Cavities[probe].MasterLaser = "Probemaster";
            tclConfigProbe.Cavities[probe].RampOffset = "ProbeCavityLengthVoltage";
            tclConfigProbe.Cavities[probe].AddDefaultGain("Master", 0.4);
            tclConfigProbe.Cavities[probe].AddDefaultGain("TopticaSHGPZT", 0.04);
            tclConfigProbe.Cavities[probe].AddFSRCalibration("TopticaSHGPZT", 3.84);

            Info.Add("TCLConfigPump", tclConfigPump);
            Info.Add("TCLConfigProbe", tclConfigProbe);
            Info.Add("DefaultCavity", tclConfigProbe);





            ////Boards.Add(digitalPatternBoardName, digitalPatternBoardAddress);

            ////Boards.Add("tclBoardProbe", "/PXI1Slot5");
            //string tclBoardProbe = (string)Boards["tclBoardProbe"];


            ////string analogPatternBoardName = "analogPattern";//NI PXIe-6229
            ////string analogPatternBoardAddress = "/PXI1Slot6";
            ////Boards.Add(analogPatternBoardName, analogPatternBoardAddress);


            ////string pgBoard = (string)Boards["pg"];

            AddDigitalOutputChannel("q", digitalPatternBoardAddress, 0, 6);
            AddDigitalOutputChannel("flash", digitalPatternBoardAddress, 0, 5);
            AddDigitalOutputChannel("analogPatternTrigger", digitalPatternBoardAddress, 0, 4);//connect to daq board PFI 0
            AddDigitalOutputChannel("sourceHeater", digitalPatternBoardAddress, 0, 3);
            AddDigitalOutputChannel("cryoCooler", digitalPatternBoardAddress, 0, 1);
            AddDigitalOutputChannel("unused", digitalPatternBoardAddress, 0, 2);

            ////AddAnalogInputChannel("4Kthermistor", analogPatternBoardAddress + "/ai3", AITerminalConfiguration.Rse);
            ////AddAnalogInputChannel("pmt", analogPatternBoardAddress + "/ai10", AITerminalConfiguration.Rse);
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

            ////Info.Add("analogTrigger0", analogPatternBoardAddress + "/PFI2");

            Info.Add("defaultTOFRange", new double[] { 4000, 12000 }); // these entries are the two ends of the range for the upper TOF graph
            Info.Add("defaultTOF2Range", new double[] { 0, 1000 }); // these entries are the two ends of the range for the middle TOF graph
            Info.Add("defaultGate", new double[] { 6000, 2000 }); // the first entry is the centre of the gate, the second is the half width of the gate (upper TOF graph)


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



            //Counter Channels
            AddCounterChannel("westLeakage", pgBoard +"/pfi7");
            AddCounterChannel("eastLeakage", pgBoard +"/pfi8");
        }

    }
}