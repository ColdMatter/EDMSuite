using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.DAQmx;

namespace DAQ.HAL
{
    /// <summary>
    /// This is the specific hardware that the RF MOT experiment has. This class conforms
    /// to the Hardware interface.
    /// </summary>
    public class RFMOTHardware : DAQ.HAL.Hardware
    {
        public RFMOTHardware()
        {

        Boards.Add("analog", "/PXI1Slot4");

        Boards.Add("multiDAQPCI", "/Dev1");

        string daqBoard = (string)Boards["analog"];

        string multiDAQ = (string)Boards["multiDAQPCI"];

        #region PXI AO Card

        #region DO channels

        AddDigitalOutputChannel("repumpShutter", daqBoard, 0, 0);

        AddDigitalOutputChannel("coolaom2", daqBoard, 0, 1);

        AddDigitalOutputChannel("coolaom", daqBoard, 0, 2);
            
        AddDigitalOutputChannel("camtrig", daqBoard, 0, 3);

        AddDigitalOutputChannel("ctrInputSelect", daqBoard, 0, 6);

        AddDigitalOutputChannel("AnalogPatternTrigger", daqBoard, 0, 5);

        AddDigitalOutputChannel("refaom", daqBoard, 0, 7);

        #endregion

        #region AO Channels

        AddAnalogOutputChannel("motfet", daqBoard + "/ao0");

        AddAnalogOutputChannel("coolsetpt", daqBoard + "/ao1");

        AddAnalogOutputChannel("motlightatn2", daqBoard + "/ao2");

        AddAnalogOutputChannel("motlightatn", daqBoard + "/ao3");
            
        AddAnalogOutputChannel("coolingfeedfwd", daqBoard + "/ao5");

        AddAnalogOutputChannel("zbias", daqBoard + "/ao7");

        #endregion

        #endregion


        #region PCI AI Card

        #region AO

        AddAnalogOutputChannel("biasA", multiDAQ + "/ao0");

        AddAnalogOutputChannel("biasB", multiDAQ + "/ao1");

        #endregion

        #region DO

        AddDigitalOutputChannel("PCIDOTest", multiDAQ, 0, 0);

        AddDigitalOutputChannel("multiDAQDO1", multiDAQ, 0, 1);

        #endregion

        #region AI

        AddAnalogInputChannel("multiDAQAI1", multiDAQ + "/ai1", AITerminalConfiguration.Differential);

        AddAnalogInputChannel("coolerrsig", multiDAQ + "/ai2", AITerminalConfiguration.Differential);

        AddAnalogInputChannel("MOTCoilSense", multiDAQ + "/ai3", AITerminalConfiguration.Differential);

        AddAnalogInputChannel("MOTFluoresence", multiDAQ + "/ai4", AITerminalConfiguration.Differential);

        AddAnalogInputChannel("MOTFieldGradient", multiDAQ + "/ai6", AITerminalConfiguration.Differential);

        #endregion

        #endregion
        //AddCounterChannel("pmt", multiDAQ + "/ctr0"); 

        Instruments.Add("dds", new NovatechSerialPort());
       // AddCounterChannel("PGClockCounter", daqBoard + "/ctr0");

        Instruments.Add("mcFrqCtr", new mcFreqCtr());
      //  Info.Add("PGClockLine", daqBoard + "/PFI5");

        Info.Add("PGClockLine", multiDAQ + "/PFI0");//This is the channel that the AO sample clock will be exported TO on the PCI Card.
        Info.Add("PatternGeneratorBoard", daqBoard);
        Info.Add("PGClockCounter",  "/ctr0");
        Info.Add("APGClockCounter", daqBoard + "/ctr0");
        Info.Add("AOClockSource", daqBoard + "/ctr0");
        Info.Add("AOPatternTrigger", daqBoard + "/PFI0");
        Info.Add("AIAcquireTrigger", multiDAQ + "/PFI1");
        Info.Add("PGType", "integrated");
       // Info.Add("PGTrigger",);

        AddCalibration("MOTFieldGradient", new PolynomialCalibration(new double[] {-0.0156971,-2.06836},-10.0,10.0));
        }
    }
}
