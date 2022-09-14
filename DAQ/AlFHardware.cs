using System;
using System.Collections;

using NationalInstruments.DAQmx;
using DAQ.WavemeterLock;
using DAQ.Pattern;
using DAQ.TransferCavityLock2012;
using DAQ.Remoting;
using System.Runtime.Remoting;
using System.Collections.Generic;

namespace DAQ.HAL
{
    public class AlFHardware : DAQ.HAL.Hardware
    {
        public AlFHardware()
        {
            // add the boards
            string PatternBoardName = "pg";
            string PatternBoardAddress = "/PXI1Slot4";
            Boards.Add(PatternBoardName, PatternBoardAddress);
            Info.Add("PatternGeneratorBoard", PatternBoardAddress);
            Info.Add("PGType", "integrated");
            Info.Add("PGClockCounter", "/ctr0");
            Info.Add("PGClockLine", PatternBoardAddress + "/PFI4");


            // Test analog signals
            AddAnalogInputChannel("testIn", PatternBoardAddress + "/ai6", AITerminalConfiguration.Rse);
            AddAnalogOutputChannel("testOut", PatternBoardAddress + "/ao0", -10, 10);
            AddAnalogOutputChannel("WMLOut", PatternBoardAddress + "/ao1", -10, 10);

            WavemeterLockConfig wmlConfig = new WavemeterLockConfig("WMLServer");


            wmlConfig.AddSlaveLaser("VECSEL", "WMLOut", 3);//name, analog, wavemeter channel

            Info.Add("WMLServer", wmlConfig);

            // map the digital channels of the "pg" card
            AddDigitalOutputChannel("q", PatternBoardAddress, 0, 1);//Pin 
            AddDigitalOutputChannel("flash", PatternBoardAddress, 0, 2);
            AddDigitalOutputChannel("valve", PatternBoardAddress, 0, 3);
            AddDigitalOutputChannel("detector", PatternBoardAddress, 0, 4);
            AddDigitalOutputChannel("ttlSwitch", PatternBoardAddress, 0, 5);
            AddDigitalOutputChannel("detectorprime", PatternBoardAddress, 0, 6);

        }

        public override void ConnectApplications()
        {
            // ask the remoting system for access to TCL2012
            // Type t = Type.GetType("TransferCavityLock2012.Controller, TransferCavityLock");
            // System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownClientType(t, "tcp://localhost:1190/controller.rem");
        }
    }
}
