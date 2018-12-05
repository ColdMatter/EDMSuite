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
    public class RbCaFHardware : DAQ.HAL.Hardware
    {

        public RbCaFHardware()
        {
            // add the boards
            Boards.Add("daq", "/PXI2SLOT4");
            string daqBoard = (string)Boards["daq"];
            // add things to the info
            //Info.Add("PGClockLine", Boards["pg"] + "/PFI2");
            //Info.Add("PatternGeneratorBoard", pgBoard);
            Info.Add("PGType", "integrated");
            Info.Add("PatternGeneratorBoard", "/PXI2SLOT4");
            Info.Add("PGClockCounter", "/ctr0");
            Info.Add("AOPatternTrigger", daqBoard + "/PFI0");
            Info.Add("Element", "");
            // the analog triggers
            //Info.Add("analogTrigger0", (string)Boards["daq"] + "/PFI0");

            // map the digital channels
            // these channels are to be part of the "PatternList" and shoud all be on the low half of the board
            AddDigitalOutputChannel("DOTest1", daqBoard, 0, 0); //Pin 10
            AddDigitalOutputChannel("AnalogPatternTrigger", daqBoard, 0, 1);
            // map the analog input channels
            //AddAnalogInputChannel("pmt", daqBoard + "/ai0", AITerminalConfiguration.Rse); 

            //map the analog output channels
            AddAnalogOutputChannel("AOTest1", daqBoard + "/ao0");
            AddAnalogOutputChannel("AOTest2", daqBoard + "/ao1"); 

            // map the counter channels
            //AddCounterChannel("pmt", daqBoard + "/ctr0"); 
            MMConfig mmConfig = new MMConfig(true, false, true, false);
            mmConfig.ExternalFilePattern = "*.tif";
            Info.Add("MotMasterConfiguration", mmConfig);

        }

        public override void ConnectApplications()
        {
            // RemotingHelper.ConnectDecelerationHardwareControl();
        }


    }
}