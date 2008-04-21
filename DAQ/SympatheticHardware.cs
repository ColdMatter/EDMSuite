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
            Boards.Add("usbDAQ1", "/dev3");
            string pgBoard = (string)Boards["pg"];
            string daqBoard = (string)Boards["daq"];
            string usbDAQ1 = (string)Boards["usbDAQ1"];

            // add things to the info

            // the analog triggers
            Info.Add("analogTrigger0", (string)Boards["daq"] + "/PFI0"); //DAQ Pin 11
            Info.Add("analogTrigger1", (string)Boards["daq"] + "/PFI1"); //DAQ Pin 10
            //distance information
            Info.Add("sourceToDetect", 0.787);
            Info.Add("sourceToSoftwareDecelerator", 0.123);
            //information about the molecule
            Info.Add("moleculeMass", 8.024); //this is 7LiH in amu
            Info.Add("moleculeRotationalConstant", 2.22545E11); //in Hz
            Info.Add("moleculeDipoleMoment", 29600.0); //in Hz/(V/m)
            //information about the decelerator
            Info.Add("deceleratorStructure", DecelerationConfig.DecelerationExperiment.SwitchStructure.H_V); //Horizontal first
            Info.Add("deceleratorLensSpacing", 0.006);
            Info.Add("deceleratorFieldMap", "RodLayout3_EonAxis.dat");
            Info.Add("mapPoints", 121);
            Info.Add("mapStartPoint", 0.0);
            Info.Add("mapResolution", 0.0001);

            // map the GPIB instruments
            GPIBInstruments.Add("microwave", new EIP578Synth("GPIB0::19::INSTR"));
            GPIBInstruments.Add("agilent", new Agilent33250Synth("GPIB0::10::INSTR"));
            
            // map the digital channels
            // these channels are to be part of the "pattern" and shoud all be on the low half of the board
			AddDigitalOutputChannel("valve", pgBoard, 0, 0); //Pin 10
			AddDigitalOutputChannel("flash", pgBoard, 0, 1); //Pin 44
			AddDigitalOutputChannel("q", pgBoard, 0,2 ); //Pin 45
			AddDigitalOutputChannel("detector", pgBoard, 0,7); //Pin 15
            AddDigitalOutputChannel("detectorprime", pgBoard, 1, 7); //Pin 22
		//	AddDigitalOutputChannel("fig", pgBoard, 3, 1); //Pin 63
			AddDigitalOutputChannel("aom", pgBoard, 0, 4); //Pin 13
            AddDigitalOutputChannel("flash2", pgBoard, 0, 5); //Pin 47
            AddDigitalOutputChannel("q2", pgBoard, 0, 6); //Pin 48
            // the following are the decelerator channels for the burst
			AddDigitalOutputChannel("decelhplus", pgBoard, 1, 0); //Pin 16
	        AddDigitalOutputChannel("decelhminus", pgBoard, 1, 1); //Pin 17
			AddDigitalOutputChannel("decelvplus", pgBoard, 1, 2); //Pin 51
			AddDigitalOutputChannel("decelvminus", pgBoard, 1, 3); //Pin 52

            // these channels are to be switched "manually" and should all be on the high half of the board
            // the following set of switches are used to enable or disable the burst
            AddDigitalOutputChannel("hplusBurstEnable", pgBoard, 2, 0);
            AddDigitalOutputChannel("hminusBurstEnable", pgBoard, 3, 4);
            AddDigitalOutputChannel("vplusBurstEnable", pgBoard, 2, 2);
            AddDigitalOutputChannel("vminusBurstEnable", pgBoard, 2, 3);
            // the following set of switches are used for dc on or off
            AddDigitalOutputChannel("hplusdc", pgBoard, 2, 4);
            AddDigitalOutputChannel("hminusdc", pgBoard, 2, 5);
            AddDigitalOutputChannel("vplusdc", pgBoard, 2, 6);
            AddDigitalOutputChannel("vminusdc", pgBoard, 2, 7);


			// map the analog input channels
			AddAnalogInputChannel("pmt", daqBoard + "/ai0", AITerminalConfiguration.Rse); //Pin 68
            AddAnalogInputChannel("lockcavity", daqBoard + "/ai1", AITerminalConfiguration.Rse); //Pin 33
            AddAnalogInputChannel("refcavity", daqBoard + "/ai2", AITerminalConfiguration.Rse); //Pin 65
            AddAnalogInputChannel("fig", daqBoard + "/ai5", AITerminalConfiguration.Rse); //Pin 60
            AddAnalogInputChannel("atomSourcePressure1", usbDAQ1 + "/ai0", AITerminalConfiguration.Differential); //ai0+ is pin 2, ai0- is pin 3
            AddAnalogInputChannel("atomSourcePressure2", usbDAQ1 + "/ai1", AITerminalConfiguration.Differential); //ai1+ is pin 5, ai1- is pin 6

            //map the analog output channels
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