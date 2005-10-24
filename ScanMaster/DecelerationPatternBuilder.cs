using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;
using DecelerationConfig;


namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// 
	/// </summary>
	public class DecelerationPatternBuilder : PatternBuilder32
	{
		
		private const int FLASH_PULSE_LENGTH = 100;
		private const int Q_PULSE_LENGTH = 100;
		private const int DETECTOR_TRIGGER_LENGTH = 20;
	
		public int ShotSequence( int startTime, int numberOfOnOffShots, int padShots, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int delayToDetectorTrigger,
			int delayToDeceleration, TimingSequence decelSequence, bool modulation)
		{
		
			int time = 0;
		
			for (int i = 0 ; i < numberOfOnOffShots ; i++ ) 
			{
				// first with decelerator on
				Shot( time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger, delayToDeceleration, "detector",
					decelSequence);
				time += flashlampPulseInterval;
				// then with the decelerator off if modulation is true (otherwise another on shot)
				if (modulation)
				{
					Shot( time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger, delayToDeceleration, "detectorprime",
						null);
				}
				else
				{
					Shot( time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger, delayToDeceleration, "detector",
						decelSequence);
				}
				time += flashlampPulseInterval;
				for (int p = 0 ; p < padShots ; p++)
				{
					FlashlampPulse(time, valveToQ, flashToQ);
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

		public int Shot( int startTime, int valvePulseLength, int valveToQ, int flashToQ,
			int delayToDetectorTrigger, int delayToDeceleration, string detectorTriggerSource, TimingSequence decelSequence)  
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
			// Detector trigger
			tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
			if (tempTime > time) time = tempTime;

			// Deceleration sequence
			if (decelSequence != null)
			{
				foreach (TimingSequence.Edge edge in decelSequence.Sequence)
				{
					tempTime = startTime + valveToQ + delayToDeceleration + edge.Time;
					AddEdge(
						((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[edge.Channel]).BitNumber,
						tempTime,
						edge.Sense
						);
				}	
			}
			return time;
		}

	
		
	}
	
}
