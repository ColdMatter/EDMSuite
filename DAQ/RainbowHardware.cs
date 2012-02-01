using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;

namespace DAQ.HAL
{
    /// <summary>
    /// This is the specific hardware that the edm machine has. This class conforms
    /// to the Hardware interface.
    /// </summary>
    public class RainbowHardware : DAQ.HAL.Hardware
    {

        public RainbowHardware()
        {

            // add the boards
            Boards.Add("daq", "/PXI1Slot4");
            Boards.Add("TCLBoard", "/PXI1Slot6");
            string daqBoard = (string)Boards["daq"];
            string TCLBoard = (string)Boards["TCLBoard"];

            // add things to the info
            // the analog triggers
            Info.Add("analogTrigger0", (string)Boards["daq"] + "/PFI1");
            Info.Add("analogTrigger1", (string)Boards["daq"] + "/PFI2");
            Info.Add("sourceToDetect", 1.3);
            Info.Add("moleculeMass", 193.0);
            Info.Add("phaseLockControlMethod", "synth");
            Info.Add("PGClockLine", daqBoard + "/PFI4");
            Info.Add("PatternGeneratorBoard", daqBoard);
            Info.Add("PGType", "integrated");
            Info.Add("PGClockCounter", "/ctr0");

            //TCL Lockable lasers
            Info.Add("TCLLockableLasers", new string[] { "laser"});
            Info.Add("TCLPhotodiodes", new string[] { "cavity", "master", "p1" });// THE FIRST TWO MUST BE CAVITY AND MASTER PHOTODIODE!!!!
            Info.Add("TCL_Slave_Voltage_Limit_Upper", 2.0); //volts: Laser control
            Info.Add("TCL_Slave_Voltage_Limit_Lower", 0.0); //volts: Laser control
            Info.Add("TCL_Default_Gain", -0.01);
            Info.Add("TCL_Default_VoltageToLaser", 1.0);
            // Some matching up for TCL
            Info.Add("laser", "p1");
           // Info.Add("laser2", "p2");
           // Info.Add("laser3", "p3");
            Info.Add("TCLTrigger", TCLBoard + "/PFI0");

            // YAG laser
            yag = new BrilliantLaser("ASRL3::INSTR");

            // add the GPIB instruments
 
            // map the digital channels
            AddDigitalOutputChannel("valve", daqBoard, 0, 0);
            AddDigitalOutputChannel("flash", daqBoard, 0, 1);
            AddDigitalOutputChannel("q", daqBoard, 0, 2);
            AddDigitalOutputChannel("detector", daqBoard, 0, 3);
            AddDigitalOutputChannel("detectorprime", daqBoard, 0, 4); // this trigger is for switch scanning
            AddDigitalOutputChannel("aom", daqBoard, 0, 5); // this trigger is for switch scanning

            // map the analog input channels
            AddAnalogInputChannel("pmt", daqBoard + "/ai0", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("norm", daqBoard + "/ai1", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("iodine", daqBoard + "/ai2", AITerminalConfiguration.Nrse);
            //AddAnalogInputChannel("cavity", daqBoard + "/ai3", AITerminalConfiguration.Nrse);

            // map the analog output channels
            AddAnalogOutputChannel("laser", daqBoard + "/ao0");
            AddAnalogOutputChannel("laser2", daqBoard + "/ao2");
            AddAnalogOutputChannel("laser3", daqBoard + "/ao3");
            AddAnalogInputChannel("master", TCLBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("p1", TCLBoard + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("cavity", TCLBoard + "/ai2", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("p2", TCLBoard + "/ai0", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("p3", TCLBoard + "/ai5", AITerminalConfiguration.Rse);

            //Transfer Cavity Lock
            //AddAnalogOutputChannel("cavity", daqBoard + "/ao1");
            //Info.Add("analogTrigger2", (string)Boards["daq"] + "/PFI0");
            //Info.Add("analogTrigger3", (string)Boards["daq"] + "/PFI3");
            //AddDigitalOutputChannel("scanTrigger", daqBoard, 0, 6);
            //AddAnalogInputChannel("slavepd", daqBoard + "/ai4", AITerminalConfiguration.Nrse);
            //AddAnalogInputChannel("masterpd", daqBoard + "/ai5", AITerminalConfiguration.Nrse);


             // map the counter channels
            //AddCounterChannel("phaseLockOscillator", daqBoard + "/ctr7");
            //AddCounterChannel("phaseLockReference", daqBoard + "/pfi10");
            //AddCounterChannel("northLeakage", counterBoard + "/ctr0");
            //AddCounterChannel("southLeakage", counterBoard + "/ctr1");

        }

    }
}
