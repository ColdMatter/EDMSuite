using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;
using NationalInstruments.DAQmx;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// See the documentation for the PumpProbePatternPlugin
	/// </summary>
	public class LatticeBFieldModPatternBuilder : DAQ.Pattern.PatternBuilder32
	{
		public LatticeBFieldModPatternBuilder()
		{
		}

    //	private const int FLASH_PULSE_LENGTH = 100;
		private const int Q_PULSE_LENGTH = 15;
		private const int DETECTOR_TRIGGER_LENGTH = 20;
		private const int CAMERA_TRIGGER_LENGTH = 10000;

		// Field Stuff
		//string physicalChannel = ((AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["B_Field"]).PhysicalChannel;//"PXI1Slot6/ao1";
		//double outputVoltage = 2.5;  // volts


		public int ShotSequence(int startTime, int shots, int padShots, int padStart, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int shutterPulseLength, int delayToDetectorTrigger,
			int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration,/* int shutteroffdelay,*/ int shutterslowdelay, int DurationV0,
			/*int shutterV1delay,*//* int shutterV2delay,*/ /*int DurationV2,*//* int DurationV1,*/ bool modulation, int switchLineDelay,/* int shutter1offdelay,*/ 
			int v3delaytime, int repumpDuration, int repumpDelay, int vacShutterDelay, int vacShutterDuration, int v0chirpTriggerDelay, int v0chirpTriggerDuration,
			int cameraTriggerDelay, int cameraBackgroundDelay, int offShotSlowingDuration,int v2OffDupoint, int bfieldDelay, int bfieldDuration) 
		{


			//Task analogOutTask = new Task();

			//// Create the AO channel
			//AOChannel myAOChannel = analogOutTask.AOChannels.CreateVoltageChannel(
			//	"PXI1Slot6/ao1",      // Replace with your actual device name
			//	"B_Field",   // Name of the channel (can be anything)
			//	0.0,             // Minimum voltage
			//	5.0,             // Maximum voltage
			//	AOVoltageUnits.Volts
			//);

			//// Create the writer
			//AnalogSingleChannelWriter writer = new AnalogSingleChannelWriter(analogOutTask.Stream);

			//// Start the task
			//analogOutTask.Start();

			//// Output the voltage
			//double analogDataOut = 3.0;
			//writer.WriteSingleSample(true, analogDataOut);

			//// Optional: Stop the task if you're done
			//analogOutTask.Stop();
			//analogOutTask.Dispose();  // Frees the hardware resource


			int padEnd = padStart;
			int time;
            if (padStart == 0)
            {
                time = startTime;
            }
            else
            {
                time = startTime + padStart;
            }
			for (int i = 0 ; i < shots ; i++ ) 
			{
                ///ON shot has positive B-Field polarity and slows

                //STEVE shutter

                //int IRDelay = valveToQ - switchLineDelay - padStart; Usual trigger but outdated.
                ///Safekeeping
                //Steve1
                //Pulse(time, repumpDelay + 560 + 160 - 2680 - padStart, shutterPulseLength, switchChannel); // This is used for the opening pulse of the STEVE shutter. The newport ones need a pulse to turn on and one to turn off
                //Pulse(time, repumpDelay + 5000 - padStart + 13500, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterSTEVE1off"]).BitNumber); // this is the V1/V2 STEVE shutter
                //Steve2
                //Pulse(time, repumpDelay + 560 - 2680 + repumpDuration - padStart, 21000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterSTEVE2"]).BitNumber);
                //Steve1

                //V0 Slowing


                //Camera
                //Pulse(time, CameraTrigger, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["camerashutter"]).BitNumber);//Guanchen added camera trigger for the molecule image
                //Pulse(time, BgTrigger, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["camerashutter"]).BitNumber); //Guanchen added camera trigger for the light background image
                pattern(time, startTime, shots, padShots, padStart, flashlampPulseInterval,
					valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, shutterPulseLength, delayToDetectorTrigger,
					ttlSwitchPort, ttlSwitchLine, switchLineDuration,/* shutteroffdelay,*/ shutterslowdelay, DurationV0,
					/*shutterV1delay,*//* shutterV2delay,*//* DurationV2,*//* DurationV1,*/ modulation, switchLineDelay,/* shutter1offdelay,*/ v3delaytime, 
					repumpDuration, repumpDelay, v0chirpTriggerDelay, v0chirpTriggerDuration, true, offShotSlowingDuration, v2OffDupoint,
					bfieldDelay, bfieldDuration);

				
				Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector",
					vacShutterDelay,  vacShutterDuration, padStart, cameraTriggerDelay, cameraBackgroundDelay);


				time += flashlampPulseInterval;
				
				for (int p = 0 ; p < padShots ; p++)
				{
                    FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
					time += flashlampPulseInterval;
				}
				
                if (modulation)
                {

					///OFF Shot inverts B-Field Polarity, keeps slowing on

					//Pulse(time, CameraTrigger, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["camerashutter"]).BitNumber);//Guanchen added camera trigger for the molecule image
					//Pulse(time, BgTrigger, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["camerashutter"]).BitNumber); //Guanchen added camera trigger for the light background image
					 
					pattern(time, startTime, shots, padShots, padStart, flashlampPulseInterval,
						valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, shutterPulseLength, delayToDetectorTrigger,
						ttlSwitchPort, ttlSwitchLine, switchLineDuration, /*shutteroffdelay,*/ shutterslowdelay, DurationV0,
						/*shutterV1delay,*//* shutterV2delay,*//* DurationV2,*//* DurationV1,*/ modulation, switchLineDelay,/* shutter1offdelay,*/ v3delaytime, 
						repumpDuration, repumpDelay, v0chirpTriggerDelay, v0chirpTriggerDuration, false, offShotSlowingDuration, v2OffDupoint,
						bfieldDelay, bfieldDuration); // Guanchen 20/11/2024

					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detectorprime",  vacShutterDelay,  vacShutterDuration, padStart, cameraTriggerDelay, cameraBackgroundDelay);



					time += flashlampPulseInterval;
                    for (int p = 0; p < padShots; p++)
                    {
                        FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
                        time += flashlampPulseInterval;
                    }
                }
                else
                {
					///ON shot open slowing beam
					//int IRDelay = valveToQ - switchLineDelay - padStart; Usual trigger but outdated.
					///Safekeeping:
					//Steve1
					//Pulse(time, repumpDelay +160 + 560 - 2680 - padStart, shutterPulseLength, switchChannel); // This is used for the opening pulse of the STEVE shutter. The newport ones need a pulse to turn on and one to turn off
					//Pulse(time, repumpDelay + 5000 - padStart + 13500, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterSTEVE1off"]).BitNumber); // this is the V1/V2 STEVE shutter
					//Steve2
					//Pulse(time, repumpDelay + 560 - 2680 + repumpDuration - padStart, 21000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterSTEVE2"]).BitNumber);


					//Camera
					//Pulse(time, CameraTrigger, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["camerashutter"]).BitNumber);//Guanchen added camera trigger for the molecule image
					//Pulse(time, BgTrigger, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["camerashutter"]).BitNumber); //Guanchen added camera trigger for the light background image

					/// Michail (20-11-24) added a boolean statement to toggle slowing light,
					/// Horacio (21-11-24) verified workign of the pattern and changed the method name onShot to pattern.
					pattern(time, startTime, shots, padShots, padStart, flashlampPulseInterval,
						valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, shutterPulseLength, delayToDetectorTrigger,
						ttlSwitchPort, ttlSwitchLine, switchLineDuration, /*shutteroffdelay, */shutterslowdelay, DurationV0,
						/*shutterV1delay,*//* shutterV2delay,*//* DurationV2,*//* DurationV1,*/ modulation, switchLineDelay, /*shutter1offdelay,*/v3delaytime,
						repumpDuration, repumpDelay, v0chirpTriggerDelay, v0chirpTriggerDuration, true, offShotSlowingDuration, v2OffDupoint,
						bfieldDelay, bfieldDuration);

					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector",
						vacShutterDelay,  vacShutterDuration, padStart, cameraTriggerDelay, cameraBackgroundDelay);

					time += flashlampPulseInterval;
                    for (int p = 0; p < padShots; p++)
                    {
                        FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
                        time += flashlampPulseInterval;
                    }
                }
			} 
			return time;
		}

        public int FlashlampPulse(int startTime, int valveToQ, int flashToQ, int flashlampPulseLength)
		{
            return Pulse(startTime, valveToQ - flashToQ, flashlampPulseLength,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
		}
		public void pattern(int time, int startTime, int shots, int padShots, int padStart, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int shutterPulseLength, int delayToDetectorTrigger,
			int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration,/* int shutteroffdelay,*/ int shutterslowdelay, int DurationV0,
			/*int shutterV1delay,*//* int shutterV2delay,*//* int DurationV2,*//* int DurationV1,*/ bool modulation, int switchLineDelay,/* int shutter1offdelay, */
			int v3delaytime, int repumpDuration, int repumpDelay, int v0chirpTriggerDelay, int v0chirpTriggerDuration, bool bfieldBool, int offShotSlowingDuration,
			int v2OffDupoint, int bfieldDelay, int bfieldDuration)
		    // patterns that only do operations for the on shot measurement
		{
			int shutterslowdelayCorrection = 570;//29Sept2024 we found an extra 0.6 ms correction is needed. This may change depends on the alignment of the V0 slowing beam relative to its shutters.
			int pmtVetoRelaxation = 500; // 31 Mar 2025 by Michail; adding a TTL for PMT veto, which also has its own relaxation
			if (bfieldBool)
			{
				///V0 Slowing
				// The complicated pulse sequence below is to make the V0 AOM following a sequence of constantly on, quickly switch off, pulsed on, quickly switch off, constantly on, which makes it most of the time warm to reduce the warm-up effect

				Pulse(time, shutterslowdelay + shutterslowdelayCorrection - 3000, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber); //V0 AOM
				Pulse(time, shutterslowdelay + shutterslowdelayCorrection + DurationV0, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//V0 AOM
				Pulse(time, shutterslowdelay + shutterslowdelayCorrection - 4000, DurationV0 + 4000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow2"]).BitNumber);//V0 Uniblitz Shutter. Which takes approximately 2ms (2000 us) to respond and up to 1ms to change state. 

				if (DurationV0 > 50)
                {
					Pulse(time, shutterslowdelay - pmtVetoRelaxation, DurationV0 + pmtVetoRelaxation + 500, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["pmtveto"]).BitNumber); // pmt veto TTL, added by Michail on 31Mar2025
				}
				///B-Field Switching
				Pulse(time, bfieldDelay + 500, bfieldDuration, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["B-Field_MSD"]).BitNumber);
			}

			else //Switch Bfield polarity and keep slowing on
            {
				// int offShotSlowingDuration = 10;
				///V0 Slowing
				// The complicated pulse sequence below is to make the V0 AOM following a sequence of constantly on, quickly switch off, pulsed on, quickly switch off, constantly on, which makes it most of the time warm to reduce the warm-up effect

				Pulse(time, shutterslowdelay + shutterslowdelayCorrection - 3000, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber); //V0 AOM
				Pulse(time, shutterslowdelay + shutterslowdelayCorrection + DurationV0, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//V0 AOM
				Pulse(time, shutterslowdelay + shutterslowdelayCorrection - 4000, DurationV0 + 4000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow2"]).BitNumber);//V0 Uniblitz Shutter. Which takes approximately 2ms (2000 us) to respond and up to 1ms to change state. 

				if (DurationV0 > 50)
				{
					Pulse(time, shutterslowdelay - pmtVetoRelaxation, DurationV0 + pmtVetoRelaxation + 500, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["pmtveto"]).BitNumber); // pmt veto TTL, added by Michail on 31Mar2025
				}
				///B-Field Switching
				Pulse(time, bfieldDelay + 500, bfieldDuration, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["B-Field_LSD"]).BitNumber);
			}

			///V1V2V3, all the repumps are controlled by the same two Shutters		
			//This shutter, Tom U (it was Thorlab) is responsible for the opening edge of the repump light pulse, we only care about the timing of the rising edge of this shutter itself
			//so this shutter pulse duration is better to be much longer than the desired repump light pulse duration
			int repumpFirstShutterDelayCorrection = -3000; //repumpOpenShutterDelayCorrection:it changed from 8.2 to 8.8 ms, which is likely due to the alignment that has changed.
			int repumpFirsShutterDurationCorrection = -3200;
			int repumpFirsShutterDuration = 100000;//27Sept2024, I don't think we are going to have any repump duration longer than 100 ms, so this should be pretty safe.
			Pulse(time, repumpDelay + repumpFirstShutterDelayCorrection, repumpFirsShutterDuration+repumpFirsShutterDurationCorrection, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterSTEVE2"]).BitNumber);//This is the thorlab shutter, it only need one TTL


			//Steve becomes SteveU after replacing the NewportShutter with an Uniblitz shutter, it opens with the rising edge and closes after the TTL high finishes
			// we use this shutter to close the light pulse such that the duration of the repump light pulse is controlled. 
			// so we care about the closing edge timing of this shutter, while the openning edge should be earlier than the other shutter
			//Pulse(time, 0, shutterPulseLength, PatternBuilder32.ChannelFromNIPort(ttlSwitchPort, ttlSwitchLine));// one pulse open this shutter, as early as it can
			int repumpSecondShutterDelayCorrection = -5200;
			//Pulse(time, repumpDelay + repumpDuration + repumpSecondShutterDelayCorrection, shutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterSTEVE1off"]).BitNumber);// Another pulse closes this shutter
			
			//Again, we use this shutter to be the closing edge of the repump light, so it should be opened as early as possible
			Pulse(time, 0, repumpDelay + repumpDuration + repumpSecondShutterDelayCorrection, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterSTEVE1off"]).BitNumber);// Another pulse closes this shutter, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterSTEVE1off"]).BitNumber);// Another pulse closes this shutter							  

			//V1 AOM Before EOM to reduce crystal wear. Opens at 0 same as 2nd shutter, clsoes with 1st shutter so ON only when repumping shutters are in use
			Pulse(time, 0, repumpDelay + repumpFirstShutterDelayCorrection + repumpFirsShutterDuration + repumpFirsShutterDurationCorrection, 
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["safetyV1AOM"]).BitNumber);

			//V2 Aom//before 19June2025
			Pulse(time, 0, shutterslowdelay + shutterslowdelayCorrection + DurationV0 + v2OffDupoint,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["v2AOM"]).BitNumber);
			//V2 Aom//on 19June2025
			//Pulse(time,0+ shutterslowdelay + shutterslowdelayCorrection+ DurationV0, v2OffDupoint, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["v2AOM"]).BitNumber);

			///Chirping
			int chirpDuration = v0chirpTriggerDuration; // 10Sept2024, modified to match the profile variable name, but a bit tedious.
			Pulse(time, v0chirpTriggerDelay, chirpDuration, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["v0chirpTrigger"]).BitNumber);// This is the trigger pulse for the chirp ramp signal, its duration doesn't control the chirp ramp signal on 30Aug2024

			
			

		}
		public int Shot(int startTime, int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int delayToDetectorTrigger,
			string detectorTriggerSource, int vacShutterDelay, int vacShutterDuration, int padStart, int cameraTriggerDelay, int cameraBackgroundDelay)
		{	// operations that happens in both on and off shots. The detector trigger changes its trigger source for di
			int time = 0;
			int tempTime = 0;

			if (tempTime > time) time = tempTime;
			//Vacuum Shutter
			tempTime = Pulse(startTime - padStart, valveToQ + vacShutterDelay, vacShutterDuration,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["vacuumShutter"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// valve pulse
			tempTime = Pulse(startTime, 0, valvePulseLength,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["valve"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Flash pulse
			tempTime = Pulse(startTime, valveToQ - flashToQ, flashlampPulseLength,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Q pulse
			tempTime = Pulse(startTime, valveToQ, Q_PULSE_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["q"]).BitNumber);
			if (tempTime > time) time = tempTime;

			// Detector trigger, PMT data acquisition trigger
			tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
			if (tempTime > time) time = tempTime;

			// Dector trigger, Camera enabler trigger: enable the camera data acquisition trigger
			tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, CAMERA_TRIGGER_LENGTH,
		    ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["cameraEnablerTrigger"]).BitNumber);
			if (tempTime > time) time = tempTime;

			// Camera trigger
			tempTime = Pulse(startTime, cameraTriggerDelay + valveToQ, CAMERA_TRIGGER_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["cameratrigger"]).BitNumber);
			if (tempTime > time) time = tempTime;

			// Camera background trigger
			tempTime = Pulse(startTime, cameraBackgroundDelay + valveToQ, CAMERA_TRIGGER_LENGTH,	// Uncommented by Michail, 26/11/2024
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["cameratrigger"]).BitNumber);
			if (tempTime > time) time = tempTime;

			return time;
		}

	}
}
