using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;
using System.Collections.Generic;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// See the documentation for the PumpProbePatternPlugin
	/// </summary>
	public class LatticeBFieldFourShotBuilder : DAQ.Pattern.PatternBuilder32
	{
		public LatticeBFieldFourShotBuilder()
		{
		}

		//	private const int FLASH_PULSE_LENGTH = 100;
		private const int Q_PULSE_LENGTH = 15;
		private const int DETECTOR_TRIGGER_LENGTH = 20;
		private const int CAMERA_TRIGGER_LENGTH = 10000;
		

		//Add bool statement to funciton shotA which if false implies the shot should be a B pattern (no YAG) and if true A pattern (yes YAG)
		private List<bool> yagBool;
		private List<bool> slowBool;
		private int n;
		private List<string> detectors = new List<string>{ "detector","detectoralpha", "detectorprime", "detectorbeta" };
		private List<string> bfieldDigit = new List<string> { "B-Field_MSD", "B-Field_LSD" };

		public int ShotSequence(int startTime, int shots, int padShots, int padStart, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int shutterPulseLength, int delayToDetectorTrigger,
			int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration, int shutteroffdelay, int shutterslowdelay, int DurationV0,
			int shutterV1delay, int shutterV2delay, int DurationV2, int DurationV1, bool modulation, int switchLineDelay, int shutter1offdelay,
			int v3delaytime, int repumpDuration, int repumpDelay, int vacShutterDelay, int vacShutterDuration, int v0chirpTriggerDelay, int v0chirpTriggerDuration,
			int cameraTriggerDelay, int cameraBackgroundDelay, int offShotSlowingDuration, int v2OffDupoint, int bfieldDelay, int bfieldDuration)
		{
			
			int padEnd = padStart;
			int time;

            
            time = startTime + padStart;
            
			//Checks if modulation is on to choose which detectors should be used and what kind of shots to take
			//In this case all shots have slowing BUT we alrternate the B-field polarity
			
			if (modulation)
			{
				n = 4;
				detectors = new List<string> { "detector", "detectoralpha", "detectorprime", "detectorbeta" };			
				slowBool = new List<bool> {true, true, true, true };
				yagBool = new List<bool> {true, false, true, false };
				bfieldDigit = new List<string> { "B-Field_MSD", "B-Field_MSD", "B-Field_LSD", "B-Field_LSD" };
			}
			else
			{
				n = 2;
				detectors = new List<string> { "detector", "detectoralpha" };
				flashlampPulseInterval = 2 * flashlampPulseInterval;
				slowBool = new List<bool> {true, false};
				yagBool = new List<bool> {true, true};
			}


			for (int ShotIndex = 0; ShotIndex < shots; ShotIndex++)
			{

				for (int i = 0; i < n; i++)//Iterate over the on and off patterns with two kinds of each kind
				{
					pattern(time, startTime, shots, padShots, padStart, flashlampPulseInterval,
					valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, shutterPulseLength, delayToDetectorTrigger,
					ttlSwitchPort, ttlSwitchLine, switchLineDuration, shutteroffdelay, shutterslowdelay, DurationV0,
					shutterV1delay, shutterV2delay, DurationV2, DurationV1, modulation, switchLineDelay, shutter1offdelay, v3delaytime,
					repumpDuration, repumpDelay, v0chirpTriggerDelay, v0chirpTriggerDuration, slowBool[i], offShotSlowingDuration, v2OffDupoint,
					bfieldDelay, bfieldDuration,bfieldDigit[i]);

					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, detectors[i],
						vacShutterDelay, vacShutterDuration, padStart, cameraTriggerDelay, cameraBackgroundDelay, yagBool[i]);

					time += flashlampPulseInterval;
				}

				for (int p = 0 ; p < padShots ; p++)
				{
                    FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
					time += flashlampPulseInterval;
				}

				//Old way of doign things will delete later
				//if (yagBool)
				//{
				//	detectors = new List<string> { "detector", "detectorprime" };//"analogTrigger0" and "analogTrigger1"
				//}
				//            else
				//            {
				//	detectors = new List<string> { "detectoralpha", "detectorbeta" };//"analogTrigger3" and "analogTrigger4"
				//}
				/////ON shot open slowing beam with YAG
				//pattern(time, startTime, shots, padShots, padStart, flashlampPulseInterval,
				//	valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, shutterPulseLength, delayToDetectorTrigger,
				//	ttlSwitchPort, ttlSwitchLine, switchLineDuration, shutteroffdelay, shutterslowdelay, DurationV0,
				//	shutterV1delay, shutterV2delay, DurationV2, DurationV1, modulation, switchLineDelay, shutter1offdelay, v3delaytime,
				//	repumpDuration, repumpDelay, v0chirpTriggerDelay, v0chirpTriggerDuration, true, offShotSlowingDuration);


				//Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, detectors[0],
				//	vacShutterDelay, vacShutterDuration, padStart, cameraTriggerDelay, cameraBackgroundDelay, shotBool);
				//yagBool = (((ShotIndex % 4) & 1) == 0);
				//slowBool = (((ShotIndex % 4) & 2) == 0);
				//           if (modulation)
				//           {


				//pattern(time, startTime, shots, padShots, padStart, flashlampPulseInterval,
				//	valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, shutterPulseLength, delayToDetectorTrigger,
				//	ttlSwitchPort, ttlSwitchLine, switchLineDuration, shutteroffdelay, shutterslowdelay, DurationV0,
				//	shutterV1delay, shutterV2delay, DurationV2, DurationV1, modulation, switchLineDelay, shutter1offdelay, v3delaytime,
				//	repumpDuration, repumpDelay, v0chirpTriggerDelay, v0chirpTriggerDuration, false, offShotSlowingDuration);


				//Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, detectors[1],
				//	vacShutterDelay, vacShutterDuration, padStart, cameraTriggerDelay, cameraBackgroundDelay, shotBool);


				//time += flashlampPulseInterval;
				//               for (int p = 0; p < padShots; p++)
				//               {
				//                   FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
				//                   time += flashlampPulseInterval;
				//               }
				//           }
				//           else
				//           {

				//pattern(time, startTime, shots, padShots, padStart, flashlampPulseInterval,
				//	valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, shutterPulseLength, delayToDetectorTrigger,
				//	ttlSwitchPort, ttlSwitchLine, switchLineDuration, shutteroffdelay, shutterslowdelay, DurationV0,
				//	shutterV1delay, shutterV2delay, DurationV2, DurationV1, modulation, switchLineDelay, shutter1offdelay, v3delaytime,
				//	repumpDuration, repumpDelay, v0chirpTriggerDelay, v0chirpTriggerDuration, true, offShotSlowingDuration);

				//Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detectorprime",
				//	vacShutterDelay, vacShutterDuration, padStart, cameraTriggerDelay, cameraBackgroundDelay, true);

				//time += flashlampPulseInterval;
				//               for (int p = 0; p < padShots; p++)
				//               {
				//                   FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
				//                   time += flashlampPulseInterval;
				//               }
				//           }

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
			int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration, int shutteroffdelay, int shutterslowdelay, int DurationV0,
			int shutterV1delay, int shutterV2delay, int DurationV2, int DurationV1, bool modulation, int switchLineDelay, int shutter1offdelay,
			int v3delaytime, int repumpDuration, int repumpDelay, int v0chirpTriggerDelay, int v0chirpTriggerDuration, bool V0slowingOn, 
			int offShotSlowingDuration, int v2OffDupoint, int bfieldDelay, int bfieldDuration, string bfieldDigit)

		{
			int shutterslowdelayCorrection = 570;//29Sept2024 we found an extra 0.6 ms correction is needed. This may change depends on the alignment of the V0 slowing beam relative to its shutters.

			///B-Field Switching
			Pulse(time, bfieldDelay + 500, bfieldDuration, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[bfieldDigit]).BitNumber);


			if (V0slowingOn)
			{
				///V0 Slowing
				// The complicated pulse sequence below is to make the V0 AOM following a sequence of constantly on, quickly switch off, pulsed on, quickly switch off, constantly on, which makes it most of the time warm to reduce the warm-up effect
				Pulse(time, shutterslowdelay + shutterslowdelayCorrection - 3000, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber); //V0 AOM
				Pulse(time, shutterslowdelay + shutterslowdelayCorrection + DurationV0, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//V0 AOM
				Pulse(time, shutterslowdelay + shutterslowdelayCorrection - 4000, DurationV0 + 4000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow2"]).BitNumber);//V0 Uniblitz Shutter. Which takes approximately 2ms (2000 us) to respond and up to 1ms to change state. 
			}

			else // Michail 24/11/2024. On the OFF shots, v0 slowing still shines but only for 30 us
			{
				// int offShotSlowingDuration = 10;
				///V0 Slowing
				// The complicated pulse sequence below is to make the V0 AOM following a sequence of constantly on, quickly switch off, pulsed on, quickly switch off, constantly on, which makes it most of the time warm to reduce the warm-up effect
				Pulse(time, shutterslowdelay + shutterslowdelayCorrection - 3000, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber); //V0 AOM
				Pulse(time, shutterslowdelay + shutterslowdelayCorrection + offShotSlowingDuration, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);//V0 AOM
				Pulse(time, shutterslowdelay + shutterslowdelayCorrection - 4000, offShotSlowingDuration + 4000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow2"]).BitNumber);//V0 Uniblitz Shutter. Which takes approximately 2ms (2000 us) to respond and up to 1ms to change state. 
			}

			///V1V2V3, all the repumps are controlled by the same two Shutters		
			//This shutter, Tom U (it was Thorlab) is responsible for the opening edge of the repump light pulse, we only care about the timing of the rising edge of this shutter itself
			//so this shutter pulse duration is better to be much longer than the desired repump light pulse duration
			int repumpFirstShutterDelayCorrection = -3000; //repumpOpenShutterDelayCorrection:it changed from 8.2 to 8.8 ms, which is likely due to the alignment that has changed.
			int repumpFirsShutterDurationCorrection = -3200;
			int repumpFirsShutterDuration = 100000;//27Sept2024, I don't think we are going to have any repump duration longer than 100 ms, so this should be pretty safe.
			Pulse(time, repumpDelay + repumpFirstShutterDelayCorrection, repumpFirsShutterDuration + repumpFirsShutterDurationCorrection, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterSTEVE2"]).BitNumber);//This is the thorlab shutter, it only need one TTL


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

			//V2 Aom
			
			Pulse(time, 0, shutterslowdelay + shutterslowdelayCorrection + DurationV0 + v2OffDupoint,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["v2AOM"]).BitNumber);

			///Chirping
			int chirpDuration = v0chirpTriggerDuration; // 10Sept2024, modified to match the profile variable name, but a bit tedious.
			Pulse(time, v0chirpTriggerDelay, chirpDuration, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["v0chirpTrigger"]).BitNumber);// This is the trigger pulse for the chirp ramp signal, its duration doesn't control the chirp ramp signal on 30Aug2024


		}
		
		public int Shot(int startTime, int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int 
			delayToDetectorTrigger, string detectorTriggerSource, int vacShutterDelay, int vacShutterDuration, int padStart,
			int cameraTriggerDelay, int cameraBackgroundDelay, bool yagON)
		{   // operations that happens in both on and off shots. The detector trigger changes its trigger source for di
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
			
			if (yagON)
			{
				// Flash pulse
				tempTime = Pulse(startTime, valveToQ - flashToQ, flashlampPulseLength,
					((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
				if (tempTime > time) time = tempTime;
				// Q pulse
				tempTime = Pulse(startTime, valveToQ, Q_PULSE_LENGTH,
					((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["q"]).BitNumber);
				if (tempTime > time) time = tempTime;
			}

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
			tempTime = Pulse(startTime, cameraBackgroundDelay + valveToQ, CAMERA_TRIGGER_LENGTH,    // Uncommented by Michail, 26/11/2024
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["cameratrigger"]).BitNumber);
			if (tempTime > time) time = tempTime;

			return time;
		}

	}
}
