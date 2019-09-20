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
			int delayToDeceleration, TimingSequence decelSequence, string modulationMode, int decelOnStart, int decelOnDuration, bool modulation)
		{
		
			int time = startTime;
		
			for (int i = 0 ; i < numberOfOnOffShots ; i++ ) 
			{
                if (modulationMode == "BurstAndOff" || modulationMode == "BurstAndOn")
                {
                    // first with decelerator on
                    Shot(time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger, delayToDeceleration, "detector",
                        decelSequence, modulationMode, decelOnStart, decelOnDuration);
                    time += flashlampPulseInterval;
                    // then with the decelerator off if modulation is true (otherwise another on shot)
                    if (modulation)
                    {
                        Shot(time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger, delayToDeceleration, "detectorprime",
                            null, modulationMode, decelOnStart, decelOnDuration);
                    }
                    else
                    {
                        Shot(time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger, delayToDeceleration, "detector",
                            decelSequence, modulationMode, decelOnStart, decelOnDuration);
                    }
                    time += flashlampPulseInterval;
                }
                if (modulationMode == "OffAndBurst" || modulationMode == "OnAndBurst")
                {
                    // first with decelerator off
                    Shot(time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger, delayToDeceleration, "detector",
                        null, modulationMode, decelOnStart, decelOnDuration);
                    time += flashlampPulseInterval;
                    // then with the decelerator on if modulation is true (otherwise another off shot)
                    if (modulation)
                    {
                        Shot(time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger, delayToDeceleration, "detectorprime",
                            decelSequence, modulationMode, decelOnStart, decelOnDuration);
                    }
                    else
                    {
                        Shot(time, valvePulseLength, valveToQ, flashToQ, delayToDetectorTrigger, delayToDeceleration, "detector",
                            null, modulationMode, decelOnStart, decelOnDuration);
                    }
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

		public int FlashlampPulse( int startTime, int valveToQ, int flashToQ )
		{
			return Pulse(startTime, valveToQ - flashToQ, FLASH_PULSE_LENGTH,
				((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["flash"]).BitNumber);
		}

		public int Shot( int startTime, int valvePulseLength, int valveToQ, int flashToQ,
			int delayToDetectorTrigger, int delayToDeceleration, string detectorTriggerSource, TimingSequence decelSequence, 
            string modulationMode, int decelOnStart, int decelOnDuration)  
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
            else
            {
                if (modulationMode == "BurstAndOn" || modulationMode == "OnAndBurst") // long on pulse every other shot; otherwise, the decelerator stays off on every other shot
                {
                    tempTime = startTime + valveToQ + decelOnStart;
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["decelhplus"]).BitNumber, tempTime, true);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["decelhminus"]).BitNumber, tempTime, true);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["decelvplus"]).BitNumber, tempTime, true);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["decelvminus"]).BitNumber, tempTime, true);
                    tempTime = startTime + valveToQ + decelOnStart + decelOnDuration;
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["decelhplus"]).BitNumber, tempTime, false);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["decelhminus"]).BitNumber, tempTime, false);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["decelvplus"]).BitNumber, tempTime, false);
                    AddEdge(((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["decelvminus"]).BitNumber, tempTime, false);
                }
            }
			return time;
		}

	
		
	}
	
}
