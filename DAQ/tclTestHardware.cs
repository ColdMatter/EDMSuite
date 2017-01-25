using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NationalInstruments.DAQmx;
using DAQ.Pattern;
using DAQ.Remoting;
using DAQ.TransferCavityLock2012;
using System.Runtime.Remoting;
using System.Collections.Generic;

namespace DAQ.HAL
{
	class tclTestHardware : DAQ.HAL.Hardware
	{
		public tclTestHardware()
		{
// add boards
			Boards.Add("TCLBoard", "/dev1");				// add a hardware board: (board name, NI-MAX identifier -- all lower case?)
			string TCLBoard = (string)Boards["TCLBoard"];	// add a string for that board

// transfer-cavity lock configuration
			TCLConfig tcl = new TCLConfig("DefaultCavity");
			tcl.AnalogSampleRate = 1000;					// make it slow for testing
			tcl.MaxInputVoltage = 5.0;						// and using an USB card for now to make the hardware configuration happy

			tcl.AddLaser("slaveLaser", "p1");				// laser name and its associated photodiode (photodiode hardware connection defined below)
			tcl.Trigger = TCLBoard + "/PFI0";
			tcl.Cavity = "cavity";
			tcl.MasterLaser = "master";
			tcl.Ramp = "rampfb";
			tcl.TCPChannel = 1190;
			Info.Add("DefaultCavity", tcl);

// add info:
//			Info.Add("analogTrigger0", (string)Boards["analogIn"] + "/PFI0");

// map analog and digital pins:
			AddAnalogInputChannel("master", TCLBoard + "/ai0", AITerminalConfiguration.Rse);
			AddAnalogInputChannel("cavity", TCLBoard + "/ai1", AITerminalConfiguration.Rse);
			AddAnalogInputChannel("p1", TCLBoard + "/ai2", AITerminalConfiguration.Rse);

			AddAnalogOutputChannel("slaveLaser", TCLBoard + "/ao0", 0.0, 5.0);
			AddAnalogOutputChannel("rampfb", TCLBoard + "/ao1", 0.0, 5.0);

// add instruments:
			//Instruments.Add("green", new HP8657ASynth("GPIB0::7::INSTR"));
		}

	}
}
