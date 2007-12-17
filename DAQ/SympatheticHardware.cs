using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;
using DAQ.Remoting;

namespace DAQ.HAL
{
	
	/// <summary>
	/// This is the specific hardware that the sympathetic cooling experiment has. This class conforms
	/// to the Hardware interface.
	/// </summary>
	public class SympatheticHardware : DAQ.HAL.Hardware
	{

		public SympatheticHardware()
		{

			// YAG laser
			yag = new QuantaRayLaser();

			// add the boards
			Boards.Add("daq", "/dev2");
			Boards.Add("pg", "/dev1");

            // add things to the info
            // the analog triggers
            Info.Add("analogTrigger0", (string)Boards["daq"] + "/PFI0"); //DAQ Pin 11
            Info.Add("analogTrigger1", (string)Boards["daq"] + "/PFI1"); //DAQ Pin 10
            
            // map the GPIB instruments
            GPIBInstruments.Add("microwave", new EIP578Synth("GPIB0::19::INSTR"));
            GPIBInstruments.Add("agilent", new Agilent33250Synth("GPIB0::10::INSTR"));
            
            // map the digital channels
			string pgBoard = (string)Boards["pg"];
			AddDigitalOutputChannel("valve", pgBoard, 0, 0); //Pin 10
			AddDigitalOutputChannel("flash", pgBoard, 0, 1); //Pin 44
			AddDigitalOutputChannel("q", pgBoard, 0,2 ); //Pin 45
			AddDigitalOutputChannel("detector", pgBoard, 3, 0); //Pin 29
            AddDigitalOutputChannel("detectorprime", pgBoard, 3, 6); //Pin 67
			AddDigitalOutputChannel("fig", pgBoard, 3, 1); //Pin 63
			AddDigitalOutputChannel("aom", pgBoard, 0, 4); //Pin 13
            AddDigitalOutputChannel("flash2", pgBoard, 0, 5); //Pin 47
            AddDigitalOutputChannel("q2", pgBoard, 0, 6); //Pin 48
			AddDigitalOutputChannel("decelhplus", pgBoard, 1, 0); //Pin 16
	        AddDigitalOutputChannel("decelhminus", pgBoard, 1, 1); //Pin 17
			AddDigitalOutputChannel("decelvplus", pgBoard, 1, 2); //Pin 51
			AddDigitalOutputChannel("decelvminus", pgBoard, 1, 3); //Pin 52

			// map the analog channels
			string daqBoard = (string)Boards["daq"];
			AddAnalogInputChannel("pmt", daqBoard + "/ai0", AITerminalConfiguration.Rse); //Pin 68
            AddAnalogInputChannel("lockcavity", daqBoard + "/ai1", AITerminalConfiguration.Rse); //Pin 33
            AddAnalogInputChannel("refcavity", daqBoard + "/ai2", AITerminalConfiguration.Rse); //Pin 65
            AddAnalogInputChannel("fig", daqBoard + "/ai5", AITerminalConfiguration.Rse); //Pin 60
			AddAnalogOutputChannel("laser", daqBoard + "/ao0"); // Pin 22
            AddAnalogOutputChannel("dyelaser", daqBoard + "/ao1"); // Pin 21
            AddAnalogOutputChannel("highvoltage", daqBoard + "/ao1"); // Note - this is just here because a channel called "highvoltage" has been hard-wired into DecelerationHardwareControl - this needs to be rectified

            // map the counter channels
            AddCounterChannel("pmt", daqBoard + "/ctr0"); //Source is pin 37, gate is pin 3, out is pin 2
            AddCounterChannel("sample clock", daqBoard + "/ctr1"); //Source is pin 42, gate is pin 41, out is pin 40

		}

        public override void ConnectApplications()
        {
           // RemotingHelper.ConnectDecelerationHardwareControl();
        }


	}
}