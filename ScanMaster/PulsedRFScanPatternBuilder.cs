using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// A pattern that switches between rf systems at a given time.
	/// </summary>
	public class PulsedRFScanPatternBuilder : PatternBuilder32
	{
		private const int FLASH_PULSE_LENGTH = 100;
		private const int Q_PULSE_LENGTH = 100;
		private const int DETECTOR_TRIGGER_LENGTH = 20;
		private const int START_PADDING = 500;	// it's critical that this is the same as the start
												// padding in the flashlamp pattern builder to avoid
												// discontinuities.

		int rf1Channel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["rf1Switch"]).BitNumber;
		int rf2Channel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["rf2Switch"]).BitNumber;
		int fmChannel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["greenFM"]).BitNumber;
		int piChannel = ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["piFlip"]).BitNumber;

	
		public int ShotSequence( int startTime, int numberOfOnOffShots, int padShots, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int delayToDetectorTrigger,
			int rf1CentreTime, int rf1Length, int rf2CentreTime, int rf2Length, int piFlipTime,
			int fmCentreTime, int fmLength ) 
		{
		
			int time = 0;

			// Disable both rf
			AddEdge(rf1Channel, 0, false);
			AddEdge(rf2Channel, 0, false);
			AddEdge(piChannel, 0, true);
		
			for (int i = 0 ; i < numberOfOnOffShots ; i++ ) 
			{
				Shot( time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger,
						rf1CentreTime, rf1Length, rf2CentreTime, rf2Length, piFlipTime, fmCentreTime, fmLength );
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
			return Pulse(startTime + START_PADDING, valveToQ - flashToQ, FLASH_PULSE_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
		}

		public int Shot( int startTime, int valvePulseLength, int valveToQ, int flashToQ,
			int delayToDetectorTrigger, int rf1CentreTime, int rf1Length, int rf2CentreTime, int rf2Length,
			int piFlipTime, int fmCentreTime, int fmLength )  
		{
			int time = 0;
			int tempTime = 0;

			// piFlip off
			AddEdge(piChannel, startTime + START_PADDING, false);

			// valve pulse
			tempTime = Pulse(startTime + START_PADDING, 0, valvePulseLength,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["valve"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Flash pulse
			tempTime = Pulse(startTime + START_PADDING, valveToQ - flashToQ, FLASH_PULSE_LENGTH, 
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Q pulse
			tempTime = Pulse(startTime + START_PADDING, valveToQ, Q_PULSE_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["q"]).BitNumber);
			if (tempTime > time) time = tempTime;

			// pulse rf1
			if (rf1Length != 0)
			{
				tempTime = Pulse(startTime + START_PADDING, valveToQ + rf1CentreTime - (rf1Length/2), rf1Length, rf1Channel);
				if (tempTime > time) time = tempTime;
			}
			// pulse rf2
			if (rf2Length != 0)
			{
				tempTime = Pulse(startTime + START_PADDING, valveToQ + rf2CentreTime - (rf2Length/2), rf2Length, rf2Channel);
				if (tempTime > time) time = tempTime;
			}
			// pulse green fm
			tempTime = Pulse(startTime + START_PADDING, valveToQ + fmCentreTime - (fmLength/2), fmLength, fmChannel);
			if (tempTime > time) time = tempTime;
			// piFlip on
			AddEdge(piChannel, startTime + piFlipTime + START_PADDING, true);

			// Detector trigger
			tempTime = Pulse(startTime + START_PADDING, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["detector"]).BitNumber);
			if (tempTime > time) time = tempTime;
		
			return time;
		}
		
	}
}
