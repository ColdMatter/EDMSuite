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

			// map the digital channels
			string pgBoard = (string)Boards["pg"];
			AddDigitalOutputChannel("valve", pgBoard, 0, 6);
			AddDigitalOutputChannel("flash", pgBoard, 0, 1);
			AddDigitalOutputChannel("q", pgBoard, 0,0 );
			AddDigitalOutputChannel("detector", pgBoard, 1, 0);
			AddDigitalOutputChannel("aom", pgBoard, 2, 0);

			// map the analog channels
			string daqBoard = (string)Boards["daq"];
			AddAnalogInputChannel("pmt", daqBoard + "/ai0", AITerminalConfiguration.Rse);
			AddAnalogInputChannel("photodiode", daqBoard + "/ai1", AITerminalConfiguration.Rse);
			AddAnalogInputChannel("bogus", daqBoard + "/ai2", AITerminalConfiguration.Rse);
			AddAnalogOutputChannel("laser", daqBoard + "/ao0");

			//map the counter channels
			AddCounterChannel("pmt", daqBoard + "/ctr0");
			AddCounterChannel("sample clock", daqBoard + "/ctr1");
		}
			
	}
}
