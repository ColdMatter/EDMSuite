using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;

namespace DAQ.HAL
{
	
	/// <summary>
	/// This is the specific hardware that the LiH experiment has. This class conforms
	/// to the Hardware interface.
	/// </summary>
	public class LiHHardware : DAQ.HAL.Hardware
	{

		public LiHHardware()
		{

			// YAG laser
			yag = new QuantaRayLaser();

			// add the boards
			Boards.Add("daq", "/dev2");
			Boards.Add("pg", "/dev1");
			
			// map the digital channels
			string pgBoard = (string)Boards["pg"];
			AddDigitalOutputChannel("valve", pgBoard, 0, 0); //Pin 10
			AddDigitalOutputChannel("flash", pgBoard, 0, 1); //Pin 44
			AddDigitalOutputChannel("q", pgBoard, 0,2 ); //Pin 45
			AddDigitalOutputChannel("detector", pgBoard, 3, 0); //Pin 29
			AddDigitalOutputChannel("fig", pgBoard, 3, 1); //Pin 63
			AddDigitalOutputChannel("aom", pgBoard, 0, 4);
            AddDigitalOutputChannel("flash2", pgBoard, 0, 5);
            AddDigitalOutputChannel("q2", pgBoard, 0, 6);
	//		AddDigitalOutputChannel("decelhplus", pgBoard, 1, 0);
	//		AddDigitalOutputChannel("decelhminus", pgBoard, 1, 1);
	//		AddDigitalOutputChannel("decelvplus", pgBoard, 1, 2);
	//		AddDigitalOutputChannel("decelvminus", pgBoard, 1, 3);

			// map the analog channels
			string daqBoard = (string)Boards["daq"];
			AddAnalogInputChannel("pmt", daqBoard + "/ai0", AITerminalConfiguration.Rse); //Pin 68
			AddAnalogInputChannel("photodiode", daqBoard + "/ai1", AITerminalConfiguration.Rse); //Pin 33
			AddAnalogInputChannel("cavity", daqBoard + "/ai2", AITerminalConfiguration.Rse); //Pin 65
			AddAnalogOutputChannel("laser", daqBoard + "/ao0"); // Pin 22
		//	AddAnalogOutputChannel("aom", daqBoard + "/ao1"); 

		}


	}
}