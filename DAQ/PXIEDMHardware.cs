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
    public class PXIEDMHardware : DAQ.HAL.Hardware
    {
        public PXIEDMHardware()
        {

            // add the boards
            Boards.Add("daq", "/PXI1Slot18");
            Boards.Add("pg", "/PXI1Slot10");
            Boards.Add("counter", "/PXI1Slot3");
            Boards.Add("aoBoard", "/PXI1Slot4");
            // this drives the rf attenuators
            Boards.Add("usbDAQ1", "/Dev2");
            Boards.Add("analogIn", "/PXI1Slot2");
            Boards.Add("usbDAQ2", "/dev1");
            Boards.Add("usbDAQ3", "/dev4");
            Boards.Add("usbDAQ4", "/dev3");
            Boards.Add("tclBoard", "/PXI1Slot9");
            string pgBoard = (string)Boards["pg"];
            string daqBoard = (string)Boards["daq"];
            string counterBoard = (string)Boards["counter"];
            string aoBoard = (string)Boards["aoBoard"];
            string usbDAQ1 = (string)Boards["usbDAQ1"];
            string analogIn = (string)Boards["analogIn"];
            string usbDAQ2 = (string)Boards["usbDAQ2"];
            string usbDAQ3 = (string)Boards["usbDAQ3"];
            string usbDAQ4 = (string)Boards["usbDAQ4"];
            string tclBoard = (string)Boards["tclBoard"];

            // add things to the info
            // the analog triggersf
            Info.Add("analogTrigger0", (string)Boards["analogIn"] + "/PFI0");
            Info.Add("analogTrigger1", (string)Boards["analogIn"] + "/PFI1");

            Info.Add("sourceToDetect", 1.3);
            Info.Add("moleculeMass", 193.0);
            Info.Add("machineLengthRatio", 3.842);
            Info.Add("defaultGate",new double[] {2190, 80});


            Info.Add("phaseLockControlMethod", "synth");
            Info.Add("PGClockLine", pgBoard + "/PFI4"); //Mapped to PFI2 on 6533 connector
            Info.Add("PatternGeneratorBoard", pgBoard);
            Info.Add("PGType", "dedicated");
            // rf counter switch control seq``
            Info.Add("IodineFreqMon", new bool[] { false, false }); // IN 1
            Info.Add("pumpAOMFreqMon", new bool[] { false, true }); // IN 2
            Info.Add("FLModulationFreqMon", new bool[] { true, false }); // IN 3

            Info.Add("PGTrigger", pgBoard + "/PFI5"); //Mapped to PFI7 on 6533 connector

            // YAG laser
            yag = new BrilliantLaser("ASRL2::INSTR");

            // add the GPIB/RS232 instruments
            Instruments.Add("green", new HP8657ASynth("GPIB0::7::INSTR"));
            Instruments.Add("red", new HP3325BSynth("GPIB0::12::INSTR"));
            Instruments.Add("4861", new ICS4861A("GPIB0::4::INSTR"));
            Instruments.Add("bCurrentMeter", new HP34401A("GPIB0::22::INSTR"));
            Instruments.Add("rfCounter", new Agilent53131A("GPIB0::3::INSTR"));
            //Instruments.Add("rfCounter2", new Agilent53131A("GPIB0::5::INSTR"));
            Instruments.Add("rfPower", new HP438A("GPIB0::13::INSTR"));
            Instruments.Add("BfieldController", new SerialDAQ("ASRL12::INSTR"));
            Instruments.Add("rfCounter2", new SerialAgilent53131A("ASRL8::INSTR"));
            Instruments.Add("probePolControl", new SerialMotorControllerBCD("ASRL5::INSTR"));
            Instruments.Add("pumpPolControl", new SerialMotorControllerBCD("ASRL3::INSTR"));


            // map the digital channels
            // these channels are generally switched by the pattern generator
            // they're all in the lower half of the pg
            AddDigitalOutputChannel("valve", pgBoard, 0, 0);
            AddDigitalOutputChannel("flash", pgBoard, 0, 1);
            AddDigitalOutputChannel("q", pgBoard, 0, 2);
            AddDigitalOutputChannel("detector", pgBoard, 0, 3);
            AddDigitalOutputChannel("detectorprime", pgBoard, 1, 2); // this trigger is for switch scanning
            // see ModulatedAnalogShotGatherer.cs
            // for details.
            AddDigitalOutputChannel("rfSwitch", pgBoard, 0, 4);
            AddDigitalOutputChannel("fmSelect", pgBoard, 1, 0);      // This line selects which fm voltage is
            // sent to the synth.
            AddDigitalOutputChannel("attenuatorSelect", pgBoard, 0, 5);    // This line selects the attenuator voltage
            // sent to the voltage-controlled attenuator.
            AddDigitalOutputChannel("piFlip", pgBoard, 1, 1);
            AddDigitalOutputChannel("ttlSwitch", pgBoard, 1, 3);	// This is the output that the pg
            // will switch if it's switch scanning.
            AddDigitalOutputChannel("scramblerEnable", pgBoard, 1, 4);
            
            //RF Counter Control (single pole 4 throw)
            //AddDigitalOutputChannel("rfCountSwBit1", pgBoard, 3, 5);
            //AddDigitalOutputChannel("rfCountSwBit2", pgBoard, 3, 6);
            
            // new rf amp blanking
            AddDigitalOutputChannel("rfAmpBlanking", pgBoard, 1, 5);

            // these channel are usually software switched - they should not be in
            // the lower half of the pattern generator
            AddDigitalOutputChannel("b", pgBoard, 2, 0);
            AddDigitalOutputChannel("notB", pgBoard, 2, 1);
            AddDigitalOutputChannel("db", pgBoard, 2, 2);
            AddDigitalOutputChannel("notDB", pgBoard, 2, 3);
            //			AddDigitalOutputChannel("notEOnOff", pgBoard, 2, 4);  // this line seems to be broken on our pg board
            // 			AddDigitalOutputChannel("eOnOff", pgBoard, 2, 5);  // this and the above are not used now we have analog E control
            AddDigitalOutputChannel("targetStepper", pgBoard, 2, 5);
            AddDigitalOutputChannel("ePol", pgBoard, 2, 6);
            AddDigitalOutputChannel("notEPol", pgBoard, 2, 7);
            AddDigitalOutputChannel("eBleed", pgBoard, 3, 0);
            AddDigitalOutputChannel("eSwitching", aoBoard, 0, 6);
            AddDigitalOutputChannel("piFlipEnable", pgBoard, 3, 1);
            AddDigitalOutputChannel("notPIFlipEnable", pgBoard, 3, 5);
            AddDigitalOutputChannel("pumpShutter", pgBoard, 3, 3);
            AddDigitalOutputChannel("probeShutter", pgBoard, 3, 4);
            AddDigitalOutputChannel("argonShutter", pgBoard, 3, 2);
            AddDigitalOutputChannel("patternTTL", aoBoard, 0, 7);

            //I2 Lock Control
            AddDigitalOutputChannel("I2PropSwitch", pgBoard, 2, 4);
            AddDigitalOutputChannel("I2IntSwitch", pgBoard, 3, 6);
           


            AddDigitalOutputChannel("fibreAmpEnable", aoBoard, 0, 0);

            // Map the digital input channels
            AddDigitalInputChannel("fibreAmpMasterErr", aoBoard, 0, 1);
            AddDigitalInputChannel("fibreAmpSeedErr", aoBoard, 0, 2);
            AddDigitalInputChannel("fibreAmpBackFeflectErr", aoBoard, 0, 3);
            AddDigitalInputChannel("fibreAmpTempErr", aoBoard, 0, 4);
            AddDigitalInputChannel("fibreAmpPowerSupplyErr", aoBoard, 0, 5);

            // map the analog channels
            // These channels are on the daq board. Used mainly for diagnostic purposes.
            // On no account should they switch during the edm acquisition pattern.
            AddAnalogInputChannel("diodeLaserCurrent", daqBoard + "/ai0", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("iodine", daqBoard + "/ai2", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("cavity", daqBoard + "/ai3", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("probePD", daqBoard + "/ai4", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("pumpPD", daqBoard + "/ai5", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("northLeakage", daqBoard + "/ai6", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("southLeakage", daqBoard + "/ai7", AITerminalConfiguration.Nrse);
            // Used ai13,11 & 12 over 6,7 & 8 for miniFluxgates, because ai8, 9 have an isolated ground. 
            AddAnalogInputChannel("miniFlux1", daqBoard + "/ai13", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("miniFlux2", daqBoard + "/ai11", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("miniFlux3", daqBoard + "/ai12", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("ground", daqBoard + "/ai14", AITerminalConfiguration.Nrse);
            AddAnalogInputChannel("piMonitor", daqBoard + "/ai10", AITerminalConfiguration.Nrse);
            //AddAnalogInputChannel("diodeLaserRefCavity", daqBoard + "/ai13", AITerminalConfiguration.Nrse);
            // Don't use ai10, cross talk with other channels on this line

            // high quality analog inputs (will be) on the S-series analog in board
            // The last number in AddAnalogInputChannel is an optional calibration which turns VuS 
            AddAnalogInputChannel("top", analogIn + "/ai0", AITerminalConfiguration.Differential, 0.1);
            AddAnalogInputChannel("norm", analogIn + "/ai1", AITerminalConfiguration.Differential, 0.02);
            AddAnalogInputChannel("magnetometer", analogIn + "/ai2", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("gnd", analogIn + "/ai3", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("battery", analogIn + "/ai4", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("piMonitor", analogIn + "/ai5", AITerminalConfiguration.Differential);
            //AddAnalogInputChannel("bFieldCurrentMonitor", analogIn + "/ai6", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("reflectedrf1Amplitude", analogIn + "/ai5", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("reflectedrf2Amplitude", analogIn + "/ai6", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("rfCurrent", analogIn + "/ai7 ", AITerminalConfiguration.Differential);

            AddAnalogOutputChannel("phaseScramblerVoltage", aoBoard + "/ao0");
            AddAnalogOutputChannel("b", aoBoard + "/ao1");


            // rf rack control
            //AddAnalogInputChannel("rfPower", usbDAQ1 + "/ai0", AITerminalConfiguration.Rse);

            AddAnalogOutputChannel("rf1Attenuator", usbDAQ1 + "/ao0", 0, 5);
            AddAnalogOutputChannel("rf2Attenuator", usbDAQ1 + "/ao1", 0, 5);
            AddAnalogOutputChannel("rf1FM", usbDAQ2 + "/ao0", 0, 5);
            AddAnalogOutputChannel("rf2FM", usbDAQ2 + "/ao1", 0, 5);

            // E field control and monitoring
            AddAnalogInputChannel("cPlusMonitor", usbDAQ3 + "/ai1", AITerminalConfiguration.Differential);
            AddAnalogInputChannel("cMinusMonitor", usbDAQ3 + "/ai2", AITerminalConfiguration.Differential);

            AddAnalogOutputChannel("cPlus", usbDAQ3 + "/ao0", 0, 10);
            AddAnalogOutputChannel("cMinus", usbDAQ3 + "/ao1", 0, 10);

            // B field control
            //AddAnalogOutputChannel("steppingBBias", usbDAQ4 + "/ao0", 0, 5);


            // map the counter channels
            AddCounterChannel("phaseLockOscillator", counterBoard + "/ctr7");
            AddCounterChannel("phaseLockReference", counterBoard + "/pfi10");
            //AddCounterChannel("northLeakage", counterBoard + "/ctr0");
            //AddCounterChannel("southLeakage", counterBoard + "/ctr1");

            //TCL Lockable lasers
            //Info.Add("TCLLockableLasers", new string[][] { new string[] { "flPZT2" }, /*new string[] { "flPZT2Temp" },*/ new string[] { "fibreAOM", "flPZT2Temp" } });
            Info.Add("TCLLockableLasers", new string[] { "flPZT2" }); //, new string[] { "flPZT2Temp" }, new string[] { "fibreAOM"} });
            Info.Add("TCLPhotodiodes", new string[] {"transCavV", "master", "p1" });// THE FIRST TWO MUST BE CAVITY AND MASTER PHOTODIODE!!!!
            Info.Add("TCL_Slave_Voltage_Limit_Upper", 10.0); //volts: Laser control
            Info.Add("TCL_Slave_Voltage_Limit_Lower", 0.0); //volts: Laser control
            Info.Add("TCL_Default_Gain", -1.1);
            //Info.Add("TCL_Default_ScanPoints", 250);
            Info.Add("TCL_Default_VoltageToLaser", 2.5);
            Info.Add("TCL_Default_VoltageToDependent", 1.0);
            // Some matching up for TCL
            Info.Add("flPZT2", "p1");
            Info.Add("flPZT2Temp", "p1");
            //Info.Add("fibreAOM", "p1");
            Info.Add("TCLTrigger", tclBoard + "/PFI0");
            Info.Add("TCL_MAX_INPUT_VOLTAGE", 10.0);

            AddAnalogInputChannel("transCavV", tclBoard + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("master", tclBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("p1", tclBoard + "/ai2", AITerminalConfiguration.Rse);

            // Laser control
            //AddAnalogOutputChannel("flPZT", usbDAQ4 + "/ao1", 0, 5);
            AddAnalogOutputChannel("flPZT", aoBoard + "/ao7", 0, 10);
            AddAnalogOutputChannel("flPZT2", aoBoard + "/ao2", 0, 10);
            AddAnalogOutputChannel("fibreAmpPwr", aoBoard + "/ao3");
            //AddAnalogOutputChannel("pumpAOM", aoBoard + "/ao4", 0, 10);
            AddAnalogOutputChannel("pumpAOM", usbDAQ4 + "/ao0", 0, 5);
            //AddAnalogOutputChannel("flPZT2Temp", aoBoard + "/ao5", 0, 4); //voltage must not exceed 4V for Koheras laser
            //AddAnalogOutputChannel("flPZT2Cur", aoBoard + "/ao6", 0, 5); //voltage must not exceed 5V for Koheras laser
            //AddAnalogOutputChannel("fibreAOM", usbDAQ4 + "/ao1", 0, 5);
            AddAnalogOutputChannel("rampfb", aoBoard + "/ao4", -10, 10);
            AddAnalogOutputChannel("I2LockBias", aoBoard + "/ao5", 0, 5);
        }

    }
}