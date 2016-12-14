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
			string daqBoard = (string)Boards["daq"];
            string pgBoard = (string)Boards["pg"];
                                 			
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
            AddAnalogOutputChannel("TiSappControl", daqBoard + "/ao2");
            AddAnalogOutputChannel("uCavityControl", daqBoard + "/ao3");
            AddAnalogOutputChannel("ECDLControl", daqBoard + "/ao1");

            // map the counter channels
            AddCounterChannel("uCavityReflectionAPD", daqBoard + "/ctr0");
            AddCounterChannel("sample clock", daqBoard + "/ctr1");

            // the analog triggers that is channel that wait for a trigger before acquisition
            Info.Add("analogTrigger0", (string)Boards["daq"] + "/PFI1");// pin PFI.1
            Info.Add("analogTrigger1", (string)Boards["daq"] + "/PFI2");// pin PFI.2

            //pattern board trigger
            Info.Add("PGTrigger", (string)Boards["daq"] + "/PFI5");

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
          
        // ask the remoting system for access to TCL2012
       //     Type t = Type.GetType("TransferCavityLock2012.Controller, TransferCavityLock");
          //  RemotingConfiguration.RegisterWellKnownClientType(t, "tcp://localhost:1190/controller.rem");
            
        }
    }
}
