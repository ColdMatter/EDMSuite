using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;

using NationalInstruments.DAQmx;

using DAQ.Pattern;
using DAQ.Remoting;

namespace DAQ.HAL
{
    /// <summary>
    /// Confocal Microscope specific hardware. Inherits from the base class Hardware. 
    /// </summary>
    public class ConfocalHardware : DAQ.HAL.Hardware
    {
        public ConfocalHardware()
        {
            // Add board
            Boards.Add("daq", "Dev1");
            string daqBoard = (string)Boards["daq"];

            // map the analog channels
            AddAnalogInputChannel("GalvoX", daqBoard + "/ai0", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("GalvoY", daqBoard + "/ai1", AITerminalConfiguration.Differential);

            //map the analogue output channels
            AddAnalogOutputChannel("GalvoXControl", daqBoard + "/ao0", -5, 5);
            AddAnalogOutputChannel("GalvoYControl", daqBoard + "/ao1", -5, 5);

            // map the counter channels
            AddCounterChannel("ConfocalAPD", daqBoard + "/ctr0");

            // sample clock
            AddCounterChannel("SampleClock", daqBoard + "/ctr3");
            Info.Add("SampleClockReader", "/dev1/PFI15");
        }

        public ConfocalHardware(string ConfocalAPDPath)
        {
            // Add board
            Boards.Add("daq", "Dev1");
            string daqBoard = (string)Boards["daq"];

            // map the analog channels
            AddAnalogInputChannel("GalvoX", daqBoard + "/ai0", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("GalvoY", daqBoard + "/ai1", AITerminalConfiguration.Differential);

            //map the analogue output channels
            AddAnalogOutputChannel("GalvoXControl", daqBoard + "/ao0", -5, 5);
            AddAnalogOutputChannel("GalvoYControl", daqBoard + "/ao1", -5, 5);

            // map the counter channels
            AddCounterChannel("ConfocalAPD", ConfocalAPDPath);

            // sample clock
            AddCounterChannel("SampleClock", daqBoard + "/ctr3");
            Info.Add("SampleClockReader", "/dev1/PFI15");
        }
    }
}
