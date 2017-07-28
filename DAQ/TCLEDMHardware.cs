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

            // Cavity inputs for the cavity that controls the Pump lasers
            AddAnalogInputChannel("PumpCavityRampVoltage", tclBoardPump + "/ai8", AITerminalConfiguration.Rse); //tick
            AddAnalogInputChannel("Pumpmaster", tclBoardPump + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("Pumpp1", tclBoardPump + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("Pumpp2", tclBoardPump + "/ai3", AITerminalConfiguration.Rse);

            // Lasers locked to pump cavity
            AddAnalogOutputChannel("KeopsysDiodeLaser", tclBoardPump + "/ao2", -4, 4); //tick
            AddAnalogOutputChannel("MenloPZT", tclBoardPump + "/ao0", 0, 10); //tick

            // Length stabilisation for pump cavity
            AddAnalogOutputChannel("PumpCavityLengthVoltage", tclBoardPump + "/ao1", -10, 10); //tick

            //TCL configuration for pump cavity
            TCLConfig tcl1 = new TCLConfig("Pump Cavity");
            tcl1.AddLaser("MenloPZT", "Pumpp1");
            tcl1.AddLaser("KeopsysDiodeLaser", "Pumpp2");
            tcl1.Trigger = tclBoardPump + "/PFI0";
            tcl1.Cavity = "PumpCavityRampVoltage";
            tcl1.MasterLaser = "Pumpmaster";
            tcl1.Ramp = "PumpCavityLengthVoltage";
            tcl1.TCPChannel = 1190;
            tcl1.AnalogSampleRate = 61250;
            tcl1.DefaultScanPoints = 500;
            tcl1.MaximumNLMFSteps = 20;
            tcl1.PointsToConsiderEitherSideOfPeakInFWHMs = 2.5;
            tcl1.TriggerOnRisingEdge = false;
            tcl1.AddFSRCalibration("MenloPZT", 3.84);
            tcl1.AddFSRCalibration("KeopsysDiodeLaser", 3.84);
            tcl1.AddDefaultGain("Master", 0.3);
            tcl1.AddDefaultGain("MenloPZT", -0.2);
            tcl1.AddDefaultGain("KeopsysDiodeLaser", 4);
            Info.Add("PumpCavity", tcl1);
            Info.Add("DefaultCavity", tcl1);

            // Probe cavity inputs
            AddAnalogInputChannel("ProbeRampVoltage", tclBoardProbe + "/ai0", AITerminalConfiguration.Rse); //tick
            AddAnalogInputChannel("Probemaster", tclBoardProbe + "/ai1", AITerminalConfiguration.Rse); //tick
            AddAnalogInputChannel("Probep1", tclBoardProbe + "/ai2", AITerminalConfiguration.Rse); //tick

            // Lasers locked to Probe cavity
            AddAnalogOutputChannel("TopticaSHGPZT", tclBoardProbe + "/ao0", -4, 4); //tick
            AddAnalogOutputChannel("ProbeCavityLengthVoltage", tclBoardProbe + "/ao1", -10, 10); //tick

            // TCL configuration for Probe cavity
            TCLConfig tcl2 = new TCLConfig("Probe Cavity");
            tcl2.AddLaser("TopticaSHGPZT", "Probep1");
            tcl2.Trigger = tclBoardProbe + "/PFI0";
            tcl2.Cavity = "ProbeRampVoltage";
            tcl2.MasterLaser = "Probemaster";
            tcl2.Ramp = "ProbeCavityLengthVoltage";
            tcl2.TCPChannel = 1191;
            tcl2.AnalogSampleRate = 61250 / 2;
            tcl2.DefaultScanPoints = 250;
            tcl2.MaximumNLMFSteps = 20;
            tcl2.PointsToConsiderEitherSideOfPeakInFWHMs = 12;
            tcl2.AddFSRCalibration("TopticaSHGPZT", 3.84);
            tcl2.TriggerOnRisingEdge = false;
            tcl2.AddDefaultGain("Master", 0.4);
            tcl2.AddDefaultGain("TopticaSHGPZT", 0.04);
            Info.Add("ProbeCavity", tcl2);
            //Info.Add("DefaultCavity", tcl2);
        }
    }
}
