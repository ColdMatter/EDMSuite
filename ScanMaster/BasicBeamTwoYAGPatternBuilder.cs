using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// See the documentation for the PumpProbePatternPlugin
	/// </summary>
	public class BasicBeamTwoYAGPatternBuilder : DAQ.Pattern.PatternBuilder32
	{
        public BasicBeamTwoYAGPatternBuilder() //This 
		{
		}

		private const int FLASH_PULSE_LENGTH = 400;
		private const int Q_PULSE_LENGTH = 100;
		private const int DETECTOR_TRIGGER_LENGTH = 20;
	
		public int ShotSequence( int startTime, int numberOfShots, int padShots, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int flashToQ2, int delayToDetectorTrigger) 
		{
			int time = startTime;
			for (int i = 0 ; i < numberOfShots ; i++ ) 
			{
				if ((i== 0)) //for the first shot, fire YAG 1
				{
					Shot(time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger, "detector");
					time += flashlampPulseInterval;
					for (int p = 0; p < padShots; p++)
					{
						FlashlampPulse(time, valveToQ, flashToQ);
						time += flashlampPulseInterval;
					}
				}

				if ((i == 1)) //for the next shot, fire YAG 2
				{
					Shot2(time, valvePulseLength, valveToQ, flashToQ2, delayToDetectorTrigger, "detector");
					time += flashlampPulseInterval;
				}

			} 
		
			return time;
		}

		public int FlashlampPulse( int startTime, int valveToQ, int flashToQ )
		{
			return Pulse(startTime, valveToQ - flashToQ, FLASH_PULSE_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
		}

		public int Shot( int startTime, int valvePulseLength, int valveToQ, int flashToQ, int delayToDetectorTrigger, string detectorTriggerSource)  
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
			// Scope trigger pulse
			tempTime = Pulse(startTime, valveToQ, Q_PULSE_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["scopetrigger"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Detector trigger
			tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
			if (tempTime > time) time = tempTime;

		
			return time;
		}

		public int Shot2(int startTime, int valvePulseLength, int valveToQ, int flashToQ2, int delayToDetectorTrigger, string detectorTriggerSource)
		{
			int time = 0;
			int tempTime = 0;

			// valve pulse
			tempTime = Pulse(startTime, 0, valvePulseLength,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["valve"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Flash pulse
			tempTime = Pulse(startTime, valveToQ - flashToQ2, FLASH_PULSE_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash2"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Q pulse
			tempTime = Pulse(startTime, valveToQ, Q_PULSE_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["q2"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Scope trigger pulse
			tempTime = Pulse(startTime, valveToQ, Q_PULSE_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["scopetrigger"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Detector trigger
			tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
			if (tempTime > time) time = tempTime;


			return time;
		}

	}
}
