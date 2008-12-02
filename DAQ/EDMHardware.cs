using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;

namespace DAQ.HAL
{
	/// <summary>
	/// This is the specific hardware that the edm machine has. This class conforms
	/// to the Hardware interface.
	/// </summary>
	public class EDMHardware : DAQ.HAL.Hardware
	{

		public EDMHardware()
		{

			// add the boards
			Boards.Add("daq", "/dev1");
			Boards.Add("pg", "/dev2");
			Boards.Add("counter", "/dev3");
			Boards.Add("usbDAQ1", "/dev4");
            Boards.Add("analogIn", "/dev5");
            Boards.Add("usbDAQ2", "/dev6");
            Boards.Add("usbDAQ3", "/dev7");
            Boards.Add("usbDAQ4", "/dev9");
			string pgBoard = (string)Boards["pg"];
			string daqBoard = (string)Boards["daq"];
			string counterBoard = (string)Boards["counter"];
			string usbDAQ1 = (string)Boards["usbDAQ1"];
            string analogIn = (string)Boards["analogIn"];
            string usbDAQ2 = (string)Boards["usbDAQ2"];
            string usbDAQ3 = (string)Boards["usbDAQ3"];
            string usbDAQ4 = (string)Boards["usbDAQ4"];

            // add things to the info
            // the analog triggers
            Info.Add("analogTrigger0", (string)Boards["analogIn"] + "/PFI0");
            Info.Add("analogTrigger1", (string)Boards["analogIn"] + "/PFI1");
            Info.Add("sourceToDetect", 1.3);
            Info.Add("moleculeMass", 193.0);
            Info.Add("phaseLockControlMethod", "synth");

			// YAG laser
			yag = new BrilliantLaser("ASRL1::INSTR");

			// add the GPIB instruments
			GPIBInstruments.Add("green", new HP8657ASynth("GPIB0::7::INSTR"));
			GPIBInstruments.Add("red", new HP3325BSynth("GPIB0::12::INSTR"));
			GPIBInstruments.Add("4861", new ICS4861A("GPIB0::4::INSTR"));
			GPIBInstruments.Add("bCurrentMeter", new HP34401A("GPIB0::22::INSTR"));
            GPIBInstruments.Add("rfCounter", new Agilent53131A("GPIB0::3::INSTR"));
            GPIBInstruments.Add("rfPower", new HP438A("GPIB0::13::INSTR"));

			// map the digital channels
			// these channels are generally switched by the pattern generator
			// they're all in the lower half of the pg
			AddDigitalOutputChannel("valve", pgBoard, 0, 0);
			AddDigitalOutputChannel("flash", pgBoard, 0, 1);
			AddDigitalOutputChannel("q", pgBoard, 0, 2);
			AddDigitalOutputChannel("detector", pgBoard, 0, 3);
			AddDigitalOutputChannel("detectorprime", pgBoard, 1, 2); // this trigger is for switch scanning
																	// see ModulatedAnalogShotGatherer.cs
																	// for details.
			AddDigitalOutputChannel("rfSwitch", pgBoard, 0, 4);
			AddDigitalOutputChannel("fmSelect", pgBoard, 1, 0);      // This line selects which fm voltage is
                                                                    // sent to the synth.
            AddDigitalOutputChannel("attenuatorSelect", pgBoard, 0, 5);    // This line selects the attenuator voltage
                                                                    // sent to the voltage-controlled attenuator.
			AddDigitalOutputChannel("piFlip", pgBoard, 1, 1);
			AddDigitalOutputChannel("ttlSwitch", pgBoard, 1, 3);	// This is the output that the pg
																	// will switch if it's switch scanning.
            AddDigitalOutputChannel("scramblerEnable", pgBoard, 1, 4);

			// these channel are usually software switched - they should not be in
			// the lower half of the pattern generator
			AddDigitalOutputChannel("b", pgBoard, 2, 0);
			AddDigitalOutputChannel("notB", pgBoard, 2, 1);
			AddDigitalOutputChannel("db", pgBoard, 2, 2);
			AddDigitalOutputChannel("notDB", pgBoard, 2, 3);
//			AddDigitalOutputChannel("notEOnOff", pgBoard, 2, 4);  // this line seems to be broken on our pg board
// 			AddDigitalOutputChannel("eOnOff", pgBoard, 2, 5);  // this and the above are not used now we have analog E control
            AddDigitalOutputChannel("targetStepper", pgBoard, 2, 5);
            AddDigitalOutputChannel("ePol", pgBoard, 2, 6);
			AddDigitalOutputChannel("notEPol", pgBoard, 2, 7);
			AddDigitalOutputChannel("eBleed", pgBoard, 3, 0);
			AddDigitalOutputChannel("piFlipEnable", pgBoard, 3, 1);
			AddDigitalOutputChannel("notPIFlipEnable", pgBoard, 3, 5);
            AddDigitalOutputChannel("pumpShutter", pgBoard, 3, 3);
            AddDigitalOutputChannel("probeShutter", pgBoard, 3, 4);
            AddDigitalOutputChannel("argonShutter", pgBoard, 3, 6);
 
            // map the analog channels
            
            // These channels are on the daq board. Used mainly for diagnostic purposes.
			// On no account should they switch during the edm acquisition pattern.
            AddAnalogInputChannel("iodine", daqBoard + "/ai2", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("cavity", daqBoard + "/ai3", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("probePD", daqBoard + "/ai4", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("pumpPD", daqBoard + "/ai5", AITerminalConfiguration.Nrse);
            // Used ai10,11 & 12 over 6,7 & 8 for miniFluxgates, because ai8, 9 have an isolated ground. 
            AddAnalogInputChannel("miniFlux1", daqBoard + "/ai10", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("miniFlux2", daqBoard + "/ai11", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("miniFlux3", daqBoard + "/ai12", AITerminalConfiguration.Nrse);


            // high quality analog inputs (will be) on the S-series analog in board
            AddAnalogInputChannel("pmt", analogIn + "/ai0", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("norm", analogIn + "/ai1", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("magnetometer", analogIn + "/ai2", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("gnd", analogIn + "/ai3", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("battery", analogIn + "/ai4", AITerminalConfiguration.Differential);
            
          
            AddAnalogOutputChannel("phaseScramblerVoltage", daqBoard + "/ao0");
			AddAnalogOutputChannel("b", daqBoard + "/ao1");

            // rf rack control
            //AddAnalogInputChannel("rfPower", usbDAQ1 + "/ai0", AITerminalConfiguration.Rse);

            AddAnalogOutputChannel("rf1Attenuator", usbDAQ1 + "/ao0", 0, 5);
            AddAnalogOutputChannel("rf2Attenuator", usbDAQ1 + "/ao1", 0, 5);
            AddAnalogOutputChannel("rf1FM", usbDAQ2 + "/ao0", 0, 5);
            AddAnalogOutputChannel("rf2FM", usbDAQ2 + "/ao1", 0, 5);

            // E field control and monitoring
            AddAnalogInputChannel("cPlusMonitor", usbDAQ3 + "/ai1", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("cMinusMonitor", usbDAQ3 + "/ai2", AITerminalConfiguration.Differential);

            AddAnalogOutputChannel("cPlus", usbDAQ3 + "/ao0");
            AddAnalogOutputChannel("cMinus", usbDAQ3 + "/ao1");

            // B field control
            AddAnalogOutputChannel("steppingBBias", usbDAQ4 + "/ao0", 0, 5);

            // FL control
            AddAnalogOutputChannel("flPZT", usbDAQ4 + "/ao1", 0, 5);

			// map the counter channels
			AddCounterChannel("phaseLockOscillator", counterBoard + "/ctr7");
			AddCounterChannel("phaseLockReference", counterBoard + "/pfi10");
			AddCounterChannel("northLeakage", counterBoard +"/ctr0");
			AddCounterChannel("southLeakage", counterBoard +"/ctr1");

		}

	}
}
