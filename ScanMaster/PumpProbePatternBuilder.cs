using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// See the documentation for the PumpProbePatternPlugin
	/// </summary>
	public class PumpProbePatternBuilder : DAQ.Pattern.PatternBuilder32
	{
		public PumpProbePatternBuilder()
		{
		}

		private const int FLASH_PULSE_LENGTH = 100;
		private const int Q_PULSE_LENGTH = 100;
		private const int DETECTOR_TRIGGER_LENGTH = 20;
	
		public int ShotSequence( int startTime, int numberOfOnOffShots, int padShots, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int aomStart1, int aomDuration1,
			int aomStart2, int aomDuration2, int delayToDetectorTrigger,
			int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration, int switchLineDelay, bool modulation) 
		{
		
			int time = startTime;
            			
		
			for (int i = 0 ; i < numberOfOnOffShots ; i++ ) 
			{
				
				int switchChannel = PatternBuilder32.ChannelFromNIPort(ttlSwitchPort,ttlSwitchLine);
				// first the pulse with the switch line high
				Pulse(time, valveToQ + switchLineDelay, switchLineDuration, switchChannel);
              	Shot( time, valvePulseLength, valveToQ, flashToQ, aomStart1, aomDuration1, aomStart2, aomDuration2, delayToDetectorTrigger , "detector");
				time += flashlampPulseInterval;
				for (int p = 0 ; p < padShots ; p++)
				{
					FlashlampPulse(time, valveToQ, flashToQ);
					time += flashlampPulseInterval;
				}
				// now with the switch line low, if modulation is true (otherwise another with line high)
                if (modulation)
                {
                    Shot(time, valvePulseLength, valveToQ, flashToQ, aomStart1, aomDuration1, aomStart2, aomDuration2, delayToDetectorTrigger, "detectorprime");
                    time += flashlampPulseInterval;
                    for (int p = 0; p < padShots; p++)
                    {
                        FlashlampPulse(time, valveToQ, flashToQ);
                        time += flashlampPulseInterval;
                    }
                }
                else
                {
                    Pulse(time, valveToQ + switchLineDelay, switchLineDuration, switchChannel);
                    Shot(time, valvePulseLength, valveToQ, flashToQ, aomStart1, aomDuration1, aomStart2, aomDuration2, delayToDetectorTrigger, "detector");
                    time += flashlampPulseInterval;
                    for (int p = 0; p < padShots; p++)
                    {
                        FlashlampPulse(time, valveToQ, flashToQ);
                        time += flashlampPulseInterval;
                    }
                }
			} 
		
			return time;
		}

		public int FlashlampPulse( int startTime, int valveToQ, int flashToQ )
		{
			return Pulse(startTime, valveToQ - flashToQ, FLASH_PULSE_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
		}

		public int Shot( int startTime, int valvePulseLength, int valveToQ, int flashToQ, int aomStart1, int aomDuration1,
            int aomStart2, int aomDuration2, int delayToDetectorTrigger, string detectorTriggerSource)  
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
			// aom pulse 1
			tempTime = Pulse(startTime, aomStart1 + valveToQ, aomDuration1,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["aom"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// aom pulse 2
			tempTime = Pulse(startTime, aomStart2 + valveToQ, aomDuration2,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["aom"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Detector trigger
			tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
			if (tempTime > time) time = tempTime;

		
			return time;
		}
	
	}
}
