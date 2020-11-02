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

    //	private const int FLASH_PULSE_LENGTH = 100;
		private const int Q_PULSE_LENGTH = 15;
		private const int DETECTOR_TRIGGER_LENGTH = 20;
	
		public int ShotSequence( int startTime, int numberOfOnOffShots, int padShots, int padStart, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int aomStart1, int aomDuration1,
			int aomStart2, int aomDuration2, int aom2Duration1, int delayToDetectorTrigger,
			int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration, int switchLineDelay, int chirpStart, int chirpDuration, bool modulation) 
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
			for (int i = 0 ; i < numberOfOnOffShots ; i++ ) 
			{
				
				int switchChannel = PatternBuilder32.ChannelFromNIPort(ttlSwitchPort,ttlSwitchLine);
				// first the pulse with the switch line high
				Pulse(time, valveToQ + switchLineDelay, switchLineDuration, switchChannel);
                Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, aomStart1, aomDuration1, aomStart2, aomDuration2, aom2Duration1, delayToDetectorTrigger, chirpStart, chirpDuration, "detector");
				time += flashlampPulseInterval;
				for (int p = 0 ; p < padShots ; p++)
				{
                    FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
					time += flashlampPulseInterval;
				}
				// now with the switch line low, if modulation is true (otherwise another with line high)
                if (modulation)
                {
                    Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, aomStart1, aomDuration1, aomStart2, aomDuration2, aom2Duration1, delayToDetectorTrigger, chirpStart, chirpDuration, "detectorprime");
                    time += flashlampPulseInterval;
                    for (int p = 0; p < padShots; p++)
                    {
                        FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
                        time += flashlampPulseInterval;
                    }
                }
                else
                {
                    Pulse(time, valveToQ + switchLineDelay, switchLineDuration, switchChannel);
                    Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, aomStart1, aomDuration1, aomStart2, aomDuration2, aom2Duration1, delayToDetectorTrigger, chirpStart, chirpDuration, "detector");
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

		public int Shot( int startTime, int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int aomStart1, int aomDuration1,
            int aomStart2, int aomDuration2, int aom2Duration1, int delayToDetectorTrigger, int chirpStart, int chirpDuration, string detectorTriggerSource)  
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
            // chirp pulse
            tempTime = Pulse(startTime, valveToQ + chirpStart, chirpDuration,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["chirpTrigger"]).BitNumber);
            if (tempTime > time) time = tempTime;
			// aom1 pulse 1
			tempTime = Pulse(startTime, aomStart1 + valveToQ, aomDuration1,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["aom"]).BitNumber);
            // aom2 pulse 1
            tempTime = Pulse(startTime, aomStart1 + valveToQ, aom2Duration1,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["aom2"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// aom1 pulse 2
			tempTime = Pulse(startTime, aomStart2 + valveToQ, aomDuration2,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["aom"]).BitNumber);
            // aom2 pulse 2
            tempTime = Pulse(startTime, aomStart2 + valveToQ, aomDuration2,
                ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["aom2"]).BitNumber);
			if (tempTime > time) time = tempTime;
			// Detector trigger
			tempTime = Pulse(startTime, delayToDetectorTrigger + valveToQ, DETECTOR_TRIGGER_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[detectorTriggerSource]).BitNumber);
			if (tempTime > time) time = tempTime;

		
			return time;
		}
	
	}
}
