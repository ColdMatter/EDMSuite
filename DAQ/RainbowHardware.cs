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
            Boards.Add("daq", "/PXI1Slot6");
            string daqBoard = (string)Boards["daq"];

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

            // YAG laser
//            yag = new BrilliantLaser("ASRL3::INSTR");

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
            AddAnalogInputChannel("cavity", daqBoard + "/ai3", AITerminalConfiguration.Nrse);

            // map the analog output channels
            AddAnalogOutputChannel("b", daqBoard + "/ao1");

             // map the counter channels
            //AddCounterChannel("phaseLockOscillator", daqBoard + "/ctr7");
            //AddCounterChannel("phaseLockReference", daqBoard + "/pfi10");
            //AddCounterChannel("northLeakage", counterBoard + "/ctr0");
            //AddCounterChannel("southLeakage", counterBoard + "/ctr1");

        }

    }
}
