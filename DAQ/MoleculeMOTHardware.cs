using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;
using DAQ.Remoting;
using DAQ.TransferCavityLock2012;
using System.Runtime.Remoting;
using System.Collections.Generic;
using DAQ.WavemeterLock;

namespace DAQ.HAL
{

    /// <summary>
    /// This is the specific hardware that the molecule MOT experiment has. This class conforms
    /// to the Hardware inteSidebandImAmp1ce.
    /// </summary>
    public class MoleculeMOTHardware : DAQ.HAL.Hardware
	{

		public MoleculeMOTHardware()
		{

            //Boards
            string digitalPatternBoardName = "digitalPattern";
            string digitalPatternBoardAddress = "/Dev1";//PCI-6534
            Boards.Add(digitalPatternBoardName, digitalPatternBoardAddress);

            string analogPatternBoardName = "analogPattern";
            string analogPatternBoardAddress = "/PXI1Slot2"; //PXIe-6738
            Boards.Add(analogPatternBoardName, analogPatternBoardAddress);

            string tclBoard1Name = "tclBoard1";
            string tclBoard1Address = "/PXI1Slot3";//PXIe-6738
            Boards.Add(tclBoard1Name, tclBoard1Address);

            string tclBoard2Name = "tclBoard2";
            string tclBoard2Address = "/PXI1Slot8"; //PXI-6221
            Boards.Add(tclBoard2Name, tclBoard2Address);

            string tclBoard3Name = "tclBoard3";
            string tclBoard3Address = "/PXI1Slot6";//PXI-6229
            Boards.Add(tclBoard3Name, tclBoard3Address);

            string usbBoard1Name = "usbBoard1";
            string usbBoard1Address = "/Dev2";//USB-6009
            Boards.Add(usbBoard1Name, usbBoard1Address);

            string usbBoard2Name = "usbBoard2";
            string usbBoard2Address = "/Dev3";//USB-6008
            Boards.Add(usbBoard2Name, usbBoard2Address);


            string digitalPatternBoardName2 = "digitalPattern2";
            string digitalPatternBoardAddress2 = "/PXI1Slot4";//PXI-6535
            Boards.Add(digitalPatternBoardName2, digitalPatternBoardAddress2);

            
            string analogPatternBoardName2 = "analogPattern2";
            string analogPatternBoardAddress2 = "/PXI1Slot7";//PXI-6738
            Boards.Add(analogPatternBoardName2, analogPatternBoardAddress2);
            

            // Channel Declarations

            AddAnalogInputChannel("ramp", tclBoard1Address + "/ai4", AITerminalConfiguration.Rse);

            // Hamish
            AddAnalogInputChannel("v00PD", tclBoard1Address + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("v10PD", tclBoard1Address + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("bXPD", tclBoard1Address + "/ai2", AITerminalConfiguration.Rse);////////////////////////////////////////////////////
            AddDigitalInputChannel("bXLockBlockFlag", tclBoard1Address, 0, 0);
            AddDigitalInputChannel("v00LockBlockFlag", tclBoard1Address, 0, 1);
            AddAnalogInputChannel("refPDHamish", tclBoard1Address + "/ai3", AITerminalConfiguration.Rse);

            AddAnalogOutputChannel("v00Lock", tclBoard1Address + "/ao0", 0, 10);//Reused for Rb D1 Cooling Wavemeter Lock 14/03/23
            AddAnalogOutputChannel("v10Lock", usbBoard2Address + "/ao1", 0, 5);
            AddAnalogOutputChannel("bXLock", tclBoard3Address + "/ao2"); 
            //AddAnalogOutputChannel("rbD1Frequency", tclBoard1Address + "/ao0"); //Reused Channel 14/03/23

            AddAnalogOutputChannel("cavityLockHamish", tclBoard3Address + "/ao3");


            // Carlos
            AddAnalogInputChannel("v21PD", tclBoard1Address + "/ai5", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("v32PD", tclBoard1Address + "/ai6", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("refPDCarlos", tclBoard1Address + "/ai7", AITerminalConfiguration.Rse);/////////////////////////////////////////
            AddAnalogInputChannel("bXBeastPD", tclBoard1Address + "/ai9", AITerminalConfiguration.Rse);

            AddAnalogOutputChannel("v21Lock", usbBoard2Address + "/ao0", 0.0, 1.0);         // 0-5 v range was for DBR
            AddAnalogOutputChannel("v32Lock", usbBoard1Address + "/ao0", 0.0, 1.0);         // 0-5 v range was for DBR
            AddAnalogOutputChannel("bXBeastLock", usbBoard1Address + "/ao1", 0, 5);
            AddAnalogOutputChannel("TCoolSidebandVCO", analogPatternBoardAddress2 + "/ao3"); //Reused for Rb Repump Wavemeter Lock 20/03/23
            //AddAnalogOutputChannel("rbRepumpFrequency", tclBoard1Address + "/ao1"); //Reused Channel 20/03/23


            // Digital Pattern
            AddDigitalOutputChannel("flashLamp", digitalPatternBoardAddress, 0, 0);
            AddDigitalOutputChannel("qSwitch", digitalPatternBoardAddress, 0, 1);
            AddDigitalOutputChannel("bXSlowingAOM", digitalPatternBoardAddress, 0, 2);
            AddDigitalOutputChannel("v0rfswitch1", digitalPatternBoardAddress, 0, 3);
            AddDigitalOutputChannel("v10SlowingAOM", digitalPatternBoardAddress, 0, 4);
            AddDigitalOutputChannel("QCLShutter", digitalPatternBoardAddress2, 2, 2);
            //AddDigitalOutputChannel("microwaveA", digitalPatternBoardAddress, 0, 5);
            AddDigitalOutputChannel("microwaveB", digitalPatternBoardAddress, 0, 6);
            AddDigitalOutputChannel("cameraTrigger", digitalPatternBoardAddress, 0, 7);
            AddDigitalOutputChannel("cameraTrigger2", digitalPatternBoardAddress, 1, 7);
            AddDigitalOutputChannel("aoPatternTrigger", digitalPatternBoardAddress, 1, 0);
            AddDigitalOutputChannel("v00MOTShutter", digitalPatternBoardAddress, 1, 1);
            AddDigitalOutputChannel("bXSlowingShutter", digitalPatternBoardAddress, 1, 2);
            AddDigitalOutputChannel("bXLockBlock", digitalPatternBoardAddress, 1, 3);
            AddDigitalOutputChannel("v00LockBlock", digitalPatternBoardAddress, 2, 1);
            AddDigitalOutputChannel("topCoilDirection", digitalPatternBoardAddress, 1, 4);
            AddDigitalOutputChannel("bottomCoilDirection", digitalPatternBoardAddress, 1, 5);
            AddDigitalOutputChannel("rbCoolingAOM", digitalPatternBoardAddress, 1, 6);
            AddDigitalOutputChannel("v0rfswitch2", digitalPatternBoardAddress, 2, 0);
            AddDigitalOutputChannel("heliumShutter", digitalPatternBoardAddress, 2, 2);
            AddDigitalOutputChannel("microwaveC", digitalPatternBoardAddress, 3, 2);
            AddDigitalOutputChannel("v0rfswitch3", digitalPatternBoardAddress, 0, 5);
            AddDigitalOutputChannel("tofTrigger", digitalPatternBoardAddress2, 1, 4);
            AddDigitalOutputChannel("v0rfswitch4", digitalPatternBoardAddress2, 0, 6);
            AddDigitalOutputChannel("microwaveSwitch", digitalPatternBoardAddress2, 1, 7);

            // Lambda cooling and blue MOT
            AddDigitalOutputChannel("v0ddsSwitchA", digitalPatternBoardAddress2, 2, 0);
            AddDigitalOutputChannel("v0ddsSwitchB", digitalPatternBoardAddress2, 2, 1);
            AddDigitalOutputChannel("v0ddsSwitchC", digitalPatternBoardAddress2, 1, 5);
            AddDigitalOutputChannel("v0ddsSwitchD", digitalPatternBoardAddress2, 1, 6);

            AddDigitalOutputChannel("DDSTrigger", digitalPatternBoardAddress2, 2, 3);

            // Rb Digital Pattern
            AddDigitalOutputChannel("rbPushBeam", digitalPatternBoardAddress, 1, 6);
            AddDigitalOutputChannel("rbOpticalPumpingAOM", digitalPatternBoardAddress, 2, 3);
            AddDigitalOutputChannel("rbAbsImagingBeam", digitalPatternBoardAddress, 2, 5);
            AddDigitalOutputChannel("rbRepump", digitalPatternBoardAddress, 2, 6);
            AddDigitalOutputChannel("rb2DCooling", digitalPatternBoardAddress, 2, 7);
            AddDigitalOutputChannel("rb3DCooling", digitalPatternBoardAddress, 3, 0);
            AddDigitalOutputChannel("rbAbsImgCamTrig", digitalPatternBoardAddress, 3, 1);
            AddDigitalOutputChannel("rbDDSFrequencySwitch", digitalPatternBoardAddress2, 1, 2);
            // Rb shutters
            AddDigitalOutputChannel("rb3DMOTShutter", digitalPatternBoardAddress, 2, 4);
            AddDigitalOutputChannel("rb2DMOTShutter", digitalPatternBoardAddress, 3, 5);

            //AddDigitalOutputChannel("rbspeedbumpCoilsBamAbsorptionShutter", digitalPatternBoardAddress, 3, 6);
            AddDigitalOutputChannel("rbPushBamAbsorptionShutter", digitalPatternBoardAddress, 3, 6);
            
            AddDigitalOutputChannel("rbOPShutter", digitalPatternBoardAddress, 3, 7);
            AddDigitalOutputChannel("dipoleTrapAOM", digitalPatternBoardAddress, 3, 3);
            AddDigitalOutputChannel("transportTrack", digitalPatternBoardAddress, 3, 4);
            AddDigitalOutputChannel("rbD1CoolingSwitch", digitalPatternBoardAddress2, 1, 1);


            // tweezer new digital pattern board
            AddDigitalOutputChannel("slavePatternCardTrigger", digitalPatternBoardAddress2, 0, 0);
            //AddDigitalOutputChannel("test01", digitalPatternBoardAddress2, 0, 1);
            AddDigitalOutputChannel("cafOptPumpingAOM", digitalPatternBoardAddress2, 0, 2);
            AddDigitalOutputChannel("flowEnable", digitalPatternBoardAddress2, 0, 3);
            AddDigitalOutputChannel("cafOptPumpingShutter", digitalPatternBoardAddress2, 0, 4);
            AddDigitalOutputChannel("test10", digitalPatternBoardAddress2, 1, 0);
            AddDigitalOutputChannel("motLightSwitch", digitalPatternBoardAddress2, 0, 1);
            AddDigitalOutputChannel("TransverseCoolingShutter", digitalPatternBoardAddress2, 0, 5);

            AddDigitalOutputChannel("TweezerChamberRbMOTAOMs", digitalPatternBoardAddress2, 1, 3);

            // Analog Pattern
            AddAnalogOutputChannel("slowingChirp", analogPatternBoardAddress + "/ao8");
            AddAnalogOutputChannel("v00Intensity", analogPatternBoardAddress + "/ao9");
            AddAnalogOutputChannel("v00EOMAmp", analogPatternBoardAddress + "/ao11");
            AddAnalogOutputChannel("v00Frequency", analogPatternBoardAddress + "/ao12");
            AddAnalogOutputChannel("MOTCoilsCurrent", analogPatternBoardAddress + "/ao13"); //13
            //AddAnalogOutputChannel("triggerDelay", analogPatternBoardAddress + "/ao15");
            AddAnalogOutputChannel("xShimCoilCurrent", analogPatternBoardAddress + "/ao17");
            AddAnalogOutputChannel("yShimCoilCurrent", analogPatternBoardAddress + "/ao16");
            AddAnalogOutputChannel("zShimCoilCurrent", analogPatternBoardAddress + "/ao21"); 
            AddAnalogOutputChannel("slowingCoilsCurrent", analogPatternBoardAddress + "/ao18");
            AddAnalogOutputChannel("v00Chirp", analogPatternBoardAddress + "/ao22");
            AddAnalogOutputChannel("topCoilShunt", analogPatternBoardAddress + "/ao26");
            AddAnalogOutputChannel("lightSwitch", analogPatternBoardAddress + "/ao19");

            AddAnalogOutputChannel("BXAttenuation", analogPatternBoardAddress2 + "/ao1");
            AddAnalogOutputChannel("SlowingRepumpAttenuation", analogPatternBoardAddress2 + "/ao5");


            // Old Rb Analog Pattern
            AddAnalogOutputChannel("rbCoolingIntensity", analogPatternBoardAddress + "/ao23"); // from old setup
            AddAnalogOutputChannel("rbCoolingFrequency", analogPatternBoardAddress + "/ao24"); // TTL in?


            // New Rb
            AddAnalogOutputChannel("rb3DCoolingFrequency", analogPatternBoardAddress + "/ao1");
            AddAnalogOutputChannel("rbD1VCO", analogPatternBoardAddress + "/ao3");
            //AddAnalogOutputChannel("rbRepumpFrequency", analogPatternBoardAddress + "/ao3");
            AddAnalogOutputChannel("rbAbsImagingFrequency", analogPatternBoardAddress + "/ao4");
            AddAnalogOutputChannel("rb3DCoolingAttenuation", analogPatternBoardAddress + "/ao0");
            AddAnalogOutputChannel("v0AOMSidebandAmp", analogPatternBoardAddress + "/ao2");

            //AddAnalogOutputChannel("rbRepumpAttenuation", analogPatternBoardAddress + "/ao5"); //Highjacked for D1 attenuation 21/03/2023
            AddAnalogOutputChannel("rbD1CoolingAttenuation", analogPatternBoardAddress + "/ao5");
            AddAnalogOutputChannel("rbOffsetLock", analogPatternBoardAddress + "/ao15");
            AddAnalogOutputChannel("rbRepumpOffsetLock", analogPatternBoardAddress + "/ao10");

            // Transfer coil
            AddAnalogOutputChannel("transferCoils", analogPatternBoardAddress + "/ao6");
            AddAnalogOutputChannel("transferCoilsShunt1", analogPatternBoardAddress + "/ao7");
            AddAnalogOutputChannel("transferCoilsShunt2", analogPatternBoardAddress + "/ao27");

            // Tweezer MOT coils

            AddAnalogOutputChannel("speedbumpCoils", analogPatternBoardAddress + "/ao20");
            AddAnalogOutputChannel("DipoleTrapLaserControl", analogPatternBoardAddress + "/ao29"); //Dipole trap DDS attenuation
            AddAnalogOutputChannel("TweezerMOTCoils", analogPatternBoardAddress + "/ao28");

            /* New Ananlog board
             * This specific channel "newAnalogTest" has been added to the
             * LoadMoleculeMOT, LoadMoleculeMOTNoSlowingEdge and LoadMoleculeMOTDualSpecies
             * inside the MOTMaster folder to make the old scripts compatible with both boards
             * If you plan on changing the name make sure you change it inside
             * those files too.
            */
            //AddAnalogOutputChannel("newAnalogTest", analogPatternBoardAddress2 + "/ao7");

            // Source
            AddDigitalOutputChannel("cryoCooler", tclBoard2Address, 0, 0);
            AddDigitalOutputChannel("sourceHeater", tclBoard2Address, 0, 1);
            AddDigitalOutputChannel("sf6Valve", tclBoard2Address, 0, 2);
            AddDigitalOutputChannel("heValve", tclBoard2Address, 0, 3);
            AddDigitalOutputChannel("sourceHeater40K", tclBoard2Address, 0, 4);
            AddDigitalOutputChannel("sourceHeaterMaster", tclBoard2Address, 0, 5);
            AddDigitalOutputChannel("sourceHeaterSF6", tclBoard2Address, 0, 6);

            AddAnalogInputChannel("sourceTemp", tclBoard2Address + "/ai4", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("sf6Temp", tclBoard2Address + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("sourcePressure", tclBoard2Address + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("MOTPressure", tclBoard2Address + "/ai8", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("sourceTemp2", tclBoard2Address + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("sourceTemp3", tclBoard2Address + "/ai10", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("sourceTemp40K", tclBoard2Address + "/ai5", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("sourceTemp40KDiode", tclBoard2Address + "/ai11", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("sf6FlowMonitor", tclBoard2Address + "/ai7", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("he6FlowMonitor", tclBoard2Address + "/ai6", AITerminalConfiguration.Rse);

            Info.Add("ToFPMTSignal", tclBoard2Address + "/ai3");
            Info.Add("PowerMonitorPD", tclBoard2Address + "/ai9");
            Info.Add("ToFTrigger", tclBoard2Address + "/PFI1");
            Info.Add("flowConversionSF6", 0.2); //Flow Conversions for flow monitor in sccm per Volt. 0.2 sccm per V for Alicat
            Info.Add("flowConversionHe", 1.0); 
            AddAnalogOutputChannel("hardwareControlAO0", tclBoard2Address + "/ao0");
            AddAnalogOutputChannel("hardwareControlAO1", tclBoard2Address + "/ao1");

            //Cavity combiner
            AddAnalogOutputChannel("Rf1Freq", analogPatternBoardAddress2 + "/ao0");
            AddAnalogOutputChannel("Rf2Freq", analogPatternBoardAddress2 + "/ao2");
            AddAnalogOutputChannel("Rf3Freq", analogPatternBoardAddress2 + "/ao4");
            AddAnalogOutputChannel("Rf4Freq", analogPatternBoardAddress2 + "/ao6");
            AddAnalogOutputChannel("FeedforwardS", analogPatternBoardAddress2 + "/ao8");
            AddAnalogOutputChannel("FeedforwardF", analogPatternBoardAddress2 + "/ao10");

            AddAnalogOutputChannel("Rf1Amp", analogPatternBoardAddress2 + "/ao17");
            AddAnalogOutputChannel("Rf2Amp", analogPatternBoardAddress2 + "/ao19");
            AddAnalogOutputChannel("Rf3Amp", analogPatternBoardAddress2 + "/ao21");
            AddAnalogOutputChannel("Rf4Amp", analogPatternBoardAddress2 + "/ao23");


            AddAnalogOutputChannel("BXFreq", analogPatternBoardAddress2 + "/ao9");


            WavemeterLockConfig wmlConfig = new WavemeterLockConfig("Default");
            //wmlConfig.AddSlaveLaser("RbD1Cooling", "rbD1Frequency", 7);//Laser name, analog channel, wavemeter channel
            //wmlConfig.AddLaserConfiguration("RbD1Cooling", 377.105206, -100, -1000);
            //wmlConfig.AddSlaveLaser("RbRepump", "rbRepumpFrequency", 5);
            
            wmlConfig.AddSlaveLaser("v0", "v00Lock", 1);
            wmlConfig.AddLaserConfiguration("v0", 494.432395, -500, -1500);

            wmlConfig.AddSlaveLaser("v1", "v10Lock", 2);
            wmlConfig.AddLaserConfiguration("v1", 476.958908, -200, -1000);

            wmlConfig.AddSlaveLaser("v2", "v21Lock", 3);
            wmlConfig.AddLaserConfiguration("v2", 477.299380, 20, 200);

            wmlConfig.AddSlaveLaser("v3", "v32Lock", 4);
            wmlConfig.AddLaserConfiguration("v3", 477.628176, -50, -500);

            wmlConfig.AddSlaveLaser("BX", "bXLock", 5);
            wmlConfig.AddLaserConfiguration("BX", 564.582406, 500, 500);
            //Use TC for sowing Mar 5th 2024
            //wmlConfig.AddLockBlock("BX", "bXLockBlockFlag");
            wmlConfig.AddLockBlock("TCool", "bXLockBlockFlag");

            wmlConfig.AddSlaveLaser("TCool", "bXBeastLock", 6);
            wmlConfig.AddLaserConfiguration("TCool", 564.582240, 50, 500);

            Info.Add("Default", wmlConfig);


            //AddDigitalInputChannel("tofTrig", tclBoard2Address, 0, 0);

            // TCL Config
            //TCLConfig tcl1 = new TCLConfig("Hamish");
            //tcl1.AddLaser("v00Lock", "v00PD");
            //tcl1.AddLaser("v10Lock", "v10PD");
            //tcl1.AddLaser("bXLock", "bXPD");
            //tcl1.Trigger = tclBoard1Address + "/PFI0";
            //tcl1.Cavity = "rampHamish";
            //tcl1.MasterLaser = "refPDHamish";
            //tcl1.Ramp = "cavityLockHamish";
            //tcl1.TCPChannel = 1190;
            //tcl1.AddDefaultGain("Master", 1.0);
            //tcl1.AddDefaultGain("v00Lock", 2);
            //tcl1.AddDefaultGain("v10Lock", 0.5);
            //tcl1.AddDefaultGain("bXLock", -2);
            //tcl1.AddFSRCalibration("v00Lock", 3.95); //This is an approximate guess
            //tcl1.AddFSRCalibration("v10Lock", 4.15);
            //tcl1.AddFSRCalibration("bXLock", 3.9);
            //tcl1.DefaultScanPoints = 850;
            //tcl1.PointsToConsiderEitherSideOfPeakInFWHMs = 3;
            //Info.Add("Hamish", tcl1);

            //TCLConfig tcl2 = new TCLConfig("Carlos");
            //tcl2.AddLaser("v21Lock", "v21PD");
            //tcl2.AddLaser("v32Lock", "v32PD");
            //tcl2.Trigger = tclBoard2Address + "/PFI0";
            //tcl2.Cavity = "rampCarlos";
            //tcl2.MasterLaser = "refPDCarlos";
            //tcl2.Ramp = "cavityLockCarlos";
            //tcl2.TCPChannel = 1191;
            //tcl2.AddDefaultGain("Master", 1.0);
            //tcl2.AddDefaultGain("v21Lock", -0.4);
            //tcl2.AddDefaultGain("v32Lock", 0.2);
            //tcl2.AddFSRCalibration("v21Lock", 3.7); //This is an approximate guess
            //tcl2.AddFSRCalibration("v32Lock", 3.7);
            //tcl2.DefaultScanPoints = 900;
            //tcl2.PointsToConsiderEitherSideOfPeakInFWHMs = 3;
            //Info.Add("Carlos", tcl2);


            //Aug 11th 2023
            /*
            TCLConfig tclConfig = new TCLConfig("Hamish & Carlos");
            tclConfig.Trigger = tclBoard1Address + "/PFI0";
            tclConfig.BaseRamp = "ramp";
            tclConfig.TCPChannel = 1190;
            tclConfig.DefaultScanPoints = 1000;
            tclConfig.PointsToConsiderEitherSideOfPeakInFWHMs = 4;
            tclConfig.AnalogSampleRate = 55000;//62000
            string hamish = "Hamish";
            string carlos = "Carlos";
            
            
            tclConfig.AddCavity(hamish);
            tclConfig.Cavities[hamish].AddSlaveLaser("v00Lock", "v00PD");
            tclConfig.Cavities[hamish].AddLockBlocker("v00Lock", "v00LockBlockFlag");
            tclConfig.Cavities[hamish].AddSlaveLaser("v10Lock", "v10PD");
            tclConfig.Cavities[hamish].AddSlaveLaser("bXLock", "bXPD");
            tclConfig.Cavities[hamish].AddLockBlocker("bXLock", "bXLockBlockFlag");
            tclConfig.Cavities[hamish].MasterLaser = "refPDHamish";
            tclConfig.Cavities[hamish].RampOffset = "cavityLockHamish";
            tclConfig.Cavities[hamish].AddDefaultGain("Master", 1.0);
            tclConfig.Cavities[hamish].AddDefaultGain("v00Lock", 2);
            tclConfig.Cavities[hamish].AddDefaultGain("v10Lock", 0.5);
            tclConfig.Cavities[hamish].AddDefaultGain("bXLock", -2);
            tclConfig.Cavities[hamish].AddFSRCalibration("v00Lock", 3.95); //This is an approximate guess
            tclConfig.Cavities[hamish].AddFSRCalibration("v10Lock", 4.15);
            tclConfig.Cavities[hamish].AddFSRCalibration("bXLock", 3.9);
            

            tclConfig.AddCavity(carlos);
            tclConfig.Cavities[carlos].AddSlaveLaser("v21Lock", "v21PD");
            tclConfig.Cavities[carlos].AddSlaveLaser("v32Lock", "v32PD");
            tclConfig.Cavities[carlos].AddSlaveLaser("bXBeastLock", "bXBeastPD");
            tclConfig.Cavities[carlos].MasterLaser = "refPDCarlos";
            tclConfig.Cavities[carlos].RampOffset = "cavityLockCarlos";
            tclConfig.Cavities[carlos].AddDefaultGain("Master", 1.0);
            tclConfig.Cavities[carlos].AddDefaultGain("v21Lock", -0.2);
            tclConfig.Cavities[carlos].AddDefaultGain("v32Lock", 1.0);
            tclConfig.Cavities[carlos].AddDefaultGain("bXBeastLock", 1.0);
            tclConfig.Cavities[carlos].AddFSRCalibration("v21Lock", 3.7); //This is an approximate guess
            tclConfig.Cavities[carlos].AddFSRCalibration("v32Lock", 3.7);
            tclConfig.Cavities[carlos].AddFSRCalibration("bXBeastLock", 4.5);

            Info.Add("TCLConfig", tclConfig);
            Info.Add("DefaultCavity", tclConfig);
            */

            // MOTMaster configuration
            MMConfig mmConfig = new MMConfig(false, false, true, false, false);
            mmConfig.ExternalFilePattern = "*.tif";
            Info.Add("MotMasterConfiguration", mmConfig);
            
            Info.Add("PGType", "dedicated");
            Info.Add("Element", "CaF");

            Info.Add("AOPatternTrigger", analogPatternBoardAddress + "/PFI4"); //PFI6
            Info.Add("AOClockLine", analogPatternBoardAddress + "/PFI6"); //PFI6
            Info.Add("SecondAOPatternTrigger", analogPatternBoardAddress2 + "/PFI4");
            Info.Add("SecondAOClockLine", analogPatternBoardAddress2 + "/PFI3");

            /*
            Info.Add("PatternGeneratorBoard", digitalPatternBoardAddress);
            Info.Add("PGMaster_ClockLine", digitalPatternBoardAddress + "/PFI2");
            Dictionary<string, string> additionalPatternBoards = new Dictionary<string,string>();
            additionalPatternBoards.Add(digitalPatternBoardAddress2, digitalPatternBoardAddress2);
            Info.Add("AdditionalPatternGeneratorBoards", additionalPatternBoards);
            Info.Add("PGSlave0_ClockLine", digitalPatternBoardAddress2 + "/PFI4");
            Info.Add("PGSlave0_TriggerLine", digitalPatternBoardAddress2 + "/PFI3");
            
            */

            
            Dictionary<string, string> analogBoards = new Dictionary<string, string>();
            analogBoards.Add("AO", analogPatternBoardAddress);
            analogBoards.Add("SecondAO", analogPatternBoardAddress2);
            Info.Add("AnalogBoards", analogBoards);
            
            Info.Add("PatternGeneratorBoard", digitalPatternBoardAddress2);
            Info.Add("PGClockLine", digitalPatternBoardAddress2 + "/PFI4");
            Info.Add("PGTriggerLine", digitalPatternBoardAddress2 + "/PFI3");
            Dictionary<string, string> additionalPatternBoards = new Dictionary<string, string>();
            additionalPatternBoards.Add(digitalPatternBoardAddress, digitalPatternBoardAddress);
            Info.Add("AdditionalPatternGeneratorBoards", additionalPatternBoards);
            Info.Add("PGSlave0ClockLine", digitalPatternBoardAddress + "/PFI2");
            Info.Add("PGSlave0TriggerLine", digitalPatternBoardAddress + "/PFI6");



            /*********/
            //Info.Add("PGTrigger", Boards["pg"] + "/PFI2");   // trigger from "cryocooler sync" box, delay controlled from "triggerDelay" analog output


            // ScanMaster configuration
            //Info.Add("defaultTOFRange", new double[] { 4000, 12000 }); // these entries are the two ends of the range for the upper TOF graph
            //Info.Add("defaultTOF2Range", new double[] { 0, 1000 }); // these entries are the two ends of the range for the middle TOF graph
            //Info.Add("defaultGate", new double[] { 6000, 2000 }); // the first entry is the centre of the gate, the second is the half width of the gate (upper TOF graph)


            // Instruments
            Instruments.Add("windfreak", new WindfreakSynth("ASRL8::INSTR"));
            Instruments.Add("gigatronics 1", new Gigatronics7100Synth("GPIB0::19::INSTR"));
            Instruments.Add("gigatronics 2", new Gigatronics7100Synth("GPIB0::6::INSTR"));


            // Calibrations
            //AddCalibration("freqToVoltage", new PolynomialCalibration(new double[] { -9.7727, 0.16604, -0.0000272 }, 70, 130)); //this is a quadratic fit to the manufacturer's data for a POS-150
            //AddCalibration("motAOMAmp", new PolynomialCalibration(new double[] {6.2871, -0.5907, -0.0706, -0.0088, -0.0004}, -12, 4)); // this is a polynomial fit (up to quartic) to measured behaviour
            
		}

        
       public override void ConnectApplications()
        {

        }
	}
}
