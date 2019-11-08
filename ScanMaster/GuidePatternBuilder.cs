using System;
using System.Collections.Generic;
using System.Text;
using DAQ.HAL;
using DAQ.Environment;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// Pattern builder for the guide. The guide uses the two channels called decelhplus and decelhminus to drive the switches.
	/// At the beginning and end of the pulse sequence, both channels will be low. Both channels will have the same state at all times -
	/// any required inversions must be done later in the hardware. With the firing of the Q-switch being t=0, the channels first go high 
	/// at 'delayToGuide'. They remain high for a time 'lensSwitchPeriod', then go low for a further 'lensSwitchPeriod' etc. You'll get 
	/// 'numberOfLenses / 2' high-low cycles. The channels have to be in the same state at the end as at the start, so 'numberOfLenses' has to be even.
	/// </summary>
	public class GuidePatternBuilder : PatternBuilder32
	{
		private const int FLASH_PULSE_LENGTH = 100;
		private const int Q_PULSE_LENGTH = 100;
		private const int DETECTOR_TRIGGER_LENGTH = 20;

		public int ShotSequence(int startTime, int numberOfOnOffShots, int padShots, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int delayToDetectorTrigger,
			int delayToGuide, int lensSwitchPeriod, int numberOfLenses, int guideDcDuration, bool modulation)
		{
			int time = startTime;
			for (int i = 0; i < numberOfOnOffShots; i++)
			{
				//first with the guide switching
				Shot(time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger, "detector", delayToGuide, lensSwitchPeriod, numberOfLenses, guideDcDuration, true);
				time += flashlampPulseInterval;
				//then with the guide DC if modulation is true
				if (modulation)
				{
					Shot(time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger, "detectorprime", delayToGuide, lensSwitchPeriod, numberOfLenses, guideDcDuration, false);
					time += flashlampPulseInterval;
				}
				//otherwise, another shot with the guide switching
				else
				{
					Shot(time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger, "detector", delayToGuide, lensSwitchPeriod, numberOfLenses, guideDcDuration, true);
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

		public int Shot(int startTime, int valvePulseLength, int valveToQ, int flashToQ,
			int delayToDetectorTrigger, string detectorTriggerSource, int delayToGuide, int lensSwitchPeriod, int numberOfLenses, int guideDcDuration, bool switchGuide)
		{
			int time = 0;
			int tempTime = 0;
			// valve pulse
			tempTime = Pulse(startTime, 0, valvePulseLength,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["valve"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Flash pulse
			tempTime = Pulse(startTime, valveToQ - flashToQ, FLASH_PULSE_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Q pulse
			tempTime = Pulse(startTime, valveToQ, Q_PULSE_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["q"]).BitNumber);
			if (tempTime > time) time = tempTime;

			// Guiding sequence
			if (switchGuide)
			{
				for (int i = 0; i < (int)Math.Floor((double)(numberOfLenses / 2)); i++)
				{
					tempTime = Pulse(startTime, valveToQ + delayToGuide + (lensSwitchPeriod * 2 * i), lensSwitchPeriod,
						((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["decelhplus"]).BitNumber);
					tempTime = Pulse(startTime, valveToQ + delayToGuide + (lensSwitchPeriod * 2 * i), lensSwitchPeriod,
						((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["decelhminus"]).BitNumber);
					if (tempTime > time) time = tempTime;
				}
			}
			else
			{
				tempTime = Pulse(startTime, valveToQ + delayToGuide , guideDcDuration,
						((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["decelhplus"]).BitNumber);
				tempTime = Pulse(startTime, valveToQ + delayToGuide, guideDcDuration,
					((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["decelhminus"]).BitNumber);
				if (tempTime > time) time = tempTime;
			}
			// Detector trigger
			tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
			if (tempTime > time) time = tempTime;
			
			return time;
		}
	}
}
