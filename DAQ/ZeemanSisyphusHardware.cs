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

            string usbDaqName = "usbDaq";
            string usbDaqAddress = "/Dev1";
            Boards.Add(usbDaqName, usbDaqAddress); // NI USB-6009
           

            // map the digital channels
            string pgBoard = (string)Boards["pg"];

            AddDigitalOutputChannel("q", digitalPatternBoardAddress, 2, 7);
            AddDigitalOutputChannel("flash", digitalPatternBoardAddress, 2, 6);
            AddDigitalOutputChannel("analogPatternTrigger", digitalPatternBoardAddress, 2, 4);//connect to daq board PFI 0
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
            AddAnalogInputChannel("pmt", analogPatternBoardAddress + "/ai6", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("pmt1", analogPatternBoardAddress + "/ai5", AITerminalConfiguration.Rse);

            AddAnalogOutputChannel("v0AOMFrequency", analogPatternBoardAddress + "/ao0");

            // map the USB channels
            AddDigitalOutputChannel("sourceHeater", usbDaqAddress, 0, 0);
            AddDigitalOutputChannel("cryoCooler", usbDaqAddress, 0, 1);

            AddAnalogInputChannel("thermVref", usbDaqAddress + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("SF6thermistor", usbDaqAddress + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("40Kthermistor", usbDaqAddress + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("4Kthermistor", usbDaqAddress + "/ai3", AITerminalConfiguration.Rse);
            // AddAnalogInputChannel("40Kthermistor", usbDaqAddress + "/ai3", AITerminalConfiguration.Rse); // Dummy channel so we don't have to remove refs to 40K therm from front-end.
            AddAnalogInputChannel("sourcePressureFar", usbDaqAddress + "/ai4", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("sourcePressureNear", usbDaqAddress + "/ai5", AITerminalConfiguration.Rse);
            

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
