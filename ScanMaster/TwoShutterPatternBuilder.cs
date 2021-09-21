using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// See the documentation for the PumpProbePatternPlugin
	/// </summary>
	public class TwoShutterPatternBuilder : DAQ.Pattern.PatternBuilder32
	{
		public TwoShutterPatternBuilder()
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
				// if we're on an odd number of shots and we're modulating the switch line do this:
				if (modulation & i % 2 != 0)
				{
					Pulse(startTime, valveToQ - flashToQ, ShutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber);
					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detectorprime");
					Pulse(time, shutteroffdelay, ShutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2off"]).BitNumber);
					time += flashlampPulseInterval;
					for (int p = 0; p < padShots; p++)
					{
						FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
						time += flashlampPulseInterval;
					}
				}
				else //else do a normal shot with the switch line high.
				{
					int switchChannel = PatternBuilder32.ChannelFromNIPort(ttlSwitchPort, ttlSwitchLine);
					// first the pulse with the switch line high
					Pulse(time, valveToQ + switchLineDelay, switchLineDuration, switchChannel); // This is just a digital output ttl. We'll use this as the trigger for one of the shutters
					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector");
					time += flashlampPulseInterval;
					for (int p = 0; p < padShots; p++)
					{
						FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
						time += flashlampPulseInterval;
					}
					// now with the switch line low, if modulation is true (otherwise another with line high)
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
