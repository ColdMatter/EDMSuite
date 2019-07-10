using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NationalInstruments.DAQmx;
using DAQ.Pattern;
using DAQ.TransferCavityLock2012;
using DAQ.Remoting;

namespace DAQ.HAL
{
    public class TCLEDMHardware : DAQ.HAL.Hardware
    {
        public override void ConnectApplications()
        {
        }

        public TCLEDMHardware()
        {

            //Add the boards
            Boards.Add("tclBoardPump", "/PXI1Slot5");
            Boards.Add("tclBoardProbe", "/PXI1Slot2");

            string tclBoardPump = (string)Boards["tclBoardPump"];
            string tclBoardProbe = (string)Boards["tclBoardProbe"];

            // Analog inputs
            //AddAnalogInputChannel("CavityRampVoltage", tclBoardProbe + "/ai0", AITerminalConfiguration.Rse); //tick
            //AddAnalogInputChannel("Pumpmaster", tclBoardProbe + "/ai3", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("Pumpp1", tclBoardProbe + "/ai4", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("Pumpp2", tclBoardProbe + "/ai5", AITerminalConfiguration.Rse);

            // Change on 27/06/2019: I move the pump PD inputs to tclBoardPump 
            // to increase the sample rate on tclBoardProbe
            AddAnalogInputChannel("PumpCavityRampVoltage", tclBoardPump + "/ai8", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("Pumpmaster", tclBoardPump + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("Pumpp1", tclBoardPump + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("Pumpp2", tclBoardPump + "/ai3", AITerminalConfiguration.Rse);

            // Lasers locked to pump cavity
            AddAnalogOutputChannel("KeopsysDiodeLaser", tclBoardPump + "/ao2", -4, 4); //tick
            AddAnalogOutputChannel("MenloPZT", tclBoardPump + "/ao0", 0, 10); //tick

            // Length stabilisation for pump cavity
            AddAnalogOutputChannel("PumpCavityLengthVoltage", tclBoardPump + "/ao1", -10, 10); //tick

            // Probe cavity inputs
            AddAnalogInputChannel("ProbeCavityRampVoltage", tclBoardProbe + "/ai0", AITerminalConfiguration.Rse); //tick
            AddAnalogInputChannel("Probemaster", tclBoardProbe + "/ai1", AITerminalConfiguration.Rse); //tick
            AddAnalogInputChannel("Probep1", tclBoardProbe + "/ai2", AITerminalConfiguration.Rse); //tick

            // Lasers locked to Probe cavity
            AddAnalogOutputChannel("TopticaSHGPZT", tclBoardProbe + "/ao0", -4, 4); //tick
            AddAnalogOutputChannel("ProbeCavityLengthVoltage", tclBoardProbe + "/ao1", -10, 10); //tick

            //TCL configuration for pump cavity: 27/06/2019 (Chris)
            TCLConfig tclConfigPump = new TCLConfig("Pump");
            tclConfigPump.Trigger = tclBoardPump + "/PFI0";
            tclConfigPump.BaseRamp = "PumpCavityRampVoltage";
            tclConfigPump.TCPChannel = 1191;
            tclConfigPump.DefaultScanPoints = 500;
            tclConfigPump.PointsToConsiderEitherSideOfPeakInFWHMs = 4;
            tclConfigPump.AnalogSampleRate = 61250;
            tclConfigPump.MaximumNLMFSteps = 20;
            tclConfigPump.TriggerOnRisingEdge = false;
            string pump = "PumpCavity";

            tclConfigPump.AddCavity(pump);
            tclConfigPump.Cavities[pump].AddSlaveLaser("MenloPZT", "Pumpp1");
            tclConfigPump.Cavities[pump].AddSlaveLaser("KeopsysDiodeLaser", "Pumpp2");
            tclConfigPump.Cavities[pump].MasterLaser = "Pumpmaster";
            tclConfigPump.Cavities[pump].RampOffset = "PumpCavityLengthVoltage";
            tclConfigPump.Cavities[pump].AddDefaultGain("Master", 0.3);
            tclConfigPump.Cavities[pump].AddDefaultGain("MenloPZT", -0.2);
            tclConfigPump.Cavities[pump].AddDefaultGain("KeopsysDiodeLaser", 1);
            tclConfigPump.Cavities[pump].AddFSRCalibration("MenloPZT", 3.84);
            tclConfigPump.Cavities[pump].AddFSRCalibration("KeopsysDiodeLaser", 3.84);

            //TCL configuration of probe cavity: 27/06/2019 (Chris)
            TCLConfig tclConfigProbe = new TCLConfig("Probe");
            tclConfigProbe.Trigger = tclBoardProbe + "/PFI0";
            tclConfigProbe.ExtTTLClock = tclBoardProbe + "/PFI1";
            tclConfigProbe.BaseRamp = "ProbeCavityRampVoltage";
            tclConfigProbe.TCPChannel = 1190;
            tclConfigProbe.DefaultScanPoints = 500 * 1 / 2;
            tclConfigProbe.PointsToConsiderEitherSideOfPeakInFWHMs = 4;
            tclConfigProbe.AnalogSampleRate = 61250 * 1 / 2;
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

            ////TCL configuration for pump cavity
            //TCLConfig tcl1 = new TCLConfig("Pump Cavity");
            //tcl1.AddLaser("MenloPZT", "Pumpp1");
            //tcl1.AddLaser("KeopsysDiodeLaser", "Pumpp2");
            //tcl1.Trigger = tclBoardPump + "/PFI0";
            //tcl1.Cavity = "PumpCavityRampVoltage";
            //tcl1.MasterLaser = "Pumpmaster";
            //tcl1.Ramp = "PumpCavityLengthVoltage";
            //tcl1.TCPChannel = 1190;
            //tcl1.AnalogSampleRate = 61250;
            //tcl1.DefaultScanPoints = 500;
            //tcl1.MaximumNLMFSteps = 20;
            //tcl1.PointsToConsiderEitherSideOfPeakInFWHMs = 2.5;
            //tcl1.TriggerOnRisingEdge = false;
            //tcl1.AddFSRCalibration("MenloPZT", 3.84);
            //tcl1.AddFSRCalibration("KeopsysDiodeLaser", 3.84);
            //tcl1.AddDefaultGain("Master", 0.3);
            //tcl1.AddDefaultGain("MenloPZT", -0.2);
            //tcl1.AddDefaultGain("KeopsysDiodeLaser", 4);
            //Info.Add("PumpCavity", tcl1);
            //Info.Add("DefaultCavity", tcl1);



            //// TCL configuration for Probe cavity
            //TCLConfig tcl2 = new TCLConfig("Probe Cavity");
            //tcl2.AddLaser("TopticaSHGPZT", "Probep1");
            //tcl2.Trigger = tclBoardProbe + "/PFI0";
            //tcl2.Cavity = "ProbeRampVoltage";
            //tcl2.MasterLaser = "Probemaster";
            //tcl2.Ramp = "ProbeCavityLengthVoltage";
            //tcl2.TCPChannel = 1191;
            //tcl2.AnalogSampleRate = 61250 / 2;
            //tcl2.DefaultScanPoints = 250;
            //tcl2.MaximumNLMFSteps = 20;
            //tcl2.PointsToConsiderEitherSideOfPeakInFWHMs = 12;
            //tcl2.AddFSRCalibration("TopticaSHGPZT", 3.84);
            //tcl2.TriggerOnRisingEdge = false;
            //tcl2.AddDefaultGain("Master", 0.4);
            //tcl2.AddDefaultGain("TopticaSHGPZT", 0.04);
            //Info.Add("ProbeCavity", tcl2);
            //Info.Add("DefaultCavity", tcl2);

            // TCL configuration for pump and probe cavities
            //TCLConfig tclConfig = new TCLConfig("Pump and Probe");
            //tclConfig.Trigger = tclBoardProbe + "/PFI0";
            //tclConfig.BaseRamp = "CavityRampVoltage";
            //tclConfig.TCPChannel = 1190;
            //tclConfig.DefaultScanPoints = 500 * 5 / 2;
            //tclConfig.PointsToConsiderEitherSideOfPeakInFWHMs = 12;
            //tclConfig.AnalogSampleRate = 61250 * 5 / 2;
            //tclConfig.MaximumNLMFSteps = 20;
            //tclConfig.TriggerOnRisingEdge = false;
            //string pump = "PumpCavity";
            //string probe = "ProbeCavity";

            //tclConfig.AddCavity(pump);
            //tclConfig.Cavities[pump].AddSlaveLaser("MenloPZT", "Pumpp1");
            //tclConfig.Cavities[pump].AddSlaveLaser("KeopsysDiodeLaser", "Pumpp2");
            //tclConfig.Cavities[pump].MasterLaser = "Pumpmaster";
            //tclConfig.Cavities[pump].RampOffset = "PumpCavityLengthVoltage";
            //tclConfig.Cavities[pump].AddDefaultGain("Master", 0.3);
            //tclConfig.Cavities[pump].AddDefaultGain("MenloPZT", -0.2);
            //tclConfig.Cavities[pump].AddDefaultGain("KeopsysDiodeLaser", 1);
            //tclConfig.Cavities[pump].AddFSRCalibration("MenloPZT", 3.84);
            //tclConfig.Cavities[pump].AddFSRCalibration("KeopsysDiodeLaser", 3.84);

            //tclConfig.AddCavity(probe);
            //tclConfig.Cavities[probe].AddSlaveLaser("TopticaSHGPZT", "Probep1");
            //tclConfig.Cavities[probe].MasterLaser = "Probemaster";
            //tclConfig.Cavities[probe].RampOffset = "ProbeCavityLengthVoltage";
            //tclConfig.Cavities[probe].AddDefaultGain("Master", 0.4);
            //tclConfig.Cavities[probe].AddDefaultGain("TopticaSHGPZT", 0.04);
            //tclConfig.Cavities[probe].AddFSRCalibration("TopticaSHGPZT", 3.84);

            //Info.Add("TCLConfig", tclConfig);
            //Info.Add("DefaultCavity", tclConfig);
        }
    }
}
