using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;

namespace DAQ.HAL
{
	/// <summary>
	/// 
	/// </summary>
	public class BufferGasHardware : DAQ.HAL.Hardware
	{
		public BufferGasHardware()
		{
//			// add the boards
			Boards.Add("daq", "/dev1");
			Boards.Add("pg", "/dev2");

            // add things to the info
            // the analog triggers
            Info.Add("analogTrigger0", (string)Boards["daq"] + "/PFI0");
            Info.Add("analogTrigger1", (string)Boards["daq"] + "/PFI1");

			// map the digital channels
			string pgBoard = (string)Boards["pg"];
			AddDigitalOutputChannel("valve", pgBoard, 0, 6);
			AddDigitalOutputChannel("flash", pgBoard, 0, 2);
			AddDigitalOutputChannel("q", pgBoard, 0,0 );
			AddDigitalOutputChannel("detector", pgBoard, 1, 0); //Pin 16
			AddDigitalOutputChannel("aom", pgBoard, 2, 0);

			// map the analog channels
			string daqBoard = (string)Boards["daq"];
			AddAnalogInputChannel("pmt", daqBoard + "/ai0", AITerminalConfiguration.Rse);//Pin 68
			AddAnalogInputChannel("photodiode", daqBoard + "/ai1", AITerminalConfiguration.Rse);//Pin 33
			AddAnalogInputChannel("bogus", daqBoard + "/ai2", AITerminalConfiguration.Rse);//Pin 65
			AddAnalogOutputChannel("laser", daqBoard + "/ao0");//Pin 22

			//map the counter channels
			AddCounterChannel("pmt", daqBoard + "/ctr0");
			AddCounterChannel("sample clock", daqBoard + "/ctr1");
		}
			
	}
}
