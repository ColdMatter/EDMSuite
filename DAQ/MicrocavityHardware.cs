using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;
using DAQ.Remoting;
using DAQ.TransferCavityLock2012;
using System.Runtime.Remoting;
using System.Collections.Generic;

namespace DAQ.HAL
{
	
	/// <summary>
	/// This is the specific hardware that the microcavity experiment has. This class conforms
	/// to the Hardware interface.
	/// </summary>
	public class MicrocavityHardware : DAQ.HAL.Hardware
	{
        protected BathCryostat bathCryostat;
        public BathCryostat BATHCRYOSTAT
        {
            get { return bathCryostat; }
        }

		public MicrocavityHardware()
		{

			// add the boards
			Boards.Add("daq", "/dev1");
            Boards.Add("pg", "/dev1");
            Boards.Add("TCLBoard", "/dev2");
			string daqBoard = (string)Boards["daq"];
            string pgBoard = (string)Boards["pg"];
            string TCLBoard = (string)Boards["TCLBoard"];

            //TCL Configuration
            TCLConfig tcl = new TCLConfig("Microcavity McCavity");
            tcl.AddLaser("tclTiSapphControl", "tclpdTiSapph");
            tcl.AddLaser("tclECDLControl", "tclpdECDL");
            tcl.Trigger = TCLBoard + "/PFI0";
            tcl.Cavity = "cavityRampMonitor";
            tcl.MasterLaser = "master";
            tcl.Ramp = "rampfb";
            tcl.AnalogSampleRate = 100000;
            tcl.TCPChannel = 1190;
            tcl.DefaultScanPoints = 4000;
            tcl.SlaveVoltageUpperLimit = 10;
            tcl.SlaveVoltageLowerLimit = -10;
            Info.Add("Microcavitytcl", tcl);
            Info.Add("DefaultCavity", tcl);
         			
			// map the digital channels
            //AddDigitalOutputChannel("valve", pgBoard, 0, 6);

            //add Serial controllers
            //bathCryostat = new BathCryostat("ASRL1::INSTR");
            Info.Add("BathCryostat", "ASRL1::INSTR");

			// map the analog channels
            AddAnalogInputChannel("ECDLError", daqBoard + "/ai4", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("TiSapphError", daqBoard + "/ai5", AITerminalConfiguration.Differential);
			AddAnalogInputChannel("uCavityReflectionECDL", daqBoard + "/ai6", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("uCavityReflectionTiSapph", daqBoard + "/ai7", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("uCavityVoltage", daqBoard + "/ai3", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("TiSapphMonitor", daqBoard + "/ai1", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("QuartzRefCavity", daqBoard + "/ai0", AITerminalConfiguration.Differential);

            AddAnalogInputChannel("master", TCLBoard + "/ai1", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("tclpdTiSapph", TCLBoard + "/ai2", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("cavityRampMonitor", TCLBoard + "/ai7", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("tclpdECDL", TCLBoard + "/ai3", AITerminalConfiguration.Differential);

            //map the analogue output channels
            AddAnalogOutputChannel("TiSappControl", daqBoard + "/ao2");
            AddAnalogOutputChannel("uCavityControl", daqBoard + "/ao3");
            AddAnalogOutputChannel("ECDLControl", daqBoard + "/ao1");

            AddAnalogOutputChannel("tclTiSapphControl", TCLBoard + "/ao0",-10,10);
            AddAnalogOutputChannel("tclECDLControl", TCLBoard + "/ao1",0,10);
            AddAnalogOutputChannel("rampfb", TCLBoard + "/ao2");

            // map the counter channels
            AddCounterChannel("uCavityReflectionAPD", daqBoard + "/ctr0");
            AddCounterChannel("sample clock", daqBoard + "/ctr1");
            AddCounterChannel("shot gate", daqBoard + "/ctr3");

            // the analog triggers that is channel that wait for a trigger before acquisition
            Info.Add("analogTrigger0", (string)Boards["daq"] + "/PFI1");// pin PFI.1
            Info.Add("analogTrigger1", (string)Boards["daq"] + "/PFI2");// pin PFI.2

            //sample clock reader to syncronise analog and digital channels
            Info.Add("sample clock reader", daqBoard + "/PFI9");

            // the pause trigger for the counter channel
            Info.Add("shotTrigger0", daqBoard + "/PFI10");

            //pattern board trigger
            Info.Add("PGTrigger", (string)Boards["daq"] + "/PFI4");
            //AddDigitalInputChannel("PGTrigger", daqBoard, 0, 3);
            

            //counter triggers
            //Info.Add("counterSampleClockTrigger1", (string)Boards["daq"] + "/PFI3");
            //Info.Add("counterSampleClockTrigger2", (string)Boards["daq"] + "/PFI6");

            //this is a trigger from the same board to start acquisition
            AddDigitalOutputChannel("AcqTriggerOut", daqBoard, 0, 1);// trigger output

            //external triggers
            AddDigitalOutputChannel("sampleAndHoldTriggerOut", daqBoard, 1, 4);

            // add things to the info
            //Info.Add("PGClockLine", (string)Boards["pg"] + "/do/SampleClock");
            Info.Add("PGClockLine", ""); //this set to null because DAQMxPatternGenerator is written only for
            //separate pg Boards. This works around that.
            Info.Add("PatternGeneratorBoard",pgBoard);
            Info.Add("PGType", "integrated");
            Info.Add("PGClockCounter", "/Ctr1");
   		}

        
       public override void ConnectApplications()
        {
            //Commented out for debugging to get ScanMaster to compile
            //RemotingHelper.ConnectMicrocavityHardwareControl();

            RemotingHelper.ConnectScanMaster();

            // ask the remoting system for access to TCL2012
            Type t = Type.GetType("TransferCavityLock2012.Controller, TransferCavityLock");
            RemotingConfiguration.RegisterWellKnownClientType(t, "tcp://localhost:1190/controller.rem");

            // ask the remoting system for access to ScanMaster
            //Type p = Type.GetType("ScanMaster.Controller, ScanMaster");
            //RemotingConfiguration.RegisterWellKnownClientType(p, "tcp://localhost:1170/controller.rem");

        }
    }
}
