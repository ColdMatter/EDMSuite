using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;
using DAQ.TransferCavityLock2012;
using DAQ.Remoting;
using System.Runtime.Remoting;
using System.Collections.Generic;
using DAQ.WavemeterLock;

namespace DAQ.HAL
{
    public class WMLServerHardware : DAQ.HAL.Hardware
    {
       
        public WMLServerHardware()
        {
            Boards.Add("WMLBoard", "/Dev1");
            string WMLBoard = (string)Boards["WMLBoard"];

            AddAnalogOutputChannel("WavemeterLock1", WMLBoard + "/ao0", 0, 5);
            AddAnalogOutputChannel("WavemeterLock2", WMLBoard + "/ao1", 0, 5);
            
            //Configuration for wavemeterlock
            WavemeterLockConfig wmlConfig = new WavemeterLockConfig("Default");
            //wmlConfig.AddSlaveLaser("v0LaserTest", "WavemeterLock1",6);//Laser name, analog channel, wavemeter channel
            wmlConfig.AddSlaveLaser("WavemeterTest", "WavemeterLock2", 4);
            Info.Add("Default", wmlConfig);

        }

        public override void ConnectApplications()
        {
            // ask the remoting system for access to TCL2012
            // Type t = Type.GetType("TransferCavityLock2012.Controller, TransferCavityLock");
            // System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownClientType(t, "tcp://localhost:1190/controller.rem");
        }
    }
}
