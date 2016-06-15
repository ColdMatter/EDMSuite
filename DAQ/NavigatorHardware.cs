using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;

namespace DAQ.HAL
{
    /// <summary>
    /// This is the specific hardware for the Navigator experiment. Currently, the channels used must be specified here. At a later date, the physical channels may be defined inside a settings file for the hardware controller.
    /// </summary>
    public class NavigatorHardware : DAQ.HAL.Hardware
    {
        public NavigatorHardware()
        {
            //add the boards - perhaps these values can be derived from a settings file
            Boards.Add("multiDAQ", "/dev1");
            Boards.Add("analogOut", "/dev2");
            Boards.Add("hsDigital", "/dev3");
            Boards.Add("analogIn", "/dev4");

            string multiBoard = (string)Boards["multiDAQ"];
            string aoBoard = (string)Boards["analogOut"];
            string hsdioBoard = (string)Boards["hsDigital"];
            string aiBoard = (string)Boards["analogIn"];

            //add various info - these will be added as I think of them. Things such as trigger channels and so on

            //Add other instruments such as serial channels
            Instruments.Add("muquansSlave", new RS232Instrument("ASRL18::INSTR"));
            Instruments.Add("muquansAOM", new RS232Instrument("ASRL20::INSTR"));

            //map the digital channels - again these could be defined in a settings file
            AddDigitalOutputChannel("motTTL", multiBoard, 0, 0);
            AddDigitalOutputChannel("mphiTTL", multiBoard, 0, 1);
            AddDigitalOutputChannel("ramanTTL",multiBoard, 0, 2);
            AddDigitalOutputChannel("cameraTTL", multiBoard, 0, 3);

            //map the analog output channels
            AddAnalogOutputChannel("motCTRL", aoBoard + "/ao0", 0, 5);
            AddAnalogOutputChannel("mphiCTRL", aoBoard + "/ao1", 0, 5);
            AddAnalogOutputChannel("ramanCTRL", aoBoard + "/ao2", 0, 5);

            //map the analog input channels
            AddAnalogInputChannel("photodiode", aiBoard + "/ai0", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("accelpos", aiBoard + "/ai1", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("accelmin", aiBoard + "/ai2", AITerminalConfiguration.Differential);

        }
    }
}
