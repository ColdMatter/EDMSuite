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
	public class ClassicFourShotPatternBuilder : DAQ.Pattern.PatternBuilder32
	{
		public ClassicFourShotPatternBuilder()
		{
		}

		//	private const int FLASH_PULSE_LENGTH = 100;

		private const int FLASH_PULSE_LENGTH = 400;
		private const int Q_PULSE_LENGTH = 100;
		private const int DETECTOR_TRIGGER_LENGTH = 20;

		//Add string lists to iterate over shutters and detector triggers to separate shots

		private int n;
		private List<string> detectors;// = new List<string>{ "detector","detectoralpha", "detectorprime", "detectorbeta" };
		private List<string> shutters;
		private List<int> shutterDelays;
		private List<int> shutterDurations;

		public int ShotSequence(int startTime, int shots, int padShots, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int delayToDetectorTrigger, bool modulation,
			int valve, int shutter1Delay, int shutter1Duration, int shutter2Delay, int shutter2Duration)
		
		{
			
			int time = startTime;
			
            
			//Checks if modulation is on to choose which detectors should be used and what kind of shots to take
			if (modulation)
			{
				n = 4;
				detectors = new List<string> { "detector", "detectorprime", "detectoralpha", "detectorbeta" };			
				shutters = new List<string> { "v0Shutter1", "4fShutter1", "v0Shutter2", "4fShutter2" };
				shutterDelays = new List<int> {shutter1Delay, shutter1Delay , shutter2Delay , shutter2Delay };
				shutterDurations = new List<int> { shutter1Duration, shutter1Duration, shutter2Duration, shutter2Duration };
				
			}
			else
			{
				n = 2;
				detectors = new List<string> { "detector", "detectorprime" };
				shutters = new List<string> { "v0Shutter1", "4fShutter1"  };
				shutterDelays = new List<int> { shutter1Delay, shutter1Delay };
				shutterDurations = new List<int> { shutter1Duration, shutter1Duration };
				
			}
			shots = shots / n;

			for (int ShotIndex = 0; ShotIndex < shots; ShotIndex++)
			{

				for (int i = 0; i < n; i++)//Iterate over shutters (and thei rrespective timings) and detector triggers
				{

					pattern(time, shutters[i], shutterDelays[i], shutterDurations[i]);

					Shot(time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger, valve, detectors[i]);
					time += flashlampPulseInterval;
				}

				for (int p = 0 ; p < padShots ; p++)
				{
                    FlashlampPulse(time, valveToQ, flashToQ);
					time += flashlampPulseInterval;
				}

			}

			return time;
		}

		public int FlashlampPulse(int startTime, int valveToQ, int flashToQ)
		{
			return Pulse(startTime, valveToQ - flashToQ, FLASH_PULSE_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
		}

		public void pattern(int time,string shutter,int shutterDelay, int shutterDuration)

		{
			int shuttersCorrection = 11600;

			Pulse(time, shutterDelay-shuttersCorrection, shutterDuration,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[shutter]).BitNumber);
		}

		public int Shot(int startTime, int valvePulseLength, int valveToQ, int flashToQ, int delayToDetectorTrigger,
			int valve, string detectorTriggerSource)
		{
			int time = 0;
			int tempTime = 0;

			// valve pulse
			tempTime = Pulse(startTime, valve, valvePulseLength,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["valve"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Flash pulse
			tempTime = Pulse(startTime, valve + valveToQ - flashToQ, FLASH_PULSE_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Q pulse
			tempTime = Pulse(startTime, valve + valveToQ, Q_PULSE_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["q"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Detector trigger
			tempTime = Pulse(startTime, valve + delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
			if (tempTime > time) time = tempTime;


			return time;
		}
	}
}
