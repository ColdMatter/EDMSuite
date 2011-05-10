using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;

namespace DAQ.HAL

{
    public class BufferGasHardware : DAQ.HAL.Hardware
    {
        public BufferGasHardware()
        {
            // add the boards
            Boards.Add("daq", "/dev1");
            Boards.Add("pg", "/dev2");

            // map the digital channels
            string pgBoard = (string)Boards["pg"];

            AddDigitalOutputChannel("q", pgBoard, 0, 0);//Pin 10
            AddDigitalOutputChannel("aom", pgBoard, 1, 1);//
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
            Info.Add("PGClockLine", Boards["pg"] + "/PFI2");
            Info.Add("PatternGeneratorBoard", pgBoard);
            Info.Add("PGType", "dedicated");

            // map the analog channels
            string daqBoard = (string)Boards["daq"];
            AddAnalogInputChannel("detector1", daqBoard + "/ai0", AITerminalConfiguration.Nrse);//Pin 68
            AddAnalogInputChannel("detector2", daqBoard + "/ai3", AITerminalConfiguration.Nrse);//Pin 
            AddAnalogInputChannel("detector3", daqBoard + "/ai8", AITerminalConfiguration.Nrse);//Pin 34
            AddAnalogInputChannel("pressure1", daqBoard + "/ai1", AITerminalConfiguration.Nrse);//Pin 33 pressure reading at the moment
            AddAnalogInputChannel("cavity", daqBoard + "/ai2", AITerminalConfiguration.Nrse);//Pin 65
            AddAnalogInputChannel("cavitylong", daqBoard + "/ai4", AITerminalConfiguration.Nrse);//Pin 28
            AddAnalogInputChannel("cavityshort", daqBoard + "/ai5", AITerminalConfiguration.Nrse);//Pin 60


            AddAnalogOutputChannel("laser", daqBoard + "/ao0");//Pin 22
            AddAnalogOutputChannel("phaseLockAnalogOutput", daqBoard + "/ao1"); //pin 21

            //map the counter channels
            //AddCounterChannel("pmt", daqBoard + "/ctr0");
            //AddCounterChannel("sample clock", daqBoard + "/ctr1");

            //These need to be activated for the phase lock
            AddCounterChannel("phaseLockOscillator", daqBoard + "/ctr0"); //This should be the source pin of a counter
            AddCounterChannel("phaseLockReference", daqBoard + "/PFI9"); //This should be the gate pin of the same counter - need to check it's name
        }
    }
}
