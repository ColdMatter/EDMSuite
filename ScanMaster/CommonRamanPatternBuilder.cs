using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// 
	/// </summary>
	public class CommonRamanPatternBuilder : PatternBuilder32
	{
		private const int FLASH_PULSE_LENGTH = 100;
		private const int Q_PULSE_LENGTH = 100;
		private const int DETECTOR_TRIGGER_LENGTH = 20;

		int rfSwitchChannel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["rfSwitch"]).BitNumber;
		int fmChannel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["fmSelect"]).BitNumber;
		int piChannel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["piFlip"]).BitNumber;

	
		public int ShotSequence( int startTime, int numberOfOnOffShots, int padShots, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int delayToDetectorTrigger,
			int rf1CentreTime, int rf1Length, int fmCentreTime, int fmLength, int piFlipTime ) 
		{
		
			int time = startTime;

			// Disable rf and fm
			AddEdge(rfSwitchChannel, 0, false);
			AddEdge(fmChannel, 0, false);
			AddEdge(piChannel, 0, true);
	
			for (int i = 0 ; i < numberOfOnOffShots ; i++ ) 
			{
				Shot( time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger,
					rf1CentreTime, rf1Length, fmCentreTime, fmLength, piFlipTime );
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
			int delayToDetectorTrigger, int rf1CentreTime, int rf1Length, int fmCentreTime, int fmLength,
			int piFlipTime )  
		{
			int time = 0;
			int tempTime = 0;

			// piFlip off
			AddEdge(piChannel, startTime, false);

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

			// pulse rf1
			tempTime = Pulse(startTime, valveToQ + rf1CentreTime - (rf1Length/2), rf1Length, rfSwitchChannel);
			if (tempTime > time) time = tempTime;
			// pulse green fm
			tempTime = Pulse(startTime, valveToQ + fmCentreTime - (fmLength/2), fmLength, fmChannel);
			if (tempTime > time) time = tempTime;
			// piFlip on
			AddEdge(piChannel, startTime + piFlipTime, true);

			// Detector trigger
			tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["detector"]).BitNumber);
			if (tempTime > time) time = tempTime;
		
			return time;
		}
		
	}
}
