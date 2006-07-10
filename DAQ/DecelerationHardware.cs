using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;
using DAQ.Remoting;

namespace DAQ.HAL
{
	
	/// <summary>
	/// This is the specific hardware that the deceleration experiment has. This class conforms
	/// to the Hardware interface.
	/// </summary>
	public class DecelerationHardware : DAQ.HAL.Hardware
	{

		public DecelerationHardware()
		{

			// YAG laser
			yag = new MiniliteLaser();

			// add the boards
			Boards.Add("daq", "/dev1");
			Boards.Add("pg", "/dev2");
			
			// map the digital channels
			string pgBoard = (string)Boards["pg"];
			AddDigitalOutputChannel("valve", pgBoard, 0, 6);
			AddDigitalOutputChannel("flash", pgBoard, 0, 5);
			AddDigitalOutputChannel("q", pgBoard, 0,2 );
			AddDigitalOutputChannel("detector", pgBoard, 3, 7);
			AddDigitalOutputChannel("detectorprime", pgBoard, 3, 6);
			AddDigitalOutputChannel("aom", pgBoard, 0, 4);
			AddDigitalOutputChannel("decelhplus", pgBoard, 1, 0);
			AddDigitalOutputChannel("decelhminus", pgBoard, 1, 1);
			AddDigitalOutputChannel("decelvplus", pgBoard, 1, 2);
			AddDigitalOutputChannel("decelvminus", pgBoard, 1, 3);

			// map the analog channels
			string daqBoard = (string)Boards["daq"];
			AddAnalogInputChannel("pmt", daqBoard + "/ai0", AITerminalConfiguration.Rse);
			AddAnalogInputChannel("longcavity", daqBoard + "/ai3", AITerminalConfiguration.Rse);
			AddAnalogInputChannel("lockcavity", daqBoard + "/ai1", AITerminalConfiguration.Rse);
			AddAnalogOutputChannel("laser", daqBoard + "/ao0");
			AddAnalogOutputChannel("highvoltage", daqBoard + "/ao1");

            // map the counter channels
            AddCounterChannel("pmt", daqBoard + "/ctr0");
            AddCounterChannel("sample clock", daqBoard + "/ctr1");

		}

        public override void ConnectApplications()
        {
            RemotingHelper.ConnectDecelerationHardwareControl();
        }

	}
}
