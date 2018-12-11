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
        public ConfocalHardware(string id)
        {
            switch (id)
            {
                case "ColdConfocal":
                    // Add board
                    Boards.Add("daq", "Dev1");
                    string daqBoard = (string)Boards["daq"];

                    // map the analog channels
                    AddAnalogInputChannel("AI0", daqBoard + "/ai0", AITerminalConfiguration.Differential);
                    AddAnalogInputChannel("AI1", daqBoard + "/ai1", AITerminalConfiguration.Differential);
                    AddAnalogInputChannel("AI2", daqBoard + "/ai2", AITerminalConfiguration.Differential);
                    AddAnalogInputChannel("AI3", daqBoard + "/ai3", AITerminalConfiguration.Differential);
                    AddAnalogInputChannel("AI4", daqBoard + "/ai4", AITerminalConfiguration.Differential);
                    AddAnalogInputChannel("AI5", daqBoard + "/ai5", AITerminalConfiguration.Differential);

                    //map the analogue output channels
                    AddAnalogOutputChannel("AO0", daqBoard + "/ao0", -5, 5);
                    AddAnalogOutputChannel("AO1", daqBoard + "/ao1", -5, 5);

                    // map the counter channels
                    AddCounterChannel("APD0", daqBoard + "/ctr0");
                    AddCounterChannel("APD1", daqBoard + "/ctr1");
                    AddCounterChannel("APD2", daqBoard + "/ctr2");

                    // pause trigger
                    AddDigitalOutputChannel("StartTrigger", daqBoard, 0, 23);
                    Info.Add("StartTriggerReader", "/dev1/PFI7");

                    // sample clock
                    AddCounterChannel("SampleClock", daqBoard + "/ctr3");
                    Info.Add("SampleClockReader", "/dev1/PFI15");

                    // IP
                    Info.Add("IPAdress", "192.168.1.23");
                    Info.Add("Port", 23232);

                    break;

                case "RoomTConfocal":
                    // Add board
                    Boards.Add("daq", "Dev1");
                    daqBoard = (string)Boards["daq"];

                    // map the analog channels
                    AddAnalogInputChannel("AI0", daqBoard + "/ai0", AITerminalConfiguration.Differential);
                    AddAnalogInputChannel("AI1", daqBoard + "/ai1", AITerminalConfiguration.Differential);
                    AddAnalogInputChannel("AI2", daqBoard + "/ai2", AITerminalConfiguration.Differential);
                    AddAnalogInputChannel("AI3", daqBoard + "/ai3", AITerminalConfiguration.Differential);
                    AddAnalogInputChannel("AI4", daqBoard + "/ai4", AITerminalConfiguration.Differential);
                    AddAnalogInputChannel("AI5", daqBoard + "/ai5", AITerminalConfiguration.Differential);

                    //map the analogue output channels
                    AddAnalogOutputChannel("AO0", daqBoard + "/ao0", -5, 5);
                    AddAnalogOutputChannel("AO1", daqBoard + "/ao1", -5, 5);

                    // map the counter channels
                    AddCounterChannel("APD0", daqBoard + "/ctr0");
                    AddCounterChannel("APD1", daqBoard + "/ctr1");
                    AddCounterChannel("APD2", daqBoard + "/ctr2");

                    // pause trigger
                    AddDigitalOutputChannel("StartTrigger", daqBoard, 0, 0);
                    Info.Add("StartTriggerReader", "/dev1/PFI7");

                    // sample clock
                    AddCounterChannel("SampleClock", daqBoard + "/ctr3");
                    Info.Add("SampleClockReader", "/dev1/PFI15");

                    // IP
                    Info.Add("IPAdress", "192.168.1.24");
                    Info.Add("Port", 24242);

                    break;

                case "SASRb":
                    // Add board
                    Boards.Add("daq", "Dev1");
                    daqBoard = (string)Boards["daq"];

                    // map the analog channels
                    AddAnalogInputChannel("AI0", daqBoard + "/ai0", AITerminalConfiguration.Differential);
                    AddAnalogInputChannel("AI1", daqBoard + "/ai1", AITerminalConfiguration.Differential);
                    AddAnalogInputChannel("AI2", daqBoard + "/ai2", AITerminalConfiguration.Differential);
                    AddAnalogInputChannel("AI3", daqBoard + "/ai3", AITerminalConfiguration.Differential);
                    AddAnalogInputChannel("AI4", daqBoard + "/ai4", AITerminalConfiguration.Differential);
                    AddAnalogInputChannel("AI5", daqBoard + "/ai5", AITerminalConfiguration.Differential);

                    //map the analogue output channels
                    AddAnalogOutputChannel("AO0", daqBoard + "/ao0", -5, 5);
                    AddAnalogOutputChannel("AO1", daqBoard + "/ao1", -5, 5);

                    // map the counter channels
                    AddCounterChannel("APD0", daqBoard + "/ctr0");
                    AddCounterChannel("APD1", daqBoard + "/ctr1");
                    AddCounterChannel("APD2", daqBoard + "/ctr2");

                    // pause trigger
                    AddDigitalOutputChannel("StartTrigger", daqBoard, 0, 0);
                    Info.Add("StartTriggerReader", "/dev1/PFI1");

                    // sample clock
                    AddCounterChannel("SampleClock", daqBoard + "/ctr3");
                    Info.Add("SampleClockReader", "/dev1/PFI15");

                    // IP
                    Info.Add("IPAdress", "192.168.1.23");
                    Info.Add("Port", 23232);

                    break;

                default:
                    throw new System.Exception("No hardware defined");
            }
        }
    }
}
