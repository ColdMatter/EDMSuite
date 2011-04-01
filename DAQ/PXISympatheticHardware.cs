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

            // the analog triggers
            Info.Add("analogTrigger3", aoBoard + "/PFI0");
            Info.Add("analogTrigger2", multiDAQ + "/PFI1");
           
            // map the digital output channels
            AddDigitalOutputChannel("aom0Enable", multiDAQ, 0, 0);
            AddDigitalOutputChannel("aom1Enable", multiDAQ, 0, 1);
            AddDigitalOutputChannel("aom2Enable", multiDAQ, 0, 2);
            AddDigitalOutputChannel("aom3Enable", multiDAQ, 0, 3);
            AddDigitalOutputChannel("cavityTriggerOut", multiDAQ, 0, 4);
           
            // map the analog input channels
            AddAnalogInputChannel("p1", multiDAQ + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("p2", multiDAQ + "/ai1", AITerminalConfiguration.Rse);
           
            // map the analog output channels
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
            AddAnalogOutputChannel("laser", aoBoard + "/ao1");
            AddAnalogOutputChannel("cavity", multiDAQ + "/ao1");

        }

    }
}
