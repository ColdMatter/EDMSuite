using System;

using DAQ.Environment;
using DAQ.HAL;
using DAQ.Pattern;

namespace ScanMaster.Acquire.Patterns
{
	/// <summary>
	/// See the documentation for the PumpProbePatternPlugin
	/// </summary>
	public class VelocityMeasSlowedPatternBuilder : DAQ.Pattern.PatternBuilder32
	{
		public VelocityMeasSlowedPatternBuilder()
		{
		}

		//	private const int FLASH_PULSE_LENGTH = 100;
		private const int Q_PULSE_LENGTH = 15;
		private const int DETECTOR_TRIGGER_LENGTH = 20;

		public int ShotSequence(int startTime, int shots, int padShots, int padStart, int flashlampPulseInterval,
			int valvePulseLength, int valveToQ, int flashToQ, int flashlampPulseLength, int ShutterPulseLength, int delayToDetectorTrigger,
			int ttlSwitchPort, int ttlSwitchLine, int switchLineDuration, int Probe1ShutterDelay,int Probe2ShutterDelay, int shutter1offdelay, int shutterslowdelay, 
			int shutterV1delay, int shutterV2delay, int DurationV0, int DurationV1, int DurationV2, bool modulation)
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
				// Shots 1 and 2 (indices 0,1) are the off shots -> the perpendicular probe port:
				if (modulation & (i == 0))//e.g 1,3,5 etc
				{
					int switchChannel = PatternBuilder32.ChannelFromNIPort(ttlSwitchPort, ttlSwitchLine);
					// Perpendicular Port with Slowing light
					Pulse(time, valveToQ - Probe1ShutterDelay - padStart, ShutterPulseLength, switchChannel); // This is just a digital output ttl. We'll use this as the trigger for one of the newport shutters 
					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector");// This triggers data aquisition in shot plugin
					Pulse(time, shutter1offdelay - padStart, ShutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter1off"]).BitNumber); // The newport control box needs a ttl to turn on shutter and a seperate ttl to turn off. This is to turn off
					Pulse(time, shutterslowdelay - padStart - 4000, DurationV0 + 4000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow2"]).BitNumber); // Uniblitz shutter for v=0 control
					Pulse(time, shutterslowdelay-padStart-3000, 3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber); // This is the AOM off pulse
					Pulse(time, shutterslowdelay - padStart+ DurationV0,3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber); //Two AOM off pulses as the AOM's default state is on.
					Pulse(time, shutterV1delay-padStart, DurationV1, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1"]).BitNumber); // This is just the shutter for v1
					Pulse(time, shutterV2delay-padStart, DurationV2, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv2"]).BitNumber); // This is just the shutter for v2
					time += flashlampPulseInterval;
					for (int p = 0; p < padShots; p++)
					{
						FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
						time += flashlampPulseInterval;
					}
				}


				else if (modulation & (i == 1))//e.g 1,3,5 etc
				{
					int switchChannel = PatternBuilder32.ChannelFromNIPort(ttlSwitchPort, ttlSwitchLine);
					// Perpendicular Port without Slowing light
					Pulse(time, valveToQ - Probe1ShutterDelay - padStart, ShutterPulseLength, switchChannel); // This is just a digital output ttl. We'll use this as the trigger for one of the newport shutters 
					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detector");// This triggers data aquisition in shot plugin
					Pulse(time, shutter1offdelay - padStart, ShutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter1off"]).BitNumber); // The newport control box needs a ttl to turn on shutter and a seperate ttl to turn off. This is to turn off
					Pulse(time, shutterV1delay-padStart, DurationV1, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1"]).BitNumber); // This is just the shutter for v1
					Pulse(time, shutterV2delay-padStart, DurationV2, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv2"]).BitNumber); // This is just the shutter for v2
					time += flashlampPulseInterval;


					time += flashlampPulseInterval;
					for (int p = 0; p < padShots; p++)
					{
						FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
						time += flashlampPulseInterval;
					}
				}

				// Shots 3 and 4 (indices 2,3) are the on shots -> the angled probe port:

				else if ((i % 2 == 0))//e.g 1,3,5 etc //else do a normal shot with the switch line high. e.f 0,2,4,6 it hits this one first
				{
					// Angled port with slowing light
					Pulse(time, valveToQ -Probe2ShutterDelay - padStart, shutter1offdelay, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow"]).BitNumber); // Probe shutter (one of the newport ones but with a different control box so doesn't need an off pulse)
					Pulse(time, shutterslowdelay - padStart - 4000, DurationV0 + 4000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow2"]).BitNumber); // Uniblitz shutter for v=0 control
					Pulse(time, shutterslowdelay - padStart-3000, ShutterPulseLength, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber); // This is the AOM off pulse
					Pulse(time, shutterslowdelay - padStart+ DurationV0,3000, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutter2on"]).BitNumber); // Two AOM off pulses as the AOM's default state is on.
					Pulse(time, shutterV1delay-padStart, DurationV1, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1"]).BitNumber); // This is just the shutter for v1
					Pulse(time, shutterV2delay-padStart, DurationV2, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv2"]).BitNumber); // This is just the shutter for v2
					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detectorprime");
					

					time += flashlampPulseInterval;
					for (int p = 0; p < padShots; p++)
					{
						FlashlampPulse(time, valveToQ, flashToQ, flashlampPulseLength);
						time += flashlampPulseInterval;
					}
					// now with the switch line low, if modulation is true (otherwise another with line high)
				}

				else if ((i % 2 != 0))//e.g 1,3,5 etc //else do a normal shot with the switch line high. e.f 0,2,4,6 it hits this one first
				{
					// Angled port without slowing light
					Pulse(time, valveToQ -Probe2ShutterDelay-padStart, shutter1offdelay, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterslow"]).BitNumber); // Probe shutter (one of the newport ones but with a different control box so doesn't need an off pulse)
					Pulse(time, shutterV1delay-padStart, DurationV1, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv1"]).BitNumber); // This is just the shutter for v1
					Pulse(time, shutterV2delay-padStart, DurationV2, ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["shutterv2"]).BitNumber); // This is just the shutter for v2
					Shot(time, valvePulseLength, valveToQ, flashToQ, flashlampPulseLength, delayToDetectorTrigger, "detectorprime");

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
