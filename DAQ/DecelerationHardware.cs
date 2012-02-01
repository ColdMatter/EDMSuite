using System;
using System.Collections;

using NationalInstruments.DAQmx;

using DAQ.Pattern;
using DAQ.Remoting;

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
            Boards.Add("PXI6", "/PXI1Slot6");
            Boards.Add("PXI4", "/PXI1Slot4");
            string pgBoard = (string)Boards["pg"];
            string usbBoard = (string)Boards["usbDev"];
            string daqBoard = (string)Boards["daq"];
            string PXIBoard = (string)Boards["PXI6"];
            string TCLBoard = (string)Boards["PXI4"];


            // add things to the info
            Info.Add("PGClockLine", Boards["pg"] + "/PFI2");
            Info.Add("PatternGeneratorBoard", pgBoard);
            Info.Add("PGType", "dedicated");

            //TCL Lockable lasers
            Info.Add("TCLLockableLasers", new string[] {"laser", "laser2"});
            Info.Add("TCLPhotodiodes", new string[] {"cavity", "master", "p1" ,"p2"});// THE FIRST TWO MUST BE CAVITY AND MASTER PHOTODIODE!!!!
            Info.Add("TCL_Slave_Voltage_Limit_Upper", 10.0); //volts: Laser control
            Info.Add("TCL_Slave_Voltage_Limit_Lower", -10.0); //volts: Laser control
            Info.Add("TCL_Default_Gain", 0.5);
            Info.Add("TCL_Default_VoltageToLaser", 0.0);
            // Some matching up for TCL
            Info.Add("laser", "p1");
            Info.Add("laser2", "p2");


            // the analog triggers
            Info.Add("analogTrigger0", (string)Boards["daq"] + "/PFI0");// pin 10
            Info.Add("analogTrigger1", (string)Boards["daq"] + "/PFI1");// pin 11
            Info.Add("TCLTrigger", (string)Boards["PXI4"] + "/PFI0");
            //Info.Add("analogTrigger2", (string)Boards["usbDev"] + "/PFI0"); //Pin 29
            Info.Add("analogTrigger3", (string)Boards["daq"] + "/PFI6"); //Pin 5 - breakout 31
            //distance information
            Info.Add("sourceToDetect", 0.81); //in m
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
			AddDigitalOutputChannel("aom", pgBoard, 0, 4);
			AddDigitalOutputChannel("decelhplus", pgBoard, 1, 0); //Pin 16
			AddDigitalOutputChannel("decelhminus", pgBoard, 1, 1); //Pin 17
			AddDigitalOutputChannel("decelvplus", pgBoard, 1, 2); //Pin 51
			AddDigitalOutputChannel("decelvminus", pgBoard, 1, 3); //Pin 52
            AddDigitalOutputChannel("cavityTriggerOut", usbBoard, 0, 1);//Pin 18
            AddDigitalOutputChannel("ttl1", pgBoard, 2, 2); //Pin 58
            AddDigitalOutputChannel("ttl2", pgBoard, 2, 1); //Pin 57

			// map the analog channels
			AddAnalogInputChannel("pmt", daqBoard + "/ai0", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("pmt2", daqBoard + "/ai8", AITerminalConfiguration.Rse);
			AddAnalogInputChannel("longcavity", daqBoard + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("refcavity", daqBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("lockcavity", daqBoard + "/ai2", AITerminalConfiguration.Rse);
            
            AddAnalogInputChannel("master", TCLBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("p1", TCLBoard + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("cavity", TCLBoard + "/ai2", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("p2", TCLBoard + "/ai0", AITerminalConfiguration.Rse);

            AddAnalogOutputChannel("laser", PXIBoard + "/ao13");
            //AddAnalogOutputChannel("cavity", daqBoard + "/ao0");
           // AddAnalogOutputChannel("cavity", PXIBoard + "/ao5");
            AddAnalogOutputChannel("laser2", PXIBoard + "/ao25");
            AddAnalogOutputChannel("laser3", PXIBoard + "/ao31");
           
            AddAnalogOutputChannel("highvoltage", daqBoard + "/ao1");// hardwareController has "highvoltage" hardwired into it and so needs to see this ao, otherwise it crashes. Need to fix this.
            

            // map the counter channels
            AddCounterChannel("pmt", daqBoard + "/ctr0");
            AddCounterChannel("sample clock", daqBoard + "/ctr1");

		}

        public override void ConnectApplications()
        {
            RemotingHelper.ConnectDecelerationHardwareControl();
        }

	}
}
