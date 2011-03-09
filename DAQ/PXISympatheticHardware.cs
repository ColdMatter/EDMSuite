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
    public class PXISympatheticHardware : DAQ.HAL.Hardware
    {

        public PXISympatheticHardware()
        {

            // add the boards
            Boards.Add("multiDAQ", "/PXI1Slot4");
            Boards.Add("aoBoard", "/PXI1Slot5");
            // this drives the rf attenuators
            string multiDAQ = (string)Boards["multiDAQ"];
            string aoBoard = (string)Boards["aoBoard"];
            
            // add things to the info
            //Info.Add("PGType", "split");
            //Info.Add("ScanMaster-PGAllocation", "lower");
            //Info.Add("SHC-PGAllocation", "upper");
            Info.Add("analogTrigger0", multiDAQ + "/PFI0"); //DAQ Pin 11
            Info.Add("PGClockLineLow", multiDAQ + "/PFI1");
            Info.Add("PGClockLineHigh", multiDAQ + "/PFI2");
            Info.Add("PatternGeneratorBoard", multiDAQ);
            Info.Add("PGClockCounter", "/ctr0");

            //Test this
            //Info.Add("PGType", "dedicated");
            Info.Add("PGType", "integrated");

            // the analog triggers
            //Info.Add("analogTrigger0", (string)Boards["analogIn"] + "/PFI0");
           
            // map the digital output channels
            // Control of atoms
            AddDigitalOutputChannel("aom0Enable", multiDAQ, 0, 16);
            AddDigitalOutputChannel("aom1Enable", multiDAQ, 0, 17);
            AddDigitalOutputChannel("aom2Enable", multiDAQ, 0, 18);
            AddDigitalOutputChannel("aom3Enable", multiDAQ, 0, 19);
            
            //Control of molecules
            AddDigitalOutputChannel("valve", multiDAQ, 0, 0); 
            AddDigitalOutputChannel("valve2", multiDAQ, 0, 1);
            AddDigitalOutputChannel("q", multiDAQ, 0, 2);
            AddDigitalOutputChannel("discharge", multiDAQ, 0, 3);
            AddDigitalOutputChannel("aom", multiDAQ, 0, 4); 
            AddDigitalOutputChannel("flash2", multiDAQ, 0, 5); 
            AddDigitalOutputChannel("q2", multiDAQ, 0, 6); 
            AddDigitalOutputChannel("detector", multiDAQ, 0, 7);
            AddDigitalOutputChannel("detectorprime", multiDAQ, 0, 8);
            AddDigitalOutputChannel("flash", multiDAQ, 0, 9);
            
            

            // map the analog input channels
            AddAnalogInputChannel("pmt", multiDAQ + "/ai0", AITerminalConfiguration.Rse); //Pin 68
            AddAnalogInputChannel("lockcavity", multiDAQ + "/ai1", AITerminalConfiguration.Rse); //Pin 33
            AddAnalogInputChannel("probepower", multiDAQ + "/ai9", AITerminalConfiguration.Rse); //Pin 66
           
            // map the analog output channels
            // Control of atoms
            AddAnalogOutputChannel("aom0amplitude", aoBoard + "/ao8");
            AddAnalogOutputChannel("aom0frequency", aoBoard + "/ao9");
            AddAnalogOutputChannel("aom1amplitude", aoBoard + "/ao10");
            AddAnalogOutputChannel("aom1frequency", aoBoard + "/ao11");
            AddAnalogOutputChannel("aom2amplitude", aoBoard + "/ao12");
            AddAnalogOutputChannel("aom2frequency", aoBoard + "/ao13");
            AddAnalogOutputChannel("aom3amplitude", aoBoard + "/ao14");
            AddAnalogOutputChannel("aom3frequency", aoBoard + "/ao15");
            AddAnalogOutputChannel("coil0Current", aoBoard + "/ao16");
            AddAnalogOutputChannel("coil1Current", aoBoard + "/ao17");

            //Control of molecules
            AddAnalogOutputChannel("laser", aoBoard + "/ao0"); // Pin 22
            AddAnalogOutputChannel("highvoltage", aoBoard + "/ao1"); // Note - this is just here because a channel called "highvoltage" has been hard-wired into DecelerationHardwareControl - this needs to be rectified
            AddAnalogOutputChannel("cavity", aoBoard + "/ao1"); // Pin 21

            // map the counter channels
            AddCounterChannel("pmt", multiDAQ + "/ctr0"); //Source is pin 37, gate is pin 3, out is pin 2
            AddCounterChannel("sample clock", multiDAQ + "/ctr1"); //Source is pin 42, gate is pin 41, out is pin 40

        }

    }
}
