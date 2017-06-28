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
	/// This is the specific hardware that the deceleration experiment has. This class conforms
	/// to the Hardware interface.
	/// </summary>
	public class DecelerationHardware : DAQ.HAL.Hardware
	{

		public DecelerationHardware()
		{

			// YAG laser
			yag = new MiniliteLaser();

			// add the boards
			Boards.Add("daq", "/dev2");
            Boards.Add("multiDAQ", "/dev1");
			Boards.Add("pg", "/dev1");
            Boards.Add("aoBoard", "/PXI1Slot5");
            Boards.Add("usbDev", "/dev4");
            //Boards.Add("PXI6", "/PXI1Slot6_4");
            Boards.Add("PXI6", "/PXI1Slot6");
            Boards.Add("PXI4", "/PXI1Slot4");
            Boards.Add("PXI5", "/PXI1Slot5");
            string pgBoard =  (string)Boards["pg"];
            string usbBoard = (string)Boards["usbDev"];
            string daqBoard = (string)Boards["daq"];
            string PXIBoard = (string)Boards["PXI6"];
            string TCLBoard = (string)Boards["PXI4"];
            string TCLBoard2 = (string)Boards["PXI6"];
            string aoBoard = (string)Boards["aoBoard"];

            //configure instance 1 of transfer cavity lock
            TCLConfig tcl1 = new TCLConfig("Hamish McCavity");
            tcl1.AddLaser("v00cooling", "p1");
            tcl1.AddLaser("v10repump", "p2");
            tcl1.AddLaser("eylsa", "p3");
            tcl1.Trigger = TCLBoard + "/PFI0";
            tcl1.Cavity = "cavity";
            tcl1.MasterLaser = "master";
            tcl1.Ramp = "rampfb";
            tcl1.TCPChannel = 1190;
            tcl1.AddDefaultGain("Master", 1.0);
            tcl1.AddDefaultGain("v00cooling", 2);
            tcl1.AddDefaultGain("v10repump", 0.5);
            tcl1.AddDefaultGain("eylsa", -2);
            tcl1.AddFSRCalibration("v00cooling", 3.95); //This is an approximate guess
            tcl1.AddFSRCalibration("v10repump", 4.15);
            tcl1.AddFSRCalibration("eylsa", 3.9);
            tcl1.DefaultScanPoints = 850;
            tcl1.PointsToConsiderEitherSideOfPeakInFWHMs = 30;
            Info.Add("Hamish", tcl1);

            //configure instance 2 of transfer cavity lock
            TCLConfig tcl2 = new TCLConfig("Carlos the Cavity");
            tcl2.AddLaser("v21repump", "p12");
            tcl2.AddLaser("v32repump", "p22");
            tcl2.Trigger = TCLBoard2 + "/PFI0";
            tcl2.Cavity = "cavity2";
            tcl2.MasterLaser = "master2";
            tcl2.Ramp = "rampfb2";
            tcl2.TCPChannel = 1191;
            tcl2.AddDefaultGain("Master", 1.0);
            tcl2.AddDefaultGain("v21repump", -0.4);
            tcl2.AddDefaultGain("v32repump", 0.2);
            tcl2.AddFSRCalibration("v21repump", 3.7); //This is an approximate guess
            tcl2.AddFSRCalibration("v32repump", 3.7);
            tcl2.DefaultScanPoints = 900;
            tcl2.PointsToConsiderEitherSideOfPeakInFWHMs = 30;
            Info.Add("Carlos", tcl2);

            //MotMaster configuration
            MMConfig mmConfig = new MMConfig(false, false, false, false);
            mmConfig.ExternalFilePattern = "*.tif";
            Info.Add("MotMasterConfiguration", mmConfig);


            Instruments.Add("synth", new HP8673BSynth("GPIB0::19::INSTR"));
            //Instruments.Add("counter", new HP5350BCounter("GPIB0::14::INSTR"));

            //Instruments.Add("flowmeter", new FlowMeter("ASRL1::INSTR"));


            //VCO lock
            //AddAnalogOutputChannel("VCO_Out", PXIBoard + "/ao12", 0.0, 10.0);

            // add things to the info
            Info.Add("PGTrigger", Boards["pg"] + "/PFI2");          // trigger from "cryocooler sync" box, delay controlled from "triggerDelay" analog output
            //Info.Add("PGClockLine", Boards["pg"] + "/PFI2");
            Info.Add("PatternGeneratorBoard", pgBoard);
            Info.Add("PGType", "dedicated");
            Info.Add("AOPatternTrigger", aoBoard + "/PFI0");

            Info.Add("defaultTOFRange", new double[] {4000, 12000}); // these entries are the two ends of the range for the upper TOF graph
            Info.Add("defaultTOF2Range", new double[] { 0, 1000 }); // these entries are the two ends of the range for the middle TOF graph
            Info.Add("defaultGate", new double[] { 6000, 2000 }); // the first entry is the centre of the gate, the second is the half width of the gate (upper TOF graph)
            

            // the analog triggers
            Info.Add("analogTrigger0", (string)Boards["daq"] + "/PFI0");// pin 11
            Info.Add("analogTrigger1", (string)Boards["daq"] + "/PFI1");// pin 10
            //Info.Add("TCLTrigger", (string)Boards["PXI4"] + "/PFI0");
            //Info.Add("analogTrigger2", (string)Boards["usbDev"] + "/PFI0"); //Pin 29
            Info.Add("analogTrigger3", (string)Boards["daq"] + "/PFI6"); //Pin 5 - breakout 31
            Info.Add("usbAnalogTrigger", usbBoard + "/PFI0");
            //distance information
            Info.Add("sourceToDetect", 0.535); //in m
            Info.Add("sourceToSoftwareDecelerator", 0.12); //in m
            //information about the molecule
            Info.Add("molecule", "caf");
            Info.Add("Element", "CaF");
            Info.Add("moleculeMass", 58.961); // this is 40CaF in atomic mass units
            Info.Add("moleculeRotationalConstant", 1.02675E10); //in Hz
            Info.Add("moleculeDipoleMoment", 15400.0); //in Hz/(V/m)
            //information about the decelerator

			//These settings for WF
			Info.Add("deceleratorStructure", DecelerationConfig.DecelerationExperiment.SwitchStructure.V_H); //Vertical first
			Info.Add("deceleratorLensSpacing", 0.006);
			Info.Add("deceleratorFieldMap", "RodLayout3_EonAxis.dat");
			Info.Add("mapPoints", 121);
			Info.Add("mapStartPoint", 0.0);
			Info.Add("mapResolution", 0.0001);

            // These settings for AG
		//	Info.Add("deceleratorStructure", DecelerationConfig.DecelerationExperiment.SwitchStructure.H_V); //Horizontal first
        //  Info.Add("deceleratorLensSpacing", 0.024);
        //  Info.Add("deceleratorFieldMap", "Section1v1_onAxisFieldTemplate.dat");
        //  Info.Add("mapPoints", 481);
        //  Info.Add("mapStartPoint", 0.0);
        //  Info.Add("mapResolution", 0.0001);

            //Here are constants for 174YbF for future reference
            //Info.Add("molecule", "ybf");
            //Info.Add("moleculeMass", 192.937); // this is 174YbF in atomic mass units
            //Info.Add("moleculeRotationalConstant", 7.2338E9); //in Hz
            //Info.Add("moleculeDipoleMoment", 19700.0); //in Hz/(V/m)
                        			
			// map the digital channels
            AddDigitalOutputChannel("valve", pgBoard, 0, 6);
            AddDigitalOutputChannel("tclBlock", pgBoard, 0, 6); //Same as valve; deliberately!
			AddDigitalOutputChannel("flash", pgBoard, 0, 0);//Changed from pg board P.0.5 because that appears to have died mysteriously (line dead in ribbon cable?) TEW 06/04/09
			AddDigitalOutputChannel("q", pgBoard, 0,2 );
            AddDigitalOutputChannel("chirpTrigger", pgBoard, 1, 0);
			AddDigitalOutputChannel("detector", pgBoard, 3, 7);
			AddDigitalOutputChannel("detectorprime", pgBoard, 3, 6);
		    AddDigitalOutputChannel("aom", pgBoard, 2, 1);//Same channel as "ttl2" as used by the AomLevelControlPlugin. Now commented out.
            AddDigitalOutputChannel("aom2", pgBoard, 1, 6); // Pin 21 of PG board. Output 31 of front panel
            AddDigitalOutputChannel("v00Shutter", pgBoard, 2,2);
            //AddDigitalOutputChannel("digitalSwitchChannel", pgBoard, 2, 2);
            AddDigitalOutputChannel("v00AOM", pgBoard, 1, 1); //Pin 17
            AddDigitalOutputChannel("shimCoilSwitch", pgBoard, 1, 2); //Pin 51
            AddDigitalOutputChannel("bXShutter", pgBoard, 1, 3); //Pin 52
            AddDigitalOutputChannel("cameraTrigger", pgBoard, 0, 4); // Pin 13
            AddDigitalOutputChannel("AnalogPatternTrigger", pgBoard, 3, 3); //Pin 31

			// map the analog channels
			AddAnalogInputChannel("pmt", daqBoard + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("pmt2", daqBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("refcavity", daqBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("lockcavity", daqBoard + "/ai2", AITerminalConfiguration.Rse);
            
            AddAnalogInputChannel("master", TCLBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("cavity", TCLBoard + "/ai15", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("p1", TCLBoard + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("p2", TCLBoard + "/ai10", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("p3", TCLBoard + "/ai4", AITerminalConfiguration.Rse);

            AddAnalogOutputChannel("v10repump", TCLBoard + "/ao0");
            AddAnalogOutputChannel("rampfb", TCLBoard + "/ao1");
        
            AddAnalogOutputChannel("v00cooling", TCLBoard2 + "/ao2");
            AddAnalogOutputChannel("eylsa", TCLBoard2 + "/ao3");

            AddAnalogOutputChannel("slowingChirp", aoBoard + "/ao8");

            AddAnalogOutputChannel("v00Intensity", aoBoard + "/ao9");
            AddAnalogOutputChannel("v00Frequency", aoBoard + "/ao12");

            AddAnalogOutputChannel("MOTCoilsCurrent", aoBoard + "/ao13");

            AddAnalogOutputChannel("xShimCoilCurrent", aoBoard + "/ao17");
            AddAnalogOutputChannel("yShimCoilCurrent", aoBoard + "/ao16");
            AddAnalogOutputChannel("zShimCoilCurrent", aoBoard + "/ao14");

            AddAnalogOutputChannel("slowingCoilsCurrent", aoBoard + "/ao18");
            
            //second cavity

            AddAnalogInputChannel("master2", TCLBoard2 + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("cavity2", TCLBoard2 + "/ai4", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("p12", TCLBoard2 + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("p22", TCLBoard2 + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogOutputChannel("v21repump", TCLBoard2 + "/ao0");
            AddAnalogOutputChannel("v32repump", usbBoard + "/ao0", 0 , 5);
            AddAnalogOutputChannel("rampfb2", TCLBoard2 + "/ao1");
                       
           
            

            // map the counter channels
            AddCounterChannel("pmt", daqBoard + "/ctr0");
            AddCounterChannel("sample clock", daqBoard + "/ctr1");

            //map the monitoring source chamber in deceleration hardware
           // AddAnalogInputChannel("RoughVacuum", PXIBoard + "/ai0", AITerminalConfiguration.Rse);
          //  AddAnalogInputChannel("PressureSourceChamber", PXIBoard + "/ai1", AITerminalConfiguration.Rse);
          //  AddAnalogInputChannel("VoltageReference", PXIBoard + "/ai2", AITerminalConfiguration.Rse);
          //  AddAnalogInputChannel("10KThermistor30KPlate", PXIBoard + "/ai3", AITerminalConfiguration.Rse);
          //  AddAnalogInputChannel("30KShield", PXIBoard + "/ai4", AITerminalConfiguration.Rse);
          //  AddAnalogInputChannel("4KCell", PXIBoard + "/ai5", AITerminalConfiguration.Rse);

            //map the channels to monitor the sidebands in deceleration hardware
            AddAnalogInputChannel("cavityVoltage", usbBoard + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("mot606", usbBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("mot628V1", usbBoard + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("mot628V2", usbBoard + "/ai3", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("mot628V3", usbBoard + "/ai4", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("slowing531", usbBoard + "/ai4", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("slowing628V1", usbBoard + "/ai7", AITerminalConfiguration.Rse);

            //analog output channels controlled by the hardware controller and/or MOTMaster
            AddAnalogOutputChannel("motAOMFreq", aoBoard + "/ao10");
            AddAnalogOutputChannel("v00EOMAmp", aoBoard + "/ao11");
            AddAnalogOutputChannel("triggerDelay", aoBoard + "/ao15");

         //   AddCalibration("freqToVoltage", new PolynomialCalibration(new double[] { -9.7727, 0.16604, -0.0000272 }, 70, 130)); //this is a quadratic fit to the manufacturer's data for a POS-150
            //AddCalibration("motAOMAmp", new PolynomialCalibration(new double[] {6.2871, -0.5907, -0.0706, -0.0088, -0.0004}, -12, 4)); // this is a polynomial fit (up to quartic) to measured behaviour
            
		}

        
       public override void ConnectApplications()
        {
         //   RemotingHelper.ConnectMoleculeMOTHardwareControl();
          // ask the remoting system for access to TCL2012
       //     Type t = Type.GetType("TransferCavityLock2012.Controller, TransferCavityLock");
          //  RemotingConfiguration.RegisterWellKnownClientType(t, "tcp://localhost:1190/controller.rem");
        }

	}
}
