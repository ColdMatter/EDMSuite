using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// This is a pulgin to have our YAG's flashlamp run at 10Hz but only fire a q-switch at 2Hz. Effectively experiment still runs at 2Hz but flash at 10Hz. 
	/// </summary>
	public class TenHzTwoHzPatternBuilder : DAQ.Pattern.PatternBuilder32
	{
		public TenHzTwoHzPatternBuilder()
		{
		}

		//	private const int FLASH_PULSE_LENGTH = 100;
		private const int Q_PULSE_LENGTH = 15;
		private const int DETECTOR_TRIGGER_LENGTH = 20;

		public int ShotSequence(int startTime, int shots, int padShots, int padStart, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int ShutterPulseLength, int delayToDetectorTrigger,
			int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration, int switchLineDelay, int shutteroffdelay, bool modulation)
		{
			int time;
			if (padStart == 0)
			{
				time = startTime;
			}
			else
			{
				time = startTime + padStart;
			}
			for (int i = 0; i < shots; i++)
			{
				// if we're on a 5th shot fire a q-switch. 
				if (i % 5 == 0)
				{
					if (modulation & i % 10 == 0) //If modulation is on then every 10th shot will be this. Aka the ON shot.
                    {
                        Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detectorprime"); 
					    time += flashlampPulseInterval;
					    for (int p = 0; p < padShots; p++)
					    {
						    FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
						    time += flashlampPulseInterval;
					    }
                    }
                    else //If modulation is off then every 5th flashlamp shot will be this. 
                    {
                        Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector");
					    time += flashlampPulseInterval;
					    for (int p = 0; p < padShots; p++)
					    {
						    FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
						    time += flashlampPulseInterval;
					    }
                    }
                    
				}
                
				else //else only fire the flashlamp
				{
                    FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);

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

		public int Shot(int startTime, int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int delayToDetectorTrigger, string detectorTriggerSource)
		{
			int time = 0;
			int tempTime = 0;

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
			// Detector trigger
			tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
			if (tempTime > time) time = tempTime;


			return time;
		}

	}
}
