using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;
using DAQ.TransferCavityLock2012;

namespace DAQ.HAL
{
    public class BufferClassicHardware : DAQ.HAL.Hardware
    {
        public BufferClassicHardware()
        {
            // add the boards
            //Boards.Add("daq", "/PXI1Slot2");
            //Boards.Add("pg", "/PXI1Slot3");
            Boards.Add("daq", "/DAQ");
            Boards.Add("pg", "/PG");
            Boards.Add("tcl", "/PXI1Slot4");
            Boards.Add("tclvis", "/VisCavity");
            Boards.Add("tclvis2", "/VisCavity2");
            string TCLBoard = (string)Boards["tcl"];
            string TCLBoard2 = (string)Boards["tclvis"];
            string TCLBoard3 = (string)Boards["tclvis2"];

            // map the digital channels
            string pgBoard = (string)Boards["pg"];

            AddDigitalOutputChannel("q", pgBoard, 0, 0);//Pin 10
            AddDigitalOutputChannel("aom", pgBoard, 1, 1);//
            AddDigitalOutputChannel("aom2", pgBoard, 1, 2);//
            AddDigitalOutputChannel("flash", pgBoard, 0, 2);//Pin 45
            //(0,3) pin 12 is unconnected
            AddDigitalOutputChannel("shutterTrig1", pgBoard, 1, 6);// Pin 21, triggers camera for on-shots (not wired up)
            AddDigitalOutputChannel("shutterTrig2", pgBoard, 1, 7);// Pin 22, triggers camera for off-shots (not wired up)
            AddDigitalOutputChannel("probe", pgBoard, 0, 1);//Pin 44 previously connected to aom (not wired up)

            AddDigitalOutputChannel("valve", pgBoard, 0, 6);//

            AddDigitalOutputChannel("detector", pgBoard, 1, 0); //Pin 16 (onShot)from pg to daq
            AddDigitalOutputChannel("detectorprime", pgBoard, 0, 7); //Pin 15 (OffShot)from pg to daq

            //digital output P 0.6 wired up, not used (Pin 48)
            // this is the digital output from the daq board that the TTlSwitchPlugin wil switch
            AddDigitalOutputChannel("digitalSwitchChannel", (string)Boards["daq"], 0, 0);//enable for camera

           

            // add things to the info
            // the analog triggers
            Info.Add("analogTrigger0", (string)Boards["daq"] + "/PFI0");
            Info.Add("analogTrigger1", (string)Boards["daq"] + "/PFI1");
            Info.Add("phaseLockControlMethod", "analog");
            Info.Add("PGClockLine", Boards["pg"] + "/PFI4");
            Info.Add("PatternGeneratorBoard", pgBoard);
            Info.Add("PGType", "dedicated");

            // external triggering control
            Info.Add("PGTrigger", pgBoard + "/PFI1"); //Mapped to PFI7 on 6533 connector

            // map the analog channels
            //string daqBoard = (string)Boards["daq"];
            //AddAnalogInputChannel("detector1", daqBoard + "/ai0", AITerminalConfiguration.Nrse);//Pin 68
            //AddAnalogInputChannel("detector2", daqBoard + "/ai3", AITerminalConfiguration.Nrse);//Pin 
            //AddAnalogInputChannel("detector3", daqBoard + "/ai8", AITerminalConfiguration.Nrse);//Pin 34
            //AddAnalogInputChannel("pressure1", daqBoard + "/ai1", AITerminalConfiguration.Nrse);//Pin 33 pressure reading at the moment
            //AddAnalogInputChannel("cavity", daqBoard + "/ai2", AITerminalConfiguration.Nrse);//Pin 65
            //AddAnalogInputChannel("cavitylong", daqBoard + "/ai4", AITerminalConfiguration.Nrse);//Pin 28
            //AddAnalogInputChannel("cavityshort", daqBoard + "/ai5", AITerminalConfiguration.Nrse);//Pin 60

            // map the analog channels
            string daqBoard = (string)Boards["daq"];
            AddAnalogInputChannel("detector1", daqBoard + "/ai4", AITerminalConfiguration.Rse);//Pin 68
            AddAnalogInputChannel("detector2", daqBoard + "/ai5", AITerminalConfiguration.Rse);//Pin 
            AddAnalogInputChannel("detector3", daqBoard + "/ai6", AITerminalConfiguration.Rse);//Pin 34

            AddAnalogInputChannel("cavitylong", daqBoard + "/ai7", AITerminalConfiguration.Rse);//Pin 28
            AddAnalogInputChannel("cavityshort", daqBoard + "/ai8", AITerminalConfiguration.Rse);//Pin 60

            AddAnalogInputChannel("Temp1", daqBoard + "/ai0", AITerminalConfiguration.Rse);//Pin 31
            AddAnalogInputChannel("Temp2", daqBoard + "/ai1", AITerminalConfiguration.Rse);//Pin 31
            AddAnalogInputChannel("TempRef", daqBoard + "/ai2", AITerminalConfiguration.Rse);//Pin 66
            AddAnalogInputChannel("pressure1", daqBoard + "/ai3", AITerminalConfiguration.Rse);//Pin 33 pressure reading at the moment
            
            
            AddAnalogOutputChannel("phaseLockAnalogOutput", daqBoard + "/ao1"); //pin 21

            //TransferCavityLock info
            TCLConfig tcl1 = new TCLConfig("IR Cavity");
            tcl1.AddLaser("pumplaser", "p1");
            tcl1.AddLaser("v21repump", "p2");
            tcl1.Trigger = TCLBoard + "/PFI0";
            tcl1.Cavity = "cavityRampMonitor";
            tcl1.MasterLaser = "master";
            tcl1.Ramp = "rampfb";
            tcl1.TCPChannel = 1190;
            tcl1.SlaveVoltageLowerLimit = 0.0;
            tcl1.SlaveVoltageUpperLimit = 10.0;
      //      tcl1.DefaultScanPoints = 600;
            Info.Add("IR", tcl1);


            //TransferCavityLock info
            TCLConfig tcl2 = new TCLConfig("VIS Cavity");
            tcl2.AddLaser("v1repump", "VISp1");
            tcl2.Trigger = TCLBoard2 + "/PFI0";
            tcl2.Cavity = "VIScavityRampMonitor";
            tcl2.MasterLaser = "VISmaster";
            tcl2.Ramp = "VISrampfb";
            tcl2.TCPChannel = 1191;
            tcl2.SlaveVoltageLowerLimit = -10.0;
            tcl2.SlaveVoltageUpperLimit = 10.0;
            //tcl2.AnalogSampleRate = 15000;
     //       tcl2.DefaultScanPoints = 300;
            Info.Add("VIS", tcl2);
            // The next line is try two DAQ cards for one cavity. (14 Nov 2016) Feel free to delete it.
            tcl2.AddLaser("v2repump", "VISp2");

            //TCL Lockable lasers
           // Info.Add("TCLLockableLasers", new string[] { "pumplaser"});
           // Info.Add("TCLPhotodiodes", new string[] { "cavityRampMonitor", "master", "p1"});// THE FIRST TWO MUST BE CAVITY AND MASTER PHOTODIODE!!!!
           // Info.Add("TCL_Slave_Voltage_Limit_Upper", 2.0); //volts: Laser control
           // Info.Add("TCL_Slave_Voltage_Limit_Lower", -2.0); //volts: Laser control
           // Info.Add("TCL_Default_Gain", -0.01);
           // Info.Add("TCL_Default_VoltageToLaser", 0.0);
           // Info.Add("TCL_MAX_INPUT_VOLTAGE", 10.0);
           // Info.Add("TCL_Default_ScanPoints", 1000);
            // Some matching up for TCL
           // Info.Add("pumplaser", "p1");
           // Info.Add("TCLTrigger", TCLBoard + "/PFI0");

            AddAnalogInputChannel("master", TCLBoard + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("p1", TCLBoard + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("p2", TCLBoard + "/ai16", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("cavityRampMonitor", TCLBoard + "/ai1", AITerminalConfiguration.Rse);

      //      AddAnalogInputChannel("VISmaster", TCLBoard2 + "/ai5", AITerminalConfiguration.Rse);
      //      AddAnalogInputChannel("VISp1", TCLBoard2 + "/ai2", AITerminalConfiguration.Rse);
      //      AddAnalogInputChannel("VIScavityRampMonitor", TCLBoard2 + "/ai4", AITerminalConfiguration.Rse);
      //      //The next line is try two DAQ cards for one cavity. (14 Nov 2016) Feel free to delete it.
      //      AddAnalogInputChannel("VISp2", TCLBoard2 + "/ai7", AITerminalConfiguration.Rse);

            AddAnalogInputChannel("VISmaster", TCLBoard2 + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("VISp1", TCLBoard2 + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("VIScavityRampMonitor", TCLBoard2 + "/ai1", AITerminalConfiguration.Rse);
            //The next line is try two DAQ cards for one cavity. (14 Nov 2016) Feel free to delete it.
            AddAnalogInputChannel("VISp2", TCLBoard2 + "/ai3", AITerminalConfiguration.Rse);




            // map the analog output channels
            AddAnalogOutputChannel("pumplaser", TCLBoard + "/ao1");
            AddAnalogOutputChannel("v21repump", TCLBoard + "/ao2");
            AddAnalogOutputChannel("laser", daqBoard + "/ao0");//Pin 22
            AddAnalogOutputChannel("rampfb", TCLBoard + "/ao0");


            AddAnalogOutputChannel("v1repump", TCLBoard2 + "/ao1");
            AddAnalogOutputChannel("VISrampfb", TCLBoard2 + "/ao0");
            AddAnalogOutputChannel("v2repump", TCLBoard2 + "/ao2");

           

            //map the counter channels
            //AddCounterChannel("pmt", daqBoard + "/ctr0");
            //AddCounterChannel("sample clock", daqBoard + "/ctr1");

            //These need to be activated for the phase lock
            AddCounterChannel("phaseLockOscillator", daqBoard + "/ctr0"); //This should be the source pin of a counter PFI 8
            AddCounterChannel("phaseLockReference", daqBoard + "/PFI9"); //This should be the gate pin of the same counter - need to check it's name
        }

        public override void ConnectApplications()
        {
            // ask the remoting system for access to TCL2012
           // Type t = Type.GetType("TransferCavityLock2012.Controller, TransferCavityLock");
           // System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownClientType(t, "tcp://localhost:1190/controller.rem");
        }
    }
}
