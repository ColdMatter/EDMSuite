using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;
using DAQ.TransferCavityLock2012;

namespace DAQ.HAL
{
    public class ZeemanSisyphusHardware : DAQ.HAL.Hardware
    {
        public ZeemanSisyphusHardware()
        {
            // add the boards
          

            string digitalPatternBoardName = "digitalPattern";//NI PXIe-6535
            string digitalPatternBoardAddress = "/PXI1SLOT2";
            Boards.Add(digitalPatternBoardName, digitalPatternBoardAddress);

            string analogPatternBoardName = "analogPattern";//NI PXIe-6229
            string analogPatternBoardAddress = "/PXI1Slot4";
            Boards.Add(analogPatternBoardName, analogPatternBoardAddress);
           

            // map the digital channels
            string pgBoard = (string)Boards["pg"];

            AddDigitalOutputChannel("q", digitalPatternBoardAddress, 2, 7);
            AddDigitalOutputChannel("flash", digitalPatternBoardAddress, 2, 6);
            AddDigitalOutputChannel("analogPatternTrigger", digitalPatternBoardAddress, 2, 4);//connect to daq board PFI 0
            AddDigitalOutputChannel("sourceHeater", digitalPatternBoardAddress, 2, 5);
            AddDigitalOutputChannel("cryoCooler", digitalPatternBoardAddress, 0, 5);
           // AddDigitalOutputChannel("unused", digitalPatternBoardAddress, 0, 4);
           // AddDigitalOutputChannel("valve", digitalPatternBoardAddress, 3, 3);
            //AddDigitalOutputChannel("unused", digitalPatternBoardAddress, 3, 2);
            //AddDigitalOutputChannel("unused", digitalPatternBoardAddress, 3, 1);
            //AddDigitalOutputChannel("unused", digitalPatternBoardAddress, 3, 0);
            //AddDigitalOutputChannel("unused", digitalPatternBoardAddress, 1, 0);
            //AddDigitalOutputChannel("unused", digitalPatternBoardAddress, 1, 1);
            //AddDigitalOutputChannel("unused", digitalPatternBoardAddress, 0, 6);
            //AddDigitalOutputChannel("unused", digitalPatternBoardAddress, 0, 7);


            //Add triggers for other boards
            //   Info.Add("PGClockLine", digitalPatternBoardAddress + "/PFI2");
            //  Info.Add("PGClockLine", digitalPatternBoardAddress + "/PFI3");

     

      

            // map the analog channels
            AddAnalogInputChannel("4KRTthermistor", analogPatternBoardAddress + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("4Kthermistor", analogPatternBoardAddress + "/ai4", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("thermVref", analogPatternBoardAddress + "/ai5", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("pmt", analogPatternBoardAddress + "/ai6", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("sourcePressure", analogPatternBoardAddress + "/ai7", AITerminalConfiguration.Rse);
            AddAnalogOutputChannel("v0AOMFrequency", analogPatternBoardAddress + "/ao0");
            

            // ScanMaster configuration
            Info.Add("PGType", "dedicated");
            Info.Add("PatternGeneratorBoard", digitalPatternBoardAddress);
            Info.Add("analogTrigger0", analogPatternBoardAddress + "/PFI0");

            Info.Add("defaultTOFRange", new double[] { 4000, 12000 }); // these entries are the two ends of the range for the upper TOF graph
            Info.Add("defaultTOF2Range", new double[] { 0, 1000 }); // these entries are the two ends of the range for the middle TOF graph
            Info.Add("defaultGate", new double[] { 6000, 2000 }); // the first entry is the centre of the gate, the second is the half width of the gate (upper TOF graph)


            //These need to be activated for the phase lock
          //  AddCounterChannel("phaseLockOscillator", daqBoard + "/ctr0"); //This should be the source pin of a counter PFI 8
          //  AddCounterChannel("phaseLockReference", daqBoard + "/PFI9"); //This should be the gate pin of the same counter - need to check it's name
        }

        public override void ConnectApplications()
        {
            // ask the remoting system for access to TCL2012
           // Type t = Type.GetType("TransferCavityLock2012.Controller, TransferCavityLock");
           // System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownClientType(t, "tcp://localhost:1190/controller.rem");
        }
    }
}
