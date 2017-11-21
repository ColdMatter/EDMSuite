using System;
using System.Linq;
using System.Collections.Generic;

using NationalInstruments.DAQmx;

using DAQ.Pattern;

namespace DAQ.HAL
{
    /// <summary>
    /// This is the specific hardware for the Navigator experiment. Currently, the channels used must be specified here. At a later date, the physical channels may be defined inside a settings file for the hardware controller.
    /// </summary>
    public class NavigatorHardware : DAQ.HAL.Hardware
    {
        public MMConfig config { get; set; }
        public NavigatorHardware()
        {
          
            //add information for MMConfig
            config = new MMConfig(false, false, false,Environment.Environs.Debug);
            config.HSDIOCard = true;
            config.UseAI = true;
            config.DigitalPatternClockFrequency = 20000000;
            config.UseMuquans = true;
            config.UseMMScripts = false;
            config.UseMSquared = true;
            Info.Add("MotMasterConfiguration", config);
            //add the boards - perhaps these values can be derived from a settings file
            Boards.Add("multiDAQ", "/Dev1");
            Boards.Add("analogOut", "/Dev2");
            //The HSDIO card cannot be referenced with a leading forward slash like DAQ cards
            Boards.Add("hsDigital", "Dev3");
            Boards.Add("analogIn", "/Dev4");
            string multiBoard = (string)Boards["multiDAQ"];
            string aoBoard = (string)Boards["analogOut"];
            string hsdioBoard = (string)Boards["hsDigital"];
            string aiBoard = (string)Boards["analogIn"];
            //Collect each type of board into a list - this is useful if we need to loop over each
            List<string> aoBoards = new List<string>();
            List<string> aiBoards = new List<string>();
            List<string> doBoards = new List<string>();
            aoBoards.Add(multiBoard);
            aiBoards.Add(aiBoard);
            aiBoards.Add(multiBoard);
            doBoards.Add(hsdioBoard);


            //A list of trigger lines for each card
            Info.Add("sampleClockLine", (string)Boards["hsDigital"] + "/PXI_Trig0");
            Info.Add("analogInTrigger0", (string)Boards["multiDAQ"] + "/PXI_Trig1");
            Info.Add("AOPatternTrigger", (string)Boards["analogOut"] + "/PXI_Trig1");
            //Info.Add("analogInClock", (string)Boards["analogOut"] + "/ao/SampleClock");
            Info.Add("analogInTrigger1", (string)Boards["multiDAQ"] + "/PXI_Trig2");
            Info.Add("HSTrigger", "PXI_Trig1");
          
            //Add identifiers for each card
          
            Info.Add("analogOutBoards", aoBoards);
            Info.Add("analogInBoards", aiBoards);
            Info.Add("digitalBoards", doBoards);
            Info.Add("AIAcquireTrigger", "pfi0");
            //Add other instruments such as serial channels
            Instruments.Add("muquansSlave", new MuquansRS232("ASRL18::INSTR","slave"));
            Instruments.Add("muquansAOM", new MuquansRS232("ASRL20::INSTR","aom"));
            Instruments.Add("microwaveSynth", new WindfreakSynth("ASRL13::INSTR"));

            Instruments.Add("MSquaredDCS", new ICEBlocDCS());
            Instruments.Add("MSquaredPLL", new ICEBlocPLL());
            //Instruments.Add("microwaveSynth", new Gigatronics7100Synth("GPIB1::19::INSTR"));

            
            //map the digital channels

            AddDigitalOutputChannel("motTTL", hsdioBoard, 0, 0);
            AddDigitalOutputChannel("lcTTL", hsdioBoard, 0, 1);
            AddDigitalOutputChannel("mphiTTL", hsdioBoard, 0, 2);
            AddDigitalOutputChannel("slaveDDSTrig", hsdioBoard, 0, 3);
            AddDigitalOutputChannel("msquaredTTL", hsdioBoard, 0, 15);
            AddDigitalOutputChannel("aomDDSTrig", hsdioBoard, 0, 5);
            AddDigitalOutputChannel("fp1MicrowaveTTL", hsdioBoard, 0, 4);
            AddDigitalOutputChannel("xaomTTL", hsdioBoard, 0, 7);
            AddDigitalOutputChannel("yaomTTL", hsdioBoard, 0, 8);
            AddDigitalOutputChannel("zpaomTTL", hsdioBoard, 0, 9);
            AddDigitalOutputChannel("zmaomTTL", hsdioBoard, 0, 10);
            AddDigitalOutputChannel("2DaomTTL", hsdioBoard, 0, 11);
            AddDigitalOutputChannel("pushaomTTL", hsdioBoard, 0, 12);
            AddDigitalOutputChannel("mainMicrowaveTTL", hsdioBoard, 0, 13);
            AddDigitalOutputChannel("acquisitionTrigger", hsdioBoard, 0, 14);
            AddDigitalOutputChannel("Digital Test", hsdioBoard, 0, 17);
            AddDigitalOutputChannel("fm1MicrowaveTTL", hsdioBoard, 0, 16);
            AddDigitalOutputChannel("serialPreTrigger", hsdioBoard, 0, 31);

            //map the analog output channels
            AddAnalogOutputChannel("motCTRL", aoBoard + "/ao0", -10, 10);
            AddAnalogOutputChannel("ramanCTRL", aoBoard + "/ao1", -10, 10);
            AddAnalogOutputChannel("mphiCTRL", aoBoard + "/ao2", -10, 10);
            AddAnalogOutputChannel("mot3DCoil", aoBoard + "/ao9", -10, 10);
            AddAnalogOutputChannel("mot2DCoil", aoBoard + "/ao11", -10, 10);
            AddAnalogOutputChannel("xbiasCoil", aoBoard + "/ao6", -10, 10);
            AddAnalogOutputChannel("ybiasCoil", aoBoard + "/ao7", -10, 10);
            AddAnalogOutputChannel("zbiasCoil", aoBoard + "/ao5", -10, 10);
            AddAnalogOutputChannel("xbias2DCoil", aoBoard + "/ao3", -10, 10);
            AddAnalogOutputChannel("ybias2DCoil", aoBoard + "/ao4", -10, 10);
            AddAnalogOutputChannel("vertPiezo", aoBoard + "/ao10", 0, 10);
            AddAnalogOutputChannel("horizPiezo", aoBoard + "/ao24", -10, 10);
            AddAnalogOutputChannel("xaomFreq", aoBoard + "/ao12", -10, 10);
            AddAnalogOutputChannel("yaomFreq", aoBoard + "/ao13", -10, 10);
            AddAnalogOutputChannel("zpaomFreq", aoBoard + "/ao14", -10, 10);
            AddAnalogOutputChannel("zmaomFreq", aoBoard + "/ao15", -10, 10);
            AddAnalogOutputChannel("2DaomFreq", aoBoard + "/ao16", -10, 10);
            AddAnalogOutputChannel("pushaomFreq", aoBoard + "/ao17", -10, 10);
            AddAnalogOutputChannel("xaomAtten", aoBoard + "/ao18", -10, 10);
            AddAnalogOutputChannel("yaomAtten", aoBoard + "/ao19", -10, 10);
            AddAnalogOutputChannel("zpaomAtten", aoBoard + "/ao20", -10, 10);
            AddAnalogOutputChannel("zmaomAtten", aoBoard + "/ao21", -10, 10);
            AddAnalogOutputChannel("2DaomAtten", aoBoard + "/ao22", -10, 10);
            AddAnalogOutputChannel("pushaomAtten", aoBoard + "/ao23", -10, 10);
           // AddAnalogOutputChannel("analogTest", aoBoard + "/ao24", -10, 10);

            //map the analog input channels
            AddAnalogInputChannel("accelerometer", aiBoard + "/ai0", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("photodiode", aiBoard + "/ai1", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("fibrePD", aiBoard + "/ai3", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("forwardRamanPD", multiBoard + "/ai0", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("backwardRamanPD", multiBoard + "/ai1", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("motPD", multiBoard + "/ai2", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("slave0Error", multiBoard + "/ai3", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("slave1Error", multiBoard + "/ai4", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("slave2Error", multiBoard + "/ai5", AITerminalConfiguration.Differential);

            AddCounterChannel("Counter", multiBoard + "/ctr0");

            //Adds a Channel map to convert channel names from old sequences
            Dictionary<string, string> channelMap = new Dictionary<string, string>();
            channelMap["cameraTTL"] = "mainMicrowaveTTL";
            channelMap["Analog Trigger"] = "fm1MicrowaveTTL";
            channelMap["ramanTTL"] = "lcTTL";
            channelMap["ramanDDSTrig"] = "msquaredTTL";
            channelMap["shutter"] = "fp1MicrowaveTTL";

            Info.Add("channelMap", channelMap);
        }
    }
}
