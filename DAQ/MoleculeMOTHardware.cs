using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;
using DAQ.Remoting;
using DAQ.TransferCavityLock2012;
using System.Runtime.Remoting;
using System.Collections.Generic;

namespace DAQ.HAL
{
	
	/// <summary>
	/// This is the specific hardware that the molecule MOT experiment has. This class conforms
	/// to the Hardware interface.
	/// </summary>
	public class MoleculeMOTHardware : DAQ.HAL.Hardware
	{

		public MoleculeMOTHardware()
		{

            //Boards
            string digitalPatternBoardName = "digitalPattern";
            string digitalPatternBoardAddress = "/Dev1";
            Boards.Add(digitalPatternBoardName, digitalPatternBoardAddress);

            string analogPatternBoardName = "analogPattern";
            string analogPatternBoardAddress = "/PXI1Slot7";
            Boards.Add(analogPatternBoardName, analogPatternBoardAddress);

            string tclBoard1Name = "tclBoard1";
            string tclBoard1Address = "/PXI1Slot6";
            Boards.Add(tclBoard1Name, tclBoard1Address);

            string tclBoard2Name = "tclBoard2";
            string tclBoard2Address = "/PXI1Slot8";
            Boards.Add(tclBoard2Name, tclBoard2Address);

            string usbBoard1Name = "usbBoard1";
            string usbBoard1Address = "/Dev2";
            Boards.Add(usbBoard1Name, usbBoard1Address);

            string usbBoard2Name = "usbBoard2";
            string usbBoard2Address = "/Dev3";
            Boards.Add(usbBoard2Name, usbBoard2Address);


            // Channel Declarations

            // Hamish
            AddAnalogInputChannel("v00PD", tclBoard1Address + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("v10PD", tclBoard1Address + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("bXPD", tclBoard1Address + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("refPDHamish", tclBoard1Address + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("rampHamish", tclBoard1Address + "/ai4", AITerminalConfiguration.Rse);

            AddAnalogOutputChannel("v00Lock", tclBoard1Address + "/ao0");
            AddAnalogOutputChannel("v10Lock", tclBoard1Address + "/ao1");
            AddAnalogOutputChannel("bXLock", tclBoard1Address + "/ao2");
            AddAnalogOutputChannel("cavityLockHamish", tclBoard1Address + "/ao3");


            // Carlos
            AddAnalogInputChannel("v21PD", tclBoard2Address + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("v32PD", tclBoard2Address + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("refPDCarlos", tclBoard2Address + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("rampCarlos", tclBoard2Address + "/ai3", AITerminalConfiguration.Rse);

            AddAnalogOutputChannel("v21Lock", tclBoard2Address + "/ao0");
            AddAnalogOutputChannel("v32Lock", usbBoard1Address + "/ao0", 0, 5);
            AddAnalogOutputChannel("cavityLockCarlos", tclBoard2Address + "/ao1");


            // Digital Pattern
            AddDigitalOutputChannel("flashLamp", digitalPatternBoardAddress, 0, 0);
            AddDigitalOutputChannel("qSwitch", digitalPatternBoardAddress, 0, 1);
            AddDigitalOutputChannel("bXSlowingAOM", digitalPatternBoardAddress, 0, 2);
            AddDigitalOutputChannel("v00MOTAOM", digitalPatternBoardAddress, 0, 3);
            AddDigitalOutputChannel("v10SlowingAOM", digitalPatternBoardAddress, 0, 4);
            AddDigitalOutputChannel("microwaveA", digitalPatternBoardAddress, 0, 5);
            AddDigitalOutputChannel("microwaveB", digitalPatternBoardAddress, 0, 6);
            AddDigitalOutputChannel("cameraTrigger", digitalPatternBoardAddress, 0, 7);
            AddDigitalOutputChannel("cameraTrigger2", digitalPatternBoardAddress, 1, 7);
            AddDigitalOutputChannel("aoPatternTrigger", digitalPatternBoardAddress, 1, 0);
            AddDigitalOutputChannel("v00MOTShutter", digitalPatternBoardAddress, 1, 1);
            AddDigitalOutputChannel("bXSlowingShutter", digitalPatternBoardAddress, 1, 2);
            AddDigitalOutputChannel("tclBlock", digitalPatternBoardAddress, 1, 3);
            AddDigitalOutputChannel("topCoilDirection", digitalPatternBoardAddress, 1, 4);
            AddDigitalOutputChannel("bottomCoilDirection", digitalPatternBoardAddress, 1, 5);
            AddDigitalOutputChannel("rbCoolingAOM", digitalPatternBoardAddress, 1, 6);


            // Analog Pattern
            AddAnalogOutputChannel("slowingChirp", analogPatternBoardAddress + "/ao8");
            AddAnalogOutputChannel("v00Intensity", analogPatternBoardAddress + "/ao9");
            AddAnalogOutputChannel("v00EOMAmp", analogPatternBoardAddress + "/ao11");
            AddAnalogOutputChannel("v00Frequency", analogPatternBoardAddress + "/ao12");
            AddAnalogOutputChannel("MOTCoilsCurrent", analogPatternBoardAddress + "/ao13");
            AddAnalogOutputChannel("triggerDelay", analogPatternBoardAddress + "/ao15");
            AddAnalogOutputChannel("xShimCoilCurrent", analogPatternBoardAddress + "/ao17");
            AddAnalogOutputChannel("yShimCoilCurrent", analogPatternBoardAddress + "/ao16");
            AddAnalogOutputChannel("zShimCoilCurrent", analogPatternBoardAddress + "/ao21");
            AddAnalogOutputChannel("slowingCoilsCurrent", analogPatternBoardAddress + "/ao18");


            // Source
            AddDigitalOutputChannel("cryoCooler", usbBoard2Address, 0, 0);
            AddDigitalOutputChannel("sourceHeater", usbBoard2Address, 0, 1);
            AddAnalogInputChannel("sourceTemp", usbBoard2Address + "/ai0", AITerminalConfiguration.Rse);


            // TCL Config
            TCLConfig tcl1 = new TCLConfig("Hamish");
            tcl1.AddLaser("v00Lock", "v00PD");
            tcl1.AddLaser("v10Lock", "v10PD");
            tcl1.AddLaser("bXLock", "bXPD");
            tcl1.Trigger = tclBoard1Address + "/PFI0";
            tcl1.Cavity = "rampHamish";
            tcl1.MasterLaser = "refPDHamish";
            tcl1.Ramp = "cavityLockHamish";
            tcl1.TCPChannel = 1190;
            tcl1.AddDefaultGain("Master", 1.0);
            tcl1.AddDefaultGain("v00Lock", 2);
            tcl1.AddDefaultGain("v10Lock", 0.5);
            tcl1.AddDefaultGain("bXLock", -2);
            tcl1.AddFSRCalibration("v00Lock", 3.95); //This is an approximate guess
            tcl1.AddFSRCalibration("v10Lock", 4.15);
            tcl1.AddFSRCalibration("bXLock", 3.9);
            tcl1.DefaultScanPoints = 850;
            tcl1.PointsToConsiderEitherSideOfPeakInFWHMs = 3;
            Info.Add("Hamish", tcl1);

            TCLConfig tcl2 = new TCLConfig("Carlos");
            tcl2.AddLaser("v21Lock", "v21PD");
            tcl2.AddLaser("v32Lock", "v32PD");
            tcl2.Trigger = tclBoard2Address + "/PFI0";
            tcl2.Cavity = "rampCarlos";
            tcl2.MasterLaser = "refPDCarlos";
            tcl2.Ramp = "cavityLockCarlos";
            tcl2.TCPChannel = 1191;
            tcl2.AddDefaultGain("Master", 1.0);
            tcl2.AddDefaultGain("v21Lock", -0.4);
            tcl2.AddDefaultGain("v32Lock", 0.2);
            tcl2.AddFSRCalibration("v21Lock", 3.7); //This is an approximate guess
            tcl2.AddFSRCalibration("v32Lock", 3.7);
            tcl2.DefaultScanPoints = 900;
            tcl2.PointsToConsiderEitherSideOfPeakInFWHMs = 3;
            Info.Add("Carlos", tcl2);


            // MOTMaster configuration
            MMConfig mmConfig = new MMConfig(false, false, true, false);
            mmConfig.ExternalFilePattern = "*.tif";
            Info.Add("MotMasterConfiguration", mmConfig);
            Info.Add("AOPatternTrigger", analogPatternBoardAddress + "/PFI6");
            Info.Add("PatternGeneratorBoard", digitalPatternBoardAddress);
            Info.Add("PGType", "dedicated");
            Info.Add("Element", "CaF");
            //Info.Add("PGTrigger", Boards["pg"] + "/PFI2");   // trigger from "cryocooler sync" box, delay controlled from "triggerDelay" analog output


            // ScanMaster configuration
            Info.Add("defaultTOFRange", new double[] { 4000, 12000 }); // these entries are the two ends of the range for the upper TOF graph
            Info.Add("defaultTOF2Range", new double[] { 0, 1000 }); // these entries are the two ends of the range for the middle TOF graph
            Info.Add("defaultGate", new double[] { 6000, 2000 }); // the first entry is the centre of the gate, the second is the half width of the gate (upper TOF graph)


            // Instruments
            Instruments.Add("windfreak", new WindfreakSynth("ASRL8::INSTR"));
            Instruments.Add("gigatronics", new Gigatronics7100Synth("GPIB0::19::INSTR"));


            // Calibrations
            //AddCalibration("freqToVoltage", new PolynomialCalibration(new double[] { -9.7727, 0.16604, -0.0000272 }, 70, 130)); //this is a quadratic fit to the manufacturer's data for a POS-150
            //AddCalibration("motAOMAmp", new PolynomialCalibration(new double[] {6.2871, -0.5907, -0.0706, -0.0088, -0.0004}, -12, 4)); // this is a polynomial fit (up to quartic) to measured behaviour
            
		}

        
       public override void ConnectApplications()
        {

        }
	}
}
