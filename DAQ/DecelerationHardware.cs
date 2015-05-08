using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;
using DAQ.Remoting;
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
			Boards.Add("pg", "/dev1");
            Boards.Add("usbDev", "/dev3");
            Boards.Add("PXI6", "/PXI1Slot6_4");
            Boards.Add("PXI4", "/PXI1Slot4");
            Boards.Add("PXI5", "/PXI1Slot5");
            string pgBoard =  (string)Boards["pg"];
            string usbBoard = (string)Boards["usbDev"];
            string daqBoard = (string)Boards["daq"];
            string PXIBoard = (string)Boards["PXI6"];
            string TCLBoard = (string)Boards["PXI4"];
            string TCLBoard2 = (string)Boards["PXI5"];

            //Instruments.Add("synth", new HP8673BSynth("GPIB0::19::INSTR"));
            //Instruments.Add("counter", new HP5350BCounter("GPIB0::14::INSTR"));

          //  Instruments.Add("flowmeter", new FlowMeter("ASRL1::INSTR"));


            //VCO lock
            //AddAnalogOutputChannel("VCO_Out", PXIBoard + "/ao12", 0.0, 10.0);

            // add things to the info
            Info.Add("PGClockLine", Boards["pg"] + "/PFI2");
            Info.Add("PatternGeneratorBoard", pgBoard);
            Info.Add("PGType", "dedicated");
            
            Info.Add("TCL_Slave_Voltage_Limit_Upper", 5.0); //volts: Laser control
            Info.Add("TCL_Slave_Voltage_Limit_Lower", 0.0); //volts: Laser control
            Info.Add("TCL_Default_Gain", 0.5);
            Info.Add("TCL_Default_VoltageToLaser", 0.0);
            Info.Add("TCL_MAX_INPUT_VOLTAGE", 10.0);
            Info.Add("TCL_Default_ScanPoints", 1000);

            //TCL Lockable lasers
            //For TCL 2012
            Info.Add("TCLLockableLasers", new string[] { "v00cooling", "v10repump" ,"spectra"});
            Info.Add("TCLPhotodiodes", new string[] { "cavity", "master", "p1", "p2","p3" });// THE FIRST TWO MUST BE CAVITY AND MASTER PHOTODIODE!!!!
            
            //Some matching up for TCL
            Info.Add("v00cooling", "p1");
            Info.Add("v10repump", "p2");
            Info.Add("spectra", "p3");
            Info.Add("TCLTrigger", TCLBoard + "/PFI0");


            //TCL Lockable lasers
            //For TCL 2014

            //List<string> lockablelasers1 = new List<string> { "v00cooling", "v10repump" };
           //List<string> lockablelasers2 = new List<string> { "v21repump" };
           //List<List<string>> TCLlockableLasers = new List<List<string>> { lockablelasers1, lockablelasers2 };
            
            //List<string> photodiodes1 = new List<string> { "cavity0", "master0", "p1", "p2" };
            //List<string> photodiodes2 = new List<string> { "cavity1", "master1", "p3" };
            //List<List<string>> TCLphotodiodes = new List<List<string>> { photodiodes1, photodiodes2 };
              
            //Info.Add("TCLTriggers", new string[] { "trigger1", "trigger2" });
            //Info.Add("TCLLockableLasers", TCLlockableLasers );
            //Info.Add("TCLPhotodiodes", TCLphotodiodes ); 
            //Info.Add("laser", "p1");                                                    
            //Info.Add("laser2", "p2");

            // Some matching up for TCL
           // Info.Add("v00cooling", "p1");
            //Info.Add("v10repump", "p2");
            //Info.Add("v21repump", "p3");
           // Info.Add("trigger1", TCLBoard + "/PFI0");
           // Info.Add("trigger2", TCLBoard2 + "/PFI0");


            // the analog triggers
            Info.Add("analogTrigger0", (string)Boards["daq"] + "/PFI0");// pin 11
            Info.Add("analogTrigger1", (string)Boards["daq"] + "/PFI1");// pin 10
            //Info.Add("TCLTrigger", (string)Boards["PXI4"] + "/PFI0");
            //Info.Add("analogTrigger2", (string)Boards["usbDev"] + "/PFI0"); //Pin 29
            Info.Add("analogTrigger3", (string)Boards["daq"] + "/PFI6"); //Pin 5 - breakout 31
            //distance information
            Info.Add("sourceToDetect", 0.535); //in m
            Info.Add("sourceToSoftwareDecelerator", 0.12); //in m
            //information about the molecule
            Info.Add("molecule", "caf");
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
			AddDigitalOutputChannel("flash", pgBoard, 0, 0);//Changed from pg board P.0.5 because that appears to have died mysteriously (line dead in ribbon cable?) TEW 06/04/09
			AddDigitalOutputChannel("q", pgBoard, 0,2 );
			AddDigitalOutputChannel("detector", pgBoard, 3, 7);
			AddDigitalOutputChannel("detectorprime", pgBoard, 3, 6);
		    AddDigitalOutputChannel("aom", pgBoard, 2, 1);//Same channel as "ttl2" as used by the AomLevelControlPlugin. Now commented out.
			AddDigitalOutputChannel("decelhplus", pgBoard, 1, 0); //Pin 16
			AddDigitalOutputChannel("decelhminus", pgBoard, 1, 1); //Pin 17
			AddDigitalOutputChannel("decelvplus", pgBoard, 1, 2); //Pin 51
			AddDigitalOutputChannel("decelvminus", pgBoard, 1, 3); //Pin 52
            AddDigitalOutputChannel("cavityTriggerOut", usbBoard, 0, 1);//Pin 18
            //AddDigitalOutputChannel("ttl1", pgBoard, 2, 2); //Pin 58 Used to be used with AomLevelControlPlugin.
            //AddDigitalOutputChannel("ttl2", pgBoard, 2, 1); //Pin 57
            AddDigitalOutputChannel("ttlSwitch", pgBoard, 3, 0);	// This is the output that the pg
            // will switch if it's switch scanning.
            //AddDigitalOutputChannel("digitalSwitchChannel", pgBoard, 2, 2);

			// map the analog channels
			AddAnalogInputChannel("pmt", daqBoard + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("pmt2", daqBoard + "/ai2", AITerminalConfiguration.Rse);
			//AddAnalogInputChannel("longcavity", daqBoard + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("refcavity", daqBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("lockcavity", daqBoard + "/ai2", AITerminalConfiguration.Rse);
            
            AddAnalogInputChannel("master", TCLBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("cavity", TCLBoard + "/ai15", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("p1", TCLBoard + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("p2", TCLBoard + "/ai10", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("p3", TCLBoard + "/ai4", AITerminalConfiguration.Rse);

            //AddAnalogInputChannel("master1", TCLBoard2 + "/ai1", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("cavity1", TCLBoard2 + "/ai15", AITerminalConfiguration.Rse);
            //AddAnalogInputChannel("p3", TCLBoard2 + "/ai3", AITerminalConfiguration.Rse);

            //AddAnalogOutputChannel("spectra", PXIBoard + "/ao13");
            //AddAnalogOutputChannel("rampfb", PXIBoard + "/ao19");
            //AddAnalogOutputChannel("v00cooling", TCLBoard + "/ao0");
            AddAnalogOutputChannel("v00cooling", PXIBoard + "/ao0");
            AddAnalogOutputChannel("v10repump", TCLBoard + "/ao0");
            AddAnalogOutputChannel("rampfb", TCLBoard + "/ao1");
        
            //AddAnalogOutputChannel("v00cooling", TCLBoard + "/ao0");
            
            //AddAnalogOutputChannel("v10repump", PXIBoard + "/ao0");
            AddAnalogOutputChannel("spectra", PXIBoard + "/ao1");
            AddAnalogOutputChannel("v21repump", TCLBoard2 + "/ao0");
            //AddAnalogOutputChannel("cavity", daqBoard + "/ao0");
            // AddAnalogOutputChannel("cavity", PXIBoard + "/ao5");
            //AddAnalogOutputChannel("laser3", PXIBoard + "/ao31");
            
           
            AddAnalogOutputChannel("highvoltage", daqBoard + "/ao1");// hardwareController has "highvoltage" hardwired into it and so needs to see this ao, otherwise it crashes. Need to fix this.
            

            // map the counter channels
            AddCounterChannel("pmt", daqBoard + "/ctr0");
            AddCounterChannel("sample clock", daqBoard + "/ctr1");

            //map the monitoring source chamber in deceleration hardware
            AddAnalogInputChannel("RoughVacuum", PXIBoard + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("PressureSourceChamber", PXIBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("VoltageReference", PXIBoard + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("10KThermistor30KPlate", PXIBoard + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("30KShield", PXIBoard + "/ai4", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("4KCell", PXIBoard + "/ai5", AITerminalConfiguration.Rse);


		}

        //public override void ConnectApplications()
        //{
        //   RemotingHelper.ConnectDecelerationHardwareControl();
        //}
        
       public override void ConnectApplications()
        {
            RemotingHelper.ConnectDecelerationHardwareControl();
          // ask the remoting system for access to TCL2012
            Type t = Type.GetType("TransferCavityLock2012.Controller, TransferCavityLock");
            RemotingConfiguration.RegisterWellKnownClientType(t, "tcp://localhost:1190/controller.rem");
        }

	}
}
