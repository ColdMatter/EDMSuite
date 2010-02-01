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
			Boards.Add("daq", "/dev1");
			Boards.Add("pg", "/dev2");


            // add things to the info
            Info.Add("PGClockLine", Boards["pg"] + "/PFI2");

            // the analog triggers
            Info.Add("analogTrigger0", (string)Boards["daq"] + "/PFI0");
            Info.Add("analogTrigger1", (string)Boards["daq"] + "/PFI1");
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
			string pgBoard = (string)Boards["pg"];
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

			// map the analog channels
			string daqBoard = (string)Boards["daq"];
			AddAnalogInputChannel("pmt", daqBoard + "/ai0", AITerminalConfiguration.Rse);
			AddAnalogInputChannel("longcavity", daqBoard + "/ai3", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("refcavity", daqBoard + "/ai1", AITerminalConfiguration.Rse);
            AddAnalogInputChannel("lockcavity", daqBoard + "/ai2", AITerminalConfiguration.Rse);
			AddAnalogOutputChannel("laser", daqBoard + "/ao0");
			AddAnalogOutputChannel("highvoltage", daqBoard + "/ao1");

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
